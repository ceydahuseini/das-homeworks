<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebApplication2._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main>
        <section class="row">
            <h1>Stock Data Viewer</h1>
            <p class="lead">Retrieve and display stock data dynamically from the Macedonian Stock Exchange.</p>
        </section>

        <div class="row">
            <section class="col-md-12">
                <asp:Label ID="Label1" runat="server" CssClass="text-danger"></asp:Label>
                <asp:DropDownList ID="IssuerDropDown" runat="server" CssClass="form-control">
                </asp:DropDownList>

                
                <div class="form-group">
                    <label for="StartDate">Start Date:</label>
                    <asp:TextBox ID="StartDatePicker" runat="server" CssClass="form-control" placeholder="04.11.2024"></asp:TextBox>
                </div>

                
                <div class="form-group">
                    <label for="EndDate">End Date:</label>
                    <asp:TextBox ID="EndDatePicker" runat="server" CssClass="form-control" placeholder="04.11.2024"></asp:TextBox>
                </div>

                <asp:Button ID="ShowButton" runat="server" Text="Show" CssClass="btn btn-primary" OnClick="ShowButton_Click" />

                <asp:GridView ID="GridView1" runat="server" CssClass="table table-striped table-bordered"></asp:GridView>

                
                <asp:Button ID="ExportButton" runat="server" Text="Export to CSV" CssClass="btn btn-success" OnClick="ExportButton_Click" />
            </section>
        </div>
    </main>

    
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap-datepicker/dist/js/bootstrap-datepicker.min.js"></script>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-datepicker/dist/css/bootstrap-datepicker.min.css" rel="stylesheet" />

    <script>
        $(document).ready(function () {

            $('#<%= StartDatePicker.ClientID %>').datepicker({
                format: 'dd.mm.yyyy',
                autoclose: true
            });

            $('#<%= EndDatePicker.ClientID %>').datepicker({
                format: 'dd.mm.yyyy',
                autoclose: true
            });
        });
    </script>
</asp:Content>