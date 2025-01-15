namespace MadDogs.TaskbarGroups.Background.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Timers;
    using System.Windows.Forms;

    using Classes;

    using Common.Extensions;
    using Common.Model;

    using User_Controls;

    using Timer = System.Timers.Timer;

    public partial class FrmMain : Form
    {
        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_MAXIMIZE = 0xf030;

        private readonly string[] _argumentList;

        private readonly Keys[] _keyList =
        {
            Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9, Keys.D0
        };

        private readonly Category _loadedCat;

        public List<UcShortcut> ControlList;
        public Color HoverColor;

        public Point MousePos;

        public int UcShortcutIconLocation;
        public int UcShortcutIconSize;

        public int UcShortcutSize;

        internal FrmMain(Category category, string[] arguments)
        {
            InitializeComponent();

            _loadedCat = category;

            ProfileOptimization.StartProfile("frmMain.Profile");

            MousePos = new Point(Cursor.Position.X, Cursor.Position.Y);

            EDpi = Display(DpiType.Effective);
            XDpi = EDpi / 96m;
            FormBorderStyle = FormBorderStyle.None;
            _argumentList = arguments;

            Icon = category.GroupIco;

            ControlList = new List<UcShortcut>();

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Category.FromString(category.ColorString);
            Opacity = 1 - (category.Opacity / 100);

            TopLevel = true;
            TopMost = true;

            HoverColor = category.HoverColor == null ? category.CalculateHoverColor() : ColorTranslator.FromHtml(category.HoverColor);

            var jumpList = new JumpList(Handle);
            jumpList.BuildJumpList(category.AllowOpenAll, category.Name);

            if (arguments[0] == "setGroupContextMenu")
            {
                Close();
            }
        }

        public sealed override Color BackColor { get; set; }

        public static uint EDpi { get; set; }
        public static decimal XDpi { get; set; }

        [DllImport("User32.dll")]
        internal static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern void SwitchToThisWindow(IntPtr hWnd, bool turnOn);

        public uint Display(DpiType type)
        {
            foreach (Screen screen in Screen.AllScreens)
            {
                if (!screen.Bounds.Contains(MousePos))
                {
                    continue;
                }

                screen.GetDpi(DpiType.Effective, out var x, out _);
                EDpi = x;

                return x;
            }

            return EDpi;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            LoadCategory();

            if (_argumentList[0] == "tskBarOpen_allGroup")
            {
                foreach (UcShortcut usc in ControlList)
                {
                    usc.ucShortcut_Click(usc, EventArgs.Empty);
                }

                Close();
            }

            SetLocation();

            SetForegroundWindow(Handle);
            SwitchToThisWindow(Handle, true);

            var t = new Timer();
            t.Interval = 50;
            t.AutoReset = false;
            t.Elapsed += TimerElapsed;
            t.Start();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            Deactivate += frmMain_Deactivate;
            LostFocus += frmMain_Deactivate;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        private void SetLocation()
        {
            Dictionary<string, Rectangle> taskbarList = FindDockedTaskBars();
            var taskbar = new Rectangle();
            var screen = new Rectangle();

            int locationY;
            int locationX;

            if (taskbarList.Count != 0)
            {
                foreach (Screen scr in Screen.AllScreens)
                {
                    if (!scr.Bounds.Contains(MousePos))
                    {
                        continue;
                    }

                    screen.X = scr.Bounds.X;
                    screen.Y = scr.Bounds.Y;
                    screen.Width = scr.Bounds.Width;
                    screen.Height = scr.Bounds.Height;
                    taskbarList.TryGetValue(scr.DeviceName, out taskbar);
                }

                if (taskbar.Contains(MousePos))
                {
                    if (taskbar.Top == screen.Top && taskbar.Width == screen.Width)
                    {
                        locationY = screen.Y + taskbar.Height + 10;
                        locationX = MousePos.X - (Width / 2);
                    }
                    else if (taskbar.Bottom == screen.Bottom && taskbar.Width == screen.Width)
                    {
                        locationY = screen.Y + screen.Height - Height - taskbar.Height - 10;
                        locationX = MousePos.X - (Width / 2);
                    }
                    else if (taskbar.Left == screen.Left)
                    {
                        locationY = MousePos.Y - (Height / 2);
                        locationX = screen.X + taskbar.Width + 10;
                    }
                    else
                    {
                        locationY = MousePos.Y - (Height / 2);
                        locationX = screen.X + screen.Width - Width - taskbar.Width - 10;
                    }
                }
                else
                {
                    locationY = MousePos.Y - Height - 20;
                    locationX = MousePos.X - (Width / 2);
                }
            }
            else
            {
                foreach (Screen scr in Screen.AllScreens)
                {
                    if (!scr.Bounds.Contains(MousePos))
                    {
                        continue;
                    }

                    screen.X = scr.Bounds.X;
                    screen.Y = scr.Bounds.Y;
                    screen.Width = scr.Bounds.Width;
                    screen.Height = scr.Bounds.Height;
                }

                if (MousePos.Y > Screen.PrimaryScreen.Bounds.Height - 35)
                {
                    locationY = Screen.PrimaryScreen.Bounds.Height - Height - 45;
                }
                else
                {
                    locationY = MousePos.Y - Height - 20;
                }

                locationX = MousePos.X - (Width / 2);
            }

            Location = new Point(locationX, locationY);

            if (Left < screen.Left)
            {
                Left = screen.Left + 10;
            }

            if (Top < screen.Top)
            {
                Top = screen.Top + 10;
            }

            if (Right > screen.Right)
            {
                Left = screen.Right - Width - 10;
            }

            if (taskbar.Contains(Left, Top) && taskbar.Contains(Right, Top))
            {
                Top = screen.Top + 10 + taskbar.Height;
            }

            if (taskbar.Contains(Left, Top))
            {
                Left = screen.Left + 10 + taskbar.Width;
            }

            if (taskbar.Contains(Right, Top))
            {
                Left = screen.Right - Width - 10 - taskbar.Width;
            }
        }

        public static Dictionary<string, Rectangle> FindDockedTaskBars()
        {
            var dockedRects = new Dictionary<string, Rectangle>();

            foreach (Screen screen in Screen.AllScreens)
            {
                if (screen.Bounds.Equals(screen.WorkingArea))
                {
                    continue;
                }

                var rect = new Rectangle();

                var leftDockedWidth = Math.Abs(Math.Abs(screen.Bounds.Left) - Math.Abs(screen.WorkingArea.Left));
                var topDockedHeight = Math.Abs(Math.Abs(screen.Bounds.Top) - Math.Abs(screen.WorkingArea.Top));
                var rightDockedWidth = screen.Bounds.Width - leftDockedWidth - screen.WorkingArea.Width;
                var bottomDockedHeight = screen.Bounds.Height - topDockedHeight - screen.WorkingArea.Height;

                if (leftDockedWidth > 0)
                {
                    rect.X = screen.Bounds.Left;
                    rect.Y = screen.Bounds.Top;
                    rect.Width = leftDockedWidth;
                    rect.Height = screen.Bounds.Height;
                }
                else if (rightDockedWidth > 0)
                {
                    rect.X = screen.WorkingArea.Right;
                    rect.Y = screen.Bounds.Top;
                    rect.Width = rightDockedWidth;
                    rect.Height = screen.Bounds.Height;
                }
                else if (topDockedHeight > 0)
                {
                    rect.X = screen.WorkingArea.Left;
                    rect.Y = screen.Bounds.Top;
                    rect.Width = screen.WorkingArea.Width;
                    rect.Height = topDockedHeight;
                }
                else if (bottomDockedHeight > 0)
                {
                    rect.X = screen.WorkingArea.Left;
                    rect.Y = screen.WorkingArea.Bottom;
                    rect.Width = screen.WorkingArea.Width;
                    rect.Height = bottomDockedHeight;
                }

                dockedRects.Add(screen.DeviceName, rect);
            }

            return dockedRects;
        }

        private void LoadCategory()
        {
            Width = 0;
            Height = (int)((_loadedCat.IconSize + (_loadedCat.Separation * 2)) * XDpi);
            UcShortcutSize = Height;
            UcShortcutIconSize = (int)(_loadedCat.IconSize * XDpi);
            UcShortcutIconLocation = (int)(_loadedCat.Separation * XDpi);
            var x = 0;
            var y = 0;
            var width = _loadedCat.Width;
            var columns = 1;

            for (var i = 0; i < _loadedCat.ShortcutList.Count; i++)
            {
                ProgramShortcut psc = _loadedCat.ShortcutList[i];

                if (columns > width)
                {
                    x = 0;
                    y += (int)((_loadedCat.IconSize + (_loadedCat.Separation * 2)) * XDpi);
                    Height += (int)((_loadedCat.IconSize + (_loadedCat.Separation * 2)) * XDpi);
                    columns = 1;
                }

                if (Width < width * (int)((_loadedCat.IconSize + (_loadedCat.Separation * 2)) * XDpi))
                {
                    Width += (int)((_loadedCat.IconSize + (_loadedCat.Separation * 2)) * XDpi);
                }

                var pscPanel = new UcShortcut
                               {
                                   Psc = psc,
                                   MotherForm = this,
                                   BkgImage = _loadedCat.ProgramImages[i],
                                   LoadedCategory = _loadedCat
                               };

                pscPanel.Location = new Point(x, y);
                Controls.Add(pscPanel);
                ControlList.Add(pscPanel);
                pscPanel.Show();
                pscPanel.BringToFront();
                x += (int)((_loadedCat.IconSize + (_loadedCat.Separation * 2)) * XDpi);
                columns++;
            }
        }

        public void OpenFile(string arguments, string path, string workingDirectory)
        {
            var proc = new ProcessStartInfo { Arguments = arguments, FileName = path, WorkingDirectory = workingDirectory };

            try
            {
                Process.Start(proc);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void frmMain_Deactivate(object sender, EventArgs e)
        {
            if (GetForegroundWindow() != Handle)
            {
                Close();
            }
        }

        private void frmMain_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (_keyList.Contains(e.KeyCode))
                {
                    ControlList[Array.IndexOf(_keyList, e.KeyCode)]
                        .ucShortcut_MouseEnter(sender, e);
                }
            }
            catch
            {
                // ignored
            }
        }

        private void frmMain_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.Enter && _loadedCat.AllowOpenAll)
            {
                foreach (UcShortcut usc in ControlList)
                {
                    usc.ucShortcut_Click(sender, e);
                }
            }

            try
            {
                if (!_keyList.Contains(e.KeyCode))
                {
                    return;
                }

                ControlList[Array.IndexOf(_keyList, e.KeyCode)]
                    .ucShortcut_MouseEnter(sender, e);

                ControlList[Array.IndexOf(_keyList, e.KeyCode)]
                    .ucShortcut_Click(sender, e);
            }
            catch
            {
                // ignored
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_SYSCOMMAND)
            {
                if (m.WParam.ToInt32() == SC_MAXIMIZE)
                {
                    m.Result = IntPtr.Zero;

                    return;
                }
            }

            base.WndProc(ref m);
        }
    }
}