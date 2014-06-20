using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel.Composition;

using TextClipper.Plugin;

namespace TextClipper.Plugins.Base64
{
    [Export(typeof(IPlugin))]
    public class Base64Decoder : IPlugin, ISupportInput
    {
        public Base64Decoder()
        {
            _icon = Properties.Resources.frombase64.ToImageSource();
        }

        public string Name
        {
            get { return "Base64Decoder"; }
        }

        public string Description
        {
            get { return "Base64エンコードされた文字列を検出、変換します。"; }
        }

        private ImageSource _icon;
        public System.Windows.Media.ImageSource Icon
        {
            get { return _icon; }
        }

        public void Exit()
        {

        }

        public string Inputting(string value)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(value, "[^A-Za-z0-9+-=]"))
                try
                {
                    value = Encoding.UTF8.GetString(Convert.FromBase64String(value));
                }
                catch { }
            return value;
        }
    }

    [Export(typeof(IPlugin))]
    public class Base64Encoder : IPlugin, ISupportOutput
    {
        public Base64Encoder()
        {
            _icon = Properties.Resources.tobase64.ToImageSource();
        }

        public string Name
        {
            get { return "Base64Encoder"; }
        }

        public string Description
        {
            get { return "文字列をBase64エンコードし、書き出します。"; }
        }

        private ImageSource _icon;
        public System.Windows.Media.ImageSource Icon
        {
            get { return _icon; }
        }

        public void Exit()
        {

        }

        public string Outputting(string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        }
    }
}
