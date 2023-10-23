using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WPF_Paint
{
    static class HelperMethods
    {
        #region Shapes
        private static T CreateShape<T>() where T : Shape, new()
        {
            return new T
            {
                Fill = Brushes.LightBlue,
                Stroke = Brushes.Blue,
                StrokeThickness = 2
            };
        }

        public static System.Windows.Shapes.Path CreateRectangle() => CreateShape<System.Windows.Shapes.Path>();

        public static System.Windows.Shapes.Path CreateEllipse() => CreateShape<System.Windows.Shapes.Path>();

        public static System.Windows.Shapes.Path CreateTriangle() => CreateShape<System.Windows.Shapes.Path>();

        public static System.Windows.Shapes.Path CreateLine() => CreateShape<System.Windows.Shapes.Path>();
        #endregion

        #region Keys
        public static bool IsArrowKey(Key key)
        {
            return key == Key.Up || key == Key.Down || key == Key.Left || key == Key.Right;
        }

        public static bool IsControlPressed()
        {
            return (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
        }

        public static bool IsShiftPressed()
        {
            return (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
        }
        #endregion

    }
}
