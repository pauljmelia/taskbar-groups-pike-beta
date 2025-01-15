namespace MadDogs.TaskbarGroups.Editor.Classes
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Xml;

    using Windows.ApplicationModel;
    using Windows.Management.Deployment;

    using Shell32;

    internal class HandleWindowsApp
    {
        public static Dictionary<string, string> FileDirectoryCache = new Dictionary<string, string>();

        private static readonly PackageManager _pkgManger = new PackageManager();

        public static Bitmap GetWindowsAppIcon(string file, bool alreadyAppId = false)
        {
            var microsoftAppName = !alreadyAppId ? GetLnkTarget(file)[0] : file;
            var subAppName = microsoftAppName.Split('!')[0];
            var appPath = FindWindowsAppsFolder(subAppName);
            var appManifest = new XmlDocument();
            appManifest.Load(Path.Combine(appPath, "AppxManifest.xml"));

            var appManifestNamespace = new XmlNamespaceManager(new NameTable());
            appManifestNamespace.AddNamespace("sm", "http://schemas.microsoft.com/appx/manifest/foundation/windows10");

            var logoLocation = appManifest.SelectSingleNode("/sm:Package/sm:Properties/sm:Logo", appManifestNamespace)
                                          ?.InnerText.Replace("\\", @"\");

            if (string.IsNullOrWhiteSpace(logoLocation))
            {
                return Icon.ExtractAssociatedIcon(file)
                           ?.ToBitmap();
            }

            var lastIndexOf = logoLocation.LastIndexOf(@"\", StringComparison.Ordinal);
            string logoLocationFullPath;

            string logoPng;

            if (lastIndexOf != -1)
            {
                logoPng = logoLocation.Substring(lastIndexOf + 1, logoLocation.LastIndexOf(@".", StringComparison.Ordinal) - lastIndexOf - 1);
                logoLocation = logoLocation.Substring(0, lastIndexOf);
                logoLocationFullPath = Path.GetFullPath(Path.Combine(appPath, logoLocation));
            }
            else
            {
                logoPng = logoLocation;
                logoLocationFullPath = Path.GetFullPath(appPath + "\\");
            }
            var logoDirectory = new DirectoryInfo(logoLocationFullPath);

            string[] keysToTest = { logoPng, "scale-200", "StoreLogo" };

            foreach (var key in keysToTest)
            {
                FileInfo[] filesInDir = GetLogoFolder(key, logoDirectory);

                if (filesInDir.Length != 0)
                {
                    return GetLogo(filesInDir.Last()
                                             .FullName,
                                   file);
                }
            }

            return Icon.ExtractAssociatedIcon(file)
                       ?.ToBitmap();

        }

        private static FileInfo[] GetLogoFolder(string key, DirectoryInfo logoDirectory)
        {
            FileInfo[] filesInDir = logoDirectory.GetFiles("*" + key + "*.*");

            return filesInDir;
        }

        private static Bitmap GetLogo(string logoPath, string defaultFile)
        {
            if (!File.Exists(logoPath))
            {
                return Icon.ExtractAssociatedIcon(defaultFile)
                           ?.ToBitmap();
            }

            using (var ms = new MemoryStream(File.ReadAllBytes(logoPath)))
            {
                return ImageFunctions.ResizeImage(Image.FromStream(ms), 64, 64);
            }

        }

        public static string[] GetLnkTarget(string lnkPath)
        {
            var shl = new Shell();
            lnkPath = Path.GetFullPath(lnkPath);
            Folder dir = shl.NameSpace(Path.GetDirectoryName(lnkPath));

            FolderItem itm = dir.Items()
                                .Item(Path.GetFileName(lnkPath));

            var lnk = (ShellLinkObject)itm.GetLink;
            const string LINK_ID = "";

            return new[] { lnk.Target.Path, LINK_ID };
        }

        public static string FindWindowsAppsFolder(string subAppName)
        {
            if (FileDirectoryCache.TryGetValue(subAppName, out var folder))
            {
                return folder;
            }

            try
            {
                IEnumerable<Package> packages = _pkgManger.FindPackagesForUser("", subAppName).ToList();

                var finalPath = packages.First()
                                        .InstalledLocation.Path;

                FileDirectoryCache[subAppName] = finalPath;

                return finalPath;
            }
            catch (UnauthorizedAccessException) { }

            return "";

        }

        public static string FindWindowsAppsName(string appName)
        {
            var subAppName = appName.Split('!')[0];
            var appPath = FindWindowsAppsFolder(subAppName);
            var appManifest = new XmlDocument();
            appManifest.Load(Path.Combine(appPath, "AppxManifest.xml"));

            var appManifestNamespace = new XmlNamespaceManager(new NameTable());
            appManifestNamespace.AddNamespace("sm", "http://schemas.microsoft.com/appx/manifest/foundation/windows10");
            appManifestNamespace.AddNamespace("uap", "http://schemas.microsoft.com/appx/manifest/uap/windows10");

            try
            {
                return appManifest.SelectSingleNode("/sm:Package/sm:Properties/sm:DisplayName", appManifestNamespace)
                                  ?.InnerText;
            }
            catch (Exception)
            {
                return appManifest.SelectSingleNode("/sm:Package/sm:Applications/sm:Application/uap:VisualElements",
                                                    appManifestNamespace)
                                  ?.Attributes?.GetNamedItem("DisplayName")
                                  .InnerText;
            }
        }
    }
}