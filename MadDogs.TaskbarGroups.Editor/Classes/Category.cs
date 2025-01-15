namespace MadDogs.TaskbarGroups.Editor.Classes
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;
    using System.Xml.Serialization;

    using Common.Model;

    using Forms;

    using Properties;

    using User_controls;

    internal class Category: CategoryBase
    {
        private static readonly int[] _iconSizes = { 16, 32, 64, 128, 256, 512 };
        public bool AllowOpenAll;
        public string ColorString = ColorTranslator.ToHtml(Color.FromArgb(31, 31, 31));
        public string HoverColor;
        public int IconSize = 30;
        public string Name = "";
        public double Opacity = 10;
        private string _path;
        public int Separation = 8;
        public List<ProgramShortcut> ShortcutList;

        public int Width;

        public Category(string inputPath)
        {
            _path = inputPath;
            var fullPath = Path.GetFullPath(Path.Combine(inputPath, "ObjectData.xml"));

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
        }

        public Category() { }

        public void CreateConfig(Image groupImage)
        {
            try
            {
                _path = Path.Combine(Paths.ConfigPath, Name);
                Directory.CreateDirectory(_path);

                WriteXml();

                Image img = ImageFunctions.ResizeImage(groupImage, 256, 256);
                img.Save(Path.Combine(_path, "GroupImage.png"));

                if (GetMimeType(groupImage) == "*.PNG")
                {
                    CreateMultiIcon(groupImage, Path.Combine(_path, "GroupIcon.ico"));
                }
                else
                {
                    using (var fs = new FileStream(Path.Combine(_path, "GroupIcon.ico"), FileMode.Create))
                    {
                        ImageFunctions.IconFromImage(img)
                                      .Save(fs);

                        fs.Close();
                    }
                }
                ShellLink.InstallShortcut(Paths.BackgroundApplication,
                                          "mad-dogs.taskbarGroup.menu." + Name,
                                          _path + " shortcut",
                                          _path,
                                          Path.Combine(_path, "GroupIcon.ico"),
                                          Path.Combine(_path, Name + ".lnk"),
                                          Name);
                CacheIcons();

                File.Move(Path.Combine(_path, Name + ".lnk"),
                          Path.Combine(Paths.ShortcutsPath,
                                       Regex.Replace(Name, @"(_)+", " ") + ".lnk"));
            }
            catch
            {
                // ignored
            }
            finally
            {
                CloseBackgroundApp();

                var backgroundProcess = new Process();
                backgroundProcess.StartInfo.FileName = Paths.BackgroundApplication;
                backgroundProcess.Start();

                var p = new Process();
                p.StartInfo.FileName = Paths.BackgroundApplication;
                p.StartInfo.Arguments = Name + " setGroupContextMenu";
                p.Start();
            }
        }

        private void WriteXml()
        {
            var writer = new XmlSerializer(typeof(Category));

            using (FileStream file = File.Create(Path.Combine(_path, "ObjectData.xml")))
            {
                writer.Serialize(file, this);
                file.Close();
            }
        }

        private static void CreateMultiIcon(Image iconImage, string filePath)
        {
            var diffList = from number in _iconSizes
                           select new { number, difference = Math.Abs(number - iconImage.Height) };

            var nearestSize = (from diffItem in diffList orderby diffItem.difference select diffItem).First()
                .number;

            var iconList = new List<Bitmap>();

            while (nearestSize != 16)
            {
                iconList.Add(ImageFunctions.ResizeImage(iconImage, nearestSize, nearestSize));
                nearestSize = (int)Math.Round((decimal)nearestSize / 2);
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                IconFactory.SavePngsAsIcon(iconList.ToArray(), stream);
            }
        }

        public Bitmap LoadIconImage()
        {
            var path = Path.Combine(Paths.ConfigPath, Name, "GroupImage.png");

            using (var ms = new MemoryStream(File.ReadAllBytes(path)))
            {
                return new Bitmap(ms);
            }
        }
        public void CacheIcons()
        {
            var path = Path.Combine(Paths.ConfigPath, Name);
            var iconPath = Path.Combine(path, "Icons");
            if (Directory.Exists(iconPath))
            {
                Directory.Delete(iconPath, true);
            }
            Directory.CreateDirectory(iconPath);

            for (var i = 0; i < ShortcutList.Count; i++)
            {
                var filePath = ShortcutList[i].FilePath;

                var programShortcutControl = Application.OpenForms["frmGroup"]
                                                        ?.Controls["pnlShortcuts"]
                                                        .Controls[i] as UcProgramShortcut;

                var savePath = Path.Combine(iconPath, GenerateMd5Hash(filePath + ShortcutList[i].Arguments) + ".png");

                programShortcutControl?.Logo.Save(savePath);
            }
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
                var path = Path.Combine(Paths.ConfigPath,
                                        Name,
                                        "Icons",
                                        GenerateMd5Hash(programPath + shortcutObject.Arguments) + ".png");

                Image finalImage;

                if (Path.GetExtension(programPath)
                        .ToLower()
                    == ".lnk")
                {
                    finalImage = FrmGroup.HandleLnkExt(programPath);
                }
                else if (Directory.Exists(programPath))
                {
                    finalImage = HandleFolder.GetFolderIcon(programPath)
                                             .ToBitmap();
                }
                else
                {
                    finalImage = Icon.ExtractAssociatedIcon(programPath)
                                     ?.ToBitmap();
                }
                finalImage?.Save(path);
                return finalImage;
            }
        }

        public string GenerateCachePath(ProgramShortcut ps) =>
            Path.Combine(Paths.ConfigPath, Name, "Icons", GenerateMd5Hash(ps.FilePath + ps.Arguments) + ".png");

        public static string GetMimeType(Image i)
        {
            Guid guid = i.RawFormat.Guid;

            foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageDecoders())
            {
                if (codec.FormatID == guid)
                {
                    return codec.FilenameExtension;
                }
            }

            return "image/unknown";
        }

        public Color CalculateHoverColor()
        {
            Color backColor = ImageFunctions.FromString(ColorString);

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

        private static string GenerateMd5Hash(string s)
        {
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.ASCII.GetBytes(s);
                var hashBytes = md5.ComputeHash(inputBytes);

                var sb = new StringBuilder();

                foreach (var t in hashBytes)
                {
                    sb.Append(t
                                  .ToString("X2"));
                }

                return sb.ToString();
            }
        }

        public static void CloseBackgroundApp(string path = "")
        {
            Process[] process = Process.GetProcessesByName(Path.GetFileNameWithoutExtension("Taskbar Groups Background"));

            if (process.Length == 0)
            {
                return;
            }

            Process bkg = process[0];

            var p = new Process();

            if (path == "")
            {
                path = Paths.BackgroundApplication;
            }

            p.StartInfo.FileName = path;
            p.StartInfo.Arguments = "exitApplicationModeReserved";
            p.Start();

            if (!bkg.WaitForExit(2000))
            {
                bkg.Kill();
            }
        }
    }
}