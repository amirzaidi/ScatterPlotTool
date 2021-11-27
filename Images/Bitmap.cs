using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ScatterPlotTool.Images
{
    class Bitmap
    {
        private const double DPI = 96.0;

        private readonly WriteableBitmap mBitmap;
        private readonly int mBytesPerPixel, mStride;

        // Optimalization.
        private byte[] mPrevGetPixelsBuf;

        public Bitmap(int width, int height, PixelFormat format)
        {
            mBytesPerPixel = format.BitsPerPixel / 8;
            mStride = mBytesPerPixel * width;
            mBitmap = new WriteableBitmap(width, height, DPI, DPI, format, null);
        }

        public Bitmap(string file)
        {
            // Load the bitmap from the image.
            var source = new BitmapImage(new Uri(file));

            // Create the same bitmap.
            var width = source.PixelWidth;
            var height = source.PixelHeight;
            var dpiX = source.DpiX;
            var dpiY = source.DpiY;
            var format = source.Format;

            // Ugly hack.
            dpiX = DPI;
            dpiY = DPI;

            mBytesPerPixel = format.BitsPerPixel / 8;
            mStride = width * mBytesPerPixel;
            mBitmap = new WriteableBitmap(width, height, dpiX, dpiY, format, null);

            // Copy the data.
            var pixels = new byte[height * mStride];
            source.CopyPixels(pixels, mStride, 0);
            mBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, mStride, 0);
        }

        public byte[] GetPixels(int x, int y, int width = 1, int height = 1)
        {
            var bufLen = mBytesPerPixel * width * height;
            if (mPrevGetPixelsBuf == null || mPrevGetPixelsBuf.Length != bufLen)
            {
                mPrevGetPixelsBuf = new byte[bufLen];
            }
            mBitmap.CopyPixels(new Int32Rect(x, y, width, height), mPrevGetPixelsBuf, width * mBytesPerPixel, 0);
            return mPrevGetPixelsBuf;
        }

        public void SetPixels(int x, int y, int width = 1, int height = 1, params byte[] pixels)
        {
            mBitmap.WritePixels(new Int32Rect(x, y, width, height), pixels, width * mBytesPerPixel, 0);
        }

        public void ApplyTo(Image image)
        {
            image.Source = mBitmap;
            image.Width = GetWidth();
            image.Height = GetHeight();
        }

        public BitmapSource GetImage() => mBitmap;

        public int GetWidth() => mBitmap.PixelWidth;

        public int GetHeight() => mBitmap.PixelHeight;
    }
}
