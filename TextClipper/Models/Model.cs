using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Livet;

namespace TextClipper.Models
{
    public class Model : NotificationObject
    {
        /*
         * NotificationObjectはプロパティ変更通知の仕組みを実装したオブジェクトです。
         */

        public ObservableCollection<ClipItem> Texts { get; set; }

        public void Initialize()
        {
            Texts = new ObservableCollection<ClipItem>() { new ClipItem("") };
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
