<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/UsageDataAnalysis.Master" Inherits="System.Web.Mvc.ViewPage<UsageDataAnalysisWebClient.Models.UsageViewModel>" %>

<%@ Register Assembly="System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
    Namespace="System.Web.UI.DataVisualization.Charting" TagPrefix="asp" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Usage
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <% using (Html.BeginForm()) { %>
        Show data from <%: Html.EditorFor(model => model.StartDate)%> to <%: Html.EditorFor(model => model.EndDate)%>.
        <input type="submit" value="Go" />
    <% } %>
    <h2>Number of users per day</h2>
    <%
        Model.DailyUsers.Page = this;
        Model.DailyUsers.RenderControl(new HtmlTextWriter(Page.Response.Output));  
   %>
    <h2>Number of users per week</h2>
    <%
        Model.WeeklyUsers.Page = this;
        Model.WeeklyUsers.RenderControl(new HtmlTextWriter(Page.Response.Output));  
   %>
    <h2>Number of users per month</h2>
    <%
        Model.MonthlyUsers.Page = this;
        Model.MonthlyUsers.RenderControl(new HtmlTextWriter(Page.Response.Output));  
   %>
</asp:Content>

