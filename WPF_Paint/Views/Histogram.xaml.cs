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
        private int[] currentHistogram = new int[256];

        public Histogram(ImgHistogram histogram)
        {
            InitializeComponent();

            ImgHistogram = histogram;
            currentHistogram = ImgHistogram.Histogram;
        }

        private void HistogramCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            DrawHistogram();
        }

        private void HistogramCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawHistogram();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton && ImgHistogram is not null)
            {
                if (rbHistogram.IsChecked == true)
                {
                    if (rbOryginal.IsChecked == true)
                    {
                        if (rbAvg.IsChecked == true) currentHistogram = ImgHistogram.Histogram;
                        else if (rbRed.IsChecked == true) currentHistogram = ImgHistogram.RedHistogram;
                        else if (rbGreen.IsChecked == true) currentHistogram = ImgHistogram.GreenHistogram;
                        else currentHistogram = ImgHistogram.BlueHistogram;
                    }
                    else {
                        if (rbAvg.IsChecked == true) currentHistogram = ImgHistogram.EqHistogram;
                        else if (rbRed.IsChecked == true) currentHistogram = ImgHistogram.EqRedHistogram;
                        else if (rbGreen.IsChecked == true) currentHistogram = ImgHistogram.EqGreenHistogram;
                        else currentHistogram = ImgHistogram.EqBlueHistogram;
                    }
                }
                else
                {
                    if (rbOryginal.IsChecked == true)
                    {
                        if (rbAvg.IsChecked == true) currentHistogram = ImgHistogram.Cdf;
                        else if (rbRed.IsChecked == true) currentHistogram = ImgHistogram.CdfRed;
                        else if (rbGreen.IsChecked == true) currentHistogram = ImgHistogram.CdfGreen;
                        else currentHistogram = ImgHistogram.CdfBlue;
                    }
                    else
                    {
                        if (rbAvg.IsChecked == true) currentHistogram = ImgHistogram.CdfEq;
                        else if (rbRed.IsChecked == true) currentHistogram = ImgHistogram.CdfEqRed;
                        else if (rbGreen.IsChecked == true) currentHistogram = ImgHistogram.CdfEqGreen;
                        else currentHistogram = ImgHistogram.CdfEqBlue;
                    }
                }
                DrawHistogram();
            }
        }

        private void DrawHistogram()
        {
            HistogramCanvas.Children.Clear();
            double canvasWidth = HistogramCanvas.ActualWidth;
            double canvasHeight = HistogramCanvas.ActualHeight;
            double barWidth = canvasWidth / currentHistogram.Length;

            int maxValue = currentHistogram.Max();

            for (int i = 0; i < currentHistogram.Length; i++)
            {
                double barHeight = (currentHistogram[i] / (double)maxValue) * canvasHeight;

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
