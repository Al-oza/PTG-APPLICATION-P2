<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="PTGApplication.Views.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Tantalum Health</title>
</head>
<body>
    <nav>
        <a href="Default.aspx">Home</a>
        <a href="Inventory.aspx">Inventory</a>
        <a href="Reports.aspx">Reports</a>
        <a href="About.aspx">About</a>
    </nav>
    <footer>&copy; <%= DateTime.Now.Year %> Tantalum Health</footer>
</body>
</html>
