using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LogAnalyzer
{
    public partial class SiteMaster : System.Web.UI.MasterPage
    {
        public string VoterID { get; set; }
        protected void Page_Load(object sender, EventArgs e)
        {
            VoterID = Request.UserHostAddress;
        }
    }
}
