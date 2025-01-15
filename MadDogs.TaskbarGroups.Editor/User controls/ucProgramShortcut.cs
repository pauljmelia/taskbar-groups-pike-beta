namespace MadDogs.TaskbarGroups.Editor.User_controls
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;

    using Classes;

    using Forms;

    public partial class UcProgramShortcut : UserControl
    {
        private readonly float _characterWidth = 8f;

        public bool IsSelected = false;

        public Bitmap Logo;

        public UcProgramShortcut()
        {
            InitializeComponent();
            picShortcut.AllowDrop = true;
        }

        public ProgramShortcut Shortcut { get; set; }
        public FrmGroup MotherForm { get; set; }
        public int Position { get; set; }

        private void ucProgramShortcut_Load(object sender, EventArgs e)
        {
            // Ensure the shortcut name cannot be empty
            if (Shortcut.Name == "")
            {
                Shortcut.Name = FrmGroup.GetShortcutName(Shortcut.Name, Shortcut.IsWindowsApp, Shortcut.FilePath);
            }

            txtShortcutName.Text = Shortcut.Name;
            /*
            // Grab the file name without the extension to be used later as the naming scheme for the icon .jpg image
            if (Shortcut.isWindowsApp)
            {
                txtShortcutName.Text = handleWindowsApp.findWindowsAppsName(Shortcut.FilePath);
            } else if (Shortcut.name == "")
            {
                if (File.Exists(Shortcut.FilePath) && Path.GetExtension(Shortcut.FilePath).ToLower() == ".lnk")
                {
                    txtShortcutName.Text = frmGroup.handleExtName(Shortcut.FilePath);
                }
                else
                {
                    txtShortcutName.Text = Path.GetFileNameWithoutExtension(Shortcut.FilePath);
                }
            } else
            {
                txtShortcutName.Text = Shortcut.name;
            }
            */
            txtShortcutName.Width = Width - txtShortcutName.Bounds.Left - (Width - pictureBox1.Bounds.Left);

            var cacheIconPath = MotherForm.Category.GenerateCachePath(Shortcut);
            Image bkgImg = new Bitmap(1, 1);

            if (File.Exists(cacheIconPath))
            {
                bkgImg = FrmGroup.BitmapFromFile(cacheIconPath);
            }
            else
            {
                if (Shortcut.IsWindowsApp)
                {
                    bkgImg = HandleWindowsApp.GetWindowsAppIcon(Shortcut.FilePath, true);
                }
                else if
                    (File.Exists(Shortcut
                                     .FilePath)) // Checks if the shortcut actually exists; if not then display an error image
                {
                    var imageExtension = Path.GetExtension(Shortcut.FilePath)
                                             .ToLower();

                    // Start checking if the extension is an lnk (shortcut) file
                    // Depending on the extension, the icon can be directly extracted or it has to be gotten through other methods as to not get the shortcut arrow
                    if (imageExtension == ".lnk")
                    {
                        bkgImg = FrmGroup.HandleLnkExt(Shortcut.FilePath);
                    }
                    else
                    {
                        bkgImg = Icon.ExtractAssociatedIcon(Shortcut.FilePath)
                                     .ToBitmap();
                    }
                }
                else if (Directory.Exists(Shortcut.FilePath))
                {
                    try
                    {
                        bkgImg = HandleFolder.GetFolderIcon(Shortcut.FilePath)
                                             .ToBitmap();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else
                {
                    bkgImg = Resources.Error;
                }
            }

            picShortcut.BackgroundImage = bkgImg;

            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            txtShortcutName.GotFocus += txtShortcutName_GotFocus;

            txtShortcutName.Text = Truncate(Shortcut.Name, (int)Math.Floor(txtShortcutName.Width / _characterWidth));
        }

        public void ucProgramShortcut_ReadjustArrows(int pos)
        {
            Position = pos;

            // Reset to default
            // Rerun checks
            if (Position == 0)
            {
                cmdNumUp.Enabled = false;
                cmdNumUp.BackgroundImage = Resources.NumUpGray;
            }
            else
            {
                cmdNumUp.Enabled = true;
                cmdNumUp.BackgroundImage = Resources.NumUp;
            }

            if (Position == MotherForm.Category.ShortcutList.Count - 1)
            {
                cmdNumDown.Enabled = false;
                cmdNumDown.BackgroundImage = Resources.NumDownGray;
            }
            else
            {
                cmdNumDown.Enabled = true;
                cmdNumDown.BackgroundImage = Resources.NumDown;
            }
        }

        private void ucProgramShortcut_MouseEnter(object sender, EventArgs e) => UcSelected();

        private void ucProgramShortcut_MouseLeave(object sender, EventArgs e)
        {
            if (MotherForm.SelectedShortcut != this)
            {
                UcDeselected();
            }
        }

        private void cmdNumUp_Click(object sender, EventArgs e) =>
            MotherForm.Swap(MotherForm.Category.ShortcutList, Position, Position - 1);

        private void cmdNumDown_Click(object sender, EventArgs e) =>
            MotherForm.Swap(MotherForm.Category.ShortcutList, Position, Position + 1);

        private void cmdDelete_Click(object sender, EventArgs e) => MotherForm.DeleteShortcut(Shortcut);

        // Handle what is selected/deselected when a shortcut is clicked on
        // If current item is already selected, then deselect everything
        private void ucProgramShortcut_Click(object sender, EventArgs e)
        {
            if (MotherForm.SelectedShortcut == this)
            {
                MotherForm.ResetSelection();

                //IsSelected = false;
            }
            else
            {
                if (MotherForm.SelectedShortcut != null)
                {
                    MotherForm.ResetSelection();

                    //IsSelected = false;
                }

                MotherForm.EnableSelection(this);

                //IsSelected = true;
            }
        }

        public void UcDeselected()
        {
            txtShortcutName.DeselectAll();
            txtShortcutName.Enabled = false;
            txtShortcutName.Enabled = true;
            txtShortcutName.TabStop = false; // Deselecting textbox text

            BackColor = Color.FromArgb(31, 31, 31);
            txtShortcutName.BackColor = Color.FromArgb(31, 31, 31);
            cmdNumUp.BackColor = Color.FromArgb(31, 31, 31);
            cmdNumDown.BackColor = Color.FromArgb(31, 31, 31);
        }

        public void UcSelected()
        {
            BackColor = Color.FromArgb(26, 26, 26);
            txtShortcutName.BackColor = Color.FromArgb(26, 26, 26);
            cmdNumUp.BackColor = Color.FromArgb(26, 26, 26);
            cmdNumDown.BackColor = Color.FromArgb(26, 26, 26);
        }

        private void lbTextbox_TextChanged(object sender, EventArgs e)
        {
            if (txtShortcutName.Focused)
            {
                Shortcut.Name = txtShortcutName.Text;
            }
        }

        private void ucProgramShortcut_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                picShortcut.Focus();

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void txtShortcutName_Click(object sender, EventArgs e)
        {
            if (!IsSelected)
            {
                ucProgramShortcut_Click(sender, e);
            }
        }

        private void ucProgramShortcut_SizeChanged(object sender, EventArgs e)
        {
            try
            {
                txtShortcutName.Width = Width - txtShortcutName.Bounds.Left - (Width - pictureBox1.Bounds.Left);

                if (!txtShortcutName.Focused)
                {
                    txtShortcutName.Text =
                        Truncate(Shortcut.Name, (int)Math.Floor(txtShortcutName.Width / _characterWidth));
                }
            }
            catch (Exception) { }
        }

        public static string Truncate(string value, int maxChars) =>
            value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";

        private void txtShortcutName_GotFocus(object sender, EventArgs e)
        {
            if (txtShortcutName.Text != Shortcut.Name)
            {
                txtShortcutName.Text = Shortcut.Name;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (!IsSelected)
            {
                ucProgramShortcut_Click(sender, e);
            }

            if (!txtShortcutName.Focused)
            {
                txtShortcutName.Focus();
                txtShortcutName.Select(txtShortcutName.TextLength, txtShortcutName.TextLength);
            }
        }

        private void picShortcut_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog // ask user to select img as group icon
                                 {
                                     InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                                     Title = "Select Group Icon",
                                     CheckFileExists = true,
                                     CheckPathExists = true,
                                     DefaultExt = "img",
                                     Filter =
                                         "Image files and exec (*.jpg, *.jpeg, *.jpe, *.jfif, *.png, *.exe, *.ico) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png; *.ico; *.exe",
                                     FilterIndex = 2,
                                     RestoreDirectory = true,
                                     ReadOnlyChecked = true,
                                     DereferenceLinks = false
                                 };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var imageExtension = Path.GetExtension(openFileDialog.FileName)
                                         .ToLower();

                picShortcut.BackgroundImage = MotherForm.HandleIcon(openFileDialog.FileName, imageExtension);
            }
        }

        private void picShortcut_DragEnter(object sender, DragEventArgs e) =>
            MotherForm.CheckExtensions(e, MotherForm.NewExt);

        private void picShortcut_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            var imageExtension = Path.GetExtension(files[0])
                                     .ToLower();

            if (files.Length == 1 && MotherForm.NewExt.Contains(imageExtension) && File.Exists(files[0]))
            {
                // Checks if the files being added/dropped are an .exe or .lnk in which tye icons need to be extracted/processed
                picShortcut.BackgroundImage = MotherForm.HandleIcon(files[0], imageExtension);
            }
        }

        // Bind this so that any changes to the logos automatically bind to changing the logo variable
        private void picShortcut_BackgroundImageChanged(object sender, EventArgs e) =>
            Logo = new Bitmap(picShortcut.BackgroundImage);
    }
}