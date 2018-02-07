using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParseSCCMLogs
{
    class LogLine
    {
        public string Component { get; set; }
        public string File { get; set; }
        public DateTime dateTime { get; set; }
        public int Thread { get; set; }
        public string Filename { get; set; }
        public string Text { get; set; }

        public LogLine(string component, string file, DateTime dateTime, int thread, string filename, string text)
        {
            Component = component;
            File = file;
            this.dateTime = dateTime;
            Thread = thread;
            Filename = filename;
            Text = text;
        }
    }
}
