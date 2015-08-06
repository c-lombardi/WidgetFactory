using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WidgetFactory;

namespace FactoryApp
{
    public class Factory : IFactory
    {
        const string WidgetNameDelimiter = ":";
        const string WidgetPartDelimiter = ",";

        private Func<List<String>, List<String>> RemoveSpacesFromBeginningAndEnd = x =>
        {
            var stringsWithoutSpacesAtBeginningOrEnd = new List<String>();
            foreach (var str in x)
            {
                stringsWithoutSpacesAtBeginningOrEnd.Add(str.Trim());//http://stackoverflow.com/questions/3381952/how-automatically-remove-all-white-spaces-start-or-end-in-a-string  
            }
            return stringsWithoutSpacesAtBeginningOrEnd;
        };

        public List<SpecedWidget> ParseSpecFile(string pathToFile) //http://stackoverflow.com/questions/8037070/whats-the-fastest-way-to-read-a-text-file-line-by-line
        {
            var fileLines = File.ReadLines(pathToFile);
            var wh = new Warehouse();
            var lineCount = 0;
            var widgetsSpeced = new List<SpecedWidget>();
            foreach (var line in fileLines)
            {
                lineCount++;
                try
                {
                    widgetsSpeced.Add(ReadSpecFileLines(line, lineCount));
                }
                // handle exceptions to continue through the loop and not ignore the rest of the file to try and meet client demands as best as possible
                catch (LineEmptyException ex)
                {
                    Trace.WriteLine(String.Format(LineEmptyException.message, ex.Message));
                }
                catch (NoWidgetNameException ex)
                {
                    Trace.WriteLine(String.Format(NoWidgetNameException.message, ex.Message));
                }
                catch (NoPartsException ex)
                {
                    Trace.WriteLine(String.Format(NoPartsException.message, ex.Message));
                }
                catch (MalformedLineException ex)
                {
                    Trace.WriteLine(String.Format(MalformedLineException.message, ex.Message));
                }
            }
            if (lineCount.Equals(0))
            {
                throw new NoWidgetsException(pathToFile);
            }
            return widgetsSpeced;
        }

        //Parse the file
        public SpecedWidget ReadSpecFileLines(String line, int lineCount)
        {

            //Empty Line check
            if (line.Length.Equals(0))
                throw new LineEmptyException(lineCount.ToString());
            //No Widget Name check
            var firstIndexOfColon = line.IndexOf(WidgetNameDelimiter);
            if (firstIndexOfColon == -1)
                throw new MalformedLineException(lineCount.ToString());
            var widgetName = line.Substring(0, firstIndexOfColon);
            if (widgetName.Length.Equals(0))
                throw new NoWidgetNameException(lineCount.ToString());
            //No Parts Check
            var partIdsCommaDelimitedList = line.Substring((firstIndexOfColon + 1), (line.Length - firstIndexOfColon - 1));
            var commaCount = Regex.Matches(partIdsCommaDelimitedList, WidgetPartDelimiter).Count; //http://stackoverflow.com/questions/3016522/count-the-number-of-times-a-string-appears-within-a-string
            if ((partIdsCommaDelimitedList.Length.Equals(0)) || (commaCount.Equals(0)))
                throw new NoPartsException(lineCount.ToString());
            //Malformed Line Check
            var parsedPartIds = RemoveSpacesFromBeginningAndEnd(partIdsCommaDelimitedList.Split(WidgetPartDelimiter.ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList());
            if ((parsedPartIds.Count.Equals(0)) || (parsedPartIds.Count != (commaCount + 1))) //There should be 1 less comma than there are parts
                throw new MalformedLineException(lineCount.ToString());
            return new SpecedWidget() { WidgetName = widgetName, Parts = parsedPartIds.Distinct().Select(s => new SpecedPart() { partId = s, numberOfPart = parsedPartIds.Count(c => s == c) }) };
        }

        public void ShowOrderedAndAssembledWidgets(List<Widget> widgetsAssembled)
        {
            foreach (var widget in widgetsAssembled)
            {
                Trace.WriteLine(String.Format("We have successfully assembled your widget {0}", widget.WidgetName));
                Trace.WriteLine("The parts list includes:");
                foreach (var part in widget.Parts)
                {
                    Trace.WriteLine(String.Format("Part with id {0}, and serial number {1}", part.Id, part.SerialNo));
                }
            }
        }

        public List<Widget> OrderWidgetsFromParsedOrderFile(string pathToFile, List<String> widgetsOrdered, List<SpecedWidget> specedWidgets)
        {
            var wh = new Warehouse();
            if (widgetsOrdered.Count > 0)
            {
                var assembledWidgets = OrderManyParts(wh, widgetsOrdered, specedWidgets);
                return assembledWidgets;
            }
            else
            {
                throw new NoWidgetsException(pathToFile);
            }
        }

        public List<String> ParseOrderFile(string pathToFile, List<SpecedWidget> specedWidgets)
        {
            var fileLines = File.ReadLines(pathToFile);
            var widgetListToReturn = new List<String>();
            var wh = new Warehouse();
            var lineCount = 0;
            foreach (var widgetName in fileLines)
            {
                lineCount++;
                try
                {
                    widgetListToReturn.Add(ReadOrderFileLines(widgetName, lineCount, specedWidgets));
                }
                catch (NoWidgetNameException ex)
                {
                    Trace.WriteLine(String.Format(NoWidgetNameException.message, ex.Message));
                }
                catch (WidgetNotSpecedException ex)
                {
                    Trace.WriteLine(String.Format(WidgetNotSpecedException.message, ex.Message));
                }
            }
            if (lineCount.Equals(0))
            {
                throw new NoWidgetsException(pathToFile);
            }
            return RemoveSpacesFromBeginningAndEnd(widgetListToReturn);
        }

        public string ReadOrderFileLines(string widgetName, int lineCount, List<SpecedWidget> specedWidgets)
        {
            if (widgetName.Length.Equals(0))
                throw new NoWidgetNameException(lineCount.ToString());
            var currentWidgetNames = specedWidgets.Select(widget => widget.WidgetName);
            if (!currentWidgetNames.Contains(widgetName))
                throw new WidgetNotSpecedException(widgetName);
            return widgetName;
        }

        public List<Widget> OrderManyParts(Warehouse wh, List<String> widgetsOrdered, List<SpecedWidget> specedWidgets)
        {
            var OrderedWidgetsThatAreSpeced = specedWidgets.Where(w => widgetsOrdered.Contains(w.WidgetName)); //Widgets that we know we have the spec for.
            var OrderedWidgetsThatAreNotSpeced = widgetsOrdered.Except(OrderedWidgetsThatAreSpeced.Select(s => s.WidgetName)); //Widgets that we do not have the spec for.
            var widgetsAssembled = new List<Widget>();
            foreach (var widgetThatWeCantOrder in OrderedWidgetsThatAreNotSpeced)//Display messages to let users know that we cannot fulfill their order on these items
            {
                Trace.WriteLine(String.Format("We currently do not produce widget {0}. Please resubmit your request for this item at a later date.", widgetThatWeCantOrder));
            }
            foreach (var widget in OrderedWidgetsThatAreSpeced)
            {
                Widget w = new Widget();
                bool Assembled = false;
                foreach (var part in widget.Parts)
                {
                    try
                    {
                        for (var i = 0; i < part.numberOfPart; i++)
                        {
                            w.Parts.Add(OrderOnePart(part.partId, widget.WidgetName, wh));
                        }
                    }
                    catch (PartOrderException)
                    {
                        Trace.WriteLine(String.Format("Ordering failed for part {0}, for widget {1}. This part does not exist in our system at the moment.", part.partId, widget.WidgetName));
                        Assembled = false;
                    }
                }
                if (Assembled)
                    widgetsAssembled.Add(w);
            }
            return widgetsAssembled;
        }

        private Part OrderOnePart(string partId, string widgetName, Warehouse wh)
        {
            if (!wh.Available(partId))
            {
                wh.Order(partId);
            }
            var part = wh.Retreive(partId);
            Trace.WriteLine(String.Format("We have successfully ordered part {0}, with serial number {1}, for widget {2}", part.Id, part.SerialNo, widgetName));
            return part;
        }
    }
}
