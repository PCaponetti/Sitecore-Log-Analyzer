using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Text;
using LogAnalyzer.ModelClasses;

namespace LogAnalyzer.ControllerClasses
{
    public class LogAnalyzerController
    {
        public List<LogEntry> ParseFile(Stream file, string filename, List<LogEntry> existingEntries)
        {
            // TODO: error checking
            System.IO.StreamReader sr = new StreamReader(file);
            StringBuilder sb = new StringBuilder();

            List<LogEntry> entries = existingEntries??new List<LogEntry>();

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (LogEntry.LogTimeStamp.IsMatch(line))
                {
                    if (sb.Length > 0)
                    {
                        // found a full log entry in sb, parse it and clear it out
                        LogEntry entry = null;
                        if (LogEntry.TryParse(sb.ToString(), filename, out entry))
                        {
                            // got one!
                            entries.Add(entry);
                        }
                        else
                        {
                            // what do I do with this error?
                        }
                        sb.Clear();
                    }
                }
                sb.AppendLine(line);

            }
            return entries;
        }
    }
}
