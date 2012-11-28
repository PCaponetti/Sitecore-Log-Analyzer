using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using LogAnalyzer.ModelClasses;

namespace LogAnalyzer
{
    public partial class ExceptionViewer : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string exception = Request.QueryString[Constants.QUERY_STRING_EXCEPTION];
            string date = Request.QueryString[Constants.QUERY_STRING_DATE];
            List<LogEntry> entries = Session[Constants.SESSION_KEY_LOG_ENTRIES] as List<LogEntry>;
            if (!string.IsNullOrEmpty(exception) && entries != null && string.IsNullOrEmpty(date))
            {
                litExceptionType.Text = " of type " + exception;
                rptExceptions.DataSource = entries.Where(x => x.Message == exception).ToList();
                rptExceptions.DataBind();
            }
            if(!string.IsNullOrEmpty(date) && entries != null)
            {
                litExceptionType.Text = " from " + date;
                rptExceptions.DataSource = entries.Where(x => x.TimeStamp.ToString(Constants.DATETIME_FORMAT) == date).ToList();
                rptExceptions.DataBind();
            }
        }

        protected void rptExceptions_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.DataItem == null) return;
            var dataItem = e.Item.DataItem as LogEntry;
            Literal litException = e.Item.FindControl("litException") as Literal;
            litException.Text = dataItem.TimeStamp + "<br />" + dataItem.FullLogEntry;
        }
    }
}