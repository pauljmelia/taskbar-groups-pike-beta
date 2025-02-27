﻿namespace MadDogs.TaskbarGroups.Editor.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using System.Transactions;
    using System.Windows.Forms;

    using ChinhDo.Transactions;

    using Classes;

    using IWshRuntimeLibrary;

    using Microsoft.WindowsAPICodePack.Dialogs;
    using Microsoft.WindowsAPICodePack.Shell;

    using Shell32;

    using User_controls;

    using File = System.IO.File;
    using Folder = Shell32.Folder;
    using IDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;

    public partial class FrmGroup : Form
    {
        public static Shell Shell = new Shell();

        public Category Category;
        public FrmClient Client;

        private readonly Regex _directoryRegex =
            new Regex(@"^[a-zA-Z]:\\(?:(?:(?![<>:""\/\\|?*]).)+(?:(?<![ .])\\)?)*$");

        private readonly string[] _extensionExt = { ".exe", ".lnk", ".url" };

        private readonly Regex _fileRegex = new Regex(@"^(?:[\w]\:|\\)(?:\\[A-Za-z_\-\s0-9\.]+)+(?:\.[a-zA-Z].*$)");
        private readonly string[] _imageExt = { ".png", ".jpg", ".jpe", ".jfif", ".jpeg" };
        public bool IsNew;
        public string[] NewExt;

        public UcProgramShortcut SelectedShortcut;
        private readonly string[] _specialImageExt = { ".ico", ".exe", ".lnk" };

        //--------------------------------------
        // CTOR AND LOAD
        //--------------------------------------

        // CTOR for creating a new group
        public FrmGroup(FrmClient client)
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            // Setting from profile
            ProfileOptimization.StartProfile("frmGroup.Profile");

            InitializeComponent();

            // Setting default category properties  
            Category = new Category { ShortcutList = new List<ProgramShortcut>() };
            Client = client;
            IsNew = true;

            // Setting default control values
            cmdDelete.Visible = false;
            cmdSave.Left += 70;
            cmdExit.Left += 70;
            radioDark.Checked = true;
        }

        // CTOR for editing an existing group
        public FrmGroup(FrmClient client, Category category)
        {
            // Setting form profile
            ProfileOptimization.StartProfile("frmGroup.Profile");

            InitializeComponent();

            // Setting properties
            Category = category;
            Client = client;
            IsNew = false;

            // Setting control values from loaded group
            Text = "Edit group";
            txtGroupName.Text = Regex.Replace(Category.Name, @"(_)+", " ");
            pnlAllowOpenAll.Checked = category.AllowOpenAll;
            cmdAddGroupIcon.BackgroundImage = Category.LoadIconImage();
            lblNum.Text = Category.Width.ToString();
            lblOpacity.Text = Category.Opacity.ToString();
            lblIconSize.Text = Category.IconSize.ToString();
            lblIconSeparation.Text = Category.Separation.ToString();

            if (Category.ColorString
                == null) // Handles if groups is created from earlier releas w/o ColorString property
            {
                Category.ColorString = ColorTranslator.ToHtml(Color.FromArgb(31, 31, 31));
            }

            if (Category.ColorString == "sys")
            {
                radioSystem.Checked = true;
            }
            else
            {
                Color categoryColor = ImageFunctions.FromString(Category.ColorString);

                if (categoryColor == Color.FromArgb(31, 31, 31))
                {
                    radioDark.Checked = true;
                }
                else if (categoryColor == Color.FromArgb(230, 230, 230))
                {
                    radioLight.Checked = true;
                }
                else
                {
                    radioCustom.Checked = true;

                    //pnlCustomColor.Visible = true;
                    pnlCustomColor.BackColor = categoryColor;

                    if (category.HoverColor != null)
                    {
                        pnlCustomColor1.BackColor = ImageFunctions.FromString(category.HoverColor);
                    }
                    else
                    {
                        pnlCustomColor1.BackColor = category.CalculateHoverColor();
                    }
                }
            }

            colorConfigPage.Update();
            colorConfigPage.Refresh();

            // Loading existing shortcutpanels
            var position = 0;

            foreach (ProgramShortcut psc in category.ShortcutList)
            {
                LoadShortcut(psc, position);
                position++;
            }

            pnlShortcuts_ControlAdded(this, new ControlEventArgs(this));
        }

        [DllImport("shell32.dll")]
        private static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid,
                                                       uint dwFlags,
                                                       IntPtr hToken,
                                                       out IntPtr ppszPath);

        // Handle scaling etc(?) (WORK IN PROGRESS)
        private void frmGroup_Load(object sender, EventArgs e)
        {
            // Scaling form (WORK IN PROGRESS)
            MaximumSize = new Size(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);

            typeof(Control).GetProperty("ResizeRedraw", BindingFlags.NonPublic | BindingFlags.Instance)
                           .SetValue(pnlDeleteConfo, true, null);

            NewExt = _imageExt.Concat(_specialImageExt)
                             .ToArray();
        }

        //--------------------------------------
        // SHORTCUT PANEL HANLDERS
        //--------------------------------------

        // Load up shortcut panel
        public void LoadShortcut(ProgramShortcut psc, int position)
        {
            pnlShortcuts.AutoScroll = false;

            var ucPsc = new UcProgramShortcut
                        {
                            MotherForm = this,
                            Shortcut = psc,
                            Position = position,
                            Width = pnlAddShortcut.Width - (3 * (int)(FrmClient.EDpi / 96))
                        };

            pnlShortcuts.Controls.Add(ucPsc);
            ucPsc.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            ucPsc.Show();
            ucPsc.SendToBack();

            if (pnlShortcuts.Controls.Count < 6)
            {
                pnlShortcuts.Height += 50 * (int)(FrmClient.EDpi / 96);
                pnlAddShortcut.Top += 50 * (int)(FrmClient.EDpi / 96);
            }

            //ucPsc.Location = new Point(25 * (int)(frmClient.eDpi / 96), (pnlShortcuts.Controls.Count * 50 * (int)(frmClient.eDpi / 96)) - 50 * (int)(frmClient.eDpi / 96));

            pnlShortcuts.AutoScroll = true;
        }

        // Adding shortcut by button
        private void pnlAddShortcut_Click(object sender, EventArgs e)
        {
            ResetSelection();

            lblErrorShortcut.Visible = false; // resetting error msg

            if (Category.ShortcutList.Count >= 50)
            {
                lblErrorShortcut.Text = "Max 50 shortcuts in one group";
                lblErrorShortcut.BringToFront();
                lblErrorShortcut.Visible = true;
            }

            var openFileDialog = new OpenFileDialog // ask user to select exe file
                                 {
                                     InitialDirectory =
                                         Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms),
                                     Title = "Create New Shortcut",
                                     CheckFileExists = true,
                                     CheckPathExists = true,
                                     Multiselect = true,
                                     DefaultExt = "exe",
                                     Filter = "Executable or Shortcut|*.exe;*.lnk;*.url;*.bat|All files (*.*)|*.*",
                                     RestoreDirectory = true,
                                     ReadOnlyChecked = true,
                                     DereferenceLinks = false
                                 };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (var file in openFileDialog.FileNames)
                {
                    AddShortcut(file);
                }

                ResetSelection();
            }

            if (pnlShortcuts.Controls.Count != 0)
            {
                pnlShortcuts.ScrollControlIntoView(pnlShortcuts.Controls[0]);
            }
        }

        // Handle dropped programs into the add program/shortcut field
        private void PnlDragDropExt(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files == null)
            {
                var shellObj = ShellObjectCollection.FromDataObject((IDataObject)e.Data);

                foreach (ShellNonFileSystemItem item in shellObj)
                {
                    AddShortcut(item.ParsingName, true);
                }
            }
            else
            {
                // Loops through each file to make sure they exist and to add them directly to the shortcut list
                foreach (var file in files)
                {
                    if ((_extensionExt.Contains(Path.GetExtension(file)) && File.Exists(file)) || Directory.Exists(file))
                    {
                        AddShortcut(file);
                    }
                }
            }

            if (pnlShortcuts.Controls.Count != 0)
            {
                pnlShortcuts.ScrollControlIntoView(pnlShortcuts.Controls[0]);
            }

            ResetSelection();
        }

        // Handle adding the shortcut to list
        private void AddShortcut(string file, bool isExtension = false)
        {
            var workingDirec = GetProperDirectory(file);
            var appName = "";
            var appFilePath = ExpandEnvironment(file);

            appName = GetShortcutName(appName, isExtension, appFilePath);

            var psc = new ProgramShortcut
                      {
                          FilePath = appFilePath,
                          IsWindowsApp = isExtension,
                          WorkingDirectory = workingDirec,
                          Name = appName
                      }; //Create new shortcut obj

            Category.ShortcutList.Add(psc); // Add to panel shortcut list
            LoadShortcut(psc, Category.ShortcutList.Count - 1);
        }

        // Handle setting/getting shortcut name
        public static string GetShortcutName(string appName, bool isExtension, string appFilePath)
        {
            // Grab the file name without the extension to be used later as the naming scheme for the icon .jpg image
            if (isExtension)
            {
                return HandleWindowsApp.FindWindowsAppsName(appFilePath);
            }

            if (File.Exists(appFilePath)
                && Path.GetExtension(appFilePath)
                       .ToLower()
                == ".lnk")
            {
                return HandleExtName(appFilePath);
            }

            return Path.GetFileNameWithoutExtension(appFilePath);
        }

        // Delete shortcut
        public void DeleteShortcut(ProgramShortcut psc)
        {
            ResetSelection();

            Category.ShortcutList.Remove(psc);
            ResetSelection();
            var after = false;
            var controlIndex = 0;

            //int i = 0;

            foreach (UcProgramShortcut ucPsc in pnlShortcuts.Controls)
            {
                if (after)
                {
                    //ucPsc.Top -= 50 * (int)(frmClient.eDpi / 96);
                    ucPsc.Position -= 1;
                }

                if (ucPsc.Shortcut == psc)
                {
                    //i = pnlShortcuts.Controls.IndexOf(ucPsc);

                    controlIndex = pnlShortcuts.Controls.IndexOf(ucPsc);

                    if (controlIndex + 1 != pnlShortcuts.Controls.Count)
                    {
                        try
                        {
                            pnlShortcuts.ScrollControlIntoView(pnlShortcuts.Controls[controlIndex + 1]);
                        }
                        catch
                        {
                            if (pnlShortcuts.Controls.Count != 0)
                            {
                                pnlShortcuts.ScrollControlIntoView(pnlShortcuts.Controls[controlIndex - 1]);
                            }
                        }
                    }

                    after = true;
                }
            }

            pnlShortcuts.Controls.Remove(pnlShortcuts.Controls[controlIndex]);

            /*
            if (pnlShortcuts.Controls.Count < 5)
            {
                pnlShortcuts.Height -= 50 * (int)(frmClient.eDpi / 96);
                pnlAddShortcut.Top -= 50 * (int)(frmClient.eDpi / 96);
            }
            */
            pnlShortcuts_ControlAdded(this, new ControlEventArgs(this));
        }

        // Change positions of shortcut panels
        public void Swap<T>(IList<T> list, int indexA, int indexB)
        {
            // Get move amount via eDPI calculation
            var moveAmount = 50 * (int)(FrmClient.EDpi / 96);
            (list[indexA], list[indexB]) = (list[indexB], list[indexA]); // Swap items
            ResetSelection(); // Reset item selection

            if (indexA > indexB)
            {
                //pnlShortcuts.Controls[indexA].Top -= moveAmount;
                //pnlShortcuts.Controls[indexB].Top += moveAmount;

                pnlShortcuts.Controls.SetChildIndex(pnlShortcuts.Controls[indexB], indexA);
            }
            else
            {
                //pnlShortcuts.Controls[indexA].Top += moveAmount;
                //pnlShortcuts.Controls[indexB].Top -= moveAmount;
                pnlShortcuts.Controls.SetChildIndex(pnlShortcuts.Controls[indexA], indexB);
            }

            pnlShortcuts_ControlAdded(this, new ControlEventArgs(this));
        }

        //--------------------------------------
        // IMAGE HANDLERS
        //--------------------------------------

        // Adding icon by button
        private void cmdAddGroupIcon_Click(object sender, EventArgs e)
        {
            ResetSelection();

            lblErrorIcon.Visible = false; //resetting error msg

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

                cmdAddGroupIcon.BackgroundImage = HandleIcon(openFileDialog.FileName, imageExtension);
            }
        }

        // Handle drag and dropped images
        private void PnlDragDropImg(object sender, DragEventArgs e)
        {
            ResetSelection();

            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            var imageExtension = Path.GetExtension(files[0])
                                     .ToLower();

            if (files.Length == 1 && NewExt.Contains(imageExtension) && File.Exists(files[0]))
            {
                // Checks if the files being added/dropped are an .exe or .lnk in which tye icons need to be extracted/processed
                cmdAddGroupIcon.BackgroundImage = HandleIcon(files[0], imageExtension);
            }
        }

        public Bitmap HandleIcon(string file, string imageExtension)
        {
            // Checks if the files being added/dropped are an .exe or .lnk in which tye icons need to be extracted/processed
            if (_specialImageExt.Contains(imageExtension))
            {
                if (imageExtension == ".lnk")
                {
                    return HandleLnkExt(file);
                }

                return Icon.ExtractAssociatedIcon(file)
                           .ToBitmap();
            }

            return BitmapFromFile(file);
        }

        // Handle returning images of icon files (.lnk)
        public static Bitmap HandleLnkExt(string file)
        {
            /*
            Shell shell = new Shell();
            string path = Path.GetDirectoryName(file);   // Get individual path/directory strings
            string file_name = Path.GetFileName(file);
            Shell32.Folder folder = shell.NameSpace(path); // Pass into Shell32 to get link for the shortcut
            FolderItem folderItem = folder.ParseName(file_name);


            ShellLinkObject link = (ShellLinkObject)folderItem.GetLink;
            */
            var shellData = WindowsLnkFile.FromFile(file);

            var targetPath = "";
            var iconLc = "";

            // Pass #1 using Kaitai reading
            //LnkFile linkFile = Lnk.Lnk.LoadFile(file);

            if (shellData.RelPath != null)
            {
                var test = Path.GetDirectoryName(file);
                targetPath = Path.GetFullPath(Path.GetDirectoryName(file) + "\\" + shellData.RelPath.Str);
            }

            if (shellData.IconLocation != null && shellData.IconLocation.Str != null)
            {
                targetPath = shellData.IconLocation.Str;
            }

            /*

            // Pass #2 using Lnk library
            if(string.IsNullOrEmpty(targetPath))
            {
                LnkFile linkFile = Lnk.Lnk.LoadFile(file);
                linkFile.TargetIDs.ForEach(s =>
                {
                    var sType = s.GetType().Name.ToUpper();
                    if (sType == "SHELLBAG0X2F")
                    {
                        targetPath += ((ShellBag0X2F)s).Value + "\\";
                    }
                    else if (sType == "SHELLBAG0X31")
                    {
                        targetPath += ((ShellBag0X31)s).ShortName + "\\";
                    }
                    else if (sType == "SHELLBAG0X32")
                    {
                        targetPath += ((ShellBag0X32)s).ShortName;
                    }
                    else if (sType == "SHELLBAG0X00")
                    {
                        ShellBag0X00 castedShellBag = ((ShellBag0X00)s);
                        //targetPath += ((ShellBag0X00)s).PropertyStore.Sheets.First().PropertyNames.First().Value; // Super super hacky
                        for (int i = 0; i < castedShellBag.PropertyStore.Sheets.Count; i++)
                        {
                            var testPath = "";
                            if (castedShellBag.PropertyStore.Sheets[i].PropertyNames.TryGetValue("2", out testPath))
                            {
                                if (System.IO.File.Exists(testPath))
                                {
                                    targetPath = testPath;
                                }
                            }
                        }

                        //((ShellBag0X00)s).PropertyStore.Sheets.First().PropertyNames.TryGetValue(2, out testPath); // Super super hacky
                    }
                });


                if (string.IsNullOrEmpty(iconLC))
                {
                    iconLC = linkFile.IconLocation;
                }
            }

            */

            //var iconLC = linkFile.IconLocation;

            // Pass #3 using IWshShortcut (native)
            if (string.IsNullOrEmpty(targetPath))
            {
                try
                {
                    //string[] testPath = handleWindowsApp.GetLnkTarget(file); // Try using old method to get path
                    var lnkIcon = (IWshShortcut)new WshShell().CreateShortcut(file);
                    var icLocation = lnkIcon.IconLocation.Split(',');
                    var testPath = lnkIcon.TargetPath;

                    if (string.IsNullOrEmpty(targetPath) && (File.Exists(testPath) || Directory.Exists(testPath)))
                    {
                        targetPath = testPath;
                    }

                    if (string.IsNullOrEmpty(iconLc))
                    {
                        iconLc = icLocation[0];
                    }
                }
                catch (Exception e) { }
            }

            //String[] icLocation = iconLC.Split(',');
            // Check if iconLocation exists to get an .ico from; if not then take the image from the .exe it is referring to
            // Checks for link iconLocations as those are used by some applications

            // Return the icon
            try
            {
                if (!string.IsNullOrEmpty(iconLc) && !iconLc.Contains("http"))
                {
                    return Icon.ExtractAssociatedIcon(Path.GetFullPath(ExpandEnvironment(iconLc)))
                               .ToBitmap();
                }

                if (string.IsNullOrEmpty(iconLc) && (targetPath == "" || !File.Exists(targetPath)))
                {
                    return HandleWindowsApp.GetWindowsAppIcon(file);
                }

                return Icon.ExtractAssociatedIcon(Path.GetFullPath(ExpandEnvironment(targetPath)))
                           .ToBitmap();
            }
            catch (Exception e)
            {
                return Icon.ExtractAssociatedIcon(Path.GetFullPath(ExpandEnvironment(file)))
                           .ToBitmap();
            }
        }

        public static string HandleExtName(string file)
        {
            var fileName = Path.GetFileName(file);
            file = Path.GetDirectoryName(Path.GetFullPath(file));
            Folder shellFolder = Shell.NameSpace(file);

            FolderItem shellItem = shellFolder.Items()
                                              .Item(fileName);

            return shellItem.Name;
        }

        // Below two functions highlights the background as you would if you hovered over it with a mosue
        // Use checkExtension to allow file dropping after a series of checks
        // Only highlights if the files being dropped are valid in extension wise
        private void PnlDragDropEnterExt(object sender, DragEventArgs e)
        {
            ResetSelection();

            if (CheckExtensions(e, _extensionExt))
            {
                pnlAddShortcut.BackColor = Color.FromArgb(23, 23, 23);
            }
        }

        private void PnlDragDropEnterImg(object sender, DragEventArgs e)
        {
            ResetSelection();

            if (CheckExtensions(e, NewExt))
            {
                pnlGroupIcon.BackColor = Color.FromArgb(23, 23, 23);
            }
        }

        // Series of checks to make sure it can be dropped
        public bool CheckExtensions(DragEventArgs e, string[] exts)
        {
            // Make sure the file can be dragged dropped
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return false;
            }

            if (e.Data.GetDataPresent("Shell IDList Array"))
            {
                e.Effect = e.AllowedEffect;

                return true;
            }

            // Get the list of files of the files dropped
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            // Loop through each file and make sure the extension is allowed as defined by a series of arrays at the top of the script
            foreach (var file in files)
            {
                var ext = Path.GetExtension(file);

                if (exts.Contains(ext.ToLower()) || Directory.Exists(file))
                {
                    // Gives the effect that it can be dropped and unlocks the ability to drop files in
                    e.Effect = DragDropEffects.Copy;

                    return true;
                }

                return false;
            }

            return false;
        }

        //--------------------------------------
        // SAVE/EXIT/DELETE GROUP
        //--------------------------------------

        // Exit editor
        private void cmdExit_Click(object sender, EventArgs e)
        {
            Close();
            Client.Reload(); //flush and reload category panel
            Client.Reset();
        }

        // Save group
        private void cmdSave_Click(object sender, EventArgs e)
        {
            ResetSelection();

            //List <Directory> directories = 

            if (txtGroupName.Text == "Name the new group...") // Verify category name
            {
                lblErrorTitle.Text = "Must select a name";
                lblErrorTitle.Visible = true;
            }
            else if ((IsNew && Directory.Exists(Path.Combine(Paths.ConfigPath, txtGroupName.Text)))
                     || (!IsNew
                         && Category.Name != txtGroupName.Text
                         && Directory.Exists(Path.Combine(Paths.ConfigPath, txtGroupName.Text))))
            {
                lblErrorTitle.Text = "There is already a group with that name";
                lblErrorTitle.Visible = true;
            }
            else if (!new Regex("^[0-9a-zA-Z \b]+$").IsMatch(txtGroupName.Text))
            {
                lblErrorTitle.Text = "Name must not have any special characters";
                lblErrorTitle.Visible = true;
            }
            else if (Category.ShortcutList.Count == 0) // Verify shortcuts
            {
                lblErrorShortcut.Text = "Must select at least one shortcut";
                lblErrorShortcut.Visible = true;
            }
            else
            {
                if ((string)cmdAddGroupIcon.Tag == "Unchanged") // Verify icon
                {
                    cmdAddGroupIcon.BackgroundImage = ConstructIcons();

                    //lblErrorIcon.Text = "Must select group icon";
                    //lblErrorIcon.Visible = true;
                }

                try
                {
                    if (!IsNew)
                    {
                        //
                        // Delete old config
                        //
                        var configPath = Path.Combine(Paths.ConfigPath, Category.Name);

                        var shortcutPath = Path.Combine(Paths.ShortcutsPath,
                                                        Regex.Replace(Category.Name, @"(_)+", " ") + ".lnk");

                        try
                        {
                            IFileManager fm = new TxFileManager();

                            using (var scope1 = new TransactionScope())
                            {
                                fm.DeleteDirectory(configPath);
                                fm.Delete(shortcutPath);
                                scope1.Complete();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox
                                .Show("Please close all programs used within the taskbar group in order to save!");

                            return;
                        }
                    }

                    //
                    // Creating new config
                    //
                    //int width = int.Parse(lblNum.Text);

                    Category.Width = int.Parse(lblNum.Text);

                    //Category category = new Category(txtGroupName.Text, Category.ShortcutList, width, System.Drawing.ColorTranslator.ToHtml(CategoryColor), Category.Opacity); // Instantiate category

                    // Normalize string so it can be used in path; remove spaces
                    Category.Name = Regex.Replace(txtGroupName.Text, @"\s+", "_");

                    Category.CreateConfig(cmdAddGroupIcon.BackgroundImage); // Creating group config files
                    Client.LoadCategory(Path.Combine(Paths.ConfigPath, Category.Name)); // Loading visuals

                    Close();
                    Client.Reload();
                }
                catch (IOException ex)
                {
                    MessageBox.Show(ex.Message);
                }

                Client.Reset();
            }
        }

        // Delete group
        private void cmdDelete_Click(object sender, EventArgs e)
        {
            ResetSelection();

            try
            {
                var configPath = Path.Combine(Paths.ConfigPath, Category.Name);

                var shortcutPath =
                    Path.Combine(Paths.ShortcutsPath, Regex.Replace(Category.Name, @"(_)+", " ") + ".lnk");

                var dir = new DirectoryInfo(configPath);

                try
                {
                    IFileManager fm = new TxFileManager();

                    using (var scope1 = new TransactionScope())
                    {
                        fm.DeleteDirectory(configPath);
                        fm.Delete(shortcutPath);

                        //this.Hide();
                        //this.Dispose();
                        Close();

                        Client.Reload(); //flush and reload category panels
                        scope1.Complete();
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Please close all programs used within the taskbar group in order to delete!");

                    return;
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message);
            }

            Client.Reset();
        }

        //--------------------------------------
        // UI CUSTOMIZATION
        //--------------------------------------

        // Change category width
        private void cmdWidthUp_Click(object sender, EventArgs e)
        {
            ResetSelection();

            var num = int.Parse(lblNum.Text);

            if (num > 19)
            {
                lblErrorNum.Text = "Max width";
                lblErrorNum.Visible = true;
            }
            else
            {
                num++;
                lblErrorNum.Visible = false;
                lblNum.Text = num.ToString();
            }
        }

        private void cmdWidthDown_Click(object sender, EventArgs e)
        {
            ResetSelection();

            var num = int.Parse(lblNum.Text);

            if (num == 1)
            {
                lblErrorNum.Text = "Width cant be less than 1";
                lblErrorNum.Visible = true;
            }
            else
            {
                num--;
                lblErrorNum.Visible = false;
                lblNum.Text = num.ToString();
            }
        }

        // Custom colors
        private void radioCustom_Click(object sender, MouseEventArgs e)
        {
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                Category.ColorString = ColorTranslator.ToHtml(colorDialog.Color);

                //pnlCustomColor.Visible = true;
                pnlCustomColor.BackColor = colorDialog.Color;

                pnlCustomColor1.BackColor = Category.CalculateHoverColor();
                Category.HoverColor = ColorTranslator.ToHtml(pnlCustomColor1.BackColor);
            }
        }

        private void pnlCustomColor1_Click(object sender, EventArgs e)
        {
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                Category.HoverColor = ColorTranslator.ToHtml(colorDialog.Color);
                pnlCustomColor1.BackColor = colorDialog.Color;
            }
        }

        private void radioDark_Click(object sender, EventArgs e) =>
            Category.ColorString = ColorTranslator.ToHtml(Color.FromArgb(31, 31, 31));

        //pnlCustomColor.Visible = false;
        private void radioLight_Click(object sender, EventArgs e) =>
            Category.ColorString = ColorTranslator.ToHtml(Color.FromArgb(230, 230, 230));

        //pnlCustomColor.Visible = false;
        private void radioSystem_Click(object sender, EventArgs e) => Category.ColorString = "sys";

        // Opacity buttons
        private void numOpacUp_Click(object sender, EventArgs e)
        {
            var op = double.Parse(lblOpacity.Text);
            op += 10;
            Category.Opacity = op;
            lblOpacity.Text = op.ToString();
            numOpacDown.Enabled = true;
            numOpacDown.BackgroundImage = Resources.NumDownWhite;

            if (op > 90)
            {
                numOpacUp.Enabled = false;
                numOpacUp.BackgroundImage = Resources.NumUpGray;
            }
        }

        private void numOpacDown_Click(object sender, EventArgs e)
        {
            var op = double.Parse(lblOpacity.Text);
            op -= 10;
            Category.Opacity = op;
            lblOpacity.Text = op.ToString();
            numOpacUp.Enabled = true;
            numOpacUp.BackgroundImage = Resources.NumUpWhite;

            if (op < 10)
            {
                numOpacDown.Enabled = false;
                numOpacDown.BackgroundImage = Resources.NumDownGray;
            }
        }

        //--------------------------------------
        // FORM VISUAL INTERACTIONS
        //--------------------------------------

        private void pnlGroupIcon_MouseEnter(object sender, EventArgs e) =>
            pnlGroupIcon.BackColor = Color.FromArgb(23, 23, 23);

        private void pnlGroupIcon_MouseLeave(object sender, EventArgs e) =>
            pnlGroupIcon.BackColor = Color.FromArgb(31, 31, 31);

        private void pnlAddShortcut_MouseEnter(object sender, EventArgs e) =>
            pnlAddShortcut.BackColor = Color.FromArgb(23, 23, 23);

        private void pnlAddShortcut_MouseLeave(object sender, EventArgs e) =>
            pnlAddShortcut.BackColor = Color.FromArgb(31, 31, 31);

        // Handles placeholder text for group name
        private void txtGroupName_MouseClick(object sender, MouseEventArgs e)
        {
            ResetSelection();

            if (txtGroupName.Text == "Name the new group...")
            {
                txtGroupName.Text = "";
            }
        }

        private void txtGroupName_Leave(object sender, EventArgs e)
        {
            if (txtGroupName.Text == "")
            {
                txtGroupName.Text = "Name the new group...";
            }
        }

        // Error labels
        private void txtGroupName_TextChanged(object sender, EventArgs e) => lblErrorTitle.Visible = false;

        //--------------------------------------
        // SHORTCUT/PROGRAM SELECTION
        //--------------------------------------

        // Deselect selected program/shortcut
        public void ResetSelection()
        {
            // If either timer has pending checks, do them before selected shortcut gets nulled
            if (validaitonTimerDirec.Enabled)
            {
                validaitonTimerDirec.Stop();
                validaitonTimerDirec_Tick(this, new EventArgs());
            }

            if (validationTimerPrgm.Enabled)
            {
                validationTimerPrgm.Stop();
                validationTimer_Tick(this, new EventArgs());
            }

            pnlArgumentTextbox.Enabled = false;
            cmdSelectDirectory.Enabled = false;
            pnlProgramPath.Enabled = false;
            cmdSelectProgramPath.Enabled = false;

            if (SelectedShortcut != null)
            {
                pnlColor.Visible = true;
                pnlArguments.Visible = false;
                SelectedShortcut.UcDeselected();
                SelectedShortcut.IsSelected = false;
                SelectedShortcut = null;
            }
        }

        // Enable the argument textbox once a shortcut/program has been selected
        public void EnableSelection(UcProgramShortcut passedShortcut)
        {
            SelectedShortcut = passedShortcut;
            passedShortcut.UcSelected();
            passedShortcut.IsSelected = true;

            pnlArgumentTextbox.Text = Category.ShortcutList[SelectedShortcut.Position].Arguments;
            pnlArgumentTextbox.Enabled = true;

            pnlProgramPath.Text = Category.ShortcutList[SelectedShortcut.Position].FilePath;
            pnlProgramPath.Enabled = true;
            cmdSelectProgramPath.Enabled = true;

            pnlWorkingDirectory.Text = Category.ShortcutList[SelectedShortcut.Position].WorkingDirectory;
            pnlWorkingDirectory.Enabled = true;
            cmdSelectDirectory.Enabled = true;

            pnlColor.Visible = false;
            pnlArguments.Visible = true;
        }

        // Set the argument property to whatever the user set
        private void pnlArgumentTextbox_TextChanged(object sender, EventArgs e) =>
            Category.ShortcutList[SelectedShortcut.Position].Arguments = pnlArgumentTextbox.Text;

        // Clear textbox focus
        private void pnlArgumentTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                lblAddGroupIcon.Focus();

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        // Manage the checkbox allowing opening all shortcuts
        private void pnlAllowOpenAll_CheckedChanged(object sender, EventArgs e) =>
            Category.AllowOpenAll = pnlAllowOpenAll.Checked;

        private void cmdSelectDirectory_Click(object sender, EventArgs e)
        {
            var openFileDialog = new CommonOpenFileDialog
                                 {
                                     EnsurePathExists = true,
                                     IsFolderPicker = true,
                                     InitialDirectory =
                                         Category.ShortcutList[SelectedShortcut.Position].WorkingDirectory
                                 };

            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Category.ShortcutList[SelectedShortcut.Position].WorkingDirectory = openFileDialog.FileName;
                pnlWorkingDirectory.Text = openFileDialog.FileName;
            }

            Focus();
        }

        private void cmdSelectProgramPath_Click(object sender, EventArgs e)
        {
            var openFileDialog = new CommonOpenFileDialog
                                 {
                                     EnsurePathExists = true,
                                     IsFolderPicker = false,
                                     InitialDirectory =
                                         Category.ShortcutList[SelectedShortcut.Position].WorkingDirectory
                                 };

            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Category.ShortcutList[SelectedShortcut.Position].FilePath = openFileDialog.FileName;
                pnlProgramPath.Text = openFileDialog.FileName;
            }

            Focus();
        }

        // Wait 1 second for user to "finish typing"
        // Reset the timer everytime the text is changed
        private void pnlWorkingDirectory_TextChanged(object sender, EventArgs e)
        {
            validaitonTimerDirec.Stop();
            validaitonTimerDirec.Start();
        }

        private void pnlProgramPath_TextChanged(object sender, EventArgs e)
        {
            validationTimerPrgm.Stop();
            validationTimerPrgm.Start();
        }

        // Both timers actively validate the path after a certain amount of time
        private void validationTimer_Tick(object sender, EventArgs e)
        {
            validationTimerPrgm.Stop();

            if (_fileRegex.IsMatch(pnlProgramPath.Text))
            {
                if (File.Exists(pnlProgramPath.Text))
                {
                    Category.ShortcutList[SelectedShortcut.Position].FilePath = pnlProgramPath.Text;
                }
            }
        }

        private void validaitonTimerDirec_Tick(object sender, EventArgs e)
        {
            validaitonTimerDirec.Stop();

            if (_directoryRegex.IsMatch(pnlWorkingDirectory.Text))
            {
                if (Directory.Exists(pnlWorkingDirectory.Text))
                {
                    Category.ShortcutList[SelectedShortcut.Position].WorkingDirectory = pnlWorkingDirectory.Text;
                }
            }
        }

        private string GetProperDirectory(string file)
        {
            try
            {
                if (Path.GetExtension(file)
                        .ToLower()
                    == ".lnk")
                {
                    var extension = (IWshShortcut)new WshShell().CreateShortcut(file);

                    return Path.GetDirectoryName(extension.TargetPath);
                }

                return Path.GetDirectoryName(file);
            }
            catch (Exception)
            {
                return Paths.ExeString;
            }
        }

        private void frmGroup_MouseClick(object sender, MouseEventArgs e)
        {
            if (pnlDeleteConfo.Visible)
            {
                pnlDeleteConfo.Visible = false;
            }

            ResetSelection();
        }

        public static string ExpandEnvironment(string path)
        {
            if (path.Contains("%ProgramFiles%"))
            {
                path = path.Replace("%ProgramFiles%", "%ProgramW6432%");
            }

            return Environment.ExpandEnvironmentVariables(path);
        }

        private void frmGroup_SizeChanged(object sender, EventArgs e)
        {
            if (pnlAddShortcut.Bounds.IntersectsWith(pnlShortcuts.Bounds)
                || pnlColor.Bounds.IntersectsWith(pnlAddShortcut.Bounds))
            {
                MinimumSize = new Size(MinimumSize.Width, Height);
            }

            if (pnlDeleteConfo.Visible)
            {
                Point deleteButton = cmdDelete.FindForm()
                                              .PointToClient(cmdDelete.Parent.PointToScreen(cmdDelete.Location));

                pnlDeleteConfo.Location = new Point(deleteButton.X - 63, deleteButton.Y - 100);
            }

            pnlAddShortcut.Location = new Point(pnlAddShortcut.Location.X,
                                                pnlShortcuts.Bottom + (int)(20 * FrmClient.EDpi / 96));

            pnlAddShortcut.Left = (ClientSize.Width - pnlAddShortcut.Width) / 2;
        }

        private void OpenDeleteConformation(object sender, EventArgs e)
        {
            Point deleteButton = cmdDelete.FindForm()
                                          .PointToClient(cmdDelete.Parent.PointToScreen(cmdDelete.Location));

            pnlDeleteConfo.Location = new Point(deleteButton.X - 63, deleteButton.Y - 100);
            pnlDeleteConfo.Visible = true;
        }

        private Image ConstructIcons()
        {
            var iconImages = new List<Image>();

            if (pnlShortcuts.Controls.Count >= 4)
            {
                for (var i = 0; i < 4; i++)
                {
                    iconImages.Insert(0, ((UcProgramShortcut)pnlShortcuts.Controls[i]).Logo);
                }
            }
            else
            {
                foreach (UcProgramShortcut controlItem in pnlShortcuts.Controls)
                {
                    iconImages.Insert(0, controlItem.Logo);
                }
            }

            var image = new Bitmap(256, 256, PixelFormat.Format32bppArgb);

            using (var g = Graphics.FromImage(image))
            {
                g.Clear(Color.Transparent);

                var drawLocation = new PointF(0, 0);
                var counter = 0;

                foreach (Image iconImage in iconImages)
                {
                    if (counter == 2)
                    {
                        counter = 0;
                        drawLocation.Y += 128;
                        drawLocation.X = 0;
                    }

                    g.DrawImage(ImageFunctions.ResizeImage(iconImage, 128, 128), drawLocation);

                    drawLocation.X += 128;
                    counter += 1;
                }

                g.Dispose();
            }

            return image;
        }

        private void cmdAddGroupIcon_BackgroundImageChanged(object sender, EventArgs e) =>
            cmdAddGroupIcon.Tag = "Changed";

        public static Bitmap BitmapFromFile(string path)
        {
            var bytes = File.ReadAllBytes(path);
            var ms = new MemoryStream(bytes);
            var bp = (Bitmap)Image.FromStream(ms);

            return bp;
        }

        private void pnlShortcuts_ControlAdded(object sender, ControlEventArgs e)
        {
            for (var i = 0; i < pnlShortcuts.Controls.Count; i++)
            {
                ((UcProgramShortcut)pnlShortcuts.Controls[i]).ucProgramShortcut_ReadjustArrows(i);
            }
        }

        // Icon size
        private void IconSizeTopButton_Click(object sender, EventArgs e)
        {
            var iconSize = int.Parse(lblIconSize.Text);
            iconSize += 1;
            Category.IconSize = iconSize;
            lblIconSize.Text = iconSize.ToString();
            IconSizeBottomButton.Enabled = true;
            IconSizeBottomButton.BackgroundImage = Resources.NumDownWhite;

            if (iconSize > 255)
            {
                IconSizeTopButton.Enabled = false;
                IconSizeTopButton.BackgroundImage = Resources.NumUpGray;
            }
        }

        private void IconSizeBottomButton_Click(object sender, EventArgs e)
        {
            var iconSize = int.Parse(lblIconSize.Text);
            iconSize -= 1;
            Category.IconSize = iconSize;
            lblIconSize.Text = iconSize.ToString();
            IconSizeTopButton.Enabled = true;
            IconSizeTopButton.BackgroundImage = Resources.NumUpWhite;

            if (iconSize < 11)
            {
                IconSizeBottomButton.Enabled = false;
                IconSizeBottomButton.BackgroundImage = Resources.NumDownGray;
            }
        }

        // Icon separation
        private void IconSeparationTopButton_Click(object sender, EventArgs e)
        {
            var separation = int.Parse(lblIconSeparation.Text);
            separation += 1;
            Category.Separation = separation;
            lblIconSeparation.Text = separation.ToString();
            IconSeparationBottomButton.Enabled = true;
            IconSeparationBottomButton.BackgroundImage = Resources.NumDownWhite;

            if (separation > 49)
            {
                IconSeparationTopButton.Enabled = false;
                IconSeparationTopButton.BackgroundImage = Resources.NumUpGray;
            }
        }

        private void IconSeparationBottomButton_Click(object sender, EventArgs e)
        {
            var separation = int.Parse(lblIconSeparation.Text);
            separation -= 1;
            Category.Separation = separation;
            lblIconSeparation.Text = separation.ToString();
            IconSeparationTopButton.Enabled = true;
            IconSeparationTopButton.BackgroundImage = Resources.NumUpWhite;

            if (separation < 2)
            {
                IconSeparationBottomButton.Enabled = false;
                IconSeparationBottomButton.BackgroundImage = Resources.NumDownGray;
            }
        }

        private void cmdRightSettings_Click(object sender, EventArgs e)
        {
            if (groupSettingsTabControl.SelectedIndex < groupSettingsTabControl.TabCount)
            {
                groupSettingsTabControl.SelectedIndex += 1;
                cmdLeftSettings.Enabled = true;
                cmdLeftSettings.BackgroundImage = Resources.LeftArrow;

                if (groupSettingsTabControl.SelectedIndex == groupSettingsTabControl.TabCount - 1)
                {
                    cmdRightSettings.BackgroundImage = Resources.RightArrowGrey;
                    cmdRightSettings.Enabled = false;
                }
            }
        }

        private void cmdLeftSettings_Click(object sender, EventArgs e)
        {
            if (groupSettingsTabControl.SelectedIndex > 0)
            {
                groupSettingsTabControl.SelectedIndex -= 1;
                cmdRightSettings.Enabled = true;
                cmdRightSettings.BackgroundImage = Resources.RightArrow;

                if (groupSettingsTabControl.SelectedIndex == 0)
                {
                    cmdLeftSettings.BackgroundImage = Resources.LeftArrowGrey;
                    cmdLeftSettings.Enabled = false;
                }
            }
        }
    }
}