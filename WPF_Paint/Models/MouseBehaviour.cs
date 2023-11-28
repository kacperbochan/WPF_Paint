using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows;

namespace WPF_Paint.Models
{
    public class MouseBehaviour : Behavior<UIElement>
    {
        protected override void OnAttached()
        {
            AssociatedObject.MouseDown += AssociatedObject_MouseDown;
            AssociatedObject.MouseUp += AssociatedObject_MouseUp;
            AssociatedObject.MouseMove += AssociatedObject_MouseMove;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
            AssociatedObject.MouseUp -= AssociatedObject_MouseUp;
            AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
        }

        private void AssociatedObject_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(AssociatedObject);
            MouseDownCommand?.Execute(position);
        }

        private void AssociatedObject_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(AssociatedObject);
            MouseUpCommand?.Execute(position);
        }

        private void AssociatedObject_MouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(AssociatedObject);
            MouseMoveCommand?.Execute(position);
        }

        public ICommand MouseDownCommand
        {
            get { return (ICommand)GetValue(MouseDownCommandProperty); }
            set { SetValue(MouseDownCommandProperty, value); }
        }

        public static readonly DependencyProperty MouseDownCommandProperty =
            DependencyProperty.Register(nameof(MouseDownCommand), typeof(ICommand), typeof(MouseBehaviour));

        public ICommand MouseUpCommand
        {
            get { return (ICommand)GetValue(MouseUpCommandProperty); }
            set { SetValue(MouseUpCommandProperty, value); }
        }

        public static readonly DependencyProperty MouseUpCommandProperty =
            DependencyProperty.Register(nameof(MouseUpCommand), typeof(ICommand), typeof(MouseBehaviour));

        public ICommand MouseMoveCommand
        {
            get { return (ICommand)GetValue(MouseMoveCommandProperty); }
            set { SetValue(MouseMoveCommandProperty, value); }
        }

        public static readonly DependencyProperty MouseMoveCommandProperty =
            DependencyProperty.Register(nameof(MouseMoveCommand), typeof(ICommand), typeof(MouseBehaviour));
    }

}
