using FactoryApp;
using System;
using System.Collections.Generic;
using WidgetFactory;

public interface IFactory {
    void ShowOrderedAndAssembledWidgets(List<Widget> widgetsAssembled);
    List<Widget> OrderWidgetsFromParsedOrderFile(string pathToFile, List<String> widgetsOrdered, List<SpecedWidget> specedWidgets);
    List<String> ParseOrderFile(string pathToFile, List<SpecedWidget> SpecedWidgets);
    String ReadOrderFileLines(string widgetName, int lineCount, List<SpecedWidget> specedWidgets);
    List<SpecedWidget> ParseSpecFile(string pathToFile);
    SpecedWidget ReadSpecFileLines(String line, int lineCount);
    List<Widget> OrderManyParts(Warehouse wh, List<String> widgetsOrdered, List<SpecedWidget> specedWidgets);
}