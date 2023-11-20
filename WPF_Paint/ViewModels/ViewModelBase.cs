using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
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

        public ICommand OpenCommand { get; }
        public ICommand SaveCommand { get; }

        public ICommand FillColorCommand { get; }
        public ICommand BorderColorCommand { get; }

        public ICommand OpenFillingColorSelectorCommand { get; }
        public ICommand OpenBorderColorSelectorCommand { get; }
        public ICommand ApplyAverageFilterCommand { get; }
        public ICommand ApplyMedianFilterCommand { get; }
        public ICommand ApplySobelEdgeDetectionCommand { get; }
        public ICommand ApplyHighPassFilterCommand { get; }
        public ICommand ApplyGaussianBlurFilterCommand { get; }

        
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

            OpenCommand = new RelayCommand(OpenPicture);
            SaveCommand = new RelayCommand(SaveCanvas);

            OpenFillingColorSelectorCommand = new RelayCommand(() => OpenColorSelector(0));
            OpenBorderColorSelectorCommand = new RelayCommand(() => OpenColorSelector(1));
            ApplyAverageFilterCommand = new RelayCommand(() => ApplyFilter(0));
            ApplyMedianFilterCommand = new RelayCommand(() => ApplyFilter(1));
            ApplySobelEdgeDetectionCommand = new RelayCommand(() => ApplyFilter(2));
            ApplyHighPassFilterCommand = new RelayCommand(() => ApplyFilter(3));
            ApplyGaussianBlurFilterCommand = new RelayCommand(() => ApplyFilter(4));

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

        private void OpenPicture()
        {
            ChangeDrawingMode(DrawingMode.None);

            string filePath = GetPicturePath();

            string fileContent = "";

            try
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        fileContent = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., file is not an image)
                MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                return;
            }

            BitmapSource bitmap;
            if (System.IO.Path.GetExtension(filePath).Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                System.IO.Path.GetExtension(filePath).Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                System.IO.Path.GetExtension(filePath).Equals(".png", StringComparison.OrdinalIgnoreCase))
            {
                // Load JPEG image
                BitmapImage bitmapImage = new BitmapImage(new Uri(filePath));
                bitmap = new WriteableBitmap(bitmapImage);
            }

            else
            {
                int Px = fileContent[1] - 48;

                if (Px < 4)
                {
                    MainCanvas.Children.Clear();

                    // Create the BitmapSource based on the file content
                    bitmap = CreateBitmapSourceFromPixelData(fileContent, Px);

                }
                else
                {
                    string pattern = @"#.*?(?=\n|$)";
                    fileContent = Regex.Replace(fileContent, pattern, "");
                    string[] values = Regex.Split(fileContent, @"\s+");

                    int width = int.Parse(values[1]);
                    int height = int.Parse(values[2]);

                    //if P4 it will just not ever be used;
                    int MaxValue = 255;
                    int.TryParse(values[3], out MaxValue);

                    MainCanvas.Children.Clear();

                    int headerLength = GetHeaderLength(filePath, (Px == 4) ? 3 : 4);

                    using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
                    {

                        byte[] headerBytes = new byte[headerLength];
                        fileStream.Read(headerBytes, 0, headerLength);

                        if (Px == 4)
                        {
                            int bytesPerRow = (int)Math.Ceiling((decimal)width / 8);
                            byte[] rowBuffer = new byte[bytesPerRow];
                            byte[] pixels = new byte[width * height]; // One byte per pixel for the image

                            for (int row = 0; row < height; row++)
                            {
                                fileStream.Read(rowBuffer, 0, bytesPerRow);

                                for (int col = 0; col < width; col++)
                                {
                                    int byteIndex = col / 8;
                                    int bitIndex = 7 - (col % 8); // From most significant to least significant bit
                                    byte mask = (byte)(1 << bitIndex);
                                    pixels[row * width + col] = (byte)((rowBuffer[byteIndex] & mask) != 0 ? 0 : 255); // 0 for black, 255 for white
                                }
                            }

                            bitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, width);
                        }
                        else if (Px == 5)
                        {
                            // Now read the image data
                            byte[] pixels = new byte[width * height];
                            fileStream.Read(pixels, 0, pixels.Length);

                            // Check if normalization is needed
                            if (MaxValue != 255)
                            {
                                for (int i = 0; i < pixels.Length; i++)
                                {
                                    // Normalize the pixel value
                                    double normalizedValue = (pixels[i] / (double)MaxValue) * 255;
                                    pixels[i] = (byte)Math.Clamp(normalizedValue, 0, 255);
                                }
                            }

                            bitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, width);
                        }
                        else
                        {

                            // For P6, each pixel has three components (R, G, B)
                            byte[] pixels = new byte[width * height * 3];
                            fileStream.Read(pixels, 0, pixels.Length);

                            // Check if normalization is needed
                            if (MaxValue != 255)
                            {
                                for (int i = 0; i < pixels.Length; i++)
                                {
                                    // Normalize each component of the pixel
                                    double normalizedValue = (pixels[i] / (double)MaxValue) * 255;
                                    pixels[i] = (byte)Math.Clamp(normalizedValue, 0, 255);
                                }
                            }

                            int stride = width * 3; // Stride for Rgb24 format
                            bitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Rgb24, null, pixels, stride);
                        }

                    }
                }
            }

            // Create an Image control and set the BitmapSource as its source
            Image image = new Image();
            image.Source = bitmap;

            // Add the Image control to the MainCanvas
            MainCanvas.Children.Add(image);
        }
        private int GetHeaderLength(string filePath, int requiredHeaderParts)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            {
                int count = 0;
                int sections = 0;
                bool inSection = false;

                while (true)
                {

                    int b = fileStream.ReadByte();
                    if (b == -1) // End of file
                        break;

                    char c = (char)b;
                    count++;

                    if (char.IsWhiteSpace(c))
                    {
                        if (inSection)
                        {
                            sections++;
                            inSection = false;

                            if (sections == requiredHeaderParts) // Header ends after the forth section
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        inSection = true;
                    }

                    if (c == '#')
                    {
                        while (true)
                        {
                            c = (char)fileStream.ReadByte();
                            count++;
                            if (c == '\n' || c == '\r') break;
                        }
                    }
                }

                return count;
            }
        }

        private BitmapSource CreateBitmapSourceFromPixelData(string fileContent, int px)
        {
            //removing comments
            string pattern = @"#.*?(?=\n|$)";
            fileContent = Regex.Replace(fileContent, pattern, "");

            string[] pixelData = Regex.Split(fileContent, @"\s+");

            int width = int.Parse(pixelData[1]);
            int height = int.Parse(pixelData[2]);

            //If it is P1, we will just not use it
            int MaxValue = int.Parse(pixelData[3]);

            if (px == 1)
            {
                byte[] pixels = new byte[width * height];
                for (int i = 3; i < pixelData.Length; i++)
                {
                    if (int.TryParse(pixelData[i], out int value))
                    {   
                        pixels[i - 3] = (byte)(value==1? 0: 255);
                    }
                }

                return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, width);
            }
            else if (px == 2)
            {
                byte[] pixels = new byte[width * height];
                for (int i = 4; i < pixelData.Length; i++)
                {
                    if (int.TryParse(pixelData[i], out int value))
                    {
                        // Normalize the pixel value to be within the 0-255 range
                        double normalizedValue = (value / (double)MaxValue) * 255;
                        pixels[i - 4] = (byte)Math.Clamp(normalizedValue, 0, 255);
                    }
                }
                return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, width);
            }
            else if (px == 3)
            {
                byte[] pixels = new byte[width * height * 3];
                int stride = width * 3; // Stride for Rgb24 format
                double normalizationFactor = 255.0 / MaxValue;

                Parallel.For(0, height, y =>
                {
                    int baseIndex = y * stride;
                    for (int x = 0; x < width; x++)
                    {
                        int pixelDataIndex = 4 + (y * width + x) * 3;
                        for (int j = 0; j < 3; j++)
                        {
                            if (int.TryParse(pixelData[pixelDataIndex + j], out int value))
                            {
                                double normalizedValue = value * normalizationFactor;
                                pixels[baseIndex + x * 3 + j] = (byte)Math.Clamp(normalizedValue, 0, 255);
                            }
                        }
                    }
                });

                return BitmapSource.Create(width, height, 96, 96, PixelFormats.Rgb24, null, pixels, stride);
            }


            return null;
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
            CroppedBitmap canvasBitmap = GetCanvasBitmap();

            (int fileType, string filePath) = GetFileNameAndExtension();
            
            if(fileType == 1)                
                SaveToJPG(filePath, canvasBitmap);
            else if(fileType < 8)
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

        private CroppedBitmap GetCanvasBitmap()
        {
            Rect rect = new Rect(90, 0, MainCanvas.ActualWidth, MainCanvas.ActualHeight);
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)rect.Right, (int)rect.Bottom, 96d, 96d, System.Windows.Media.PixelFormats.Default);
            rtb.Render(MainCanvas);

            // Określ prostokąt, który chcesz zachować (x, y, width, height)
            Int32Rect cropRect = new Int32Rect(90, 0, (int)(rtb.PixelWidth - 90), (int)rtb.PixelHeight);

            // Utwórz CroppedBitmap na podstawie RenderTargetBitmap i prostokąta
            CroppedBitmap croppedBitmap = new CroppedBitmap(rtb, cropRect);

            return croppedBitmap;
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
            return (dlg.ShowDialog() == true) ? (dlg.FilterIndex, dlg.FileName) : (0,"");
        }

        private void SaveToJPG(string filePath, CroppedBitmap canvasBitmap)
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
        private void SaveToP(int fileType, string filePath, CroppedBitmap canvasBitmap)
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
                    int byteIndex = (y * width + x) * ((isColored) ? 3: 1);

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

        private void ApplyFilter(int filterType)
        {
            if (_mainCanvas != null)
            {
                BitmapSource source = (BitmapSource)_mainCanvas.Children[0].GetValue(Image.SourceProperty);

                if (source != null)
                {
                    // Konwersja obrazu na format WriteableBitmap, aby móc modyfikować piksele
                    WriteableBitmap writableBitmap = new WriteableBitmap(source);

                    int width = writableBitmap.PixelWidth;
                    int height = writableBitmap.PixelHeight;

                    // Konwersja obrazu na format array pikseli
                    int stride = width * 4; // 4 kanały (RGBA) na piksel
                    byte[] pixels = new byte[height * stride];
                    writableBitmap.CopyPixels(pixels, stride, 0);

                    int radius = 1; // Promień filtra 

                    for (int y = radius; y < height - radius; y++)
                    {
                        for (int x = radius; x < width - radius; x++)
                        {
                            switch (filterType)
                            {
                                case 0:
                                    ApplyAveragePixelFilter(x, y, radius, writableBitmap, pixels, stride);
                                    break;
                                case 1:
                                    ApplyMedianPixelFilter(x, y, radius, writableBitmap, pixels, stride);
                                    break;
                                case 2:
                                    ApplySobelEdgeDetection(x, y, writableBitmap, pixels, stride);
                                    break;
                                case 3:
                                    ApplyHighPassFilter(x, y, writableBitmap, pixels, stride);
                                    break;
                                case 4:
                                    ApplyGaussianBlurFilter(x, y, writableBitmap, pixels, stride);
                                    break;
                            }
                        }
                    }

                    // Ustawienie zmodyfikowanych pikseli z powrotem do obrazu
                    writableBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);

                    // Ustawienie zmodyfikowanego obrazu z powrotem na Canvas
                    Image modifiedImage = new Image();
                    modifiedImage.Source = writableBitmap;

                    _mainCanvas.Children.Clear();
                    _mainCanvas.Children.Add(modifiedImage);
                }
            }
        }

        private void ApplyAveragePixelFilter(int x, int y, int radius, WriteableBitmap writableBitmap, byte[] pixels, int stride)
        {
            int sumR = 0, sumG = 0, sumB = 0;

            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    int offsetX = x + i;
                    int offsetY = y + j;

                    // Pobierz składowe koloru piksela
                    byte[] pixel = new byte[4];
                    int pixelIndex = offsetY * stride + offsetX * 4;
                    Array.Copy(pixels, pixelIndex, pixel, 0, 4);

                    sumR += pixel[2]; // Red
                    sumG += pixel[1]; // Green
                    sumB += pixel[0]; // Blue
                }
            }

            // Średnie wartości kolorów
            byte averageR = (byte)(sumR / ((2 * radius + 1) * (2 * radius + 1)));
            byte averageG = (byte)(sumG / ((2 * radius + 1) * (2 * radius + 1)));
            byte averageB = (byte)(sumB / ((2 * radius + 1) * (2 * radius + 1)));

            // Ustaw nowe wartości piksela
            int currentIndex = y * stride + x * 4;
            pixels[currentIndex + 2] = averageR; // Red
            pixels[currentIndex + 1] = averageG; // Green
            pixels[currentIndex] = averageB;     // Blue
        }

        private void ApplyMedianPixelFilter(int x, int y, int radius, WriteableBitmap writableBitmap, byte[] pixels, int stride)
        {
            List<byte> redValues = new List<byte>();
            List<byte> greenValues = new List<byte>();
            List<byte> blueValues = new List<byte>();

            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    int offsetX = x + i;
                    int offsetY = y + j;

                    // Pobierz składowe koloru piksela
                    byte[] pixel = new byte[4];
                    int pixelIndex = offsetY * stride + offsetX * 4;
                    Array.Copy(pixels, pixelIndex, pixel, 0, 4);

                    redValues.Add(pixel[2]); // Red
                    greenValues.Add(pixel[1]); // Green
                    blueValues.Add(pixel[0]); // Blue
                }
            }

            // Posortuj listy wartości kolorów
            redValues.Sort();
            greenValues.Sort();
            blueValues.Sort();

            // Wybierz medianę z posortowanych wartości
            byte medianR = redValues[redValues.Count / 2];
            byte medianG = greenValues[greenValues.Count / 2];
            byte medianB = blueValues[blueValues.Count / 2];

            // Ustaw nowe wartości piksela
            int currentIndex = y * stride + x * 4;
            pixels[currentIndex + 2] = medianR; // Red
            pixels[currentIndex + 1] = medianG; // Green
            pixels[currentIndex] = medianB;     // Blue
        }

        private void ApplySobelEdgeDetection(int x, int y, WriteableBitmap writableBitmap, byte[] pixels, int stride)
        {
            int width = writableBitmap.PixelWidth;
            int height = writableBitmap.PixelHeight;

            // Macierze Sobela do detekcji krawędzi
            int[,] sobelX = { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            int[,] sobelY = { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };

            int intensityX = 0;
            int intensityY = 0;

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int offsetX = x + i;
                    int offsetY = y + j;

                    if (offsetX >= 0 && offsetX < width && offsetY >= 0 && offsetY < height)
                    {
                        int pixelIndex = offsetY * stride + offsetX * 4;
                        byte intensity = (byte)(0.299 * pixels[pixelIndex + 2] + 0.587 * pixels[pixelIndex + 1] + 0.114 * pixels[pixelIndex]);

                        intensityX += intensity * sobelX[i + 1, j + 1];
                        intensityY += intensity * sobelY[i + 1, j + 1];
                    }
                }
            }

            // Oblicz kierunek gradientu
            double gradientDirection = Math.Atan2(intensityY, intensityX);

            // Oblicz moduł gradientu
            int gradientMagnitude = (int)Math.Sqrt(intensityX * intensityX + intensityY * intensityY);

            // Normalizuj moduł gradientu
            int normalizedMagnitude = Math.Min(255, gradientMagnitude);

            // Ustaw nową wartość piksela
            int currentIndex = y * stride + x * 4;
            pixels[currentIndex + 2] = (byte)normalizedMagnitude; // Red
            pixels[currentIndex + 1] = (byte)normalizedMagnitude; // Green
            pixels[currentIndex] = (byte)normalizedMagnitude;     // Blue
        }

        private void ApplyHighPassFilter(int x, int y, WriteableBitmap writableBitmap, byte[] pixels, int stride)
        {
            int[,] kernel = {
                { -1, -1, -1 },
                { -1,  9, -1 },
                { -1, -1, -1 }
            };

            int width = writableBitmap.PixelWidth;
            int height = writableBitmap.PixelHeight;

            int radius = kernel.GetLength(0) / 2;

            int sumR = 0, sumG = 0, sumB = 0;

            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    int offsetX = x + i;
                    int offsetY = y + j;

                    if (offsetX >= 0 && offsetX < width && offsetY >= 0 && offsetY < height)
                    {
                        int pixelIndex = offsetY * stride + offsetX * 4;
                        byte[] pixel = new byte[4];
                        Array.Copy(pixels, pixelIndex, pixel, 0, 4);

                        sumR += pixel[2] * kernel[i + radius, j + radius]; // Red
                        sumG += pixel[1] * kernel[i + radius, j + radius]; // Green
                        sumB += pixel[0] * kernel[i + radius, j + radius]; // Blue
                    }
                }
            }

            // Ustaw nowe wartości piksela
            int currentIndex = y * stride + x * 4;
            byte newR = (byte)Math.Max(0, Math.Min(255, sumR));
            byte newG = (byte)Math.Max(0, Math.Min(255, sumG));
            byte newB = (byte)Math.Max(0, Math.Min(255, sumB));

            pixels[currentIndex + 2] = newR; // Red
            pixels[currentIndex + 1] = newG; // Green
            pixels[currentIndex] = newB;     // Blue
        }
        private void ApplyGaussianBlurFilter(int x, int y, WriteableBitmap writableBitmap, byte[] pixels, int stride)
        {
            int width = writableBitmap.PixelWidth;
            int height = writableBitmap.PixelHeight;

            // Kernel Gaussa
            double[,] kernel = {
                { 1, 2, 1 },
                { 2, 4, 2 },
                { 1, 2, 1 }
            };

            int radius = kernel.GetLength(0) / 2;

            double sumR = 0, sumG = 0, sumB = 0;
            double sumKernel = 0;

            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    int offsetX = x + i;
                    int offsetY = y + j;

                    if (offsetX >= 0 && offsetX < width && offsetY >= 0 && offsetY < height)
                    {
                        int pixelIndex = offsetY * stride + offsetX * 4;
                        byte[] pixel = new byte[4];
                        Array.Copy(pixels, pixelIndex, pixel, 0, 4);

                        double kernelValue = kernel[i + radius, j + radius];

                        sumR += pixel[2] * kernelValue; // Red
                        sumG += pixel[1] * kernelValue; // Green
                        sumB += pixel[0] * kernelValue; // Blue
                        sumKernel += kernelValue;
                    }
                }
            }

            // Ustaw nowe wartości piksela
            int currentIndex = y * stride + x * 4;
            byte newR = (byte)Math.Max(0, Math.Min(255, sumR / sumKernel));
            byte newG = (byte)Math.Max(0, Math.Min(255, sumG / sumKernel));
            byte newB = (byte)Math.Max(0, Math.Min(255, sumB / sumKernel));

            pixels[currentIndex + 2] = newR; // Red
            pixels[currentIndex + 1] = newG; // Greens
            pixels[currentIndex] = newB;     // Blue
        }



    }
}
