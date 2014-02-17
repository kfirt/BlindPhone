using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyzeTrafficLight
{

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
}
