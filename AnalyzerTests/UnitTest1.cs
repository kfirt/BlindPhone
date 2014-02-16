using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using sdkCameraGrayscaleCS;
using System.Drawing;

namespace AnalyzerTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        [DeploymentItem("reds")]
        public void TestMethod1()
        {
            Assert.AreEqual(AnalyzedState.Red, runAnalyzer(@"C:\Users\Adi\Source\Repos\BlindPhone\AnalyzerTests\reds\WP_20140216_016.jpg"));
        }


        private AnalyzedState runAnalyzer(string imagePath)
        {
            //Uri uri = new Uri(imagePath, UriKind.RelativeOrAbsolute);
            Analyzer analyzer = new Analyzer();
            
            Bitmap image = new Bitmap(imagePath);

            // Loop through the image
            int[] map = new int[(int)image.Width * (int)image.Height];
            
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Color pixelColor = image.GetPixel(x, y);
                    map[x*image.Width + y] = pixelColor.ToArgb();
                }
            }
            return analyzer.process(map);
            //return AnalyzedState.Green;

        }
    }
}
