using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LogAnalyzer
{
    public class Constants
    {
        public const string SESSION_KEY_LOG_ENTRIES = "LogEntries";
        public const string SESSION_KEY_LOG_FILES = "LogFiles";
        public const string QUERY_STRING_EXCEPTION = "ex";
        public const string QUERY_STRING_DATE = "dt";
        public const string QUERY_STRING_FILENAME = "fn";

        public const string DATETIME_FORMAT = "MM/dd/yy hh:mm";

        public const double MAXIMUM_TIME_WINDOW_HOURS = 24.0;
    }
}