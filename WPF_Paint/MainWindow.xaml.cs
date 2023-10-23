using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static WPF_Paint.HelperMethods;

namespace WPF_Paint
{
    public partial class MainWindow : Window
    {

        private enum DrawingMode
        {
            None,
            Shape,
            Draw,
            Text,
            // Dodaj inne tryby rysowania, jeśli są potrzebne
        }

        private enum ShapeType { Triangle, Rectangle, Ellipse, Line }

        private Shape _currentShape;
        private Point _startPosition, _endPosition;
        private DrawingMode _currentDrawingMode = DrawingMode.None;
        private TextBox? activeTextBox;
        private ShapeType _currentShapeType, _nextShapeType = ShapeType.Ellipse;


        public MainWindow()
        {
            InitializeComponent();
            MainCanvas.Background = Brushes.White;
        }
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch (_currentDrawingMode)
            {
                case DrawingMode.Shape:
                    // Obsługa rysowania kształtów
                    StartDrawingShape(sender, e);
                    break;
                case DrawingMode.Draw:
                    // Obsługa rysowania linii
                    StartDrawing(sender, e);
                    break;
                case DrawingMode.Text:
                    // Obsługa dodawania tekstu
                    if (activeTextBox != null)
                    {
                        // Zamień TextBox na TextBlock
                        ReplaceTextBoxWithTextBlock(activeTextBox);
                        activeTextBox = null;
                    }
                    else
                    {
                        // Dodaj nowy TextBox
                        AddTextBox(e.GetPosition(MainCanvas));
                    }
                    break;
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(_currentDrawingMode == DrawingMode.Shape)
            {
                EndDrawingShape(sender, e);
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            switch (_currentDrawingMode)
            {
                case DrawingMode.Shape:
                    // Obsługa rysowania prostokątów
                    DragShape(sender, e);
                    break;
                case DrawingMode.Draw:
                    // Obsługa rysowania linii
                    ContinueDrawing(sender, e);
                    break;
                    // Dodaj inne przypadki dla innych trybów rysowania
            }
        }

        private void Canvas_KeyDown(object sender, KeyEventArgs e)
        {
            // Obsługa zmiany trybu rysowania
            switch (_currentDrawingMode)
            {
                case DrawingMode.Shape:
                    KeyUpdate(sender, e);
                    break;

             // Dodaj inne przypadki dla innych trybów rysowania
            }
        }

        private void ChangeDrawingMode(DrawingMode newMode)
        {
            // gdy użytkownik zmienia tryb rysowania
            // poprzez kliknięcie w przycisk zmiany trybu
            if (_currentDrawingMode == DrawingMode.Text && activeTextBox != null)
            {
                // Zamień TextBox na TextBlock
                ReplaceTextBoxWithTextBlock(activeTextBox);
                activeTextBox = null;
            }

            _currentDrawingMode = newMode;
        }

        private void DrawButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeDrawingMode(DrawingMode.Draw);
        }

        private void ShapeButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null && button.Tag is string value)
            {
                int number;
                if (int.TryParse(value, out number))
                {
                    if (number == 0)
                        _nextShapeType = ShapeType.Triangle;
                    else if (number == 1)
                        _nextShapeType = ShapeType.Rectangle;
                    else if (number == 2)
                        _nextShapeType = ShapeType.Ellipse;
                    else if (number == 3)
                        _nextShapeType = ShapeType.Line;
                }
            }

            ChangeDrawingMode(DrawingMode.Shape);
        }

        private void TextButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeDrawingMode(DrawingMode.Text);
        }
        private void SafeButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeDrawingMode(DrawingMode.None);
            SaveCanvasToJpg();
        }

        //MouseLeftButtonDown
        //pobiera lokalizacje punkpo naciśnieciu lewego przycisku myszy
        private void StartDrawing(object sender, MouseButtonEventArgs e)
        {
            _currentDrawingMode = DrawingMode.Draw;
            _startPosition = e.GetPosition(MainCanvas);
        }

        //MouseMove
        //aktualizuje linię przy poruszaniu myszą,
        //przed puszczeniem jej lewego przycisku
        private void ContinueDrawing(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Line line = new Line
                {
                    Stroke = Brushes.Black,
                    X1 = _startPosition.X,
                    Y1 = _startPosition.Y,
                    X2 = e.GetPosition(MainCanvas).X,
                    Y2 = e.GetPosition(MainCanvas).Y
                };

                MainCanvas.Children.Add(line);

                _startPosition = e.GetPosition(MainCanvas);
            }
        }

        private void AddTextBox(Point position)
        {
            TextBox textBox = new TextBox
            {
                Width = 200,
                Height = 50,
                Foreground = Brushes.Black,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                FontSize = 12,
                AcceptsReturn = true,
            };

            // Ustaw pozycję tekstu
            textBox.SetValue(Canvas.LeftProperty, position.X);
            textBox.SetValue(Canvas.TopProperty, position.Y);

            // Dodaj TextBox do Canvas
            MainCanvas.Children.Add(textBox);

            // Ustaw focus na dodanym TextBox, aby można było od razu wprowadzać tekst
            textBox.Focus();

            // Przypisz zdarzenie LostFocus do obsługi zamiany TextBox na TextBlock
            textBox.LostFocus += TextBox_LostFocus;

            // Przypisz pole tekstowe do aktywnego TextBox, aby można było później je zamienić na TextBlock
            activeTextBox = textBox;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Zamień TextBox na TextBlock
            ReplaceTextBoxWithTextBlock((TextBox)sender);
        }

        private void ReplaceTextBoxWithTextBlock(TextBox textBox)
        {
            TextBlock textBlock = new TextBlock
            {
                Width = textBox.Width,
                Height = textBox.Height,
                Foreground = Brushes.Black,
                FontSize = 12,
                Text = textBox.Text
            };

            // Ustaw pozycję tekstu
            textBlock.SetValue(Canvas.LeftProperty, Canvas.GetLeft(textBox));
            textBlock.SetValue(Canvas.TopProperty, Canvas.GetTop(textBox));

            // Usuń TextBox i dodaj TextBlock
            MainCanvas.Children.Remove(textBox);
            MainCanvas.Children.Add(textBlock);
        }

        private void SaveCanvasToJpg()
        {
            Rect rect = new Rect(0, 0, MainCanvas.ActualWidth, MainCanvas.ActualHeight);
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)rect.Right, (int)rect.Bottom, 96d, 96d, System.Windows.Media.PixelFormats.Default);
            rtb.Render(MainCanvas);

            BitmapEncoder jpgEncoder = new JpegBitmapEncoder();
            jpgEncoder.Frames.Add(BitmapFrame.Create(rtb));

            // Wybierz ścieżkę i nazwę pliku do zapisania
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "PaintImage"; // Nazwa domyślna
            dlg.DefaultExt = ".jpg"; // Rozszerzenie domyślne
            dlg.Filter = "JPEG files (.jpg)|*.jpg"; // Filtry rozszerzeń

            // Wyświetl okno dialogowe i zapisz plik, jeśli użytkownik kliknie "Zapisz"
            if (dlg.ShowDialog() == true)
            {
                using (FileStream fs = File.Open(dlg.FileName, FileMode.Create))
                {
                    jpgEncoder.Save(fs);
                }
            }
        }


        /// <summary>
        /// Uruchamiana przy przyciśnięciu lewego przycisku myszy
        /// Rozpoczyna rysowanie kształtu
        /// </summary>
        private void StartDrawingShape(object sender, MouseButtonEventArgs e)
        {
            _startPosition = e.GetPosition(MainCanvas);
            _endPosition = _startPosition;

            switch (_nextShapeType)
            {
                case ShapeType.Triangle:
                    _currentShape = CreateTriangle();
                    break;
                case ShapeType.Rectangle:
                    _currentShape = CreateRectangle();
                    break;
                case ShapeType.Ellipse:
                    _currentShape = CreateEllipse();
                    break;
                case ShapeType.Line:
                    _currentShape = CreateLine();
                    break;
            }

            _currentShapeType = _nextShapeType;

            MainCanvas.Children.Add(_currentShape);
            Mouse.Capture(MainCanvas);
        }

        /// <summary>
        /// Uruchamiana przy podniesieniu lewego przycisku myszy
        /// Kończy rysowanie kształtu
        /// </summary>
        private void EndDrawingShape(object sender, MouseButtonEventArgs e)
        {
            _endPosition = e.GetPosition(MainCanvas);
            UpdateShape(_currentShape);
            Mouse.Capture(null);
        }

        /// <summary>
        /// Uruchamiana gdy lewy przycisk myszy jest przyciśnięty, a mysz się przesunęła
        /// Rysuje kształt
        /// </summary>
        private void DragShape(object sender, MouseEventArgs e)
        {
            if (Mouse.Captured == MainCanvas && e.LeftButton == MouseButtonState.Pressed)
            {
                _endPosition = e.GetPosition(MainCanvas);
                UpdateShape(_currentShape);
            }
        }

        /// <summary>
        /// Nanosi nowo wprowadzone zmiany na rysowany kształt
        /// </summary>
        /// <param name="shape"></param>
        private void UpdateShape(Shape shape)
        {
            StreamGeometry geometry = new StreamGeometry();
            geometry.FillRule = FillRule.EvenOdd;

            using (StreamGeometryContext ctx = geometry.Open())
            {
                if (_currentShapeType == ShapeType.Ellipse)
                {
                    // Liczymy środek i promienie elipsy
                    Point center = new Point((_startPosition.X + _endPosition.X) / 2, (_startPosition.Y + _endPosition.Y) / 2);
                    double radiusX = Math.Abs(_endPosition.X - _startPosition.X) / 2;
                    double radiusY = Math.Abs(_endPosition.Y - _startPosition.Y) / 2;

                    // Zaczynamy rysować od najbardziej lewego krańca elipsy
                    ctx.BeginFigure(new Point(center.X - radiusX, center.Y), true /* is filled */, true /* is closed */);

                    // Rysuje górny łuk elipsy
                    ctx.ArcTo(new Point(center.X + radiusX, center.Y), new Size(radiusX, radiusY), 0, false, SweepDirection.Clockwise, true, false);

                    // Rysuje dolny łuk elipsy
                    ctx.ArcTo(new Point(center.X - radiusX, center.Y), new Size(radiusX, radiusY), 0, false, SweepDirection.Clockwise, true, false);
                }
                else
                {
                    ctx.BeginFigure(new Point(_startPosition.X, _startPosition.Y), true /* Wypełniony */, true /* zamknięty */);

                    if (_currentShapeType != ShapeType.Line)
                        ctx.LineTo(new Point(_startPosition.X, _endPosition.Y), true /* Widać linię */, false /* łagodne połączenie*/);

                    ctx.LineTo(new Point(_endPosition.X, _endPosition.Y), true /* Widać linię */, true /* łagodne połączenie */);

                    if (_currentShapeType == ShapeType.Rectangle)
                        ctx.LineTo(new Point(_endPosition.X, _startPosition.Y), true /* Widać linię */, false /* łagodne połączenie*/);

                }
            }
            geometry.Freeze();

            ((System.Windows.Shapes.Path)shape).Data = geometry;    
        }

        /// <summary>
        /// Uruchamiana gdy urzytkownik naciśnie klawisz klawiatury
        /// Służy do wybierania kszrałtu, skalowania figury, poruszania figury
        /// </summary>
        private void KeyUpdate(object sender, KeyEventArgs e)
        {
            //UpdateNextShapeType(e);

            if (MainCanvas.Children.Count == 0) return;

            if (IsArrowKey(e.Key))
            {
                int value = IsControlPressed() ? 1 : 5;

                if (IsShiftPressed())
                {
                    AdjustShapeSize(e.Key, value);
                }
                else
                {
                    MoveShape(e.Key, value);
                }

                UpdateShape(_currentShape);
                MainCanvas.Children[MainCanvas.Children.Count - 1] = _currentShape;
            }
        }

        /// <summary>
        /// Jeśli to konieczne zmienia kształt następnej figury
        /// </summary>
        /// <param name="e"></param>
        private void UpdateNextShapeType(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.T:
                    _nextShapeType = ShapeType.Triangle;
                    break;
                case Key.R:
                    _nextShapeType = ShapeType.Rectangle;
                    break;
                case Key.E:
                    _nextShapeType = ShapeType.Ellipse;
                    break;
                case Key.L:
                    _nextShapeType = ShapeType.Line;
                    break;

            }
        }

        /// <summary>
        /// Za pomocą inputów ze strzałek skaluje figurę
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private void AdjustShapeSize(Key key, int value)
        {
            switch (key)
            {
                case Key.Up:
                    _endPosition.Y -= value;
                    break;
                case Key.Left:
                    _endPosition.X -= value;
                    break;
                case Key.Down:
                    _endPosition.Y += value;
                    break;
                case Key.Right:
                    _endPosition.X += value;
                    break;
            }
        }

        /// <summary>
        /// Za pomocą inputów ze strzałek przesuwa figurę
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private void MoveShape(Key key, int value)
        {
            switch (key)
            {
                case Key.Up:
                    _startPosition.Y -= value;
                    _endPosition.Y -= value;
                    break;
                case Key.Left:
                    _startPosition.X -= value;
                    _endPosition.X -= value;
                    break;
                case Key.Down:
                    _startPosition.Y += value;
                    _endPosition.Y += value;
                    break;
                case Key.Right:
                    _startPosition.X += value;
                    _endPosition.X += value;
                    break;
            }
        }
    }
}
