namespace MadDogs.TaskbarGroups.Editor.User_controls
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;

    using Classes;

    using Forms;

    public partial class UcCategoryPanel : UserControl
    {
        public Category Category;
        public FrmClient Client;

        //
        // endregion
        //
        public PictureBox ShortcutPanel;

        public UcCategoryPanel(FrmClient client, Category category)
        {
            InitializeComponent();
            Client = client;
            Category = category;
            lblTitle.Text = Regex.Replace(category.Name, @"(_)+", " ");
            picGroupIcon.BackgroundImage = Category.LoadIconImage();

            // starting values for position of shortcuts
            var x = (int)(90 * FrmClient.EDpi / 96);
            var y = (int)(56 * FrmClient.EDpi / 96);
            var columns = 1;

            if (!Directory.Exists(Path.Combine(Paths.ConfigPath, category.Name, "Icons")))
            {
                category.CacheIcons();
            }

            foreach (ProgramShortcut psc in
                     Category.ShortcutList) // since this is calculating uc height it cant be placed in load
            {
                if (columns == 8)
                {
                    x = (int)(90 * FrmClient.EDpi / 96); // resetting x
                    y += (int)(40 * FrmClient.EDpi / 96); // adding new row

                    //this.Height += (int)(40 * frmClient.eDpi / 96);
                    columns = 1;
                }

                CreateShortcut(x, y, psc);
                x += (int)(50 * FrmClient.EDpi / 96);
                columns++;
            }
        }

        private void CreateShortcut(int x, int y, ProgramShortcut programShortcut)
        {
            // creating shortcut picturebox from shortcut
            ShortcutPanel = new PictureBox
                            {
                                BackColor = Color.Transparent,
                                Location = new Point(x, y),
                                Size = new Size((int)(30 * FrmClient.EDpi / 96), (int)(30 * FrmClient.EDpi / 96)),
                                BackgroundImageLayout = ImageLayout.Stretch,
                                TabStop = false
                            };

            ShortcutPanel.MouseEnter += (sender, e) => Client.EnterControl(sender, e, this);
            ShortcutPanel.MouseLeave += (sender, e) => Client.LeaveControl(sender, e, this);
            ShortcutPanel.Click += (sender, e) => OpenFolder(sender, e);

            // Check if file is stil existing and if so render it
            if (File.Exists(programShortcut.FilePath)
                || Directory.Exists(programShortcut.FilePath)
                || programShortcut.IsWindowsApp)
            {
                ShortcutPanel.BackgroundImage =
                    ImageFunctions.ResizeImage(Category.LoadImageCache(programShortcut),
                                               ShortcutPanel.Width,
                                               ShortcutPanel.Height);
            }
            else // if file does not exist
            {
                ShortcutPanel.BackgroundImage = Resources.Error;
                var tt = new ToolTip { InitialDelay = 0, ShowAlways = true };
                tt.SetToolTip(ShortcutPanel, "Program does not exist");
            }

            pnlShortcuts.Controls.Add(ShortcutPanel);
            ShortcutPanel.Show();
            ShortcutPanel.BringToFront();
        }

        private void ucNewCategory_Load(object sender, EventArgs e) =>
            cmdDelete.Top = (Height / 2) - (cmdDelete.Height / 2);

        public void OpenFolder(object sender, EventArgs e)
        {
            // Open the shortcut folder for the group when click on category panel

            // Build path based on the directory of the main .exe file
            var filePath = Path.Combine(Paths.ShortcutsPath, Regex.Replace(Category.Name, @"_+", " ") + ".lnk");

            // Open directory in explorer and highlighting file
            Process.Start("explorer.exe", string.Format("/select,\"{0}\"", filePath));
        }

        private void cmdDelete_Click(object sender, EventArgs e)
        {
            if (!Client.EditOpened)
            {
                var editGroup = new FrmGroup(Client, Category);
                editGroup.Show();
                editGroup.BringToFront();
                editGroup.FormClosed += frmGroup_Closed;
                Client.EditOpened = true;
            }
        }

        public static Bitmap LoadBitmap(string path) // needed to access img without occupying read/write
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(stream))
            {
                var memoryStream = new MemoryStream(reader.ReadBytes((int)stream.Length));
                reader.Close();
                stream.Close();

                return new Bitmap(memoryStream);
            }
        }

        private void lblTitle_MouseEnter(object sender, EventArgs e) => Client.EnterControl(sender, e, this);

        private void lblTitle_MouseLeave(object sender, EventArgs e) => Client.LeaveControl(sender, e, this);

        private void frmGroup_Closed(object sender, FormClosedEventArgs e) => Client.EditOpened = false;
    }
}