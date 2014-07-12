using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;

namespace TextClipper.Plugin
{
    /// <summary>
    /// プラグインの情報を格納するクラスです。
    /// </summary>
    public class PluginInfo : Livet.NotificationObject
    {
        private ISupportInput _inputter;
        public ISupportInput Inputter { get { return _inputter; } }

        private ISupportOutput _outputter;
        public ISupportOutput Outputter { get { return _outputter; } }
        public IPlugin Plugin { get; set; }

        private bool _enabled;
        public bool IsEnabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    RaisePropertyChanged();
                }
            }
        }

        public PluginInfo(IPlugin plugin)
        {
            this.Plugin = plugin;
            _inputter = plugin is ISupportInput ? (ISupportInput)plugin : null;
            _outputter = plugin is ISupportOutput ? (ISupportOutput)plugin : null;
            IsEnabled = true;
        }

        #region static
        /// <summary>
        /// 存在するプラグインを取得します。
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<PluginInfo> GetPlugins()
        {
            const string PluginPath = "Plugins";
            if (!System.IO.Directory.Exists(PluginPath)) System.IO.Directory.CreateDirectory(PluginPath);

            var extensions = new DirectoryCatalog(PluginPath);
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(extensions);

            var container = new CompositionContainer(catalog);
            var plugins = container.GetExportedValues<IPlugin>();
            var list = new List<PluginInfo>();

            // 適用順に則る
            // 保存されていたリストを読んで
            var saved = OpenOrder();
            // 新たに追加されたプラグインを取得
            var diff = plugins.Select(p => p.Name).Except(saved.Select(p => p.Key));
            // とりあえず記録の最後に連結して
            var order = saved.Select(p => p.Key).Concat(diff);
            // 存在するプラグインと帳尻合わせ
            var ordered = order.Join(plugins, p => p, q => q.Name, (p, q) => q);

            foreach (IPlugin p in ordered)
                list.Add(new PluginInfo(p)
                {
                    IsEnabled = saved.SingleOrDefault(q => p.Name == q.Key).Value
                });
            return list;
        }

        private static readonly Encoding enc = Encoding.UTF8;
        private static readonly string PluginsPath = "Plugins.txt";

        /// <summary>
        /// プラグインの適用順を保存します。
        /// </summary>
        public static void SaveOrder(IEnumerable<PluginInfo> plugins)
        {
            var sb = new StringBuilder(plugins.Sum(p => p.Plugin.Name.Length) * 2);
            foreach (PluginInfo p in plugins)
                sb.AppendLine(
                    string.Join(",",
                        Convert.ToBase64String(enc.GetBytes(p.Plugin.Name), Base64FormattingOptions.None),
                        p.IsEnabled.ToString())
                );

            using (var writer = new System.IO.StreamWriter(PluginsPath))
                writer.Write(sb.ToString());
        }

        /// <summary>
        /// プラグインの適用順を示す<see cref="System.Collections.Generic.IEnumerable{KeyValuePair{string, bool}}"/> を取得します。
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<KeyValuePair<string, bool>> OpenOrder()
        {
            var list = new Dictionary<string, bool>();
            if (!System.IO.File.Exists(PluginsPath)) return list;

            string source = new System.IO.StreamReader(PluginsPath).ReadToEnd();
            var line = source.SplitByNewLine().Where(p => p != "");
            foreach (string s in line)
            {
                string[] str = s.Split(',');
                list.Add(enc.GetString(Convert.FromBase64String(str[0])), bool.Parse(str[1]));
            }
            return list;
        }

        #endregion
    }
}
