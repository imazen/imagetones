/* 
 * Copyright (c) 2014 Imazen See license.txt for your rights 
 * 
 * This code has been *heavily* modified from its original versions
 * 
 * Derived from: http://codebetter.com/brendantompkins/2007/06/14/gif-image-color-quantizer-now-with-safe-goodness/
 * 
 * Portions of this file are under the following license:
 * 
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF 
 * ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A 
 * PARTICULAR PURPOSE. 
 *
 * This is sample code and is freely distributable. 
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImageTones

  
{
    /// <summary>
    /// Abstract Quantizer class - handles the messy, algorithm-independent details of quantization. 
    /// Subclasses must implement InitialQuantizePixel, GetPallete(), and QuantizePixel. Not thread-safe!
    /// </summary>
    public abstract class Quantizer
    {



        private int _pixelSize;
        /// <summary>
        /// The number of bytes in a ARGB structure. Should be 4
        /// </summary>
        public int PixelSize {
            get { return _pixelSize; }
        }


        private bool _resizeForFirstPass = false;
        /// <summary>
        /// If true, the first pass (InitialQuantizePixel) will be performed on a size-limited version of the original image to control performance. Ignored if FixedPalette=True
        /// </summary>
        public bool ResizeForFirstPass {
            get { return _resizeForFirstPass; }
            set { _resizeForFirstPass = value; }
        }

        private long _firstPassPixelCount = 256 * 256;
        /// <summary>
        /// The approximate number of pixels to use when making a scaled copy of the image for the first pass. Only used when ResizeForFirstPass=True and FirstPassPixelThreshold is exceeded.
        /// </summary>
        public long FirstPassPixelCount {
            get { return _firstPassPixelCount; }
            set { _firstPassPixelCount = value; }
        }

        private long _firstPassPixelThreshold = 512 * 512;
        /// <summary>
        /// The maximum number of pixels the original image may contain before a scaled copy is made for the first pass. 
        /// Only relevant when ResizeForFirstPass=True
        /// </summary>
        public long FirstPassPixelThreshold {
            get { return _firstPassPixelThreshold; }
            set { _firstPassPixelThreshold = value; }
        }



        /// <summary>
        /// Construct the quantizer
        /// </summary>
        /// <param name="fixedPalette">If true, the quantization only needs to loop through the source pixels once - InitialQuantiize</param>
        /// <remarks>
        /// If you construct this class with a true value for singlePass, then the code will, when quantizing your image,
        /// only call the 'QuantizeImage' function. If two passes are required, the code will call 'InitialQuantizeImage'
        /// and then 'QuantizeImage'.
        /// </remarks>
        public Quantizer()
        {
            _pixelSize = Marshal.SizeOf(typeof (Color32));
        }

        /// <summary>
        /// Resets the quantizer so it can process a new image. 
        /// </summary>
        public virtual void Reset()
        {

        }

        public IList<Tuple<Color, long>> CalculateWeightedColors(Bitmap src, int maxColors){

            if (!src.PixelFormat.Equals(PixelFormat.Format32bppArgb) || FirstPassPixelThreshold < src.Width * src.Height){
                double factor = FirstPassPixelCount / ((double)src.Width * (double)src.Height);
                using (var firstPass = new Bitmap((int)Math.Floor((double)src.Width * factor), (int)Math.Floor((double)src.Height * factor), PixelFormat.Format32bppArgb)){
                    using (Graphics g = Graphics.FromImage(firstPass)) {
                        //Use the low-quality settings - we want the original colors of the image, nearest neighbor is better than bicubic spline here.
                        g.PageUnit = GraphicsUnit.Pixel;
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighSpeed;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                        g.DrawImage(src, 0, 0, firstPass.Width, firstPass.Height);
                    }
                    return AnalyzeAndWeight(firstPass, maxColors);
                }
            }
            else
            {
                return AnalyzeAndWeight(src, maxColors);
            }
        }
  

        protected IList<Tuple<Color, long>> AnalyzeAndWeight(Bitmap firstPass, int maxColors)
        {

            //This is our standard quantize, calling AnalyzeImage and QuantizeImage once each

            BitmapData firstPassData = null;
            try
            {
                firstPassData = firstPass.LockBits(new Rectangle(0, 0, firstPass.Width, firstPass.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                //Analyze image and build data structures so we can generate a palette
                AnalyzeImage(firstPassData, firstPass.Width, firstPass.Height);
                // Then set the color palette on the output bitmap, using the existing one since no ctor exists.
                return WeightedPalette(maxColors);
            }
            finally
            {
                firstPass.UnlockBits(firstPassData);
            }
            return null;
        }

  
        /// <summary>
        /// Execute the first pass through the pixels in the image
        /// </summary>
        /// <param name="sourceData">The source data</param>
        /// <param name="width">The width in pixels of the image</param>
        /// <param name="height">The height in pixels of the image</param>
        protected  virtual void AnalyzeImage(BitmapData sourceData, int width, int height)
        {
            // Define the source data pointers. The source row is a byte to
            // keep addition of the stride value easier (as this is in bytes)              
            IntPtr pSourceRow = sourceData.Scan0;

            // Loop through each row
            for (int row = 0; row < height; row++)
            {
                // Set the source pixel to the first pixel in this row
                IntPtr pSourcePixel = pSourceRow;

                // And loop through each column
                for (int col = 0; col < width; col++)
                {            
                    InitialQuantizePixel(new Color32(pSourcePixel)); 
                    pSourcePixel = (IntPtr)((long)pSourcePixel + PixelSize); //Increment afterwards
                }	// Now I have the pixel, call the FirstPassQuantize function...

                // Add the stride to the source row
                pSourceRow = (IntPtr)((long)pSourceRow + sourceData.Stride);
            }
        }
 



        //Truncates an int to a byte. 5-18-09 ndj
        protected byte ToByte(int i) {
            if (i < 0) return 0;
            if (i > 255) return 255;
            return (byte)i;
        }

        protected abstract IList<Tuple<Color, long>> WeightedPalette(int colorCount);

        /// <summary>
        /// Override this to process the pixel in the first pass of the algorithm
        /// </summary>
        /// <param name="pixel">The pixel to quantize</param>
        /// <remarks>
        /// This function need only be overridden if your quantize algorithm needs two passes,
        /// such as an Octree quantizer.
        /// </remarks>
        protected virtual void InitialQuantizePixel(Color32 pixel)
        {
        }


        /// <summary>
        /// Struct that defines a 32 bpp colour
        /// </summary>
        /// <remarks>
        /// This struct is used to read data from a 32 bits per pixel image
        /// in memory, and is ordered in this manner as this is the way that
        /// the data is layed out in memory
        /// </remarks>
        [StructLayout(LayoutKind.Explicit)]
        public struct Color32
        {
            public Color32(Color c){
                this.ARGB = c.ToArgb();
                Blue = c.B;
                Green = c.G;
                Red = c.R;
                Alpha = c.A;
            }
            public Color32(IntPtr pSourcePixel)
            {
              this = (Color32) Marshal.PtrToStructure(pSourcePixel, typeof(Color32));
                          
            }

            /// <summary>
            /// Holds the blue component of the colour
            /// </summary>
            [FieldOffset(0)]
            public byte Blue;
            /// <summary>
            /// Holds the green component of the colour
            /// </summary>
            [FieldOffset(1)]
            public byte Green;
            /// <summary>
            /// Holds the red component of the colour
            /// </summary>
            [FieldOffset(2)]
            public byte Red;
            /// <summary>
            /// Holds the alpha component of the colour
            /// </summary>
            [FieldOffset(3)]
            public byte Alpha;

            /// <summary>
            /// Permits the color32 to be treated as an int32
            /// </summary>
            [FieldOffset(0)]
            public int ARGB;

            /// <summary>
            /// Return the color for this Color32 object
            /// </summary>
            public Color Color
            {
                get { return Color.FromArgb(Alpha, Red, Green, Blue); }
            }
        }
    }
}
