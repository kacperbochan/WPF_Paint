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

            if (RedButton.IsChecked == true)
            {
                _analisysHelper.SetBufferColorDom(0, buffer);
            }
            else if (GreenButton.IsChecked == true)
            {
                _analisysHelper.SetBufferColorDom(1, buffer);
            }
            else
            {
                _analisysHelper.SetBufferColorDom(2, buffer);
            }

            RedPercent.Text = Math.Round(_analisysHelper.RedPercent, 2).ToString();
            GreenPercent.Text = Math.Round(_analisysHelper.GreenPercent, 2).ToString();
            BluePercent.Text = Math.Round(_analisysHelper.BluePercent, 2).ToString();

            _analisysHelper.ReplaceImage();
        }
    }
}
