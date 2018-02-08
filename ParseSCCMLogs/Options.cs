﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace ParseSCCMLogs
{
    class Options
    {
        [Option('h', "hostname", Required = false, HelpText = "Remote Hostname to get log files from")]
        public string Hostname { get; set; }

        [Option('o',"output",Required = true, HelpText = "Output Type - csv,csv1 or sql")]
        public string OutputType { get; set; }

        [Option('d',"directory", Required = false, HelpText = "If OutputType = csv or csv1 specify the directory")]
        public string DestinationDir { get; set; }
    }
}
