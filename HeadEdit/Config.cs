﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace HeadEdit
{
    class Config
    {
        public static bool debug = false;
        public static bool useCamera = false;   // false means use mouse move
        public static Key calibrationKey = Key.F12;
        public static Key moveKey = Key.F1;
        public static Key editKey = Key.LeftShift;
        public static Key cancelKey = Key.Escape;
        public static Key enterSelect = Key.Enter;
        public static Key selectNext = Key.Tab;
        public static Key changeMode = Key.F4;
        public static Key NextTask = Key.F11;
        public static Key TaskStart = Key.Space;
        public static Brush FontColor = Brushes.Red;
        public static Brush BackGroundColor = new SolidColorBrush(Color.FromArgb(205, 0, 0, 128));
        public static Brush DefaultFontColor = Brushes.Black;
        public static Brush DefaultBackGroundColor = Brushes.White;
        public static Brush WrongWordTipColor = Brushes.Yellow;
        public static double stableThreshold = 0.05;
        public static double richTextBoxFontSize = 30;
        public static double fontWidth = 1807.93774319066 / 99.0;
        public static FontFamily fontFamily = new FontFamily("Courier New");
        public static int headMoveMode = 0; //0 means absolute, 1 means relative
    }
}