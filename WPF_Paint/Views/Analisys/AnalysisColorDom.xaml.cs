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
    public partial class AnalysisColorDom : Window
    {
        private AnalysisHelper _analisysHelper { get; set; }
        private bool finishedLoading = false;
        private bool showBiggest = false;

        public AnalysisColorDom(AnalysisHelper helper)
        {
            _analisysHelper = helper;
            InitializeComponent();
            finishedLoading = true;
            UpdateVisualisation();


        }

        private void ColorDom_Changed(object sender, RoutedEventArgs e)
        {
            if (finishedLoading)
            {
                UpdateVisualisation();
            }
        }

        private void UpdateVisualisation()
        {
            int buffer = (int)Math.Round(DomSlider.Value);
            DomValue.Content = buffer.ToString();

            int color = (RedButton.IsChecked == true) ? 0 : (GreenButton.IsChecked == true) ? 1 : 2;

            _analisysHelper.SetBufferColorDom(color, buffer, showBiggest);

            RedPercent.Text = Math.Round(_analisysHelper.RedPercent, 2).ToString();
            GreenPercent.Text = Math.Round(_analisysHelper.GreenPercent, 2).ToString();
            BluePercent.Text = Math.Round(_analisysHelper.BluePercent, 2).ToString();

            _analisysHelper.ReplaceImage();
        }

        private void ShowBiggest_Checked(object sender, RoutedEventArgs e)
        {
            showBiggest = !showBiggest;
            UpdateVisualisation();
        }
    }
}
