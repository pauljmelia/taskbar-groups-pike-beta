namespace MadDogs.TaskbarGroups.Editor
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    using Classes;

    using Forms;

    internal static class EditorClient
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        public static string[] Arguments = Environment.GetCommandLineArgs();

        // Define functions to set AppUserModelID
        [DllImport("shell32.dll", SetLastError = true)]
        private static extern void SetCurrentProcessExplicitAppUserModelID(
            [MarshalAs(UnmanagedType.LPWStr)] string appId);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool SetProcessDpiAwarenessContext(int dpiFlag);

        [DllImport("SHCore.dll", SetLastError = true)]
        internal static extern bool SetProcessDpiAwareness(ProcessDpiAwareness awareness);

        [DllImport("user32.dll")]
        internal static extern bool SetProcessDPIAware();

        [STAThread]
        private static void Main()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
                                                       {
                                                           var resourceName = new AssemblyName(args.Name).Name + ".dll";

                                                           var resource = Array.Find(Assembly.GetExecutingAssembly()
                                                                   .GetManifestResourceNames(),
                                                               element => element.EndsWith(resourceName));

                                                           using (Stream stream = Assembly.GetExecutingAssembly()
                                                                      .GetManifestResourceStream(resource))
                                                           {
                                                               var assemblyData = new byte[stream.Length];
                                                               stream.Read(assemblyData, 0, assemblyData.Length);

                                                               return Assembly.Load(assemblyData);
                                                           }
                                                       };

            if (Environment.OSVersion.Version >= new Version(6, 3, 0)) // win 8.1 added support for per monitor dpi
            {
                if (Environment.OSVersion.Version
                    >= new Version(10, 0, 15063)) // win 10 creators update added support for per monitor v2
                {
                    SetProcessDpiAwarenessContext((int)DpiAwarenessContext
                                                      .DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
                }
                else
                {
                    SetProcessDpiAwareness(ProcessDpiAwareness.PROCESS_PER_MONITOR_DPI_AWARE);
                }
            }
            else
            {
                SetProcessDPIAware();
            }

            // Use existing methods to obtain cursor already imported as to not import any extra functions
            // Pass as two variables instead of Point due to Point requiring System.Drawing

            // OLD - No longer handled here
            //int cursorX = Cursor.Position.X;
            //int cursorY = Cursor.Position.Y;

            // Creates folder for JIT compilation.
            try
            {
                ProfileOptimization.SetProfileRoot(Paths.OptimizationProfilePath);
            }
            catch (Exception ex) { }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                File.Create(Paths.Path + "\\directoryTestingDocument.txt")
                    .Close();

                File.Delete(Paths.Path + "\\directoryTestingDocument.txt");
            }
            catch
            {
                /*
                using (Process configTool = new Process())
                {
                    configTool.StartInfo.FileName = Paths.exeString;
                    configTool.StartInfo.Verb = "runas";
                    try
                    {
                        configTool.Start();
                    } catch
                    {
                        Process.GetCurrentProcess().Kill();
                    }
                }
                */
                MessageBox.Show("This program does not have access to this directory!");

                Process.GetCurrentProcess()
                       .Close();
            }

            /*
            if (arguments.Length > 1) // Checks for additional arguments; opens either main application or taskbar drawer application
            {
                if (Directory.Exists(Path.Combine(Paths.ConfigPath, arguments[1])))
                {
                    // Sets the AppUserModelID to mad-dogs.taskbarGroup.menu.groupName
                    // Distinguishes each shortcut process from one another to prevent them from stacking with the main application
                    SetCurrentProcessExplicitAppUserModelID("mad-dogs.taskbarGroup.menu." + arguments[1]);

                    System.Threading.Mutex mutexThread = new Mutex(true, "mad-dogs.taskbarGroup.menu." + arguments[1]);

                    try
                    {
                        if (!mutexThread.WaitOne(TimeSpan.Zero, false))
                        {
                            Application.Exit();
                        }
                    }
                    catch { }
                    //Application.Run(new frmMain(arguments[1], cursorX, cursorY, arguments.ToList(), mutexThread));
                } else if (arguments[1] == "editingGroupMode")
                {
                    // See comment above
                    SetCurrentProcessExplicitAppUserModelID("mad-dogs.taskbarGroup.main");
                    Application.Run(new frmClient(arguments.ToList()));
                } else
                {
                    Application.Exit();
                }
            } else
            {
                // See comment above
                SetCurrentProcessExplicitAppUserModelID("mad-dogs.taskbarGroup.main");
                Application.Run(new frmClient(arguments.ToList()));
            }
            */

            SetCurrentProcessExplicitAppUserModelID("mad-dogs.taskbarGroup.main");
            Application.Run(new FrmClient(Arguments.ToList()));
        }

        internal enum ProcessDpiAwareness
        {
            PROCESS_DPI_UNAWARE = 0,
            PROCESS_SYSTEM_DPI_AWARE = 1,
            PROCESS_PER_MONITOR_DPI_AWARE = 2
        }

        internal enum DpiAwarenessContext
        {
            DPI_AWARENESS_CONTEXT_UNAWARE = 16,
            DPI_AWARENESS_CONTEXT_SYSTEM_AWARE = 17,
            DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE = 18,
            DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = 34
        }
    }
}