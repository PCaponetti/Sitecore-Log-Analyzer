using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using LogAnalyzer.ControllerClasses;
using LogAnalyzer.ModelClasses;
using MS.Internal.Xml.XPath;
using System.IO;
using System.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LogAnalyzer
{
    public partial class Default : System.Web.UI.Page
    {
        private LogAnalyzer.ControllerClasses.LogAnalyzerController logController = new LogAnalyzerController();
        private const int BAR_MAGNIFICATION = 4;
        private const string SERVICE_URL = "http://www.sitecoreloganalyzer.com/GetSuggestedFixes.svc/";
        //private const string SERVICE_URL = "http://localhost:56299/GetSuggestedFixes.svc/";
        private const string GET_COMMENTS_FOR_EXCEPTIONS_URL = "GetCommentNumbersForExceptions";

        protected void Page_Load(object sender, EventArgs e)
        {
            var asm = Assembly.Load("Sitecore.Kernel");
            string logFolder = string.Empty;
            if(asm != null)
            {
                try
                {
                    var sc = asm.GetType("Sitecore.Configuration.Settings");
                    logFolder = sc.GetProperty("LogFolder").GetValue(null, new object[0]).ToString();
                    btnLocalLogs.Visible = Directory.Exists(logFolder);
                }
                catch(Exception ex)
                {
                    // doesn't matter
                }
            }
            string localFileName = Request.QueryString[Constants.QUERY_STRING_FILENAME];
            if (!string.IsNullOrEmpty(localFileName))
            {
                // get the file from the query string
                var f = new FileStream(localFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                ParseFile(f, localFileName.Substring(localFileName.LastIndexOf("\\") + 1));
                Response.Redirect("Default.aspx");
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (Session[Constants.SESSION_KEY_LOG_ENTRIES] != null)
                BindData(Session[Constants.SESSION_KEY_LOG_ENTRIES] as List<LogEntry>);
            litFiles.Text = string.Empty;
            btnStartOver.Visible = false;
            if (Session[Constants.SESSION_KEY_LOG_FILES] != null)
            {
                litFiles.Text = "Files analyzed: ";
                var tmpLogFiles = (List<string>)Session[Constants.SESSION_KEY_LOG_FILES];
                foreach (string filename in tmpLogFiles)
                    litFiles.Text += string.Format("<div class=\"filename\">{0}</div>", filename);
                btnStartOver.Visible = tmpLogFiles.Count > 0;
            }
        }

        protected void btnAnalyze_Click(object sender, EventArgs e)
        {
            if (fileLogFile.PostedFile != null)
            {
                if (!ParseFile(fileLogFile.PostedFile.InputStream, fileLogFile.PostedFile.FileName)) return;
            }
        }

        private bool ParseFile(Stream file, string filename)
        {
            // add filename to list of files being analyzed
            if (Session[Constants.SESSION_KEY_LOG_FILES] == null)
                Session[Constants.SESSION_KEY_LOG_FILES] = new List<string>();
            // check to see if we already have the file in Session
            if (((List<string>)Session[Constants.SESSION_KEY_LOG_FILES]).Contains(filename))
                return false;

            // parse the file
            var tmpEntries = new List<LogEntry>();
            var entries = new List<LogEntry>();
            if (Session[Constants.SESSION_KEY_LOG_ENTRIES] != null)
            {
                tmpEntries = ((List<LogEntry>)Session[Constants.SESSION_KEY_LOG_ENTRIES]).ToList();
                entries = logController.ParseFile(file, filename, ((List<LogEntry>)Session[Constants.SESSION_KEY_LOG_ENTRIES]));
            }
            else
            {
                entries = logController.ParseFile(file, filename, null);
            }
            entries.Sort((x, y) => x.TimeStamp.CompareTo(y.TimeStamp));
            if (entries.Count > 1 && entries[entries.Count - 1].TimeStamp.Subtract(entries[0].TimeStamp).TotalHours >= Constants.MAXIMUM_TIME_WINDOW_HOURS)
            {
                // TODO: make this more elegant?
                Response.Write(string.Format("<script>alert('Analyzed logs cannot span over {0} hours')</script>", Constants.MAXIMUM_TIME_WINDOW_HOURS));
                entries = tmpEntries;
            }
            else
            {
                ((List<string>)Session[Constants.SESSION_KEY_LOG_FILES]).Add(filename);
            }
            Session[Constants.SESSION_KEY_LOG_ENTRIES] = entries;

            return true;
        }

        private void BindData(List<LogEntry> entries)
        {
            // quick null check
            if (entries == null)
            {
                rptExceptions.Visible = false;
                rptWarnings.Visible = false;
                pnlTimelineSlider.Visible = false;
                return;
            }

            var whatsThis = GetCommentCountsForExceptions(entries);

            // get the exceptions table data
            var tmp = entries.Where(x => x.LogType == LogType.Error).GroupBy(x => x.DisplayString).ToList();
            tmp.Sort((x, y) => y.Count().CompareTo(x.Count()));
            rptExceptions.Visible = tmp.Count > 0;
            if (tmp.Count > 0)
            {
                SetCommentCounts(whatsThis, tmp);
                rptExceptions.DataSource = tmp;
                rptExceptions.DataBind();
            }

            // get the warnings table data
            tmp = entries.Where(x => x.LogType == LogType.Warning).GroupBy(x => x.DisplayString).ToList();
            tmp.Sort((x, y) => y.Count().CompareTo(x.Count()));
            rptWarnings.Visible = tmp.Count > 0;
            if (tmp.Count > 0)
            {
                SetCommentCounts(whatsThis, tmp);
                rptWarnings.DataSource = tmp;
                rptWarnings.DataBind();
            }

            // get the timeline data
            pnlTimelineSlider.Visible = entries.Count > 0;
            tmp = entries.GroupBy(x => x.TimeStamp.ToString("yyyy-MM-dd hh:mm")).ToList();
            var topRow = new StringBuilder();
            var bottomRow = new StringBuilder();
            int entriesIndex = 0;

            if (entries.Count < 1) return;
            for (int min = 0; min < entries[entries.Count - 1].TimeStamp.Subtract(entries[0].TimeStamp).TotalMinutes; min++)
            {
                if (min % 5 == 0)
                {
                    // output to top row <td colspan="5">TIMESTAMP</td>
                    topRow.Append(string.Format("<td colspan=\"5\">{0}</td>",
                                                entries[0].TimeStamp.AddMinutes(min).ToString(Constants.DATETIME_FORMAT)));
                }

                // get all entries in this minute and aggregate numbers
                DateTime nextMin = entries[0].TimeStamp.AddMinutes(min + 1);
                int numInfo = 0;
                int numWarn = 0;
                int numErr = 0;
                int numUnknown = 0;
                while (entriesIndex < entries.Count && entries[entriesIndex].TimeStamp < nextMin)
                {
                    if (entries[entriesIndex].LogType == LogType.Info) numInfo++;
                    else if (entries[entriesIndex].LogType == LogType.Warning) numWarn++;
                    else if (entries[entriesIndex].LogType == LogType.Error) numErr++;
                    else numUnknown++;
                    entriesIndex++;
                }
                string curTimestamp = entries[0].TimeStamp.AddMinutes(min).ToString(Constants.DATETIME_FORMAT);
                bottomRow.Append("<td><table cellspacing=0 cellpadding=0><tr>");
                // info bar
                bottomRow.Append(
                    string.Format(
                        "<td><div class=\"graphBar info\" style=\"height: {0}px;\" title=\"{1}: {2} informational logs\" dt=\"{3}\"></div></td>",
                        numInfo * BAR_MAGNIFICATION, curTimestamp, numInfo, curTimestamp));
                // warn bar
                bottomRow.Append(
                    string.Format(
                        "<td><div class=\"graphBar warn\" style=\"height: {0}px;\" title=\"{1}: {2} warnings\" dt=\"{3}\"></div></td>",
                        numWarn * BAR_MAGNIFICATION, curTimestamp, numWarn, curTimestamp));
                // error bar
                bottomRow.Append(
                    string.Format(
                        "<td><div class=\"graphBar err\" style=\"height: {0}px;\" title=\"{1}: {2} errors\" dt=\"{3}\"></div></td>",
                        numErr * BAR_MAGNIFICATION, curTimestamp, numErr, curTimestamp));
                // unknown bar
                bottomRow.Append(
                    string.Format(
                        "<td><div class=\"graphBar unknown\" style=\"height: {0}px;\" title=\"{1}: {2} unrecognized log entries\" dt=\"{3}\"></div></td>",
                        numUnknown * BAR_MAGNIFICATION, curTimestamp, numUnknown, curTimestamp));
                bottomRow.Append("</tr></table></td>");
            }
            litTimelineTableContent.Text = string.Format("<tr>{0}</tr><tr>{1}</tr>", topRow.ToString(), bottomRow.ToString());
        }

        private static JArray GetCommentCountsForExceptions(List<LogEntry> entries)
        {
            var JSONyo = JsonConvert.SerializeObject((from LogEntry entry in entries select entry.DisplayString).Distinct());
            var JSONresp = string.Empty;

            try
            {
                var request = (HttpWebRequest) WebRequest.Create(SERVICE_URL + GET_COMMENTS_FOR_EXCEPTIONS_URL);
                request.Method = "POST";
                request.ContentType = "application/json; charset=utf-8";

                var writer = new StreamWriter(request.GetRequestStream());
                writer.Write(JSONyo);
                writer.Close();

                var response = request.GetResponse();
                var sr = new StreamReader(response.GetResponseStream());
                JSONresp = sr.ReadToEnd();
            }
            catch
            {
            }

            JsonSerializerSettings serializerSettings = new JsonSerializerSettings
                                                            {
                                                                TypeNameHandling = TypeNameHandling.All,
                                                                TypeNameAssemblyFormat =
                                                                    FormatterAssemblyStyle.Simple
                                                            };
            var whatsThis = JsonConvert.DeserializeObject(JSONresp, typeof(Newtonsoft.Json.Linq.JArray), serializerSettings) as Newtonsoft.Json.Linq.JArray;
            return whatsThis;
        }

        private static void SetCommentCounts(JArray whatsThis, List<IGrouping<string, LogEntry>> tmp)
        {
            if (whatsThis != null)
            {
                foreach (var log in tmp)
                {
                    var logComCnt = whatsThis.Where(x => x.Value<string>("Key") == log.Key);
                    var firstOrDefault = logComCnt.FirstOrDefault();
                    if (firstOrDefault != null)
                        log.First().CommentCount = firstOrDefault.Value<int>("Value");
                }
            }
        }

        protected void rptExceptions_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.DataItem == null) return;
            var dataItem = e.Item.DataItem as IGrouping<string, LogEntry>;
            if (dataItem == null || string.IsNullOrEmpty(dataItem.Key)) return;

            var litException = (HtmlGenericControl)e.Item.FindControl("litException");
            var litExceptionCommentCount = (HtmlGenericControl)e.Item.FindControl("litExceptionCommentCount");
            var lnkExceptionCount = (HtmlAnchor)e.Item.FindControl("lnkExceptionCount");

            litException.InnerHtml = dataItem.Key.Trim();
            var firstOrDefault = dataItem.FirstOrDefault();
            if (firstOrDefault != null)
                litExceptionCommentCount.InnerHtml = firstOrDefault.CommentCount.ToString();

            //// now we show comments on an exception / warning instead of the google search.
            //lnkException.HRef = "http://www.google.com?q=" + Server.UrlEncode(dataItem.Key);
            lnkExceptionCount.InnerText = dataItem.Count().ToString();
            if (dataItem.Count() > 0)
                lnkExceptionCount.HRef = "ExceptionViewer.aspx?ex=" + Server.UrlEncode(dataItem.ToList()[0].Message);
        }

        protected void btnStartOver_Click(object sender, EventArgs e)
        {
            if (Session[Constants.SESSION_KEY_LOG_ENTRIES] != null) 
                ((List<LogEntry>)Session[Constants.SESSION_KEY_LOG_ENTRIES]).Clear();
            if (Session[Constants.SESSION_KEY_LOG_FILES] != null)
                ((List<string>)Session[Constants.SESSION_KEY_LOG_FILES]).Clear();
        }

        protected void btnLocalLogs_Click(object sender, EventArgs e)
        {
            var asm = Assembly.Load("Sitecore.Kernel");
            string logFolder = string.Empty;
            btnLocalLogs.Visible = false;
            if (asm != null)
            {
                try
                {
                    var sc = asm.GetType("Sitecore.Configuration.Settings");
                    logFolder = sc.GetProperty("LogFolder").GetValue(null, new object[0]).ToString();
                    btnLocalLogs.Visible = Directory.Exists(logFolder);
                }
                catch (Exception ex)
                {
                    // doesn't matter
                }
            }
            if (string.IsNullOrEmpty(logFolder))
            {
                // uh oh, can't find the log folder
                // TODO: message the client
                return;
            }
            pnlLocalLogs.Visible = btnLocalLogs.Visible;
            btnLocalLogs.Visible = !pnlLocalLogs.Visible;
            if (pnlLocalLogs.Visible)
            {
                var sb = new StringBuilder();
                sb.Append("Local Log Files to Analyze:<br />");

                foreach (var file in Directory.GetFiles(logFolder))
                {
                    // make link for file
                    sb.Append(string.Format("<a href=\"Default.aspx?{0}={1}\">({2}) {1}</a><br />", Constants.QUERY_STRING_FILENAME, file, GetFileSize(file)));
                }
                litLocalLogsLinks.Text = sb.ToString();
            }

        }

        private static string GetFileSize(string file)
        {
            var fileInfo = new FileInfo(file);
            var len = fileInfo.Length;
            if (len > 1000000) return (len/1000000.0).ToString("n") + "MB";
            if (len > 1000) return (len / 1000.0).ToString("n") + "KB";
            return len + "bytes";
        }
    }
}