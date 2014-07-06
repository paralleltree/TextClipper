using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;

using TextClipper.Models;
using TextClipper.Plugin;

namespace TextClipper.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        /* コマンド、プロパティの定義にはそれぞれ 
         * 
         *  lvcom   : ViewModelCommand
         *  lvcomn  : ViewModelCommand(CanExecute無)
         *  llcom   : ListenerCommand(パラメータ有のコマンド)
         *  llcomn  : ListenerCommand(パラメータ有のコマンド・CanExecute無)
         *  lprop   : 変更通知プロパティ(.NET4.5ではlpropn)
         *  
         * を使用してください。
         * 
         * Modelが十分にリッチであるならコマンドにこだわる必要はありません。
         * View側のコードビハインドを使用しないMVVMパターンの実装を行う場合でも、ViewModelにメソッドを定義し、
         * LivetCallMethodActionなどから直接メソッドを呼び出してください。
         * 
         * ViewModelのコマンドを呼び出せるLivetのすべてのビヘイビア・トリガー・アクションは
         * 同様に直接ViewModelのメソッドを呼び出し可能です。
         */

        /* ViewModelからViewを操作したい場合は、View側のコードビハインド無で処理を行いたい場合は
         * Messengerプロパティからメッセージ(各種InteractionMessage)を発信する事を検討してください。
         */

        /* Modelからの変更通知などの各種イベントを受け取る場合は、PropertyChangedEventListenerや
         * CollectionChangedEventListenerを使うと便利です。各種ListenerはViewModelに定義されている
         * CompositeDisposableプロパティ(LivetCompositeDisposable型)に格納しておく事でイベント解放を容易に行えます。
         * 
         * ReactiveExtensionsなどを併用する場合は、ReactiveExtensionsのCompositeDisposableを
         * ViewModelのCompositeDisposableプロパティに格納しておくのを推奨します。
         * 
         * LivetのWindowテンプレートではViewのウィンドウが閉じる際にDataContextDisposeActionが動作するようになっており、
         * ViewModelのDisposeが呼ばれCompositeDisposableプロパティに格納されたすべてのIDisposable型のインスタンスが解放されます。
         * 
         * ViewModelを使いまわしたい時などは、ViewからDataContextDisposeActionを取り除くか、発動のタイミングをずらす事で対応可能です。
         */

        /* UIDispatcherを操作する場合は、DispatcherHelperのメソッドを操作してください。
         * UIDispatcher自体はApp.xaml.csでインスタンスを確保してあります。
         * 
         * LivetのViewModelではプロパティ変更通知(RaisePropertyChanged)やDispatcherCollectionを使ったコレクション変更通知は
         * 自動的にUIDispatcher上での通知に変換されます。変更通知に際してUIDispatcherを操作する必要はありません。
         */

        private Model model = new Model();

        public System.Collections.ObjectModel.ObservableCollection<ClipItem> ClippedTexts
        {
            get
            {
                return model.ClippedTexts;
            }
        }

        public IEnumerable<PluginInfo> Plugins { get { return model.Plugins; } }

        private bool LastWrap;

        #region InputTextCommand
        private ListenerCommand<DateTime> _InputTextCommand;

        public ListenerCommand<DateTime> InputTextCommand
        {
            get
            {
                if (_InputTextCommand == null)
                {
                    _InputTextCommand = new ListenerCommand<DateTime>(InputText, CanInputText);
                }
                return _InputTextCommand;
            }
        }

        public bool CanInputText()
        {
            return System.Windows.Clipboard.ContainsText();
        }

        public void InputText(DateTime parameter)
        {
            if (!CanInputText()) return;
            model.InputText(System.Windows.Clipboard.GetText(), parameter);
        }
        #endregion

        #region OutputTextCommand
        private ListenerCommand<DateTime> _OutputTextCommand;

        public ListenerCommand<DateTime> OutputTextCommand
        {
            get
            {
                if (_OutputTextCommand == null)
                {
                    _OutputTextCommand = new ListenerCommand<DateTime>(OutputText);
                }
                return _OutputTextCommand;
            }
        }

        public void OutputText(DateTime parameter)
        {
            model.OutputText(parameter);
        }
        #endregion

        #region RemoveTextCommand
        private ListenerCommand<DateTime> _RemoveTextCommand;

        public ListenerCommand<DateTime> RemoveTextCommand
        {
            get
            {
                if (_RemoveTextCommand == null)
                {
                    _RemoveTextCommand = new ListenerCommand<DateTime>(RemoveText, CanRemoveText);
                }
                return _RemoveTextCommand;
            }
        }

        public bool CanRemoveText()
        {
            return ClippedTexts.Count > 1;
        }

        public void RemoveText(DateTime parameter)
        {
            if (!CanRemoveText()) return;
            model.RemoveText(parameter);
        }
        #endregion

        #region EditTextCommand

        #region EditTextCommand
        private ListenerCommand<DateTime> _EditTextCommand;

        public ListenerCommand<DateTime> EditTextCommand
        {
            get
            {
                if (_EditTextCommand == null)
                {
                    _EditTextCommand = new ListenerCommand<DateTime>(EditText);
                }
                return _EditTextCommand;
            }
        }

        public void EditText(DateTime parameter)
        {
            var item = ClippedTexts.Where(p => p.Created == parameter).Single();
            var msg = Messenger.GetResponse<TransitionMessage>(
                new TransitionMessage(
                    new EditWindowViewModel()
                    {
                        Item = item.Value,
                        IsWrapping = LastWrap
                    },
                    "EditText"));

            var editvm = (EditWindowViewModel)msg.TransitionViewModel;
            LastWrap = editvm.IsWrapping;
            if (editvm.IsEdited)
                model.EditText(editvm.Item, item.Created);
        }
        #endregion

        #endregion

        #region View
        private bool _topmost = false;
        public bool TopMost
        {
            get { return _topmost; }
            set
            {
                if (_topmost != value)
                {
                    _topmost = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _showInTaskbar = true;
        public bool ShowInTaskbar
        {
            get { return _showInTaskbar; }
            set
            {
                if (_showInTaskbar != value)
                {
                    _showInTaskbar = value;
                    RaisePropertyChanged();
                }
            }
        }


        #region MinimizeCommand
        private ViewModelCommand _MinimizeCommand;

        public ViewModelCommand MinimizeCommand
        {
            get
            {
                if (_MinimizeCommand == null)
                {
                    _MinimizeCommand = new ViewModelCommand(Minimize, CanMinimize);
                }
                return _MinimizeCommand;
            }
        }

        public bool CanMinimize()
        {
            return ShowInTaskbar == true;
        }

        public void Minimize()
        {
            if (CanMinimize())
            {
                Messenger.Raise(new WindowActionMessage(WindowAction.Minimize, "Minimize"));
            }
        }
        #endregion

        #endregion

        public void Initialize()
        {
            model.Initialize();
            RaisePropertyChanged("ClippedTexts");
            RaisePropertyChanged("Plugins");
        }

        protected override void Dispose(bool disposing)
        {
            model.Exit();
            base.Dispose(disposing);
        }
    }
}
