using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WidgetFactory;
using System.Xml;
namespace FactoryApp
{
    public class Factory : IFactory
    {
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
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(pathToFile);
            var childNodes = xmlDoc.DocumentElement.ChildNodes;
            if(childNodes.Count == 0)
                throw new NoWidgetsException(pathToFile);
            var widgetCounter = 0;
            var widgetsSpeced = new List<SpecedWidget>();
            foreach(XmlNode widget in childNodes)
            {
                try
                {
                    widgetCounter++;
                    widgetsSpeced.Add(ReadSpecFileLines(widget, widgetCounter));
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
            return widgetsSpeced;
        }

        //Parse the file
        public SpecedWidget ReadSpecFileLines(XmlNode widget, int widgetCounter)
        {
            if (!widget.HasChildNodes)
                throw new LineEmptyException(widgetCounter.ToString());
            var widgetName = widget.FirstChild.InnerText;
            if (widgetName.Length == 0)
                throw new NoWidgetNameException();
            var specedWidgetList = new List<SpecedWidget>();
            var widgetPartNodes = widget.SelectNodes("part");
            if (widgetPartNodes.Count == 0)
                throw new NoPartsException(widgetName);
            StringBuilder commaDelimitedPartString = new StringBuilder();
            foreach (XmlNode widgetProperty in widgetPartNodes)
            {
                var partId = widgetProperty.InnerText;
                if (String.IsNullOrEmpty(partId))
                    throw new MalformedLineException(widgetCounter.ToString());
                if (widget.LastChild != widgetProperty)
                {
                    commaDelimitedPartString.Append(partId + ",");
                }
                else
                {
                    commaDelimitedPartString.Append(partId);
                }
            }
            var parsedPartIds = RemoveSpacesFromBeginningAndEnd(commaDelimitedPartString.ToString().Split(WidgetPartDelimiter.ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList());
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
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(pathToFile);
            var childNodes = xmlDoc.DocumentElement.ChildNodes;
            if (childNodes.Count == 0)
                throw new NoWidgetsException(pathToFile);
            var widgetCounter = 0;
            var widgetListToReturn = new List<String>();
            foreach (XmlNode widget in childNodes)
            {
                try
                {
                    widgetCounter++;
                    widgetListToReturn.Add(ReadOrderFileLines(widget, widgetCounter, specedWidgets));
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
            return RemoveSpacesFromBeginningAndEnd(widgetListToReturn);
        }

        public string ReadOrderFileLines(XmlNode widget, int widgetCounter, List<SpecedWidget> specedWidgets)
        {
            var widgetName = widget.InnerText;
            if (widgetName.Length == 0)
                throw new NoWidgetNameException();
            var currentWidgetNames = specedWidgets.Select(w => w.WidgetName);
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
                bool Assembled = true;
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
