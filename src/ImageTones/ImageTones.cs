using ImageTones;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageTones
{
    public class ImageTones
    {
        OctreeQuantizer q;
        int bits;
        int count;
        bool analyzeResizedCopy;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="count">The maximum number of colors to collect</param>
        /// <param name="analyzeResizedCopy">If true, images over 512x512 will be scaled down to 256x256 before being analyzed for color.</param>
        public ImageTones(int count, bool analyzeResizedCopy)
        {
            bits = (int)Math.Ceiling(Math.Log(count, 2));
            this.count = count;
            this.analyzeResizedCopy = analyzeResizedCopy;

        }

        public IList<Tuple<Color, long>>  GetWeightedColors(Bitmap src){
            var q = new OctreeQuantizer(count, bits);
            q.ResizeForFirstPass = analyzeResizedCopy;

            return q.CalculateWeightedColors(src,count);
        }
    }
}
