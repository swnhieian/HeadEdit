using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeadEdit
{
    public enum LogType { NextTask, TaskStart, MouseDown, Type, Delete, Enter, Select,  F1, Shift, Esc };
    public class Log
    {
        private MainWindow window;
        private List<LogRecord> loglist;
        private StreamWriter sw;
        private DateTime startTime;
        private string logDir;
        public Log(MainWindow mainWindow ,DateTime time )
        {
            this.window = mainWindow;
            this.startTime = time;

            logDir = Directory.GetCurrentDirectory() + "\\logs\\" ;
        }

        public void addLog(LogType type, DateTime time)
        {
            loglist.Add(new LogRecord(type, time));
        }

        public void addLog(LogType type, DateTime time, string wrongWord, string targetWord)
        {
            loglist.Add(new LogRecord(type, time, wrongWord, targetWord));
        }

        public void saveLog()
        {
            if (loglist == null) return;
            string fileName = startTime.ToString("_MM_dd_HH_mm_ss")+ ".txt";

            sw = new StreamWriter(logDir + "\\" + fileName, true);
            foreach (LogRecord log in loglist)
            {
                    sw.WriteLine(log.ToString());
            }
            sw.Close();
            loglist.Clear();
        }

    }
}
