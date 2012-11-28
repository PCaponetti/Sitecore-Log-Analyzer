<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="LogAnalyzer.Default"
    MasterPageFile="Site.Master" %>

<asp:Content runat="server" ContentPlaceHolderID="MainContent">
    <div id="pnlUploadFile" runat="server">
        <h2>
            Upload a log file</h2>
        <input type="file" runat="server" id="fileLogFile" />&nbsp;
        <asp:Button ID="btnAnalyze" runat="server" Text="Analyze" OnClick="btnAnalyze_Click" /> 
        <asp:Button ID="btnLocalLogs" runat="server" Text="Or Analyze Local Logs" 
            onclick="btnLocalLogs_Click" /> 
        <asp:Button ID="btnStartOver" runat="server" Text="Or Start Over" 
            onclick="btnStartOver_Click" /><br />
        <div id="pnlLocalLogs" runat="server" visible="false">
            <asp:Literal id="litLocalLogsLinks" runat="server" />
        </div>
        <asp:Literal runat="server" ID="litFiles"></asp:Literal>
    </div>
    <div id="pnlTimelineSlider" class="pnlTimelineSlider" runat="server" visible="false">
        <h2>
            Timeline</h2>
        <div id="pnlTimelineSliderContent" class="pnlTimelineSliderContent">
            <table border="0" cellpadding="0" cellspacing="0">
                <asp:Literal runat="server" ID="litTimelineTableContent"></asp:Literal>
            </table>
        </div>
    </div>
    <asp:Repeater runat="server" ID="rptExceptions" OnItemDataBound="rptExceptions_ItemDataBound">
        <HeaderTemplate>
            <div id="pnlExceptions">
                <div id="pnlExceptionsContent">
                    <h2>
                        Exceptions</h2>
                    <table border="0" cellpadding="0" cellspacing="0">
                        <tr>
                            <th>
                                Exception
                            </th>
                            <th>
                                Count
                            </th>
                            <th>&nbsp;</th>
        </HeaderTemplate>
        <ItemTemplate>
            </tr>
            <td class="tdException">
                <a class="lnkException" title="Click here to learn more about this type of exception">
                    <span runat="server" id="litExceptionCommentCount" class="litExceptionCommentCount"></span>
                    <span runat="server" id="litException" class="litException"></span>
                </a>
                <div class="commentsForException">
                </div>
            </td>
            <td>
                <a id="lnkExceptionCount" runat="server" href="#" title="Click here to see all of these expceptions"></a>
            </td>
            <td>
                <a class="postComment" href="#">Post Comment</a>
            </td>
            </tr>
        </ItemTemplate>
        <FooterTemplate>
            </table> </div> </div>
        </FooterTemplate>
    </asp:Repeater>
    <asp:Repeater runat="server" ID="rptWarnings" OnItemDataBound="rptExceptions_ItemDataBound">
        <HeaderTemplate>
            <div id="pnlWarnings">
                <div id="pnlWarningsContent">
                    <h2>
                        Warnings</h2>
                    <table border="0" cellpadding="0" cellspacing="0">
                        <tr>
                            <th>
                                Warning
                            </th>
                            <th>
                                Count
                            </th>
                            <th>&nbsp;</th>
        </HeaderTemplate>
        <ItemTemplate>
            </tr>
            <td class="tdException">
                <a class="lnkException" title="Click here to learn more about this type of warning">
                    <span runat="server" id="litExceptionCommentCount" class="litExceptionCommentCount"></span>
                    <span runat="server" id="litException" class="litException"></span>
                </a>
                <div class="commentsForException">
                </div>
            </td>
            <td>
                <a id="lnkExceptionCount" runat="server" href="#" title="Click here to see all 400 of these expceptions"></a>
            </td>
            <td>
                <a class="postComment" href="#">Post Comment</a>
            </td>
            </tr>
        </ItemTemplate>
        <FooterTemplate>
            </table> </div> </div>
        </FooterTemplate>
    </asp:Repeater>
</asp:Content>
