using System;
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
        public static bool debug = true;
        public static bool useCamera = false;   // false means use mouse move
        public static Key calibrationKey = Key.F12;
        public static Key moveKey = Key.F1;
        public static Key editKey = Key.F2;
        public static Key cancelKey = Key.Escape;
        public static Key enterSelect = Key.Enter;
        public static Key selectNext = Key.Tab;
        public static Brush HeadSelectColor = Brushes.DarkGreen;
        public static Brush FontColor = Brushes.DarkCyan;
        public static Brush BackGroundColor = Brushes.Blue;
        public static Brush DefaultFontColor = Brushes.Black;
        public static Brush DefaultBackGroundColor = Brushes.White;
        public static double stableThreshold = 0.03;
        public static double richTextBoxFontSize = 30;
    }
}
