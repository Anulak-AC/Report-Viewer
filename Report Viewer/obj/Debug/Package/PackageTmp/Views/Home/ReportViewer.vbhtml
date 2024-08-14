<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ReportViewer.aspx.vb" Inherits="YourNamespace.ReportViewer" %>
<%@ Register Assembly="CrystalDecisions.Web, Version=13.0.2000.0, Culture=neutral, PublicKeyToken=692fbea5521e1304" TagPrefix="CRWeb" TagName="CrystalReportViewer" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Crystal Report Viewer</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <CRWeb:CrystalReportViewer ID="CrystalReportViewer1" runat="server" />
        </div>
    </form>
</body>
</html>
