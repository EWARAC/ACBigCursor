# AC Big Cursor command reference

All commands are entered in Asheron's Call chat.

## Toggle

`/bigcursor`

Toggles AC Big Cursor between on and off.

`/bigcursor on`

Turns the large cursor on.

`/bigcursor off`

Turns the large cursor off.

## Status

`/bigcursor status`

Shows whether the cursor is enabled, whether the overlay is ready, the renderer, click-through state, current hotspot offset and current scale.

## Hotspot calibration

`/bigcursor offset X Y`

Changes the pointer hotspot offset and saves it.

Example:

`/bigcursor offset 2 -1`

The tested default is:

`/bigcursor offset 0 0`

In testing, changing Asheron's Call's resolution and changing Windows display scaling did not move the cursor point, so most systems should not need an offset adjustment.

## Cursor size

`/bigcursor scale N`

Sets the cursor scale and saves it. Valid values are **0.5 through 3.0**.

Examples:

`/bigcursor scale 1.5`

`/bigcursor scale 2`

## Reset

`/bigcursor reset`

Restores:

- offset `0,0`
- scale `1.0`

The reset values are saved immediately.

## Saved settings

Settings are written to:

`%APPDATA%\ACBigCursor\settings.ini`

They remain in place if the plugin is uninstalled.
