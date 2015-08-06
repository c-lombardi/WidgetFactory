using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FactoryApp;
using System.Diagnostics;

namespace ConsoleApplication2
{
    class Program
    {
        static void Main(string[] args) //https://social.msdn.microsoft.com/Forums/vstudio/en-US/f7d51fd0-c57a-478f-9d79-bc7cab34d03e/running-an-c-console-app-from-command-line-and-passing-args?forum=csharpgeneral
        {
            Factory f = new Factory();
            var SpecificationListLocation = args[0]; //read specification file path from the command line
            var OrderFileLocation = args[1]; //read order file path from command line
            try
            {
                var WidgetPartIdsSpeced = f.ParseSpecFile(SpecificationListLocation); //Set the Widgets and their parts that are Specified in the Specification List only if the parts exist
                var WidgetsOrdered = f.ParseOrderFile(OrderFileLocation, WidgetPartIdsSpeced);
                var WidgetsAssembled = f.OrderWidgetsFromParsedOrderFile(OrderFileLocation, WidgetsOrdered, WidgetPartIdsSpeced);
                f.ShowOrderedAndAssembledWidgets(WidgetsAssembled);
            }
            catch (NoWidgetsException ex)
            {
                Trace.WriteLine(String.Format(NoWidgetsException.message, ex.Message));
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
    }
}
