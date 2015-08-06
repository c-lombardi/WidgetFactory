using System;
using System.Collections.Generic;
using System.Xml;
using WidgetFactory;

namespace ConsoleApplication2
{
    public interface IFactory {
        void ShowOrderedAndAssembledWidgets(List<Widget> widgetsAssembled);
        List<Widget> OrderWidgetsFromParsedOrderFile(string pathToFile, List<String> widgetsOrdered, List<SpecedWidget> specedWidgets);
        List<String> ParseOrderFile(string pathToFile, List<SpecedWidget> specedWidgets);
        String ReadOrderFileLines(XmlNode widget, int widgetCounter, List<SpecedWidget> specedWidgets);
        List<SpecedWidget> ParseSpecFile(string pathToFile);
        SpecedWidget ReadSpecFileLines(XmlNode widget, int widgetCounter);
        List<Widget> OrderManyParts(Warehouse wh, List<String> widgetsOrdered, List<SpecedWidget> specedWidgets);
    }
}