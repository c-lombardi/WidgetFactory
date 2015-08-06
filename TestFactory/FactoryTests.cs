using System;
using ConsoleApplication2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace TestFactory
{
    [TestClass]
    public class FactoryTests
    {
        private Factory _f;
        private const string SpecFileName = "../../Spec.xml";
        private const string OrderFilename = "../../Widget.xml";
        public const string EmptyLineFileName = "EmptyLineException.txt";
        public const string NoWidgetNameFileName = "NoWidgetNameException.txt";
        private const string MotorCycleName = "motorcycle";
        private const string CarName = "car";
        private const string TruckName = "truck";
        readonly String[] _widgetNames = { MotorCycleName, CarName, TruckName };
        readonly String[] _orders = { CarName, CarName, MotorCycleName, TruckName, CarName, TruckName, MotorCycleName, CarName, CarName };
        readonly String[] _motorCycleParts = { "wheel", "wheel", "engine", "seat", "handlebar" };
        readonly String[] _carParts = { "wheel", "wheel", "wheel", "wheel", "engine", "seat", "steering wheel", "sunroof" };
        readonly String[] _truckParts = { "wheel", "wheel", "wheel", "wheel", "engine", "seat", "steering wheel", "truck-bed" };

        [TestInitialize]
        public void Init()
        {
            _f = new Factory();
        }
        [TestMethod]
        public void ParseOrderFileTest()
        {
            var specedWidgets = new List<SpecedWidget>
            {
                new SpecedWidget
                {
                    WidgetName = MotorCycleName,
                    Parts =
                        _motorCycleParts.Select(
                            s => new SpecedPart {PartId = s, NumberOfPart = _motorCycleParts.Count(c => c == s)})
                },
                new SpecedWidget
                {
                    WidgetName = CarName,
                    Parts =
                        _carParts.Select(s => new SpecedPart {PartId = s, NumberOfPart = _carParts.Count(c => c == s)})
                },
                new SpecedWidget
                {
                    WidgetName = TruckName,
                    Parts =
                        _truckParts.Select(
                            s => new SpecedPart {PartId = s, NumberOfPart = _truckParts.Count(c => c == s)})
                }
            };
            var widgetsOrdered = _f.ParseOrderFile(OrderFilename, specedWidgets);
            var widgetCount = 0;
            Assert.IsTrue(widgetsOrdered.Count == _orders.Count());
            foreach (var widget in widgetsOrdered)
            {
                Assert.AreEqual(widget, _orders[widgetCount]);
                widgetCount++;
            }
        }

        [TestMethod]
        public void ParseSpecFile()
        {
            var allParts = _motorCycleParts.Concat(_carParts).Concat(_truckParts);
            var widgetsSpeced = _f.ParseSpecFile(SpecFileName);
            Assert.IsTrue(widgetsSpeced.Count == _widgetNames.Count());
            var partCount = 0;
            foreach (var partId in widgetsSpeced.SelectMany(widget => widget.Parts.Select(s => s.PartId)).Distinct())
            {
                Assert.IsTrue(allParts.Contains(partId));
                partCount++;
            }
            foreach (var widget in widgetsSpeced)
            {
                Assert.IsTrue(_widgetNames.Contains(widget.WidgetName));
                switch(widget.WidgetName)
                {
                    case MotorCycleName:
                    {
                        foreach(var motorcyclePart in widget.Parts)
                        {
                            Assert.IsTrue(_motorCycleParts.Contains(motorcyclePart.PartId));
                            Assert.IsTrue(motorcyclePart.NumberOfPart == _motorCycleParts.Count(c => c == motorcyclePart.PartId));
                        }
                        break;
                    }
                    case CarName:
                    {
                        foreach (var carPart in widget.Parts)
                        {
                            Assert.IsTrue(_carParts.Contains(carPart.PartId));
                            Assert.IsTrue(carPart.NumberOfPart == _carParts.Count(c => c == carPart.PartId));
                        }
                        break;
                    }
                    case TruckName:
                    {
                        foreach (var truckPart in widget.Parts)
                        {
                            Assert.IsTrue(_truckParts.Contains(truckPart.PartId));
                            Assert.IsTrue(truckPart.NumberOfPart == _truckParts.Count(c => c == truckPart.PartId));
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
        [ExpectedException(typeof(WidgetNotSpecedException))]//https://msdn.microsoft.com/en-us/library/ms162365(v=vs.110).aspx
        public void ParseOrderFileWidgetNotSpecedException()
        {
            var doc = new XmlDocument();
            var dummyNode = doc.CreateNode("text", "widget", "NameSpace");
            dummyNode.InnerText = "test";
            _f.ReadOrderFileLines(dummyNode, 0, new List<SpecedWidget>());
        }

        [TestMethod]
        [ExpectedException(typeof(NoWidgetNameException))]
        public void ParseOrderFileNoWidgetNameException()
        {
            var doc = new XmlDocument();
            var dummyNode = doc.CreateNode("text", "widget", "NameSpace");
            _f.ReadOrderFileLines(dummyNode, 0, new List<SpecedWidget>());
        }

        [TestMethod]
        [ExpectedException(typeof(NoPartsException))]
        public void ParseSpecFileExpectNoPartsException()
        {
            var doc = new XmlDocument();
            var allWidgetsNode = doc.CreateNode("element", "allWidgets", "NameSpace");
            var widgetNode = doc.CreateNode("element", "widget", "NameSpace");
            var widgetNameNode = doc.CreateNode("text", "widgetName", "NameSpace");
            widgetNameNode.InnerText = "testName";
            widgetNode.AppendChild(widgetNameNode);
            allWidgetsNode.AppendChild(widgetNode);
            _f.ReadSpecFileLines(allWidgetsNode, 10);
        }
        [ExpectedException(typeof(LineEmptyException))]
        public void ParseSpecFileExpectLineEmptyException()
        {
            var doc = new XmlDocument();
            var allWidgetsNode = doc.CreateNode("text", "allWidets", "NameSpace");
            allWidgetsNode.InnerText = " ";
            _f.ReadSpecFileLines(allWidgetsNode, 10);
        }
        [TestMethod]
        [ExpectedException(typeof(MalformedLineException))]
        public void ParseSpecFileExpectMalformedLineEmptyException()
        {
            var doc = new XmlDocument();
            doc.LoadXml(@"<widget>
		                    <widgetName>motorcycle</widgetName>
		                    <part></part>
		                    <part>wheel</part>
		                    <part>engine</part>
		                    <part>seat</part>
		                    <part>handlebar</part>
	                    </widget>");
            _f.ReadSpecFileLines(doc.FirstChild, 10);
        }

        [TestMethod]
        [ExpectedException(typeof(NoWidgetNameException))]
        public void ParseSpecFileExpectNoWidgetNameException()
        {
            var doc = new XmlDocument();
            doc.LoadXml(@"<widget>
		                    <widgetName></widgetName>
		                    <part>wheel</part>
		                    <part>wheel</part>
		                    <part>engine</part>
		                    <part>seat</part>
		                    <part>handlebar</part>
	                    </widget>");
            _f.ReadSpecFileLines(doc.FirstChild, 10);
        }

        [TestCleanup]
        public void CleanUp()
        {
            
        }
    }
}
