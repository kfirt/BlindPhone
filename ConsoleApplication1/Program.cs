using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;


namespace ConsoleApplication1
{
    class Program
    {


        static void printProps(Bitmap orig)
        {
            int redSum = 0;
            int greenSum = 0;
            int blueSum = 0;

            int redSum2 = 0;
            int greenSum2 = 0;
            int blueSum2 = 0;


            for (int i = 0; i < orig.Width; ++i)
            {
                for (int j = 0; j < orig.Height; ++j)
                {

                    Color c = orig.GetPixel(i, j);
                    redSum += c.R;
                    greenSum += c.G;
                    blueSum += c.B;

                    redSum2 += c.R * c.R;
                    greenSum2 += c.G * c.G;
                    blueSum2 += c.B * c.B;

                }
            }

            int size = orig.Width * orig.Height;
            double redAv = redSum / size;
            double greenAv = greenSum / size;
            double blueAv = blueSum / size;

            double redStd = Math.Sqrt(redSum2 / size - redAv * redAv);
            double greenStd = Math.Sqrt(greenSum2 / size - greenAv * greenAv);
            double blueStd = Math.Sqrt(blueSum2 / size - blueAv * blueAv);

            Console.WriteLine("Red Av = " + redAv + ", Red Std = " + redStd);
            Console.WriteLine("Green Av = " + greenAv + ", Green Std = " + greenStd);
            Console.WriteLine("Blue Av = " + blueAv + ", Blue Std = " + blueStd);
        }


        public class ColorRange
        {
            public int redMin;
            public int redMax;
            public int greenMin;
            public int greenMax;
            public int blueMin;
            public int blueMax;

            public bool inRange(Color c)
            {
                if (c.R < redMin || c.R > redMax) return false;
                if (c.G < greenMin || c.G > greenMax) return false;
                if (c.B < blueMin || c.B > blueMax) return false;
                return true;
            }

        }

        public class ColorObj
        {
            public int x;
            public int y;
            public int size;
            public Color c;
        }

        static void dilate(Bitmap orig, ref Bitmap res, Color paint, int x, int y, int w)
        {
            Color pixel = orig.GetPixel(x,y);
            ColorRange range = new ColorRange();
            range.redMin = Convert.ToInt32(Math.Max(pixel.R - 20, 0));
            range.redMax = Convert.ToInt32(Math.Min(pixel.R + 20, 255));
            range.greenMin = Convert.ToInt32(Math.Max(pixel.G - 20, 0));
            range.greenMax = Convert.ToInt32(Math.Min(pixel.G + 20, 255));
            range.blueMin = Convert.ToInt32(Math.Max(pixel.B - 20, 0));
            range.blueMax = Convert.ToInt32(Math.Min(pixel.B + 20, 255));

            int startX = Math.Max(0, x - w);
            int endX = Math.Min(orig.Width, x + w); 
            int startY = Math.Max(0, y - w);
            int endY = Math.Min(orig.Height, y + w);

            for (int i = startX; i < endX; ++i)
            {
                for (int j = startY; j < endY; ++j)
                {
                    if (range.inRange(orig.GetPixel(i,j)))
                    {
                        res.SetPixel(i, j, paint);
                    }
                }
            }
        }

        static Bitmap modify(Bitmap im, ColorRange redRange, ColorRange greenRange)
        {
            Color red = Color.FromName("Red");
            Color green = Color.FromName("Green");

             Bitmap res = new Bitmap(im.Width,im.Height);

            for (int i = 0; i < im.Width; ++i)
            {
                for (int j = 0; j < im.Height; ++j)
                {
                    if (redRange.inRange(im.GetPixel(i,j)))
                    {
                        res.SetPixel(i, j, red);
                        dilate(im, ref res, red, i, j, 10);
                    }
                    else if (greenRange.inRange(im.GetPixel(i,j)))
                    {
                        res.SetPixel(i, j, green);
                        dilate(im, ref res, green, i, j, 10);
                    }
                }
            }

            return res;
        }

        static int getObj(Bitmap im, int x, int y, bool[,] markMat, Color c)
        {
            if (y < 0 || y >= im.Height || x < 0 || x >= im.Width) 
                return 0;
            if (markMat[x, y]) 
                return 0;
            Color thisP = im.GetPixel(x, y);
            if (thisP.R != c.R || thisP.G != c.G || thisP.B != c.B)
                return 0;

            // new object
            markMat[x,y] = true;
            int size = 1;

            for (int i = x - 1; i <= x + 1; ++i)
            {
                for (int j = y - 1; j <= y + 1; ++j)
                {
                    size += getObj(im, i, j, markMat, c);
                }
            }

            return size;
        }

        static void detectObj(Bitmap im, List<ColorObj> objects) 
        {
            bool[,] markMat = new bool[im.Width,im.Height];

            for (int i = 0; i < im.Width; ++i)
            {
                for (int j = 0; j < im.Height; ++j)
                {
                    Color c = im.GetPixel(i, j);
                    if (c.B == 0 && c.R == 0 && c.G == 0)
                    {
                        continue;
                    }

                    // we are on a non white pixel
                    int size = getObj(im, i, j, markMat, c);
                    if (size == 0) continue;

                    ColorObj obj = new ColorObj();
                    obj.x = i;
                    obj.y = j;
                    obj.size = size;
                    obj.c = c;

                    objects.Add(obj);

                }
            }

        }

        static void purgeObj(List<ColorObj> objects, List<ColorObj> result)
        {
            foreach (var obj in objects)
            {
                if (obj.size < 200) continue;
                if (obj.size > 600) continue;
                result.Add(obj);
            }
        }

        static int decide(List<ColorObj> objects)
        {
            if (objects.Count == 0) return 0;
            ColorObj first = objects.ElementAt<ColorObj>(0);
            foreach (var obj in objects)
            {
                if (first.c != obj.c) return 0;
            }

            Color green = Color.FromName("Green");
            if (first.c.R == green.R && first.c.G == green.G && first.c.B == green.B)
                return 1;
            return 2;
        }

        static int analyzeImage(Bitmap orig)
        {
            ColorRange redRange = new ColorRange();
            redRange.redMin = 150;
            redRange.redMax = 256;
            redRange.greenMin = 0;
            redRange.greenMax = 80;
            redRange.blueMin = 0;
            redRange.blueMax = 80;

            ColorRange greenRange = new ColorRange();
            greenRange.redMin = 0;
            greenRange.redMax = 100;
            greenRange.greenMin = 200;
            greenRange.greenMax = 256;
            greenRange.blueMin = 200;
            greenRange.blueMax = 256;

            Bitmap im = modify(orig, redRange, greenRange);
            List<ColorObj> objects = new List<ColorObj>();
            detectObj(im, objects);
            
            List<ColorObj> result = new List<ColorObj>();
            purgeObj(objects, result);
            return decide(result);

            Form form = new Form();
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            form.Controls.Add(panel);
            form.Text = "Image Viewer";
            PictureBox pictureBox = new PictureBox();
            pictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
            pictureBox.Image = im;
            pictureBox.Dock = DockStyle.None;
            panel.Controls.Add(pictureBox);
            panel.AutoScroll = true;
            Application.Run(form);
        }


        static void Main(string[] args)
        {
            Bitmap orig = new Bitmap("C:/Users/Adi/Desktop/1.bmp");
            //printProps(orig);
            int ans = analyzeImage(orig);
            Console.WriteLine(ans);
        }
    }
}
