using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WidgetFactory;

namespace FactoryApp
{
    public class SpecedWidget
    {
        public string WidgetName { get; set; } //In database design this would be Unique
        public IEnumerable<SpecedPart> Parts { get; set; }
    }
    public class SpecedPart
    {
        public string partId { get; set; }
        public int numberOfPart { get; set; }
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
