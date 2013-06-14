<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/UsageDataAnalysis.Master" Inherits="System.Web.Mvc.ViewPage<UsageDataAnalysisWebClient.Models.EnvironmentViewModel>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Environment
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <% using (Html.BeginForm()) { %>
        Show data from <%: Html.EditorFor(model => model.StartDate)%> to <%: Html.EditorFor(model => model.EndDate)%>.
        <input type="submit" value="Go" />
    <% } %>
    <%
        foreach (var chart in Model.Charts) {
            %>
                <img src="<%: Url.Action("Chart", new { startDate = Model.StartDate, endDate = Model.EndDate, title = chart.Title, id = chart.Id }) %>"
                   alt="<%: chart.Title %>" width="<%: chart.Width %>" height="<%: chart.Height %>" />
                <br />
            <%
        }
   %>
</asp:Content>
