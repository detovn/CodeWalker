using System.ComponentModel;
using System.Text.RegularExpressions;
using CodeWalker.GameFiles;
using ModelContextProtocol.Server;
using SharpDX;

namespace CodeWalker.Mcp.Tools;

[McpServerToolType]
public static class GameTools
{
    [McpServerTool(Name = "status", ReadOnly = true, Idempotent = true)]
    [Description("Shows which GTA V install CodeWalker is using and whether the game files are loaded. Pass load=true to start loading now (takes ~30-90 s the first time; every other tool loads on demand).")]
    public static async Task<string> Status(CwGame game, CancellationToken ct,
        [Description("Start loading (and wait for) the game files now.")] bool load = false)
    {
        if (load) await game.EnsureLoadedAsync(ct);
        string[] errors;
        lock (game.RecentErrors) errors = game.RecentErrors.TakeLast(10).ToArray();
        return CwUtil.ToJson(new
        {
            gta_folder = game.Config.GtaFolder,
            edition = game.Config.Gen9 ? "Enhanced (gen9)" : "Legacy",
            config_source = game.Config.Source,
            game_files = game.Status,
            collision_world = game.SpaceStatus,
            selected_dlc_level = game.Cache?.SelectedDlc,
            recent_load_errors = errors.Length > 0 ? errors : null,
        });
    }

    [McpServerTool(Name = "search_files", ReadOnly = true, Idempotent = true)]
    [Description("Searches every file inside the game's RPF archives by name or path. Supports * and ? wildcards (e.g. 'prop_bench*.ydr', '*vinewood*.ymap'); without wildcards it matches any path containing the text. Returns full archive paths usable by the other tools.")]
    public static async Task<string> SearchFiles(CwGame game, CancellationToken ct,
        [Description("Name, path fragment or wildcard pattern. Case-insensitive.")] string query,
        [Description("Only return files with this extension, e.g. 'ydr', 'ymap', 'ytyp'.")] string? extension = null,
        [Description("Max results (default 50).")] int limit = 50)
    {
        await game.EnsureLoadedAsync(ct);
        string q = query.Trim().Replace('/', '\\').ToLowerInvariant();
        bool wild = q.Contains('*') || q.Contains('?');
        Regex? rx = wild ? new Regex("^" + Regex.Escape(q).Replace("\\*", ".*").Replace("\\?", ".") + "$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) : null;
        bool pathPattern = q.Contains('\\');
        string? ext = extension == null ? null : "." + extension.TrimStart('.').ToLowerInvariant();

        var hits = new List<object>();
        int total = 0;
        foreach (var e in game.AllEntries)
        {
            if (e is not RpfFileEntry fe) continue;
            if (ext != null && !fe.NameLower.EndsWith(ext, StringComparison.Ordinal)) continue;
            bool match = rx != null
                ? rx.IsMatch(pathPattern ? fe.Path : fe.NameLower)
                : fe.Path.Contains(q, StringComparison.OrdinalIgnoreCase);
            if (!match) continue;
            total++;
            if (hits.Count < limit)
                hits.Add(new { path = fe.Path, size = fe.GetFileSize(), type = fe is RpfResourceFileEntry ? "resource" : "binary" });
        }
        return CwUtil.ToJson(new { total_matches = total, shown = hits.Count, results = hits });
    }

    [McpServerTool(Name = "list_folder", ReadOnly = true, Idempotent = true)]
    [Description("Lists what's inside a folder or RPF archive in the game files, like the RPF Explorer. Pass an empty path to list the top-level archives.")]
    public static async Task<string> ListFolder(CwGame game, CancellationToken ct,
        [Description("Archive path such as 'x64a.rpf' or 'update\\update.rpf\\x64\\levels'. Empty = top level.")] string path = "",
        [Description("Max entries (default 300).")] int limit = 300)
    {
        await game.EnsureLoadedAsync(ct);
        var rpfman = game.Cache!.RpfMan;
        string p = path.Trim().Replace('/', '\\').TrimEnd('\\');
        if (p.Length == 0)
        {
            var roots = rpfman.AllRpfs.Where(r => r.Parent == null).Select(r => r.Path).Distinct().OrderBy(s => s).ToList();
            return CwUtil.ToJson(new { path = "", archives = roots.Take(limit), total = roots.Count });
        }
        RpfDirectoryEntry? dir = rpfman.GetEntry(p) as RpfDirectoryEntry;
        if (dir == null && rpfman.RpfDict.TryGetValue(p.ToLowerInvariant(), out var rpf)) dir = rpf.Root;
        if (dir == null)
        {
            // a nested .rpf file entry: find its RpfFile
            var asFile = rpfman.GetEntry(p);
            var child = asFile?.File?.Children?.FirstOrDefault(c => c.Path.Equals(p, StringComparison.OrdinalIgnoreCase));
            dir = child?.Root;
        }
        if (dir == null) throw new CwException($"'{path}' is not a folder or archive in the game files.");

        var dirs = dir.Directories.Select(d => d.Name + "\\").ToList();
        var files = dir.Files.Select(f => new { name = f.Name, size = f.GetFileSize() }).ToList();
        return CwUtil.ToJson(new
        {
            path = dir.Path,
            folders = dirs.Take(limit),
            files = files.Take(Math.Max(0, limit - dirs.Count)),
            total_folders = dirs.Count,
            total_files = files.Count,
        });
    }

    [McpServerTool(Name = "search_archetypes", ReadOnly = true, Idempotent = true)]
    [Description("Finds placeable models (archetypes) by name, across the base game and DLC. Use this to find prop names before placing them. Returns size, draw distance and the ytyp each comes from.")]
    public static async Task<string> SearchArchetypes(CwGame game, CancellationToken ct,
        [Description("Text to look for in the name, or a wildcard pattern like 'prop_*bench*'.")] string query,
        [Description("Max results (default 40).")] int limit = 40,
        [Description("Only include models no bigger than this (bounding sphere radius, metres).")] float? maxRadius = null)
    {
        await game.EnsureLoadedAsync(ct);
        string q = query.Trim().ToLowerInvariant();
        Regex? rx = q.Contains('*') || q.Contains('?')
            ? new Regex("^" + Regex.Escape(q).Replace("\\*", ".*").Replace("\\?", ".") + "$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
            : null;
        var list = new List<Archetype>();
        foreach (var a in game.Archetypes.Values)
        {
            string n = a.Name;
            if (n.StartsWith("hash_", StringComparison.Ordinal) && !q.StartsWith("hash_", StringComparison.Ordinal)) continue;
            if (rx != null ? !rx.IsMatch(n) : !n.Contains(q, StringComparison.OrdinalIgnoreCase)) continue;
            if (maxRadius.HasValue && a.BSRadius > maxRadius.Value) continue;
            list.Add(a);
        }
        var shown = list.OrderBy(a => a.Name.Length).ThenBy(a => a.Name, StringComparer.OrdinalIgnoreCase).Take(limit)
            .Select(a => new
            {
                name = a.Name,
                kind = ArchetypeKind(a),
                size = CwUtil.Arr(a.BBMax - a.BBMin),
                radius = CwUtil.R(a.BSRadius, 2),
                lod_dist = CwUtil.R(a.LodDist, 1),
                ytyp = a.Ytyp?.RpfFileEntry?.Path,
            });
        return CwUtil.ToJson(new { total_matches = list.Count, results = shown });
    }

    [McpServerTool(Name = "get_archetype", ReadOnly = true, Idempotent = true)]
    [Description("Full details for one model/archetype: bounds, draw distances, texture/drawable/collision dictionaries, flags, the files that make it up, and for interiors (MLOs) its rooms, portals and entity sets.")]
    public static async Task<string> GetArchetype(CwGame game, CancellationToken ct,
        [Description("Archetype name (e.g. prop_bench_01) or hash.")] string name)
    {
        await game.EnsureLoadedAsync(ct);
        var a = game.GetArchetype(name) ?? throw new CwException($"No archetype named '{name}'. Try search_archetypes.");
        var d = a._BaseArchetypeDef;
        var c = game.Cache!;
        uint assetHash = d.assetName.Hash != 0 ? d.assetName.Hash : d.name.Hash;
        string? drawablePath = d.assetType switch
        {
            rage__fwArchetypeDef__eAssetType.ASSET_TYPE_FRAGMENT => c.GetYftEntry(assetHash)?.Path,
            rage__fwArchetypeDef__eAssetType.ASSET_TYPE_DRAWABLEDICTIONARY => c.GetYddEntry(d.drawableDictionary.Hash)?.Path,
            _ => c.GetYdrEntry(assetHash)?.Path,
        };
        object? mlo = null;
        if (a is MloArchetype m)
        {
            mlo = new
            {
                rooms = m.rooms?.Select(r => new { index = r.Index, name = r.RoomName, bb_min = CwUtil.Arr(r._Data.bbMin), bb_max = CwUtil.Arr(r._Data.bbMax), entities = r.AttachedObjects?.Length ?? 0 }),
                portals = m.portals?.Select(p => new { index = p.Index, room_from = p._Data.roomFrom, room_to = p._Data.roomTo, flags = p._Data.flags }),
                entity_sets = m.entitySets?.Select(s => new { name = s.Name, entities = s.Entities?.Length ?? 0 }),
                entity_count = m.entities?.Length ?? 0,
            };
        }
        return CwUtil.ToJson(new
        {
            name = a.Name,
            hash = "0x" + a.Hash.Hash.ToString("X8"),
            kind = ArchetypeKind(a),
            asset_type = d.assetType.ToString(),
            asset_name = CwUtil.HashName(d.assetName.Hash),
            bb_min = CwUtil.Arr(a.BBMin),
            bb_max = CwUtil.Arr(a.BBMax),
            size = CwUtil.Arr(a.BBMax - a.BBMin),
            bs_centre = CwUtil.Arr(a.BSCenter),
            bs_radius = CwUtil.R(a.BSRadius, 3),
            lod_dist = d.lodDist,
            hd_texture_dist = d.hdTextureDist,
            flags = d.flags,
            special_attribute = d.specialAttribute,
            texture_dictionary = CwUtil.HashName(d.textureDictionary.Hash),
            drawable_dictionary = CwUtil.HashName(d.drawableDictionary.Hash),
            physics_dictionary = CwUtil.HashName(d.physicsDictionary.Hash),
            clip_dictionary = CwUtil.HashName(d.clipDictionary.Hash),
            ytyp = a.Ytyp?.RpfFileEntry?.Path,
            model_file = drawablePath,
            texture_file = d.textureDictionary.Hash != 0 ? c.GetYtdEntry(d.textureDictionary.Hash)?.Path : null,
            time_flags = a is TimeArchetype t ? t.TimeFlags : (uint?)null,
            mlo,
        });
    }

    [McpServerTool(Name = "hash", ReadOnly = true, Idempotent = true)]
    [Description("Computes the GTA (Jenkins one-at-a-time) hash of text, or looks up the name behind a hash if CodeWalker knows it. Accepts several values at once.")]
    public static async Task<string> Hash(CwGame game, CancellationToken ct,
        [Description("Strings to hash, or hashes to reverse (decimal, 0x hex, or hash_XXXXXXXX).")] string[] values,
        [Description("Load the game's name index first so more hashes can be reversed (slower the first time).")] bool useGameNames = true)
    {
        if (useGameNames) await game.EnsureLoadedAsync(ct);
        var rows = values.Select(v =>
        {
            string s = v.Trim();
            bool looksLikeHash = s.StartsWith("0x", StringComparison.OrdinalIgnoreCase) || s.StartsWith("hash_", StringComparison.OrdinalIgnoreCase) || (s.Length > 0 && s.All(char.IsDigit));
            if (looksLikeHash)
            {
                uint h = CwUtil.ParseHash(s);
                string name = JenkIndex.TryGetString(h);
                return (object)new { input = v, hash_uint = h, hash_hex = "0x" + h.ToString("X8"), hash_signed = unchecked((int)h), known_name = string.IsNullOrEmpty(name) ? null : name };
            }
            uint hh = JenkHash.GenHash(s.ToLowerInvariant());
            return new { input = v, hash_uint = hh, hash_hex = "0x" + hh.ToString("X8"), hash_signed = unchecked((int)hh), known_name = (string?)null };
        });
        return CwUtil.ToJson(rows);
    }

    [McpServerTool(Name = "ground_height", ReadOnly = true, Idempotent = true)]
    [Description("Finds the ground (static map collision) under one or more X,Y points by casting a ray straight down, like CodeWalker's snap-to-ground. Returns the hit height and surface normal. The first call loads the collision world (~10-30 s).")]
    public static async Task<string> GroundHeight(CwGame game, CancellationToken ct,
        [Description("Points as [x, y] or [x, y, startZ]. The ray starts at startZ (default 1500, i.e. above everything) and goes down.")] float[][] points,
        [Description("Also hit props/buildings' own collision, not just terrain and map collision.")] bool includeProps = false)
    {
        var results = new List<object>();
        foreach (var p in points)
        {
            if (p.Length < 2) throw new CwException("Each point needs at least [x, y].");
            float fromZ = p.Length > 2 ? p[2] : 1500f;
            var (hit, pos, normal, src) = await game.ProbeDownAsync(p[0], p[1], fromZ, includeProps, ct);
            results.Add(hit
                ? new { x = p[0], y = p[1], hit = true, z = (float?)CwUtil.R(pos.Z), normal = CwUtil.Arr(normal), slope_deg = (float?)CwUtil.R(MathUtil.RadiansToDegrees((float)Math.Acos(Math.Clamp(normal.Z, -1f, 1f))), 1), collision_file = src }
                : (object)new { x = p[0], y = p[1], hit = false });
        }
        return CwUtil.ToJson(results);
    }

    [McpServerTool(Name = "entities_near", ReadOnly = true, Idempotent = true)]
    [Description("Lists what the game already places near a world position: every entity (prop, building, tree...) from the game's ymaps within the radius, nearest first, with the ymap it lives in. Useful to see what's at a location before building there.")]
    public static async Task<string> EntitiesNear(CwGame game, CancellationToken ct,
        [Description("World position [x, y, z].")] float[] position,
        [Description("Search radius in metres (default 30).")] float radius = 30,
        [Description("Only HD entities (skip LOD/SLOD stand-ins). Default true.")] bool hdOnly = true,
        [Description("Optional text the archetype name must contain.")] string? nameContains = null,
        [Description("Max results (default 100).")] int limit = 100)
    {
        await game.EnsureLoadedAsync(ct);
        var center = CwUtil.Vec(position, Vector3.Zero);
        var cache = game.Cache!;
        var found = new List<(float dist, YmapEntityDef ent, string ymap)>();
        var nodes = cache.YmapHierarchyDict?.Values.Where(n =>
            n.streamingExtentsMin.X - radius <= center.X && center.X <= n.streamingExtentsMax.X + radius &&
            n.streamingExtentsMin.Y - radius <= center.Y && center.Y <= n.streamingExtentsMax.Y + radius &&
            n.entitiesExtentsMin.X - radius <= center.X && center.X <= n.entitiesExtentsMax.X + radius &&
            n.entitiesExtentsMin.Y - radius <= center.Y && center.Y <= n.entitiesExtentsMax.Y + radius &&
            (!hdOnly || (n.ContentFlags & 1) != 0)).ToList() ?? new();

        await game.Gate.WaitAsync(ct);
        try
        {
            foreach (var n in nodes)
            {
                var entry = cache.GetYmapEntry(n.Name);
                if (entry == null) continue;
                var ymap = cache.RpfMan.GetFile<YmapFile>(entry);
                if (ymap?.AllEntities == null) continue;
                foreach (var e in ymap.AllEntities)
                {
                    if (hdOnly && e._CEntityDef.lodLevel is not (rage__eLodType.LODTYPES_DEPTH_HD or rage__eLodType.LODTYPES_DEPTH_ORPHANHD)) continue;
                    float dist = Vector3.Distance(e.Position, center);
                    if (dist > radius) continue;
                    string an = CwUtil.HashName(e._CEntityDef.archetypeName.Hash);
                    if (nameContains != null && !an.Contains(nameContains, StringComparison.OrdinalIgnoreCase)) continue;
                    found.Add((dist, e, entry.Path));
                }
            }
        }
        finally { game.Gate.Release(); }

        var rows = found.OrderBy(f => f.dist).Take(limit).Select(f => new
        {
            archetype = CwUtil.HashName(f.ent._CEntityDef.archetypeName.Hash),
            distance = CwUtil.R(f.dist, 2),
            position = CwUtil.Arr(f.ent.Position),
            rotation = CwUtil.Arr(CwUtil.QuatToEuler(f.ent.Orientation)),
            scale = new[] { f.ent._CEntityDef.scaleXY, f.ent._CEntityDef.scaleZ },
            lod_level = f.ent._CEntityDef.lodLevel.ToString().Replace("LODTYPES_DEPTH_", ""),
            ymap = f.ymap,
            index = f.ent.Index,
        });
        return CwUtil.ToJson(new { ymaps_checked = nodes.Count, total_found = found.Count, results = rows });
    }

    internal static string ArchetypeKind(Archetype a) => a switch
    {
        MloArchetype => "interior (MLO)",
        TimeArchetype => "time-based",
        _ => a._BaseArchetypeDef.assetType switch
        {
            rage__fwArchetypeDef__eAssetType.ASSET_TYPE_FRAGMENT => "fragment (breakable/physics)",
            rage__fwArchetypeDef__eAssetType.ASSET_TYPE_DRAWABLEDICTIONARY => "drawable dictionary",
            _ => "drawable",
        },
    };
}
