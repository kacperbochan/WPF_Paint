using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WPF_Paint.Models
{
    public class Bezier
    {
        private  List<ObservablePoint> _bezierPoints = new List<ObservablePoint>();
        public List<ObservablePoint> BezierPoints
        {
            get { return _bezierPoints; }
            set
            {
                _bezierPoints = value;
            }
        }

        private List<Line> supportLines = new List<Line>();
        private Path _bezierPath;
        private Canvas _canvas;

        // You can create an event for notifying about changes
        public event Action PointsChanged;

        // Call this method when points are added or removed
        public void NotifyPointsChanged()
        {
            PointsChanged?.Invoke();
        }

        public Bezier(){}

        public void SetMainCanvas(Canvas canvas)
        {
            _canvas = canvas;
        }

        private double DistanceBetweenPoints(Point point1, ObservablePoint point2)
        {
            double deltaX = point2.X - point1.X;
            double deltaY = point2.Y - point1.Y;
            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }

        public int GetClosestBezierPoint(Point cursor)
        {
            double minDistance = 10;
            int closestPoint = -1;

            for (int i = 0; i < BezierPoints.Count; i++)
            {
                double distance = DistanceBetweenPoints(cursor, BezierPoints[i]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestPoint = i;
                }
            }

            return closestPoint;
        }

        private double BinomialCoefficient(int n, int k)
        {
            double result = 1;
            for (int i = 1; i <= k; i++)
            {
                result *= (n + 1 - i) / (double)i;
            }
            return result;
        }

        private Point CalculateBezierPoint(double t, List<ObservablePoint> points)
        {
            int n = points.Count - 1;
            double x = 0;
            double y = 0;

            for (int i = 0; i <= n; i++)
            {
                double binCoeff = BinomialCoefficient(n, i);
                double term = binCoeff * Math.Pow(t, i) * Math.Pow(1 - t, n - i);

                x += term * points[i].X;
                y += term * points[i].Y;
            }

            return new Point(x, y);
        }

        private void DrawSupportNet()
        {
            // Update or add new lines
            for (int i = 0; i < BezierPoints.Count - 1; i++)
            {
                Line line;
                if (i < supportLines.Count)
                {
                    // Update existing line
                    line = supportLines[i];
                }
                else
                {
                    // Create new line and add to canvas and list
                    line = new Line { Stroke = new SolidColorBrush(ColorSettings.BorderColor) };
                    supportLines.Add(line);
                    _canvas.Children.Add(line);
                }

                line.X1 = BezierPoints[i].X;
                line.Y1 = BezierPoints[i].Y;
                line.X2 = BezierPoints[i + 1].X;
                line.Y2 = BezierPoints[i + 1].Y;
            }
        }

        public void UpdateBezierPreview()
        {
            DrawSupportNet();

            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = new Point(BezierPoints[0].X,BezierPoints[0].Y) ; // Start at the first point

            PathGeometry pathGeometry = new PathGeometry();
            PolyLineSegment polyLine = new PolyLineSegment();

            for (double t = 0; t <= 1; t += 0.01) // Adjust the step for precision
            {
                Point bezierPoint = CalculateBezierPoint(t, BezierPoints);
                polyLine.Points.Add(bezierPoint);
            }

            pathFigure.Segments.Add(polyLine);
            pathGeometry.Figures.Add(pathFigure);


            if (supportLines.Count == 0)
            {
                _bezierPath = new System.Windows.Shapes.Path
                {
                    Stroke = new SolidColorBrush(ColorSettings.FillColor),
                    StrokeThickness = 2,
                    Data = pathGeometry
                };

                _canvas.Children.Add(_bezierPath);
            }
            else
            {
                _bezierPath.Data = pathGeometry;
            }
            NotifyPointsChanged();
        }

        public void RemoveSupportNet()
        {
            for(int i=0; i<supportLines.Count; i++)
                _canvas.Children.Remove(supportLines[i]);
        }

        public void ClearData()
        {
            BezierPoints.Clear();
            supportLines.Clear();
            NotifyPointsChanged();
        }
    }
}
