﻿using System;
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
    /// 指定の<see cref="System.Windows.FrameworkElement"/>へのドラッグされたデータの検査、処理を行うための添付ビヘイビアを提供します。
    /// </summary>
    public sealed class AcceptDragBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(DropAcceptDescription),
            typeof(AcceptDragBehavior), new PropertyMetadata(null));

        public DropAcceptDescription Description
        {
            get { return (DropAcceptDescription)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        protected override void OnAttached()
        {
            this.AssociatedObject.PreviewDragOver += AssociatedObject_DragOver;
            this.AssociatedObject.PreviewDrop += AssociatedObject_Drop;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.PreviewDragOver -= AssociatedObject_DragOver;
            this.AssociatedObject.PreviewDrop -= AssociatedObject_Drop;
            base.OnDetaching();
        }

        void AssociatedObject_DragOver(object sender, DragEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("|DragOver");
            var desc = Description;
            if (desc == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }
            desc.OnDragOver(e);
            e.Handled = true;
        }

        void AssociatedObject_Drop(object sender, DragEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("*Drop");
            var desc = Description;
            if (desc == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }
            desc.OnDrop(e);
            e.Handled = true;
        }
    }

    public sealed class DropAcceptDescription
    {
        public event Action<DragEventArgs> DragOver;

        public void OnDragOver(DragEventArgs e)
        {
            var handler = DragOver;
            if (handler != null)
                handler(e);
        }

        public event Action<DragEventArgs> DragDrop;

        public void OnDrop(DragEventArgs e)
        {
            var handler = DragDrop;
            if (handler != null) handler(e);
        }
    }
}
