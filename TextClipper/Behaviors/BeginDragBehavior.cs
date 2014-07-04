using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace TextClipper.Behaviors
{
    /// <summary>
    /// 指定の<see cref="System.Windows.FrameworkElement"/>へのドラッグを開始するための添付ビヘイビアを提供します。
    /// </summary>
    class BeginDragBehavior : Behavior<FrameworkElement>
    {
        private Point _origin;
        private bool _isButtonDown;

        public static readonly DependencyProperty AllowedEffectsProperty =
            DependencyProperty.Register("AllowedEffects", typeof(DragDropEffects),
            typeof(BeginDragBehavior), new UIPropertyMetadata(DragDropEffects.All));

        /// <summary>
        /// データに対しての許容する操作を設定、取得します。
        /// </summary>
        public DragDropEffects AllowedEffects
        {
            get { return (DragDropEffects)GetValue(AllowedEffectsProperty); }
            set { SetValue(AllowedEffectsProperty, value); }
        }

        public static readonly DependencyProperty DragDropDataProperty =
            DependencyProperty.Register("DragDropData", typeof(object),
            typeof(BeginDragBehavior), new PropertyMetadata(null));

        /// <summary>
        /// ViewModelからバインドされた渡されるデータを設定、取得します。
        /// </summary>
        public object DragDropData
        {
            get { return GetValue(DragDropDataProperty); }
            set { SetValue(DragDropDataProperty, value); }
        }

        protected override void OnAttached()
        {
            this.AssociatedObject.PreviewMouseDown += AssociatedObject_PreviewMouseDown;
            this.AssociatedObject.PreviewMouseMove += AssociatedObject_PreviewMouseMove;
            this.AssociatedObject.PreviewMouseUp += AssociatedObject_PreviewMouseUp;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.PreviewMouseDown -= AssociatedObject_PreviewMouseDown;
            this.AssociatedObject.PreviewMouseMove -= AssociatedObject_PreviewMouseMove;
            this.AssociatedObject.PreviewMouseUp -= AssociatedObject_PreviewMouseUp;
            base.OnDetaching();
        }

        void AssociatedObject_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _origin = e.GetPosition(this.AssociatedObject);
            _isButtonDown = true;
            System.Diagnostics.Debug.WriteLine("*PreviewMouseDown");
        }

        void AssociatedObject_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("|MouseMove");
            System.Diagnostics.Debug.WriteLine(" >Mouse:{0},{1}", e.LeftButton.ToString(), _isButtonDown);
            if (e.LeftButton != MouseButtonState.Pressed || !_isButtonDown) return;
            var point = e.GetPosition(this.AssociatedObject);
            System.Diagnostics.Debug.WriteLine(" >pos: " + point.ToString());
            if (CheckDistance(point, _origin))
            {
                System.Diagnostics.Debug.WriteLine("*DoDragDrop");
                DragDrop.DoDragDrop(this.AssociatedObject, this.DragDropData, this.AllowedEffects);
                _isButtonDown = false;
                e.Handled = true;
            }
        }

        void AssociatedObject_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("*MouseUp");
            _isButtonDown = false;
        }

        private bool CheckDistance(Point x, Point y)
        {
            double dx = Math.Abs(x.X - y.X);
            double dy = Math.Abs(x.Y - y.Y);
            return dx >= SystemParameters.MinimumHorizontalDragDistance ||
               dy >= SystemParameters.MinimumVerticalDragDistance;
        }
    }
}
