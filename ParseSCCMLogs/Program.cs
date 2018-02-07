using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace ParseSCCMLogs
{
    class Program
    {
        static string Hostname;
        static void Main(string[] args)
        {

            string LogPath = "C:\\Windows\\CCM\\Logs\\";

            if (args.Length == 0)
            {
                // No Argument Provided
                System.Console.WriteLine("No Hostname provided, working on C:\\Windows\\CCM\\Logs");
                Hostname = null;
            }
            else if (args.Length > 1)
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

            //// TESTING /// 
            // Lets work on a few log files for initial test...
            logfiles.RemoveAll(u => !u.Contains("\\App"));
            
#if DEBUG
            System.Console.ReadLine();
#endif
        }


        static List<LogLine> ParseLogFile(string path)
        {
            // Fastest way to read files : http://cc.davelozinski.com/c-sharp/the-fastest-way-to-read-and-process-text-files

            List<LogLine> loglinelist = new List<LogLine>();

            // Read File into String Array
            string[] AllLines = File.ReadAllLines(path);
            Parallel.For(0, AllLines.Length, x =>
            {
                
            });

            return loglinelist;
        }
        
        static LogLine ParseLogLine(string logline)
        {

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
