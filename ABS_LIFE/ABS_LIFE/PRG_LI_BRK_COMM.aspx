﻿<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="PRG_LI_BRK_COMM.aspx.vb" Inherits="ABS_LIFE.PRG_LI_BRK_COMM" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
<script language="JavaScript" src="calendar_eu.js" type="text/javascript"></script>
	<link rel="stylesheet" href="calendar.css" />
    <link href="css/general.css" rel="stylesheet" type="text/css" />   
    <link href="css/grid.css" rel="stylesheet" type="text/css" />   
    <link href="css/rounded.css" rel="stylesheet" type="text/css" />   
    <title></title>
    
    
</head>
<body>
 <asp:Label runat=server ID=lblError Font-Bold=true ForeColor=Red Visible=false> </asp:Label>
    <form id="PRG_LI_BRK_COMM" runat="server">
<div>
     <div class="grid">
            <div class="rounded">
                <div class="top-outer"><div class="top-inner"><div class="top">
                    <h2>Brokers Commission Rates</h2>
                </div></div></div>
                <div class="mid-outer"><div class="mid-inner">
                <div class="mid">     
                	
			<table>
				<tr>
					<td>Product Code</td>
					<td><asp:Dropdownlist ID="cmbProductCodeID" runat="server" Width="270px">
        				</asp:Dropdownlist></td>
				</tr>
				
				<tr>
					<td>Start Year</td>
					<td><asp:TextBox ID="txtStartYear" runat="server" Width="270px"></asp:TextBox></td>
				</tr>
				<tr>
					<td>End Year</td>
					<td><asp:TextBox ID="txtEndYear" runat="server" Width="270px"></asp:TextBox></td>
				</tr>
				<tr>
					<td>Policy Term Start</td>
					<td><asp:TextBox ID="txtPolicyTermStart" runat="server" Width="270px"></asp:TextBox></td>
				</tr>
				<tr>
					<td>Policy Term End</td>
					<td><asp:TextBox ID="txtPolicyTermEnd" runat="server" Width="270px"></asp:TextBox></td>
				</tr>
				<tr>
					<td>Agency Commission Rate</td>
					<td><asp:TextBox ID="txtBrokersCommRate" runat="server" Width="270px"></asp:TextBox></td>
				</tr>
				
				<tr>
					<td></td><td>
                        <asp:Button ID="butSave" runat="server" Text="Save" onclick="butSave_Click" />
                        <asp:Button ID="butDelete" runat="server" Text="Delete" style="height: 29px"  /></td>
				</tr>
			</table>
			
			                      </div></div></div>
            <div class="bottom-outer"><div class="bottom-inner">
            <div class="bottom"></div></div></div>                
        </div>      
    </div>
</div>

<div>
      <div class="grid">
            <div class="rounded">
                <div class="top-outer"><div class="top-inner"><div class="top">
                    <h2>Loan Interest </h2>
                </div></div></div>
                <div class="mid-outer"><div class="mid-inner">
                <div class="mid">     
<!-- grid end here-->       
<asp:GridView ID="grdData"  runat="server"  AutoGenerateColumns="False"
        DataSourceID="ods1" AllowSorting="True" CssClass="datatable"
        CellPadding="0" BorderWidth="0px" AlternatingRowStyle-BackColor="#CDE4F1" GridLines="None" HeaderStyle-BackColor="#099cc" ShowFooter="True" >
        <PagerStyle CssClass="pager-row" />
           <RowStyle CssClass="row" />
              <PagerSettings Mode="NumericFirstLast" PageButtonCount="7" FirstPageText="«" LastPageText="»" />      
            <Columns>
            
              <asp:HyperLinkField DataTextField="bcId" DataNavigateUrlFields="bcId,ProductCode"
         DataNavigateUrlFormatString="~/PRG_LI_BRK_COMM.aspx?idd={0},{1}" HeaderText="Id"  
         HeaderStyle-CssClass="first" ItemStyle-CssClass="first"  />
         
              <asp:HyperLinkField DataTextField="ProductCode" DataNavigateUrlFields="bcId,ProductCode"
         DataNavigateUrlFormatString="~/PRG_LI_BRK_COMM.aspx?idd={0},{1}" HeaderText="ProductCode"  
         HeaderStyle-CssClass="first" ItemStyle-CssClass="first"  />
         
                <asp:BoundField DataField="StartYear" HeaderText="Start Yr"  />
                <asp:BoundField DataField="EndYear" HeaderText="End Yr"  />
                <asp:BoundField DataField="CommissionRate" HeaderText="Com.Rate" />
            </Columns>
            
        <HeaderStyle HorizontalAlign="Justify" VerticalAlign="Top" />
                <AlternatingRowStyle BackColor="#CDE4F1" />
        </asp:GridView>
                       </div></div></div>
            <div class="bottom-outer"><div class="bottom-inner">
            <div class="bottom"></div></div></div>                
        </div>      
    </div>
         <asp:ObjectDataSource ID="ods1" runat="server" SelectMethod="BrokersCommissionsDetails" TypeName="CustodianLife.Data.BrokersCommRatesRepository">
    </asp:ObjectDataSource>  
    </div>
		</form>
</body>
</html>
