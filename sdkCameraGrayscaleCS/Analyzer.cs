using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace sdkCameraGrayscaleCS
{
    public class Analyzer
    {
        public Analyzer()
        {

        }

        public AnalyzedState process(int[] bitmap)
        {
            return AnalyzedState.Red;
            //Array values = Enum.GetValues(typeof(AnalyzedState));
            //Random random = new Random();
            //return (AnalyzedState)values.GetValue(random.Next(values.Length));
        }
    }
}
