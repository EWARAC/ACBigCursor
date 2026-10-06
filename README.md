# AC Big Cursor

**AC Big Cursor** is a small Decal plugin for **Asheron's Call** that makes the mouse cursor larger and easier to see.

## Why I made it

I have poor eyesight, and the original Asheron's Call mouse cursor is simply too small and too easy for me to lose on screen.

On a modern monitor, especially during combat or when moving quickly around the interface, I found myself spending far too much time trying to find the pointer.

I just wanted the existing cursor to be bigger and easier to see without changing normal mouse behaviour.

AC Big Cursor does exactly that. It adds a larger, click-through cursor over Asheron's Call. The size can be adjusted in game, the hotspot can be calibrated if required, and the settings are saved between sessions.

It started as a simple accessibility fix for myself, but I figured other players with similar eyesight problems might find it useful too.

## What it does

- Draws a larger cursor over the normal Asheron's Call pointer.
- Uses a native transparent Windows overlay owned by the AC client.
- Remains visible over the 3D world, chat, inventory and the rest of the AC interface.
- Is click-through and does not take keyboard focus.
- Supports adjustable size and hotspot offset.
- Saves scale and offset between sessions.
- Does **not** automate gameplay or replace normal mouse input.

## Compatibility

Version **0.6.0** has been tested successfully on both **ACE** and **GDLE** servers.

Changing the in-game resolution did not disturb cursor alignment in testing. Changing Windows display scaling also did not disturb alignment. If a particular system does require calibration, the plugin includes adjustable X/Y hotspot offsets.

## Requirements

- Windows
- Asheron's Call
- Decal 3.0
- .NET Framework 4.8

## Installation

1. Close Asheron's Call.
2. Run `AC Big Cursor Setup v0.6.0.exe`.
3. Accept the Windows UAC prompt.
4. Start Asheron's Call normally.

The installer places the plugin in:

`C:\Games\Decal Plugins\AC Big Cursor`

It registers AC Big Cursor with Decal and installs a normal Windows/Decal uninstaller. In Decal Agent, selecting **AC Big Cursor** and clicking **Remove** should uninstall it normally.

## Commands

| Command | Action |
|---|---|
| `/bigcursor` | Toggle the large cursor on/off |
| `/bigcursor on` | Turn it on |
| `/bigcursor off` | Turn it off |
| `/bigcursor status` | Show current status and settings |
| `/bigcursor offset X Y` | Adjust hotspot offset and save it |
| `/bigcursor scale N` | Set size from 0.5 to 3.0 and save it |
| `/bigcursor reset` | Reset to offset `0,0` and scale `1.0` |

See [COMMANDS.md](COMMANDS.md) for more detail.

## Settings

Settings are stored in:

`%APPDATA%\ACBigCursor\settings.ini`

Uninstalling the plugin leaves these saved settings in place so a later reinstall keeps the preferred size and calibration.

## Building from source

The included `BUILD RELEASE.cmd` uses the 32-bit .NET Framework compiler and the installed Decal 3.0 `Decal.Adapter.dll`.

The project targets:

- .NET Framework 4.8
- x86
- C# 7.3

Build output is intentionally excluded from source control. The tested installer is provided separately in the repository's `release` folder.

## v0.6.0 testing

The v0.6.0 build was verified for:

- ACE server compatibility
- GDLE server compatibility
- cursor visibility over the whole AC client, including UI/chat/inventory
- Ctrl not exposing a VVS HUD-management frame
- click-through behaviour without stealing focus
- saved scale/offset surviving restart
- clean logout without Direct3D teardown errors
- default hotspot alignment at offset `0,0`
- stable alignment across tested in-game resolution changes
- stable alignment across tested Windows display-scaling changes

The v0.6.0 installer is the same tested release-candidate build promoted to release after these final compatibility tests; no plugin code was changed after testing.

## Project status

Current release: **v0.6.0**

Author/publisher: **EWARAC**
