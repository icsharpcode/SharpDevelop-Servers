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
        System.Web.UI.DataVisualization.Charting.Chart Chart2 = new System.Web.UI.DataVisualization.Charting.Chart();  
        Chart2.Width = 800;  
        Chart2.Height = 300;  
        Chart2.RenderType = RenderType.ImageTag;  

        Chart2.Palette = ChartColorPalette.BrightPastel;  
        Chart2.ChartAreas.Add("Series 1");
        var s = Chart2.Series.Add("Number of users");
        s.ChartType = SeriesChartType.Line;
        foreach (var value in Model.DiagramData)  
        {  
            s.Points.AddXY(value.Date, value.UserCount);
        }

        // Render chart control  
        Chart2.Page = this;  
        Chart2.RenderControl(new HtmlTextWriter(Page.Response.Output));  
 
   %>
</asp:Content>

