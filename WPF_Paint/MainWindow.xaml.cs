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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPF_Paint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private enum DrawingMode
        {
            None,
            Rectangle,
            Draw,
            Text,
            // Dodaj inne tryby rysowania, jeśli są potrzebne
        }

        private Point _startPosition, _endPosition;
        private DrawingMode _currentDrawingMode = DrawingMode.None;
        private TextBox? activeTextBox;


        public MainWindow()
        {
            InitializeComponent();
        }
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch (_currentDrawingMode)
            {
                case DrawingMode.Rectangle:
                    // Obsługa rysowania prostokątów
                    StartRectangle(sender, e);
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
            switch (_currentDrawingMode)
            {
                case DrawingMode.Rectangle:
                    // Obsługa zakończenia rysowania prostokątów
                    EndRectangle(sender, e);
                    break;
                    // Dodaj inne przypadki dla innych trybów rysowania
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            switch (_currentDrawingMode)
            {
                case DrawingMode.Rectangle:
                    // Obsługa rysowania prostokątów
                    DrawRectangle(sender, e);
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
                case DrawingMode.Rectangle:
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

        private void RectangleButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeDrawingMode(DrawingMode.Rectangle);
        }
        private void TextButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeDrawingMode(DrawingMode.Text);
        }


        //MouseLeftButtonDown
        //tworzy prostokąt przy przyciśnięciu lewego przycisku myszy
        private void StartRectangle(object sender, MouseButtonEventArgs e)
        {
            //pobieramy pozycję myszy, jako początek prostokąta
            _startPosition = e.GetPosition(MainCanvas);

            //tworzymy podstawowy kwadrat, jeszcze bez wymiarów, jedynie z kolorami
            Rectangle rectangle = new Rectangle
            {
                Width = 0,
                Height = 0,
                Fill = Brushes.LightBlue,
                Stroke = Brushes.Blue,
                StrokeThickness = 2
            };

            //przypinamy stworzony prostokąt do miejsca kliknięcia myszy
            rectangle.SetValue(Canvas.TopProperty, _startPosition.Y);
            rectangle.SetValue(Canvas.LeftProperty, _startPosition.X);
            
            //dodajemy obecny prostokąt do dzieci canvasu
            MainCanvas.Children.Add(rectangle);
            
            //każemy programowi śledzić mysz, nawet jeśli wyszła poza obrys okna
            Mouse.Capture(MainCanvas); 
            //okazuje się, że trzeba było dać to na końcu,
            //ponieważ od razu przekazywał do MouseMove (DrawRectangle)
            //który oczekiwał najnowszego kwadrata
        }

        //MouseLeftButtonUp
        //kończy rysowanie prostokąta przy puszczeniu lewego przycisku myszy
        private void EndRectangle(object sender, MouseButtonEventArgs e)
        {
            //pozyskujemy końcową pozycję myszy 
            _endPosition = e.GetPosition(MainCanvas);

            //nanosimy poprawki z pozycją końcową myszy
            updateRectangleByMouse();

            //program nie śledzi już myszy
            Mouse.Capture(null);
        }

        //MouseMove
        //aktualizuje obraz prostokąta przy poruszaniu myszą,
        //przed puszczeniem jej lewego przycisku
        private void DrawRectangle(object sender, MouseEventArgs e)
        {
            //jeśli uwaga jest na naszym oknie i lewy przycisk jest wciśnięty
            if (Mouse.Captured == MainCanvas && e.LeftButton == MouseButtonState.Pressed)
            {
                //pozyskujemy nową pozycję myszy
                _endPosition = e.GetPosition(MainCanvas);

                //nanosimy poprawki z nową pozycją myszy
                updateRectangleByMouse();
            }
        }

        //nanoszenie poprawek do najnowszego prostokąta z nową pozycją myszy
        private void updateRectangleByMouse()
        {
            //dla skrócenia zapamiętujemy index najnowszego prostokąta i pobieramy jego kopię
            int lastIndex = MainCanvas.Children.Count - 1;
            Rectangle rectangle = (Rectangle)MainCanvas.Children[lastIndex];

            //dla kopii aktualizujemy wielkość na bazie absolutnej różnicy między punktem początkowym a obecnym myszy
            rectangle.Width = Math.Abs(_endPosition.X - _startPosition.X);
            rectangle.Height = Math.Abs(_endPosition.Y - _startPosition.Y);

            //zapisujemy najbardziej lewy punkt prostokąta jako jego początek
            rectangle.SetValue(Canvas.LeftProperty, Math.Min(_startPosition.X, _endPosition.X));
            rectangle.SetValue(Canvas.TopProperty, Math.Min(_startPosition.Y, _endPosition.Y));

            //aktualizujemy najnowszy prostokąt i ostatnie dziecko canvasu
            MainCanvas.Children[lastIndex] = rectangle;
        }

        //KeyDown
        //aktualizowanie stworzonego prostokąta po użyciu strzałek lub strzałek z shiftem
        private void KeyUpdate(object sender, KeyEventArgs e) 
        {

            //musimy mieć co przesuwać
            if(MainCanvas.Children.Count == 0) return;

            //dla skrócenia zapamiętujemy index najnowszego prostokąta i pobieramy jego kopię
            int lastIndex = MainCanvas.Children.Count - 1;
            Rectangle rectangle = (Rectangle)MainCanvas.Children[lastIndex];

            //będzie określać prędkość zmian (wolniej)
            bool control = ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control);

            //sprawdzamy czy użytkownik wcisnął shift
            if ((e.KeyboardDevice.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                //jeśli tak, to zależnie od strzałki zmieniamy wielkość prostokąta
                switch (e.Key)
                {
                    //w góre ograniczamy rozmiar do zera
                    case Key.Up:
                        if (rectangle.Height > 0)
                            rectangle.Height -= (control) ? 1 : (rectangle.Height > 5) ? 5: rectangle.Height;
                        break;
                    //w lewo ograniczamy rozmiar do zera
                    case Key.Left:
                        if (rectangle.Width > 0)
                            rectangle.Width -= (control) ? 1 : (rectangle.Width > 5) ? 5 : rectangle.Width;
                        break;
                    case Key.Down:
                        rectangle.Height += (control) ? 1 : 5;
                        break;
                    case Key.Right:
                        rectangle.Width += (control) ? 1 : 5;
                        break;
                }
            }
            else
            {
                //jeśli nie, to zależnie od strzałki zmieniamy położenie prostokąta
                switch (e.Key)
                {
                    //wszystkie działają podobnie, aktualizujemy obecną wartość początku, podając mu obecną ze zmianą wynikającą z klawisza
                    case Key.Up:
                        rectangle.SetValue(Canvas.TopProperty, (double)rectangle.GetValue(Canvas.TopProperty) - ((control) ? 1 : 5));
                        break;
                    case Key.Down:
                        rectangle.SetCurrentValue(Canvas.TopProperty, (double)rectangle.GetValue(Canvas.TopProperty) + ((control) ? 1 : 5));
                        break;
                    case Key.Right:
                        rectangle.SetCurrentValue(Canvas.LeftProperty, (double)rectangle.GetValue(Canvas.LeftProperty) + ((control) ? 1 : 5));
                        break;
                    case Key.Left:
                        rectangle.SetCurrentValue(Canvas.LeftProperty, (double)rectangle.GetValue(Canvas.LeftProperty) - ((control) ? 1 : 5));
                        break;
                }
            }

            //aktualizujemy obecny
            MainCanvas.Children[lastIndex] = rectangle;

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
                AcceptsReturn= true,
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
    }
}
