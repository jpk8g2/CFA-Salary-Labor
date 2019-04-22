using System;
using System.Xml;
using System.Configuration;
using System.Text.RegularExpressions;

namespace CFA_Salary_Labor
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(Calculator.FindEmployeeClockedIn("Anthony Panarello"));
        }
    }
}
