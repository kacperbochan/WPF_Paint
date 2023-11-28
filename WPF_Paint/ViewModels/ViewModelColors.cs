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
using WPF_Paint.Models;
using static WPF_Paint.Models.ColorConverter;

namespace WPF_Paint.ViewModels
{
    public class ViewModelColors : INotifyPropertyChanged
    {
        #region Inputs
        private bool _isUpdatingColorSpaces = false;

        public class ColorOption
        {
            public System.Windows.Media.Color ButtonColor { get; set; }
            public string BackgroundColor { get; set; }
            public ICommand ColorCommand { get; set; }
        }

        public ObservableCollection<ColorOption> ColorOptions { get; set; } = new ObservableCollection<ColorOption>();

        private string _coloredPartName;
        public string ColoredPartName {
            get { return _coloredPartName; }
            set
            {
                _coloredPartName = value;
                OnPropertyChanged(nameof(ColoredPartName));
            }
        }

        private System.Windows.Media.Color _currentRGBColor;
        public System.Windows.Media.Color CurrentRGBColor {
            get { return _currentRGBColor; } 
            set {
                _currentRGBColor = value;
                OnPropertyChanged(nameof(CurrentRGBColor));
            } 
        }

        private System.Windows.Media.Color _oldRGBColor;
        public System.Windows.Media.Color OldRGBColor
        {
            get { return _oldRGBColor; }
            set
            {
                _oldRGBColor = value;
                OnPropertyChanged(nameof(OldRGBColor));
            }
        }

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

        private string _hexValue = "000000";
        public string HexValue {
            get { return _hexValue; }
            set
            {
                _hexValue = value;
                OnPropertyChanged(nameof(HexValue));
                UpdateOtherColorSpacesHEX();
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
                HueValue = h.ToString("F0");
                SaturationValue = (s * 100).ToString("F0");
                ValueColor = (v * 100).ToString("F0");

                // Update HEX Value
                HexValue = RgbToHex(r, g, b);

                // Convert RGB to CMYK
                RgbToCmyk(r, g, b, out double c, out double m, out double y, out double k);
                CyanValue = Math.Round(c * 100,0).ToString("F0");
                MagentaValue = Math.Round(m * 100, 0).ToString("F0");
                YellowValue = Math.Round(y * 100, 0).ToString("F0");
                BlackValue = Math.Round(k * 100, 0).ToString("F0");

                CurrentRGBColor = System.Windows.Media.Color.FromRgb((byte)r, (byte)g, (byte)b);
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
                RedValue = r.ToString("F0");
                GreenValue = g.ToString("F0");
                BlueValue = b.ToString("F0");

                // Update HEX Value
                HexValue = RgbToHex(r, g, b);

                // Convert RGB to CMYK
                RgbToCmyk(r, g, b, out double c, out double m, out double y, out double k);
                CyanValue = Math.Round(c * 100, 0).ToString("F0");
                MagentaValue = Math.Round(m * 100, 0).ToString("F0");
                YellowValue = Math.Round(y * 100, 0).ToString("F0");
                BlackValue = Math.Round(k * 100, 0).ToString("F0");

                CurrentRGBColor = System.Windows.Media.Color.FromRgb((byte)r, (byte)g, (byte)b);
            }

            _isUpdatingColorSpaces = false; // zwolnij blokadę
        }

        private void UpdateOtherColorSpacesCMYK()
        {
            if (_isUpdatingColorSpaces) return; // jeśli blokada jest aktywna, nie aktualizuj przestrzeni kolorów

            _isUpdatingColorSpaces = true; // ustaw blokadę

            if (double.TryParse(_cyanValue, out double c) && double.TryParse(_magentaValue, out double m) && double.TryParse(_yellowValue, out double y) && double.TryParse(_blackValue, out double k))
            {
                // Convert CMYK to RGB
                CmykToRgb(c / 100, m / 100, y / 100, k / 100, out int r, out int g, out int b);
                RedValue = r.ToString("F0");
                GreenValue = g.ToString("F0");
                BlueValue = b.ToString("F0");

                // Update HEX Value
                HexValue = RgbToHex(r, g, b);

                // Convert RGB to HSV
                RgbToHsv(r, g, b, out double h, out double s, out double v);
                HueValue = Math.Round(h, 0).ToString("F0");
                SaturationValue = Math.Round(s * 100, 0).ToString("F0");
                ValueColor = Math.Round(v * 100, 0).ToString("F0");

                CurrentRGBColor = System.Windows.Media.Color.FromRgb((byte)r, (byte)g, (byte)b);
            }

            _isUpdatingColorSpaces = false; // zwolnij blokadę
        }

        private void UpdateOtherColorSpacesHEX()
        {
            if (_isUpdatingColorSpaces) return; // If updating lock is active, don't update color spaces

            _isUpdatingColorSpaces = true; // Set the lock

            HexToRgb(_hexValue, out int r, out int g, out int b);
            RedValue = r.ToString();
            GreenValue = g.ToString();
            BlueValue = b.ToString();

            // Convert RGB to HSV
            RgbToHsv(r, g, b, out double h, out double s, out double v);
            HueValue = h.ToString("F0");
            SaturationValue = (s * 100).ToString("F0");
            ValueColor = (v * 100).ToString("F0");

            // Convert RGB to CMYK
            RgbToCmyk(r, g, b, out double c, out double m, out double y, out double k);
            CyanValue = Math.Round(c * 100).ToString("F0");
            MagentaValue = Math.Round(m * 100).ToString("F0");
            YellowValue = Math.Round(y * 100).ToString("F0");
            BlackValue = Math.Round(k * 100).ToString("F0");

            CurrentRGBColor = System.Windows.Media.Color.FromRgb((byte)r, (byte)g, (byte)b);

            _isUpdatingColorSpaces = false; // Release the lock
        }


        #endregion
        public ICommand NewColorCommand { get; }
        public ICommand ResetColor { get; }


        public ViewModelColors()
        {
            if (ColorSettings.Border)
            {
                OldRGBColor = ColorSettings.BorderColor;
                ColoredPartName = "Border";
            }
            else
            {
                OldRGBColor = ColorSettings.FillColor;
                ColoredPartName = "Fill";
            }
            ResetColorCommand();
            NewColorCommand = new RelayCommandA(param => SetNewColor(param));
            ResetColor = new RelayCommand(ResetColorCommand);
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.Black, BackgroundColor = "Black", ColorCommand = NewColorCommand });
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.Gray, BackgroundColor = "Gray", ColorCommand = NewColorCommand });
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.DarkRed, BackgroundColor = "DarkRed", ColorCommand = NewColorCommand });
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.Red, BackgroundColor = "Red", ColorCommand = NewColorCommand });
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.DarkOrange, BackgroundColor = "DarkOrange", ColorCommand = NewColorCommand });
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.Yellow, BackgroundColor = "Yellow", ColorCommand = NewColorCommand });
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.Green, BackgroundColor = "Green", ColorCommand = NewColorCommand });
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.Blue, BackgroundColor = "Blue", ColorCommand = NewColorCommand });
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.DarkBlue, BackgroundColor = "DarkBlue", ColorCommand = NewColorCommand });
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.Purple, BackgroundColor = "Purple", ColorCommand = NewColorCommand });

            ColorOptions.Add(new ColorOption { ButtonColor = Colors.White, BackgroundColor = "White", ColorCommand = NewColorCommand });
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.LightGray, BackgroundColor = "LightGray", ColorCommand = NewColorCommand });
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.SaddleBrown, BackgroundColor = "SaddleBrown", ColorCommand = NewColorCommand });
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.Pink, BackgroundColor = "Pink", ColorCommand = NewColorCommand });
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.Orange, BackgroundColor = "Orange", ColorCommand = NewColorCommand });
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.LightYellow, BackgroundColor = "LightYellow", ColorCommand = NewColorCommand });
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.Lime, BackgroundColor = "Lime", ColorCommand = NewColorCommand });
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.LightBlue, BackgroundColor = "LightBlue",ColorCommand = NewColorCommand });
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.Cyan, BackgroundColor = "Cyan", ColorCommand = NewColorCommand });
            ColorOptions.Add(new ColorOption { ButtonColor = Colors.Lavender, BackgroundColor = "Lavender", ColorCommand = NewColorCommand });

            UpdateOtherColorSpacesRGB();
        }

        private void ResetColorCommand()
        {
            
            RedValue = OldRGBColor.R.ToString();
            GreenValue = OldRGBColor.G.ToString();
            BlueValue = OldRGBColor.B.ToString();
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
