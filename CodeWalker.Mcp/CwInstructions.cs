namespace CodeWalker.Mcp;

internal static class CwInstructions
{
    public const string Text = """
CodeWalker MCP: GTA V map and modding tools built on CodeWalker.Core. It reads the user's
GTA V install read-only and writes new files only where you tell it to.

The game index loads on first use (about 30-90 s, once per session); call status(load=true) to warm it up.
Ground probes load the collision world the first time (another ~10-30 s).

Coordinates are GTA world metres, Z up. Rotation is [x, y, z] degrees: pitch, roll and
heading (heading turns counter-clockwise seen from above; 0 = facing +Y/north).

Typical ways to build things:
- Place props: search_archetypes -> (ground_height / entities_near to scout) -> create_ymap with
  snapToGround -> create_project so the user can open it in CodeWalker and see it.
- Edit placements: read_ymap -> edit_ymap_entities / add_entities_to_ymap / remove_ymap_entities.
- Change a vanilla file: extract_file (or export_xml) -> edit -> import_xml. Never write into the game folder.
- Custom models: create_ytyp_from_folder on the folder of .ydr/.yft, then pass that ytyp in
  ytypFiles when creating ymaps with them.
- Ship it: put the files in a 'stream' folder (FiveM: plus fxmanifest.lua) or pack_rpf for a dlc.rpf.

Paths: game files take an archive path ('x64a.rpf\\levels\\...') or a unique file name ('prop_bench_01.ydr');
local files take an ordinary Windows path. Put outputs in the user's mod/project folder, not in the GTA folder.
""";
}
