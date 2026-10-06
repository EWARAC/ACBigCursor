AC Big Cursor v0.6.0 release candidate
======================================

AC Big Cursor is a tiny Decal plugin for Asheron's Call.

Why it exists
-------------
Asheron's Call's original mouse pointer can be surprisingly easy to lose on a
modern or high-resolution display, especially in combat or across a busy UI.
AC Big Cursor does not automate the game and does not replace AC's own input.
It simply draws a larger, click-through pointer over the normal cursor.

The current renderer uses a small native transparent Windows overlay owned by
the Asheron's Call window. This keeps the cursor visible across the whole AC
client, avoids Virindi View Service's Ctrl HUD-management frame, and does not
use direct D3D rendering.

Install
-------
1. Close Asheron's Call.
2. Run "AC Big Cursor Setup v0.6.0.exe".
3. Accept the Windows UAC prompt.
4. Start Asheron's Call normally.

The installer puts the plugin in:
C:\Games\Decal Plugins\AC Big Cursor

It registers AC Big Cursor with Decal and creates a normal Windows/Decal
uninstaller. In Decal Agent, selecting AC Big Cursor and clicking Remove should
remove it normally without a command window.

Commands
--------
/bigcursor               Toggle the cursor on/off
/bigcursor on            Turn it on
/bigcursor off           Turn it off
/bigcursor status        Show current status/settings
/bigcursor offset X Y    Adjust hotspot offset and save it
/bigcursor scale N       Set size from 0.5 to 3.0 and save it
/bigcursor reset         Reset to offset 0,0 and scale 1.0

Settings
--------
Saved in:
%APPDATA%\ACBigCursor\settings.ini

Uninstalling leaves your saved settings in place so a later reinstall keeps
your preferred scale and calibration.

Requirements
------------
- Windows
- Asheron's Call with Decal 3.0
- .NET Framework 4.8

Release candidate notes
-----------------------
Tested behavior before packaging:
- cursor visible over the 3D world and AC UI/chat/inventory
- Ctrl does not reveal a VVS frame or hide the cursor
- overlay is click-through and does not take keyboard focus
- saved scale/offset survive restart
- logout is clean with no Direct3D teardown error
- native-overlay default hotspot is 0,0
