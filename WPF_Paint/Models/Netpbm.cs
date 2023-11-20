using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Ink;

namespace WPF_Paint.Models
{
    internal class Netpbm
    {
        private int _px = 0;
        private int _width = 0;
        private int _height = 0;
        private int _maxValue = 0;
        private bool _hasMaxValue = false;
        private byte[] _content;
        private BitmapSource _bitmap;

        public int Px
        {
            get { return _px; }
            set { _px = value; }
        }
        public int Width {
            get { return _width; }
            set { _width = value; }
        }
        public int Height
        {
            get { return _height; }
            set { _height = value; }
        }
        public int MaxValue { 
            get { return _maxValue; }
            set { _maxValue = value; }
        }
        public bool HasMaxValue
        {
            get { return _hasMaxValue; }
        }
        public byte[] Content
        {
            get { return _content; }
            set { _content = value; }
        }
        public BitmapSource Bitmap
        {
            get { return _bitmap; }
            set { _bitmap = value; }
        }

        public Netpbm(string filePath)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            {
                HeatherDataExtraciton(fileStream);

                //Bellow we have content data extraction
                long contentLenght = (fileStream.Length - fileStream.Position);
                byte[] content = new byte[contentLenght];
                fileStream.Read(content, 0, content.Length);
                _content = content;
            }

            RemoveComments();
            BitToPixels();
        }

        /// <summary>
        /// Reads through the headher and extracts all data from it.
        /// Ends after last variable
        /// </summary>
        /// <param name="fileStream"></param>
        private void HeatherDataExtraciton(FileStream fileStream)
        {
            int requiredHeaderParts = 3; // Default for PBM
            int sections = 0;
            bool inSection = false;
            StringBuilder currentSection = new StringBuilder();
            char prevChar = '\0';

            while (true)
            {
                int b = fileStream.ReadByte();
                if (b == -1) // End of file
                    break;

                char c = (char)b;

                if (c == '#')
                {
                    do
                    {
                        c = (char)fileStream.ReadByte();
                    } while (c == '\n' || c == '\r');
                }
                else if (prevChar == 'P' && char.IsNumber(c))
                {
                    _px = c - 48;
                    if (_px != 1 && _px != 4)
                    {
                        requiredHeaderParts = 4; // For PGM and PPM
                    }
                    sections++;
                    break;
                }
                prevChar = c;
            }

            while (true)
            {
                int b = fileStream.ReadByte();
                if (b == -1) // End of file
                    break;

                char c = (char)b;

                
                if (char.IsWhiteSpace(c))
                {
                    if (inSection)
                    {
                        string sectionValue = currentSection.ToString();
                        switch (sections)
                        {
                            case 1: _width = int.Parse(sectionValue); break;
                            case 2: _height = int.Parse(sectionValue); break;
                            case 3: _maxValue = int.Parse(sectionValue); break;
                        }

                        sections++;
                        inSection = false;
                        currentSection.Clear();

                        if (sections == requiredHeaderParts)
                        {
                            break;
                        }
                    }
                }
                else if (c != '#')
                {
                    inSection = true;
                    currentSection.Append(c);
                }
                else if (c == '#')
                {
                    while (true)
                    {
                        c = (char)fileStream.ReadByte();
                        if (c == '\n' || c == '\r') break;
                    }
                }

            }
        }

        public bool ContainsComment()
        {
            return Array.IndexOf(_content, (byte)35) != -1;
        }

        /// <summary>
        /// Removes comments from content
        /// </summary>
        private void RemoveComments()
        {
            if (!ContainsComment()) return;

            if(_px > 3)
            {
                int expectedContentLength = _height * _width;
                if (_px == 4) expectedContentLength = _height * (int)Math.Ceiling((decimal)_width / 8);
                else if (_px == 6) expectedContentLength *= 3;

                // Check if the length matches or is shorter; no action needed in this case
                if (_content.Length <= expectedContentLength) return;

                // Calculate the starting index of the actual content
                int startIndex = _content.Length - expectedContentLength;

                // Create a new array to hold the adjusted content
                byte[] adjustedContent = new byte[expectedContentLength];

                // Copy the relevant part of _content into adjustedContent
                Array.Copy(_content, startIndex, adjustedContent, 0, expectedContentLength);

                // Assign the adjusted content back to _content
                _content = adjustedContent;

            }
            else
            {
                List<byte> clearContentList = new List<byte>();
                bool skip = false;

                foreach (var b in _content)
                {
                    if (b == 35) // '#' character
                    {
                        skip = true;
                        continue;
                    }

                    if (skip && (b == '\n' || b == '\r'))
                    {
                        skip = false;
                        continue;
                    }

                    if (!skip)
                    {
                        clearContentList.Add(b);
                    }
                }

                _content = clearContentList.ToArray();
            }
        }

        private void BitToPixels()
        {
            switch (_px)
            {
                case 1:
                    bitToP1();
                    break; 
                case 2:
                    bitToP2();
                    break; 
                case 3:
                    bitToP3();
                    break; 
                case 4:
                    bitToP4();
                    break; 
                case 5:
                    bitToP5();
                    break; 
                case 6:
                    bitToP6();
                    break;
            }
        }

        private void bitToP1()
        {
            byte[] pixels = new byte[_width * _height];

            int pixelCount = 0;

            for (int i = 0; i < _content.Length; i++)
            {
                char c = (char)_content[i];
                if (char.IsDigit(c))
                {
                    pixels[pixelCount++] = (byte)((c == '1') ? 0 : 255);
                }
            }

            _bitmap = BitmapSource.Create(_width, _height, 96, 96, PixelFormats.Gray8, null, pixels, _width);
        }

        private void bitToP2()
        {
            byte[] pixels = new byte[_width * _height];

            int pixelCount = 0;
            bool inNumber = false;
            StringBuilder currentNumber = new StringBuilder();

            for (int i = 0; i < _content.Length; i++)
            {
                char c = (char)_content[i];
                if (char.IsDigit(c))
                {
                    inNumber = true;
                    currentNumber.Append(c);
                }
                else if (inNumber && char.IsWhiteSpace(c))
                {
                    inNumber = false;
                    string number = currentNumber.ToString();

                    double normalizedValue = (int.Parse(number) * 255 / (double)MaxValue);
                    pixels[pixelCount++] = (byte)Math.Clamp(normalizedValue, 0, 255);
                    currentNumber.Clear();
                }
            }

            _bitmap = BitmapSource.Create(_width, _height, 96, 96, PixelFormats.Gray8, null, pixels, _width);
        }

        private void bitToP3()
        {
            byte[] pixels = new byte[_width * _height * 3];

            int pixelCount = 0;
            bool inNumber = false;
            StringBuilder currentNumber = new StringBuilder();

            for (int i = 0; i < _content.Length; i++)
            {
                char c = (char)_content[i];
                if (char.IsDigit(c))
                {
                    inNumber = true;
                    currentNumber.Append(c);
                }
                else if (inNumber && char.IsWhiteSpace(c))
                {
                    inNumber = false;
                    string number = currentNumber.ToString();

                    double normalizedValue = (int.Parse(number) * 255 / (double)MaxValue);
                    pixels[pixelCount++] = (byte)Math.Clamp(normalizedValue, 0, 255);
                    currentNumber.Clear();
                }
            }

            _bitmap = BitmapSource.Create(_width, _height, 96, 96, PixelFormats.Rgb24, null, pixels, _width * 3);
        }

        private void bitToP4()
        {
            byte[] pixels = new byte[_width * _height];

            for (int row = 0; row < _height; row++)
            {
                for (int col = 0; col < _width; col++)
                {
                    int byteIndex = col / 8;
                    int bitIndex = 7 - (col % 8); // From most significant to least significant bit
                    byte mask = (byte)(1 << bitIndex);
                    pixels[row * _width + col] = (byte)((_content[byteIndex] & mask) != 0 ? 0 : 255); // 0 for black, 255 for white
                }
            }

            _bitmap = BitmapSource.Create(_width, _height, 96, 96, PixelFormats.Gray8, null, pixels, _width);
        }

        private void bitToP5()
        {
            if (MaxValue != 255)
            {
                for (int i = 0; i < _content.Length; i++)
                {
                    // Normalize the pixel value
                    double normalizedValue = (_content[i] * 255 / (double)MaxValue);
                    _content[i] = (byte)Math.Clamp(normalizedValue, 0, 255);
                }
            }

            _bitmap = BitmapSource.Create(_width, _height, 96, 96, PixelFormats.Gray8, null, _content, _width);
        }
        
        private void bitToP6()
        {
            if (MaxValue != 255)
            {
                for (int i = 0; i < _content.Length; i++)
                {
                    // Normalize each component of the pixel
                    double normalizedValue = (_content[i] *255 / (double)MaxValue);
                    _content[i] = (byte)Math.Clamp(normalizedValue, 0, 255);
                }
            }

            int stride = _width * 3; // Stride for Rgb24 format
            _bitmap = BitmapSource.Create(_width, _height, 96, 96, PixelFormats.Rgb24, null, _content, stride);
        }
    }
    }
