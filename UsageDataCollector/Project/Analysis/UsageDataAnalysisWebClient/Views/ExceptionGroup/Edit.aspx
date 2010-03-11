<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/UsageDataAnalysis.Master" Inherits="System.Web.Mvc.ViewPage<UsageDataAnalysisWebClient.Models.ExceptionGroup>" %>

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
            </div>
            <div class="editor-field">
                <%= Html.Label(Model.ExceptionGroupId.ToString()) %>
            </div>
            
            <div class="editor-label">
                <%= Html.LabelFor(model => model.TypeFingerprintSha256Hash) %>
            </div>
            <div class="editor-field">
                <%= Html.Label(Model.TypeFingerprintSha256Hash) %>
model.TypeFingerprintSha256Hash) %>
            </div>
            
            <div class="editor-label">
                <%= Html.LabelFor(model => model.ExceptionType) %>
            </div>
            <div class="editor-field">
                <%=Html.Label(Model.ExceptionType) %>
            </div>
            
            <div class="editor-label">
                <%= Html.LabelFor(model => model.ExceptionFingerprint) %>
            </div>
            <div class="editor-field">
                <%=Html.Label(Model.ExceptionFingerprint) %>
            </div>
            
            <div class="editor-label">
                <%= Html.LabelFor(model => model.ExceptionLocation) %>
            </div>
            <div class="editor-field">
                <%= Html.Label(Model.ExceptionLocation) %>
            </div>
            
            <div class="editor-label">
                <%= Html.LabelFor(model => model.UserComment) %>
            </div>
            <div class="editor-field">
                <%= Html.TextBoxFor(model => model.UserComment) %>
                <%= Html.ValidationMessageFor(model => model.UserComment) %>
            </div>
            
            <div class="editor-label">
                <%= Html.LabelFor(model => model.UserFixedInRevision) %>
            </div>
            <div class="editor-field">
                <%= Html.TextBoxFor(model => model.UserFixedInRevision) %>
                <%= Html.ValidationMessageFor(model => model.UserFixedInRevision) %>
            </div>
            
            <p>
                <input type="submit" value="Save" />
            </p>
        </fieldset>

    <% } %>

    <div>
        <%=Html.ActionLink("Back to List", "Index") %>
    </div>

</asp:Content>

