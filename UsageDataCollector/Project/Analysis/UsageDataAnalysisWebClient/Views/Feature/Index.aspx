<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/UsageDataAnalysis.Master" Inherits="System.Web.Mvc.ViewPage<UsageDataAnalysisWebClient.Models.FeatureIndexModel>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Features
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Features</h2>

    <% using (Html.BeginForm(null, null, FormMethod.Get)) { %>
    Feature Name: <%: Html.TextBoxFor(model => model.FeatureFilter)%><br />
    Version: <%: Html.TextBoxFor(model => model.VersionFilter)%><br />
        <input type="submit" value="Go" />
    <% } %>

    <% if (Model.ErrorMessage != null) { %>
    <p style="color: red"><%: Model.ErrorMessage%></p>
    <% } %>

    <% if (Model.Entries != null) { %>
    <table>
        <tr>
            <th>Feature</th>
            <th>Use Count</th>
            <th>Userdays</th>
        </tr>
        <% foreach (var entry in Model.Entries) { %>
        <tr>
            <td><%: entry.FeatureName %></td>
            <td><%: entry.TotalUseCount %></td>
            <td><%: entry.UserDays %></td>
        </tr>
        <% } %>
    </table>
    <% } %>


</asp:Content>
