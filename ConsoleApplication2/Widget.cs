using System.Collections.Generic;
using WidgetFactory;

namespace ConsoleApplication2
{
    public class SpecedWidget
    {
        public string WidgetName { get; set; } //In database design this would be Unique
        public IEnumerable<SpecedPart> Parts { get; set; }
    }
    public class SpecedPart
    {
        public string PartId { get; set; }
        public int NumberOfPart { get; set; }
    }
    public class Widget
    {
        public string WidgetName { get; set; } //In database design this would be Unique
        public ICollection<Part> Parts { get; set; }

        public Widget()
        {
            Parts = new List<Part>();
        }
    }
}
