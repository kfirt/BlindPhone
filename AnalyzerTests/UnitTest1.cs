using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using sdkCameraGrayscaleCS;
using System.IO;
using AnalyzeTrafficLight;
using System.Collections.Generic; 

namespace AnalyzerTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestReds16()
        {
            Assert.AreEqual(AnalyzedState.Red, runAnalyzer(@"C:\Pics\Red\WP_20140216_016.jpg"));

        }
        [TestMethod]
        public void TestReds17()
        {
            Assert.AreEqual(AnalyzedState.Red, runAnalyzer(@"C:\Pics\Red\WP_20140216_017.jpg"));

        }
        [TestMethod]
        public void TestReds18()
        {
            Assert.AreEqual(AnalyzedState.Red, runAnalyzer(@"C:\Pics\Red\WP_20140216_018.jpg"));

        }
        [TestMethod]
        public void TestReds40()
        {
            Assert.AreEqual(AnalyzedState.Red, runAnalyzer(@"C:\Pics\Red\WP_20140216_040.jpg"));

        }
        
        // green tests 
        [TestMethod]
        public void TestGreens11()
        {
            Assert.AreEqual(AnalyzedState.Green, runAnalyzer(@"C:\Pics\Green\WP_20140216_011.jpg"));
        }
        [TestMethod]
        public void TestGreens12()
        {
            Assert.AreEqual(AnalyzedState.Green, runAnalyzer(@"C:\Pics\Green\WP_20140216_012.jpg"));
        }
        [TestMethod]
        public void TestGreens13()
        {
            Assert.AreEqual(AnalyzedState.Green, runAnalyzer(@"C:\Pics\Green\WP_20140216_013.jpg"));
        }

       // unknown tests
        [TestMethod]
        public void TestUnknown1()
        {
            Assert.AreEqual(AnalyzedState.Unknown, runAnalyzer(@"C:\Pics\Unknown\WP_20140216_001.jpg"));
        }
        [TestMethod]
        public void TestUnknown2()
        {
            Assert.AreEqual(AnalyzedState.Unknown, runAnalyzer(@"C:\Pics\Unknown\WP_20140216_002.jpg"));
        }
        [TestMethod]
        public void TestUnknown3()
        {
            Assert.AreEqual(AnalyzedState.Unknown, runAnalyzer(@"C:\Pics\Unknown\WP_20140216_003.jpg"));
        }


        private AnalyzedState runAnalyzer(string imagePath)
        {
            Uri uri = new Uri(imagePath, UriKind.RelativeOrAbsolute);
            Analyzer analyzer = new Analyzer();
            
            System.Drawing.Bitmap image = new System.Drawing.Bitmap(imagePath);

            //// Loop through the image
            int[] map = new int[(int)image.Width * (int)image.Height];
            
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    System.Drawing.Color pixelColor = image.GetPixel(x, y);
                    map[y*image.Width + x] = pixelColor.ToArgb();
                    pixelColor.GetHue();  
                }
            }
           
            List<AnalyzedObject> objectList = analyzer.analyzeImage(map, image.Width, image.Height); 
            foreach (AnalyzedObject o in objectList)
                if (o.decision == true)
                {
                    if (o.color.Equal(Color.green)) return AnalyzedState.Green;
                    else if (o.color.Equal(Color.red)) return AnalyzedState.Red;
                    else return AnalyzedState.Unknown; 
                }
            return AnalyzedState.Unknown; 
        }
    }
}