namespace MadDogs.TaskbarGroups.Background.Classes
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using System.Xml.Serialization;

    using Common.Model;
    using Common.Properties;

    using Forms;

    internal class Category : CategoryBase
    {
        public Icon GroupIco;

        public List<Image> ProgramImages = new List<Image>();

        public Category() { }

        public Category(string loadCat)
        {
            var fullPath = Path.GetFullPath(Path.Combine(loadCat, "ObjectData.xml"));

            var reader = new XmlSerializer(typeof(Category));

            using (var file = new StreamReader(fullPath))
            {
                var category = (Category)reader.Deserialize(file);
                Name = category.Name;
                ShortcutList = category.ShortcutList;
                Width = category.Width;
                ColorString = category.ColorString;
                Opacity = category.Opacity;
                AllowOpenAll = category.AllowOpenAll;
                HoverColor = category.HoverColor;
                IconSize = category.IconSize;
                Separation = category.Separation;
            }

            foreach (ProgramShortcut psc in ShortcutList)
            {
                ProgramImages.Add(LoadImageCache(psc));
            }

            using (var ms = new MemoryStream(File.ReadAllBytes(Path.Combine(loadCat, "GroupIcon.ico"))))
            {
                GroupIco = new Icon(ms);
            }

            Color sysColor = BkgProcess.SystemColors;

            if (ColorString == "sys")
            {
                ColorString = "#" + sysColor.R.ToString("X2") + sysColor.G.ToString("X2") + sysColor.B.ToString("X2");
            }
        }

        public Color CalculateHoverColor()
        {
            Color backColor = FromString(ColorString);

            if ((backColor.R * 0.2126) + (backColor.G * 0.7152) + (backColor.B * 0.0722) > 127d)
            {
                var backColorR = backColor.R - 50 >= 0 ? backColor.R - 50 : 0;
                var backColorG = backColor.G - 50 >= 0 ? backColor.G - 50 : 0;
                var backColorB = backColor.B - 50 >= 0 ? backColor.B - 50 : 0;

                return Color.FromArgb(backColor.A, backColorR, backColorG, backColorB);
            }
            else
            {
                var backColorR = backColor.R + 50 <= 255 ? backColor.R + 50 : 255;
                var backColorG = backColor.G + 50 <= 255 ? backColor.G + 50 : 255;
                var backColorB = backColor.B + 50 <= 255 ? backColor.B + 50 : 255;

                return Color.FromArgb(backColor.A, backColorR, backColorG, backColorB);
            }
        }

        public static Color FromString(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("name");
            }

            return Enum.TryParse(name, out KnownColor knownColor)
                       ? Color.FromKnownColor(knownColor)
                       : ColorTranslator.FromHtml(name);
        }

        public Image LoadImageCache(ProgramShortcut shortcutObject)
        {
            var programPath = shortcutObject.FilePath;

            if (!File.Exists(programPath) && !Directory.Exists(programPath) && !shortcutObject.IsWindowsApp)
            {
                return Resources.Error;
            }

            try
            {
                var cacheImagePath = GenerateCachePath(shortcutObject);

                using (var ms = new MemoryStream(File.ReadAllBytes(cacheImagePath)))
                {
                    return Image.FromStream(ms);
                }
            }
            catch (Exception)
            {
                return Resources.Error;
            }
        }

        public string GenerateCachePath(ProgramShortcut shortcutObject) =>
            Path.Combine(Common.Paths.ConfigPath,
                         Name,
                         "Icons",
                         GenerateMd5Hash(shortcutObject.FilePath + shortcutObject.Arguments) + ".png");

        private static string GenerateMd5Hash(string s)
        {
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.ASCII.GetBytes(s);
                var hashBytes = md5.ComputeHash(inputBytes);

                var sb = new StringBuilder();

                foreach (var t in hashBytes)
                {
                    sb.Append(t.ToString("X2"));
                }

                return sb.ToString();
            }
        }
    }
}