<%@ Page Language="C#" 
    MasterPageFile="~/Build.Master" 
    AutoEventWireup="true" 
    CodeBehind="Default.aspx.cs"
    Inherits="ArtefactsSite.Default" 
    Title="SharpDevelop Build Server - Build Artefacts" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="PageContent" runat="server">
    
    <div>
        <h1>SharpDevelop "Mirador" 4.0</h1>
        <asp:ObjectDataSource ID="miradorArtefactsSource" runat="server" SelectMethod="GetFileListing"
            TypeName="DownloadInformationComponent" SortParameterName="sortExpression">
            <SelectParameters>
                <asp:Parameter DefaultValue="SharpDevelop_4*" Name="artefactQuery" Type="String" />
            </SelectParameters>
        </asp:ObjectDataSource>
    </div>
    
    <asp:GridView ID="miradorArtefactsGrid" runat="server" AutoGenerateColumns="False"
        CellPadding="4" DataSourceID="miradorArtefactsSource" ForeColor="#333333" GridLines="None"
        AllowSorting="True">
        <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
        <Columns>
            <asp:BoundField DataField="FileName" HeaderText="File name" SortExpression="FileName" />
            <asp:BoundField DataField="CreationDate" HeaderText="Build time" SortExpression="CreationDate" />
            <asp:HyperLinkField DataNavigateUrlFields="FileName" DataNavigateUrlFormatString="{0}"
                Text="Download" />
        </Columns>
        <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
        <EditRowStyle BackColor="#999999" />
        <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
        <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
        <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
        <AlternatingRowStyle BackColor="White" ForeColor="#284775" />
    </asp:GridView>
    
    <br />
	
	<div>
        <h1>Mirador Reports Branch</h1>
        <asp:ObjectDataSource ID="miradorreportsArtefactsSource" runat="server" SelectMethod="GetFileListing"
            TypeName="DownloadInformationComponent" SortParameterName="sortExpression">
            <SelectParameters>
                <asp:Parameter DefaultValue="mirador-reports*" Name="artefactQuery" Type="String" />
            </SelectParameters>
        </asp:ObjectDataSource>
    </div>
    
    <asp:GridView ID="miradorreportsArtefactsGrid" runat="server" AutoGenerateColumns="False"
        CellPadding="4" DataSourceID="miradorreportsArtefactsSource" ForeColor="#333333"
        GridLines="None" AllowSorting="True">
        <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
        <Columns>
            <asp:BoundField DataField="FileName" HeaderText="File name" SortExpression="FileName" />
            <asp:BoundField DataField="CreationDate" HeaderText="Build time" SortExpression="CreationDate" />
            <asp:HyperLinkField DataNavigateUrlFields="FileName" DataNavigateUrlFormatString="{0}"
                Text="Download" />
        </Columns>
        <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
        <EditRowStyle BackColor="#999999" />
        <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
        <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
        <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
        <AlternatingRowStyle BackColor="White" ForeColor="#284775" />
    </asp:GridView>
    
    <br />
	
	    <div>
        <h1>SharpDevelop 3.x</h1>
        <asp:ObjectDataSource ID="montferrerArtefactsSource" runat="server" SelectMethod="GetFileListing"
            TypeName="DownloadInformationComponent" SortParameterName="sortExpression">
            <SelectParameters>
                <asp:Parameter DefaultValue="SharpDevelop_3*" Name="artefactQuery" Type="String" />
            </SelectParameters>
        </asp:ObjectDataSource>
    </div>
    
    <asp:GridView ID="montferrerArtefactsGrid" runat="server" AutoGenerateColumns="False"
        CellPadding="4" DataSourceID="montferrerArtefactsSource" ForeColor="#333333"
        GridLines="None" AllowSorting="True">
        <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
        <Columns>
            <asp:BoundField DataField="FileName" HeaderText="File name" SortExpression="FileName" />
            <asp:BoundField DataField="CreationDate" HeaderText="Build time" SortExpression="CreationDate" />
            <asp:HyperLinkField DataNavigateUrlFields="FileName" DataNavigateUrlFormatString="{0}"
                Text="Download" />
        </Columns>
        <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
        <EditRowStyle BackColor="#999999" />
        <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
        <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
        <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
        <AlternatingRowStyle BackColor="White" ForeColor="#284775" />
    </asp:GridView>
    
    <br />
    
    <asp:Label ID="renderInformationLabel" runat="server" Text="Label"></asp:Label>
    
</asp:Content>
