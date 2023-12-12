using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Diagnostics;

namespace WPF_Paint.Models
{
    internal static class Filter
    {
        public static void ApplyAveragePixelFilter(int x, int y, int radius, WriteableBitmap writableBitmap, byte[] sourcePixels, byte[] bufferpixels, int stride)
        {
            int sumR = 0, sumG = 0, sumB = 0;
            
            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    int offsetX = Math.Clamp(x + i, 0, (int)writableBitmap.Width - 1);
                    int offsetY = Math.Clamp(y + j, 0, (int)writableBitmap.Height - 1);



                    // Pobierz składowe koloru piksela
                    byte[] pixel = new byte[4];
                    int pixelIndex = offsetY * stride + offsetX * 4;
                    
                    Array.Copy(sourcePixels, pixelIndex, pixel, 0, 4);

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
            bufferpixels[currentIndex + 2] = averageR; // Red
            bufferpixels[currentIndex + 1] = averageG; // Green
            bufferpixels[currentIndex] = averageB;     // Blue
        }

        public static void ApplyMedianPixelFilter(int x, int y, int radius, WriteableBitmap writableBitmap, byte[] sourcePixels, byte[] bufferpixels, int stride)
        {
            List<byte> redValues = new List<byte>();
            List<byte> greenValues = new List<byte>();
            List<byte> blueValues = new List<byte>();

            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    int offsetX = Math.Clamp(x + i, 0, (int)writableBitmap.Width - 1);
                    int offsetY = Math.Clamp(y + j, 0, (int)writableBitmap.Height - 1);

                    // Pobierz składowe koloru piksela
                    byte[] pixel = new byte[4];
                    int pixelIndex = offsetY * stride + offsetX * 4;
                    Array.Copy(sourcePixels, pixelIndex, pixel, 0, 4);

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
            bufferpixels[currentIndex + 2] = medianR; // Red
            bufferpixels[currentIndex + 1] = medianG; // Green
            bufferpixels[currentIndex] = medianB;     // Blue
        }

        public static void ApplySobelEdgeDetection(int x, int y, WriteableBitmap writableBitmap, byte[] sourcePixels, byte[] bufferpixels, int stride)
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
                    int offsetX = Math.Clamp(x + i, 0, (int)writableBitmap.Width - 1);
                    int offsetY = Math.Clamp(y + j, 0, (int)writableBitmap.Height - 1);

                    if (offsetX >= 0 && offsetX < width && offsetY >= 0 && offsetY < height)
                    {
                        int pixelIndex = offsetY * stride + offsetX * 4;
                        byte intensity = (byte)(0.299 * sourcePixels[pixelIndex + 2] + 0.587 * sourcePixels[pixelIndex + 1] + 0.114 * sourcePixels[pixelIndex]);

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
            bufferpixels[currentIndex + 2] = (byte)normalizedMagnitude; // Red
            bufferpixels[currentIndex + 1] = (byte)normalizedMagnitude; // Green
            bufferpixels[currentIndex] = (byte)normalizedMagnitude;     // Blue
        }



        public static void ApplyHighPassFilter(int x, int y, WriteableBitmap writableBitmap, byte[] sourcePixels, byte[] bufferpixels, int stride)
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
                    int offsetX = Math.Clamp(x + i, 0, (int)writableBitmap.Width - 1);
                    int offsetY = Math.Clamp(y + j, 0, (int)writableBitmap.Height - 1);

                    if (offsetX >= 0 && offsetX < width && offsetY >= 0 && offsetY < height)
                    {
                        int pixelIndex = offsetY * stride + offsetX * 4;
                        byte[] pixel = new byte[4];
                        Array.Copy(sourcePixels, pixelIndex, pixel, 0, 4);

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

            bufferpixels[currentIndex + 2] = newR; // Red
            bufferpixels[currentIndex + 1] = newG; // Green
            bufferpixels[currentIndex] = newB;     // Blue
        }

        public static void ApplyGaussianBlurFilter(int x, int y, WriteableBitmap writableBitmap, byte[] sourcePixels, byte[] bufferpixels, int stride)
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
                    int offsetX = Math.Clamp(x + i, 0, (int)writableBitmap.Width - 1);
                    int offsetY = Math.Clamp(y + j, 0, (int)writableBitmap.Height - 1);

                    
                    int pixelIndex = offsetY * stride + offsetX * 4;
                    byte[] pixel = new byte[4];
                    Array.Copy(sourcePixels, pixelIndex, pixel, 0, 4);

                    double kernelValue = kernel[i + radius, j + radius];

                    sumR += pixel[2] * kernelValue; // Red
                    sumG += pixel[1] * kernelValue; // Green
                    sumB += pixel[0] * kernelValue; // Blue
                    sumKernel += kernelValue;
                    
                }
            }

            // Ustaw nowe wartości piksela
            int currentIndex = y * stride + x * 4;
            byte newR = (byte)Math.Max(0, Math.Min(255, sumR / sumKernel));
            byte newG = (byte)Math.Max(0, Math.Min(255, sumG / sumKernel));
            byte newB = (byte)Math.Max(0, Math.Min(255, sumB / sumKernel));

            bufferpixels[currentIndex + 2] = newR; // Red
            bufferpixels[currentIndex + 1] = newG; // Greens
            bufferpixels[currentIndex] = newB;     // Blue
        }

        public static void ApplyCustomFilter(int x, int y, double[,] kernel, WriteableBitmap writableBitmap, byte[] sourcePixels, byte[] bufferpixels, int stride)
        {
            int width = writableBitmap.PixelWidth;
            int height = writableBitmap.PixelHeight;

            int radius = kernel.GetLength(0) / 2;

            double sumR = 0, sumG = 0, sumB = 0;
            double sumKernel = 0;

            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    int offsetX = Math.Clamp(x + i, 0, (int)writableBitmap.Width - 1);
                    int offsetY = Math.Clamp(y + j, 0, (int)writableBitmap.Height - 1);

                    if (offsetX >= 0 && offsetX < width && offsetY >= 0 && offsetY < height)
                    {
                        int pixelIndex = offsetY * stride + offsetX * 4;
                        byte[] pixel = new byte[4];
                        Array.Copy(sourcePixels, pixelIndex, pixel, 0, 4);

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

            bufferpixels[currentIndex + 2] = newR; // Red
            bufferpixels[currentIndex + 1] = newG; // Greens
            bufferpixels[currentIndex] = newB;     // Blue
        }

        public static void ApplyMDBUTMFFilter(byte[] pixels, int width, int height, int stride)
        {
            // Logika filtrowania MDBUTMF
            Trace.WriteLine("ApplyMDBUTMFFilter");
            byte[] outputPixels = new byte[pixels.Length];
            Array.Copy(pixels, outputPixels, pixels.Length);

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int pixelIndex = y * stride + x * 4;

                    if (IsNoisePixel(pixels, x, y, stride, width, height))
                    {
                        // Dla każdego kanału koloru (B, G, R)
                        for (int i = 0; i < 3; i++)
                        {
                            List<byte> neighborhood = GetNeighborhood(pixels, x, y, stride, i);
                            byte medianValue = GetMedian(neighborhood);
                            outputPixels[pixelIndex + i] = medianValue;
                        }
                    }
                    else
                    {
                        // Zachowaj oryginalny piksel
                        for (int i = 0; i < 3; i++)
                        {
                            outputPixels[pixelIndex + i] = pixels[pixelIndex + i];
                        }
                    }
                }
            }

            Array.Copy(outputPixels, pixels, pixels.Length);
        }

        private static List<byte> GetNeighborhood(byte[] pixels, int x, int y, int stride, int channelOffset)
        {
            List<byte> neighborhood = new List<byte>();

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int pixelIndex = (y + i) * stride + (x + j) * 4;
                    neighborhood.Add(pixels[pixelIndex + channelOffset]);
                }
            }

            return neighborhood;
        }

        private static bool IsNoisePixel(byte[] pixels, int x, int y, int stride, int width, int height)
        {
            int pixelIndex = y * stride + x * 4;
            byte pixelValue = pixels[pixelIndex];

            // Progi do określenia, czy piksel jest znacznie jaśniejszy lub ciemniejszy od sąsiadów
            const byte noiseThreshold = 30;

            int differentNeighbors = 0;
            int totalNeighbors = 0;

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0) continue; // Pomijamy sam piksel

                    int neighborX = x + j;
                    int neighborY = y + i;

                    // Sprawdzamy, czy sąsiad jest w granicach obrazu
                    if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height)
                    {
                        totalNeighbors++;
                        int neighborIndex = neighborY * stride + neighborX * 4;
                        byte neighborValue = pixels[neighborIndex];

                        // Porównujemy wartość piksela z wartością sąsiada
                        if (Math.Abs(pixelValue - neighborValue) > noiseThreshold)
                        {
                            differentNeighbors++;
                        }
                    }
                }
            }

            // Uznajemy piksel za szum, jeśli większość jego sąsiadów różni się od niego o próg
            return differentNeighbors > totalNeighbors / 2;
        }


        private static byte GetMedian(List<byte> values)
        {
            values.Sort();
            int middleIndex = values.Count / 2;
            return values[middleIndex];
        }
    }
}

