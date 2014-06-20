using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;

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
            _clippedtexts = new ObservableCollection<ClipItem>() { new ClipItem("") };


            // プラグインロード
            if (!System.IO.Directory.Exists("Plugins")) System.IO.Directory.CreateDirectory("plugins");

            //var assembly = new AssemblyCatalog(Assembly.GetExecutingAssembly());
            var extensions = new DirectoryCatalog("Plugins");
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(extensions);

            var container = new CompositionContainer(catalog);
            var plugins = container.GetExportedValues<IPlugin>();
            var list = new List<PluginInfo>();
            foreach (IPlugin p in plugins) list.Add(new PluginInfo(p));
            this._plugins = list;
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
        {
            this.Value = value;
            this.Created = DateTime.Now;
        }
        public ClipItem(DateTime created, string value)
        {
            this.Created = Created;
            this.Value = value;
        }
    }
}
