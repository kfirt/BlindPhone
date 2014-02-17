using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyzeTrafficLight
{
    public class Analyzer
    {
        public Analyzer()
        {

        }

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


        static void dilate(Bitmap orig, ref Bitmap res, Color paint, int x, int y, int w)
        {
            Color pixel = orig.GetPixel(x, y);
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
                    if (range.inRange(orig.GetPixel(i, j)))
                    {
                        res.SetPixel(i, j, paint);
                    }
                }
            }
        }

        static Bitmap modify(Bitmap im, ColorRange redRange, ColorRange greenRange)
        {
            Bitmap res = new Bitmap(im.Width, im.Height);

            for (int i = 0; i < im.Width; ++i)
            {
                for (int j = 0; j < im.Height; ++j)
                {
                    if (redRange.inRange(im.GetPixel(i, j)))
                    {
                        res.SetPixel(i, j, Color.red);
                        dilate(im, ref res, Color.red, i, j, 10);
                    }
                    else if (greenRange.inRange(im.GetPixel(i, j)))
                    {
                        res.SetPixel(i, j, Color.green);
                        dilate(im, ref res, Color.green, i, j, 10);
                    }
                }
            }

            return res;
        }

        static int getObj(Bitmap im, int x, int y, bool[,] markMat, Color c, BoundingBox bBox)
        {
            if (y < 0 || y >= im.Height || x < 0 || x >= im.Width)
                return 0;
            if (markMat[x, y])
                return 0;
            Color thisP = im.GetPixel(x, y);
            if (thisP.R != c.R || thisP.G != c.G || thisP.B != c.B)
                return 0;

            // new object
            markMat[x, y] = true;
            int size = 1;

            if (x < bBox.leftTop.x) bBox.leftTop.x = x;
            if (x > bBox.rightBottom.x) bBox.rightBottom.x = x;
            if (y < bBox.leftTop.y) bBox.leftTop.y = y;
            if (y > bBox.rightBottom.y) bBox.rightBottom.y = y;

            for (int i = x - 1; i <= x + 1; ++i)
            {
                for (int j = y - 1; j <= y + 1; ++j)
                {
                    size += getObj(im, i, j, markMat, c, bBox);
                }
            }

            return size;
        }

        static void detectObj(Bitmap im, List<AnalyzedObject> objects)
        {
            bool[,] markMat = new bool[im.Width, im.Height];

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
                    BoundingBox bBox = new BoundingBox();
                    bBox.leftTop.x = 10000;
                    bBox.leftTop.y = 10000;
                    bBox.rightBottom.x = 0;
                    bBox.rightBottom.y = 0;

                    int size = getObj(im, i, j, markMat, c, bBox);
                    if (size == 0) continue;

                    AnalyzedObject obj = new AnalyzedObject();
                    obj.leftTop.x = i;
                    obj.leftTop.y = j;
                    obj.size = size;
                    obj.color = c;
                    obj.bBox = bBox;

                    objects.Add(obj);

                }
            }

        }

        static void purgeObj(List<AnalyzedObject> objects)
        {
            foreach (var obj in objects)
            {
                if (obj.size < 200) continue;
                if (obj.size > 600) continue;
                obj.decision = true;
            }
        }

        static AnalyzedState decide(List<AnalyzedObject> objects)
        {
            if (objects.Count == 0) return AnalyzedState.Unknown;
            AnalyzedObject first = objects.ElementAt<AnalyzedObject>(0);
            foreach (var obj in objects)
            {
                if (first.color != obj.color) return 0;
            }

            if (first.color.R == Color.green.R && first.color.G == Color.green.G && first.color.B == Color.green.B)
                return AnalyzedState.Green;
            return AnalyzedState.Red;
        }

        public List<AnalyzedObject> analyzeImage(int[] argb, int width, int height)
        {
            Bitmap bit = new Bitmap(argb, width, height);
            return analyzeImage(bit); 
        }

        public List<AnalyzedObject> analyzeImage(Bitmap orig)
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
            List<AnalyzedObject> objects = new List<AnalyzedObject>();
            detectObj(im, objects);
            purgeObj(objects);
            //decide(result);

            return objects;
            
        }
        //public AnalyzedState process(int[] bitmap)
        //{
        //    return AnalyzedState.Red;
        //    //Array values = Enum.GetValues(typeof(AnalyzedState));
        //    //Random random = new Random();
        //    //return (AnalyzedState)values.GetValue(random.Next(values.Length));
        //}
    }
}
