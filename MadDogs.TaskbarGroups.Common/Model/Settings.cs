// Copyright © Mad-Dogs. All rights reserved.

namespace MadDogs.TaskbarGroups.Common.Model
{
    using System;
    using System.IO;
    using System.Windows.Forms;
    using System.Xml.Serialization;

    internal static class Settings
    {
        private static readonly string _appDataRelative = Path.Combine("mad-dogs", "taskbar-groups");

        static Settings()
        {
            DefaultSettingsPath = SettingsPath;

            if (File.Exists(SettingsPath)) { }
            else if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings.xml")))
            {
                SettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings.xml");
            }
            else
            {
                Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder
                                                           .ApplicationData),
                                                       _appDataRelative));

                SettingInfo = new Setting();
                WriteXml();

                return;
            }

            var reader = new XmlSerializer(typeof(Setting));

            using (var file = new StreamReader(SettingsPath))
            {
                SettingInfo = (Setting)reader.Deserialize(file);
                file.Close();
            }

            if (!SettingInfo.PortableMode
                || File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings.xml")))
            {
                return;
            }

            SettingInfo.PortableMode = false;
            WriteXml();
        }

        public static string SettingsPath { get; set; } =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                         _appDataRelative,
                         "Settings.xml");

        public static string DefaultSettingsPath { get; set; }
        public static Setting SettingInfo { get; set; }

        public static void WriteXml()
        {
            try
            {
                var writer = new XmlSerializer(typeof(Setting));

                using (FileStream file = File.Create(SettingsPath))
                {
                    writer.Serialize(file, SettingInfo);
                    file.Close();
                }
            }
            catch (IOException)
            {
                MessageBox.Show("Settings.xml may be open in another file.", "Exit", MessageBoxButtons.OK);

                Environment.Exit(4);
            }
        }
    }
}