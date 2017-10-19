using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalMeet.Models
{

    public class OcrResult
    {
        public string Language;
        public double TextAngle;
        public string Orientation;

        public IList<Region> regions;
    }

    public class Word
    {
        public string BoundingBox;
        public string Text;
    }

    public class Line
    {
        public string BoundingBox;
        
        public IList<Word> Words;
    }

    public class Region
    {
        public string BoundingBox;
        public IList<Line> Lines;
    }
}
