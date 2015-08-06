using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryApp
{
    public partial class LineEmptyException : Exception //https://msdn.microsoft.com/en-us/library/87cdya3t(v=vs.110).aspx
    {
        public const string message = "Line {0} was empty";
        public LineEmptyException() { }
        public LineEmptyException(string message) : base(message) { }
        public LineEmptyException(string message, Exception inner): base( message, inner) { }
    }

    public partial class NoWidgetNameException : Exception
    {
        public const string message = "There was no widget name on line {0}";
        public NoWidgetNameException() { }
        public NoWidgetNameException(string message) : base(message) { }
        public NoWidgetNameException(string message, Exception inner) : base(message, inner) { }
    }

    public partial class NoPartsException : Exception
    {
        public const string message = "There were no parts listed on widget {0}";
        public NoPartsException() { }
        public NoPartsException(string message) : base(message) { }
        public NoPartsException(string message, Exception inner) : base(message, inner) { }
    }

    public partial class NoWidgetsException : Exception
    {
        public const string message = "There were no widgets in the file at {0}";
        public NoWidgetsException() { }
        public NoWidgetsException(string message) : base(message) { }
        public NoWidgetsException(string message, Exception inner) : base(message, inner) { }
    }

    public partial class MalformedLineException : Exception
    {
        public const string message = "Line {0} was malformed. Proper formation is: <allWidgets><widget><widgetName></widgetName><part></part><part></part></widget>...</allWidgets>";
        public MalformedLineException() { }
        public MalformedLineException(string message) : base(message) { }
        public MalformedLineException(string message, Exception inner) : base(message, inner) { }
    }

    public partial class WidgetNotSpecedException : Exception
    {
        public const string message = "There is no data on how to build widget {0}";
        public WidgetNotSpecedException() { }
        public WidgetNotSpecedException(string message) : base(message) { }
        public WidgetNotSpecedException(string message, Exception inner) : base(message, inner) { }
    }
}
