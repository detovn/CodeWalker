# What's different in this fork

This fork is upstream CodeWalker (dexyfex/CodeWalker, as of commit `485d56b`) plus 35 of the 40 pull requests that were
open against it, plus a few things written specifically for this fork. Everything lives on the `merge-upstream-prs` branch.

The PR numbers below are upstream's (github.com/dexyfex/CodeWalker/pull/N), so you can read the original discussion.

---

## 1. New tools you'll notice straight away

### Paint Props / Grass brush (written for this fork)
Open it from **Ymap → Paint Props / Grass...** (right under *New Grass Batch*), **Tools → Paint Props / Grass...** (right above
*Delete Grass*) in the Project window, or **Tools → Paint props / grass...** in the main map window.

* **Props mode:** build a list of prop names (type them, or click an entity in the world and press *Selected*). Hold **Ctrl** and
  drag across the ground and random props from your list are dropped onto the surface, each as a normal entity in the ymap you
  have selected. Controls: brush radius, props per dab, minimum spacing, random rotation, tilt to the ground, steepest slope
  allowed, scale range, height offset.
* **Grass mode:** pick a grass type (default `proc_grasses01`), tint, AO and scale range, and paint. The tool puts blades into a
  nearby grass batch of the same type in your ymap and creates new batches automatically as you move, so you no longer have to
  create a batch, set its archetype and switch its brush on first.
* **Erase:** hold **Ctrl + Shift** and drag. The brush circle turns red. It erases only the props in your list (props mode) or
  grass blades (grass mode). Grass batches that end up empty are removed for you.
* **Keys:** `B` turns painting on/off (and opens the panel), `G` swaps between props and grass, `[` and `]` shrink and grow the
  brush, and **Ctrl + mouse wheel** resizes it while you paint.
* It paints into the ymap that's selected in the Project window, and that ymap has to already be in your project.
* There is no undo for painting yet — erase with Ctrl + Shift instead.

### Auto Setup Ytyp from Folder (written for this fork)
Open it from **Tools → Auto Setup Ytyp from Folder...** or **Ytyp → Auto Setup from Folder...** in the Project window.
Pick a folder of custom models and it builds the whole `.ytyp` for you in one go, instead of adding archetypes one by one:

* Finds every `.ydr` and `.yft` in the folder and its sub-folders. Each one becomes an archetype named after the file.
* Fills in the box and sphere sizes from the model itself, and sets the asset type (drawable or fragment).
* Links the texture dictionary if there's a matching `.ytd` beside the model (or textures inside the model), and the collision
  if there's a matching `.ybn` (or collision inside the model).
* Picks a draw distance from the model's size: small props stop drawing sooner, big ones stay longer (60 to 300).
* If you have a `.ytyp` open in your project it adds to that one and skips names already in it; otherwise it makes a new one.
  It asks before doing anything, then tells you how many were added, skipped, or couldn't be read.
* Save the `.ytyp` afterwards. The model files still need to be streamed with it under the same names.

### Select Connected (written for this fork)
Select a model in the main map window and press **L** (or **Tools → Select connected**). It selects everything linked to it,
so you can see, move or delete the whole group together:

* its full LOD chain, from the HD model up through the LOD and SLOD parents and every sibling underneath,
* if it's an interior (MLO), every entity inside it,
* box occluders in view that sit on top of that group (within 10 m).

It only changes what's selected; it never edits or splits any ymap, so game files stay untouched. Occlusion *model*
triangles and grass/lights aren't included.

### Smart placement and alignment (written for this fork)
Select two or more entities (Ctrl+click; the last one you click is the **anchor**) and open **Tools → Smart placement &
alignment...**. Every button only changes a **preview**: translucent ghost boxes show where each entity would end up, and
nothing moves until you press **Apply**. Apply is one undo step (Ctrl+Z), and **Reset preview** throws the preview away.
Commands can be chained (e.g. Align bottom, then Equal spacing, then Apply).

* **Align** left/center/right, front/center/back, bottom/center/top, to the whole group or to the anchor.
* **Equal spacing** along X, Y or Z: equal gaps between the models (or equal centres). Needs three or more.
* **Match rotation** (full, or heading only keeping each model's tilt) and **Face anchor** (turns chairs towards a table;
  Flip is for models that face the other way).
* **Fit to surface** drops each entity onto the collision or model underneath it, ignoring the entities being moved. It can
  also tilt to follow a slope (up to 45 degrees).
* **Snap to wall** pushes each entity sideways until its bounds touch the nearest surface within the search distance, and
  can turn it to face away from the wall (doors, pictures, shelves, wall lights).
* **Move without changing elevation** makes dragging the move widget only slide things sideways.

Honest limits: it recognises surfaces through collision and model bounds, so it needs collision loaded near the entities.
Doors are lined up against the wall they're next to, but it does not detect the actual opening in a wall, and chairs are
turned towards the anchor but not pushed under the table to a set gap. Works on entities (not occluders, lights, etc.).

### Precise model selection (written for this fork)
Right-click selection used to pick whatever model's *invisible box* was in front, which is why clicking around a tree or a
fence was a guessing game. It now tests the model's real triangles, so you select the thing your cursor is actually on. If
a model's triangles can't be tested (skinned or bone-positioned models), it quietly falls back to the old box picking.
Turn it off with the **Precise (polygons)** checkbox on the Selection tab.

### Better grass selection (written for this fork)
In grass selection mode, clicking now picks the batch that owns the blade under the cursor instead of the biggest box.

### Command-line tool (upstream #386)
A new `CodeWalker.Cli` program that works without the window: list, extract, search, pack, diff and validate RPF archives;
export to XML, DDS, WAV and text; convert Gen9 files; compute hashes. It comes with its own test suite (533 tests, all
passing). It builds for .NET Framework 4.8 and .NET 8 — I removed the .NET 10 target so the solution builds with the .NET 9
SDK installed on this PC.

---

## 2. Map editor and project workflow

| PR | What it does for you |
|---|---|
| #388 | Select several entities in the project and edit their archetype, scale, flags, LOD fields and more all at once. |
| #318 | New toolbar option so each item in a multi-selection snaps to the ground on its own, plus configurable snap-to-ground distances. |
| #330 | MLO portals now show a direction arrow, can be selected in the world, and have a **Flip Portal** button. |
| #332, #333 | Fixes MLO room bounding boxes for rotated entities, and rotating an MLO child now saves the new rotation properly. |
| #385 | No more crash when deleting an MLO entity whose archetype has no portals. |
| #341 | The LOD-parent field in the entity panel shows the parent's name (with autocomplete), not just its number. |
| #347 | Fixes two Ambient Zone text boxes that were wired to the wrong values. |
| #366 | LOD lights generator rewritten: handles negative coordinates, proper small/medium/large lights, faster. Output goes to a `lodlights` folder next to your project and is added to the project. |
| #367 | The Project window no longer freezes when you have a lot loaded — it updates just the part of the tree that changed. |
| #368 | In LOD-light selection mode every light gets its own small box so you can see and click them. |
| #372 | Custom vehicle-node (YND) edits are faster, and closing a project properly restores the original nodes. |
| #381 | Deleting scenario nodes or chains keeps the scenario chaining graph consistent instead of leaving dangling links. |
| #360 | Selecting a scenario point now shows the ped model doing its scenario animation there. |
| #364 | Selecting a road node shows a car on it, facing along the road. |
| #361 | Changing the GTA folder or GTA version now offers to restart CodeWalker for you (and won't restart if nothing changed). |
| #345 | **Windows → Close All** in the explorer, and tool windows open centred on the screen. |
| #371 | Model stats show triangle and vertex counts. |
| #377 | Mouse picking is faster, and ignores drawable collision when that layer is turned off. |

## 3. Rendering

* **#359 Parallax (height-map) shaders** — terrain and standard surfaces can now show depth from a height map, with
  self-shadowing, in both forward and deferred rendering. The compiled shaders in `Shaders/` were rebuilt to match.
* **#365 / #326 Textures** — fixes textures that failed to load or save because of unusual sizes (heights not divisible
  by 4, tiny mip levels), fixes a colour-channel bug when decoding BC5/ATI2 textures, and corrects the stored stride so
  compressed textures keep their correct size when a YTD is saved again.

## 4. File format and data support

* **#363** — `audioworldsectorsinfo.dat` can now be opened and exported/imported as XML.
* **#362** — Waypoint (YWR) files show real names for their flags instead of "Unk0–Unk3", and old XML still imports.
* **#369** — Audio (REL) files show individual flags in XML for about 40 types, plus several audio fields fixed.
* **#355** — AWC audio: import and export MP3 and ADPCM WAV files, plus an *Export Select* button.
* **#378** — AWC *shader library* files open in the FXC viewer, where you can export, replace or export all shaders.
* **#375** — Collision (bounds) loaded from XML is hardened against crashes from bad data.
* **#232** — Fragment (YFT) properties use the same names as Sollumz. The XML tags keep their old names so existing files still import.
* **#282** — Support for `save_default0000.pso`, the game's default stats file.
* **#331, #342** — Adds missing AWC marker names and a few hundred missing meta names.
* **#351, #352** — The explorer sorts file names naturally (file2 before file10), and exporting twice no longer overwrites; you get `name(1).ext`.

---

## 5. Not included, and why

| PR | Reason |
|---|---|
| #47 | From 2019 and far out of date; it would replace every shader with a much larger debug build. |
| #292 | "Hide GTA map" fix that makes every saved ymap look unsaved and keeps hidden maps loading. Needs a rewrite. |
| #319 | Unfinished draft — touches the user interface from the wrong thread. |
| #370 | Replaced by #377, which does the same thing. |
| #327 | Draft that conflicts with #326; its stride fix was folded into #326. |

## 6. Bugs found and fixed after merging

Reviewing the PRs turned up real problems, so these were corrected before they landed:
* #361 could lose unsaved project work when restarting; now it asks and respects a cancelled close.
* #345's "Close All" also closed the main menu window and quit the app.
* #369 and #232 would silently zero out audio slots / vehicle physics values when files went through XML.
* #355 had several audio export bugs (garbage peaks, bad WAV header, cut-off audio tail) and an MP3 import crash.
* #360 listed every scenario region twice and removed the click-to-select on scenario points.
* #352's duplicate naming broke re-importing exported `.xml` files.
* #318 snapped items in place instead of following the drag.
* #330 left a portal selected after deselecting it.
* #341 crashed when a ymap had no parent loaded.
* #366 could leave its Generate button disabled forever after an error.
* #282 didn't compile; it's moved to the right place.
* Paint Props crashed the 3D view when it tested the ground while a frame was drawing; fixed.

## 7. Building

The projects build with the .NET SDK; Visual Studio isn't required. Notes for this PC: the CLI no longer targets .NET 10. Built
copies of the apps are in the (git-ignored) `Build` folder.
