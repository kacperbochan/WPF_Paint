﻿using System;
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
            DrawHistogram(ImgHistogram.Histogram, Brushes.Black);
        }

        private void HistogramCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawHistogram(ImgHistogram.Histogram, Brushes.Black);
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton && ImgHistogram is not null)
            {
                // Tu wykonaj działanie po wyborze przycisku radio
                string selectedColor = radioButton.Content.ToString();
                switch(selectedColor) {
                    case "Średnia":
                        DrawHistogram(ImgHistogram.Histogram, Brushes.Black);
                        break;
                    case "Czerwony":
                        DrawHistogram(ImgHistogram.RedHistogram, Brushes.Red);
                        break;
                    case "Zielony":
                        DrawHistogram(ImgHistogram.GreenHistogram, Brushes.Green);
                        break;
                    case "Niebieski":
                        DrawHistogram(ImgHistogram.BlueHistogram, Brushes.Blue);
                        break;
                    case "Dystrybuanta":
                        DrawHistogram(ImgHistogram.CDF, Brushes.Black);
                        break;
                    case "Wyrównany":
                        DrawHistogram(ImgHistogram.EqHistogram, Brushes.Black);
                        break;
                }
            }
        }

        private void DrawHistogram(int[] histogramData, Brush brush)
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
                    Fill = brush,
                    Stroke = brush
                };

                Canvas.SetLeft(rect, i * barWidth);
                Canvas.SetBottom(rect, 0);

                HistogramCanvas.Children.Add(rect);
            }
        }
    }
}
