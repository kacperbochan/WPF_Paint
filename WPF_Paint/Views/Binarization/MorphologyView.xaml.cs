using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WPF_Paint.Models;
using WPF_Paint.ViewModels;

namespace WPF_Paint
{
    /// <summary>
    /// Interaction logic for ColorSelector.xaml
    /// </summary>
    public partial class MorphologyView : Window
    {
        private BinarizationHelper _binarizationHelper;
        private byte _radius = 1;
        private byte _step = 1;
        private byte _type = 0;

        public MorphologyView(BinarizationHelper binarizationHelper, byte type)
        {
            _binarizationHelper = binarizationHelper;
            _type = type;

            InitializeComponent();

            _binarizationHelper.UpdateImageWithByteMap(CalculateNewBitmap());
        }

        private void RadiusSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Math.Round(e.NewValue) == _radius) return;

            _radius = (byte)Math.Round(e.NewValue);

            _binarizationHelper.UpdateImageWithByteMap(CalculateNewBitmap());
        }
        private void StepSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Math.Round(e.NewValue) == _radius) return;

            _step = (byte)Math.Round(e.NewValue);

            _binarizationHelper.UpdateImageWithByteMap(CalculateNewBitmap());
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            // Zamknij okno
            this.Close();
        }

        private byte[] CalculateErosion(byte[] inputBitmap)
        {
            byte[] bufferBitmap = new byte[inputBitmap.Length];

            for (int y = 0; y < _binarizationHelper.Height; y++)
            {
                for (int x = 0; x < _binarizationHelper.Width; x++)
                {
                    bufferBitmap[y * _binarizationHelper.Width + x] = ErosionFilter(inputBitmap, x, y);
                }
            }

            return bufferBitmap;
        }
        
        private byte ErosionFilter(byte[] inputBitmap, int x, int y) 
        {
            byte minVal = 255;

            for(int i = -_radius; i <= _radius; i++)
            {
                int offsetY = Math.Clamp(y + i, 0, _binarizationHelper.Height-1) * _binarizationHelper.Width;
                for (int j = -_radius; j <= _radius; j++)
                {
                    int offset = offsetY + Math.Clamp(x + j, 0, _binarizationHelper.Width-1);

                    if (inputBitmap[offset] < minVal)
                        minVal = inputBitmap[offset];
                }
            }
            return minVal;
        }

        private byte[] CalculateDilatation(byte[] inputBitmap)
        {
            byte[] bufferBitmap = new byte[inputBitmap.Length];

            for (int y = 0; y < _binarizationHelper.Height; y++)
            {
                for (int x = 0; x < _binarizationHelper.Width; x++)
                {
                    bufferBitmap[y * _binarizationHelper.Width + x] = DilatationFilter(inputBitmap, x, y);
                }
            }

            return bufferBitmap;
        }
        
        private byte DilatationFilter(byte[] inputBitmap, int x, int y) 
        {
            byte maxVal = 0;

            for (int i = -_radius; i <= _radius; i++)
            {
                int offsetY = Math.Clamp(y + i, 0, _binarizationHelper.Height-1) * _binarizationHelper.Width;
                for (int j = -_radius; j <= _radius; j++)
                {
                    int offset = offsetY + Math.Clamp(x + j, 0, _binarizationHelper.Width-1);

                    if (inputBitmap[offset] > maxVal)
                        maxVal = inputBitmap[offset];
                }
            }
            return maxVal;
        }

        private byte[] CalculateHitOrMiss(byte[] inputBitmap)
        {
            byte[] hit =
            {
                0,1,0,
                1,1,1,
                0,1,0
            };
            byte[] miss =
            {
                1,0,1,
                0,0,0,
                1,0,1
            };

            byte[] bufferBitmap = new byte[inputBitmap.Length];

            for (int y = 0; y < _binarizationHelper.Height; y++)
            {
                for (int x = 0; x < _binarizationHelper.Width; x++)
                {
                    bufferBitmap[y * _binarizationHelper.Width + x] = CheckHitMissFilter(inputBitmap, x, y, hit, miss);
                }
            }

            return bufferBitmap;
        }

        private byte CheckHitMissFilter(byte[] inputBitmap, int x, int y, byte[] hit, byte[] miss)
        {
            int id = 0;
            for (int i = -_radius; i <= _radius; i++)
            {
                int offsetY = Math.Clamp(y + i, 0, _binarizationHelper.Height - 1) * _binarizationHelper.Width;
                for (int j = -_radius; j <= _radius; j++)
                {
                    int offset = offsetY + Math.Clamp(x + j, 0, _binarizationHelper.Width - 1);


                    if (hit[id] == 1 && inputBitmap[offset] != 255 ||
                        miss[id] == 1 && inputBitmap[offset] != 0)
                        return 255;

                    id++;
                }
            }
            return 0;
        }

        private byte[] CalculateNewBitmap()
        {
            byte[] bufor = _binarizationHelper.GrayScale;

            for (int i = 0; i < _step; i++)
                switch (_type)
                {
                    case 0://Dilatation
                        bufor = CalculateDilatation(bufor);
                        break;
                    case 1://Erosion
                        bufor = CalculateErosion(bufor);
                        break;
                    case 2://Opening
                        bufor = CalculateErosion(bufor);
                        bufor = CalculateDilatation(bufor);
                        break;
                    case 3://Closing
                        bufor = CalculateDilatation(bufor);
                        bufor = CalculateErosion(bufor);
                        break;
                    case 4://Thinning -tbc
                        bufor = CalculateHitOrMiss(bufor);
                        break;
                    default://Thickening
                        //return CalculateErosion(_binarizationHelper.GrayScale);
                        break;
                }

            return bufor;
        }
        
    }
}
