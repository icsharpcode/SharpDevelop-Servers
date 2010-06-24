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
            chart.Width = 800;
            chart.Height = 300;
            chart.RenderType = System.Web.UI.DataVisualization.Charting.RenderType.ImageTag;

            chart.ChartAreas.Add("Series 1");
            chart.Legends.Add("Legend 1");

            // Render chart control  
            chart.Page = this;
            chart.RenderControl(new HtmlTextWriter(Page.Response.Output));
        }
   %>
</asp:Content>
