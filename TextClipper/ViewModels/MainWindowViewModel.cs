using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;

using TextClipper.Models;
using TextClipper.Plugin;
using TextClipper.Behaviors;

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

        private Model model;

        public System.Collections.ObjectModel.ObservableCollection<ClipItem> ClippedTexts
        {
            get { return model.ClippedTexts; }
        }

        public DispatcherCollection<PluginInfo> Plugins
        {
            get { return model.Plugins; }
        }


        public void InputText(DateTime parameter)
        {
            if (!System.Windows.Clipboard.ContainsText()) return;
            model.InputText(System.Windows.Clipboard.GetText(), parameter);
        }

        public void OutputText(DateTime parameter)
        {
            model.OutputText(parameter);
        }

        public void RemoveText(DateTime parameter)
        {
            if (!(ClippedTexts.Count > 1)) return;
            model.RemoveText(parameter);
        }


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

        #region DragDrop
        private DropAcceptDescription _description;
        public DropAcceptDescription Description
        {
            get { return this._description; }
        }

        private void OnDragOver(DragEventArgs args)
        {
            if (args.AllowedEffects.HasFlag(DragDropEffects.Move) &&
                args.Data.GetDataPresent(typeof(PluginInfo)))
                args.Effects = DragDropEffects.Move;
        }
        void OnDragDrop(DragEventArgs args)
        {
            if (!args.Data.GetDataPresent(typeof(PluginInfo))) return;

            var data = args.Data.GetData(typeof(PluginInfo)) as PluginInfo;
            if (data == null) return;
            var fe = args.OriginalSource as FrameworkElement;
            if (fe == null) return;
            var target = fe.DataContext as PluginInfo;
            if (target == null) return;

            int si = Plugins.IndexOf(data);
            int di = Plugins.IndexOf(target);
            if (si < 0 || di < 0 || si == di) return;
            System.Diagnostics.Debug.WriteLine("*moved! {0} -> {1}", si, di);
            Plugins.Move(si, di);
            RaisePropertyChanged("Plugins");
        }
        #endregion

        #endregion

        public void Initialize()
        {
            model = Model.Instance;
            RaisePropertyChanged("ClippedTexts");
            RaisePropertyChanged("Plugins");
        }

        public MainWindowViewModel()
        {
            this._description = new DropAcceptDescription();
            this._description.DragOver += this.OnDragOver;
            this._description.DragDrop += this.OnDragDrop;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
