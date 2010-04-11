<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/UsageDataAnalysis.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<UsageDataAnalysisWebClient.Models.ExceptionGroup>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Index
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Index</h2>

    <table>
        <tr>
            <th></th>
            <th>
                ExceptionGroupId
            </th>
            <th>
                TypeFingerprintSha256Hash
            </th>
            <th>
                ExceptionType
            </th>
            <th>
                ExceptionFingerprint
            </th>
            <th>
                ExceptionLocation
            </th>
            <th>
                UserComment
            </th>
            <th>
                UserFixedInRevision
            </th>
        </tr>

    <% foreach (var item in Model) { %>
    
        <tr>
            <td>
                <%: Html.ActionLink("Edit", "Edit", new { id=item.ExceptionGroupId }) %> |
                <%: Html.ActionLink("Details", "Details", new { id=item.ExceptionGroupId })%> |
                <%: Html.ActionLink("Delete", "Delete", new { id=item.ExceptionGroupId })%>
            </td>
            <td>
                <%: item.ExceptionGroupId %>
            </td>
            <td>
                <%: item.TypeFingerprintSha256Hash %>
            </td>
            <td>
                <%: item.ExceptionType %>
            </td>
            <td>
                <%: item.ExceptionFingerprint %>
            </td>
            <td>
                <%: item.ExceptionLocation %>
            </td>
            <td>
                <%: item.UserComment %>
            </td>
            <td>
                <%: item.UserFixedInRevision %>
            </td>
        </tr>
    
    <% } %>

    </table>

    <p>
        <%: Html.ActionLink("Create New", "Create") %>
    </p>

</asp:Content>

