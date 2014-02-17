using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyzeTrafficLight
{
    public class ObjPoint
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    public class BoundingBox
    {
        public ObjPoint leftTop = new ObjPoint();
        public ObjPoint rightBottom = new ObjPoint();
    }

    public class AnalyzedObject
    {
        public ObjPoint leftTop = new ObjPoint();
        
        public int size { get; set; }

        public Color color { get; set; }
        public bool decision { get; set; }

        public BoundingBox bBox = new BoundingBox();

        public ColorStat colorStdev = new ColorStat();

        public ColorStat colorAv = new ColorStat();

        public ObjPoint centroid = new ObjPoint();
    }
}
