using System.Xml;

namespace CodeWalker.Mcp;

/// <summary>
/// Where GTA V lives and which edition it is. Taken from environment variables if set,
/// otherwise from the settings CodeWalker itself saved (so it follows whatever folder you
/// picked in the CodeWalker app).
/// </summary>
public sealed class CwConfig
{
    public string GtaFolder { get; init; } = "";
    public bool Gen9 { get; init; }
    public string? Key { get; init; }
    public string Source { get; init; } = "";
    public string ExcludeFolders { get; init; } = "Installers;_CommonRedist";

    public static CwConfig Resolve()
    {
        string? envFolder = Environment.GetEnvironmentVariable("CODEWALKER_GTA_FOLDER");
        string? envGen9 = Environment.GetEnvironmentVariable("CODEWALKER_GEN9");

        var fromApp = ReadCodeWalkerUserConfig();

        string folder = !string.IsNullOrWhiteSpace(envFolder) ? envFolder! : fromApp?.folder ?? "";
        bool gen9;
        if (!string.IsNullOrWhiteSpace(envGen9)) gen9 = envGen9 is "1" or "true" or "True";
        else if (fromApp?.gen9 != null) gen9 = fromApp.Value.gen9.Value;
        else gen9 = File.Exists(Path.Combine(folder, "GTA5_Enhanced.exe")) && !File.Exists(Path.Combine(folder, "GTA5.exe"));

        return new CwConfig
        {
            GtaFolder = folder,
            Gen9 = gen9,
            Key = fromApp?.key,
            Source = !string.IsNullOrWhiteSpace(envFolder) ? "CODEWALKER_GTA_FOLDER" : fromApp?.path ?? "(not found)",
        };
    }

    private static (string folder, bool? gen9, string? key, string path)? ReadCodeWalkerUserConfig()
    {
        try
        {
            string root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "dexyfex_software");
            if (!Directory.Exists(root)) return null;
            var configs = Directory.GetDirectories(root, "CodeWalker.exe_*")
                .SelectMany(d => Directory.GetFiles(d, "user.config", SearchOption.AllDirectories))
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.LastWriteTimeUtc);
            foreach (var cfg in configs)
            {
                var doc = new XmlDocument();
                doc.Load(cfg.FullName);
                string? Get(string name) => doc.SelectSingleNode($"//setting[@name='{name}']/value")?.InnerText;
                string? folder = Get("GTAFolder");
                if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder)) continue;
                string? g9 = Get("GTAGen9");
                return (folder!, g9 == null ? null : g9.Equals("true", StringComparison.OrdinalIgnoreCase), Get("Key"), cfg.FullName);
            }
        }
        catch
        {
            // fall through: no usable settings
        }
        return null;
    }
}
