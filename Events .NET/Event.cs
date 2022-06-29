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

        public Event(string logLine, int timestampColumn, Cliver.DateTimeRoutines.DateTimeFormat format)
        {
            var splitText = logLine.Split(',');
            DateTime dt;
            var success = Cliver.DateTimeRoutines.TryParseDateTime(splitText[timestampColumn], format, out dt);
            if(success)
            {
                Timestamp = dt;
                Columns = splitText.Skip(1).ToList();
            }
            else
            {
                throw new Exception("Unable to parse timestamp.");
            }
        }
    }
}
