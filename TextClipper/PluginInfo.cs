using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;

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
    }
}
