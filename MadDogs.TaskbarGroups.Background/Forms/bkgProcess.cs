namespace MadDogs.TaskbarGroups.Background.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    using Classes;

    using Common;
    using Common.Properties;

    using IWshRuntimeLibrary;

    using File = System.IO.File;

    public partial class BkgProcess : Form
    {
        internal static Dictionary<string, Category> LoadedCategories { get; } = new Dictionary<string, Category>();

        public static Color SystemColors = Color.FromArgb(31, 31, 31);

        [SuppressMessage("ReSharper", "MustUseReturnValue")]
        public BkgProcess()
        {
            BackColor = Color.LimeGreen;
            TransparencyKey = Color.LimeGreen;
            ShowInTaskbar = false;
            Opacity = 0;

            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
                                                       {
                                                           var resourceName = new AssemblyName(args.Name).Name + ".dll";

                                                           var resource = Array.Find(GetType()
                                                                   .Assembly.GetManifestResourceNames(),
                                                               element => element.EndsWith(resourceName));

                                                           using (Stream stream = Assembly.GetExecutingAssembly()
                                                                      .GetManifestResourceStream(resource))
                                                           {
                                                               if (stream == null)
                                                               {
                                                                   return default;
                                                               }

                                                               var assemblyData = new byte[stream.Length];
                                                               stream.Read(assemblyData, 0, assemblyData.Length);

                                                               return Assembly.Load(assemblyData);
                                                           }
                                                       };

            InitializeComponent();
            UpdateColor();
            Hide();

            var folders = Directory.GetDirectories(Paths.ConfigPath);

            foreach (var folderName in folders)
            {
                if (File.Exists(Path.Combine(folderName, "ObjectData.xml")))
                {
                    LoadedCategories.Add(new DirectoryInfo(folderName).Name, new Category(folderName));
                }
            }

            notifyIcon1.Visible = true;
            notifyIcon1.Icon = Resources.Icon;

            var trayContext = new ContextMenu();

            trayContext.MenuItems.Add("Exit",
                                      (s, e) =>
                                      {
                                          Application.Exit();
                                      });

            notifyIcon1.ContextMenu = trayContext;
        }

        public sealed override Color BackColor
        {
            get => base.BackColor;
            set => base.BackColor = value;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams @params = base.CreateParams;
                @params.ExStyle |= 0x80;
                @params.ExStyle |= 0x08000000;

                return @params;
            }
        }

        [DllImport("user32.dll")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static extern bool ShowWindow(IntPtr handle, int flags);

        [DllImport("dwmapi.dll", EntryPoint = "#127")]
        public static extern void DwmGetColorizationParameters(out DwmColorizationParams parameters);

        private static void UpdateColor()
        {
            DwmGetColorizationParameters(out DwmColorizationParams windowsColors);

            SystemColors = Color.FromArgb(255,
                                          (byte)(windowsColors.ColorizationColor >> 16),
                                          (byte)(windowsColors.ColorizationColor >> 8),
                                          (byte)windowsColors.ColorizationColor);
        }

        public static void ShowFormCat(string category, string arguments)
        {
            try
            {
                new FrmMain(LoadedCategories[category], arguments.Split(' ')).Show();
            }
            catch
            {
                // ignored
            }
        }

        private void notifyIcon1_Click(object sender, EventArgs e) => OpenEditor();

        public static void OpenEditor(string arguments = "")
        {
            if (File.Exists(Paths.MainClientShortcut) && Paths.MainClientShortcut != Paths.ExeString)
            {
                var shell = new WshShell();

                var mainClientShortcut = (IWshShortcut)shell.CreateShortcut(Paths.MainClientShortcut);

                if (File.Exists(mainClientShortcut.TargetPath))
                {
                    var p = new Process();
                    p.StartInfo.FileName = mainClientShortcut.TargetPath;

                    p.StartInfo.Arguments = arguments;
                    p.Start();
                }
                else
                {
                    MessageBox.Show("The editor has moved since the last time you've used it. Reopen it for it to relink the shortcut needed to use this feature!");
                }
            }
            else
            {
                MessageBox.Show("Please reopen your Taskbar Groups editor to set the location of itself so you can use this feature!");
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x320)
            {
                UpdateColor();
            }

            base.WndProc(ref m);
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public struct DwmColorizationParams
        {
            public uint ColorizationColor,
                        ColorizationAfterglow,
                        ColorizationColorBalance,
                        ColorizationAfterglowBalance,
                        ColorizationBlurBalance,
                        ColorizationGlassReflectionIntensity,
                        ColorizationOpaqueBlend;
        }
    }
}