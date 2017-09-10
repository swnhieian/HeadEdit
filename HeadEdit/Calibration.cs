using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HeadEdit
{
    class Calibration
    {
        private int calibrateNo;
        private bool isCalibrated;
        private bool isCalibrating;
        private Point[] calibratePoints;
        private Point currentPoint;
        private double lastX, lastY;
        public Calibration()
        {
            calibrateNo = -1;
            isCalibrated = false;
            isCalibrating = false;
            calibratePoints = new Point[4];
            currentPoint = new Point();
            lastX = Double.NaN;
            lastY = Double.NaN;
        }
        public void startCalibrate()
        {
            if (isCalibrated)
            {
                isCalibrated = false;
                calibrateNo = -1;
            }
            isCalibrating = true;
            if (calibrateNo == -1)
            {
                MessageBox.Show("Start Calibration");
                calibrateNo = 0;
            } else
            {
                calibratePoints[calibrateNo] = currentPoint;
                calibrateNo++;
                if (calibrateNo == 4)
                {
                    MessageBox.Show("Calibration complete!");
                    calibrateNo = -1;
                    isCalibrated = true;
                    isCalibrating = false;
                } else
                {
                    MessageBox.Show(calibrateNo + " points calibrated!");
                }
            }
        }
        private double getDist(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }
        private bool isStable(double x1, double y1, double x2, double y2)
        {
            //return getDist(x, y, lastX, lastY) >= Config.stableThreshold;    //0.05 may be a good Threshold
            return (Math.Abs(x1 - x2) <= Config.stableThreshold && Math.Abs(y1 - y2) <= Config.stableThreshold); // 0.02 is a good threshold
        }
        public Point parsePoint(Point pos)
        {
            if (!Config.useCamera)
            {
                return pos;
            }
            currentPoint = pos;
            if (!isCalibrated || isCalibrating)
            {
                return pos;
            }
            if (isCalibrated)
            {
                double x = (pos.X - calibratePoints[0].X) / (calibratePoints[1].X - calibratePoints[0].X);
                double y = (pos.Y - calibratePoints[2].Y) / (calibratePoints[3].Y - calibratePoints[2].Y);
                x = Math.Max(x, 0);
                x = Math.Min(x, 1);
                y = Math.Max(y, 0);
                y = Math.Min(y, 1);
                if ( (!Config.useCamera ||Double.IsNaN(lastX) || Double.IsNaN(lastY)) // no need to do stable
                     || !isStable(x, y, lastX, lastY) )
                {
                    lastX = x;
                    lastY = y;
                    return new Point(x, y);
                } else //Stable head position
                {
                    return new Point(lastX, lastY);
                }
                
            }
            throw new Exception("Calibration status error!");
            //return pos;
        }
    }
}
