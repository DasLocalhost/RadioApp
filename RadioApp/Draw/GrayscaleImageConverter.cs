using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace RadioApp.Draw
{
    public class GrayscaleImageConverter : IValueConverter
    {
        public static float[][] _defaultColorMatrix = {
                                    new float[]{1,0,0,0,0},
                                    new float[]{0,1,0,0,0},
                                    new float[]{0,0,1,0,0},
                                    new float[]{0,0,0,1,0},
                                    new float[]{0,0,0,0,1},
                                };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var urlSource = value as string;
            if (urlSource == null || string.IsNullOrEmpty(urlSource))
                return value;

            try
            {
                Bitmap bitmap;

                using (var client = new HttpClient())
                {
                    var resp = client.Send(new HttpRequestMessage(HttpMethod.Get, urlSource));

                    var respStream = resp.Content.ReadAsStream();

                    bitmap = new Bitmap(respStream);
                    bitmap.Save("test.bmp");
                }

                var colorMatrix = new ColorMatrix(_defaultColorMatrix);
                colorMatrix.Matrix00 = colorMatrix.Matrix01 = colorMatrix.Matrix02 = 0.299F;
                colorMatrix.Matrix10 = colorMatrix.Matrix11 = colorMatrix.Matrix12 = 0.587F;
                colorMatrix.Matrix20 = colorMatrix.Matrix21 = colorMatrix.Matrix22 = 0.114F;

                using (var g = Graphics.FromImage(bitmap))
                using (var imgAttributes = new ImageAttributes())
                {
                    imgAttributes.SetColorMatrix(colorMatrix);
                    var rect = new Rectangle(System.Drawing.Point.Empty, bitmap.Size);
                    g.DrawImage(bitmap, rect, 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, imgAttributes);
                }

                bitmap.Save("test2.bmp");

                return Convert(bitmap);
            }
            catch (Exception ex) { }
            
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private Bitmap GetBitmap(BitmapSource source)
        {
            Bitmap bmp = new Bitmap(source.PixelWidth, source.PixelHeight, PixelFormat.Format32bppPArgb);
            BitmapData data = bmp.LockBits(new Rectangle(System.Drawing.Point.Empty, bmp.Size),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppPArgb);
            source.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
            bmp.UnlockBits(data);
            return bmp;
        }

        private BitmapImage Convert(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            ((System.Drawing.Bitmap)src).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }
    }
}
