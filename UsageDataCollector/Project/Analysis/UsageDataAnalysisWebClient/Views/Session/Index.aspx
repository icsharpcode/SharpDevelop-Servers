<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/UsageDataAnalysis.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<UsageDataAnalysisWebClient.Models.Session>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Index
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Index</h2>

    <table>
        <tr>
            <th></th>
            <th>
                SessionId
            </th>
            <th>
                ClientSessionId
            </th>
            <th>
                StartTime
            </th>
            <th>
                EndTime
            </th>
            <th>
                UserId
            </th>
        </tr>

    <% foreach (var item in Model) { %>
    
        <tr>
            <td>
                <%--<%= Html.ActionLink("Edit", "Edit", new { id=item.SessionId }) %> |
                <%= Html.ActionLink("Details", "Details", new { id=item.SessionId })%>--%>
            </td>
            <td>
                <%= Html.Encode(item.SessionId) %>
            </td>
            <td>
                <%= Html.Encode(item.ClientSessionId) %>
            </td>
            <td>
                <%= Html.Encode(String.Format("{0:g}", item.StartTime)) %>
            </td>
            <td>
                <%= Html.Encode(String.Format("{0:g}", item.EndTime)) %>
            </td>
            <td>
                <%= Html.Encode(item.UserId) %>
            </td>
        </tr>
    
    <% } %>

    </table>

    <%--<p>
        <%= Html.ActionLink("Create New", "Create") %>
    </p>--%>

</asp:Content>

