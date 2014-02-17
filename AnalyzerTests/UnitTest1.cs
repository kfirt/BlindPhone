using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using sdkCameraGrayscaleCS;
using System.Drawing;
using System.IO;
using AnalyzeTrafficLight;
using System.Collections.Generic; 

namespace AnalyzerTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestReds()
        {
            Assert.AreEqual(AnalyzedState.Red, runAnalyzer(@"C:\Users\Adi\Source\Repos\BlindPhone\AnalyzerTests\reds\WP_20140216_016.jpg"));
        }
        [TestMethod]
        public void TestGreens()
        {
            Assert.AreEqual(AnalyzedState.Green, runAnalyzer(@"C:\Users\Adi\Source\Repos\BlindPhone\AnalyzerTests\greens\WP_20140216_011.jpg"));
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
                    map[x*image.Width + y] = pixelColor.ToArgb();
                }
            }
           
            List<AnalyzedObject> objectList = analyzer.analyzeImage(map, image.Width, image.Height); 
            //foreach (AnalyzedObject o in objectList)
                //if (o.decision == true)
                //{
                //    return o.color;  
                //}
            return AnalyzedState.Red; 
        }
    }
}
