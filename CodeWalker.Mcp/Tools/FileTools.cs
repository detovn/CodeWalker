using System.ComponentModel;
using System.Xml;
using CodeWalker.GameFiles;
using ModelContextProtocol.Server;

namespace CodeWalker.Mcp.Tools;

[McpServerToolType]
public static class FileTools
{
    [McpServerTool(Name = "export_xml", ReadOnly = false, Idempotent = true)]
    [Description("Converts a GTA file to CodeWalker's editable XML, the same as 'Export XML' in the RPF Explorer. Works for ymap, ytyp, ymt, ymf, ydr, ydd, yft, ytd, ybn, ynd, ynv, ycd, rel/dat audio, meta/pso, and more. The file can come from the game archives (archive path or unique file name) or from disk. Models and texture dictionaries also export their textures as .dds into a folder next to output_file.")]
    public static async Task<string> ExportXml(CwGame game, CancellationToken ct,
        [Description("Archive path (e.g. 'x64c.rpf\\levels\\gta5\\...\\foo.ymap'), unique file name (e.g. 'prop_bench_01.ydr') or a file path on disk.")] string path,
        [Description("Where to save the XML (e.g. C:\\mods\\foo.ymap.xml). If omitted, the XML is returned in the reply instead.")] string? outputFile = null,
        [Description("Replace output_file if it already exists.")] bool overwrite = true,
        [Description("When returning inline, cut the XML off after this many characters (default 60000).")] int maxChars = 60000)
    {
        var f = await game.OpenAsync(path, ct);
        string outFull = outputFile != null ? CwUtil.PrepareOutput(outputFile, overwrite) : "";
        string folder = outputFile != null ? Path.GetDirectoryName(outFull)! : "";
        string xml = MetaXml.GetXml(f.Entry, f.Data, out string suggested, folder);
        if (string.IsNullOrEmpty(xml))
            throw new CwException($"CodeWalker can't convert {f.Entry.Name} to XML (unsupported file type).");
        if (outputFile == null)
            return $"<!-- {f.Source} -> {suggested} -->\n" + CwUtil.Truncate(xml, maxChars);
        await File.WriteAllTextAsync(outFull, xml, ct);
        return CwUtil.ToJson(new { source = f.Source, saved_to = outFull, characters = xml.Length, suggested_name = suggested });
    }

    [McpServerTool(Name = "import_xml", Destructive = false)]
    [Description("Converts a CodeWalker XML file back into the game binary, the same as 'Import XML' in the RPF Explorer. The XML file name must end in the target type plus .xml (e.g. mymap.ymap.xml, prop_x.ydr.xml, props.ytd.xml). For models and texture dictionaries, the .dds textures are read from a folder named after the file next to the XML.")]
    public static async Task<string> ImportXml(CancellationToken ct,
        [Description("Path to the .xxx.xml file on disk.")] string xmlFile,
        [Description("Output path. Defaults to the XML path without '.xml'.")] string? outputFile = null,
        [Description("Replace the output if it exists.")] bool overwrite = true)
    {
        string full = Path.GetFullPath(xmlFile);
        if (!File.Exists(full)) throw new CwException($"{full} not found.");
        string fnamel = Path.GetFileName(full).ToLowerInvariant();
        if (!fnamel.EndsWith(".xml") || fnamel.IndexOf('.') == fnamel.LastIndexOf('.'))
            throw new CwException("The file name must look like name.<type>.xml, e.g. mymap.ymap.xml.");
        var fmt = XmlMeta.GetXMLFormat(fnamel, out int trim);
        string withoutXml = full.Substring(0, full.Length - trim);
        string fpathin = Path.Combine(Path.GetDirectoryName(withoutXml)!, Path.GetFileNameWithoutExtension(withoutXml));

        var doc = new XmlDocument();
        doc.LoadXml(await File.ReadAllTextAsync(full, ct));
        byte[] data = XmlMeta.GetData(doc, fmt, fpathin);
        if (data == null || data.Length == 0)
            throw new CwException($"CodeWalker couldn't build a {XmlMeta.GetXMLFormatName(fmt)} from this XML. Check the XML matches what export_xml produces.");

        string outFull = CwUtil.PrepareOutput(outputFile ?? withoutXml, overwrite);
        await File.WriteAllBytesAsync(outFull, data, ct);
        return CwUtil.ToJson(new { format = XmlMeta.GetXMLFormatName(fmt), saved_to = outFull, bytes = data.Length });
    }

    [McpServerTool(Name = "extract_file", Destructive = false)]
    [Description("Copies a file out of the game archives to disk as a normal loose file (resources keep their RSC7 header, so they open in CodeWalker/OpenIV and can be streamed). Good for starting a mod from a vanilla file.")]
    public static async Task<string> ExtractFile(CwGame game, CancellationToken ct,
        [Description("Archive path or unique file name, e.g. 'prop_bench_01.ydr'.")] string path,
        [Description("Where to save it. If this is an existing folder, the original file name is used.")] string outputFile,
        [Description("Replace the output if it exists.")] bool overwrite = false)
    {
        var f = await game.OpenAsync(path, ct);
        if (f.IsLoose) throw new CwException("That's already a file on disk; just copy it.");
        string target = Directory.Exists(outputFile) ? Path.Combine(outputFile, f.Entry.Name) : outputFile;
        string outFull = CwUtil.PrepareOutput(target, overwrite);
        byte[] bytes = CwUtil.ToLooseFileBytes(f.Entry, f.Data);
        await File.WriteAllBytesAsync(outFull, bytes, ct);
        return CwUtil.ToJson(new { source = f.Source, saved_to = outFull, bytes = bytes.Length });
    }

    [McpServerTool(Name = "pack_rpf", Destructive = false)]
    [Description("Builds a new RPF archive (unencrypted/OPEN, as mods use) from a folder on disk. Sub-folders become folders; a sub-folder whose name ends in .rpf becomes a nested archive (e.g. dlc.rpf\\x64\\levels\\gta5\\... ). Resource files (ydr, ymap, ytd...) are stored as resources automatically. .bak files are skipped.")]
    public static Task<string> PackRpf(CancellationToken ct,
        [Description("Folder whose contents go into the archive.")] string sourceFolder,
        [Description("Path of the .rpf to create, e.g. C:\\mods\\mymod\\dlc.rpf.")] string outputRpf,
        [Description("Replace the .rpf if it already exists.")] bool overwrite = false)
    {
        string src = Path.GetFullPath(sourceFolder);
        if (!Directory.Exists(src)) throw new CwException($"{src} is not a folder.");
        string outFull = CwUtil.PrepareOutput(outputRpf, overwrite);
        if (outFull.StartsWith(src.TrimEnd('\\') + "\\", StringComparison.OrdinalIgnoreCase))
            throw new CwException("Put the output .rpf outside the folder being packed.");
        if (File.Exists(outFull)) File.Delete(outFull);

        var rpf = RpfFile.CreateNew(Path.GetDirectoryName(outFull)!, Path.GetFileName(outFull));
        var errors = new List<string>();
        int files = 0, dirs = 0;
        long bytes = 0;
        AddFolder(rpf.Root, src);
        RpfFile.Defragment(rpf);
        return Task.FromResult(CwUtil.ToJson(new { saved_to = outFull, files, folders = dirs, bytes_in = bytes, size_on_disk = new FileInfo(outFull).Length, errors = errors.Count > 0 ? errors : null }));

        void AddFolder(RpfDirectoryEntry parent, string fsDir)
        {
            foreach (var sub in Directory.GetDirectories(fsDir).OrderBy(s => s, StringComparer.OrdinalIgnoreCase))
            {
                ct.ThrowIfCancellationRequested();
                string name = Path.GetFileName(sub);
                try
                {
                    if (name.EndsWith(".rpf", StringComparison.OrdinalIgnoreCase))
                    {
                        var child = RpfFile.CreateNew(parent, name);
                        AddFolder(child.Root, sub);
                    }
                    else
                    {
                        AddFolder(RpfFile.CreateDirectory(parent, name), sub);
                    }
                    dirs++;
                }
                catch (Exception ex) { errors.Add($"{sub}: {ex.Message}"); }
            }
            foreach (var file in Directory.GetFiles(fsDir).OrderBy(s => s, StringComparer.OrdinalIgnoreCase))
            {
                ct.ThrowIfCancellationRequested();
                if (file.EndsWith(".bak", StringComparison.OrdinalIgnoreCase)) continue; // edit backups made by this server
                try
                {
                    byte[] data = File.ReadAllBytes(file);
                    RpfFile.CreateFile(parent, Path.GetFileName(file), data);
                    files++;
                    bytes += data.Length;
                }
                catch (Exception ex) { errors.Add($"{file}: {ex.Message}"); }
            }
        }
    }
}
