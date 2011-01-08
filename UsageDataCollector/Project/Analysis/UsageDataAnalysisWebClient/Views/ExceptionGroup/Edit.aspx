<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/UsageDataAnalysis.Master" Inherits="System.Web.Mvc.ViewPage<UsageDataAnalysisWebClient.Models.ExceptionGroupEditModel>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Edit
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Edit</h2>

    <% using (Html.BeginForm()) {%>

        <fieldset>
            <legend>Fields</legend>
            
            <div class="editor-label">
                <%= Html.LabelFor(model => model.ExceptionGroupId) %>
				<%=Html.HiddenFor(model => model.ExceptionGroupId) %>
            </div>
            <div class="editor-field">
                <%: Model.ExceptionGroupId %>
            </div>
            
            <div class="editor-label">
                <%= Html.LabelFor(model => model.TypeFingerprintSha256Hash) %>
            </div>
            <div class="editor-field">
                <%: Model.TypeFingerprintSha256Hash %>
            </div>
            
            <div class="editor-label">
                <%= Html.LabelFor(model => model.ExceptionType) %>
            </div>
            <div class="editor-field">
                <%: Model.ExceptionType %>
            </div>
           
            <div class="editor-label">
                <%= Html.LabelFor(model => model.ExceptionLocation) %>
            </div>
            <div class="editor-field">
                <%: Model.ExceptionLocation %>
            </div>
             
            <div class="editor-label">
                <%= Html.LabelFor(model => model.ExceptionFingerprint) %>
            </div>
            <pre>
                <%: Model.ExceptionFingerprint %>
            </pre>
            
            <div class="editor-label">
                <%= Html.LabelFor(model => model.UserComment) %>
            </div>
            <div class="editor-field">
                <%= Html.TextBoxFor(model => model.UserComment) %>
                <%= Html.ValidationMessageFor(model => model.UserComment) %>
            </div>
            
            <div class="editor-label">
                <%= Html.LabelFor(model => model.UserFixedInCommit) %>
            </div>
            <div class="editor-field">
                <%= Html.TextBoxFor(model => model.UserFixedInCommit) %>
                <%= Html.ValidationMessageFor(model => model.UserFixedInCommit) %>
            </div>
            
            <p>
                <input type="submit" value="Save" />
            </p>
        </fieldset>

    <% } %>
    
    <div>
        <%=Html.ActionLink("Back to List", "Index") %>
    </div>

    <div>
    <%
        var instancesQuery = from ex in Model.Exceptions
                             where ex.IsFirstInSession
                             orderby ex.ThrownAt descending
                             select ex;
        foreach (var instance in instancesQuery.Take(10))
        { %>
            <hr />
            <table>
            <tr>
                <th>Date</th>
                <td><%: instance.ThrownAt%></td>
            </tr>
            <tr>
                <th>UserID</th>
                <td><%: instance.Session.UserId%></td>
            </tr>
            <% 
                var environmentDataQuery = from env in instance.Session.EnvironmentDatas
                                           select new
                                           {
                                               Name = env.EnvironmentDataName,
                                               Value = env.EnvironmentDataValue
                                           };
                foreach (var environmentData in environmentDataQuery.OrderBy(e => e.Name))
                {
                    %>
                    <tr>
                        <th><%: environmentData.Name %></th>
                        <td><%: environmentData.Value %></td>
                    </tr>
                    <%
                }
            %>
            </table>
            <pre><%: instance.Stacktrace %></pre>
    <% } %>
    </div>

    <div>
        <%=Html.ActionLink("Back to List", "Index") %>
    </div>

</asp:Content>

