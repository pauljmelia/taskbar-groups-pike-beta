namespace MadDogs.TaskbarGroups.Editor.Classes
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;

    public class IconFactory
    {
        #region constants

        public const int MaxIconWidth = 256;
        public const int MaxIconHeight = 256;

        private const ushort HeaderReserved = 0;
        private const ushort HeaderIconType = 1;
        private const byte HeaderLength = 6;

        private const byte EntryReserved = 0;
        private const byte EntryLength = 16;

        private const byte PngColorsInPalette = 0;
        private const ushort PngColorPlanes = 1;

        #endregion

        #region methods

        public static void SavePngsAsIcon(IEnumerable<Bitmap> images, Stream stream)
        {
            if (images == null)
            {
                throw new ArgumentNullException(nameof(images));
            }

            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            Bitmap[] orderedImages = images.OrderBy(i => i.Width)
                                           .ThenBy(i => i.Height)
                                           .ToArray();

            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(HeaderReserved);
                writer.Write(HeaderIconType);
                writer.Write((ushort)orderedImages.Length);
                var buffers = new Dictionary<uint, byte[]>();
                uint lengthSum = 0;
                var baseOffset = (uint)(HeaderLength + (EntryLength * orderedImages.Length));

                foreach (Bitmap image in orderedImages)
                {
                    var buffer = CreateImageBuffer(image);
                    var offset = baseOffset + lengthSum;
                    writer.Write(GetIconWidth(image));
                    writer.Write(GetIconHeight(image));
                    writer.Write(PngColorsInPalette);
                    writer.Write(EntryReserved);
                    writer.Write(PngColorPlanes);
                    writer.Write((ushort)Image.GetPixelFormatSize(image.PixelFormat));
                    writer.Write((uint)buffer.Length);
                    writer.Write(offset);

                    lengthSum += (uint)buffer.Length;
                    buffers.Add(offset, buffer);
                }

                foreach (KeyValuePair<uint, byte[]> kvp in buffers)
                {
                    writer.BaseStream.Seek(kvp.Key, SeekOrigin.Begin);
                    writer.Write(kvp.Value);
                }
            }
        }

        private static byte GetIconHeight(Bitmap image)
        {
            if (image.Height == MaxIconHeight)
            {
                return 0;
            }

            return (byte)image.Height;
        }

        private static byte GetIconWidth(Bitmap image)
        {
            if (image.Width == MaxIconWidth)
            {
                return 0;
            }

            return (byte)image.Width;
        }

        private static byte[] CreateImageBuffer(Bitmap image)
        {
            using (var stream = new MemoryStream())
            {
                image.Save(stream, ImageFormat.Png);

                return stream.ToArray();
            }
        }

        #endregion
    }
}