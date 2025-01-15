namespace MadDogs.TaskbarGroups.Common
{
    using System;
    using System.Reflection;

    using Model;

    internal static class Paths
    {
        internal const string BACKGROUND_APPLICATION_NAME = "MadDogs.TaskbarGroups.Background.exe";

        internal static string ExeString =>
            Assembly.GetExecutingAssembly()
                    .Location;

        internal static string ExeFolder => AppDomain.CurrentDomain.BaseDirectory;
        internal static string Path => System.IO.Path.GetDirectoryName(ExeString);

        internal static string ConfigPath =>
            !Settings.SettingInfo.PortableMode ? DefaultConfigPath : System.IO.Path.Combine(ExeFolder, "config");

        internal static string AppDataRelativePath { get; } = System.IO.Path.Combine("mad-dogs", "taskbar-groups");

        internal static string AppDataPath { get; } =
            System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                   AppDataRelativePath);

        internal static string MainClientShortcut { get; } =
            System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                   AppDataRelativePath,
                                   "Taskbar Group Editor.lnk");

        internal static string DefaultConfigPath { get; } =
            System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                   AppDataRelativePath,
                                   "config");

        internal static string DefaultShortcutsPath { get; } =
            System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                   AppDataRelativePath,
                                   "Shortcuts");

        internal static string DefaultBackgroundApplicationPath { get; } =
            System.IO.Path.Combine(AppDataPath, BACKGROUND_APPLICATION_NAME);

        internal static string BackgroundApplicationPath =>
            !Settings.SettingInfo.PortableMode
                ? DefaultBackgroundApplicationPath
                : System.IO.Path.Combine(ExeFolder, BACKGROUND_APPLICATION_NAME);

        internal static string OptimizationsPath { get; } = System.IO.Path.Combine(AppDataPath, "JITComp");

        internal static string ShortcutsPath
        {
            get;
        } = !Settings.SettingInfo.PortableMode ? DefaultShortcutsPath : System.IO.Path.Combine(ExeFolder, "Shortcuts");
    }
}