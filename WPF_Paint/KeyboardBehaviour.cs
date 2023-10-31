using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows;

namespace WPF_Paint
{
    public class KeyboardBehaviour : Behavior<UIElement>
    {
        protected override void OnAttached()
        {
            AssociatedObject.KeyDown += AssociatedObject_KeyDown;
            AssociatedObject.KeyUp += AssociatedObject_KeyUp;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.KeyDown -= AssociatedObject_KeyDown;
            AssociatedObject.KeyUp -= AssociatedObject_KeyUp;
        }

        private void AssociatedObject_KeyDown(object sender, KeyEventArgs e)
        {
            KeyDownCommand?.Execute(e);
        }

        private void AssociatedObject_KeyUp(object sender, KeyEventArgs e)
        {
            KeyUpCommand?.Execute(e);
        }

        public ICommand KeyDownCommand
        {
            get { return (ICommand)GetValue(KeyDownCommandProperty); }
            set { SetValue(KeyDownCommandProperty, value); }
        }

        public static readonly DependencyProperty KeyDownCommandProperty =
            DependencyProperty.Register(nameof(KeyDownCommand), typeof(ICommand), typeof(KeyboardBehaviour));

        public ICommand KeyUpCommand
        {
            get { return (ICommand)GetValue(KeyUpCommandProperty); }
            set { SetValue(KeyUpCommandProperty, value); }
        }

        public static readonly DependencyProperty KeyUpCommandProperty =
            DependencyProperty.Register(nameof(KeyUpCommand), typeof(ICommand), typeof(KeyboardBehaviour));
    }

}
