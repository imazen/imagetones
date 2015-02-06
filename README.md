
ImageTones uses an octree quantization method to give you the 'major' colors of an image. It can generate between 2 and 255 colors. The weight values are the number of pixels with the given color. 




```
var tool = new ImageTones.ImageTones(int maxColorCount, bool analyzeResizedCopy);

IList<Tuple<Color, long>> colors =  tool.GetWeightedColors(Bitmap src);
```

Heavily derived from http://codebetter.com/brendantompkins/2007/06/14/gif-image-color-quantizer-now-with-safe-goodness/

Released under the terms of the  MIT license

Copyright (c) 2015 Imazen LLC

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.