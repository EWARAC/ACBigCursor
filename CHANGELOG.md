# Changelog

## v0.6.0 — 2026-10-06

First public release.

### Added

- Large click-through cursor overlay for Asheron's Call.
- Native transparent Windows overlay renderer.
- Adjustable cursor scale from 0.5 to 3.0.
- Adjustable X/Y hotspot offset.
- Persistent settings in `%APPDATA%\ACBigCursor\settings.ini`.
- In-game status, toggle, scale, offset and reset commands.
- Self-contained Windows installer.
- Normal Windows/Decal uninstaller.

### Verified

- Tested successfully on ACE servers.
- Tested successfully on GDLE servers.
- Cursor remains visible across the 3D view and AC UI/chat/inventory.
- Ctrl does not reveal a VVS management frame.
- Overlay remains click-through and does not take keyboard focus.
- Scale and offset persist between sessions.
- Logout is clean without the Direct3D teardown problem encountered during earlier renderer experiments.
- Default hotspot is correctly aligned at offset `0,0`.
- Tested in-game resolution changes do not move the cursor point.
- Tested Windows display-scaling changes do not move the cursor point.

### Development notes

Earlier VVS HudView and direct Direct3D rendering approaches were abandoned in favour of the native overlay used by v0.6.0.
