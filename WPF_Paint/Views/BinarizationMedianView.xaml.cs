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
    public partial class BinarizationMedianView : Window
    {
        private Canvas Canvas;
        private WriteableBitmap writableBitmap;
        public byte[] OriginalGrayScale;
        public byte[] BufforImage;

        private ImgHistogram histogram;

        public int[] valueMapping = new int[256];

        public int width = 0;
        public int height = 0;

        private long pixelSum = 0;
        private long pixelAmount = 0;

        private byte finalTreshold = 0;

        public BinarizationMedianView(BitmapSource source, Canvas canvas)
        {
            histogram = new ImgHistogram(source);
            width = source.PixelWidth;
            height = source.PixelHeight;
            BufforImage = new byte[width * height];

            pixelAmount = width * height; // Total number of pixels for the channel

            for (int i = 0; i < 256; i++)
                pixelSum += i * histogram.Histogram[i];

            GetGrayScale(source);
            writableBitmap = new WriteableBitmap(source);

            Canvas = canvas;

            InitializeComponent();

            GetValueMapping();
            MapTheValue();
            ReplaceImage();
            thresholdSlider.Value = finalTreshold;
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            // Zamknij okno
            this.Close();
        }

        private void GetGrayScale(BitmapSource source)
        {
            int stride = width * 4;

            byte[] sourcePixels = new byte[height * stride];
            source.CopyPixels(sourcePixels, stride, 0);
            OriginalGrayScale = new byte[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pixelIndex = y * stride + x * 4;
                    int grayscale = (sourcePixels[pixelIndex] + sourcePixels[pixelIndex+1] + sourcePixels[pixelIndex + 2]) / 3;
                    OriginalGrayScale[y * width + x] = (byte)grayscale;
                }
            }
        }

        private byte ComputeNewThreshold(byte oldThreshold) 
        {
            long sumBackground = 0, sumForeground = 0;
            long countBackground = 0, countForeground = 0;

            for (int i = oldThreshold+1; i < 256; i++)
            {
                sumBackground += i * histogram.Histogram[i];
                countBackground += histogram.Histogram[i];
            }
            sumForeground = pixelSum - sumBackground;
            countForeground = pixelAmount - countBackground;

            byte meanBackground = (byte)(sumBackground / Math.Max(1, countBackground));
            byte meanForeground = (byte)(sumForeground / Math.Max(1, countForeground));

            return (byte)((meanBackground + meanForeground) / 2);
        }

        private void GetValueMapping()
        {
            //initial treshold
            byte treshold = (byte)(pixelSum / pixelAmount);

            bool tresholdChanged;
            do
            {
                tresholdChanged = false;
                byte newTreshold = ComputeNewThreshold(treshold);
                if(newTreshold != treshold)
                {
                    treshold = newTreshold;
                    tresholdChanged = true;
                }
            }
            while (tresholdChanged);

            finalTreshold = treshold;

            for (int i = 0; i < treshold; i++)
            {
                valueMapping[i] = 0;
            }
            for (int i = treshold; i < 256; i++)
            {
                valueMapping[i] = 255;
            }
        }

        private void MapTheValue()
        {
            
            for (int i=0; i< OriginalGrayScale.Length; i++)
            {
                BufforImage[i] = (byte)valueMapping[OriginalGrayScale[i]];
            }
        }

        private void ReplaceImage()
        {
            int stride = width * 4; // 4 bytes per pixel in RGBA format

            byte[] sourcePixels = new byte[height * stride];
            writableBitmap.CopyPixels(sourcePixels, stride, 0);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * stride + x * 4;
                    int bufforindex = y * width + x;
                    
                    sourcePixels[index] = (byte)BufforImage[bufforindex];
                    sourcePixels[index + 1] = (byte)BufforImage[bufforindex];
                    sourcePixels[index + 2] = (byte)BufforImage[bufforindex];
                }
            }

            writableBitmap.WritePixels(new Int32Rect(0, 0, width, height), sourcePixels, stride, 0);

            Image equalImage = new Image();
            equalImage.Source = writableBitmap;

            Canvas.Children.Clear();
            Canvas.Children.Add(equalImage);

        }
    }
}
