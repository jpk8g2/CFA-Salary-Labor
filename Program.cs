using System;
using System.Xml;
using System.Configuration;
using System.Text.RegularExpressions;
using System.IO;

namespace CFA_Salary_Labor
{
    class Program
    {
        private static readonly StreamWriter logWrite = File.AppendText(ConfigurationManager.AppSettings["LogPath"]);
        private static bool hasDate = false;
        public static void Main(string[] args)
        {
            Employee.Start(); // DO NOT TOUCH
                                // ADD YOUR OWN CODE UNDERNEATH

            Calculator.CalculateLastHourAverage();            
        }

        public static void WriteToLog(string text)
        {
            if (!hasDate)
            {
                string date = DateTime.Now.Date.ToString().Split(' ')[0] + " " + DateTime.Now.TimeOfDay.ToString();
                logWrite.WriteLine($"[{date}]");
                logWrite.Flush();
                hasDate = true;
            }
            char[] message = text.ToCharArray();
            for (int x = 0; x < message.Length; x++)
            {
                if (message[x] == '\n')
                    logWrite.WriteLine();
                else if (message[x] == '\t')
                    logWrite.Write("        ");
                else
                    logWrite.Write(message[x]);
                logWrite.Flush();
            }
            logWrite.WriteLine("\n\n");
            logWrite.Flush();
        }
    }
}
