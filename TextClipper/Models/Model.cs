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
        /// <summary>
        /// クリップされたテキストのコレクションです。
        /// </summary>
        public ObservableCollection<ClipItem> ClippedTexts { get { return _clippedtexts; } }

        /// <summary>
        /// 読み込まれているプラグインのコレクションです。
        /// </summary>
        private DispatcherCollection<PluginInfo> _plugins;
        public DispatcherCollection<PluginInfo> Plugins { get { return _plugins; } }


        private Model()
        {
            var last = ContentUtil.OpenContents();
            if (last.Count() == 0)
                _clippedtexts = new ObservableCollection<ClipItem>() { new ClipItem("") };
            else
                _clippedtexts = new ObservableCollection<ClipItem>(last);

            _plugins = new DispatcherCollection<PluginInfo>(DispatcherHelper.UIDispatcher);
            foreach (PluginInfo p in PluginInfo.GetPlugins()) _plugins.Add(p);
        }

        ~Model()
        {
            foreach (PluginInfo p in Plugins) p.Plugin.Exit();
            PluginInfo.SaveOrder(Plugins);
            ContentUtil.SaveContents(ClippedTexts);
        }

        static Model _instance;
        public static Model Instance
        {
            get
            {
                if (_instance == null) _instance = new Model();
                return _instance;
            }
        }


        /// <summary>
        /// 新たにテキストを入力、または上書きする際に呼び出します。
        /// </summary>
        /// <param name="value">入力するテキスト</param>
        /// <param name="created">テキストを識別する時刻</param>
        public void InputText(string value, DateTime created)
        {
            foreach (PluginInfo p in Plugins)
                if (p.IsEnabled && p.Inputter != null)
                    value = p.Inputter.Inputting(value);
            EditText(value, created);
        }

        /// <summary>
        /// クリップされたテキストを編集する際に呼び出します。
        /// </summary>
        /// <param name="value">編集されたテキスト</param>
        /// <param name="created">テキストを識別する時刻</param>
        public void EditText(string value, DateTime created)
        {
            ClippedTexts.Where(p => p.Created == created).Single().Value = value;
            if (ClippedTexts.Last().Created == created) ClippedTexts.Add(new ClipItem(""));
        }

        /// <summary>
        /// クリップボードへテキストを書き出す際に呼び出します。
        /// </summary>
        /// <param name="created">テキストを識別する時刻</param>
        public void OutputText(DateTime created)
        {
            string value = ClippedTexts.Where(p => p.Created == created).Single().Value;
            foreach (PluginInfo p in Plugins)
                if (p.IsEnabled && p.Outputter != null)
                    value = p.Outputter.Outputting(value);
            System.Windows.Clipboard.SetText(value);
        }

        /// <summary>
        /// クリップされたテキストを削除する際に呼び出します。
        /// </summary>
        /// <param name="created">テキストを識別する時刻</param>
        public void RemoveText(DateTime created)
        {
            ClippedTexts.Remove(ClippedTexts.Where(p => p.Created == created).Single());
        }


        private static class ContentUtil
        {
            private static readonly Encoding enc = Encoding.UTF8;
            private static readonly string TextsPath = "Contents.txt";

            public static IEnumerable<ClipItem> OpenContents()
            {
                var list = new List<ClipItem>();
                if (!System.IO.File.Exists(TextsPath)) return list;

                string source = new System.IO.StreamReader(TextsPath).ReadToEnd();
                var line = System.Text.RegularExpressions.Regex.Split(source, @"\r*\n+").Where(p => p != "");
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

            public static void SaveContents(IEnumerable<ClipItem> values)
            {
                var sb = new StringBuilder(values.Sum(p => p.Value.Length) * 2 + values.Count() * 20);
                foreach (ClipItem item in values)
                    sb.AppendLine(string.Format("{0},{1}",
                        item.Created.ToString(),
                        Convert.ToBase64String(enc.GetBytes(item.Value), Base64FormattingOptions.None)));

                using (var writer = new System.IO.StreamWriter(TextsPath))
                    writer.Write(sb.ToString());
            }
        }

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
        { }

        public ClipItem(DateTime created, string value)
        {
            this.Created = created;
            this.Value = value;
        }
    }
}
