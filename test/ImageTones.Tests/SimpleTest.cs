using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ImageTones.Tests
{

    public class SimpleTest
    {

        public Bitmap CreateImageWithColors(Color[] colors)
        {
            var sliceWidth = 3;
            var height = 2;
            var b = new Bitmap(colors.Length * sliceWidth, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(b))
            {
                for (var x = 0; x < colors.Length; x++)
                {
                    g.FillRectangle(new SolidBrush(colors[x]), new Rectangle(x * sliceWidth, 0, sliceWidth, height));
                }
            }
            return b;
        }
        [Fact]
        public void TestRedBlue()
        {
            using (var b = CreateImageWithColors(new Color[] { Color.Red, Color.Blue }))
            {
                var results = new ImageTones(2, true).GetWeightedColors(b);
                var colors = results.Select(t => t.Item1.ToArgb());

                Assert.Contains(Color.Red.ToArgb(), colors);
                Assert.Contains(Color.Blue.ToArgb(), colors);
            }
        }

        [Fact]
        public void TestRandom()
        {
            Color[] inputColors = new Color[1000];
            var r= new Random();
            for (var i = 0; i < inputColors.Length; i++)
            {
                inputColors[i] = Color.FromArgb(255, 255, r.Next(256), r.Next(256));
            }
            using (var b = CreateImageWithColors(inputColors))
            {
                var results = new ImageTones(64, true).GetWeightedColors(b);
                var redValues = results.Select(t => t.Item1.R).Distinct().ToArray();
                Assert.Equal(redValues,new byte[]{255});
            }
        }

    }
}
