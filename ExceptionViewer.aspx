<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ExceptionViewer.aspx.cs"
    Inherits="LogAnalyzer.ExceptionViewer" MasterPageFile="Site.Master" %>

<asp:Content runat="server" ContentPlaceHolderID="MainContent">
    <h2>
        Log Entries 
        <asp:Literal ID="litExceptionType" runat="server" /></h2>
    <a href="Default.aspx">Back to Analyzer</a>
    <asp:Repeater runat="server" ID="rptExceptions" 
        onitemdatabound="rptExceptions_ItemDataBound">
        <ItemTemplate>
            <div class="exception">
                <asp:Literal ID="litException" runat="server"></asp:Literal>
            </div>
        </ItemTemplate>
    </asp:Repeater>
    <a href="Default.aspx">Back to Analyzer</a>
</asp:Content>
