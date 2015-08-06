using System;
using System.Diagnostics;

namespace ConsoleApplication2
{
    class Program
    {
        static void Main(string[] args) //https://social.msdn.microsoft.com/Forums/vstudio/en-US/f7d51fd0-c57a-478f-9d79-bc7cab34d03e/running-an-c-console-app-from-command-line-and-passing-args?forum=csharpgeneral
        {
            var f = new Factory();
            var specificationListLocation = args[0]; //read specification file path from the command line
            var orderFileLocation = args[1]; //read order file path from command line
            try
            {
                var widgetPartIdsSpeced = f.ParseSpecFile(specificationListLocation); //Set the Widgets and their parts that are Specified in the Specification List only if the parts exist
                var widgetsOrdered = f.ParseOrderFile(orderFileLocation, widgetPartIdsSpeced);
                var widgetsAssembled = f.OrderWidgetsFromParsedOrderFile(orderFileLocation, widgetsOrdered, widgetPartIdsSpeced);
                f.ShowOrderedAndAssembledWidgets(widgetsAssembled);
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
