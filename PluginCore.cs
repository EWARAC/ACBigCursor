using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Decal.Adapter;

namespace ACBigCursor
{
    [FriendlyName("AC Big Cursor")]
    [Guid("D6211596-2E93-4755-B47F-D270561545B0")]
    public sealed class PluginCore : PluginBase
    {
        private const int OverlaySize = 192;
        private const int HotspotX = 3;
        private const int HotspotY = 3;

        private CursorOverlayForm _overlay;
        private bool _enabled = true;
        private bool _ready;
        private IntPtr _ownerHwnd = IntPtr.Zero;
        private Point _lastScreenLocation = new Point(int.MinValue, int.MinValue);
        private int _offsetX = 0;
        private int _offsetY = 0;
        private float _scale = 1.0f;

        private static readonly int DefaultOffsetX = 0;
        private static readonly int DefaultOffsetY = 0;
        private static readonly float DefaultScale = 1.0f;

        protected override void Startup()
        {
            try
            {
                LoadSettings();
                CoreManager.Current.RenderFrame += Current_RenderFrame;
                CoreManager.Current.CharacterFilter.LoginComplete += CharacterFilter_LoginComplete;
                CoreManager.Current.CommandLineText += Current_CommandLineText;
            }
            catch (Exception ex)
            {
                Fail("Startup", ex);
            }
        }

        protected override void Shutdown()
        {
            try
            {
                if (CoreManager.Current != null)
                {
                    CoreManager.Current.RenderFrame -= Current_RenderFrame;
                    CoreManager.Current.CommandLineText -= Current_CommandLineText;
                    if (CoreManager.Current.CharacterFilter != null)
                        CoreManager.Current.CharacterFilter.LoginComplete -= CharacterFilter_LoginComplete;
                }
            }
            catch { }

            DisposeOverlay();
        }

        private void CharacterFilter_LoginComplete(object sender, EventArgs e)
        {
            try
            {
                Chat("v0.6.0 loaded. Native overlay. Saved offset=" + _offsetX + "," + _offsetY +
                     ", scale=" + _scale.ToString("0.##", CultureInfo.InvariantCulture) + ".");
            }
            catch (Exception ex) { Fail("LoginComplete", ex); }
        }

        private void EnsureOverlay(IntPtr acHwnd)
        {
            if (_overlay == null || _overlay.IsDisposed)
            {
                _overlay = new CursorOverlayForm(OverlaySize, _scale);
                _overlayHwndCreateAndOwn(acHwnd);
                _ready = true;
                _lastScreenLocation = new Point(int.MinValue, int.MinValue);
                return;
            }

            if (_ownerHwnd != acHwnd && acHwnd != IntPtr.Zero)
                SetOverlayOwner(acHwnd);
        }

        private void _overlayHwndCreateAndOwn(IntPtr acHwnd)
        {
            IntPtr overlayHwnd = _overlay.Handle;
            if (acHwnd != IntPtr.Zero)
                SetOverlayOwner(acHwnd);
        }

        private void SetOverlayOwner(IntPtr acHwnd)
        {
            try
            {
                if (_overlay == null || _overlay.IsDisposed || acHwnd == IntPtr.Zero) return;
                SetWindowLong(_overlay.Handle, GWL_HWNDPARENT, acHwnd.ToInt32());
                _ownerHwnd = acHwnd;
            }
            catch { }
        }

        private void DisposeOverlay()
        {
            _ready = false;
            _ownerHwnd = IntPtr.Zero;
            _lastScreenLocation = new Point(int.MinValue, int.MinValue);

            try
            {
                if (_overlay != null)
                {
                    try { _overlay.Hide(); } catch { }
                    _overlay.Dispose();
                }
            }
            catch { }

            _overlay = null;
        }

        private void Current_RenderFrame(object sender, EventArgs e)
        {
            try
            {
                if (!_enabled)
                {
                    HideOverlay();
                    return;
                }

                IntPtr hwnd = GetAcWindowHandle();
                if (hwnd == IntPtr.Zero || GetForegroundWindow() != hwnd)
                {
                    HideOverlay();
                    return;
                }

                NativePoint cursorScreen;
                if (!GetCursorPos(out cursorScreen))
                {
                    HideOverlay();
                    return;
                }

                NativePoint cursorClient = cursorScreen;
                if (!ScreenToClient(hwnd, ref cursorClient))
                {
                    HideOverlay();
                    return;
                }

                NativeRect client;
                if (!GetClientRect(hwnd, out client) ||
                    cursorClient.X < 0 || cursorClient.Y < 0 ||
                    cursorClient.X >= client.Right || cursorClient.Y >= client.Bottom)
                {
                    HideOverlay();
                    return;
                }

                EnsureOverlay(hwnd);
                if (_overlay == null || _overlay.IsDisposed) return;

                var loc = new Point(
                    cursorScreen.X - HotspotX + _offsetX,
                    cursorScreen.Y - HotspotY + _offsetY);

                if (loc != _lastScreenLocation)
                {
                    _overlay.Location = loc;
                    _lastScreenLocation = loc;
                }

                if (!_overlay.Visible)
                    _overlay.Show();

                SetWindowPos(_overlay.Handle, HWND_TOP, loc.X, loc.Y, 0, 0,
                    SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
            }
            catch
            {
                HideOverlay();
            }
        }

        private void HideOverlay()
        {
            try
            {
                if (_overlay != null && !_overlay.IsDisposed && _overlay.Visible)
                    _overlay.Hide();
            }
            catch { }
        }

        private void Current_CommandLineText(object sender, ChatParserInterceptEventArgs e)
        {
            try
            {
                if (e == null || string.IsNullOrEmpty(e.Text)) return;
                string cmd = e.Text.Trim().ToLowerInvariant();
                if (!cmd.StartsWith("/bigcursor")) return;

                e.Eat = true;

                if (cmd == "/bigcursor on") _enabled = true;
                else if (cmd == "/bigcursor off") _enabled = false;
                else if (cmd.StartsWith("/bigcursor offset "))
                {
                    string[] parts = e.Text.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    int x, y;
                    if (parts.Length == 4 && int.TryParse(parts[2], out x) && int.TryParse(parts[3], out y))
                    {
                        _offsetX = x;
                        _offsetY = y;
                        _lastScreenLocation = new Point(int.MinValue, int.MinValue);
                        SaveSettings();
                        Chat("offset=" + _offsetX + "," + _offsetY + " (saved)");
                    }
                    else
                    {
                        Chat("usage: /bigcursor offset X Y");
                    }
                    return;
                }
                else if (cmd.StartsWith("/bigcursor scale "))
                {
                    string[] parts = e.Text.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    float scale = 0.0f;
                    bool parsed = parts.Length == 3 &&
                                  (float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out scale) ||
                                   float.TryParse(parts[2], NumberStyles.Float, CultureInfo.CurrentCulture, out scale));

                    if (parsed && scale >= 0.5f && scale <= 3.0f)
                    {
                        _scale = scale;
                        try { if (_overlay != null && !_overlay.IsDisposed) _overlay.CursorScale = _scale; } catch { }
                        SaveSettings();
                        Chat("scale=" + _scale.ToString("0.##", CultureInfo.InvariantCulture) + " (saved)");
                    }
                    else
                    {
                        Chat("usage: /bigcursor scale N   (0.5 to 3.0)");
                    }
                    return;
                }
                else if (cmd == "/bigcursor reset")
                {
                    _offsetX = DefaultOffsetX;
                    _offsetY = DefaultOffsetY;
                    _scale = DefaultScale;
                    _lastScreenLocation = new Point(int.MinValue, int.MinValue);
                    try { if (_overlay != null && !_overlay.IsDisposed) _overlay.CursorScale = _scale; } catch { }
                    SaveSettings();
                    Chat("reset to offset=" + _offsetX + "," + _offsetY +
                         ", scale=" + _scale.ToString("0.##", CultureInfo.InvariantCulture) + " (saved)");
                    return;
                }
                else if (cmd == "/bigcursor status")
                {
                    Chat("enabled=" + _enabled + ", ready=" + _ready +
                         ", renderer=NativeOverlay, clickThrough=True" +
                         ", offset=" + _offsetX + "," + _offsetY +
                         ", scale=" + _scale.ToString("0.##", CultureInfo.InvariantCulture));
                    return;
                }
                else _enabled = !_enabled;

                if (!_enabled) HideOverlay();
                Chat("cursor " + (_enabled ? "ON" : "OFF"));
            }
            catch (Exception ex) { Fail("command", ex); }
        }

        private static string SettingsDirectory
        {
            get
            {
                return System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "ACBigCursor");
            }
        }

        private static string SettingsPath
        {
            get { return System.IO.Path.Combine(SettingsDirectory, "settings.ini"); }
        }

        private void LoadSettings()
        {
            _offsetX = DefaultOffsetX;
            _offsetY = DefaultOffsetY;
            _scale = DefaultScale;

            try
            {
                if (!File.Exists(SettingsPath)) return;

                foreach (string rawLine in File.ReadAllLines(SettingsPath))
                {
                    string line = rawLine == null ? string.Empty : rawLine.Trim();
                    if (line.Length == 0 || line.StartsWith("#") || line.StartsWith(";")) continue;

                    int eq = line.IndexOf('=');
                    if (eq <= 0) continue;

                    string key = line.Substring(0, eq).Trim().ToLowerInvariant();
                    string value = line.Substring(eq + 1).Trim();

                    int intValue;
                    float floatValue;
                    if (key == "offsetx" && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out intValue))
                        _offsetX = intValue;
                    else if (key == "offsety" && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out intValue))
                        _offsetY = intValue;
                    else if (key == "scale" && float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out floatValue) && floatValue >= 0.5f && floatValue <= 3.0f)
                        _scale = floatValue;
                }
            }
            catch
            {
                _offsetX = DefaultOffsetX;
                _offsetY = DefaultOffsetY;
                _scale = DefaultScale;
            }
        }

        private void SaveSettings()
        {
            try
            {
                Directory.CreateDirectory(SettingsDirectory);
                File.WriteAllLines(SettingsPath, new[]
                {
                    "# AC Big Cursor settings",
                    "offsetX=" + _offsetX.ToString(CultureInfo.InvariantCulture),
                    "offsetY=" + _offsetY.ToString(CultureInfo.InvariantCulture),
                    "scale=" + _scale.ToString("0.###", CultureInfo.InvariantCulture)
                });
            }
            catch (Exception ex)
            {
                Fail("SaveSettings", ex);
            }
        }

        private static IntPtr GetAcWindowHandle()
        {
            try
            {
                object decal = CoreManager.Current.Decal;
                if (decal == null) return IntPtr.Zero;
                PropertyInfo p = decal.GetType().GetProperty("Hwnd");
                if (p == null) return IntPtr.Zero;
                object raw = p.GetValue(decal, null);
                if (raw is IntPtr) return (IntPtr)raw;
                return new IntPtr(Convert.ToInt64(raw));
            }
            catch { return IntPtr.Zero; }
        }

        private static void Chat(string text)
        {
            try { CoreManager.Current.Actions.AddChatText("[AC Big Cursor] " + text, 5); }
            catch { }
        }

        private static void Fail(string where, Exception ex)
        {
            try { CoreManager.Current.Actions.AddChatText("[AC Big Cursor] " + where + " error: " + ex.Message, 5); }
            catch { }
        }

        private sealed class CursorOverlayForm : Form
        {
            private float _cursorScale;

            public CursorOverlayForm(int size, float scale)
            {
                _cursorScale = scale;

                AutoScaleMode = AutoScaleMode.None;
                FormBorderStyle = FormBorderStyle.None;
                ShowInTaskbar = false;
                StartPosition = FormStartPosition.Manual;
                Size = new Size(size, size);
                MinimumSize = Size;
                MaximumSize = Size;
                BackColor = Color.Fuchsia;
                TransparencyKey = Color.Fuchsia;
                TopMost = false;
                Enabled = true;

                SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint |
                         ControlStyles.OptimizedDoubleBuffer, true);
                UpdateStyles();
            }

            public float CursorScale
            {
                get { return _cursorScale; }
                set
                {
                    _cursorScale = value;
                    try { Invalidate(); } catch { }
                }
            }

            protected override bool ShowWithoutActivation
            {
                get { return true; }
            }

            protected override CreateParams CreateParams
            {
                get
                {
                    CreateParams cp = base.CreateParams;
                    cp.ExStyle |= WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE | WS_EX_TRANSPARENT | WS_EX_LAYERED;
                    return cp;
                }
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                e.Graphics.Clear(Color.Fuchsia);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                e.Graphics.SmoothingMode = SmoothingMode.None;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.None;

                PointF[] baseArrow =
                {
                    new PointF(3,  3),
                    new PointF(3,  43),
                    new PointF(14, 32),
                    new PointF(23, 55),
                    new PointF(31, 51),
                    new PointF(21, 29),
                    new PointF(39, 29)
                };

                PointF[] arrow = new PointF[baseArrow.Length];
                for (int i = 0; i < baseArrow.Length; i++)
                {
                    arrow[i] = new PointF(
                        HotspotX + ((baseArrow[i].X - HotspotX) * _cursorScale),
                        HotspotY + ((baseArrow[i].Y - HotspotY) * _cursorScale));
                }

                using (var fill = new SolidBrush(Color.White))
                using (var outline = new Pen(Color.Black, Math.Max(1.0f, 3.5f * _cursorScale)))
                using (var inner = new Pen(Color.White, Math.Max(0.75f, 1.0f * _cursorScale)))
                {
                    outline.LineJoin = LineJoin.Round;
                    inner.LineJoin = LineJoin.Round;
                    e.Graphics.FillPolygon(fill, arrow);
                    e.Graphics.DrawPolygon(outline, arrow);
                    e.Graphics.DrawPolygon(inner, arrow);
                }
            }

            protected override void WndProc(ref System.Windows.Forms.Message m)
            {
                if (m.Msg == WM_NCHITTEST)
                {
                    m.Result = new IntPtr(HTTRANSPARENT);
                    return;
                }

                base.WndProc(ref m);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativePoint
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeRect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private const int GWL_HWNDPARENT = -8;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_LAYERED = 0x00080000;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int WM_NCHITTEST = 0x0084;
        private const int HTTRANSPARENT = -1;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private static readonly IntPtr HWND_TOP = IntPtr.Zero;

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out NativePoint lpPoint);

        [DllImport("user32.dll")]
        private static extern bool ScreenToClient(IntPtr hWnd, ref NativePoint lpPoint);

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out NativeRect lpRect);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy, uint uFlags);
    }
}
