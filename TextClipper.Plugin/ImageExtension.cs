using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing.Imaging;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TextClipper.Plugin
{
    public static class ImageExtension
    {
        public static ImageSource ToImageSource(this System.Drawing.Image image)
        {
            var ms = new MemoryStream();
            image.Save(ms, ImageFormat.Png);
            var src = (ImageSource)(new ImageSourceConverter().ConvertFrom(ms));
            if (src.CanFreeze) src.Freeze();
            return src;
        }
    }
}
