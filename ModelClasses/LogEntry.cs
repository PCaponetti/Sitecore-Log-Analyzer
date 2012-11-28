using System;
using System.Text.RegularExpressions;

namespace LogAnalyzer.ModelClasses
{
    public enum LogType { Unknown = 0, Info, Warning, Error }
    public class LogEntry
    {
        /**
         * 
         * log messages are made up of
         * source - ManagedPoolThread ###, ####, Heartbeat
         * timestamp
         * type - warn, info, error, etc
         * message (optional)
         * exception (optional)
         * stack trace (optional)
         * 
         **/
        public static Regex LogTimeStamp = new Regex("\\b\\d\\d\\:\\d\\d\\:\\d\\d\\b");
        public static Regex LogExceptionLine = new Regex("\nException\\:\\s.*?\n");
        public static Regex LogExceptionMessageLine = new Regex("\nMessage\\:\\s.*?\n");
        public static Regex LogNumber = new Regex(@"\b[0-9][0-9.,]* ?[KGM]?B?\b");
        public static Regex LogGuid = new Regex(@"\b[{|\(]?[0-9a-fA-F]{8}[-]?([0-9a-fA-F]{4}[-]?){3}[0-9a-fA-F]{12}[\)|}]?\b");

        public DateTime TimeStamp { get; set; }
        public string Source { get; set; }
        public LogType LogType { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
        public string FullLogEntry { get; set; }
        public int CommentCount { get; set; }

        public string DisplayString
        {
            get
            {
                return (LogType == LogType.Error ? Exception + "\n" + Message : Message + "").Replace("\r", "\n").Trim();
            }
        }

        public static bool TryParse(string log, string filename, out LogEntry entry)
        {
            entry = new LogEntry();
            var matches = LogTimeStamp.Matches(log);
            if (matches.Count > 0)
            {
                try
                {
                    entry.FullLogEntry = log;

                    // get the timestamp from the filename and timestamp in the log
                    string strTime = matches[0].Value;
                    entry.TimeStamp = new DateTime(
                        int.Parse(filename.Substring(4, 4))
                        , int.Parse(filename.Substring(8, 2))
                        , int.Parse(filename.Substring(10, 2))
                        , int.Parse(strTime.Substring(0, 2))
                        , int.Parse(strTime.Substring(3, 2))
                        , int.Parse(strTime.Substring(6, 2)));

                    // get source of log entry
                    int timestampIndex = log.IndexOf(strTime);
                    entry.Source = log.Substring(0, timestampIndex).Trim();

                    // get any exceptions
                    matches = LogExceptionLine.Matches(log);
                    if (matches.Count > 0) entry.Exception = matches[0].Value.Trim();

                    // get any exception messages
                    matches = LogExceptionMessageLine.Matches(log);
                    if (matches.Count > 0) entry.Message = matches[0].Value.Trim();

                    // get type of log entry
                    if (log.IndexOf("WARN") == timestampIndex + 9)
                    {
                        entry.LogType = LogType.Warning;

                        entry.Message = entry.FullLogEntry.Substring(entry.FullLogEntry.IndexOf("WARN") + 5);
                        // special case for analytics warnings
                        if (entry.Message.IndexOf("Analystics: Max size of insert queue reached. Dropped ") > -1)
                        {
                            entry.Message =
                                entry.Message.Substring(0,
                                                        entry.Message.IndexOf(
                                                            "Analystics: Max size of insert queue reached. Dropped ") +
                                                        46);
                        }
                        // special case for Item threshold
                        if (entry.FullLogEntry.Contains("Item threshold exceeded for web page. Items accessed: "))
                        {
                            entry.Message = "Item threshold exceeded for web page.Items accessed: [num items] " +
                                            entry.Message.Substring(entry.Message.IndexOf("Threshold"));
                        }
                    }
                    else if (log.IndexOf("ERROR") == timestampIndex + 9)
                    {
                        entry.LogType = LogType.Error;
                        if (string.IsNullOrEmpty(entry.Message))
                        {
                            entry.Message = entry.FullLogEntry.Substring(entry.FullLogEntry.IndexOf("ERROR") + 6);
                        }
                    }
                    else if (log.IndexOf("FATAL") == timestampIndex + 9)
                    {
                        entry.LogType = LogType.Error;
                        if (string.IsNullOrEmpty(entry.Message))
                        {
                            entry.Message = entry.FullLogEntry.Substring(entry.FullLogEntry.IndexOf("FATAL") + 6);
                        }
                    }
                    else if (log.IndexOf("INFO") == timestampIndex + 9)
                    {
                        entry.LogType = LogType.Info;
                    }
                    else
                    {
                        entry.LogType = LogType.Unknown;
                    }

                    if (!string.IsNullOrEmpty(entry.Message))
                    {
                        entry.Message = entry.Message.Trim();
                        // remove any specific GUIDs from the entry.Message
                        entry.Message = LogGuid.Replace(entry.Message, "[GUID]");
                        // remove any specific numbers from the entry.Message
                        entry.Message = LogNumber.Replace(entry.Message, "[number]");
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    // TODO: what do I do with this error?
                }
            }
            return false;
        }
    }
}