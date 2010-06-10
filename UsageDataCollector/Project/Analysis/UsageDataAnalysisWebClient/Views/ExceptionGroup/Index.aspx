<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/UsageDataAnalysis.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<UsageDataAnalysisWebClient.Models.ExceptionGroupIndexModel>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Index
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Index</h2>

    <table>
        <tr>
            <th>ID</th>
            <th>Type</th>
            <th>Location</th>
            <th>Users affected</th>
            <th>Occurrences</th>
            <th>First Seen</th>
            <th>Last Seen</th>
            <th>Comment</th>
        </tr>

    <% foreach (var item in Model) { %>
    
        <tr class='<%: item.UserFixedInRevision != null ? (item.UserFixedInRevision > item.LastSeenRevision ? "goodRow" : "badRow") : "normalRow" %>'>
           <%--  <td>
                <%: Html.ActionLink("Details", "Details", new { id=item.ExceptionGroupId })%> |
                <%: Html.ActionLink("Delete", "Delete", new { id=item.ExceptionGroupId })%>
            </td> --%>
            <td>
                <%: Html.ActionLink(item.ExceptionGroupId.ToString(), "Edit", new { id = item.ExceptionGroupId })%>
            </td>
            <td class="exceptionType">
                <%: item.ShortExceptionType %>
            </td>
            <td class="exceptionLocation">
                <%: item.ExceptionLocation %>
            </td>
            <td>
                <%: item.AffectedUsers %>
            </td>
            <td>
                <%: item.Occurrences %>
            </td>
            <td>
                <%: item.FirstSeenVersion %>
            </td>
            <td>
                <%: item.LastSeenVersion %>
            </td>
            <td>
                <%: item.RichUserComment %>
                <% if (item.UserFixedInRevision != null)
                   { %>
                   fixed in <a href="http://fisheye2.atlassian.com/changelog/sharpdevelop/?cs=<%: item.UserFixedInRevision %>">r<%: item.UserFixedInRevision %></a>
                <% } %>
            </td>
        </tr>
    
    <% } %>

    </table>

   <%-- <p>
        <%: Html.ActionLink("Create New", "Create") %>
    </p>--%>

</asp:Content>

