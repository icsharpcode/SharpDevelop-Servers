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
                <%: Html.LabelFor(model => model.ExceptionGroupId) %>
				<%: Html.HiddenFor(model => model.ExceptionGroupId) %>
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
                <%: Html.LabelFor(model => model.ExceptionType) %>
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
                First Occurrence:
            </div>
            <div class="editor-field">
                <a href="https://github.com/icsharpcode/SharpDevelop/commits/<%: Model.FirstOccurrenceCommitHash %>"><%: Model.FirstOccurrenceCommit %></a>
            </div>

            <div class="editor-label">
                Last Occurrence:
            </div>
            <div class="editor-field">
                <%: Model.LastOccurrenceCommit %>
            </div>

            <div class="editor-label">
                <%= Html.LabelFor(model => model.ExceptionFingerprint) %>
            </div>
            <pre>
                <%: Model.ExceptionFingerprint %>
            </pre>
            
            <div class="editor-label">
                <%: Html.LabelFor(model => model.UserComment) %>
            </div>
            <div class="editor-field">
                <%: Html.TextBoxFor(model => model.UserComment) %>
                <%: Html.ValidationMessageFor(model => model.UserComment) %>
            </div>
            
            <% if (Model.UserFixedInCommit != null) { %>
            <div class="editor-label">
                Fixed in Version:
            </div>
            <div class="editor-field">
                <a href="https://github.com/icsharpcode/SharpDevelop/commit/<%: Model.UserFixedInCommitHash %>"><%: Model.UserFixedInCommit%></a>
            </div>
            <% } %>

            <div class="editor-label">
                <%: Html.LabelFor(model => model.UserFixedInCommitHash) %>
            </div>
            <div class="editor-field">
                <%: Html.TextBoxFor(model => model.UserFixedInCommitHash) %>
                <%: Html.ValidationMessageFor(model => model.UserFixedInCommit) %>
            </div>
            
            <p>
                <input type="submit" value="Save" />
            </p>
        </fieldset>

    <% } %>
    
    <div>
        <%:Html.ActionLink("Back to List", "Index") %>
    </div>

    <%
        if (Model.CrashProbabilities.Count > 1) {
           %>
           <img src="<%: Url.Action("CrashProbabilityChart", new { Id = Model.ExceptionGroupId }) %>" width="800" height="300" />
           <%
        } else if (Model.CrashProbabilities.Count == 1) {
            %>
            <p>This exception occurred in only one release (<%: Model.CrashProbabilities[0].Item1%>).
            The instability in that release caused by this exception is <%: Model.CrashProbabilities[0].Item2.ToString("f2")%>%.</p>
            <%
        } else {
            %><p>This exception did not occur in any tagged release versions.</p><%
        }
   %>

    <div>
    <%
        foreach (var instance in Model.Exceptions)
        { %>
            <p>&nbsp;</p>
            <hr />
            <p>&nbsp;</p>

            <table>
            <tr>
                <th>Date</th>
                <td><%: instance.ThrownAt%></td>
            </tr>
            <tr>
                <th>UserID</th>
                <td><%: instance.UserId%></td>
            </tr>
            <% 
                foreach (var environmentData in instance.Environment.OrderBy(e => e.Name))
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
            <p>Previous feature uses:</p>
            <table>
            <tr><th>Date</th><th>Name</th><th>Activation Method</th></tr>
            <%
                foreach (var featureUse in instance.PreviousFeatureUses) {
                    %>
                    <tr>
                        <td><%: featureUse.UseTime %></td>
                        <td><%: featureUse.FeatureName %></td>
                        <td><%: featureUse.ActivationMethod %></td>
                    </tr>
                    <%
                }
            %>
            </table>
    <% } %>
    </div>

    <div>
        <%=Html.ActionLink("Back to List", "Index") %>
    </div>

</asp:Content>

