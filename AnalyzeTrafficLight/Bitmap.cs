using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyzeTrafficLight
{
    public class Bitmap
    {
        public Bitmap(int[] pargb, int width, int height)
        {
            argb = pargb;
            Width = width;
            Height = height;
        }

        public Bitmap(int width, int height)
        {
            argb = new int[width * height];
            Width = width;
            Height = height;
        }

        public int[] argb;
        public int Width;
        public int Height;

        public Color GetPixel(int x, int y)
        {
            int index = y * Width + x;
            int value = argb[index];
            byte r = (byte)((value & 0x00ff0000) >> 16);
            byte g = (byte)((value & 0x0000ff00) >> 8);
            byte b = (byte)(value & 0x000000ff);

            return new Color(r, g, b);
        }

        public void SetPixel(int x, int y, Color c)
        {
            int index = y * Width + x;
            int value = (c.R << 16) | (c.G << 8) | (c.B);
            argb[index] = value;
        }
    }
}