using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using WPF_Paint.Models;
using WPF_Paint.Views;
using static WPF_Paint.Models.HelperMethods;

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

        private double _zoomLevel = 1.0;

        public double ZoomLevel
        {
            get { return _zoomLevel; }
            set
            {
                _zoomLevel = value;
                OnPropertyChanged(nameof(ZoomLevel));
            }
        }

        private double _canvasWidth = 300;

        public double CanvasWidth
        {
            get { return _canvasWidth; }
            set
            {
                _canvasWidth = value;
                OnPropertyChanged(nameof(CanvasWidth));
            }
        }

        private double _canvasHeight = 200;

        public double CanvasHeight
        {
            get { return _canvasHeight; }
            set
            {
                _canvasHeight = value;
                OnPropertyChanged(nameof(CanvasHeight));
            }
        }

        private enum DrawingMode
        {
            None,
            Shape,
            Draw,
            Text,
            Polygon, 
            Point
        }
        private enum ShapeType
        {
            Triangle,
            Rectangle,
            Ellipse,
            Line
        }

        private System.Windows.Shapes.Path _currentShape;
        private Point _startPosition, _endPosition, _currentPoint;
        private DrawingMode _currentDrawingMode = DrawingMode.None;
        private TextBox? activeTextBox;
        private ShapeType _currentShapeType, _nextShapeType = ShapeType.Ellipse;
        private bool _isMousePressed = false;

        public ICommand MouseLeftDownCommand { get; }
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

        public ICommand OpenCommand { get; }
        public ICommand ImageCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand NewCanvasCommand { get; }

        public ICommand FillColorCommand { get; }
        public ICommand BorderColorCommand { get; }

        public ICommand OpenFillingColorSelectorCommand { get; }
        public ICommand OpenBorderColorSelectorCommand { get; }
        public ICommand ApplyAverageFilterCommand { get; }
        public ICommand ApplyMedianFilterCommand { get; }
        public ICommand ApplySobelEdgeDetectionCommand { get; }
        public ICommand ApplyHighPassFilterCommand { get; }
        public ICommand ApplyGaussianBlurFilterCommand { get; }
        public ICommand ApplyCustomFilterCommand { get; }

        public ICommand AddFilterCommand { get; }
        public ICommand SubtractFilterCommand { get; }
        public ICommand MultiplyFilterCommand { get; }
        public ICommand DivideFilterCommand { get; }
        public ICommand BrightnessFilterCommand { get; }
        public ICommand GrayscaleFilterCommand { get; }

        public ICommand HistogramWindowCommand { get; }
        public ICommand HistEqualizationCommand { get; }
        public ICommand BinarizationUserCommand { get; }
        public ICommand BinarizationPercentCommand { get; }
        public ICommand BinarizationMedianCommand { get; }
        public ICommand BinarizationOtsuCommand { get; }
        public ICommand BinarizationNiblackCommand { get; }
        public ICommand BinarizationBernsensCommand { get; }

        public ICommand MorphologyDilatationCommand { get; }
        public ICommand MorphologyErosionCommand { get; }
        public ICommand MorphologyOpeningCommand { get; }
        public ICommand MorphologyClosingCommand { get; }
        public ICommand MorphologyThiningCommand { get; }
        public ICommand MorphologyThickeningCommand { get; }
        public ICommand PolygonToolCommand { get; }
        public ICommand VectorCommand { get;}
        public ICommand RotateCommand { get;  }
        public ICommand ScaleCommand { get; }
        public ICommand ChoosePointCommand { get; }

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

            PolygonToolCommand = new RelayCommand(PolygonButton_Click);
            VectorCommand = new RelayCommand(SetVectorMove);
            RotateCommand = new RelayCommand(RotateButton_Click);
            ScaleCommand = new RelayCommand(SetPolygonScale);
            ChoosePointCommand = new RelayCommand(ChoosePointButton_Click);

            ImageCommand = new RelayCommand(SetImageSize);
            OpenCommand = new RelayCommand(OpenPicture);
            SaveCommand = new RelayCommand(SaveCanvas);
            NewCanvasCommand = new RelayCommand(NewCanvas);

            OpenFillingColorSelectorCommand = new RelayCommand(() => OpenColorSelector(0));
            OpenBorderColorSelectorCommand = new RelayCommand(() => OpenColorSelector(1));

            ApplyAverageFilterCommand = new RelayCommand(() => ApplyFilter(0));
            ApplyMedianFilterCommand = new RelayCommand(() => ApplyFilter(1));
            ApplySobelEdgeDetectionCommand = new RelayCommand(() => ApplyFilter(2));
            ApplyHighPassFilterCommand = new RelayCommand(() => ApplyFilter(3));
            ApplyGaussianBlurFilterCommand = new RelayCommand(() => ApplyFilter(4));
            ApplyCustomFilterCommand = new RelayCommand(() => ApplyFilter(5));

            AddFilterCommand = new RelayCommand(() => ApplyPointFilter(0));
            SubtractFilterCommand = new RelayCommand(() => ApplyPointFilter(1));
            MultiplyFilterCommand = new RelayCommand(() => ApplyPointFilter(2));
            DivideFilterCommand = new RelayCommand(() => ApplyPointFilter(3));
            BrightnessFilterCommand = new RelayCommand(() => ApplyBrightnessFilter());
            GrayscaleFilterCommand = new RelayCommand(() => ApplyGrayscaleFilter());

            HistogramWindowCommand = new RelayCommand(() => OpenHistogramWindow());
            HistEqualizationCommand = new RelayCommand(() => HistogramEqualize());
            BinarizationUserCommand = new RelayCommand(() => BinarizationSelector(0));
            BinarizationPercentCommand = new RelayCommand(() => BinarizationSelector(1));
            BinarizationMedianCommand = new RelayCommand(() => BinarizationSelector(2));
            BinarizationOtsuCommand = new RelayCommand(() => BinarizationSelector(3));
            BinarizationNiblackCommand = new RelayCommand(() => BinarizationSelector(4));
            BinarizationBernsensCommand = new RelayCommand(() => BinarizationSelector(5));

            MorphologyDilatationCommand = new RelayCommand(() => BinarizationSelector(6));
            MorphologyErosionCommand = new RelayCommand(() => BinarizationSelector(7));
            MorphologyOpeningCommand = new RelayCommand(() => BinarizationSelector(8));
            MorphologyClosingCommand = new RelayCommand(() => BinarizationSelector(9));
            MorphologyThiningCommand = new RelayCommand(() => BinarizationSelector(10));
            MorphologyThickeningCommand = new RelayCommand(() => BinarizationSelector(11));

            ColorSettings.StaticPropertyChanged += ColorSettings_StaticPropertyChanged;
        }

        private void NewCanvas()
        {
            MainCanvas.Children.Clear();
            CanvasHeight = 200;
            CanvasWidth = 300;
        }


        //-------------------------------------------------HISTOGRAM-----------------------------------------------
        private void BinarizationSelector(int binType)
        {
            BitmapSource source = GetCanvasBitmap();

            Window binarization;
            BinarizationHelper binarizationHelper = new BinarizationHelper(source, MainCanvas);
            switch (binType)
            {
                case 0:
                    binarization = new BinarizationValueView(binarizationHelper);
                    break;
                case 1:
                    binarization = new BinarizationPercentView(binarizationHelper);
                    break;
                case 2:
                    binarization = new BinarizationMedianView(binarizationHelper);
                    break;
                case 3:
                    binarization = new BinarizationOtsuView(binarizationHelper);
                    break;
                case 4:
                    binarization = new BinarizationNiblackView(binarizationHelper);
                    break;
                case 5:
                    binarization = new BinarizationBernsensView(binarizationHelper);
                    break;
                case 6:
                    binarization = new MorphologyView(binarizationHelper,0);
                    break;
                case 7:
                    binarization = new MorphologyView(binarizationHelper,1);
                    break;
                case 8:
                    binarization = new MorphologyView(binarizationHelper,2);
                    break;
                case 9:
                    binarization = new MorphologyView(binarizationHelper,3);
                    break;
                case 10:
                    binarization = new MorphologyView(binarizationHelper,4);
                    break;
                case 11:
                    binarization = new MorphologyView(binarizationHelper,5);
                    break;
                default:
                    binarization = new BinarizationValueView(binarizationHelper);
                    break;
            }
                
            bool? dialogResult = binarization.ShowDialog();

            if (dialogResult != true)
            {
                Image originalImage = new Image();
                originalImage.Source = source;
                MainCanvas.Children.Clear();
                MainCanvas.Children.Add(originalImage);
            }
        }



        private void OpenHistogramWindow()
        {
            BitmapSource source = GetCanvasBitmap();

            ImgHistogram histogram = new ImgHistogram(source);

            WPF_Paint.Views.Histogram inputWindow = new WPF_Paint.Views.Histogram(histogram);
            bool? dialogResult = inputWindow.ShowDialog();

        }

        private void HistogramEqualize()
        {
            BitmapSource source = GetCanvasBitmap();

            ImgHistogram histogram = new ImgHistogram(source);

            Image equalImage = new Image();
            equalImage.Source = histogram.EqualBitmapSource();

            MainCanvas.Children.Clear();
            MainCanvas.Children.Add(equalImage);

        }

        //-------------------------------------------------COLOR CHANGE-----------------------------------------------


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
                OnPropertyChanged(nameof(BorderColorProperty));
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


        //-----------------------------------------------------DRAWING MODE----------------------------------------------------------
        //------------------operacje na canvasie-----------------

        /// <summary>
        /// Wywoływane gdy urzytkownik przyciśnie klawisz myszy nad Canvas
        /// </summary>
        /// <param name="point"></param>
        /// 
        private bool _isPolygonBeingMoved = false;
        private Point _lastRightClickPoint;
        private bool _isResizingPolygon = false;
        private bool _isRotatingPolygon = false;
        private Point _chosenPointByUser = new Point(0,0);

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
                case DrawingMode.Point:
                    _chosenPointByUser = point;
                    break;
                case DrawingMode.Polygon:
                    if (Mouse.LeftButton == MouseButtonState.Pressed)
                    {
                        if (!_isPolygonDrawing)
                        {
                            _isPolygonDrawing = true;
                            _polygonPoints.Clear();
                        }

                        _polygonPoints.Add(point);
                        UpdatePolygonPreview();
                    }

                    if (Mouse.RightButton == MouseButtonState.Pressed && !_isPolygonDrawing && !Keyboard.IsKeyDown(Key.O) && !Keyboard.IsKeyDown(Key.S))
                    {
                        CheckIfClickPolygon(point);
                    }
                    if (Mouse.RightButton == MouseButtonState.Pressed && !_isPolygonDrawing && Keyboard.IsKeyDown(Key.O))
                    {
                        CheckIfClickRotateHandle(point);
                    }
                    if (Mouse.RightButton == MouseButtonState.Pressed && !_isPolygonDrawing && Keyboard.IsKeyDown(Key.S))
                    {
                        CheckIfClickScaleHandle(point);
                    }

                    break;

            }
        }

        private void Canvas_MouseLeftButtonUp(Point point)
        {
            _isMousePressed = false;
            _currentPoint = point;

            switch (_currentDrawingMode)
            {
                case DrawingMode.Shape:
                    UpdateShape();
                    break;
                case DrawingMode.Polygon:
                        if (_isPolygonDrawing)
                        {
                            if (_polygonPoints.Count >= 3 && DistanceBetweenPoints(_polygonPoints.First(), _polygonPoints.Last()) < 10)
                            {
                                // Zakończ rysowanie wielokąta
                                _isPolygonDrawing = false;
                            }
                        }
                    break;
            }
            if (_isPolygonBeingMoved)
            {
                _isPolygonBeingMoved = false;
            }
            if (_isResizingPolygon)
            {
                // Zakończ zmianę rozmiaru figury
                _isResizingPolygon = false;
            }
            if (_isRotatingPolygon)
            {
                // Zakończ zobracanie figury
                _isRotatingPolygon = false;
            }



            Mouse.Capture(null);
        }

        private double DistanceBetweenPoints(Point point1, Point point2)
        {
            double deltaX = point2.X - point1.X;
            double deltaY = point2.Y - point1.Y;
            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }

        private void Canvas_MouseMove(Point point)
        {
            if (_isMousePressed && !_isPolygonBeingMoved && !_isResizingPolygon && !_isRotatingPolygon)
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
                    case DrawingMode.Polygon:
                        break;
                }
            }
            else if (_isPolygonBeingMoved)
            {
                // Przesuń wielokąt o różnicę pozycji myszy
                double deltaX = point.X - _lastRightClickPoint.X;
                double deltaY = point.Y - _lastRightClickPoint.Y;

                MovePolygon(deltaX, deltaY);

                _lastRightClickPoint = point;
            }
            else if (_isRotatingPolygon)
            {
                Point newPoint = new Point(point.X/100, point.Y/100);
                // Aktualizuj punkt końcowy podczas zmiany rozmiaru
                double angle = CalculateRotationAngle(newPoint, _lastRightClickPoint);

                RotatePolygon(angle);
                _lastRightClickPoint = point;
            }

            else if (_isResizingPolygon)
            {
                double scaleFactor = _lastRightClickPoint.Y - point.Y;
                if (scaleFactor < 0)
                {
                    scaleFactor= scaleFactor*(-1);
                    scaleFactor = MapToRange(scaleFactor, 0.5, 1);
                }
                else
                {
                    scaleFactor = MapToRange(scaleFactor, 1, 1.2);
                }
                ScalePolygon(scaleFactor);
                _lastRightClickPoint = point;
            }
        }

        static double MapToRange(double value, double minValue, double maxValue)
        {
            double range = maxValue - minValue;

            // Użyj funkcji modulo, aby przemapować wartość do przedziału minValue do maxValue
            double mappedValue = ((value - minValue) % range + range) % range + minValue;

            return mappedValue;
        }

        private void Canvas_KeyDown(KeyEventArgs e)
        {
            if (_currentDrawingMode == DrawingMode.Shape && MainCanvas.Children.Count != 0)
            {
                KeyUpdate(e);
            }
        }

        private void Canvas_KeyUp(KeyEventArgs e) { }



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

        //----------------polygon--------------
        private void PolygonButton_Click()
        {
            ChangeDrawingMode(DrawingMode.Polygon);
        }

        private List<Point> _polygonPoints = new List<Point>();
        private bool _isPolygonDrawing = false;
        private List<List<Point>> _polygonsList = new List<List<Point>>();

        private void UpdatePolygonPreview()
        {
            // Sprawdź zamknięcie figury
            if (_polygonPoints.Count >= 3 && DistanceBetweenPoints(_polygonPoints.First(), _polygonPoints.Last()) < 10)
            {
                _polygonPoints[_polygonPoints.Count - 1] = _polygonPoints.First();
                _polygonsList.Add(_polygonPoints);
            }

            // Rysuj linie łączące punkty wielokąta
            for (int i = 1; i < _polygonPoints.Count; i++)
            {
                Line line = new Line
                {
                    Stroke = Brushes.Black,
                    X1 = _polygonPoints[i - 1].X,
                    Y1 = _polygonPoints[i - 1].Y,
                    X2 = _polygonPoints[i].X,
                    Y2 = _polygonPoints[i].Y
                };

                MainCanvas.Children.Add(line);
            }
        }

        //--------przesuwanie polygonu---------
        private Point _translationVector = new Point(0, 0); // Domyślny wektor przesunięcia
        private void SetVectorMove()
        {
            BitmapSource source = GetCanvasBitmap();
            VectorMove translationVector = new VectorMove(0, 0);
            bool? dialogResult = translationVector.ShowDialog();

            if (dialogResult != true)
            {

                MainCanvas.UpdateLayout();
                return;
            }

            _translationVector = new Point(translationVector.X, translationVector.Y);
            ApplyTranslation();
        }

        private void ApplyTranslation()
        {

            if (_currentDrawingMode == DrawingMode.Polygon && !_isPolygonDrawing)
            {
                if (_polygonsList.Count > 0)
                {
                    var lastPolygon = _polygonsList.Last();

                    // Clear previous drawing of the last polygon
                    foreach (var line in MainCanvas.Children.OfType<Line>().ToList())
                    {
                        if (lastPolygon.Contains(new Point(line.X1, line.Y1)) && lastPolygon.Contains(new Point(line.X2, line.Y2)))
                        {
                            MainCanvas.Children.Remove(line);
                        }
                    }


                    // Przesuń każdy punkt wielokąta o wektor przesunięcia
                    for (int i = 0; i < lastPolygon.Count; i++)
                    {
                        lastPolygon[i] = new Point(lastPolygon[i].X + _translationVector.X, lastPolygon[i].Y + _translationVector.Y);
                    }

                    UpdatePolygonPreview();
                }

            }
        }
        //--------obracanie polygonu---------

        private Point CalculateCenterOfFigure(List<Point> polygon)
        {
            double totalX = 0;
            double totalY = 0;

            foreach (var point in polygon)
            {
                totalX += point.X;
                totalY += point.Y;
            }

            double centerX = totalX / polygon.Count;
            double centerY = totalY / polygon.Count;

            return new Point(centerX, centerY);
        }

        private void RotateButton_Click()
        {
            // Pobierz parametry obrotu od użytkownika
            RotateMove rotateMoveDialog = new RotateMove(0, 0, 0);
            bool? dialogResult = rotateMoveDialog.ShowDialog();

            if (dialogResult == true)
            {
                int angle = rotateMoveDialog.Angle;
                Point userPoint = new Point(rotateMoveDialog.X, rotateMoveDialog.Y);


                // Obróć każdy punkt wielokąta względem środka figury
                if (_currentDrawingMode == DrawingMode.Polygon && !_isPolygonDrawing)
                {
                    if (_polygonsList.Count > 0)
                    {
                        var lastPolygon = _polygonsList.Last();

                        foreach (var line in MainCanvas.Children.OfType<Line>().ToList())
                        {
                            if (lastPolygon.Contains(new Point(line.X1, line.Y1)) && lastPolygon.Contains(new Point(line.X2, line.Y2)))
                            {
                                MainCanvas.Children.Remove(line);
                            }
                        }

                        // Znajdź środek figury
                        Point centerOfFigure = CalculateCenterOfFigure(lastPolygon);
                        //wylicz punkt obrotu
                        Point pivotPoint = new Point(userPoint.X + centerOfFigure.X, userPoint.Y + centerOfFigure.Y);
                        // Obróć każdy punkt wielokąta względem środka figury + punkt od użytkownika
                        for (int i = 0; i < lastPolygon.Count; i++)
                        {
                            lastPolygon[i] = RotatePoint(lastPolygon[i], pivotPoint, angle);
                        }

                        UpdatePolygonPreview();
                    }
                }
            }
        }

        private Point RotatePoint(Point point, Point center, double angle)
        {
            double angleInRadians = angle * (Math.PI / 180.0);
            double cosTheta = Math.Cos(angleInRadians);
            double sinTheta = Math.Sin(angleInRadians);

            double xRelativeToCenter = point.X - center.X;
            double yRelativeToCenter = point.Y - center.Y;

            double xRotated = cosTheta * xRelativeToCenter - sinTheta * yRelativeToCenter + center.X;
            double yRotated = sinTheta * xRelativeToCenter + cosTheta * yRelativeToCenter + center.Y;

            return new Point(xRotated, yRotated);
        }

        //-------skalowanie polugonu-----
        private void SetPolygonScale()
        {
            BitmapSource source = GetCanvasBitmap();
            ChangeScale scalingVector = new ChangeScale(0, 0, 1); // Domyślne wartości
            bool? dialogResult = scalingVector.ShowDialog();

            if (dialogResult != true)
            {
                MainCanvas.UpdateLayout();
                return;
            }

            // Pobierz dane o skalowaniu
            Point userPoint = new Point(scalingVector.X, scalingVector.Y);

            double scaleFactor = scalingVector.ScaleFactor;

            ApplyScaling(userPoint, scaleFactor);
        }

        private void ApplyScaling(Point userPoint, double scaleFactor)
        {
            if (_currentDrawingMode == DrawingMode.Polygon && !_isPolygonDrawing)
            {
                if (_polygonsList.Count > 0)
                {
                    var lastPolygon = _polygonsList.Last();
                    // Znajdź środek figury
                    Point centerOfFigure = CalculateCenterOfFigure(lastPolygon);
                    //wylicz punkt 
                    Point scaleCenter = new Point(userPoint.X + centerOfFigure.X, userPoint.Y + centerOfFigure.Y);

                    // Clear previous drawing of the last polygon
                    foreach (var line in MainCanvas.Children.OfType<Line>().ToList())
                    {
                        if (lastPolygon.Contains(new Point(line.X1, line.Y1)) && lastPolygon.Contains(new Point(line.X2, line.Y2)))
                        {
                            MainCanvas.Children.Remove(line);
                        }
                    }

                    // Skaluj każdy punkt wielokąta względem zadanego punktu i współczynnika
                    for (int i = 0; i < lastPolygon.Count; i++)
                    {
                        double deltaX = lastPolygon[i].X - scaleCenter.X;
                        double deltaY = lastPolygon[i].Y - scaleCenter.Y;

                        // Zastosuj skalowanie
                        lastPolygon[i] = new Point(scaleCenter.X + scaleFactor * deltaX, scaleCenter.Y + scaleFactor * deltaY);
                    }

                    UpdatePolygonPreview();
                }
            }
        }
        

        //----myszka i polygon----
        // Funkcja do obliczania odległości punktu od linii
        private double DistancePointToLine(Point p, Point lineStart, Point lineEnd)
        {
            double A = p.X - lineStart.X;
            double B = p.Y - lineStart.Y;
            double C = lineEnd.X - lineStart.X;
            double D = lineEnd.Y - lineStart.Y;

            double dot = A * C + B * D;
            double len_sq = C * C + D * D;
            double param = dot / len_sq;

            double xx, yy;

            if (param < 0)
            {
                xx = lineStart.X;
                yy = lineStart.Y;
            }
            else if (param > 1)
            {
                xx = lineEnd.X;
                yy = lineEnd.Y;
            }
            else
            {
                xx = lineStart.X + param * C;
                yy = lineStart.Y + param * D;
            }

            double dx = p.X - xx;
            double dy = p.Y - yy;
            return Math.Sqrt(dx * dx + dy * dy);
        }


       
        private void CheckIfClickPolygon(Point point)
        {
            foreach (var polygon in _polygonsList)
            {
                for (int i = 1; i < polygon.Count; i++)
                {
                    double distanceToEdge = DistancePointToLine(point, polygon[i - 1], polygon[i]);
                    if (distanceToEdge < 5) // Ustaw odpowiednią odległość, aby uznać kliknięcie za trafienie w krawędź
                    {
                        _isPolygonBeingMoved = true;
                        _lastRightClickPoint = point;
                        break;
                    }
                }
            }
        }

        private void MovePolygon(double deltaX, double deltaY)
        {
            if (_currentDrawingMode == DrawingMode.Polygon && !_isPolygonDrawing)
            {
                if (_polygonsList.Count > 0)
                {
                    var lastPolygon = _polygonsList.Last();

                    // Clear previous drawing of the last polygon
                    foreach (var line in MainCanvas.Children.OfType<Line>().ToList())
                    {
                        if (lastPolygon.Contains(new Point(line.X1, line.Y1)) && lastPolygon.Contains(new Point(line.X2, line.Y2)))
                        {
                            MainCanvas.Children.Remove(line);
                        }
                    }

                    // Przesuń każdy punkt wielokąta o wektor przesunięcia
                    for (int i = 0; i < lastPolygon.Count; i++)
                    {
                        lastPolygon[i] = new Point(lastPolygon[i].X + deltaX, lastPolygon[i].Y + deltaY);
                    }

                    UpdatePolygonPreview();
                }
            }
        }
        

        private void CheckIfClickRotateHandle(Point point)
        {
            foreach (var polygon in _polygonsList)
            {
                for (int i = 1; i < polygon.Count; i++)
                {
                    double distanceToEdge = DistancePointToLine(point, polygon[i - 1], polygon[i]);
                    if (distanceToEdge < 5)
                    {
                        _isRotatingPolygon = true;
                        _lastRightClickPoint = point;
                        break;
                    }
                }
            }
        }

        private void RotatePolygon(double angle)
        {
            if (_currentDrawingMode == DrawingMode.Polygon && !_isPolygonDrawing)
            {
                if (_polygonsList.Count > 0)
                {
                    var lastPolygon = _polygonsList.Last();

                    // Clear previous drawing of the last polygon
                    foreach (var line in MainCanvas.Children.OfType<Line>().ToList())
                    {
                        if (lastPolygon.Contains(new Point(line.X1, line.Y1)) && lastPolygon.Contains(new Point(line.X2, line.Y2)))
                        {
                            MainCanvas.Children.Remove(line);
                        }
                    }

                    //Jeżeli użytkownik nie wybrał punktu
                    if (_chosenPointByUser.X == 0 && _chosenPointByUser.Y == 0)
                    {
                        // Znajdź środek figury
                        _chosenPointByUser = CalculateCenterOfFigure(lastPolygon);
                    }
                    
                    // Obróć każdy punkt wielokąta względem środka figury + punkt od użytkownika
                    for (int i = 0; i < lastPolygon.Count; i++)
                    {
                        lastPolygon[i] = RotatePoint(lastPolygon[i], _chosenPointByUser, angle);
                    }

                    UpdatePolygonPreview();
                }
            }
        }

        private double CalculateRotationAngle(Point startPoint, Point endPoint)
        {
            double angle = Math.Atan2(endPoint.Y - startPoint.Y, endPoint.X - startPoint.X);

            // Zamień wynik z radianów na stopnie
            angle *= 180 / Math.PI;

            // Dostosuj kąt do przedziału [0, 360]
            angle = (angle + 360) % 360;

            return angle;
        }

        private void CheckIfClickScaleHandle(Point point)
        {
            foreach (var polygon in _polygonsList)
            {
                for (int i = 1; i < polygon.Count; i++)
                {
                    double distanceToEdge = DistancePointToLine(point, polygon[i - 1], polygon[i]);
                    if (distanceToEdge < 5)
                    {
                        _isResizingPolygon = true;
                        _lastRightClickPoint = point;
                        break;
                    }
                }
            }
        }

        private void ScalePolygon(double scaleFactor)
        {
            if (_currentDrawingMode == DrawingMode.Polygon && !_isPolygonDrawing)
            {
                if (_polygonsList.Count > 0)
                {
                    var lastPolygon = _polygonsList.Last();

                    //Jeżeli użytkownik nie wybrał punktu
                    if (_chosenPointByUser.X == 0 && _chosenPointByUser.Y == 0)
                    {
                        // Znajdź środek figury
                        _chosenPointByUser = CalculateCenterOfFigure(lastPolygon);
                    }

                    // Clear previous drawing of the last polygon
                    foreach (var line in MainCanvas.Children.OfType<Line>().ToList())
                    {
                        if (lastPolygon.Contains(new Point(line.X1, line.Y1)) && lastPolygon.Contains(new Point(line.X2, line.Y2)))
                        {
                            MainCanvas.Children.Remove(line);
                        }
                    }

                    // Skaluj każdy punkt wielokąta względem zadanego punktu i współczynnika
                    for (int i = 0; i < lastPolygon.Count; i++)
                    {
                        double deltaX = lastPolygon[i].X - _chosenPointByUser.X;
                        double deltaY = lastPolygon[i].Y - _chosenPointByUser.Y;

                        // Zastosuj skalowanie
                        lastPolygon[i] = new Point(_chosenPointByUser.X + scaleFactor * deltaX, _chosenPointByUser.Y + scaleFactor * deltaY);
                    }

                    UpdatePolygonPreview();
                }
            }
        }

        private void ChoosePointButton_Click()
        {
            ChangeDrawingMode(DrawingMode.Point);
        }


        //--------------------------------kształty----------------------------
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



        //---------------------------------------------TEXT BUTTON---------------------------------------------
        private void TextButton_Click()
        {
            ChangeDrawingMode(DrawingMode.Text);
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

        //---------------------------------------------FILRY---------------------------------------------
        private void ApplyFilter(int filterType)
        {
            if (_mainCanvas != null)
            {
                BitmapSource source = GetCanvasBitmap();

                if (source != null)
                {
                    // Konwersja obrazu na format WriteableBitmap, aby móc modyfikować piksele
                    WriteableBitmap writableBitmap = new WriteableBitmap(source);

                    int width = writableBitmap.PixelWidth;
                    int height = writableBitmap.PixelHeight;

                    // Konwersja obrazu na format array pikseli
                    int stride = width * 4; // 4 kanały (RGBA) na piksel
                    byte[] sourcePixels = new byte[height * stride];
                    byte[] bufferPixels = new byte[height * stride];
                    writableBitmap.CopyPixels(sourcePixels, stride, 0);
                    writableBitmap.CopyPixels(bufferPixels, stride, 0);

                    double[,] kernel = {
                        { 1, 2, 1 },
                        { 2, 4, 2 },
                        { 1, 2, 1 }
                    };

                    if (filterType == 5)
                    {
                        CustomFilter inputWindow = new CustomFilter();
                        bool? dialogResult = inputWindow.ShowDialog();

                        if (dialogResult != true) return;

                        kernel = inputWindow.Values;
                    }

                    int radius = 1; // Promień filtra 

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            switch (filterType)
                            {
                                case 0:
                                    Filter.ApplyAveragePixelFilter(x, y, radius, writableBitmap, sourcePixels, bufferPixels, stride);
                                    break;
                                case 1:
                                    Filter.ApplyMedianPixelFilter(x, y, radius, writableBitmap, sourcePixels, bufferPixels, stride);
                                    break;
                                case 2:
                                    Filter.ApplySobelEdgeDetection(x, y, writableBitmap, sourcePixels, bufferPixels, stride);
                                    break;
                                case 3:
                                    Filter.ApplyHighPassFilter(x, y, writableBitmap, sourcePixels, bufferPixels, stride);
                                    break;
                                case 4:
                                    Filter.ApplyGaussianBlurFilter(x, y, writableBitmap, sourcePixels, bufferPixels, stride);
                                    break;
                                case 5:

                                    Filter.ApplyCustomFilter(x, y, kernel, writableBitmap, sourcePixels, bufferPixels, stride);
                                    break;
                            }
                        }
                    }

                    // Ustawienie zmodyfikowanych pikseli z powrotem do obrazu
                    writableBitmap.WritePixels(new Int32Rect(0, 0, width, height), bufferPixels, stride, 0);

                    // Ustawienie zmodyfikowanego obrazu z powrotem na Canvas
                    Image modifiedImage = new Image();
                    modifiedImage.Source = writableBitmap;

                    _mainCanvas.Children.Clear();
                    _mainCanvas.Children.Add(modifiedImage);
                }
            }
        }

        private void ApplyPointFilter(int filterType)
        {
            if (_mainCanvas != null)
            {
                BitmapSource source = GetCanvasBitmap();

                if (source != null)
                {
                    // Konwersja obrazu na format WriteableBitmap, aby móc modyfikować piksele
                    WriteableBitmap writableBitmap = new WriteableBitmap(source);

                    int width = writableBitmap.PixelWidth;
                    int height = writableBitmap.PixelHeight;

                    // Konwersja obrazu na format array pikseli
                    int stride = width * 4; // 4 kanały (RGBA) na piksel
                    byte[] sourcePixels = new byte[height * stride];
                    writableBitmap.CopyPixels(sourcePixels, stride, 0);

                    string filterName = "";
                    switch (filterType)
                    {
                        case 0:
                            filterName = "Dodawanie";
                            break;
                        case 1:
                            filterName = "Odejmowanie";
                            break;
                        case 2:
                            filterName = "Mnożenie";                            
                            break;
                        case 3:
                            filterName = "Dzielenie";
                            break;
                        case 4:
                            filterName = "Zmiana jaskości";
                            break;
                        case 5:
                            filterName = "Skala szarości";
                            break;
                    }

                    

                    if(filterType < 4)
                    {
                        PointFilter inputWindow = new PointFilter(filterName);
                        bool? dialogResult = inputWindow.ShowDialog();

                        if (dialogResult != true) return;

                        double[] inputValue = inputWindow.Value;

                        switch (filterType)
                        {
                            case 0:
                                for(int y = 0;y<height;y++)
                                {
                                    for(int x = 0; x < stride; x += 4)
                                    {
                                        sourcePixels[y * stride + x] = (byte)Math.Min(((int)sourcePixels[y * stride + x] + (int)inputValue[0]),255);
                                        sourcePixels[y * stride + x + 1] = (byte)Math.Min(((int)sourcePixels[y * stride + x + 1] + (int)inputValue[1]), 255);
                                        sourcePixels[y * stride + x + 2] = (byte)Math.Min(((int)sourcePixels[y * stride + x + 2] + (int)inputValue[2]), 255);
                                    }
                                }
                                break;
                            case 1:
                                for (int y = 0; y < height; y++)
                                {
                                    for (int x = 0; x < stride; x += 4)
                                    {
                                        sourcePixels[y * stride + x] = (byte)Math.Max(((int)sourcePixels[y * stride + x] - (int)inputValue[0]), 0);
                                        sourcePixels[y * stride + x + 1] = (byte)Math.Max(((int)sourcePixels[y * stride + x + 1] - (int)inputValue[1]), 0);
                                        sourcePixels[y * stride + x + 2] = (byte)Math.Max(((int)sourcePixels[y * stride + x + 2] - (int)inputValue[2]), 0);
                                    }
                                }
                                break;
                            case 2:
                                for (int y = 0; y < height; y++)
                                {
                                    for (int x = 0; x < stride; x += 4)
                                    {
                                        sourcePixels[y * stride + x] = (byte)Math.Min(((double)sourcePixels[y * stride + x] * inputValue[0]), 255);
                                        sourcePixels[y * stride + x + 1] = (byte)Math.Min(((double)sourcePixels[y * stride + x + 1] * inputValue[1]), 255);
                                        sourcePixels[y * stride + x + 2] = (byte)Math.Min(((double)sourcePixels[y * stride + x + 2] * inputValue[2]), 255);
                                    }
                                }
                                break;
                            case 3:
                                if (inputValue[0] == 0) inputValue[0] = 1;
                                if (inputValue[1] == 0) inputValue[1] = 1;
                                if (inputValue[2] == 0) inputValue[2] = 1;
                                for (int y = 0; y < height; y++)
                                {
                                    for (int x = 0; x < stride; x += 4)
                                    {
                                        sourcePixels[y * stride + x] = (byte)Math.Min(((double)sourcePixels[y * stride + x] / inputValue[0]), 255);
                                        sourcePixels[y * stride + x + 1] = (byte)Math.Min(((double)sourcePixels[y * stride + x + 1] / inputValue[1]), 255);
                                        sourcePixels[y * stride + x + 2] = (byte)Math.Min(((double)sourcePixels[y * stride + x + 2] / inputValue[2]), 255);
                                    }
                                }
                                break;
                        }
                    }
                    


                    // Ustawienie zmodyfikowanych pikseli z powrotem do obrazu
                    writableBitmap.WritePixels(new Int32Rect(0, 0, width, height), sourcePixels, stride, 0);

                    // Ustawienie zmodyfikowanego obrazu z powrotem na Canvas
                    Image modifiedImage = new Image();
                    modifiedImage.Source = writableBitmap;

                    _mainCanvas.Children.Clear();
                    _mainCanvas.Children.Add(modifiedImage);
                }
            }
        }

        private void ApplyGrayscaleFilter()
        {
            if (_mainCanvas != null)
            {
                BitmapSource source = GetCanvasBitmap();

                if (source != null)
                {
                    // Konwersja obrazu na format WriteableBitmap, aby móc modyfikować piksele
                    WriteableBitmap writableBitmap = new WriteableBitmap(source);

                    int width = writableBitmap.PixelWidth;
                    int height = writableBitmap.PixelHeight;

                    // Konwersja obrazu na format array pikseli
                    int stride = width * 4; // 4 kanały (RGBA) na piksel
                    byte[] sourcePixels = new byte[height * stride];
                    writableBitmap.CopyPixels(sourcePixels, stride, 0);
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < stride; x += 4)
                        {
                            int grayscale = (int)(0.299 * sourcePixels[y * stride + x] + 0.587 * sourcePixels[y * stride + x + 1] + 0.114 * sourcePixels[y * stride + x + 2]);
                            sourcePixels[y * stride + x] = (byte)grayscale;
                            sourcePixels[y * stride + x + 1] = (byte)grayscale;
                            sourcePixels[y * stride + x + 2] = (byte)grayscale;
                        }
                    }
                    
                    // Ustawienie zmodyfikowanych pikseli z powrotem do obrazu
                    writableBitmap.WritePixels(new Int32Rect(0, 0, width, height), sourcePixels, stride, 0);

                    // Ustawienie zmodyfikowanego obrazu z powrotem na Canvas
                    Image modifiedImage = new Image();
                    modifiedImage.Source = writableBitmap;

                    _mainCanvas.Children.Clear();
                    _mainCanvas.Children.Add(modifiedImage);
                }
            }
        }

        private void ApplyBrightnessFilter()
        {
            if (_mainCanvas != null)
            {
                BitmapSource source = GetCanvasBitmap();

                if (source != null)
                {
                    BrightnessFilter inputWindow = new BrightnessFilter();
                    bool? dialogResult = inputWindow.ShowDialog();

                    if (dialogResult != true) return;

                    double inputValue = inputWindow.Value;

                    // Konwersja obrazu na format WriteableBitmap, aby móc modyfikować piksele
                    WriteableBitmap writableBitmap = new WriteableBitmap(source);

                    int width = writableBitmap.PixelWidth;
                    int height = writableBitmap.PixelHeight;

                    // Konwersja obrazu na format array pikseli
                    int stride = width * 4; // 4 kanały (RGBA) na piksel
                    byte[] sourcePixels = new byte[height * stride];
                    writableBitmap.CopyPixels(sourcePixels, stride, 0);
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < stride; x += 4)
                        {
                            sourcePixels[y * stride + x] = (byte)Math.Max(Math.Min(((int)sourcePixels[y * stride + x] + (int)inputValue), 255),0);
                            sourcePixels[y * stride + x + 1] = (byte)Math.Max(Math.Min(((int)sourcePixels[y * stride + x + 1] + (int)inputValue), 255),0);
                            sourcePixels[y * stride + x + 2] = (byte)Math.Max(Math.Min(((int)sourcePixels[y * stride + x + 2] + (int)inputValue), 255),0);
                        }
                    }
                    
                    // Ustawienie zmodyfikowanych pikseli z powrotem do obrazu
                    writableBitmap.WritePixels(new Int32Rect(0, 0, width, height), sourcePixels, stride, 0);

                    // Ustawienie zmodyfikowanego obrazu z powrotem na Canvas
                    Image modifiedImage = new Image();
                    modifiedImage.Source = writableBitmap;

                    _mainCanvas.Children.Clear();
                    _mainCanvas.Children.Add(modifiedImage);
                }
            }
        }



        //-----------------------------------------------ZAPIS ODCZYT PLIKÓW---------------------------------------------
        private void SetImageSize()
        {
            BitmapSource source = GetCanvasBitmap();
            ImageSize imageSize = new ImageSize(source.PixelWidth, source.PixelHeight);
            bool? dialogResult = imageSize.ShowDialog();

            if (dialogResult != true)
            {

                MainCanvas.UpdateLayout();
                return;
            }

            CanvasWidth = imageSize.Width;
            CanvasHeight = imageSize.Height;


        }

        private void OpenPicture()
        {
            ChangeDrawingMode(DrawingMode.None);

            string filePath = GetPicturePath();
            if (String.IsNullOrEmpty(filePath)) return;

            BitmapSource bitmap;
            // Create an Image control and set the BitmapSource as its source
            Image image = new Image();

            if (System.IO.Path.GetExtension(filePath).Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                System.IO.Path.GetExtension(filePath).Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                System.IO.Path.GetExtension(filePath).Equals(".png", StringComparison.OrdinalIgnoreCase))
            {
                // Load JPEG image
                BitmapImage bitmapImage = new BitmapImage(new Uri(filePath));
                bitmap = new WriteableBitmap(bitmapImage);
                image.Source = bitmap;
            }
            else
            {
                Netpbm netpbm = new Netpbm(filePath);
                image.Source = netpbm.Bitmap;
            }

            MainCanvas.Children.Clear();

            CanvasWidth = image.Source.Width;
            CanvasHeight = image.Source.Height;
            // Add the Image control to the MainCanvas
            MainCanvas.Children.Add(image);
        }

        private string GetPicturePath()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter =
                "any (*.*)|*.*|" +
                "PBM (*.pbm)|*.pbm|" +
                "PGM (*.pgm)|*.pgm|" +
                "PPM (*.ppm)|*.ppm|" +
                "PBN bin (*.pbm)|*.pbm|" +
                "PGM bin (*.pgm)|*.pgm|" +
                "PPM bin (*.ppm)|*.ppm";

            return (dlg.ShowDialog() == true) ? (dlg.FileName) : ("");
        }

        private void SaveCanvas()
        {
            ChangeDrawingMode(DrawingMode.None);
            BitmapSource canvasBitmap = GetCanvasBitmap();

            (int fileType, string filePath) = GetFileNameAndExtension();

            if (String.IsNullOrEmpty(filePath)) return;

            if (fileType == 1)
                SaveToJPG(filePath, canvasBitmap);
            else if (fileType < 8)
                SaveToP(fileType, filePath, canvasBitmap);
            else
            {
                SaveToJPG(filePath.Substring(0, filePath.Length - 3) + "jpg", canvasBitmap);
                SaveToP(2, filePath.Substring(0, filePath.Length - 3) + "pbm", canvasBitmap);
                SaveToP(3, filePath.Substring(0, filePath.Length - 3) + "pgm", canvasBitmap);
                SaveToP(4, filePath.Substring(0, filePath.Length - 3) + "ppm", canvasBitmap);
                SaveToP(5, filePath.Substring(0, filePath.Length - 4) + "bin.pbm", canvasBitmap);
                SaveToP(6, filePath.Substring(0, filePath.Length - 4) + "bin.pgm", canvasBitmap);
                SaveToP(7, filePath.Substring(0, filePath.Length - 4) + "bin.ppm", canvasBitmap);
            }
        }

        private BitmapSource GetCanvasBitmap()
        {
            // Save the current transform and offset of the Canvas
            var originalTransform = MainCanvas.LayoutTransform;
            var originalOffset = new Point(Canvas.GetLeft(MainCanvas), Canvas.GetTop(MainCanvas));

            ScaleTransform St = ((ScaleTransform)MainCanvas.LayoutTransform);
            double OriginalScale = St.ScaleX;

            if (OriginalScale < 1.4)
            {
                St.ScaleX = 1.5;
                St.ScaleY = 1.5;
            }

            // Update layout for rendering
            MainCanvas.LayoutTransform = null; // Temporarily remove any layout transforms
            MainCanvas.UpdateLayout();
            MainCanvas.Measure(new Size(MainCanvas.Width, MainCanvas.Height));
            MainCanvas.Arrange(new Rect(new Size(MainCanvas.Width, MainCanvas.Height)));

            // Create a render bitmap and push the canvas to it
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                (int)MainCanvas.Width,
                (int)MainCanvas.Height,
                96d,
                96d,
                PixelFormats.Pbgra32);
            renderBitmap.Render(MainCanvas);
            MainCanvas.UpdateLayout();

            // Restore the original transform and offset of the Canvas
            MainCanvas.LayoutTransform = originalTransform;

            // Update the layout again to reflect the restored state
            MainCanvas.UpdateLayout();
            St.ScaleX = OriginalScale;
            St.ScaleY = OriginalScale;


            return renderBitmap;
        }

        private (int, string) GetFileNameAndExtension()
        {
            // Wybierz ścieżkę i nazwę pliku do zapisania
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "PaintImage"; // Nazwa domyślna
            dlg.DefaultExt = ".jpg"; // Rozszerzenie domyślne
            // Filtry rozszerzeń
            dlg.Filter =
                "JPEG (*.jpg)|*.jpg|" +
                "PBM (*.pbm)|*.pbm|" +
                "PGM (*.pgm)|*.pgm|" +
                "PPM (*.ppm)|*.ppm|" +
                "PBN bin (*.pbm)|*.pbm|" +
                "PGM bin (*.pgm)|*.pgm|" +
                "PPM bin (*.ppm)|*.ppm|" +
                "save in all (*.*)|*.";

            // Wyświetl okno dialogowe i zapisz plik, jeśli użytkownik kliknie "Zapisz"
            return (dlg.ShowDialog() == true) ? (dlg.FilterIndex, dlg.FileName) : (0, "");
        }

        private void SaveToJPG(string filePath, BitmapSource canvasBitmap)
        {
            using (FileStream fs = File.Open(filePath, FileMode.Create))
            {
                // Kod zapisywania pliku pozostaje bez zmian
                BitmapEncoder jpgEncoder = new JpegBitmapEncoder();
                jpgEncoder.Frames.Add(BitmapFrame.Create(canvasBitmap));
                jpgEncoder.Save(fs);
            }
        }

        //filetype
        //2 = P1
        //3 = P2
        //4 = P3
        //5 = P4
        //6 = P5
        //7 = P6
        private void SaveToP(int fileType, string filePath, BitmapSource canvasBitmap)
        {
            int Px = fileType - 1;
            int width = canvasBitmap.PixelWidth;
            int height = canvasBitmap.PixelHeight;
            PixelFormat format = canvasBitmap.Format;
            int bytesPerPixel = format.BitsPerPixel / 8; //bytesPerPixel = 4, RGBA
            int stride = width * bytesPerPixel; // szerokość w bajtach
            byte[] pixelColors = new byte[height * stride]; //pixele jakno kolejne kolory kolejnych pixeli

            canvasBitmap.CopyPixels(pixelColors, stride, 0);

            bool isBW = (Px == 1 || Px == 4);
            bool isGrayScale = (Px == 2 || Px == 5);
            bool isColored = (Px == 3 || Px == 6);
            bool isBinary = (Px >= 4 && Px <= 6);

            //used for P4
            int bytesPerRow = (int)Math.Ceiling((decimal)width / 8);

            // The output values in bytes
            int byteCount = isColored ? (height * width * 3) :
                            (Px == 4) ? (height * bytesPerRow) :
                            height * width;

            byte[] imageContent = new byte[byteCount];

            byte currentByte = 0;
            int bitIndex = 0;

            // Process and write pixel data
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pixelIndex = y * stride + x * bytesPerPixel;
                    int byteIndex = (y * width + x) * ((isColored) ? 3 : 1);

                    // Extract the color components
                    byte blue = pixelColors[pixelIndex];
                    byte green = pixelColors[pixelIndex + 1];
                    byte red = pixelColors[pixelIndex + 2];
                    byte alpha = pixelColors[pixelIndex + 3];

                    // If it is in color, there is no need for encoding
                    if (isColored)
                    {
                        imageContent[byteIndex] = red;
                        imageContent[byteIndex + 1] = green;
                        imageContent[byteIndex + 2] = blue;
                    }// If it is not in color (BW or grayscale) we need to encode it
                    else if (isBW)
                    {
                        if (isBinary)
                        {
                            byte bwValue = Encode_BlackWhite(red, green, blue);

                            currentByte |= (byte)((bwValue & 1) << (7 - bitIndex));

                            bitIndex++;


                            if (bitIndex == 8 || (y == height - 1 && x == width - 1) || x == width - 1)
                            {
                                imageContent[y * bytesPerRow + x / 8] = currentByte;
                                bitIndex = 0;
                                currentByte = 0;
                            }

                        }
                        else
                        {
                            imageContent[byteIndex] = Encode_BlackWhite(red, green, blue);
                        }
                    }
                    else
                    {
                        imageContent[byteIndex] = Encode_Grayscale(red, green, blue);
                    }
                }
            }


            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {
                if (!isBinary)
                {
                    using (StreamWriter writer = new StreamWriter(fileStream))
                    {
                        // Write the Px header
                        writer.WriteLine("P" + Px);
                        writer.WriteLine($"{width} {height}");

                        if (!isBW)
                        {
                            writer.WriteLine("255");
                        }

                        foreach (byte b in imageContent)
                        {
                            writer.WriteLine(b.ToString());
                        }
                    }
                }
                else
                {
                    using (BinaryWriter writer = new BinaryWriter(fileStream))
                    {
                        // Write ASCII header
                        writer.Write(Encoding.ASCII.GetBytes($"P{Px}\n"));
                        writer.Write(Encoding.ASCII.GetBytes($"{width} {height}\n"));
                        if (!isBW) // P5 and P6 have a max value
                        {
                            writer.Write(Encoding.ASCII.GetBytes("255\n"));
                        }
                        writer.Write(imageContent);

                    }
                }
            }
        }

        private static byte Encode_BlackWhite(byte red, byte green, byte blue)
        {
            // Compute the weighted average for grayscale
            // This formula takes into account the human eye's different sensitivities to these colors.
            int grayscale = (int)(0.299 * red + 0.587 * green + 0.114 * blue);

            // Convert to black and white using thresholding
            int bwValue = grayscale < 180 ? 1 : 0;

            return (byte)bwValue;
        }

        private static byte Encode_Grayscale(byte red, byte green, byte blue)
        {
            // Compute the weighted average for grayscale
            // This formula takes into account the human eye's different sensitivities to these colors.
            int grayscale = (int)(0.299 * red + 0.587 * green + 0.114 * blue);

            return (byte)grayscale;
        }

    }
}
