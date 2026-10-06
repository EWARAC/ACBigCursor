# AC Big Cursor v0.6.0

AC Big Cursor v0.6.0 is the first public release of a small accessibility-focused Decal plugin for Asheron's Call.

It draws a larger, click-through pointer over the game's normal cursor without changing normal mouse behaviour or automating gameplay.

## Highlights

- Adjustable cursor size.
- Adjustable hotspot calibration.
- Saved settings between sessions.
- Cursor remains visible across the full AC interface.
- Clean native transparent overlay renderer.
- Normal installer and Decal/Windows uninstall support.
- Tested successfully on both ACE and GDLE servers.
- Tested in-game resolution changes and Windows display-scaling changes did not disturb cursor alignment.

## Install

Close Asheron's Call and run:

`AC Big Cursor Setup v0.6.0.exe`

The plugin installs to:

`C:\Games\Decal Plugins\AC Big Cursor`

## Default calibration

The tested default is offset `0,0`, scale `1.0`.

If calibration is ever required on a particular system:

`/bigcursor offset X Y`

The v0.6.0 installer is the same tested release-candidate build promoted to release after the final ACE/GDLE and display tests. No plugin code was changed after those tests.
