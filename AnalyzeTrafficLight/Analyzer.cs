﻿using System;
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
			int delta = 30;
            range.redMin = (int) (Math.Max(pixel.R - delta, 0));
			range.redMax = (int)(Math.Min(pixel.R + delta, 255));
			range.greenMin = (int)(Math.Max(pixel.G - delta, 0));
			range.greenMax = (int)(Math.Min(pixel.G + delta, 255));
			range.blueMin = (int)(Math.Max(pixel.B - delta, 0));
			range.blueMax = (int)(Math.Min(pixel.B + delta, 255));

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
                    //if (im.GetPixel(i, j).isRed())
                    {
                        res.SetPixel(i, j, Color.red);
                        dilate(im, ref res, Color.red, i, j, 10);
                    }
                    else if (greenRange.inRange(im.GetPixel(i, j)))
                    //else if (im.GetPixel(i, j).isGreenLight())
                    {
                        res.SetPixel(i, j, Color.green);
                        dilate(im, ref res, Color.green, i, j, 10);
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

		//	This filter looks for black rectangle above/below the current object if it's green/red
		static void blackOtherFilter(List<AnalyzedObject> objects, Bitmap origImage)
		{

			foreach (var obj in objects)
			{

				if (obj.decision == false)
					continue;

				int startY=0, endY=0;
				int h = obj.bBox.bottomRight.y - obj.bBox.topLeft.y;
				int dist = (int) (h / 2);
				if(obj.color.R == Color.red.R)
				{
					startY = Math.Min(origImage.Height-1, obj.bBox.bottomRight.y + dist);
					endY = Math.Min(origImage.Height-1, startY + h);
				}
				else	//	green
				{
					endY = Math.Max(0, obj.bBox.topLeft.y - dist);
					startY = Math.Max(0, endY - h);
				}

				if(startY >= endY)
				{
					obj.decision = false;
					continue;
				}

				int sr = 0, sg = 0, sb = 0;
				int sr2 = 0, sg2 = 0, sb2 = 0;
				int n = (endY - startY + 1) * (obj.bBox.bottomRight.x - obj.bBox.topLeft.x + 1);
				for (int x = obj.bBox.topLeft.x; x <= obj.bBox.bottomRight.x; x++)
				{
					for (int y = startY; y <= endY; y++)
					{
						Color c = origImage.GetPixel(x, y);
						sr += c.R;
						sg += c.G;
						sb += c.B;
						sr2 += c.R * c.R;
						sg2 += c.G * c.G;
						sb2 += c.B * c.B;
					}
				}

				sr /= n;	//	average
				sg /= n;
				sb /= n;
				double stdr = Math.Sqrt(sr2 / n - sr * sr);		//	stdv
				double stdg = Math.Sqrt(sg2 / n - sg * sg);
				double stdb = Math.Sqrt(sb2 / n - sb * sb);

				if (sr > 20 || sg > 20 || sb > 20 || stdr > 30 || stdg > 30 || stdb > 30)
					obj.decision = false;
			}
		}

		public static int findObj(List<AnalyzedObject> objects, int x, int y)
		{
			int id = -1;
			foreach (var obj in objects)
			{
				id++;
				if(x >= obj.bBox.topLeft.x && x <= obj.bBox.bottomRight.x && y >= obj.bBox.topLeft.y && y <= obj.bBox.bottomRight.y)
					return id;
			}

			return -1;
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

        public List<AnalyzedObject> analyzeImage(int[] argb, int width, int height) //, byte[] ycbcr, int ycbcrBufferSize)
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
            greenRange.greenMin = 120;
            greenRange.greenMax = 256;
            greenRange.blueMin = 50;
            greenRange.blueMax = 180;

            Bitmap segImage = modify(origImage, redRange, greenRange);

            List<AnalyzedObject> objects = new List<AnalyzedObject>();
			detectObj(segImage, objects);
            //sizeFilter(objects, origImage);
			blackOtherFilter(objects, origImage);
			//blackBoxFilter(objects, origImage);
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
