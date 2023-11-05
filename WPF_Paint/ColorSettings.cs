using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WPF_Paint
{
    public static class ColorSettings
    {
        private static Color _fillColor = Color.FromRgb(0, 0, 255);
        private static Color _borderColor = Color.FromRgb(0, 0, 0);
        private static bool _border = false;

        public static Color FillColor
        {
            get => _fillColor;
            set
            {
                if (_fillColor != value)
                {
                    _fillColor = value;
                    OnStaticPropertyChanged(nameof(FillColor));
                }
            }
        }

        public static Color BorderColor
        {
            get => _borderColor;
            set
            {
                if (_borderColor != value)
                {
                    _borderColor = value;
                    OnStaticPropertyChanged(nameof(BorderColor));
                }
            }
        }

        public static bool Border
        {
            get => _border;
            set
            {
                if (_border != value)
                {
                    _border = value;
                    OnStaticPropertyChanged(nameof(Border));
                }
            }
        }

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        private static void OnStaticPropertyChanged(string propertyName)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }
    }

}
