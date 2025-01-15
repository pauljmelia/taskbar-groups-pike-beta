namespace MadDogs.TaskbarGroups.Editor.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Runtime;
    using System.Threading.Tasks;
    using System.Transactions;
    using System.Windows.Forms;

    using Windows.Data.Json;

    using ChinhDo.Transactions;

    using Classes;

    using IWshRuntimeLibrary;

    using Microsoft.VisualBasic.FileIO;

    using User_controls;

    using Settings = Classes.Settings;

    public partial class FrmClient : Form
    {
        private static readonly HttpClient _client = new HttpClient();
        private readonly List<Category> _categoryList = new List<Category>();
        public bool EditOpened = false;

        public FrmClient(List<string> arguments)
        {
            ProfileOptimization.StartProfile("frmClient.Profile");
            InitializeComponent();
            EDpi = Display(DpiType.Effective);
            MaximumSize = new Size(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);

            MinimumSize =
                new Size(Size.Width + 1, Size.Height); // +1 seems to fix the bottomscroll bar randomly appearing.

            Reload();

            currentVersion.Text = "v"
                                  + Assembly.GetEntryAssembly()
                                            .GetName()
                                            .Version;

            githubVersion.Text = Task.Run(() => GetVersionData())
                                     .Result;

            if (arguments.Count > 2
                && arguments[1] == "editingGroupMode"
                && Directory.Exists(Path.Combine(Paths.ConfigPath, arguments[2])))
            {
                try
                {
                    var editGroup = new FrmGroup(this,
                                                 _categoryList.Where(cat => cat.Name == arguments[2])
                                                             .First());

                    editGroup.TopMost = true;
                    editGroup.Show();
                }
                catch { }
            }

            if (Settings.SettingInfo.PortableMode)
            {
                portabilityButton.Tag = "y";
                portabilityButton.Image = Resources.toggleOn;

                var files = Directory.GetFiles(Paths.ShortcutsPath, "*.lnk");

                foreach (var lnkPath in files)
                {
                    var lk = WindowsLnkFile.FromFile(lnkPath);

                    if (lk.RelPath == null
                        || string.IsNullOrEmpty(lk.RelPath.Str)
                        || lk.RelPath.Str != "..\\Taskbar Groups Background.exe")
                    {
                        ChangeAllShortcuts();

                        break;
                    }
                }
            }
            else
            {
                portabilityButton.Tag = "n";
                portabilityButton.Image = Resources.toggleOff;
            }

            if (Paths.JustWritten)
            {
                ChangeAllShortcuts();
            }

            //pnlExistingGroups.AutoScroll = true;
            //pnlExistingGroups.AutoSize = true;
            //flowLayoutPanel1.AutoScroll = true;
            pnlExistingShortcuts.AutoSize = true;
            Reset();
        }

        public static uint EDpi { get; set; } // Effective DPI

        public uint Display(DpiType type)
        {
            foreach (Screen screen in Screen.AllScreens)
            {
                screen.GetDpi(DpiType.Effective, out var x, out _);
                EDpi = x;

                return x;
            }

            return EDpi;
        }

        public void Reload()
        {
            // flush and reload existing groups
            pnlExistingShortcuts.Controls.Clear();

            var subDirectories = new List<string>();

            using (IEnumerator<string> enumeratorDrectories = Directory.EnumerateDirectories(Paths.ConfigPath)
                                                                       .GetEnumerator())
            {
                while (true)
                {
                    try
                    {
                        if (!enumeratorDrectories.MoveNext())
                        {
                            break;
                        }

                        subDirectories.Add(enumeratorDrectories.Current);

                        // processing
                    }
                    catch (Exception e) { }
                }
            }

            //string[] subDirectories = Directory.GetDirectories(Paths.ConfigPath);
            pnlExistingShortcuts.SuspendLayout();

            foreach (var dir in subDirectories)
            {
                try
                {
                    LoadCategory(Path.Combine(Paths.ConfigPath, dir));
                }
                catch (IOException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            pnlExistingShortcuts.ResumeLayout(true);

            /*
            if (pnlExistingGroups.HasChildren) // helper if no group is created
            {
                lblHelpTitle.Text = "Click on a group to add a taskbar shortcut";
                pnlHelp.Visible = true;
            }
            else // helper if groups are created
            {
                lblHelpTitle.Text = "Press on \"Add Taskbar group\" to get started";
                pnlHelp.Visible = false;
            }
            */
            //pnlBottomMain.Location = new Point(pnlBottomMain.Location.X, tableLayoutPanel1.Bottom + (int)(20 * eDpi / 96)); // spacing between existing groups and add new group btn

            Reset();
        }

        public void LoadCategory(string dir)
        {
            var category = new Category(dir);
            _categoryList.Add(category);

            var newCategory = new UcCategoryPanel(this, category);
            pnlExistingShortcuts.RowCount += 1;
            pnlExistingShortcuts.Controls.Add(newCategory, 0, pnlExistingShortcuts.RowCount - 1);
            newCategory.Anchor = AnchorStyles.Left | AnchorStyles.Right;

            //newCategory.Top = pnlExistingGroups.Height - newCategory.Height;
            //newCategory.Dock = DockStyle.Top;
            newCategory.Show();
            newCategory.BringToFront();
            newCategory.MouseEnter += (sender, e) => EnterControl(sender, e, newCategory);
            newCategory.MouseLeave += (sender, e) => LeaveControl(sender, e, newCategory);
        }

        public void Reset()
        {
            if (EDpi == 0)
            {
                EDpi = Display(DpiType.Effective);
            }

            pnlAddGroup.Top = pnlExistingShortcuts.Bottom + (int)(20 * EDpi / 96);
            pnlAddGroup.Left = (ClientSize.Width + pnlLeftColumn.Width - pnlAddGroup.Width) / 2;
        }

        private void cmdAddGroup_Click(object sender, EventArgs e)
        {
            var newGroup = new FrmGroup(this);
            newGroup.Show();
            newGroup.BringToFront();
        }

        private void pnlAddGroup_MouseLeave(object sender, EventArgs e) =>
            pnlAddGroup.BackColor = Color.FromArgb(3, 3, 3);

        private void pnlAddGroup_MouseEnter(object sender, EventArgs e) =>
            pnlAddGroup.BackColor = Color.FromArgb(31, 31, 31);

        public void EnterControl(object sender, EventArgs e, Control control) =>
            control.BackColor = Color.FromArgb(31, 31, 31);

        public void LeaveControl(object sender, EventArgs e, Control control) =>
            control.BackColor = Color.FromArgb(3, 3, 3);

        private static async Task<string> GetVersionData()
        {
            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "taskbar-groups");

                HttpResponseMessage res =
                    await client.GetAsync("https://api.github.com/repos/PikeNote/taskbar-groups-pike-beta/releases");

                res.EnsureSuccessStatusCode();
                var responseBody = await res.Content.ReadAsStringAsync();

                var responseJson = JsonArray.Parse(responseBody);

                JsonObject jsonObjectData = responseJson[0]
                    .GetObject();

                return jsonObjectData["tag_name"]
                    .GetString();
            }
            catch { return "Not found"; }
        }

        private void githubLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) =>
            Process.Start("https://github.com/PikeNote/taskbar-groups-pike-beta");

        private void button1_Click(object sender, EventArgs e)
        {
            if ((string)portabilityButton.Tag != "n")
            {
                DialogResult res =
                    MessageBox.Show("NOTE: Pressing OK will move the following folders back to the AppData folder (config, Shortcuts) and will move Taskbar Groups Background exe back to AppData.\r\n\r\nWARNING: PLASE DO NOT CLOSE THIS APPLICATION UNTIL A COMPLETION POPUP APPEARS.",
                                    "Confirmation",
                                    MessageBoxButtons.OKCancel,
                                    MessageBoxIcon.Information);

                if (res == DialogResult.OK)
                {
                    PortibleModeToggle(0);
                }
            }
            else
            {
                DialogResult res =
                    MessageBox.Show("NOTE: Pressing OK will move the following folders to the CURRENT folder (config, Shortcuts) and will move Taskbar Groups Background exe to the CURENT folder.\r\n\r\nWARNING: PLASE DO NOT CLOSE THIS APPLICATION UNTIL A COMPLETION POPUP APPEARS.",
                                    "Confirmation",
                                    MessageBoxButtons.OKCancel,
                                    MessageBoxIcon.Information);

                if (res == DialogResult.OK)
                {
                    PortibleModeToggle(1);
                }
            }
        }

        public static void ChangeAllShortcuts()
        {
            var files = Directory.GetFiles(Paths.ShortcutsPath, "*.lnk");
            var wshShell = new WshShell();

            foreach (var filePath in files)
            {
                var newShortcut = (IWshShortcut)wshShell.CreateShortcut(filePath);
                newShortcut.TargetPath = Paths.BackgroundApplication;

                newShortcut.IconLocation =
                    Path.Combine(Paths.ConfigPath, newShortcut.Arguments.Trim(), "GroupIcon.ico");

                newShortcut.WorkingDirectory = Path.Combine(Paths.ConfigPath, newShortcut.Arguments.Trim());
                newShortcut.Save();
            }
        }

        // Moves files from AppData to current folder and vice versa
        // This is based off of the array below
        // Index 0 = Current Path
        // Index 1 = Default Path

        // Mode 0 = Current path -> Default Path (Turning off)
        // Mode 1 = Default path -> Current Path (Turning on)
        private void PortibleModeToggle(int mode)
        {
            string[,] folderArray =
            {
                { Path.Combine(Paths.ExeFolder, "config"), Paths.DefaultConfigPath },
                { Path.Combine(Paths.ExeFolder, "Shortcuts"), Paths.DefaultShortcutsPath }
            };

            string[,] fileArray =
            {
                { Path.Combine(Paths.ExeFolder, "Taskbar Groups Background.exe"), Paths.DefaultBackgroundPath },
                { Path.Combine(Paths.ExeFolder, "Settings.xml"), Settings.DefaultSettingsPath }
            };

            int int1;
            int int2;

            if (mode == 0)
            {
                int1 = 0;
                int2 = 1;
            }
            else
            {
                int1 = 1;
                int2 = 0;
            }

            try
            {
                // Kill off the background process
                Category.CloseBackgroundApp();

                IFileManager fm = new TxFileManager();

                using (var scope1 = new TransactionScope())
                {
                    Settings.SettingInfo.PortableMode = true;
                    Settings.WriteXml();

                    for (var i = 0; i < folderArray.Length / 2; i++)
                    {
                        if (fm.DirectoryExists(folderArray[i, int1]))
                        {
                            // Need to use another method to move from one partition to another
                            FileSystem.MoveDirectory(folderArray[i, int1], folderArray[i, int2]);

                            // Folders may still reside after being moved
                            if (fm.DirectoryExists(folderArray[i, int1])
                                && !Directory.EnumerateFileSystemEntries(folderArray[i, int1])
                                             .Any())
                            {
                                fm.DeleteDirectory(folderArray[i, int1]);
                            }
                        }
                        else
                        {
                            fm.CreateDirectory(folderArray[i, int2]);
                        }
                    }

                    for (var i = 0; i < fileArray.Length / 2; i++)
                    {
                        if (fm.FileExists(fileArray[i, int1]))
                        {
                            // Delete if a file is already there (edge cases where the background or another group is created)
                            if (fm.FileExists(fileArray[i, int2]))
                            {
                                fm.Delete(fileArray[i, int2]);
                            }

                            fm.Move(fileArray[i, int1], fileArray[i, int2]);
                        }
                    }

                    if (mode == 0)
                    {
                        Paths.ConfigPath = Paths.DefaultConfigPath;
                        Paths.ShortcutsPath = Paths.DefaultShortcutsPath;
                        Paths.BackgroundApplication = Paths.DefaultBackgroundPath;
                        Settings.SettingsPath = Settings.DefaultSettingsPath;

                        portabilityButton.Tag = "n";
                        portabilityButton.Image = Resources.toggleOff;
                    }
                    else
                    {
                        Paths.ConfigPath = folderArray[0, 0];
                        Paths.ShortcutsPath = folderArray[1, 0];

                        Paths.BackgroundApplication = fileArray[0, 0];
                        Settings.SettingsPath = fileArray[1, 0];

                        portabilityButton.Tag = "y";
                        portabilityButton.Image = Resources.toggleOn;
                    }

                    ChangeAllShortcuts();

                    scope1.Complete();

                    MessageBox.Show("File moving done!");
                }
            }
            catch (IOException e)
            {
                MessageBox.Show("The application does not have access to this directory!\r\n\r\nError: " + e.Message);
            }
        }

        private void frmClient_SizeChanged(object sender, EventArgs e) => Reset();
    }
}