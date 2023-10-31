using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WPF_Paint
{
    static class HelperMethods
    {
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
