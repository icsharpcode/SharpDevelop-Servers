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
                <%= Html.ActionLink("Edit", "Edit", new { id=item.ExceptionGroupId }) %> |
                <%= Html.ActionLink("Details", "Details", new { id=item.ExceptionGroupId })%>
            </td>
            <td>
                <%= Html.Encode(item.ExceptionGroupId) %>
            </td>
            <td>
                <%= Html.Encode(item.TypeFingerprintSha256Hash) %>
            </td>
            <td>
                <%= Html.Encode(item.ExceptionType) %>
            </td>
            <td>
                <%= Html.Encode(item.ExceptionFingerprint) %>
            </td>
            <td>
                <%= Html.Encode(item.ExceptionLocation) %>
            </td>
            <td>
                <%= Html.Encode(item.UserComment) %>
            </td>
            <td>
                <%= Html.Encode(item.UserFixedInRevision) %>
            </td>
        </tr>
    
    <% } %>

    </table>

    <p>
        <%= Html.ActionLink("Create New", "Create") %>
    </p>

</asp:Content>

