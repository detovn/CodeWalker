using System.Diagnostics;
using System.Reflection;
using CodeWalker.GameFiles;
using CodeWalker.World;
using SharpDX;

namespace CodeWalker.Mcp;

/// <summary>
/// One shared, lazily loaded view of the GTA V install: encryption keys, the RPF index,
/// every archetype, and (on demand) the collision world used for ground probes.
/// All access to CodeWalker's caches goes through <see cref="Gate"/>, because they are not thread safe.
/// </summary>
public sealed class CwGame
{
    public CwConfig Config { get; }
    public SemaphoreSlim Gate { get; } = new(1, 1);

    private readonly object _startLock = new();
    private Task? _loadTask;
    private Task? _spaceTask;
    private readonly Stopwatch _loadTimer = new();

    public GameFileCache? Cache { get; private set; }
    public Space? Space { get; private set; }
    public string Status { get; private set; } = "not loaded (loads on first use)";
    public string SpaceStatus { get; private set; } = "not loaded (loads on first ground probe)";
    public TimeSpan? LoadTime { get; private set; }
    public List<string> RecentErrors { get; } = new();

    private Dictionary<uint, Archetype>? _archetypes;
    private Dictionary<string, List<RpfEntry>>? _entriesByName;

    public CwGame(CwConfig config) => Config = config;

    public bool IsLoaded => _loadTask?.IsCompletedSuccessfully == true;

    // ---------------------------------------------------------------- loading

    /// <summary>Waits for the game index to load. Throws a friendly error if it isn't ready within the timeout.</summary>
    public async Task EnsureLoadedAsync(CancellationToken ct, int timeoutSeconds = 240)
    {
        Task t;
        lock (_startLock)
        {
            _loadTask ??= Task.Run(Load);
            t = _loadTask;
        }
        try
        {
            await t.WaitAsync(TimeSpan.FromSeconds(timeoutSeconds), ct);
        }
        catch (TimeoutException)
        {
            throw new CwException($"GTA V files are still loading ({Status}, {_loadTimer.Elapsed.TotalSeconds:0}s so far). Call the tool again in a moment.");
        }
        if (t.IsFaulted)
        {
            var ex = t.Exception!.GetBaseException();
            lock (_startLock) _loadTask = null; // allow a retry after fixing the config
            throw new CwException("Loading GTA V files failed: " + ex.Message);
        }
    }

    public async Task EnsureSpaceAsync(CancellationToken ct)
    {
        await EnsureLoadedAsync(ct);
        Task t;
        lock (_startLock)
        {
            _spaceTask ??= Task.Run(LoadSpace);
            t = _spaceTask;
        }
        try
        {
            await t.WaitAsync(TimeSpan.FromSeconds(240), ct);
        }
        catch (TimeoutException)
        {
            throw new CwException($"The collision world is still loading ({SpaceStatus}). Call the tool again in a moment.");
        }
        if (t.IsFaulted)
        {
            lock (_startLock) _spaceTask = null;
            throw new CwException("Loading the collision world failed: " + t.Exception!.GetBaseException().Message);
        }
    }

    private void Load()
    {
        _loadTimer.Restart();
        string folder = Config.GtaFolder;
        if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
            throw new CwException("GTA V folder not found. Set it in CodeWalker (or the CODEWALKER_GTA_FOLDER environment variable). Current value: '" + folder + "'");
        string exe = Config.Gen9 ? "GTA5_Enhanced.exe" : "GTA5.exe";
        if (!File.Exists(Path.Combine(folder, exe)))
            throw new CwException($"{exe} not found in {folder}. If this is the other edition, set CODEWALKER_GEN9.");

        Status = "loading encryption keys";
        RpfManager.IsGen9 = Config.Gen9;
        try
        {
            GTA5Keys.LoadFromPath(folder, Config.Gen9, Config.Key);
        }
        catch when (Config.Key != null)
        {
            GTA5Keys.LoadFromPath(folder, Config.Gen9, null);
        }

        Status = "scanning RPF archives";
        var cache = new GameFileCache(2L * 1024 * 1024 * 1024, 10.0, folder, Config.Gen9, "", false, Config.ExcludeFolders)
        {
            EnableDlc = true, // empty SelectedDlc + EnableDlc = latest DLC level, like the app's default
            LoadVehicles = false,
            LoadPeds = false,
            LoadAudio = false,
        };
        cache.Init(s => Status = s, LogError);
        Cache = cache;

        // archetypeDict is private in Core; read it once so archetypes can be searched by name.
        var field = typeof(GameFileCache).GetField("archetypeDict", BindingFlags.NonPublic | BindingFlags.Instance);
        _archetypes = field?.GetValue(cache) as Dictionary<uint, Archetype> ?? new();

        _entriesByName = new(StringComparer.OrdinalIgnoreCase);
        foreach (var e in cache.RpfMan.EntryDict.Values)
        {
            if (e is RpfDirectoryEntry) continue;
            if (!_entriesByName.TryGetValue(e.Name, out var list)) _entriesByName[e.Name] = list = new();
            list.Add(e);
        }

        LoadTime = _loadTimer.Elapsed;
        Status = $"ready ({_archetypes.Count:N0} archetypes, {cache.RpfMan.EntryDict.Count:N0} archive entries, loaded in {LoadTime.Value.TotalSeconds:0}s)";
    }

    private void LoadSpace()
    {
        Gate.Wait();
        try
        {
            var space = new Space();
            space.Init(Cache!, s => SpaceStatus = s);
            Space = space;
            SpaceStatus = "ready";
        }
        finally { Gate.Release(); }
    }

    private void LogError(string e)
    {
        lock (RecentErrors)
        {
            RecentErrors.Add(e);
            if (RecentErrors.Count > 50) RecentErrors.RemoveAt(0);
        }
    }

    // ---------------------------------------------------------------- archetypes

    public IReadOnlyDictionary<uint, Archetype> Archetypes => _archetypes ?? new();

    public Archetype? GetArchetype(string nameOrHash)
    {
        uint hash = CwUtil.ParseHash(nameOrHash);
        return Cache?.GetArchetype(hash);
    }

    // ---------------------------------------------------------------- files

    /// <summary>All archive entries that have this exact file name (e.g. "prop_bench_01.ydr").</summary>
    public IReadOnlyList<RpfEntry> FindEntriesByName(string name)
        => _entriesByName != null && _entriesByName.TryGetValue(name, out var l) ? l : Array.Empty<RpfEntry>();

    public IEnumerable<RpfEntry> AllEntries => Cache?.RpfMan.EntryDict.Values ?? Enumerable.Empty<RpfEntry>();

    /// <summary>
    /// Opens a file either from disk (an existing path) or from the game archives
    /// (a full archive path like "x64a.rpf\levels\...\file.ydr", or just a unique file name).
    /// Resource data is returned decompressed, ready for CodeWalker's loaders.
    /// </summary>
    public async Task<CwFile> OpenAsync(string path, CancellationToken ct)
    {
        if (File.Exists(path))
        {
            byte[] data = await File.ReadAllBytesAsync(path, ct);
            var entry = CwUtil.CreateLooseEntry(Path.GetFileName(path), path, ref data);
            return new CwFile(entry, data, path, IsLoose: true);
        }

        await EnsureLoadedAsync(ct);
        var re = ResolveEntry(path);
        await Gate.WaitAsync(ct);
        try
        {
            byte[]? data = re.File.ExtractFile(re);
            if (data == null) throw new CwException($"Could not extract {re.Path}: {re.File.LastError}");
            return new CwFile(re, data, re.Path, IsLoose: false);
        }
        finally { Gate.Release(); }
    }

    public RpfFileEntry ResolveEntry(string path)
    {
        var rpfman = Cache!.RpfMan;
        string p = path.Trim().Replace('/', '\\');
        if (rpfman.GetEntry(p) is RpfFileEntry fe) return fe;

        if (!p.Contains('\\'))
        {
            var matches = FindEntriesByName(p).OfType<RpfFileEntry>().ToList();
            if (matches.Count == 0 && Path.GetExtension(p) == "")
                throw new CwException($"'{path}' isn't a file on disk or in the game archives. Include the extension (e.g. {p}.ydr) or use search_files.");
            if (matches.Count == 1) return matches[0];
            if (matches.Count > 1)
            {
                // Prefer the copy the game actually uses (DLC/patch overrides) when it's a streamed asset.
                var active = ActiveStreamingEntry(p);
                if (active != null) return active;
                return matches[0];
            }
        }
        throw new CwException($"'{path}' isn't a file on disk or in the game archives. Use search_files to find the right archive path.");
    }

    private RpfFileEntry? ActiveStreamingEntry(string fileName)
    {
        var cache = Cache!;
        uint h = JenkHash.GenHash(Path.GetFileNameWithoutExtension(fileName).ToLowerInvariant());
        return Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".ydr" => cache.GetYdrEntry(h),
            ".ydd" => cache.GetYddEntry(h),
            ".ytd" => cache.GetYtdEntry(h),
            ".ymap" => cache.GetYmapEntry(h),
            ".yft" => cache.GetYftEntry(h),
            ".ybn" => cache.GetYbnEntry(h),
            ".ycd" => cache.GetYcdEntry(h),
            ".ynv" => cache.GetYnvEntry(h),
            _ => null,
        };
    }

    // ---------------------------------------------------------------- ground probe

    /// <summary>
    /// Casts a ray straight down at (x, y) and returns the first collision hit, loading
    /// collision files as needed. Static map collision only, unless includeProps is set.
    /// </summary>
    public async Task<(bool hit, Vector3 pos, Vector3 normal, string? source)> ProbeDownAsync(float x, float y, float fromZ, bool includeProps, CancellationToken ct)
    {
        await EnsureSpaceAsync(ct);
        var ray = new Ray(new Vector3(x, y, fromZ), new Vector3(0, 0, -1));
        await Gate.WaitAsync(ct);
        try
        {
            SpaceRayIntersectResult res = default;
            for (int i = 0; i < 400; i++)
            {
                ct.ThrowIfCancellationRequested();
                res = Space!.RayIntersect(ray, fromZ + 2000.0f, null, includeProps);
                if (res.TestComplete) break;
                // Pump CodeWalker's loader so the collision files it just asked for get loaded.
                for (int j = 0; j < 20 && Cache!.ContentThreadProc(); j++) { }
                await Task.Delay(5, ct);
            }
            if (!res.Hit) return (false, default, default, null);
            string? src = res.HitYbn?.Name ?? res.HitEntity?.Archetype?.Name;
            return (true, res.Position, res.Normal, src);
        }
        finally { Gate.Release(); }
    }
}

public sealed record CwFile(RpfFileEntry Entry, byte[] Data, string Source, bool IsLoose);

/// <summary>An error whose message is shown to the client (the SDK hides other exception messages).</summary>
public sealed class CwException(string message) : ModelContextProtocol.McpException(message);
