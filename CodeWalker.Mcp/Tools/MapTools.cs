using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Xml;
using CodeWalker.GameFiles;
using ModelContextProtocol.Server;
using SharpDX;

namespace CodeWalker.Mcp.Tools;

/// <summary>One entity to place, or the changes to make to an existing one.</summary>
public sealed class EntitySpec
{
    [JsonPropertyName("index")]
    [Description("edit_ymap_entities only: index of the entity to change (from read_ymap).")]
    public int? Index { get; set; }

    [JsonPropertyName("archetype")]
    [Description("Model/archetype name, e.g. 'prop_bench_01'. Required when placing.")]
    public string? Archetype { get; set; }

    [JsonPropertyName("position")]
    [Description("World position [x, y, z] in metres. With snapToGround, z is the height to search down from (use a high value like 1000 to land on the topmost surface).")]
    public float[]? Position { get; set; }

    [JsonPropertyName("rotation")]
    [Description("Rotation [x, y, z] in degrees: pitch about X, roll about Y, heading about Z (counter-clockwise seen from above). Default [0,0,0].")]
    public float[]? Rotation { get; set; }

    [JsonPropertyName("heading")]
    [Description("Shortcut for rotation Z (degrees). Overrides rotation[2].")]
    public float? Heading { get; set; }

    [JsonPropertyName("quaternion")]
    [Description("World orientation as a quaternion [x, y, z, w]. Overrides rotation/heading.")]
    public float[]? Quaternion { get; set; }

    [JsonPropertyName("scale")]
    [Description("Uniform scale (default 1). Note the game ignores scale on many collidable props.")]
    public float? Scale { get; set; }

    [JsonPropertyName("scaleXY")]
    public float? ScaleXY { get; set; }

    [JsonPropertyName("scaleZ")]
    public float? ScaleZ { get; set; }

    [JsonPropertyName("lodDist")]
    [Description("Draw distance in metres. Default: the archetype's own lodDist.")]
    public float? LodDist { get; set; }

    [JsonPropertyName("flags")]
    [Description("Raw entity flags. Default 1572864 (casts static+dynamic shadows), +1 automatically when the rotation has pitch/roll (Allow Full Rotation). 32 = static entity.")]
    public uint? Flags { get; set; }

    [JsonPropertyName("snapToGround")]
    [Description("Drop onto the collision below (like the editor's snap to ground). The model's bottom is placed on the surface.")]
    public bool? SnapToGround { get; set; }

    [JsonPropertyName("alignToGround")]
    [Description("With snapToGround: also tilt to match the slope.")]
    public bool? AlignToGround { get; set; }

    [JsonPropertyName("groundOffset")]
    [Description("With snapToGround: extra height above the surface (metres, may be negative).")]
    public float? GroundOffset { get; set; }

    [JsonPropertyName("lodLevel")]
    [Description("ORPHANHD (default), HD, LOD, SLOD1, SLOD2, SLOD3 or SLOD4.")]
    public string? LodLevel { get; set; }

    [JsonPropertyName("parentIndex")]
    [Description("Index of the LOD parent entity in the parent ymap (default -1 = none).")]
    public int? ParentIndex { get; set; }
}

[McpServerToolType]
public static class MapTools
{
    private const uint DefaultFlags = 1572864; // cast static + dynamic shadows

    // ------------------------------------------------------------------ read

    [McpServerTool(Name = "read_ymap", ReadOnly = true, Idempotent = true)]
    [Description("Reads a ymap (map placement file) from disk or from the game, and lists its entities (index, model, position, rotation in degrees, scale, draw distance, flags) plus its extents and counts of car generators, grass, lights and occluders. Use the indices with edit_ymap_entities / remove_ymap_entities.")]
    public static async Task<string> ReadYmap(CwGame game, CancellationToken ct,
        [Description("Path on disk, archive path, or unique file name (e.g. 'hei_dt1_02_strm_0.ymap').")] string path,
        [Description("Max entities to list (default 300).")] int maxEntities = 300,
        [Description("Skip this many entities first (paging).")] int offset = 0)
    {
        var f = await game.OpenAsync(path, ct);
        var ymap = RpfFile.GetFile<YmapFile>(f.Entry, f.Data) ?? throw new CwException("Couldn't read that ymap.");
        var md = ymap._CMapData;
        var ents = ymap.AllEntities ?? Array.Empty<YmapEntityDef>();
        return CwUtil.ToJson(new
        {
            source = f.Source,
            name = CwUtil.HashName(md.name.Hash),
            parent = md.parent.Hash != 0 ? CwUtil.HashName(md.parent.Hash) : null,
            flags = md.flags,
            content_flags = md.contentFlags,
            streaming_extents = new[] { CwUtil.Arr(md.streamingExtentsMin), CwUtil.Arr(md.streamingExtentsMax) },
            entities_extents = new[] { CwUtil.Arr(md.entitiesExtentsMin), CwUtil.Arr(md.entitiesExtentsMax) },
            entity_count = ents.Length,
            interior_instances = ymap.MloEntities?.Length ?? 0,
            car_generators = ymap.CarGenerators?.Length ?? 0,
            grass_batches = ymap.GrassInstanceBatches?.Length ?? 0,
            lod_lights = ymap.LODLights?.direction?.Length ?? 0,
            box_occluders = ymap.BoxOccluders?.Length ?? 0,
            occlude_models = ymap.OccludeModels?.Length ?? 0,
            entities = ents.Skip(offset).Take(maxEntities).Select(DescribeEntity),
        });
    }

    internal static object DescribeEntity(YmapEntityDef e)
    {
        var d = e._CEntityDef;
        return new
        {
            index = e.Index,
            archetype = CwUtil.HashName(d.archetypeName.Hash),
            position = CwUtil.Arr(e.Position),
            rotation = CwUtil.Arr(CwUtil.QuatToEuler(e.Orientation)),
            scale = new[] { d.scaleXY, d.scaleZ },
            lod_dist = d.lodDist,
            lod_level = d.lodLevel.ToString().Replace("LODTYPES_DEPTH_", ""),
            parent_index = d.parentIndex,
            flags = d.flags,
            is_interior = e.MloInstance != null ? true : (bool?)null,
        };
    }

    // ------------------------------------------------------------------ create / edit

    [McpServerTool(Name = "create_ymap", Destructive = false)]
    [Description("Creates a new .ymap that places models in the world, like making a ymap in a CodeWalker project and adding entities. Extents and flags are computed from the real model bounds. Optionally snaps each entity to the ground. Custom (non-vanilla) models: pass their .ytyp in ytypFiles so their sizes are known.")]
    public static async Task<string> CreateYmap(CwGame game, CancellationToken ct,
        [Description("Path of the .ymap to write, e.g. C:\\mods\\mymod\\stream\\my_bench_area.ymap. The file name becomes the ymap name.")] string outputFile,
        [Description("Entities to place.")] EntitySpec[] entities,
        [Description("Optional parent ymap name (for LOD hierarchies). Usually leave empty.")] string? parent = null,
        [Description("Optional .ytyp files on disk that define custom models used here.")] string[]? ytypFiles = null,
        [Description("Also write a .ymap.xml next to it for review/editing.")] bool alsoWriteXml = false,
        [Description("Replace the file if it exists.")] bool overwrite = false)
    {
        await game.EnsureLoadedAsync(ct);
        string outFull = CwUtil.PrepareOutput(outputFile, overwrite);
        RegisterYtyps(game, ytypFiles);

        var ymap = new YmapFile { RpfFileEntry = new RpfResourceFileEntry(), Loaded = true };
        ymap.SetFilePath(outFull);
        if (!string.IsNullOrWhiteSpace(parent)) ymap._CMapData.parent = CwUtil.ParseHash(parent!.Replace(".ymap", ""));

        var warnings = new List<string>();
        var placed = new List<object>();
        foreach (var spec in entities)
        {
            var ent = await NewEntity(game, ymap, spec, warnings, ct);
            ymap.AddEntity(ent);
            placed.Add(DescribeEntity(ent));
        }
        return await SaveYmap(ymap, outFull, alsoWriteXml, warnings, placed, ct);
    }

    [McpServerTool(Name = "add_entities_to_ymap", Destructive = false)]
    [Description("Adds more entities to an existing .ymap file on disk (keeps everything already in it) and recomputes its extents and flags. A .bak copy of the original is kept when saving in place.")]
    public static async Task<string> AddEntities(CwGame game, CancellationToken ct,
        [Description("The .ymap on disk to add to.")] string ymapFile,
        [Description("Entities to place.")] EntitySpec[] entities,
        [Description("Save to a different file instead of updating ymapFile in place.")] string? outputFile = null,
        [Description("Optional .ytyp files on disk that define custom models used here.")] string[]? ytypFiles = null,
        [Description("Also write a .ymap.xml next to it.")] bool alsoWriteXml = false)
    {
        var (ymap, outFull) = await OpenYmapForEdit(game, ymapFile, outputFile, ytypFiles, ct);
        var warnings = new List<string>();
        var placed = new List<object>();
        foreach (var spec in entities)
        {
            var ent = await NewEntity(game, ymap, spec, warnings, ct);
            ymap.AddEntity(ent);
            placed.Add(DescribeEntity(ent));
        }
        return await SaveYmap(ymap, outFull, alsoWriteXml, warnings, placed, ct);
    }

    [McpServerTool(Name = "edit_ymap_entities", Destructive = false)]
    [Description("Changes existing entities in a .ymap on disk by index (from read_ymap): move, rotate, rescale, swap model, change draw distance or flags, or snap to ground. Only the fields you give are changed. A .bak copy is kept when saving in place.")]
    public static async Task<string> EditEntities(CwGame game, CancellationToken ct,
        [Description("The .ymap on disk to change.")] string ymapFile,
        [Description("Changes; each must include 'index'.")] EntitySpec[] edits,
        [Description("Save to a different file instead of updating in place.")] string? outputFile = null,
        [Description("Optional .ytyp files on disk that define custom models used here.")] string[]? ytypFiles = null)
    {
        var (ymap, outFull) = await OpenYmapForEdit(game, ymapFile, outputFile, ytypFiles, ct);
        var ents = ymap.AllEntities ?? Array.Empty<YmapEntityDef>();
        var warnings = new List<string>();
        var changed = new List<object>();
        foreach (var spec in edits)
        {
            if (spec.Index is not int i || i < 0 || i >= ents.Length)
                throw new CwException($"Edit needs a valid 'index' (0..{ents.Length - 1}).");
            await ApplySpec(game, ents[i], spec, isNew: false, warnings, ct);
            changed.Add(DescribeEntity(ents[i]));
        }
        return await SaveYmap(ymap, outFull, false, warnings, changed, ct);
    }

    [McpServerTool(Name = "remove_ymap_entities", Destructive = true)]
    [Description("Removes entities from a .ymap on disk: by index, by model name, and/or everything within a radius of a point. A .bak copy is kept when saving in place.")]
    public static async Task<string> RemoveEntities(CwGame game, CancellationToken ct,
        [Description("The .ymap on disk.")] string ymapFile,
        [Description("Entity indices to remove.")] int[]? indices = null,
        [Description("Remove every entity using this model name.")] string? archetype = null,
        [Description("Remove entities within 'radius' of this [x, y, z].")] float[]? near = null,
        [Description("Radius for 'near' (metres).")] float radius = 5,
        [Description("Save to a different file instead of updating in place.")] string? outputFile = null)
    {
        var (ymap, outFull) = await OpenYmapForEdit(game, ymapFile, outputFile, null, ct);
        var ents = ymap.AllEntities ?? Array.Empty<YmapEntityDef>();
        var idx = new HashSet<int>(indices ?? Array.Empty<int>());
        uint? archHash = archetype != null ? CwUtil.ParseHash(archetype) : null;
        Vector3? c = near != null ? CwUtil.Vec(near, Vector3.Zero) : null;
        var toRemove = ents.Where(e => idx.Contains(e.Index)
            || (archHash.HasValue && e._CEntityDef.archetypeName.Hash == archHash.Value)
            || (c.HasValue && Vector3.Distance(e.Position, c.Value) <= radius)).ToList();
        if (toRemove.Count == 0) throw new CwException("Nothing matched, so nothing was removed.");
        var removed = toRemove.Select(DescribeEntity).ToList();
        foreach (var e in toRemove.OrderByDescending(e => e.Index)) ymap.RemoveEntity(e);
        return await SaveYmap(ymap, outFull, false, new List<string>(), removed, ct, "removed");
    }

    // ------------------------------------------------------------------ ytyp / project

    [McpServerTool(Name = "create_ytyp_from_folder", Destructive = false)]
    [Description("Builds a .ytyp for a folder of custom models in one go (same as CodeWalker's 'Auto Setup Ytyp from Folder'): every .ydr/.yft (sub-folders included) becomes an archetype named after the file, with bounds read from the model, texture dictionary linked if a same-named .ytd sits beside it (or textures are embedded), collision linked if a same-named .ybn exists (or is embedded), and a draw distance chosen from its size (60-300 m). If outputFile already exists, new models are added to it and existing names are skipped.")]
    public static async Task<string> CreateYtypFromFolder(CwGame game, CancellationToken ct,
        [Description("Folder containing the .ydr/.yft models.")] string folder,
        [Description("The .ytyp to create or extend, e.g. C:\\mods\\mymod\\stream\\mymod_props.ytyp.")] string outputFile)
    {
        string dir = Path.GetFullPath(folder);
        if (!Directory.Exists(dir)) throw new CwException($"{dir} is not a folder.");
        string outFull = Path.GetFullPath(outputFile);
        Directory.CreateDirectory(Path.GetDirectoryName(outFull)!);

        var files = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories).OrderBy(f => f, StringComparer.OrdinalIgnoreCase).ToList();
        var models = files.Where(f => f.EndsWith(".ydr", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".yft", StringComparison.OrdinalIgnoreCase)).ToList();
        var sidecars = new HashSet<string>(files.Where(f => f.EndsWith(".ytd", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".ybn", StringComparison.OrdinalIgnoreCase)).Select(Path.GetFileName)!, StringComparer.OrdinalIgnoreCase);
        if (models.Count == 0) throw new CwException("No .ydr or .yft files in that folder.");

        YtypFile ytyp;
        bool existed = File.Exists(outFull);
        if (existed)
        {
            byte[] data = await File.ReadAllBytesAsync(outFull, ct);
            var entry = CwUtil.CreateLooseEntry(Path.GetFileName(outFull), outFull, ref data);
            ytyp = RpfFile.GetFile<YtypFile>(entry, data) ?? throw new CwException("Couldn't read the existing ytyp.");
        }
        else
        {
            ytyp = NewYtyp(outFull);
        }

        var existing = new HashSet<uint>((ytyp.AllArchetypes ?? Array.Empty<Archetype>()).Select(a => a._BaseArchetypeDef.name.Hash));
        var added = new List<object>();
        int skipped = 0;
        var failed = new List<string>();
        foreach (var file in models)
        {
            ct.ThrowIfCancellationRequested();
            string name = Path.GetFileNameWithoutExtension(file).ToLowerInvariant();
            uint hash = JenkHash.GenHash(name);
            if (existing.Contains(hash)) { skipped++; continue; }
            try
            {
                byte[] data = await File.ReadAllBytesAsync(file, ct);
                bool isFrag = file.EndsWith(".yft", StringComparison.OrdinalIgnoreCase);
                DrawableBase? drawable;
                bool embeddedTex, embeddedBound = false;
                if (isFrag)
                {
                    var yft = new YftFile();
                    RpfFile.LoadResourceFile(yft, data, 162);
                    drawable = yft.Fragment?.Drawable;
                    embeddedTex = drawable?.ShaderGroup?.TextureDictionary != null;
                }
                else
                {
                    var ydr = new YdrFile();
                    RpfFile.LoadResourceFile(ydr, data, 165);
                    drawable = ydr.Drawable;
                    embeddedTex = drawable?.ShaderGroup?.TextureDictionary != null;
                    embeddedBound = ydr.Drawable?.Bound != null;
                }
                if (drawable == null) { failed.Add(name + " (couldn't read model)"); continue; }
                JenkIndex.Ensure(name);

                float radius = drawable.BoundingSphereRadius;
                float lod = Math.Min(Math.Max(60.0f, radius * 20.0f), 300.0f);
                var arch = ytyp.AddArchetype();
                var def = arch._BaseArchetypeDef;
                def.name = hash;
                def.assetName = hash;
                def.assetType = isFrag ? rage__fwArchetypeDef__eAssetType.ASSET_TYPE_FRAGMENT : rage__fwArchetypeDef__eAssetType.ASSET_TYPE_DRAWABLE;
                def.specialAttribute = 0;
                def.flags = 32;
                def.bbMin = drawable.BoundingBoxMin;
                def.bbMax = drawable.BoundingBoxMax;
                def.bsCentre = drawable.BoundingCenter;
                def.bsRadius = radius;
                def.lodDist = lod;
                def.hdTextureDist = Math.Min(lod, 60.0f);
                bool tex = embeddedTex || sidecars.Contains(name + ".ytd");
                bool col = embeddedBound || sidecars.Contains(name + ".ybn");
                if (tex) def.textureDictionary = hash;
                if (col) def.physicsDictionary = hash;
                arch.Init(ytyp, ref def);
                existing.Add(hash);
                if (game.IsLoaded) game.Cache!.AddProjectArchetype(arch);
                added.Add(new { name, type = isFrag ? "fragment" : "drawable", size = CwUtil.Arr(def.bbMax - def.bbMin), lod_dist = CwUtil.R(lod, 1), textures = tex, collision = col });
            }
            catch (Exception ex)
            {
                failed.Add($"{name} ({ex.Message})");
            }
        }

        byte[] outBytes = ytyp.Save();
        await File.WriteAllBytesAsync(outFull, outBytes, ct);
        return CwUtil.ToJson(new
        {
            saved_to = outFull,
            extended_existing = existed,
            added_count = added.Count,
            skipped_existing = skipped,
            failed = failed.Count > 0 ? failed : null,
            added,
            note = "Stream the model files (.ydr/.yft/.ytd/.ybn) together with this .ytyp under the same names.",
        });
    }

    [McpServerTool(Name = "create_project", Destructive = false)]
    [Description("Writes a CodeWalker project (.cwproj) that lists your files, so you can open them all at once in CodeWalker's Project window (File > Open Project) to view and keep editing visually. Accepts ymap, ytyp, ybn, ynd, ynv, ydr, ydd, yft, ytd, rel/dat151 audio, and scenario .ymt files.")]
    public static async Task<string> CreateProject(CancellationToken ct,
        [Description("Path of the .cwproj to write.")] string projectFile,
        [Description("Files to include (paths on disk). Folders are expanded to the supported files inside them.")] string[] files,
        [Description("Project name (default: the file name).")] string? name = null,
        [Description("Replace the project if it exists.")] bool overwrite = true)
    {
        string projFull = CwUtil.PrepareOutput(projectFile.EndsWith(".cwproj", StringComparison.OrdinalIgnoreCase) ? projectFile : projectFile + ".cwproj", overwrite);
        string projDir = Path.GetDirectoryName(projFull)! + "\\";
        var sections = new[] { "YmapFilenames", "YtypFilenames", "YbnFilenames", "YndFilenames", "YnvFilenames", "TrainsFilenames", "ScenarioFilenames", "AudioRelFilenames", "YdrFilenames", "YddFilenames", "YftFilenames", "YtdFilenames" }
            .ToDictionary(s => s, _ => new List<string>());
        var ignored = new List<string>();

        var all = new List<string>();
        foreach (var f in files)
        {
            string full = Path.GetFullPath(f);
            if (Directory.Exists(full)) all.AddRange(Directory.GetFiles(full, "*.*", SearchOption.AllDirectories));
            else if (File.Exists(full)) all.Add(full);
            else ignored.Add(f + " (not found)");
        }
        foreach (var full in all.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            string l = full.ToLowerInvariant();
            string? section = Path.GetExtension(l) switch
            {
                ".ymap" => "YmapFilenames",
                ".ytyp" => "YtypFilenames",
                ".ybn" => "YbnFilenames",
                ".ynd" => "YndFilenames",
                ".ynv" => "YnvFilenames",
                ".ydr" => "YdrFilenames",
                ".ydd" => "YddFilenames",
                ".yft" => "YftFilenames",
                ".ytd" => "YtdFilenames",
                ".rel" => "AudioRelFilenames",
                ".ymt" => "ScenarioFilenames",
                ".dat" when l.Contains("trains") => "TrainsFilenames",
                ".dat" when l.Contains(".dat1") || l.EndsWith(".dat151.rel") => "AudioRelFilenames",
                _ => null,
            };
            if (section == null) { if (files.Length < 50) ignored.Add(Path.GetFileName(full)); continue; }
            string rel = Uri.UnescapeDataString(new Uri(projDir).MakeRelativeUri(new Uri(full)).ToString()).Replace('/', '\\').ToLowerInvariant();
            sections[section].Add(rel);
        }

        var doc = new XmlDocument();
        var root = doc.CreateElement("CodeWalkerProject");
        doc.AppendChild(root);
        var n = doc.CreateElement("Name");
        n.InnerText = name ?? Path.GetFileNameWithoutExtension(projFull);
        root.AppendChild(n);
        var v = doc.CreateElement("Version");
        v.SetAttribute("value", "1");
        root.AppendChild(v);
        foreach (var (sec, list) in sections)
        {
            var el = doc.CreateElement(sec);
            foreach (var item in list)
            {
                var it = doc.CreateElement("Item");
                it.InnerText = item;
                el.AppendChild(it);
            }
            root.AppendChild(el);
        }
        await using (var w = XmlWriter.Create(projFull, new XmlWriterSettings { Indent = true, Async = true }))
            doc.Save(w);
        return CwUtil.ToJson(new
        {
            saved_to = projFull,
            included = sections.Where(s => s.Value.Count > 0).ToDictionary(s => s.Key, s => s.Value),
            ignored = ignored.Count > 0 ? ignored : null,
        });
    }

    // ------------------------------------------------------------------ helpers

    internal static YtypFile NewYtyp(string path)
    {
        string fname = Path.GetFileName(path);
        string shortName = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
        var ytyp = new YtypFile
        {
            RpfFileEntry = new RpfResourceFileEntry { Name = fname, NameLower = fname.ToLowerInvariant() },
            Name = fname,
            FilePath = path,
            Loaded = true,
        };
        JenkIndex.Ensure(shortName);
        ytyp.NameHash = JenkHash.GenHash(shortName);
        ytyp._CMapTypes.name = ytyp.NameHash;
        return ytyp;
    }

    private static void RegisterYtyps(CwGame game, string[]? ytypFiles)
    {
        if (ytypFiles == null) return;
        foreach (var p in ytypFiles)
        {
            string full = Path.GetFullPath(p);
            if (!File.Exists(full)) throw new CwException($"ytyp not found: {full}");
            byte[] data = File.ReadAllBytes(full);
            var entry = CwUtil.CreateLooseEntry(Path.GetFileName(full), full, ref data);
            var ytyp = RpfFile.GetFile<YtypFile>(entry, data) ?? throw new CwException($"Couldn't read {full}");
            foreach (var a in ytyp.AllArchetypes ?? Array.Empty<Archetype>())
            {
                JenkIndex.Ensure(a.Name);
                game.Cache!.AddProjectArchetype(a);
            }
        }
    }

    private static async Task<(YmapFile ymap, string outFull)> OpenYmapForEdit(CwGame game, string ymapFile, string? outputFile, string[]? ytypFiles, CancellationToken ct)
    {
        await game.EnsureLoadedAsync(ct);
        string full = Path.GetFullPath(ymapFile);
        if (!File.Exists(full))
            throw new CwException($"{full} not found. To change a vanilla ymap, extract_file it first, then edit the copy.");
        RegisterYtyps(game, ytypFiles);
        byte[] data = await File.ReadAllBytesAsync(full, ct);
        var entry = CwUtil.CreateLooseEntry(Path.GetFileName(full), full, ref data);
        var ymap = RpfFile.GetFile<YmapFile>(entry, data) ?? throw new CwException("Couldn't read that ymap.");
        ymap.FilePath = full;
        await game.Gate.WaitAsync(ct);
        try { ymap.InitYmapEntityArchetypes(game.Cache!); }
        finally { game.Gate.Release(); }

        string outFull;
        if (outputFile == null)
        {
            File.Copy(full, full + ".bak", overwrite: true);
            outFull = full;
        }
        else
        {
            outFull = CwUtil.PrepareOutput(outputFile, overwrite: true);
            ymap.SetFilePath(outFull);
        }
        return (ymap, outFull);
    }

    private static async Task<string> SaveYmap(YmapFile ymap, string outFull, bool alsoXml, List<string> warnings, List<object> touched, CancellationToken ct, string touchedLabel = "entities")
    {
        ymap.CalcFlags();
        ymap.CalcExtents();
        byte[] bytes = ymap.Save();
        await File.WriteAllBytesAsync(outFull, bytes, ct);
        string? xmlPath = null;
        if (alsoXml)
        {
            xmlPath = outFull + ".xml";
            await File.WriteAllTextAsync(xmlPath, MetaXml.GetXml(ymap, out _), ct);
        }
        var md = ymap._CMapData;
        return CwUtil.ToJson(new
        {
            saved_to = outFull,
            xml = xmlPath,
            backup = File.Exists(outFull + ".bak") ? outFull + ".bak" : null,
            ymap_name = Path.GetFileNameWithoutExtension(outFull),
            total_entities = ymap.AllEntities?.Length ?? 0,
            flags = md.flags,
            content_flags = md.contentFlags,
            entities_extents = new[] { CwUtil.Arr(md.entitiesExtentsMin), CwUtil.Arr(md.entitiesExtentsMax) },
            streaming_extents = new[] { CwUtil.Arr(md.streamingExtentsMin), CwUtil.Arr(md.streamingExtentsMax) },
            warnings = warnings.Count > 0 ? warnings : null,
            details = new Dictionary<string, object> { [touchedLabel] = touched },
        });
    }

    private static async Task<YmapEntityDef> NewEntity(CwGame game, YmapFile ymap, EntitySpec spec, List<string> warnings, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(spec.Archetype)) throw new CwException("Every new entity needs an 'archetype'.");
        if (spec.Position == null) throw new CwException($"Entity '{spec.Archetype}' needs a 'position' [x, y, z].");
        var cent = new CEntityDef
        {
            archetypeName = new MetaHash(CwUtil.ParseHash(spec.Archetype!)),
            position = CwUtil.Vec(spec.Position, Vector3.Zero),
            rotation = new Vector4(0, 0, 0, 1),
            scaleXY = 1.0f,
            scaleZ = 1.0f,
            flags = DefaultFlags,
            parentIndex = -1,
            lodDist = 100.0f,
            lodLevel = rage__eLodType.LODTYPES_DEPTH_ORPHANHD,
            priorityLevel = rage__ePriorityLevel.PRI_REQUIRED,
            ambientOcclusionMultiplier = 255,
            artificialAmbientOcclusion = 255,
        };
        var ent = new YmapEntityDef(ymap, 0, ref cent);
        await ApplySpec(game, ent, spec, isNew: true, warnings, ct);
        return ent;
    }

    /// <summary>Applies the given fields to an entity (all of them for a new one).</summary>
    private static async Task ApplySpec(CwGame game, YmapEntityDef ent, EntitySpec spec, bool isNew, List<string> warnings, CancellationToken ct)
    {
        string label = spec.Index.HasValue ? $"entity {spec.Index}" : spec.Archetype ?? "entity";

        if (spec.Archetype != null)
        {
            uint h = CwUtil.ParseHash(spec.Archetype);
            ent._CEntityDef.archetypeName = new MetaHash(h);
            var arch = game.Cache!.GetArchetype(h);
            if (arch == null)
                warnings.Add($"{label}: '{spec.Archetype}' isn't a known model (not in the game or the given ytypFiles). It was still placed, but extents use a guess and the game will only show it if you stream a model with that name.");
            else
                ent.SetArchetype(arch);
            ent.UpdateEntityHash();
        }
        var archetype = ent.Archetype;

        if (spec.LodDist.HasValue) ent._CEntityDef.lodDist = spec.LodDist.Value;
        else if (isNew && archetype != null && archetype.LodDist > 0) ent._CEntityDef.lodDist = archetype.LodDist;
        if (spec.LodLevel != null)
        {
            if (!Enum.TryParse<rage__eLodType>("LODTYPES_DEPTH_" + spec.LodLevel.ToUpperInvariant(), out var ll))
                throw new CwException($"{label}: unknown lodLevel '{spec.LodLevel}'.");
            ent._CEntityDef.lodLevel = ll;
        }
        if (spec.ParentIndex.HasValue) ent._CEntityDef.parentIndex = spec.ParentIndex.Value;

        // scale
        if (spec.Scale.HasValue || spec.ScaleXY.HasValue || spec.ScaleZ.HasValue)
        {
            float sxy = spec.ScaleXY ?? spec.Scale ?? ent._CEntityDef.scaleXY;
            float sz = spec.ScaleZ ?? spec.Scale ?? ent._CEntityDef.scaleZ;
            ent.SetScale(new Vector3(sxy, sxy, sz));
        }

        // orientation
        bool rotGiven = spec.Quaternion != null || spec.Rotation != null || spec.Heading.HasValue;
        Quaternion ori = ent.Orientation;
        if (spec.Quaternion != null)
        {
            if (spec.Quaternion.Length != 4) throw new CwException($"{label}: quaternion must be [x, y, z, w].");
            ori = Quaternion.Normalize(new Quaternion(spec.Quaternion[0], spec.Quaternion[1], spec.Quaternion[2], spec.Quaternion[3]));
        }
        else if (spec.Rotation != null || spec.Heading.HasValue)
        {
            var cur = CwUtil.QuatToEuler(ent.Orientation);
            var r = spec.Rotation != null ? CwUtil.Vec(spec.Rotation, Vector3.Zero) : (isNew ? Vector3.Zero : cur);
            if (spec.Heading.HasValue) r.Z = spec.Heading.Value;
            ori = CwUtil.EulerToQuat(r.X, r.Y, r.Z);
        }

        // position (+ ground snap)
        Vector3 pos = spec.Position != null ? CwUtil.Vec(spec.Position, ent.Position) : ent.Position;
        if (spec.SnapToGround == true)
        {
            var (hit, gpos, normal, _) = await game.ProbeDownAsync(pos.X, pos.Y, pos.Z + (spec.Position != null ? 2.0f : 5.0f), false, ct);
            if (!hit)
            {
                warnings.Add($"{label}: no ground found below [{pos.X:0.##}, {pos.Y:0.##}, {pos.Z:0.##}]; left at the given height.");
            }
            else
            {
                if (spec.AlignToGround == true && normal.Z > 0.05f)
                {
                    var axis = Vector3.Cross(Vector3.UnitZ, normal);
                    float angle = (float)Math.Acos(Math.Clamp(normal.Z, -1f, 1f));
                    if (axis.LengthSquared() > 1e-8f)
                    {
                        axis.Normalize();
                        var m = Matrix.RotationQuaternion(ori) * Matrix.RotationAxis(axis, angle);
                        ori = Quaternion.Normalize(Quaternion.RotationMatrix(m));
                        rotGiven = true;
                    }
                }
                float bottom = archetype != null ? archetype.BBMin.Z * ent._CEntityDef.scaleZ : 0f;
                pos = new Vector3(pos.X, pos.Y, gpos.Z - bottom + (spec.GroundOffset ?? 0f));
            }
        }
        else if (spec.GroundOffset.HasValue)
        {
            pos.Z += spec.GroundOffset.Value;
        }

        ent.SetPosition(pos);
        if (rotGiven || isNew) ent.SetOrientation(ori);

        // flags
        if (spec.Flags.HasValue)
        {
            ent._CEntityDef.flags = spec.Flags.Value;
        }
        else if (rotGiven)
        {
            var e = CwUtil.QuatToEuler(ori);
            bool tilted = Math.Abs(e.X) > 0.01f || Math.Abs(e.Y) > 0.01f;
            ent._CEntityDef.flags = tilted ? ent._CEntityDef.flags | 1u : ent._CEntityDef.flags & ~1u;
        }
    }
}
