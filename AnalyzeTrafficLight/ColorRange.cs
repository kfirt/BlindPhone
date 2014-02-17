using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyzeTrafficLight
{

    public class ColorStat
    {
        public double R { get; set; }
        public double G { get; set; }
        public double B { get; set; }
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

    public class Color
    {
        public Color(byte r, byte g, byte b)
        {
            R=r;
            G=g;
            B=b;
        }

        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        static public Color red = new Color(255,0,0);
        static public Color green = new Color(0,255,0);

        public bool Equal(Color other)
        {
            if (other.R != R) return false;
            if (other.G != G) return false;
            if (other.B != B) return false;
            return true;
        }
    }

}
