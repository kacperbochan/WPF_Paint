using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using static WPF_Paint.HelperMethods;

namespace WPF_Paint
{
    public partial class MainWindow : Window
    {
        private enum ShapeType { Triangle, Rectangle, Ellipse }

        private Shape _currentShape;
        private Point _startPosition, _endPosition;
        private ShapeType _currentShapeType, _nextShapeType = ShapeType.Ellipse;

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Uruchamiana przy przyciśnięciu lewego przycisku myszy
        /// Rozpoczyna rysowanie kształtu
        /// </summary>
        private void StartDrawing(object sender, MouseButtonEventArgs e)
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
            }

            _currentShapeType = _nextShapeType;
            
            MainCanvas.Children.Add(_currentShape);
            Mouse.Capture(MainCanvas);
        }

        /// <summary>
        /// Uruchamiana przy podniesieniu lewego przycisku myszy
        /// Kończy rysowanie kształtu
        /// </summary>
        private void EndDrawing(object sender, MouseButtonEventArgs e)
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
            if (_currentShapeType == ShapeType.Triangle)
            {
                StreamGeometry geometry = new StreamGeometry();
                geometry.FillRule = FillRule.EvenOdd;

                using (StreamGeometryContext ctx = geometry.Open())
                {
                    ctx.BeginFigure(new Point(_startPosition.X, _startPosition.Y), true /* Wypełniony */, true /* zamknięty */);

                    ctx.LineTo(new Point(_startPosition.X, _endPosition.Y), true /* Widać linię */, false /* łagodne połączenie*/);

                    ctx.LineTo(new Point(_endPosition.X, _endPosition.Y), true /* Widać linię */, true /* łagodne połączenie */);
                }
                geometry.Freeze();

                ((System.Windows.Shapes.Path)shape).Data = geometry;
            }
            else
            {
                shape.Width = Math.Abs(_endPosition.X - _startPosition.X);
                shape.Height = Math.Abs(_endPosition.Y - _startPosition.Y);
                shape.SetValue(Canvas.LeftProperty, Math.Min(_startPosition.X, _endPosition.X));
                shape.SetValue(Canvas.TopProperty, Math.Min(_startPosition.Y, _endPosition.Y));
            }
        }

        /// <summary>
        /// Uruchamiana gdy urzytkownik naciśnie klawisz klawiatury
        /// Służy do wybierania kszrałtu, skalowania figury, poruszania figury
        /// </summary>
        private void KeyUpdate(object sender, KeyEventArgs e)
        {
            UpdateNextShapeType(e);

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
