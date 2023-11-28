using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace WPF_Paint.Views
{
    /// <summary>
    /// Interaction logic for Histogram.xaml
    /// </summary>
    public partial class Histogram : Window
    {
        ImgHistogram ImgHistogram { get; set; }

        public Histogram(ImgHistogram histogram)
        {
            InitializeComponent();

            ImgHistogram = histogram;
        }

        private void HistogramCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(HistogramCanvas.Children);
            DrawHistogram(ImgHistogram.Histogram);
        }

        private void HistogramCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawHistogram(ImgHistogram.Histogram);
        }


        private void DrawHistogram(int[] histogramData)
        {
            HistogramCanvas.Children.Clear();
            double canvasWidth = HistogramCanvas.ActualWidth;
            double canvasHeight = HistogramCanvas.ActualHeight;
            double barWidth = canvasWidth / histogramData.Length;

            int maxValue = histogramData.Max();

            for (int i = 0; i < histogramData.Length; i++)
            {
                double barHeight = (histogramData[i] / (double)maxValue) * canvasHeight;

                Rectangle rect = new Rectangle
                {
                    Width = barWidth,
                    Height = barHeight,
                    Fill = Brushes.Black,
                    Stroke = Brushes.Black
                };

                Canvas.SetLeft(rect, i * barWidth);
                Canvas.SetBottom(rect, 0);

                HistogramCanvas.Children.Add(rect);
            }
        }
    }
}
