using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace Events_.NET
{
    public class Event
    {
        public DateTime Timestamp { get; set; }
        public List<string> Columns { get; set; }

        public Event(string logLine)
        {
            var splitText = logLine.Split(',');
            var format = "dd/MM/yy HH:mm:ss UTC";
            Timestamp = DateTime.ParseExact(splitText[0], format, CultureInfo.InvariantCulture);
            Columns = splitText.Skip(1).ToList();
        }
    }
}
