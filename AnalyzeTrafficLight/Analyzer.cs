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


        static void dilate(Bitmap orig, ref Bitmap res, Color paint, int x, int y, int w, byte[,] idMat, byte currId)
        {
            Color pixel = orig.GetPixel(x, y);
            ColorRange range = new ColorRange();
            range.redMin = (int) (Math.Max(pixel.R - 20, 0));
            range.redMax = (int) (Math.Min(pixel.R + 20, 255));
			range.greenMin = (int) (Math.Max(pixel.G - 20, 0));
            range.greenMax = (int) (Math.Min(pixel.G + 20, 255));
            range.blueMin = (int) (Math.Max(pixel.B - 20, 0));
            range.blueMax = (int) (Math.Min(pixel.B + 20, 255));

            int startX = Math.Max(0, x - w);
            int endX = Math.Min(orig.Width, x + w);
            int startY = Math.Max(0, y - w);
            int endY = Math.Min(orig.Height, y + w);

            for (int i = startX; i < endX; ++i)
            {
                for (int j = startY; j < endY; ++j)
                {
                    if (idMat[i, j] != 0) continue;
                    if (range.inRange(orig.GetPixel(i, j)))
                    {
                        res.SetPixel(i, j, paint);
                        idMat[i, j] = currId;
                    }
                }
            }
        }

        static void setId(Bitmap im, byte[,] idMat, ref byte currId, int x, int y)
        {
            if (idMat[x-1,y] != 0)
            {
                idMat[x,y] = idMat[x-1,y];
                return;
            }
            if (idMat[x-1,y-1] != 0)
            {
                idMat[x,y] = idMat[x-1,y-1];
                return;
            }
            if (idMat[x,y-1] != 0)
            {
                idMat[x,y] = idMat[x,y-1];
                return;
            }
            if (idMat[x+1, y-1] != 0)
            {
                idMat[x, y] = idMat[x+1,y-1];
                return;
            }
            ++currId;
            idMat[x, y] = currId;
        }

        static Bitmap modify(Bitmap im, ColorRange redRange, ColorRange greenRange, byte[,] idMat, ref byte currId)
        {
            Bitmap res = new Bitmap(im.Width, im.Height);

            for (int i = 1; i < im.Width-1; ++i)
            {
                for (int j = 1; j < im.Height-1; ++j)
                {
                    if (redRange.inRange(im.GetPixel(i, j)))
                    //if (im.GetPixel(i, j).isRed())
                    {
                        res.SetPixel(i, j, Color.red);
                        setId(im, idMat, ref currId, i, j);
                        dilate(im, ref res, Color.red, i, j, 10, idMat, currId);
                    }
                    else if (greenRange.inRange(im.GetPixel(i, j)))
                    //else if (im.GetPixel(i, j).isGreenLight())
                    {
                        res.SetPixel(i, j, Color.green);
                        setId(im, idMat, ref currId, i, j);
                        dilate(im, ref res, Color.green, i, j, 10, idMat, currId);
                    }
                }
            }

            return res;
        }

        static int getObj(Bitmap im, int x, int y, bool[,] markMat, int depth, Color c,
            BoundingBox bBox, ColorStat cSum, ColorStat cSum2, ObjPoint coordSum)
        {
            if (depth > 3000) return 1;
            if (y < 0 || y >= im.Height || x < 0 || x >= im.Width)
                return 0;
            if (markMat[x, y])
                return 0;
            Color thisP = im.GetPixel(x, y);
            if (thisP.R != c.R || thisP.G != c.G || thisP.B != c.B)
                return 0;

            // new object
            ++depth;
            markMat[x, y] = true;
            int size = 1;

            // set bounding box
            if (x < bBox.topLeft.x) bBox.topLeft.x = x;
            if (x > bBox.bottomRight.x) bBox.bottomRight.x = x;
            if (y < bBox.topLeft.y) bBox.topLeft.y = y;
            if (y > bBox.bottomRight.y) bBox.bottomRight.y = y;

            // set sums
            cSum.R += thisP.R;
            cSum.G += thisP.G;
            cSum.B += thisP.B;

            cSum2.R += thisP.R * thisP.R;
            cSum2.G += thisP.G * thisP.G;
            cSum2.B += thisP.B * thisP.B;

            coordSum.x += x;
            coordSum.y += y;

            for (int i = x - 1; i <= x + 1; ++i)
            {
                for (int j = y - 1; j <= y + 1; ++j)
                {
                    size += getObj(im, i, j, markMat, depth, c, bBox, cSum, cSum2, coordSum);
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
                    bBox.topLeft.x = 10000;
                    bBox.topLeft.y = 10000;
                    bBox.bottomRight.x = 0;
                    bBox.bottomRight.y = 0;

                    ColorStat cSum = new ColorStat();
                    ColorStat cSum2 = new ColorStat();

                    ObjPoint coordSum = new ObjPoint();
                    int depth = 0;

                    int size = getObj(im, i, j, markMat, depth, c, bBox, cSum, cSum2, coordSum);
                    if (size == 0) continue;

                    AnalyzedObject obj = new AnalyzedObject();
					obj.decision = true;

                    obj.leftTop.x = i;
                    obj.leftTop.y = j;
                    obj.size = size;
                    obj.color = c;
                    obj.bBox = bBox;

                    cSum.R /= size;
                    cSum.G /= size;
                    cSum.B /= size;

                    cSum2.R = Math.Sqrt(cSum2.R / size - cSum.R * cSum.R);
                    cSum2.G = Math.Sqrt(cSum2.G / size - cSum.G * cSum.G);
                    cSum2.B = Math.Sqrt(cSum2.B / size - cSum.B * cSum.B);

                    obj.colorAv = cSum;
                    obj.colorStdev = cSum2;

                    coordSum.x = (int)(coordSum.x / size);
                    coordSum.y = (int)(coordSum.y / size);
                    obj.centroid = coordSum;

                    objects.Add(obj);

                }
            }

        }

		static void sizeFilter(List<AnalyzedObject> objects, Bitmap origImage)
        {
			double size = origImage.Width * origImage.Height;
            int min = (int)((1.0 / 35520.0) * size);
            int max = 7000; // (int)((1.0 / 11840.0) * size);
            
            foreach (var obj in objects)
            {
				if (obj.size < min)
					obj.decision = false;
                if (obj.size > max)
					obj.decision = false;
			}
        }

		static void blackBoxFilter(List<AnalyzedObject> objects, Bitmap origImage)
		{
			foreach (var obj in objects)
			{
				
				if (obj.decision == false)
					continue;

                int w = 10;
				int startX = Math.Max(0, obj.bBox.topLeft.x - w);
				int endX = Math.Min(origImage.Width, obj.bBox.bottomRight.x + w);
				int startY = Math.Max(0, obj.bBox.topLeft.y - w);
				int endY = Math.Min(origImage.Height, obj.bBox.bottomRight.y + w);
				int n = 0;
				int sr = 0, sg = 0, sb = 0;

				for (int x = startX; x <= endX; x++)
				{
					Color c = origImage.GetPixel(x, startY);
					sr += c.R;
					sg += c.G;
					sb += c.B;
					n++;
				}
				for (int x = startX; x <= endX; x++)
				{
					Color c = origImage.GetPixel(x, endY);
					sr += c.R;
					sg += c.G;
					sb += c.B;
					n++;
				}
				for (int y = startY; y <= endY; y++)
				{
    				Color c = origImage.GetPixel(startX, y);
					sr += c.R;
					sg += c.G;
					sb += c.B;
	    			n++;
				}
				for (int y = startY; y <= endY; y++)
				{
					Color c = origImage.GetPixel(endX, y);
					sr += c.R;
					sg += c.G;
					sb += c.B;
					n++;
				}

				sr /= n;
				sg /= n;
				sb /= n;

				if (sr > 80 || sg > 80 || sb > 80)
					obj.decision = false;
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

        public void createIdLookup(byte[,] idMat, int Width, int Height, byte[] lookup, ref byte currId)
        {
            bool[,] groupConn = new bool[currId + 1, currId + 1];

            for (int i = 1; i < Width-1; ++i)
            {
                for (int j = 1; j < Height-1; ++j)
                {
                    if (idMat[i, j] == 0) continue;

                    if (idMat[i + 1, j] != 0)
                    {
                        groupConn[idMat[i, j], idMat[i + 1, j]] = true;
                        groupConn[idMat[i+1, j], idMat[i, j]] = true;
                    }
                    if (idMat[i + 1, j+1] != 0)
                    {
                        groupConn[idMat[i, j], idMat[i + 1, j+1]] = true;
                        groupConn[idMat[i + 1, j+1], idMat[i, j]] = true;
                    }
                    if (idMat[i, j+1] != 0)
                    {
                        groupConn[idMat[i, j], idMat[i, j+1]] = true;
                        groupConn[idMat[i, j+1], idMat[i, j]] = true;
                    }
                    if (idMat[i-1, j + 1] != 0)
                    {
                        groupConn[idMat[i, j], idMat[i-1, j + 1]] = true;
                        groupConn[idMat[i-1, j + 1], idMat[i, j]] = true;
                    }
                }
            }

            byte newId = 0;
            for (int i = 1; i <= currId; ++i)
            {
                if (lookup[i] == 0)
                {
                    ++newId;
                    lookup[i] = newId;
                }
                for (int j = 1; j <= currId; ++j)
                {
                    if (i == j) continue;
                    if (groupConn[i, j] == false) continue;
                    lookup[j] = lookup[i];
                }
            }
            currId = newId;
        }

        public void fixIds(byte[,] idMat, int Width, int Height, byte[] lookup)
        {
            for (int i = 0; i < Width; ++i)
            {
                for (int j = 0; j < Height; ++j)
                {
                    if (idMat[i, j] == 0) continue;
                    idMat[i, j] = lookup[idMat[i, j]];
                }
            }
        }

        public AnalyzedObject createInitObj()
        {
            BoundingBox bBox = new BoundingBox();
            bBox.topLeft.x = 10000;
            bBox.topLeft.y = 10000;
            bBox.bottomRight.x = 0;
            bBox.bottomRight.y = 0;
            
            AnalyzedObject obj = new AnalyzedObject();
            obj.decision = true;

            //obj.leftTop.x = i;
            //obj.leftTop.y = j;
            //obj.size = size;
            //obj.color = c;
            obj.bBox = bBox;

            return obj;
        }

        public void findObjects(Bitmap im, byte[,] idMat, AnalyzedObject[] tmpObj)
        {
            for (int i = 0; i < im.Width; ++i)
            {
                for (int j = 0; j < im.Height; ++j)
                {
                    if (idMat[i, j] == 0) continue;
                    byte index = idMat[i, j];
                    if (tmpObj[index] == null)
                    {
                        tmpObj[index] = createInitObj();
                        tmpObj[index].leftTop.x = i;
                        tmpObj[index].leftTop.y = j;
                        tmpObj[index].color = im.GetPixel(i, j);
                    }

                    // set bounding box
                    if (i < tmpObj[index].bBox.topLeft.x) tmpObj[index].bBox.topLeft.x = i;
                    if (i > tmpObj[index].bBox.bottomRight.x) tmpObj[index].bBox.bottomRight.x = i;
                    if (j < tmpObj[index].bBox.topLeft.y) tmpObj[index].bBox.topLeft.y = j;
                    if (j > tmpObj[index].bBox.bottomRight.y) tmpObj[index].bBox.bottomRight.y = j;

                    ++(tmpObj[index].size);
                }
            }
        }

        public List<AnalyzedObject> analyzeImage(int[] argb, int width, int height)
        {
            Bitmap bit = new Bitmap(argb, width, height);
            return analyzeImage(bit); 
        }

        public List<AnalyzedObject> analyzeImage(Bitmap origImage)
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
            greenRange.redMax = 50;
            greenRange.greenMin = 90;
            greenRange.greenMax = 130;
            greenRange.blueMin = 0;
            greenRange.blueMax = 60;

            byte[,] idMat = new byte[origImage.Width, origImage.Height];
            byte currId = 0;
            Bitmap segImage = modify(origImage, redRange, greenRange, idMat, ref currId);

            byte[] lookup = new byte[currId + 1];
            createIdLookup(idMat, segImage.Width, segImage.Height, lookup, ref currId);
            fixIds(idMat, segImage.Width, segImage.Height, lookup);

            List<AnalyzedObject> objects = new List<AnalyzedObject>();
            AnalyzedObject[] tmpObj = new AnalyzedObject[currId + 1];
            findObjects(segImage, idMat, tmpObj);

            for (int i = 1; i <= currId; ++i)
            {
                if (tmpObj[i].size < 40) continue;
                objects.Add(tmpObj[i]);
            }

            //detectObj(segImage, objects);
            sizeFilter(objects, origImage);
			blackBoxFilter(objects, origImage);
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
