# CodeWalker MCP server

A Model Context Protocol server that lets Claude (or any MCP client) use CodeWalker's engine
to look around GTA V's files and build map content: place props in new ymaps, edit placements,
set up ytyps for custom models, convert files to and from XML, and pack RPFs.

It runs headless on top of `CodeWalker.Core`. It does not drive the CodeWalker window. Instead
it writes files plus a `.cwproj` you can open in CodeWalker to see the result.

## Setup

```
dotnet publish CodeWalker.Mcp -c Release -o Build\CodeWalker.Mcp
claude mcp add --scope user codewalker -- "<repo>\Build\CodeWalker.Mcp\CodeWalker.Mcp.exe"
```

The GTA V folder and edition are read from CodeWalker's own saved settings (whatever you
picked in the app). Override them with the environment variables `CODEWALKER_GTA_FOLDER`
and `CODEWALKER_GEN9=1`.

The game index loads on first use and is kept for the session: about 6 s on this PC, holding
around 1 GB of memory. The collision world used for ground snapping loads on the first probe.

## Tools

| Tool | What it does |
|---|---|
| `status` | GTA folder, edition, load state; `load=true` warms up the index |
| `search_files`, `list_folder` | browse/search everything inside the RPF archives |
| `search_archetypes`, `get_archetype` | find placeable models and their size, draw distance and files |
| `entities_near` | what the game already places around a point |
| `ground_height` | raycast down onto map collision (like snap to ground) |
| `create_ymap` | new ymap from a list of entities; optional snap/align to ground |
| `read_ymap`, `add_entities_to_ymap`, `edit_ymap_entities`, `remove_ymap_entities` | inspect and edit placements (in-place edits keep a `.bak`) |
| `create_ytyp_from_folder` | ytyp for a folder of custom .ydr/.yft (same logic as *Auto Setup Ytyp from Folder*) |
| `export_xml`, `import_xml` | CodeWalker XML conversion for ymap, ytyp, ydr, yft, ytd, ybn, ynd, ynv, meta, audio and more |
| `extract_file` | copy a game file out as a loose file |
| `pack_rpf` | build an OPEN RPF from a folder (folders named `*.rpf` become nested archives) |
| `create_project` | write a `.cwproj` listing your files so you can open them in CodeWalker |
| `hash` | joaat hash and reverse lookup |

Rotations are `[x, y, z]` degrees (pitch, roll, heading). Heading is counter-clockwise seen
from above, Z is up, and units are metres. The server never writes into the GTA folder unless
an output path you pass points there.
