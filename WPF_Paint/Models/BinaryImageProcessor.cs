using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Paint.Models
{
    public class BinaryImageProcessor
    {
        private byte[] image;
        private byte[] largestShapeBitmap;
        private int width;
        private int height;
        private bool[,] visited;
        public int[,] shapes;
        private byte serchedValue = 1; 

        public BinaryImageProcessor(byte[] image, int width, int height)
        {
            this.image = image;
            this.width = width;
            this.height = height;
            largestShapeBitmap = new byte[height * width];
            visited = new bool[height, width];
            shapes = new int[height, width];
        }

        private int FindLargestBlackShape()
        {
            int largestSize = 0;
            int largestIndex = 0;
            int index = 1;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (image[y * width + x] == serchedValue && !visited[y, x])
                    {
                        int size = ExploreShape(index, x, y);
                        if (size > largestSize)
                        {
                            largestSize = size;
                            largestIndex = index;
                        }
                        index++;
                    }
                }
            }

            return largestIndex;
        }

        public byte[] GetLargestShape()
        {
            int largestIndex = FindLargestBlackShape();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    largestShapeBitmap[y * width + x] = (byte)((shapes[y, x] == largestIndex)? 1 : 0);
                }
            }
            return largestShapeBitmap;
        }

        private int ExploreShape(int index, int startX, int startY)
        {
            Stack<(int x, int y)> stack = new Stack<(int x, int y)>();
            stack.Push((startX, startY));
            visited[startY, startX] = true;
            shapes[startY, startX] = index;

            int size = 0;

            while (stack.Count > 0)
            {
                var (x, y) = stack.Pop();
                size++;

                foreach (var (nx, ny) in GetNeighborsBig(x, y))
                {
                    if (!visited[ny, nx] && image[ny * width + nx] == serchedValue)
                    {
                        stack.Push((nx, ny));
                        visited[ny, nx] = true;
                        shapes[ny, nx] = index;
                    }
                }
            }

            return size;
        }

        private IEnumerable<(int x, int y)> GetNeighbors(int x, int y)
        {
            int[] dx = { -1, 0, 1, 0 };
            int[] dy = { 0, 1, 0, -1 };

            for (int i = 0; i < 4; i++)
            {
                int nx = x + dx[i], ny = y + dy[i];
                if (nx >= 0 && ny >= 0 && nx < width && ny < height)
                {
                    yield return (nx, ny);
                }
            }
        }

        private IEnumerable<(int x, int y)> GetNeighborsBig(int x, int y) 
        {
            int[] dx = { -1, -1, -1, 0, 0, 1, 1, 1 };
            int[] dy = { -1, 0, 1, -1, 1, -1, 0, 1 };

            for (int i = 0; i < 8; i++)
            {
                int nx = x + dx[i], ny = y + dy[i];
                if (nx >= 0 && ny >= 0 && nx < width && ny < height)
                {
                    yield return (nx, ny);
                }
            }
        }
    }
}
