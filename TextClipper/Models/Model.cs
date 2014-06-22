using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Livet;
using TextClipper.Plugin;

namespace TextClipper.Models
{
    public class Model : NotificationObject
    {
        /*
         * NotificationObjectはプロパティ変更通知の仕組みを実装したオブジェクトです。
         */

        private ObservableCollection<ClipItem> _clippedtexts;
        public ObservableCollection<ClipItem> ClippedTexts { get { return _clippedtexts; } }

        private IEnumerable<PluginInfo> _plugins;
        public IEnumerable<PluginInfo> Plugins { get { return _plugins; } }

        public void Initialize()
        {
            var last = OpenContents();
            if (last.Count() == 0)
                _clippedtexts = new ObservableCollection<ClipItem>() { new ClipItem("") };
            else
                //_clippedtexts = new ObservableCollection<ClipItem>(last.Select(p => new ClipItem(p)));
                _clippedtexts = new ObservableCollection<ClipItem>(last);

            _plugins = PluginInfo.GetPlugins();
        }

        public void Exit()
        {
            foreach (PluginInfo p in Plugins) p.Plugin.Exit();
            //SaveTexts(ClippedTexts.Select(p => p.Value));
            SaveContents(ClippedTexts);
        }

        public void InputText(string value, DateTime created)
        {
            foreach (PluginInfo p in Plugins)
                if (p.IsEnabled && p.Inputter != null)
                    value = p.Inputter.Inputting(value);
            ClippedTexts.Where(p => p.Created == created).Single().Value = value;
            if (ClippedTexts.Last().Created == created) ClippedTexts.Add(new ClipItem(""));
        }

        public void OutputText(DateTime created)
        {
            string value = ClippedTexts.Where(p => p.Created == created).Single().Value;
            foreach (PluginInfo p in Plugins)
                if (p.IsEnabled && p.Outputter != null)
                    value = p.Outputter.Outputting(value);
            System.Windows.Clipboard.SetText(value);
        }

        public void RemoveText(DateTime created)
        {
            ClippedTexts.Remove(ClippedTexts.Where(p => p.Created == created).Single());
        }


        private const string TextsPath = "Contents.txt";
        #region Before
        private static IEnumerable<string> OpenTexts()
        {
            var list = new List<string>();
            if (!System.IO.File.Exists(TextsPath)) return list;

            string source = new System.IO.StreamReader(TextsPath).ReadToEnd();
            var line = System.Text.RegularExpressions.Regex.Split(source, @"\s+");
            var enc = Encoding.UTF8;
            foreach (string s in line)
                try
                {
                    list.Add(enc.GetString(Convert.FromBase64String(s)));
                }
                catch { }
            return list;
            //return line.Select(p => enc.GetString(Convert.FromBase64String(p)));
        }

        private static void SaveTexts(IEnumerable<string> values)
        {
            var enc = Encoding.UTF8;
            var sb = new StringBuilder((int)(values.Sum(p => p.Length) * 1.4));
            foreach (string s in values)
                sb.AppendLine(Convert.ToBase64String(enc.GetBytes(s), Base64FormattingOptions.None));

            using (var writer = new System.IO.StreamWriter(TextsPath))
                writer.Write(sb.ToString());
        }
        #endregion
        // なんということをしてくれたのでしょう
        // 匠の手により、簡潔だった記述がこれほど複雑に！！↓

        #region After
        private static IEnumerable<ClipItem> OpenContents()
        {
            var list = new List<ClipItem>();
            if (!System.IO.File.Exists(TextsPath)) return list;

            string source = new System.IO.StreamReader(TextsPath).ReadToEnd();
            var line = System.Text.RegularExpressions.Regex.Split(source, @"\r*\n+").TakeWhile(p => p != "");
            var enc = Encoding.UTF8;
            foreach (string s in line)
            {
                var items = s.Split(',');
                try
                {
                    list.Add(new ClipItem(DateTime.Parse(items[0]), enc.GetString(Convert.FromBase64String(items[1]))));
                }
                catch { }
            }
            return list;
        }

        private static void SaveContents(IEnumerable<ClipItem> values)
        {
            var enc = Encoding.UTF8;
            var sb = new StringBuilder((int)(values.Sum(p => p.Value.Length) * 1.5));
            foreach (ClipItem item in values)
            {
                sb.Append(item.Created.ToString());
                sb.Append(',');
                sb.AppendLine(Convert.ToBase64String(enc.GetBytes(item.Value), Base64FormattingOptions.None));
            }

            using (var writer = new System.IO.StreamWriter(TextsPath))
                writer.Write(sb.ToString());
        }

        //書いてて悲しい
        #endregion

    }

    public class ClipItem : NotificationObject
    {
        public DateTime Created { get; private set; }
        private string _value;
        public string Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    _value = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ClipItem(string value)
            : this(DateTime.Now, value)
        {
        }
        public ClipItem(DateTime created, string value)
        {
            this.Created = created;
            this.Value = value;
        }
    }
}
