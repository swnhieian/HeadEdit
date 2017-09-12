using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;


namespace HeadEdit
{
    class LogRecord
    {
        LogType type;
        DateTime time;
        string wrongWord;
        string targetWord;

        public LogRecord(LogType type ,DateTime time)
        {
            this.type = type;
            this.time = time;
        }

        public override string ToString()
        {
            string str;
            if (type == LogType.NextTask)//当进行下一个任务时，加入任务的相关信息
            {
                str = (type.ToString() + "," + time.ToString() + "," + wrongWord + "," + targetWord);
            }
            else
            {
                str = (type.ToString() + "," + time.ToString() );
            }
            return str;
        }
    }
}
