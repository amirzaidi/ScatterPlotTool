using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ScatterPlotTool.Image
{
    class Bitmap
    {
        private readonly WriteableBitmap mBitmap;
        private readonly int mBytesPerPixel, mStride;

        public Bitmap(int width, int height, PixelFormat format)
        {
            var dpiX = 96d;
            var dpiY = 96d;

            mBytesPerPixel = format.BitsPerPixel / 8;
            mStride = mBytesPerPixel * width;
            mBitmap = new WriteableBitmap(width, height, dpiX, dpiY, format, null);
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

            mBytesPerPixel = format.BitsPerPixel / 8;
            mStride = width * (format.BitsPerPixel / 8);
            mBitmap = new WriteableBitmap(width, height, dpiX, dpiY, format, null);

            // Copy the data.
            var pixels = new byte[height * mStride];
            source.CopyPixels(pixels, mStride, 0);
            mBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, mStride, 0);
        }

        public byte[] GetPixels(int x, int y, int width = 1, int height = 1)
        {
            var pixels = new byte[mBytesPerPixel * width * height];
            mBitmap.CopyPixels(new Int32Rect(x, y, width, height), pixels, mStride, 0);
            return pixels;
        }

        public void SetPixels(int x, int y, int width = 1, int height = 1, params byte[] pixels)
        {
            mBitmap.WritePixels(new Int32Rect(x, y, width, height), pixels, mStride, 0);
        }

        public BitmapSource GetImage() => mBitmap;

        public int GetWidth() => mBitmap.PixelWidth;

        public int GetHeight() => mBitmap.PixelHeight;
    }
}
