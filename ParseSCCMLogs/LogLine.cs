﻿using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParseSCCMLogs
{
    public class LogLine
    {
        public string Component { get; set; }
        public DateTime dateTime { get; set; }
        public int Thread { get; set; }
        public string Text { get; set; }
        public string Filename { get; set; }
        public short Type { get; set; }

        public LogLine()
        {
        }

        public LogLine(string component, DateTime dateTime, int thread, string text, string filename, short type)
        {
            Component = component;
            this.dateTime = dateTime;
            Thread = thread;
            Text = text;
            Filename = filename;
            Type = type;
        }
    }

    public sealed class LogLineMap : ClassMap<LogLine>
    {
        public LogLineMap()
        {
            AutoMap();
            // Default CSV DateTime to String conversion was ignoring the millisecond component. This fixes that.
            Map(m => m.dateTime).ConvertUsing(m =>
            {
                return m.dateTime.ToString("dd/MM/yyyy hh:mm:ss.fff");
            });
        }
    }
}
