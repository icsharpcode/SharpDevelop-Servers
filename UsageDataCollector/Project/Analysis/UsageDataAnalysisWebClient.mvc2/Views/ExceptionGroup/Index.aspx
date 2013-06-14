<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/UsageDataAnalysis.Master" Inherits="System.Web.Mvc.ViewPage<UsageDataAnalysisWebClient.Models.ExceptionGroupIndexModel>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Index
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Index</h2>

    <% using (Html.BeginForm(null, null, FormMethod.Get)) { %>
        Show exceptions from <%: Html.TextBoxFor(model => model.StartCommitHash) %>
        to <%: Html.TextBoxFor(model => model.EndCommitHash) %>
        <input type="submit" value="Go" />
    <% } %>

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

    <% foreach (var item in Model.Entries) { %>
    
        <tr class='<%: item.UserFixedInCommitId != null ? (item.HasRepeatedAfterFixVersion ? "badRow" : "goodRow") : "normalRow" %>'>
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
                <a href="https://github.com/icsharpcode/SharpDevelop/commits/<%: item.FirstSeenVersionHash %>"><%: item.FirstSeenVersion %></a>
            </td>
            <td>
                <%: item.LastSeenVersion %>
            </td>
            <td>
                <%: item.RichUserComment %>
                <% if (item.UserFixedInCommitHash != null)
                   { %>
                   fixed in <a href="https://github.com/icsharpcode/SharpDevelop/commit/<%: item.UserFixedInCommitHash %>"><%: item.UserFixedInCommit %></a>
                <% } %>
            </td>
        </tr>
    
    <% } %>

    </table>

   <%-- <p>
        <%: Html.ActionLink("Create New", "Create") %>
    </p>--%>

</asp:Content>

