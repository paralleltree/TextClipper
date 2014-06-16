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

        private ObservableCollection<ClipItem> _clippedtexts;
        public ObservableCollection<ClipItem> ClippedTexts { get { return _clippedtexts; } }

        public void Initialize()
        {
            _clippedtexts = new ObservableCollection<ClipItem>() { new ClipItem("") };
        }


        public void InputText(string value, DateTime created)
        {
            ClippedTexts.Where(p => p.Created == created).Single().Value = value;
            if (ClippedTexts.Last().Created == created) ClippedTexts.Add(new ClipItem(""));
        }

        public void OutputText(DateTime created)
        {
            System.Windows.Clipboard.SetText(ClippedTexts.Where(p => p.Created == created).Single().Value);
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
