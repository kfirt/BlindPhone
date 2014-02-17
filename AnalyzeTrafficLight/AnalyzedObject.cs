using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyzeTrafficLight
{
    public class AnalyzedObject
    {
        public int x { get; set; }
        public int y { get; set; }

        public int size { get; set; }

        public Color color { get; set; }
        public bool decision { get; set; }
    }
}
