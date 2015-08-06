using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FactoryApp;
using System.Diagnostics;
using WidgetFactory;
using System.Collections.Generic;
using System.Linq;
using Rhino.Mocks;
namespace TestFactory
{
    [TestClass]
    public class FactoryTests
    {
        private Factory f;
        private const string specFileName = "../../Spec.txt";
        private const string orderFilename = "../../Widget.txt";
        public const string EmptyLineFileName = "EmptyLineException.txt";
        public const string NoWidgetNameFileName = "NoWidgetNameException.txt";
        private const string motorCycleName = "motorcycle";
        private const string carName = "car";
        private const string truckName = "truck";
        String[] widgetNames = new String[] { motorCycleName, carName, truckName };
        String[] orders = new String[] { carName, carName, motorCycleName, truckName, carName, truckName, motorCycleName, carName, carName };
        String[] motorCycleParts = new String[] { "wheel", "wheel", "engine", "seat", "handlebar" };
        String[] carParts = new String[] { "wheel", "wheel", "wheel", "wheel", "engine", "seat", "steering wheel", "sunroof" };
        String[] truckParts = new String[] { "wheel", "wheel", "wheel", "wheel", "engine", "seat", "steering wheel", "truck-bed" };

        [TestInitialize]
        public void Init()
        {
            f = new Factory();
        }
        [TestMethod]
        public void ParseOrderFileTest()
        {
            var specedWidgets = new List<SpecedWidget>();
            specedWidgets.Add(new SpecedWidget() { WidgetName = motorCycleName, Parts = motorCycleParts.Select(s => new SpecedPart() { partId = s, numberOfPart = motorCycleParts.Count(c => c == s) }) });
            specedWidgets.Add(new SpecedWidget() { WidgetName = carName, Parts = carParts.Select(s => new SpecedPart() { partId = s, numberOfPart = carParts.Count(c => c == s) }) });
            specedWidgets.Add(new SpecedWidget() { WidgetName = truckName, Parts = truckParts.Select(s => new SpecedPart() { partId = s, numberOfPart = truckParts.Count(c => c == s) }) });
            var widgetsOrdered = f.ParseOrderFile(orderFilename, specedWidgets);
            var widgetCount = 0;
            Assert.IsTrue(widgetsOrdered.Count == orders.Count());
            foreach (var widget in widgetsOrdered)
            {
                Assert.AreEqual(widget, orders[widgetCount]);
                widgetCount++;
            }
        }

        [TestMethod]
        public void ParseSpecFile()
        {
            var allParts = motorCycleParts.Concat(carParts).Concat(truckParts);
            var widgetsSpeced = f.ParseSpecFile(specFileName);
            Assert.IsTrue(widgetsSpeced.Count == widgetNames.Count());
            var partCount = 0;
            foreach (var partId in widgetsSpeced.SelectMany(widget => widget.Parts.Select(s => s.partId)).Distinct())
            {
                Assert.IsTrue(allParts.Contains(partId));
                partCount++;
            }
            foreach (var widget in widgetsSpeced)
            {
                Assert.IsTrue(widgetNames.Contains(widget.WidgetName));
                switch(widget.WidgetName)
                {
                    case motorCycleName:
                    {
                        foreach(var motorcyclePart in widget.Parts)
                        {
                            Assert.IsTrue(motorCycleParts.Contains(motorcyclePart.partId));
                            Assert.IsTrue(motorcyclePart.numberOfPart == motorCycleParts.Count(c => c == motorcyclePart.partId));
                        }
                        break;
                    }
                    case carName:
                    {
                        foreach (var carPart in widget.Parts)
                        {
                            Assert.IsTrue(carParts.Contains(carPart.partId));
                            Assert.IsTrue(carPart.numberOfPart == carParts.Count(c => c == carPart.partId));
                        }
                        break;
                    }
                    case truckName:
                    {
                        foreach (var truckPart in widget.Parts)
                        {
                            Assert.IsTrue(truckParts.Contains(truckPart.partId));
                            Assert.IsTrue(truckPart.numberOfPart == truckParts.Count(c => c == truckPart.partId));
                        }
                        break;
                    }
                    default:
                    {
                        //Doesn't exist!
                        throw new Exception("Widget was not part of this test!");
                    }
                }
            }
        }
        [TestMethod]
        [ExpectedException(typeof(WidgetNotSpecedException))]
        public void ParseOrderFileWidgetNotSpecedException()
        {
            f.ReadOrderFileLines("widgetName", 0, new List<SpecedWidget>());
        }

        [TestMethod]
        [ExpectedException(typeof(NoWidgetNameException))]
        public void ParseOrderFileNoWidgetNameException()
        {
            f.ReadOrderFileLines("", 0, new List<SpecedWidget>());
        }

        [TestMethod]
        [ExpectedException(typeof(NoPartsException))]
        public void ParseSpecFileExpectNoPartsException()
        {
            f.ReadSpecFileLines("widgetName:", 10);
        }
        [TestMethod]
        [ExpectedException(typeof(MalformedLineException))]
        public void ParseSpecFileExpectMalformedLineEmptyException()
        {
            f.ReadSpecFileLines(" ", 10);
        }
        [TestMethod]
        [ExpectedException(typeof(MalformedLineException))]
        public void ParseSpecFileExpectMalformedLineCommasException()
        {
            f.ReadSpecFileLines("widgetName: wheel,, engine", 10);
        }
        [TestMethod]
        [ExpectedException(typeof(LineEmptyException))]
        public void ParseSpecFileExpectLineEmptyException()
        {
            f.ReadSpecFileLines("", 10);
        }

        [TestMethod]
        [ExpectedException(typeof(NoWidgetNameException))]
        public void ParseSpecFileExpectNoWidgetNameException()
        {
            f.ReadSpecFileLines(": wheel, wheel, engine", 10);
        }

        [TestCleanup]
        public void CleanUp()
        {
            
        }
    }
}
