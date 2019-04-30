using System;
using System.Collections.Generic;
using System.Configuration;
using System.Xml;
using System.Text.RegularExpressions;
using System.IO;

namespace CFA_Salary_Labor
{
    class Calculator
    {
        //money earned / hours clocked in = Productivity Average
        //Productivity Average * Cost (Employee Wages per Hour) = Money Spent
        //Profit / Money Spent = Sales Percent

        // Four lists that hold the data used for calculations and for final presentation of the aforementioned data
        private static List<double> ProductivityAverage = new List<double>();
        private static List<double> SalesPercent = new List<double>();
        private static List<double> HoursClocked = new List<double>();
        private static List<double> ProfitEarned = new List<double>();

        // This is a debug function
        // To be removed
        public static void AddValues(double hoursClocked, double profitEarned)
        {
            HoursClocked.Add(hoursClocked);
            ProfitEarned.Add(profitEarned);
            Program.WriteToLog($"Values Added:\n\tHoursClocked.Add({hoursClocked})\n\tProfitEarned.Add({profitEarned})");
        }

        /// <summary>
        /// Calculates the productivity average of the most recent hour, dividing profit earned the most recent
        /// hour by the amount of hours clocked in and adding the quotient to the <c>private static List<double> ProductivityAverage</c>
        /// </summary>
        public static void CalculateLastHourAverage()
        {
            // Exits function if there are no values to calculate
            if (HoursClocked.Count == 0 || ProfitEarned.Count == 0)
            {
                // Prints debug information
                Program.WriteToLog("ERROR: Cannot Calculate Last Hour Productivity Average:\n\t" +
                     $"HoursClocked.Count = {HoursClocked.Count}\n\tProfitEarned.Count = {ProfitEarned.Count}");
                return;
            }

            // Finds necessary values
            double sale = ProfitEarned[ProfitEarned.Count - 1], hour = HoursClocked[HoursClocked.Count - 1];

            // Exits function if division by 0
            if (hour == 0)
            {
                Program.WriteToLog($"WARNING: Divide by Zero:\n\thour == 0: {hour == 0}");
                return;
            }

            ProductivityAverage.Add(sale / hour); // Calculates Productivity Average

            // Logs debug information
            Program.WriteToLog("Labor Advantage Added: " + ProductivityAverage[ProductivityAverage.Count - 1] +
                $"{sale} / {hour}");
        }
        
        /// <summary>
        /// Calculates the sales percent of the most recent hour, dividing profit earned the most recent
        /// hour by the amount of money spent on wages and adding the quotient to the
        /// <c>private static List<double> SalesPercent</c>
        /// </summary>
        public static void CalculateLastHourPercent()
        {
            // Exits function if there is no values to work with
            if (HoursClocked.Count == 0 || ProfitEarned.Count == 0 || ProductivityAverage.Count == 0)
            {
                // Logs debug information
                Program.WriteToLog("ERROR: Cannot Calculate Last Hour Sales Percent:\n\t" +
                    $"HoursClocked.Count = {HoursClocked.Count} -- ProfitEarned.Count = {ProfitEarned.Count}\n\t" +
                    $"ProductivityAverage.Count = {ProductivityAverage.Count}");
                return;
            }
            
            // Exits function if the necessary xml documents have not been loaded
            if(Employee.GetEmployeeWageList() == null || Employee.GetEmployeeScheduleList() == null)
            {
                // Logs debug information
                Program.WriteToLog("ERROR: Cannot Calculate Last Hour Sales Percent:\n\t" +
                     $"WageNodeList == null: {Employee.GetEmployeeWageList() == null}\n\tScheduleNodeList == null: {Employee.GetEmployeeScheduleList() == null}");
                return;
            }
            
            // Some variables to perform calculations with
            double cost = 0, profit = ProfitEarned[ProfitEarned.Count - 1];

            // Iterates through every XmlNode in WageNodeList
            foreach (XmlNode node in Employee.GetEmployeeWageList())
            {
                // Id of employee of the current node
                int id = Employee.IdOfNode(node);

                // Percent of hour worked this hour by this employee
                double hourWorked = Employee.FindEmployeeClockedIn(id);

                // Prints debug information
                Program.WriteToLog($"{node.InnerText}\nhourWorked = {hourWorked}");

                if (hourWorked > 0) // If the employee works this hour
                {
                    double c = hourWorked * Employee.FindWageOfEmployee(id); // Calculates the cost of this employee working for one hour
                    cost += c; // Adds the cost of this employee to the total cost

                    // Logs debug information
                    Program.WriteToLog(Employee.FindNameOfEmployee(id) + " costed $" + c.ToString("#.##") + " for " + hourWorked.ToString("#.##") + " hours worked");
                }
                else if (hourWorked == -1) // If the employee is not found in the system
                {
                    // Logs debug information
                    Program.WriteToLog($"WARNING: Employee {id} is not in the system");
                    continue; // Skips to the next employee node
                }
                else // If the employee does not work this hour
                {
                    // Logs debug information
                    Program.WriteToLog($"WARNING: Employee {Employee.FindNameOfEmployee(id)} does not work this hour");
                    continue; // Skips to the next employee node
                }
            }

            // If there was no cost this hour
            if (cost == 0)
            {
                // Prints debug information
                Program.WriteToLog($"WARNING: Divide by Zero:\n\tcost == 0: {cost == 0}");
                SalesPercent.Add(-1); // Adds -1 as this hour's Sales Percent
            }
            else
            {
                double salePercent = profit / cost; // Calculates this hour's Sales Percent
                SalesPercent.Add(salePercent);      // Adds the value to the list

                // Logs debug information
                Program.WriteToLog($"Sale %:\n    {salePercent.ToString("#.##")}\n\t{profit}/{cost}");
            }
        }
    }
}
