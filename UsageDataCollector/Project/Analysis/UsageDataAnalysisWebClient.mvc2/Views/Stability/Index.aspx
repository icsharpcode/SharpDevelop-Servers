<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/UsageDataAnalysis.Master" Inherits="System.Web.Mvc.ViewPage<System.Web.UI.DataVisualization.Charting.Chart>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Stability
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Instability</h2>
	Probability that at least one exception/unclean exit will occur in a given day for a given user<br/>
     <%
        var chart = Model;
        chart.Width = 813;
        chart.Height = 300;
        chart.RenderType = System.Web.UI.DataVisualization.Charting.RenderType.ImageTag;

        chart.ChartAreas.Add("Series 1").AxisX.Interval = 1;
        chart.Legends.Add("Legend 1");

        // Render chart control  
        chart.Page = this;
        chart.RenderControl(new HtmlTextWriter(Page.Response.Output));
   %>

   <h2>Crash Frequency</h2>
   Average number of crashes per hour<br />
   <%
       var chart2 = (System.Web.UI.DataVisualization.Charting.Chart)ViewData["CrashFrequency"];
       chart2.Width = 600;
       chart2.Height = 300;
       chart2.RenderType = System.Web.UI.DataVisualization.Charting.RenderType.ImageTag;

       chart2.ChartAreas.Add("Series 1").AxisX.Interval = 1;

       // Render chart control  
       chart2.Page = this;
       chart2.RenderControl(new HtmlTextWriter(Page.Response.Output));
   %>
</asp:Content>
