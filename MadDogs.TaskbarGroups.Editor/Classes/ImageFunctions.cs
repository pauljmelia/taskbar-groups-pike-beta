namespace MadDogs.TaskbarGroups.Editor.Classes
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.IO;

    public static class ImageFunctions
    {
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public static Icon IconFromImage(Image img)
        {
            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);
            bw.Write((short)0);
            bw.Write((short)1);
            bw.Write((short)1);
            var w = img.Width;

            if (w >= 256)
            {
                w = 0;
            }

            bw.Write((byte)w);
            var h = img.Height;

            if (h >= 256)
            {
                h = 0;
            }

            bw.Write((byte)h);
            bw.Write((byte)0);
            bw.Write((byte)0);
            bw.Write((short)0);
            bw.Write((short)0);
            var sizeHere = ms.Position;
            bw.Write(0);
            var start = (int)ms.Position + 4;
            bw.Write(start);
            img.Save(ms, ImageFormat.Png);
            var imageSize = (int)ms.Position - start;
            ms.Seek(sizeHere, SeekOrigin.Begin);
            bw.Write(imageSize);
            ms.Seek(0, SeekOrigin.Begin);
            return new Icon(ms);
        }

        public static Color FromString(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("name");
            }

            return Enum.TryParse(name, out KnownColor knownColor) ? Color.FromKnownColor(knownColor) : ColorTranslator.FromHtml(name);
        }
    }
}