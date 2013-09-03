using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace MSELocator
{
    public class Line
    {
        #region Properties and Constructor

        private Point? _startPoint;
        public Point? startPoint
        {
            get { return _startPoint; }
            set { _startPoint = value; }
        }

        private Point? _endPoint;
        public Point? endPoint
        {
            get { return _endPoint; }
            set { _endPoint = value; }
        }

        private Double? _slope;
        public Double? slope
        {
            get { return _slope; }
            set { _slope = value; }
        }

        private Double _n; //n is the y interecept of the line equation
        public Double n
        {
            get { return _n; }
            set { _n = value; }
        }

        private bool _isVerticalLine;
        public bool isVerticalLine
        {
            get { return _isVerticalLine; }
            set { _isVerticalLine = value; }
        }

        private Double _x;
        public Double x
        {
            get { return _x; }
            set { _x = value; }
        }

        private bool _isLineSegment; //a line segement is not an infinite line, it has a start point and an end point
        public bool isLineSegement
        {
            get { return _isLineSegment; }
            set { _isLineSegment = value; }
        }

        public Line(Point start, Point end)
        {
            startPoint = start;
            endPoint = end;
            isLineSegement = true;

            //special exception when the line is vertical
            if (endPoint.Value.X == startPoint.Value.X)
            {
                isVerticalLine = true;
                x = startPoint.Value.X;
                slope = null;
            }
            else
            {
                isVerticalLine = false;
                slope = (endPoint.Value.Y - startPoint.Value.Y) / (endPoint.Value.X - startPoint.Value.X);
                n = startPoint.Value.Y - (Double)slope * startPoint.Value.X;
            }
        }

        public Line(Point? start, Double? orientation)
        {
            startPoint = start;
            isLineSegement = false;

            if (orientation == 90 || orientation == 270)
            {
                isVerticalLine = true;
                x = startPoint.Value.X;
                slope = null;
            }
            else
            {
                isVerticalLine = false;
                slope = (Double)orientation * Math.PI / 180;
                slope = Math.Tan((Double)slope);
                n = startPoint.Value.Y - (Double)slope * startPoint.Value.X;
            }
        }
        #endregion

        # region Line Fucntions
        public static Point? getIntersectionPoint(Line line1, Line line2)
        {
            Point? IntersectionPoint = null;
            
            //if lines are parallel
            if (line1.isVerticalLine && line2.isVerticalLine || line1.slope == line2.slope)
                return null;
            else if (line1.isVerticalLine)
            {
                Double yValue = (Double)line2.slope * line1.x + line2.n;
                IntersectionPoint = new Point(line1.x, yValue);
            }
            else if (line2.isVerticalLine)
            {
                Double yValue = (Double)line1.slope * line2.x + line1.n;
                IntersectionPoint = new Point(line2.x, yValue);
            }
            else
            {
                Double xValue = (line2.n - line1.n) / (Double)(line1.slope - line2.slope);
                Double yValue = (Double)line1.slope * xValue + line1.n;
                IntersectionPoint = new Point(xValue, yValue);
            }

            if (line1.isLineSegement)
            {
                if(isGreater(IntersectionPoint.Value.X, line1.startPoint.Value.X) && isGreater(IntersectionPoint.Value.X, line1.endPoint.Value.X) ||
                    isLess(IntersectionPoint.Value.X, line1.startPoint.Value.X) && isLess(IntersectionPoint.Value.X, line1.endPoint.Value.X) ||
                    isGreater(IntersectionPoint.Value.Y, line1.startPoint.Value.Y) && isGreater(IntersectionPoint.Value.Y, line1.endPoint.Value.Y) ||
                    isLess(IntersectionPoint.Value.Y, line1.startPoint.Value.Y) && isLess(IntersectionPoint.Value.Y, line1.endPoint.Value.Y))
                    return null;
            }
            
            if (line2.isLineSegement)
            {
                if (isGreater(IntersectionPoint.Value.X, line2.startPoint.Value.X) && isGreater(IntersectionPoint.Value.X, line2.endPoint.Value.X) ||
                    isLess(IntersectionPoint.Value.X, line2.startPoint.Value.X) && isLess(IntersectionPoint.Value.X, line2.endPoint.Value.X) ||
                    isGreater(IntersectionPoint.Value.Y, line2.startPoint.Value.Y) && isGreater(IntersectionPoint.Value.Y, line2.endPoint.Value.Y) ||
                    isLess(IntersectionPoint.Value.Y, line2.startPoint.Value.Y) && isLess(IntersectionPoint.Value.Y, line2.endPoint.Value.Y))
                    return null;
            }
             
            return IntersectionPoint;
        }

        public static double getDistanceBetweenPoints(Point p1, Point p2)
        {
            double distance = Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2);
            distance = Math.Sqrt(distance);
            return distance;
        }

        public static bool isGreater(Double num1, Double num2)
        {
            Double answer = num1 - num2;
            answer = Math.Round(answer, 3);
            if (answer > 0) { return true; }
            return false;
        }

        public static bool isLess(Double num1, Double num2)
        {
            Double answer = num1 - num2;
            answer = Math.Round(answer, 3);
            if (answer < 0) { return true; }
            return false;
        }

        #endregion
    }
}
