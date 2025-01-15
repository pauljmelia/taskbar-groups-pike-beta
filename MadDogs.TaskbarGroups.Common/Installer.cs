// Copyright © Mad-Dogs. All rights reserved.

namespace MadDogs.TaskbarGroups.Common
{
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;

    using IWshRuntimeLibrary;

    using File = System.IO.File;

    internal static class Installer
    {
        private const string BACKGROUND_PROCESS_NAME = "MadDogs.TaskbarGroups.Background";
        
        internal static bool JustWritten { get; set; }
        
        internal static void Install(byte[] backgroundApplication)
        {
            EnsureDirectory(Paths.AppDataPath);
            EnsureDirectory(Paths.ConfigPath);
            EnsureDirectory(Paths.OptimizationsPath);
            EnsureDirectory(Paths.ShortcutsPath);
            SetupMainClientShortcut();
            SetupBackgroundApplication(backgroundApplication);
        }

        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private static void SetupMainClientShortcut()
        {
            var shortcutLocation = Paths.MainClientShortcut;
            var shell = new WshShell();
            var shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);

            shortcut.Description = "Shortcut for Edit Group context menu feature";
            shortcut.TargetPath = Paths.ExeString;
            shortcut.Save();
        }

        private static void SetupBackgroundApplication(byte[] backgroundApplication)
        {
            if (!File.Exists(Paths.BackgroundApplicationPath))
            {
                File.WriteAllBytes(Paths.BackgroundApplicationPath, backgroundApplication);
                JustWritten = true;

                return;
            }

            using (var md5 = MD5.Create())
            {
                byte[] localHash;

                using (var memoryStream = new MemoryStream(backgroundApplication))
                {
                    localHash = md5.ComputeHash(memoryStream);
                }

                byte[] fileHash;

                using (FileStream fileStream = File.OpenRead(Paths.BackgroundApplicationPath))
                {
                    fileHash = md5.ComputeHash(fileStream);
                }

                if (fileHash.SequenceEqual(localHash))
                {
                    return;
                }
                
                CloseBackgroundApp();
                File.WriteAllBytes(Paths.BackgroundApplicationPath, backgroundApplication);
                JustWritten = true;
            }
        }

        internal static void CloseBackgroundApp()
        {
            Process[] processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(BACKGROUND_PROCESS_NAME));

            if (processes.Length == 0)
            {
                return;
            }

            Process process = processes[0];

            var p = new Process();

            p.StartInfo.FileName = Paths.BackgroundApplicationPath;
            p.StartInfo.Arguments = "exitApplicationModeReserved";
            p.Start();

            if (!process.WaitForExit(2000))
            {
                process.Kill();
            }
        }
    }
}