using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Snail.ImageThumb
{
    class ImageThumb
    {

        private const long KB = 1024;
        private const long MB = 1024 * 1024;
        private const long GB = 1024L * 1024L * 1024L;
        private const long TB = 1024L * 1024L * 1024L * 1024L;

        private ImageCodecInfo jpegICIinfo = null;

        public ImageThumb()
        {
            Init();
        }


        private void Init()
        {
            ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();
            //ImageCodecInfo jpegICIinfo = null;
            for (int x = 0; x < arrayICI.Length; x++)
            {
                if (arrayICI[x].FormatDescription.Equals("JPEG"))
                {
                    this.jpegICIinfo = arrayICI[x];
                    break;
                }
            }
        }

        public void Run()
        {
            try
            {

                var configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.xml");
                if (File.Exists(configFile))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(configFile);

                    var paths = doc.SelectSingleNode("config").SelectSingleNode("paths").SelectNodes("path");
                    if (paths != null && paths.Count > 0)
                    {
                        foreach (XmlNode path in paths)
                        {
                            var input = path.SelectSingleNode("input").InnerText;
                            var output = path.SelectSingleNode("output").InnerText;
                            var sizeStr = path.SelectSingleNode("size").InnerText;
                            Size size = new Size();
                            var ss = sizeStr.Split('x');
                            size.Width = int.Parse(ss[0]);
                            size.Height = int.Parse(ss[1]);
                            Debug.WriteLine("input:{0}\noutput:{1}", input, output);
                            Work(input, output, size);
                        }
                    }
                }
                else
                {
                    Console.Error.WriteLine("配置文件不存在：{0}", configFile);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("出错了：{0}", e.Message);
            }
        }

        void Work(string input, string output, Size size)
        {
            try
            {
                input = new DirectoryInfo(input).FullName;
                output = new DirectoryInfo(output).FullName;
                if (!Directory.Exists(input))
                {
                    Console.WriteLine("输入目录不存在，跳过。{0}", input);
                    return;
                }
                if (!Directory.Exists(output))
                {
                    Directory.CreateDirectory(output);
                }
                var files = new DirectoryInfo(input)
                    .GetFiles("*.*", SearchOption.AllDirectories)
                    .Where(t =>
                        {
                            var n = t.Name.ToLower();
                            return n.EndsWith(".jpg")
                            || n.EndsWith(".jpeg")
                            || n.EndsWith(".png")
                            || n.EndsWith(".bmp")
                            || n.EndsWith(".gif");
                        }
                    );
                var count = files.Count();
                var progress = 0;
                Console.WriteLine("路径：{0}，文件数：{1}", input, count);
                foreach (var item in files)
                {
                    progress++;
                    var dir = item.DirectoryName.Replace(input, "").Trim('\\', '/');
                    var outDir = Path.Combine(output, dir);
                    var outFullName = Path.Combine(outDir, Path.GetFileNameWithoutExtension(item.Name) + ".jpg");
                    if (File.Exists(outFullName))
                    {
                        Console.WriteLine("[{1}/{2}]输出文件存在，跳过。{0}", outFullName, progress, count);
                        continue;
                    }
                    if (!Directory.Exists(outDir))
                    {
                        Directory.CreateDirectory(outDir);
                    }
                    Console.WriteLine("[{3}/{4}]{0}\\{1} -> {2}", dir, item.Name, outFullName, progress, count);
                    var success = GetPicThumbnail(item.FullName, outFullName, size.Width, size.Height, 70);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("处理中出错了：{0}", e.Message);
            }

        }

        /// <summary>
        /// 无损压缩图片
        /// </summary>
        /// <param name="sFile">原图片</param>
        /// <param name="dFile">压缩后保存位置</param>
        /// <param name="dWidth">宽度</param>
        /// <param name="dHeight">高度</param>
        /// <param name="flag">压缩质量 1-100</param>
        /// <returns></returns>
        public bool GetPicThumbnail(string sFile, string dFile, int dWidth, int dHeight, int flag)
        {
            System.Drawing.Image iSource = System.Drawing.Image.FromFile(sFile);
            ImageFormat tFormat = iSource.RawFormat;

            Bitmap ob = GetPicThumbnail(iSource, dWidth, dHeight);

            try
            {
                if (this.jpegICIinfo != null)
                {
                    SaveImage(ob, dFile, this.jpegICIinfo, flag);
                }
                else
                {
                    ob.Save(dFile, tFormat);
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                if (iSource != null)
                {
                    iSource.Dispose();
                }
                if (ob != null)
                {
                    ob.Dispose();
                }
            }
            return true;
        }

        public Bitmap GetPicThumbnail(Image iSource, int dWidth, int dHeight)
        {
            ImageFormat tFormat = iSource.RawFormat;
            int sW = 0, sH = 0;
            //按比例缩放
            Size tem_size = new Size(iSource.Width, iSource.Height);

            //如果尺寸比原图小，则进行缩放，否则使用原图尺寸
            if (tem_size.Width > dHeight && tem_size.Width > dWidth)
            {
                if (dWidth == 0 || dHeight == 0)
                {
                    if (dHeight == 0)
                    {
                        sW = dWidth;
                        sH = (dWidth * tem_size.Height) / tem_size.Width;
                    }
                    else
                    {
                        sH = dHeight;
                        sW = (tem_size.Width * dHeight) / tem_size.Height;
                    }
                }
                else
                {
                    sW = dWidth;
                    sH = dHeight;
                }
            }
            else
            {
                sW = tem_size.Width;
                sH = tem_size.Height;
            }
            Bitmap ob = new Bitmap(sW, sH);
            Graphics g = Graphics.FromImage(ob);
            g.Clear(Color.WhiteSmoke);
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(iSource, new Rectangle(0, 0, sW, sH), 0, 0, iSource.Width, iSource.Height, GraphicsUnit.Pixel);
            g.Dispose();
            return ob;
        }

        public void SaveImage(Image image, string filePath, ImageCodecInfo codecInfo, int flag)
        {
            if (image != null && filePath != null && codecInfo != null)
            {
                //以下代码为保存图片时，设置压缩质量
                EncoderParameters ep = new EncoderParameters();
                long[] qy = new long[1];
                qy[0] = flag;//设置压缩的比例1-100
                EncoderParameter eParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qy);
                ep.Param[0] = eParam;
                try { File.Delete(filePath); }
                catch { }
                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }
                image.Save(filePath, codecInfo, ep);//dFile是压缩后的新路径
            }
            else
            {
                if (image == null)
                {
                    throw new ArgumentNullException("image");
                }
                if (filePath == null)
                {
                    throw new ArgumentNullException("filePath");
                }
                if (codecInfo == null)
                {
                    throw new ArgumentNullException("codecInfo");
                }
            }

        }

        /// <summary>
        /// 截取图片一部分
        /// </summary>
        /// <param name="sourceBitmap">源图片</param>
        /// <param name="width">宽</param>
        /// <param name="height">高</param>
        /// <param name="offsetX">X坐标</param>
        /// <param name="offsetY">Y坐标</param>
        /// <returns></returns>
        public Bitmap GetPartOfImage(Bitmap sourceBitmap, int width, int height, int offsetX, int offsetY)
        {
            Bitmap resultBitmap = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(resultBitmap))
            {
                Rectangle resultRectangle = new Rectangle(0, 0, width, height);
                Rectangle sourceRectangle = new Rectangle(0 + offsetX, 0 + offsetY, width, height);
                g.DrawImage(sourceBitmap, resultRectangle, sourceRectangle, GraphicsUnit.Pixel);
            }
            return resultBitmap;
        }
    }
}
