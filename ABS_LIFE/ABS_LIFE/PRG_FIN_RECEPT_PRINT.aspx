﻿<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="PRG_FIN_RECEPT_PRINT.aspx.vb" Inherits="ABS_LIFE.PRG_FIN_RECEPT_PRINT" %>

<%@ Register assembly="CrystalDecisions.Web, Version=10.5.3700.0, Culture=neutral, PublicKeyToken=692fbea5521e1304" namespace="CrystalDecisions.Web" tagprefix="CR" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
    </div>
    <CR:CrystalReportViewer ID="CrystalReportViewer1" runat="server" 
        AutoDataBind="true" ReportSourceID="CrystalReportSource1" 
        ReuseParameterValuesOnRefresh="True" />
    <CR:CrystalReportSource ID="CrystalReportSource1" runat="server">
        <Report FileName="LifeReceipt.rpt">
        </Report>
    </CR:CrystalReportSource>
    </form>
</body>
</html>
