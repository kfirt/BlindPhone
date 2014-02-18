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
        public void TestReds17_2_Red1()
        {
            Assert.AreEqual(AnalyzedState.Red, runAnalyzer(@"C:\Pics\18-2-14\Red\WP_20140217_001.jpg"));

        }
        [TestMethod]
        public void TestReds17_2_Red3()
        {
            Assert.AreEqual(AnalyzedState.Red, runAnalyzer(@"C:\Pics\18-2-14\Red\WP_20140217_003.jpg"));

        }
        [TestMethod]
        public void TestReds17_2_Red6()
        {
            Assert.AreEqual(AnalyzedState.Red, runAnalyzer(@"C:\Pics\18-2-14\Red\WP_20140217_006.jpg"));

        }
        [TestMethod]
        public void TestReds17_2_Red7()
        {
            Assert.AreEqual(AnalyzedState.Red, runAnalyzer(@"C:\Pics\18-2-14\Red\WP_20140217_007.jpg"));

        }
        [TestMethod]
        public void TestReds18_2_Red1()
        {
            Assert.AreEqual(AnalyzedState.Red, runAnalyzer(@"C:\Pics\18-2-14\Red\WP_20140218_001.jpg"));

        }
        [TestMethod]
        public void TestReds18_2_Red2()
        {
            Assert.AreEqual(AnalyzedState.Red, runAnalyzer(@"C:\Pics\18-2-14\Red\WP_20140218_002.jpg"));

        }
        [TestMethod]
        public void TestReds18_2_Red3()
        {
            Assert.AreEqual(AnalyzedState.Red, runAnalyzer(@"C:\Pics\18-2-14\Red\WP_20140218_003.jpg"));

        }
        [TestMethod]
        public void TestReds18_2_Red19()
        {
            Assert.AreEqual(AnalyzedState.Red, runAnalyzer(@"C:\Pics\18-2-14\Red\WP_20140218_019.jpg"));

        }
        [TestMethod]
        public void TestReds18_2_Red20()
        {
            Assert.AreEqual(AnalyzedState.Red, runAnalyzer(@"C:\Pics\18-2-14\Red\WP_20140218_020.jpg"));

        }
        [TestMethod]
        public void TestReds18_2_Red33()
        {
            Assert.AreEqual(AnalyzedState.Red, runAnalyzer(@"C:\Pics\18-2-14\Red\WP_20140218_033.jpg"));

        }
        [TestMethod]
        public void TestReds18_2_Red35()
        {
            Assert.AreEqual(AnalyzedState.Red, runAnalyzer(@"C:\Pics\18-2-14\Red\WP_20140218_035.jpg"));

        }
        [TestMethod]
        public void TestReds18_2_Red41()
        {
            Assert.AreEqual(AnalyzedState.Red, runAnalyzer(@"C:\Pics\18-2-14\Red\WP_20140218_041.jpg"));

        }
        [TestMethod]
        public void TestReds18_2_Red44()
        {
            Assert.AreEqual(AnalyzedState.Red, runAnalyzer(@"C:\Pics\18-2-14\Red\WP_20140218_044.jpg"));

        }
        [TestMethod]
        public void TestReds18_2_Red45()
        {
            Assert.AreEqual(AnalyzedState.Red, runAnalyzer(@"C:\Pics\18-2-14\Red\WP_20140218_045.jpg"));

        }
        [TestMethod]
        public void TestReds18_2_RedGreen4()
        {
            Assert.AreEqual(AnalyzedState.Red, runAnalyzer(@"C:\Pics\18-2-14\RedGreen\WP_20140217_004.jpg"));

        }
        [TestMethod]
        public void TestReds18_2_RedGreen8()
        {
            Assert.AreEqual(AnalyzedState.Red, runAnalyzer(@"C:\Pics\18-2-14\RedGreen\WP_20140217_008.jpg"));

        }
        [TestMethod]
        public void TestReds18_2_RedGreen14()
        {
            Assert.AreEqual(AnalyzedState.Red, runAnalyzer(@"C:\Pics\18-2-14\RedGreen\WP_20140218_014.jpg"));

        }
        [TestMethod]
        public void TestReds18_2_RedGreen26()
        {
            Assert.AreEqual(AnalyzedState.Red, runAnalyzer(@"C:\Pics\18-2-14\RedGreen\WP_20140218_026.jpg"));

        }
        [TestMethod]
        public void TestReds18_2_GreenRed2()
        {
            Assert.AreEqual(AnalyzedState.Green, runAnalyzer(@"C:\Pics\18-2-14\GreenRed\WP_20140217_002.jpg"));

        }
        [TestMethod]
        public void TestReds18_2_GreenRed5()
        {
            Assert.AreEqual(AnalyzedState.Green, runAnalyzer(@"C:\Pics\18-2-14\GreenRed\WP_20140217_005.jpg"));

        }
        [TestMethod]
        public void TestReds18_2_GreenRed25()
        {
            Assert.AreEqual(AnalyzedState.Green, runAnalyzer(@"C:\Pics\18-2-14\GreenRed\WP_20140218_025.jpg"));

        }
        [TestMethod]
        public void TestReds18_2_GreenRed32()
        {
            Assert.AreEqual(AnalyzedState.Green, runAnalyzer(@"C:\Pics\18-2-14\GreenRed\WP_20140218_032.jpg"));

        }
        [TestMethod]
        public void TestReds18_2_GreenRed43()
        {
            Assert.AreEqual(AnalyzedState.Green, runAnalyzer(@"C:\Pics\18-2-14\GreenRed\WP_20140218_043.jpg"));

        }

        
        
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
        [TestMethod]
        public void TestGreens14()
        {
            Assert.AreEqual(AnalyzedState.Green, runAnalyzer(@"C:\Pics\18-2-14\Green\WP_20140218_004.jpg"));
        }
        [TestMethod]
        public void TestGreens15()
        {
            Assert.AreEqual(AnalyzedState.Green, runAnalyzer(@"C:\Pics\18-2-14\Green\WP_20140218_007.jpg"));
        }
        [TestMethod]
        public void TestGreens16()
        {
            Assert.AreEqual(AnalyzedState.Green, runAnalyzer(@"C:\Pics\18-2-14\Green\WP_20140218_012.jpg"));
        }

        [TestMethod]
        public void TestGreens17()
        {
            Assert.AreEqual(AnalyzedState.Green, runAnalyzer(@"C:\Pics\18-2-14\Green\WP_20140218_021.jpg"));
        }

        [TestMethod]
        public void TestGreens18()
        {
            Assert.AreEqual(AnalyzedState.Green, runAnalyzer(@"C:\Pics\18-2-14\Green\WP_20140218_022.jpg"));
        }

        [TestMethod]
        public void TestGreens19()
        {
            Assert.AreEqual(AnalyzedState.Green, runAnalyzer(@"C:\Pics\18-2-14\Green\WP_20140218_034.jpg"));
        }

        [TestMethod]
        public void TestGreens20()
        {
            Assert.AreEqual(AnalyzedState.Green, runAnalyzer(@"C:\Pics\18-2-14\Green\WP_20140218_023.jpg"));
        }
        [TestMethod]
        public void TestGreens21()
        {
            Assert.AreEqual(AnalyzedState.Green, runAnalyzer(@"C:\Pics\18-2-14\Green\WP_20140218_036.jpg"));
        }
        [TestMethod]
        public void TestGreens22()
        {
            Assert.AreEqual(AnalyzedState.Green, runAnalyzer(@"C:\Pics\18-2-14\Green\WP_20140218_038.jpg"));
        }
        [TestMethod]
        public void TestGreens23()
        {
            Assert.AreEqual(AnalyzedState.Green, runAnalyzer(@"C:\Pics\18-2-14\Green\WP_20140218_037.jpg"));
        }
        [TestMethod]
        public void TestGreens24()
        {
            Assert.AreEqual(AnalyzedState.Green, runAnalyzer(@"C:\Pics\18-2-14\Green\WP_20140218_039.jpg"));
        }

        [TestMethod]
        public void TestGreens25()
        {
            Assert.AreEqual(AnalyzedState.Green, runAnalyzer(@"C:\Pics\18-2-14\Green\WP_20140218_049.jpg"));
        }
        [TestMethod]
        public void TestGreens26()
        {
            Assert.AreEqual(AnalyzedState.Green, runAnalyzer(@"C:\Pics\18-2-14\Green\WP_20140218_048.jpg"));
        }
        [TestMethod]
        public void TestGreens27()
        {
            Assert.AreEqual(AnalyzedState.Green, runAnalyzer(@"C:\Pics\18-2-14\Green\WP_20140218_047.jpg"));
        }
        [TestMethod]
        public void TestGreens28()
        {
            Assert.AreEqual(AnalyzedState.Green, runAnalyzer(@"C:\Pics\18-2-14\Green\WP_20140218_046.jpg"));
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