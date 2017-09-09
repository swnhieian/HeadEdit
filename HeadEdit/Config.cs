using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HeadEdit
{
    class Config
    {
        public static bool debug = true;
        public static bool useCamera = true;   // false means use mouse move
        public static Key calibrationKey = Key.F12;
        public static Key moveKey = Key.F1;
        public static Key editKey = Key.F2;
        public static Key cancelKey = Key.Escape;
        public static Key enterSelect = Key.Enter;
        public static Key selectNext = Key.Tab;
        public static ConsoleColor HeadSelectColor = ConsoleColor.DarkGreen;;
        public static ConsoleColor FontColor = ConsoleColor.DarkCyan;
        public static ConsoleColor BackGroundColor = ConsoleColor.Blue;
        public static ConsoleColor DefaultFontColor = ConsoleColor.Black;
        public static ConsoleColor DefaultBackGroundColor = ConsoleColor.White;
        public static double stableThreshold = 0.02;
        public static double richTextBoxFontSize = 30;
    }
}
