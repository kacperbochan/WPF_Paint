using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using static WPF_Paint.ColorConverter;

namespace WPF_Paint.ViewModels
{
    public class ViewModelColors : INotifyPropertyChanged
    {
        #region Inputs
        private bool _isUpdatingColorSpaces = false;

        public class ColorOption
        {
            public System.Windows.Media.Color ButtonColor { get; set; }
            public ICommand ColorCommand { get; set; }
        }

        public ObservableCollection<ColorOption> ColorOptions { get; set; } = new ObservableCollection<ColorOption>();



        private string _redValue = "0";
        public string RedValue
        {
            get { return _redValue; }
            set
            {
                _redValue = value;
                OnPropertyChanged(nameof(RedValue));
                UpdateOtherColorSpacesRGB();
            }
        }

        private string _greenValue = "0";
        public string GreenValue
        {
            get { return _greenValue; }
            set
            {
                _greenValue = value;
                OnPropertyChanged(nameof(GreenValue));
                UpdateOtherColorSpacesRGB();
            }
        }

        private string _blueValue = "0";
        public string BlueValue
        {
            get { return _blueValue; }
            set
            {
                _blueValue = value;
                OnPropertyChanged(nameof(BlueValue));
                UpdateOtherColorSpacesRGB();
            }
        }

        private string _hueValue = "0";
        public string HueValue
        {
            get { return _hueValue; }
            set
            {
                _hueValue = value;
                OnPropertyChanged(nameof(HueValue));
                UpdateOtherColorSpacesHSV();
            }
        }

        private string _saturationValue = "0";
        public string SaturationValue
        {
            get { return _saturationValue; }
            set
            {
                _saturationValue = value;
                OnPropertyChanged(nameof(SaturationValue));
                UpdateOtherColorSpacesHSV();
            }
        }

        private string _valueColor = "0";
        public string ValueColor
        {
            get { return _valueColor; }
            set
            {
                _valueColor = value;
                OnPropertyChanged(nameof(ValueColor));
                UpdateOtherColorSpacesHSV();
            }
        }

        private string _cyanValue = "0";
        public string CyanValue
        {
            get { return _cyanValue; }
            set
            {
                _cyanValue = value;
                OnPropertyChanged(nameof(CyanValue));
                UpdateOtherColorSpacesCMYK();
            }
        }

        private string _magentaValue = "0";
        public string MagentaValue
        {
            get { return _magentaValue; }
            set
            {
                _magentaValue = value;
                OnPropertyChanged(nameof(MagentaValue));
                UpdateOtherColorSpacesCMYK();
            }
        }

        private string _yellowValue = "0";
        public string YellowValue
        {
            get { return _yellowValue; }
            set
            {
                _yellowValue = value;
                OnPropertyChanged(nameof(YellowValue));
                UpdateOtherColorSpacesCMYK();
            }
        }

        private string _blackValue = "100";
        public string BlackValue
        {
            get { return _blackValue; }
            set
            {
                _blackValue = value;
                OnPropertyChanged(nameof(BlackValue));
                UpdateOtherColorSpacesCMYK();
            }
        }
        #endregion

        #region UpdateConversions
        private void UpdateOtherColorSpacesRGB()
        {
            if (_isUpdatingColorSpaces) return; // jeśli blokada jest aktywna, nie aktualizuj przestrzeni kolorów

            _isUpdatingColorSpaces = true; // ustaw blokadę

            if (int.TryParse(_redValue, out int r) && int.TryParse(_greenValue, out int g) && int.TryParse(_blueValue, out int b))
            {
                // Convert RGB to HSV
                RgbToHsv(r, g, b, out double h, out double s, out double v);
                HueValue = h.ToString();
                SaturationValue = (s * 100).ToString();
                ValueColor = (v * 100).ToString();

                // Convert RGB to CMYK
                RgbToCmyk(r, g, b, out double c, out double m, out double y, out double k);
                CyanValue = (c * 100).ToString();
                MagentaValue = (m * 100).ToString();
                YellowValue = (y * 100).ToString();
                BlackValue = (k * 100).ToString();
            }

            _isUpdatingColorSpaces = false; // zwolnij blokadę
        }

        private void UpdateOtherColorSpacesHSV()
        {
            if (_isUpdatingColorSpaces) return; // jeśli blokada jest aktywna, nie aktualizuj przestrzeni kolorów

            _isUpdatingColorSpaces = true; // ustaw blokadę

            if (double.TryParse(_hueValue, out double h) && double.TryParse(_saturationValue, out double s) && double.TryParse(_valueColor, out double v))
            {
                // Convert HSV to RGB
                HsvToRgb(h, s / 100, v / 100, out int r, out int g, out int b);
                RedValue = r.ToString();
                GreenValue = g.ToString();
                BlueValue = b.ToString();


                // Convert RGB to CMYK
                RgbToCmyk(r, g, b, out double c, out double m, out double y, out double k);
                CyanValue = (c * 100).ToString();
                MagentaValue = (m * 100).ToString();
                YellowValue = (y * 100).ToString();
                BlackValue = (k * 100).ToString();
            }

            _isUpdatingColorSpaces = false; // zwolnij blokadę
        }

        private void UpdateOtherColorSpacesCMYK()
        {
            if (_isUpdatingColorSpaces) return; // jeśli blokada jest aktywna, nie aktualizuj przestrzeni kolorów

            _isUpdatingColorSpaces = true; // ustaw blokadę

            if (double.TryParse(_redValue, out double c) && double.TryParse(_greenValue, out double m) && double.TryParse(_blueValue, out double y) && double.TryParse(_blueValue, out double k))
            {
                // Convert CMYK to RGB
                CmykToRgb(c / 100, m / 100, y / 100, k / 100, out int r, out int g, out int b);
                RedValue = r.ToString();
                GreenValue = g.ToString();
                BlueValue = b.ToString();


                // Convert RGB to HSV
                RgbToHsv(r, g, b, out double h, out double s, out double v);
                HueValue = h.ToString();
                SaturationValue = (s * 100).ToString();
                ValueColor = (v * 100).ToString();
            }

            _isUpdatingColorSpaces = false; // zwolnij blokadę
        }

        #endregion
        public ICommand NewColorCommand { get; }


        public ViewModelColors()
        {
            NewColorCommand = new RelayCommandA(param => SetNewColor(param));
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.Black, ColorCommand = NewColorCommand });
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.Gray, ColorCommand = NewColorCommand });
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.DarkRed, ColorCommand = NewColorCommand });
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.Red, ColorCommand = NewColorCommand });
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.DarkOrange, ColorCommand = NewColorCommand });
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.Yellow, ColorCommand = NewColorCommand });
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.Green, ColorCommand = NewColorCommand });
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.Blue, ColorCommand = NewColorCommand });
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.DarkBlue, ColorCommand = NewColorCommand });
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.Purple, ColorCommand = NewColorCommand });

            ColorOptions.Add(new ColorOption { ButtonColor = Colors.White, ColorCommand = NewColorCommand });
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.LightGray, ColorCommand = NewColorCommand });
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.SaddleBrown, ColorCommand = NewColorCommand });
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.Pink, ColorCommand = NewColorCommand });
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.Orange, ColorCommand = NewColorCommand });
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.LightYellow, ColorCommand = NewColorCommand });
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.Lime, ColorCommand = NewColorCommand });
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.LightBlue, ColorCommand = NewColorCommand });
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.Cyan, ColorCommand = NewColorCommand });
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.Lavender, ColorCommand = NewColorCommand });

        }

        private void SetNewColor(object param)
        {
            if (param is System.Windows.Media.Color color)
            {
                // Set RGB values
                RedValue = color.R.ToString();
                GreenValue = color.G.ToString();
                BlueValue = color.B.ToString();
            }
        }

        public delegate void ColorSelectedHandler(System.Windows.Media.Color color);

        public event ColorSelectedHandler ColorSelected;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void OnColorSelected(System.Windows.Media.Color color)
        {
            ColorSelected?.Invoke(color);
        }
    }



}
