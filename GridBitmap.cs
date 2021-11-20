using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ScatterPlotTool
{
    internal class GridBitmap
    {
        private readonly int mWidth, mHeight;
        private readonly byte[] mPixels;
        private readonly int mStride;

        public GridBitmap(int width, int height)
        {
            mWidth = width;
            mHeight = height;
            mPixels = new byte[width * height * 4];
            mStride = width * 4;
        }

        public GridBitmap(int width, int height, byte r, byte g, byte b, byte a = 255) : this(width, height)
        {
            var byteCount = width * height * 4;
            var index = 0;
            while (index < byteCount)
            {
                mPixels[index++] = b;
                mPixels[index++] = g;
                mPixels[index++] = r;
                mPixels[index++] = a;
            }
        }

        public void SetPixel(int x, int y, byte r, byte g, byte b, byte a = 255)
        {
            int index = y * mStride + x * 4;
            mPixels[index++] = b;
            mPixels[index++] = g;
            mPixels[index++] = r;
            mPixels[index++] = a;
        }

        public void SaveToFile(double dpiX, double dpiY, string filename)
        {
            var wbm = new WriteableBitmap(mWidth, mHeight, dpiX, dpiY, PixelFormats.Bgra32, null);
            wbm.WritePixels(new Int32Rect(0, 0, mWidth, mHeight), mPixels, mStride, 0);

            using var stream = new FileStream(filename, FileMode.Create);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(wbm));
            encoder.Save(stream);
        }

        public static void CreateRB(string filename)
        {
            var s = 501;
            var bm = new GridBitmap(s, s);
            for (var x = 0; x < s; x++)
            {
                for (var z = 0; z < s; z++)
                {
                    var xmod = x % 50;
                    var zmod = z % 50;

                    var xthick = xmod < 2 || xmod == 49 || (xmod % 10) == 0;
                    var zthick = zmod < 2 || zmod == 49 || (zmod % 10) == 0;

                    if (xthick || zthick)
                    {
                        bm.SetPixel(x, z, 0, 0, 0);
                    }
                    else
                    {
                        bm.SetPixel(
                            x,
                            z,
                            (byte)(x / 501.0 * 255.0),
                            (byte)((x + z) / 4.0 / 501.0 * 255.0),
                            (byte)(z / 501.0 * 255.0)
                        );
                    }
                }
            }
            bm.SaveToFile(96, 96, filename);
        }

        public static void CreateWhite(string filename)
        {
            var s = 501;
            var bm = new GridBitmap(s, s, 255, 255, 255);
            for (var x = 0; x < s; x++)
            {
                for (var z = 0; z < s; z++)
                {
                    var xmod = x % 50;
                    var zmod = z % 50;

                    var xthick = xmod < 2 || xmod == 49 || (xmod % 10) == 0;
                    var zthick = zmod < 2 || zmod == 49 || (zmod % 10) == 0;

                    if (xthick || zthick)
                    {
                        bm.SetPixel(x, z, 0, 0, 0);
                    }
                }
            }
            bm.SaveToFile(96, 96, filename);
        }
    }
}
