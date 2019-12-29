using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
namespace GifInfo
{
    public class GifInfo
    {
        #region Fileds
        private FileInfo fileInfo;
        private IList<Image> frames;
        private Size size;
        private bool animated;
        private bool loop;
        private TimeSpan animationDuration;
        #endregion
        #region Properties
        public FileInfo FileInfo{
            get {
                return this.fileInfo;
            }
        }
        public IList<Image> Frames{  
        get  
            {  
                return this.frames;  
            }
}

public Size Size
{
    get
    {
        return this.size;
    }
}

public bool Animated
{
    get
    {
        return this.animated;
    }
}

public bool Loop
{
    get
    {
        return this.loop;
    }
}

public TimeSpan AnimationDuration
{
    get
    {
        return this.animationDuration;
    }
}

#endregion


#region Constructors  

public GifInfo(String filePath)
{
    if (File.Exists(filePath))
    {
        using (var image = Image.FromFile(filePath))
        {
            this.size = new Size(image.Width, image.Height);

            if (image.RawFormat.Equals(ImageFormat.Gif))
            {
                this.frames = new List<Image>();
                this.fileInfo = new FileInfo(filePath);

                if (ImageAnimator.CanAnimate(image))
                {
                    //Get frames  
                    var dimension = new FrameDimension(image.FrameDimensionsList[0]);
                    int frameCount = image.GetFrameCount(dimension);

                    int index = 0;
                    int duration = 0;
                    for (int i = 0; i < frameCount; i++)
                    {
                        image.SelectActiveFrame(dimension, i);
                        var frame = image.Clone() as Image;
                        frames.Add(frame);

                        var delay = BitConverter.ToInt32(image.GetPropertyItem(20736).Value, index) * 10;
                        duration += (delay < 100 ? 100 : delay);

                        index += 4;
                    }

                    this.animationDuration = TimeSpan.FromMilliseconds(duration);
                    this.animated = true;
                    this.loop = BitConverter.ToInt16(image.GetPropertyItem(20737).Value, 0) != 1;
                }
                else
                {
                    this.frames.Add(image.Clone() as Image);
                }

            }
            else
            {
                throw new FormatException("Not valid GIF image format");
            }
        }

    }
}  
        #endregion  
    }  
}  

namespace GifToPikabu
{
    class PikabuNumLst
    {
        List<KeyValuePair<int, int>> lst = new List<KeyValuePair<int, int>>();
        int last = -1;
        public string toString()
        {
            string s = "[";
            foreach(var t in lst)
            {
                s += t.Key.ToString() + ',' + t.Value.ToString()+',';
            }
            s = s.TrimEnd(',');
            s += ']';
            return s;
        }

        public void Add(int a)
        {
            if (last == -1 || lst[last].Key != a)
            {
                lst.Add(new KeyValuePair<int, int>(a, 1));
                last++;
            }
            else lst[last] = new KeyValuePair<int, int>(lst[last].Key, lst[last].Value + 1);
        }

        public int Sum()
        {
            int a = 0;
            foreach(var s in lst)
            {
                a += s.Value;
            }
            return a;
        }

        public void Remove()
        {
            if (lst[last].Value > 1)
                lst[last] = new KeyValuePair<int, int>(lst[last].Key, lst[last].Value - 1);
            else lst.RemoveAt(last);
        }
    }


    class Program
    {
        public static Dictionary<int, Color> lst = new Dictionary<int, Color>();
        static int addy = 0, addx = 0;

        static double Distance(Color e1, Color e2)
        {
            long rmean = ((long)e1.R + (long)e2.R)/2;
            int r = e1.R - e2.R;
            int g = e1.G - e2.G;
            int b = e1.B - e2.B;
            double weightR = 2 + rmean / 256;
            double weightG = 4.0;
            double weightB = 2 + (255 - rmean) / 256;
            return Math.Sqrt(weightR * r * r + weightG * g * g + weightB * b * b);
        }

        static int closest(Color a)
        {
            double min = Distance(a, lst[1]);
            int num = 1;
            foreach(var s in lst)
            {
                double t = Distance(s.Value, a);
                if (min > t)
                {
                    min = t;
                    num = s.Key;
                }
            }
            return num;
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        static void Main(string[] args)
        {
            List<PikabuNumLst> frames = new List<PikabuNumLst>();
            lst.Add(0, Color.FromArgb(0, 0, 0));
            for (int i = 0; i < 5; i++)
            {
                lst.Add(i + 1 + 0, Color.FromArgb(119 + (i * 34), 119 + (i * 34), 119 + (i * 34)));
                lst.Add(i + 1 + 16, Color.FromArgb(119 + (i * 34), 87 + (i * 2), 83 - (i * 2)));
                lst.Add(i + 1 + 32, Color.FromArgb(89 + (4 * i), 119 + (34 * i), 69 - (16 * i)));
                lst.Add(i + 1 + 48, Color.FromArgb(69 - (16 * i), 107 + (22 * i), 119 + (34 * i)));
                lst.Add(i + 1 + 64, Color.FromArgb(119 + (34 * i), 91 + (int)(5.5 * i), 113 + (int)(28.5 * i)));
                lst.Add(i + 1 + 80, Color.FromArgb(119 + (34 * i), 117 + (int)(32.5 * i), 93 + (int)(7.5 * i)));
                lst.Add(i + 1 + 96, Color.FromArgb(68 - (17 * i), 117 + (int)(32.5 * i), 119 + (34 * i)));
                lst.Add(i + 1 + 112, Color.FromArgb(109 + (int)(23.5 * i), 104 + (int)(19 * i), 119 + (int)(34 * i)));
                lst.Add(i + 1 + 128, Color.FromArgb(119 + (34 * i), 106 + (int)(20.5 * i), 111 + (int)(26.5 * i)));
                lst.Add(i + 1 + 144, Color.FromArgb(119 + (34 * i), 104 + (int)(19 * i), 88 + (int)(3.5 * i)));
                lst.Add(i + 1 + 160, Color.FromArgb(110 + (25 * i), 119 + (int)(34 * i), 97 + (int)(6.5 * i)));
                lst.Add(i + 1 + 176, Color.FromArgb(103 + (int)(17.5 * i), 119 + (int)(34 * i), 141 + (int)(28.5 * i)));
            }
            if (args.Length == 0)
            {
                Console.WriteLine("Перетяните на иконку приложения GIF-анимацию для конвертации!");
                Console.ReadKey();
                return;
            }
            var path = args[0];
            //var path = "D:/1.gif";
            var info = new GifInfo.GifInfo(path);
            List<Image> frms = info.Frames.ToList();
            for (int i = 0; i < 40 && i < frms.Count; i++)
            {
                frames.Add(new PikabuNumLst());
                var frame = frms[i];
                int scaleparameter = Math.Max(frame.Width/40, frame.Height/20);
                Bitmap bmpFrame = ResizeImage(frame, frame.Width/scaleparameter, frame.Height/scaleparameter);
                int hparam = 0, wparam = 0;
                bool even = false;
                if (bmpFrame.Height < 20)
                    hparam = (20 - bmpFrame.Height);
                else if (bmpFrame.Width < 40)
                    wparam = (40 - bmpFrame.Width);
                for (int y = -hparam/2; y < 20-hparam/2; y++)
                {
                    for (int x = -wparam/2; x < 40-wparam/2; x++)
                    {
                        Color a;
                        if (y < 0 || y >= bmpFrame.Height || x < 0 || x >= bmpFrame.Width)
                            a = Color.Black;
                        else a = bmpFrame.GetPixel(x, y);
                        int clos = closest(a);
                        frames[i].Add(clos);
                    }
                    while (frames[i].Sum() % 20 != 0)
                        frames[i].Add(0);
                }
            }
            string s = "{\"data\":[[";
            for (int i = 0; i < 40; i++)
            {
                if (i < frames.Count)
                    s += frames[i].toString();
                else s += "null";
                s += ',';
            }
            s = s.Trim(new char[1]{','});
            s += "]], \"isMirror\":false}";
            Console.WriteLine(s);
        Console.ReadLine();
        }
    }
}
