using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;

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
    }
}

