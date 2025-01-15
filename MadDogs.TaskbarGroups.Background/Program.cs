namespace MadDogs.TaskbarGroups.Background
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    using Common;

    using Forms;

    using Microsoft.VisualBasic.ApplicationServices;

    internal class SingleInstanceApp : WindowsFormsApplicationBase
    {
        public SingleInstanceApp() => IsSingleInstance = true;

        // Define functions to set AppUserModelID
        [DllImport("shell32.dll", SetLastError = true)]
        private static extern void SetCurrentProcessExplicitAppUserModelID(
            [MarshalAs(UnmanagedType.LPWStr)] string appId);

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs e)
        {
            base.OnStartupNextInstance(e);

            var secondInstanceArguments = e.CommandLine.ToArray();

            if (secondInstanceArguments.Length
                <= 1) // Checks for additional arguments; opens either main application or taskbar drawer application
            {
                return;
            }

            if (BkgProcess.LoadedCategories.ContainsKey(secondInstanceArguments[1]))
            {
                var group = secondInstanceArguments[1];
                var argument = "";

                if (secondInstanceArguments.Length > 2)
                {
                    for (var i = 2; i < secondInstanceArguments.Length; i++)
                    {
                        argument += secondInstanceArguments[i]
                            .Trim();
                    }
                }

                SetCurrentProcessExplicitAppUserModelID("mad-dogs.taskbarGroup.menu." + secondInstanceArguments[1]);

                BkgProcess.ShowFormCat(group, argument);
            }
            else
            {
                switch (secondInstanceArguments[1])
                {
                    case "editingGroupMode":
                        {
                            if (BkgProcess.LoadedCategories.ContainsKey(secondInstanceArguments[2]))
                            {
                                BkgProcess.OpenEditor("editingGroupMode" + " " + secondInstanceArguments[2]);
                            }

                            break;
                        }
                    case "exitApplicationModeReserved":
                        Application.Exit();

                        break;
                }
            }
        }

        protected override void OnCreateMainForm()
        {
            var arguments = Environment.GetCommandLineArgs();

            base.OnCreateMainForm();

            MainForm = new BkgProcess();

            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly()
                                                                        .Location)
                                          ?? string.Empty);

            if (arguments.Length
                <= 1) // Checks for additional arguments; opens either main application or taskbar drawer application
            {
                return;
            }

            if (!BkgProcess.LoadedCategories.ContainsKey(arguments[1]) && arguments[1] != "editingGroupMode")
            {
                return;
            }

            var p = new Process();
            p.StartInfo.FileName = Paths.ExeString;

            var argument = "";

            if (arguments.Length > 2)
            {
                for (var i = 2; i < arguments.Length; i++)
                {
                    argument += arguments[i]
                        .Trim();
                }
            }

            p.StartInfo.Arguments = arguments[1] + " " + argument;
            p.Start();
        }
    }

    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            new SingleInstanceApp().Run(Environment.GetCommandLineArgs());
        }
    }
}