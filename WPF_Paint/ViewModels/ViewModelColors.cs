using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WPF_Paint.ColorConverter;

namespace WPF_Paint.ViewModels
{
    public class ViewModelColors : INotifyPropertyChanged
    {
        #region Inputs
        private bool _isUpdatingColorSpaces = false;


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

        private string _blackValue = "0";
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
                SaturationValue = s.ToString();
                ValueColor = v.ToString();

                // Convert RGB to CMYK
                RgbToCmyk(r, g, b, out double c, out double m, out double y, out double k);
                CyanValue = c.ToString();
                MagentaValue = m.ToString();
                YellowValue = y.ToString();
                BlackValue = k.ToString();
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
                HsvToRgb(h, s, v, out int r, out int g, out int b);
                RedValue = r.ToString();
                GreenValue = g.ToString();
                BlueValue = b.ToString();


                // Convert RGB to CMYK
                RgbToCmyk(r, g, b, out double c, out double m, out double y, out double k);
                CyanValue = c.ToString();
                MagentaValue = m.ToString();
                YellowValue = y.ToString();
                BlackValue = k.ToString();
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
                CmykToRgb(c, m, y, k, out int r, out int g, out int b);
                RedValue = r.ToString();
                GreenValue = g.ToString();
                BlueValue = b.ToString();


                // Convert RGB to HSV
                RgbToHsv(r, g, b, out double h, out double s, out double v);
                HueValue = h.ToString();
                SaturationValue = s.ToString();
                ValueColor = v.ToString();
            }

            _isUpdatingColorSpaces = false; // zwolnij blokadę
        }

        #endregion

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
