using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static WPF_Paint.HelperMethods;

namespace WPF_Paint.ViewModels
{
    internal class ViewModelBase : INotifyPropertyChanged
    {

        private Canvas _mainCanvas;
        

        public Canvas MainCanvas
        {
            get { return _mainCanvas; }
            set
            {
                _mainCanvas = value;
                OnPropertyChanged(nameof(MainCanvas));
            }
        }

        private enum DrawingMode
        {
            None,
            Shape,
            Draw,
            Text
        }
        private enum ShapeType 
        { 
            Triangle, 
            Rectangle, 
            Ellipse, 
            Line 
        }

        private System.Windows.Shapes.Path _currentShape;
        private Point _startPosition, _endPosition;
        private DrawingMode _currentDrawingMode = DrawingMode.None;
        private TextBox? activeTextBox;
        private ShapeType _currentShapeType, _nextShapeType = ShapeType.Ellipse;
        private bool _isMousePressed = false;

        private Point _currentPoint;

        public ICommand MouseLeftDownCommand  { get; }
        public ICommand MouseUpCommand { get; }
        public ICommand MouseMoveCommand { get; }
        
        public ICommand KeyDownCommand { get; }
        public ICommand KeyUpCommand { get; }

        public ICommand RectangleCommand { get; }
        public ICommand TriangleCommand { get; }
        public ICommand EllipseCommand { get; }
        public ICommand LineCommand { get; }

        public ICommand DrawCommand { get; }

        public ICommand TextCommand { get; }

        public ICommand SaveCommand { get; }
        
        public ICommand FillColorCommand { get; }
        public ICommand BorderColorCommand { get; }

        public ICommand OpenFillingColorSelectorCommand { get; }
        public ICommand OpenBorderColorSelectorCommand { get; }

        private void ExecuteSaveCommand()
        {
            SaveCanvasToJpg();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ViewModelBase()
        {
            MouseLeftDownCommand = new RelayCommand<Point>(Canvas_MouseLeftButtonDown); 
            MouseUpCommand = new RelayCommand<Point>(Canvas_MouseLeftButtonUp);
            MouseMoveCommand = new RelayCommand<Point>(Canvas_MouseMove);
            KeyDownCommand = new RelayCommand<KeyEventArgs>(Canvas_KeyDown);
            KeyUpCommand = new RelayCommand<KeyEventArgs>(Canvas_KeyUp);

            RectangleCommand = new RelayCommand(() => ChooseShape(1));
            TriangleCommand = new RelayCommand(() => ChooseShape(0));
            EllipseCommand = new RelayCommand(() => ChooseShape(2));
            LineCommand = new RelayCommand(() => ChooseShape(3));
            DrawCommand = new RelayCommand(DrawButton_Click);
            TextCommand = new RelayCommand(TextButton_Click);

            SaveCommand = new RelayCommand(ExecuteSaveCommand);

            FillColorCommand = new RelayCommand(ExecuteSaveCommand);
            BorderColorCommand = new RelayCommand(ExecuteSaveCommand);
            OpenFillingColorSelectorCommand = new RelayCommand(() => OpenColorSelector(0));
            OpenBorderColorSelectorCommand = new RelayCommand(() => OpenColorSelector(1));

            ColorSettings.StaticPropertyChanged += ColorSettings_StaticPropertyChanged;
        }

        private void ColorSettings_StaticPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Check which property changed and react accordingly
            if (e.PropertyName == nameof(ColorSettings.FillColor))
            {
                // Update the ViewModel's property that is bound to the view
                OnPropertyChanged(nameof(FillColorProperty));
            }
            if (e.PropertyName == nameof(ColorSettings.BorderColor))
            {
                // Update the ViewModel's property that is bound to the view
                OnPropertyChanged(nameof(FillColorProperty));
            }
            // Repeat for other properties...
        }

        // Example property that reflects the static property
        public Color FillColorProperty
        {
            get => ColorSettings.FillColor;
            set => ColorSettings.FillColor = value; // This will trigger the static property changed event
        }

        // Example property that reflects the static property
        public Color BorderColorProperty
        {
            get => ColorSettings.BorderColor;
            set => ColorSettings.BorderColor = value; // This will trigger the static property changed event
        }

        // Make sure to unsubscribe when the ViewModel is being destroyed
        public void Dispose()
        {
            ColorSettings.StaticPropertyChanged -= ColorSettings_StaticPropertyChanged;
        }


        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OpenColorSelector(int border)
        {
            ColorSettings.Border = border==1;
            
            ColorSelector colorSelectorWindow = new ColorSelector();
            colorSelectorWindow.ShowDialog();
        }


        /// <summary>
        /// Wywoływane gdy urzytkownik przyciśnie klawisz myszy nad Canvas
        /// </summary>
        /// <param name="point"></param>
        private void Canvas_MouseLeftButtonDown(Point point)
        {
            _isMousePressed = true;

            _currentPoint = point;
            _startPosition = point;
            _endPosition = point;

            switch (_currentDrawingMode)
            {
                case DrawingMode.Shape:
                    // Obsługa rysowania kształtów
                    StartDrawingShape();
                    break;    
                case DrawingMode.Draw:
                    // Obsługa rysowania linii
                    Mouse.Capture(MainCanvas);
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
                        AddTextBox();
                    }
                    break;
            }
        }

        private void Canvas_MouseLeftButtonUp(Point point)
        {
            _isMousePressed = false;

            _currentPoint = point;

            if (_currentDrawingMode == DrawingMode.Shape)
            {
                UpdateShape();
            }

            Mouse.Capture(null);
        }

        private void Canvas_MouseMove(Point point)
        {
            if (_isMousePressed)
            {
                _currentPoint = point;
                _endPosition = point;
                switch (_currentDrawingMode)
                {
                    case DrawingMode.Shape:
                        // Obsługa rysowania prostokątów
                        UpdateShape();
                        break;
                    case DrawingMode.Draw:
                        // Obsługa rysowania linii
                        ContinueDrawing();
                        break;
                        // Dodaj inne przypadki dla innych trybów rysowania
                }
            }
        }

        private void Canvas_KeyDown(KeyEventArgs e)
        {
            if (_currentDrawingMode == DrawingMode.Shape && MainCanvas.Children.Count != 0)
            {
                KeyUpdate(e);
            }
        }

        private void Canvas_KeyUp(KeyEventArgs e) { }

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

        private void DrawButton_Click()
        {
            ChangeDrawingMode(DrawingMode.Draw);
        }

        private void ChooseShape(int number)
        {
         switch(number)
            {
                case 0:
                    _nextShapeType = ShapeType.Triangle;
                    break;
                    case 1:
                    _nextShapeType = ShapeType.Rectangle;
                    break;
                    case 2:
                    _nextShapeType = ShapeType.Ellipse;
                    break;
                    case 3:
                    _nextShapeType = ShapeType.Line;
                    break;
            }       

            ChangeDrawingMode(DrawingMode.Shape);
        }

        private void TextButton_Click()
        {
            ChangeDrawingMode(DrawingMode.Text);
        }
        private void SafeButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeDrawingMode(DrawingMode.None);
            SaveCanvasToJpg();
        }

        //MouseMove
        //aktualizuje linię przy poruszaniu myszą,
        //przed puszczeniem jej lewego przycisku
        private void ContinueDrawing()
        {
            if (_isMousePressed)
            {
                Line line = new Line
                {
                    Stroke = Brushes.Black,
                    X1 = _startPosition.X,
                    Y1 = _startPosition.Y,
                    X2 = _currentPoint.X,
                    Y2 = _currentPoint.Y
                };

                MainCanvas.Children.Add(line);
                _startPosition = _currentPoint;
            }
        }

        private void AddTextBox()
        {
            TextBox textBox = new TextBox
            {
                Width = 700,
                Height = 300,
                Foreground = Brushes.Black,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                FontSize = 12,
                AcceptsReturn = true,
            };

            // Ustaw pozycję tekstu
            textBox.SetValue(Canvas.LeftProperty, _currentPoint.X);
            textBox.SetValue(Canvas.TopProperty, _currentPoint.Y);

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

        private void SaveCanvasToJpg()
        {
            Rect rect = new Rect(90, 0, MainCanvas.ActualWidth, MainCanvas.ActualHeight);
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)rect.Right, (int)rect.Bottom, 96d, 96d, System.Windows.Media.PixelFormats.Default);
            rtb.Render(MainCanvas);

            // Określ prostokąt, który chcesz zachować (x, y, width, height)
            Int32Rect cropRect = new Int32Rect(90, 0, (int)(rtb.PixelWidth - 90), (int)rtb.PixelHeight);

            // Utwórz CroppedBitmap na podstawie RenderTargetBitmap i prostokąta
            CroppedBitmap croppedBitmap = new CroppedBitmap(rtb, cropRect);

            // Kod zapisywania pliku pozostaje bez zmian

            BitmapEncoder jpgEncoder = new JpegBitmapEncoder();
            jpgEncoder.Frames.Add(BitmapFrame.Create(croppedBitmap));


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
        private void StartDrawingShape()
        {
            _currentShape = new System.Windows.Shapes.Path
            {
                Fill = new SolidColorBrush(ColorSettings.FillColor),
                Stroke = new SolidColorBrush(ColorSettings.BorderColor),
                StrokeThickness = 2
            };
            
            _currentShapeType = _nextShapeType;

            MainCanvas.Children.Add(_currentShape);
            Mouse.Capture(MainCanvas);
        }

        /// <summary>
        /// Nanosi nowo wprowadzone zmiany na rysowany kształt
        /// </summary>
        /// <param name="shape"></param>
        private void UpdateShape()
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

            _currentShape.Data = geometry;
        }

        /// <summary>
        /// Uruchamiana gdy urzytkownik naciśnie klawisz klawiatury
        /// Służy do wybierania kszrałtu, skalowania figury, poruszania figury
        /// </summary>
        private void KeyUpdate( KeyEventArgs e)
        {
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

                UpdateShape();
                MainCanvas.Children[MainCanvas.Children.Count - 1] = _currentShape;
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
