using System;
using System.Collections.Generic;
using System.Configuration;
using System.Xml;
using System.Text.RegularExpressions;

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
        // A string constant that reads from the App.config file located in this project's root folder
        private static readonly string EmployeeWage = ConfigurationManager.AppSettings["EmployeeWageXml"];
        // A string constant that reads from the App.config file located in this project's root folder
        private static readonly string EmployeeSchedule = ConfigurationManager.AppSettings["EmployeeScheduleXml"];
        // A XmlDocument used through multiple functions of the Calculator class
        private static readonly XmlDocument Document = new XmlDocument();
        // A variable whose purpose is to be returned upon an error within a function
        private static readonly int ERROR_RETURN = -1;
        // Controls if debug information will be printed to the console
        private static readonly bool DEBUG = false; 

        /// <summary>
        /// Calculates the productivity average of the most recent hour, dividing profit earned the most recent
        /// hour by the amount of hours clocked in and adding the quotient to the <c>private static List<double> ProductivityAverage</c>
        /// </summary>
        private static void CalculateLastHourAverage()
        {
            if (HoursClocked.Count == 0 && ProfitEarned.Count == 0) return;
            double sale = ProfitEarned[ProfitEarned.Count - 1], hour = HoursClocked[HoursClocked.Count - 1];
            ProductivityAverage.Add(sale / hour);
            if (DEBUG) Console.WriteLine("Labor Advantage Added: " + ProductivityAverage[ProductivityAverage.Count - 1]);
        }
        
        /// <summary>
        /// Calculates the sales percent of the most recent hour, dividing profit earned the most recent
        /// hour by the amount of money spent on wages and adding the quotient to the
        /// <c>private static List<double> SalesPercent</c>
        /// </summary>
        private static void CalculateLastHourPercent()
        {
            if (HoursClocked.Count == 0 && ProfitEarned.Count == 0 && ProductivityAverage.Count == 0) return;
            double pAve = ProductivityAverage[ProductivityAverage.Count - 1];
        }

        /// <summary>
        /// Finds the wage of an employee based on their name
        /// Looks through the file located at the path within the <c>private static readonly string EmployeeWage</c> constant
        /// </summary>
        /// <param name="nameOfEmployee">The string of the employee to find the wage of</param>
        /// <returns>The wage of the employee as a double. If an employee is not found, returns -1</returns>
        /// See <see cref="FindWageOfEmployee(int)"/> to find the wage of an employee based on an employee's id
        public static double FindWageOfEmployee(string nameOfEmployee)
        {
            Document.Load(EmployeeWage); // Loads the Xml Document containing all employee wages
            XmlNodeList list = Document.ChildNodes[1].ChildNodes; // Finds and seperates all employees into seperate nodes holding names, wages, and ids
            foreach (XmlNode node in list) // Iterates through the list of nodes
            {
                if (NameOfNode(node) == nameOfEmployee) // If the name of the employee in the current node matches the name of the employee in question
                {
                    if (DEBUG) Console.WriteLine($"{nameOfEmployee}'s Wage Found: {WageOfNode(node)}; Returned"); // Prints debug information to the console if allowed
                    return WageOfNode(node); // Returns the wage of the found employee within the XmlNode
                }
            }
            return ERROR_RETURN; // Returns -1 in the case the name of the employee was not found within the system
        }

        /// <summary>
        /// Finds the wage of an employee based on their id
        /// Looks through the file located at the path within the <c>private static readonly string EmployeeWage</c> constant 
        /// </summary>
        /// <param name="id">The id of the employee to find the wage of</param>
        /// <returns>The wage of the employee as a double. If an employee is not found, returns -1</returns>
        /// See <see cref="FindWageOfEmployee(string)"/> to find the wage of an employee based on an employee's name
        public static double FindWageOfEmployee(int id)
        {
            Document.Load(EmployeeWage); // Loads the Xml Document containing all employee wages
            XmlNodeList list = Document.ChildNodes[1].ChildNodes; // Finds and seperates all employees into seperate nodes holding names, wages, and ids

            foreach (XmlNode node in list) // Iterates through the list of nodes
            {
                if (IdOfNode(node) == id) // If the id of the employee in the current node matches the id of the employee in question
                {
                    if (DEBUG) Console.WriteLine($"{NameOfNode(node)}'s Wage Found: {WageOfNode(node)}; Returned"); // Prints debug information to the console if allowed
                    return WageOfNode(node); // Returns the wage of the found employee within the XmlNode
                }
            }
            return ERROR_RETURN; // Returns -1 in the case the id of the employee was not found within the system
        }

        /// <summary>
        /// Finds how long a certain employee is working for this hour
        /// </summary>
        /// <param name="nameOfEmployee">The name of the employee to find</param>
        /// <returns>A double from 0.0 to 1.0 that represents the percent (as a decimal) of an
        /// hour the employee will be clocked in</returns>
        /// See <see cref="FindEmployeeClockedIn(int)"/> to find how long a certain employee works this hour by their id
        public static double FindEmployeeClockedIn(string nameOfEmployee)
        {
            Document.Load(EmployeeSchedule); // Loads the Xml Document containing all employee schedule's
            XmlNodeList list = Document.ChildNodes[1].ChildNodes; // Finds and seperates all employees and their schedules
            int date = DateTime.Now.Hour; // The hour to search for

            foreach (XmlNode node in list) // Iterates through every employee in the schedule
            {
                if (NameOfNode(node) == nameOfEmployee) // If the name of the employee is found
                {
                    string hour = (date > 12 ? date - 12 : date) + (date >= 12 ? "pm" : "am"); // Formats the hour
                    return TurnNodeIntoTime(node, hour); // Returns the percent of the hour working
                }
            }
            return ERROR_RETURN; // Returns -1 if the id is not found in the system
        }
        /// <summary>
        /// Finds how long a certain employee is working for this hour
        /// </summary>
        /// <param name="id">The idof the employee to find</param>
        /// <returns>A double from 0.0 to 1.0 that represents the percent (as a decimal) of an
        /// hour the employee will be clocked in</returns>
        /// See <see cref="FindEmployeeClockedIn(string)"/> to find how long a certain employee works this hour by their name
        public static double FindEmployeeClockedIn(int id)
        {
            Document.Load(EmployeeSchedule); // Loads the Xml Document containing all employee schedule's
            XmlNodeList list = Document.ChildNodes[1].ChildNodes; // Finds and seperates all employees and their schedules
            int date = DateTime.Now.Hour; // The hour to search for

            foreach (XmlNode node in list) // Iterates through every employee in the schedule
            {
                if (IdOfNode(node) == id) // If the id of the employee is found
                {
                    string hour = (date > 12 ? date - 12 : date) + (date >= 12 ? "pm" : "am"); // Formats the hour
                    return TurnNodeIntoTime(node, hour); // Returns the percent of the hour working
                }
            }
            return ERROR_RETURN; // Returns -1 if the id is not found in the system
        }

        /// <summary>
        /// Parses the parameter <paramref name="node"/> for the name of the employee in the node 
        /// </summary>
        /// <param name="node">The XmlNode to parse</param>
        /// <returns>A string containing the name of the employee found within the node</returns>
        /// See <see cref="WageOfNode(XmlNode)"/> to find the wage of an employee within a node
        /// See <see cref="IdOfNode(XmlNode)"/> to find the id of an employee within a node
        private static string NameOfNode(XmlNode node) => node.InnerText.Split('\n')[1].Trim();

        /// <summary>
        /// Parses the parameter <paramref name="node"/> for the wage of the employee in the node
        /// </summary>
        /// <param name="node">The XmlNode to parse</param>
        /// <returns>A double containing the wage of the employee found within the node</returns>
        /// See <see cref="NameOfNode(XmlNode)"/> to find the name of an employee within a node
        /// See <see cref="IdOfNode(XmlNode)"/> to find the id of an employee within a node
        private static double WageOfNode(XmlNode node)
        {
            string wage = node.InnerText.Split('\n')[2]; // Parses the text of the node to find the correct substring
            if (double.TryParse(wage, out double b)) // If the parsed string was successfully turned into a double
                return b; // Return the parsed string as a double
            else
                return ERROR_RETURN; // Return -1 if the parsed string was unable to be turned into a double
        }

        /// <summary>
        /// Parses the parameter <paramref name="node"/> for the id of the employee in the node
        /// </summary>
        /// <param name="node">The XmlNode to parse</param>
        /// <returns>An int containing the id of the employee found within the node</returns>
        /// See <see cref="NameOfNode(XmlNode)"/> to find the name of the employee within a node
        /// See <see cref="WageOfNode(XmlNode)"/> to find the wage of the employee within a node
        private static int IdOfNode(XmlNode node)
        {
            string id = node.InnerText.Split('\n')[3]; // Parses the text of the node to find the correct substring
            if (int.TryParse(id, out int i)) // If the parsed string was successfully turned into an int
                return i; // Return the parsed string as an int
            else
                return ERROR_RETURN; // Return -1 if the parsed string was unable to be turned into an int
        }
        /// <summary>
        /// Finds the amount of time the employee of <paramref name="node" /> works in the hour <paramref name="time"/>
        /// </summary>
        /// <param name="node">The node to parse</param>
        /// <param name="time">
        /// <para>The hour to find. Much be in the format "hour(am/pm)".</para>
        /// <para>For example: "4pm" or "4:00pm"</para>
        /// </param>
        /// <returns>A double representing how much of the hour the employee works</returns>
        /// <example>
        /// If an employee of <paramref name="node"/> works from 4:15pm-9:00pm and this function is
        /// called with <c>TurnNodeIntoTime(node, "4:00pm");</c>, the function will return how long of the hour
        /// 4:00pm (or 16:00 or 1600) the employee works. Here, it is 0.75 of the hour (45 minutes).
        /// </example>
        private static double TurnNodeIntoTime(XmlNode node, string time)
        {
            Regex removeNonDigits = new Regex("\\D");
            string[] nodeTime = node.InnerText.Split('\n')[3].Split('-'); // Splits the schedule of the employee found in the constant EmployeeSchedule
                                                                          // For example, if the schedule found is "4:00pm-11:00pm", the indexes of this
                                                                          // array will be "4:00pm" and "11:00pm"

            string[] paramTime = removeNonDigits.Split(time); // Splits the string parameter time by all the non-digits within the string
                                                              // If the parameter is "4:00pm", the first index of the newly-splitted string
                                                              // will be "4", and this is what will be used (so there is no need for the :00, as "4pm"
                                                              // works just as fine as "4:00pm")

            if(DEBUG) // Prints debug information of found node
            {
                Console.WriteLine("Found schedule of: " + NameOfNode(node));
                foreach (string s in nodeTime)
                {
                    if (s == "" || s == "\n") continue; // Skips empty white space
                    Console.WriteLine($"    {s.Trim()}"); // Prints indented strings, ommiting trailing and leading white space
                }
            }

            // Splits the time of the schedule found within the node from the hours and minutes of both the starting and ending time
            string alterStartTime = removeNonDigits.Split(nodeTime[0].Trim())[0], alterEndTime = removeNonDigits.Split(nodeTime[1].Trim())[0];
            string alterStartMins = removeNonDigits.Split(nodeTime[0].Trim())[1], alterEndMins = removeNonDigits.Split(nodeTime[1].Trim())[1];
            
            // Turns the newly splitted strings of hours and minutes into integers
            int.TryParse(alterStartTime, out int startShift);
            int.TryParse(alterEndTime, out int endShift);
            int.TryParse(alterStartMins, out int startShiftMins);
            int.TryParse(alterEndMins, out int endShiftMins);
            int.TryParse(paramTime[0], out int paramTimeHour);

            if (DEBUG) Console.WriteLine($"startShift: {startShift}\nendShift: {endShift}\nstartShiftMins: {startShiftMins}\n" +
                 $"endShiftMins: {endShiftMins}\nparamTimeHour: {paramTimeHour}\n\n"); // Prints debug information to the console

            // Transforms times from "4:00pm" to "16:00"
            if (nodeTime[0].Substring(nodeTime[0].Length - 2) == "pm")
                startShift += 12;
            if (nodeTime[1].Substring(nodeTime[1].Length - 3).Trim() == "pm")
                endShift += 12;
            if (time.Substring(time.Length - 2) == "pm")
                paramTimeHour += 12;

            if(DEBUG) Console.WriteLine($"After Time Shift:\nstartShift: {startShift}\nendShift: {endShift}\nstartShiftMins: {startShiftMins}\n" +
                 $"endShiftMins: {endShiftMins}\nparamTimeHour: {paramTimeHour}\n"); // Prints debug information to the console

            if (!(startShift <= paramTimeHour && paramTimeHour <= endShift)) // If the time parameter is not within the schedule of the employee
            {
                if (DEBUG) Console.WriteLine($"startShift <= paramTimeHour = {startShift < paramTimeHour}\n" +
                     $"paramTimeHour <= endShift = {paramTimeHour < endShift}"); // Prints debug information to the console
                return 0; // Return no time worked this hour
            }

            if (startShift == paramTimeHour) return (60 - startShiftMins) / 60.0; // If the parameter time matches the starting time,
                                                                                  // returns a double of how many minutes worked that hour
                                                                                  // divided by one hour

            if (endShift == paramTimeHour) return endShiftMins / 60.0;            // If the parameter time matches the ending time,
                                                                                  // returns a double of how many minutes worked that hour
                                                                                  // divided by one hour

            return 1; // If the parameter time is not outside the schedule of the employee, or at the start or the end of
                      // the schedule, the parameter time must be within the schedule and thus the employee must have worked one hour
        }
    }
}
