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
    public partial class BinarizationEntropyView : Window
    {
        private Canvas Canvas;
        private WriteableBitmap writableBitmap;
        public byte[] OriginalGrayScale;
        public byte[] BufforImage;

        private ImgHistogram histogram;

        public int[] valueMapping = new int[256];

        public int width = 0;
        public int height = 0;

        private long pixelAmount;

        private byte finalThreshold = 0;

        public BinarizationEntropyView(BitmapSource source, Canvas canvas)
        {
            histogram = new ImgHistogram(source);
            width = source.PixelWidth;
            height = source.PixelHeight;
            BufforImage = new byte[width * height];

            pixelAmount = width * height; // Total number of pixels for the channel

            GetGrayScale(source);
            writableBitmap = new WriteableBitmap(source);

            Canvas = canvas;

            InitializeComponent();

            GetValueMapping();
            MapTheValue();
            ReplaceImage();
            thresholdSlider.Value = finalThreshold;
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

        public static byte CalculateThreshold(int[] histogram, long totalPixels)
        {
            double[] normalizedHistogram = new double[histogram.Length];
            for (int i = 0; i < histogram.Length; i++)
            {
                normalizedHistogram[i] = (double)histogram[i] / totalPixels;
            }

            double maxEntropy = -1;
            int threshold = 0;

            for (int t = 0; t < histogram.Length; t++)
            {
                double entropyBackground = 0, entropyForeground = 0;
                double sumBackground = 0, sumForeground = 0;

                // Calculate the sum of histogram values for background and foreground
                for (int i = 0; i <= t; i++) sumBackground += normalizedHistogram[i];
                for (int i = t + 1; i < histogram.Length; i++) sumForeground += normalizedHistogram[i];

                // Calculate the entropy for background and foreground
                for (int i = 0; i <= t; i++)
                {
                    if (normalizedHistogram[i] > 0)
                        entropyBackground -= (normalizedHistogram[i] / sumBackground) * Math.Log(normalizedHistogram[i] / sumBackground);
                }

                for (int i = t + 1; i < histogram.Length; i++)
                {
                    if (normalizedHistogram[i] > 0)
                        entropyForeground -= (normalizedHistogram[i] / sumForeground) * Math.Log(normalizedHistogram[i] / sumForeground);
                }

                // Check if this is the maximum entropy so far
                if (entropyBackground + entropyForeground > maxEntropy)
                {
                    maxEntropy = entropyBackground + entropyForeground;
                    threshold = t;
                }
            }

            return (byte)threshold;
        }

        private void GetValueMapping()
        {
            byte threshold = CalculateThreshold(histogram.Histogram, pixelAmount);


            finalThreshold = threshold;

            for (int i = 0; i < threshold; i++)
            {
                valueMapping[i] = 0;
            }
            for (int i = threshold; i < 256; i++)
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
