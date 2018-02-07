﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CsvHelper;
using System.Threading;
using System.Diagnostics;

namespace ParseSCCMLogs
{
    class Program
    {
        static string Hostname;
        static void Main(string[] args)
        {
            Stopwatch stop = new Stopwatch();
            stop.Start();

            string LogPath = "C:\\Windows\\CCM\\Logs\\";

            if (args.Length == 0)
            {
                // No Argument Provided
                System.Console.WriteLine("No Hostname provided, working on C:\\Windows\\CCM\\Logs");
                Hostname = null;
            }
            else if (args.Length > 2)
            {
                // Too many arguments
                System.Console.WriteLine("Too many arguments provided");
                return;
            }
            else
            {
                System.Console.WriteLine("Remote Hostname provided, checking if machine is online...");

                Hostname = args[0];
                if (PingHostname(Hostname)!= true)
                {
                    Console.WriteLine("Unable to contact hostname : {0}", Hostname);
                    return;
                }
                LogPath = "\\\\" + Hostname + "\\c$\\Windows\\CCM\\Logs\\";
                
            }

            Console.WriteLine("Log Path = {0}", LogPath);

            // Check Path Exists
            if(Directory.Exists(LogPath))
            {
                Console.WriteLine("Found path : {0}", LogPath);
            } else
            {
                Console.WriteLine("Unable to find path, SCCM client may not be installed : {0}", LogPath);
                return;
            }

            // Get Log File Names
            List<string> logfiles = Directory.EnumerateFiles(LogPath).ToList();
            // Remove log file names beginning with \SC
            logfiles.RemoveAll(u => u.Contains("\\SC"));
            // Remove log file names beginning with \_SC
            logfiles.RemoveAll(u => u.Contains("\\_SC"));
            // Remove SMSTS logs for now
            //logfiles.RemoveAll(u => u.Contains("SMSTS"));
            // Remove ZTI logs for now
            logfiles.RemoveAll(u => u.Contains("zti"));
            logfiles.RemoveAll(u => u.Contains("ZTI"));
            logfiles.RemoveAll(u => u.Contains("BDD"));
            logfiles.RemoveAll(u => u.Contains("wedmtrace.log"));

            //// TESTING /// 
            // Lets work on a few log files for initial test...
            //logfiles.RemoveAll(u => !u.Contains("\\App"));

            string OutputPath = Path.GetTempPath();
            Directory.CreateDirectory(OutputPath + "SCCMLogs");

            /*
            foreach (string log in logfiles)
            {
                Console.WriteLine("Working on file {0} in Thread x", log);
                List<LogLine> loglines = ParseLogFile(log);
           
                if (loglines.Count > 0)
                {
                    string[] filenamesplit = log.Split('\\');
                    string filename = filenamesplit[filenamesplit.Length - 1];
                    filename = filename.Replace(".log", ".csv");
                    string outfilename = OutputPath + "SCCMLogs\\" + filename;

                    StreamWriter sw = new StreamWriter(outfilename);

                    CsvWriter csv = new CsvWriter(sw);
                    csv.WriteRecords(loglines);
                    sw.Close();
                }
            }
            */

            Parallel.ForEach(logfiles, (log) =>
            {
                Console.WriteLine("Working on file {0} in Thread {1}", log, Thread.CurrentThread.ManagedThreadId);
                List<LogLine> loglines = ParseLogFile(log);

                if (loglines.Count > 0)
                {
                    string[] filenamesplit = log.Split('\\');
                    string filename = filenamesplit[filenamesplit.Length - 1];
                    filename = filename.Replace(".log", ".csv");
                    string outfilename = OutputPath + "SCCMLogs\\" + filename;

                    StreamWriter sw = new StreamWriter(outfilename);

                    CsvWriter csv = new CsvWriter(sw);
                    csv.WriteRecords(loglines);
                    sw.Close();
                }
            });

            stop.Stop();
            System.Console.WriteLine("Finished in {0}", stop.Elapsed);
#if DEBUG
            System.Console.ReadLine();
#endif
        }


        static List<LogLine> ParseLogFile(string path)
        {
            // Fastest way to read files : http://cc.davelozinski.com/c-sharp/the-fastest-way-to-read-and-process-text-files

            List<LogLine> loglinelist = new List<LogLine>();

            // Read File into String Array
            try
            {
                //string[] AllLines = File.ReadAllLines(path);
                FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader sr = new StreamReader(fs);
                List<string> AllLines = new List<string>();
                string aline;
                while((aline = sr.ReadLine()) != null)
                {
                    AllLines.Add(aline);
                }


                string buffer = "";
                foreach (string line in AllLines)
                {
                    // Buffer string is used to hangle multiline entries

                    Object[] objarr = ParseLogLine(line, buffer);
                    // If the line is part of a multiline entry the ParseLogLine function will return null until the whole multiline has been handled
                    if (objarr[0] != null)
                    {
                        loglinelist.Add((LogLine)objarr[0]);
                        buffer = "";
                    }
                    else
                    {
                        buffer = (string)objarr[1];
                    }
                }
            } catch (Exception e)
            {
                Console.WriteLine("Problem accessing the file : {0}", path);
            }
            

            /*
            Parallel.For(0, AllLines.Length, x =>
            {
                
            });
            */
            
            return loglinelist;
        }

        static MatchCollection ParseSCCMLogLine(string line)
        {
            Regex r = new Regex(@"<!\[LOG\[(.*?)\]LOG\]!><time=""(.*?)""\s*date=""(.*?)""\s*component=""(.*?)""\s*context=""""\s*type=""([\d])""\s*thread=""([\d]+)""\s*file=""(.*?)"">");
            MatchCollection matches = r.Matches(line);
            return matches;
        }
        
        static Object[] ParseLogLine(string logline, string buffer)
        {
            string dateformat = "M-d-yyyy HH:mm:ss.fff";
            // Regex test 1
            // Grok / Logstash Builtin Patterns https://github.com/logstash-plugins/logstash-patterns-core/blob/master/patterns/grok-patterns
            // Debuggex Tester - https://www.debuggex.com/
            // Best Regex Tester - https://regexr.com/
            // Pattern Test - <!\[LOG\[(.*?)\]LOG\]!><time="(.*?)"\s*date="(.*?)"\s*component="(.*?)"\s*context=""\s*type="([1-3])"\s*thread="([\d]+)"\s*file="(.*?)">

            // Is the line a oneline log entry?
            MatchCollection matches = ParseSCCMLogLine(logline);

            if (matches.Count < 1)
            {
                // Test if this is the last line
                Regex r1 = new Regex(@".*?file="".*");
                MatchCollection matches1 = r1.Matches(logline);
                if (matches1.Count > 0)
                {
                    // This is the last line ass to buffer then reparse as one line
                    buffer = buffer + logline;
                    matches = ParseSCCMLogLine(buffer);

                } else
                {
                    buffer = buffer + logline;
                    Object[] arr = new Object[] { null, buffer };
                    return arr;
                }
                // No match add line to buffer
                
            }
            
            string text = matches[0].Groups[1].Value;
            string timestring = matches[0].Groups[2].Value;
            timestring = timestring.Split('+')[0];
            timestring = timestring.Split('-')[0];
            string timems = timestring.Split('.')[1];
            timems = timems.Substring(0, 3);
            timestring = timestring.Split('.')[0] + "." + timems;
            string datetimestring = matches[0].Groups[3].Value + " " + timestring;

            DateTime time = DateTime.ParseExact(datetimestring, dateformat, System.Globalization.CultureInfo.InvariantCulture);
            string component = matches[0].Groups[4].Value;
            string type = matches[0].Groups[5].Value;
            string thread = matches[0].Groups[6].Value;
            string file = matches[0].Groups[7].Value;

            LogLine l = new LogLine(component,
                                    time,
                                    Int32.Parse(thread),
                                    text,
                                    file,
                                    Int16.Parse(type));
            Object[] retarr = new Object[] { l, buffer };
            return retarr;

        }

        static bool PingHostname(string hostname)
        {
            try
            {
                Ping ping = new Ping();
                PingReply reply = ping.Send(hostname, 1000); 
                if (reply.Status == IPStatus.Success)
                {
                    Console.WriteLine("{0} pinged back in {1} milliseconds", new[] { hostname, reply.RoundtripTime.ToString() });
                    return true;
                }
                else
                {
                    return false;
                }
            } catch (Exception e)
            {
                Console.WriteLine("Problem pinging hostname : ");
                Console.WriteLine(e.Message);
                return false;
            }
        }
    }
}
