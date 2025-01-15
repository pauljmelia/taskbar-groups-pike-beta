namespace MadDogs.TaskbarGroups.Background.User_Controls
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Windows.Forms;

    using Classes;

    using Common;
    using Common.Model;

    using Forms;

    public partial class UcShortcut : UserControl
    {
        public UcShortcut() => InitializeComponent();

        public ProgramShortcut Psc { get; set; }
        public FrmMain MotherForm { get; set; }
        public Image BkgImage { get; set; }
        internal Category LoadedCategory { get; set; }

        private void ucShortcut_Load(object sender, EventArgs e)
        {
            Size = new Size(MotherForm.UcShortcutSize, MotherForm.UcShortcutSize);
            Show();
            BringToFront();
            BackColor = MotherForm.BackColor;
            picIcon.BackgroundImage = BkgImage;
            toolTip1.SetToolTip(picIcon, Psc.Name);
            toolTip1.SetToolTip(this, Psc.Name);
            picIcon.Location = new Point(MotherForm.UcShortcutIconLocation, MotherForm.UcShortcutIconLocation);
            picIcon.Size = new Size(MotherForm.UcShortcutIconSize, MotherForm.UcShortcutIconSize);
        }

        public void ucShortcut_Click(object sender, EventArgs e)
        {
            if (Psc.IsWindowsApp)
            {
                var p = new Process
                        {
                            StartInfo = new ProcessStartInfo
                                        {
                                            UseShellExecute = true, FileName = $@"shell:appsFolder\{Psc.FilePath}"
                                        }
                        };

                p.Start();
            }
            else
            {
                if (Path.GetExtension(Psc.FilePath)
                        .ToLower()
                    == ".lnk"
                    && Psc.FilePath == Paths.ExeString)

                {
                    MotherForm.OpenFile(Psc.Arguments, Psc.FilePath, Paths.Path);
                }
                else
                {
                    MotherForm.OpenFile(Psc.Arguments, Psc.FilePath, Psc.WorkingDirectory);
                }
            }
        }

        public void ucShortcut_MouseEnter(object sender, EventArgs e) => BackColor = MotherForm.HoverColor;

        public void ucShortcut_MouseLeave(object sender, EventArgs e) => BackColor = Color.Transparent;
    }
}