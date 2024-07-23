using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using VideoCrypt.Image.Data.Models;

namespace VideoCrypt.Image.Server.Utilities
{
    public static class ResizeImageUtility
    {
        public static byte[] ResizeImage(byte[] file, int width, int height, ImageModificationType type)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Invalid image file");

            if (width <= 0 || height <= 0)
                throw new ArgumentException("Width and height must be greater than zero");

            using var fileStreams = new MemoryStream(file);
            using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(fileStreams);            
            switch (type)
            {
                case ImageModificationType.Resize:
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(width, height),
                        Mode = ResizeMode.Max
                    }));
                    break;

                case ImageModificationType.Crop:
                    var cropRectangle = GetCropRectangle(image.Width, image.Height, width, height);
                    image.Mutate(x => x.Crop(cropRectangle));
                    image.Mutate(x => x.Resize(width, height));
                    break;

                case ImageModificationType.Stretch:
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(width, height),
                        Mode = ResizeMode.Stretch
                    }));
                    break;

                default:
                    throw new NotSupportedException($"Image modification type '{type}' is not supported");
            }

            using var outputMs = new MemoryStream();
            image.Save(outputMs, image.Metadata.DecodedImageFormat);
            return outputMs.ToArray();
        }

        private static Rectangle GetCropRectangle(int originalWidth, int originalHeight, int targetWidth, int targetHeight)
        {
            var sourceX = (originalWidth - targetWidth) / 2;
            var sourceY = (originalHeight - targetHeight) / 2;

            if (sourceX < 0) sourceX = 0;
            if (sourceY < 0) sourceY = 0;

            return new Rectangle(sourceX, sourceY, targetWidth, targetHeight);
        }
    }
}
