﻿<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="DebitNoteBrowse.aspx.vb"
    Inherits="ABS_LIFE.DebitNoteBrowse" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <link href="css/general.css" rel="stylesheet" type="text/css" />
    <link href="css/grid.css" rel="stylesheet" type="text/css" />
    <link href="css/rounded.css" rel="stylesheet" type="text/css" />

    <script src="jquery-1.11.0.js" type="text/javascript"></script>

    <script language="javascript" type="text/javascript">
        // calling jquery functions once document is ready
        $(document).ready(function() {
            $("#cmbSearchAccount").hide();
            $("#cmbSearchAgent").hide();

            $("#cmbChoice").on('focusout', function(e) {
                e.preventDefault()
                //            if ($("#cmbChoice").val() == "Accounts") {
                //                $("#cmbSearchAccount").show();
                //                $("#cmbSearchAgent").hide();
                //            }
                //            else if ($("#cmbChoice").val() == "Brokers") {
                //                $("#cmbSearchAgent").show();
                //                $("#cmbSearchAccount").hide();
                //            }
                //            else {
                //                alert("Please Choose the Account Type Codes to Browse");
                //            }
                return false;
            });

        });

        function GetRowValue(val) {
            //put the value from the grid selection to an element on the page for onward retrieval
            var txtar = val.split(",");
            document.getElementById("txtValue").value = txtar[1];//Debit Note
            document.getElementById("txtDesc").value = txtar[0]; //PolicyNo
            document.getElementById("txtValue1").value = txtar[2]//BrokerCode;
//            document.getElementById("txtDesc1").value = txtar[3];
//            document.getElementById("txtDesc2").value = txtar[4];
            //  alert("sub code " + txtar[2] + " sub desc " +txtar[3]; 
        }
    </script>

    <title></title>
</head>
<body>
    <form id="frmAccountChartSearch" runat="server">
    <div id="AccountChartSearch">
        <div class="gridp">
            <div class="rounded">
                <div class="top-outer">
                    <div class="top-inner">
                        <div class="top">
                            <h2>
                                Debit Notes
                            </h2>
                        </div>
                    </div>
                </div>
                <div class="mid-outer">
                    <div class="mid-inner">
                        <div class="mid">
                            <!-- grid end here-->
                            <div>
                                <table class="datatable">
                                    <tr>
                                        <td>
                                            Search
                                        </td>
                                        <td>
                                            <asp:DropDownList ID="cmbChoice" runat="server" Visible="true">
                                                <asp:ListItem Value="0">Select</asp:ListItem>
                                                <asp:ListItem Value="Scheme">Scheme Name</asp:ListItem>
                                                <asp:ListItem Value="Policy">Policy Number</asp:ListItem>
                                            </asp:DropDownList>
                                            <asp:DropDownList ID="cmbSearchAccount" runat="server">
                                                <asp:ListItem Value="0">Select</asp:ListItem>
                                                <asp:ListItem Value="All">All</asp:ListItem>
                                                <asp:ListItem Value="Name">Main Descriptn</asp:ListItem>
                                                <asp:ListItem Value="Name1">Sub Descriptn</asp:ListItem>
                                                <asp:ListItem Value="Code">Account Code</asp:ListItem>
                                                <asp:ListItem Value="SbCode">Sub A/C Code</asp:ListItem>
                                            </asp:DropDownList>
                                            <asp:DropDownList ID="cmbSearchAgent" runat="server">
                                                <asp:ListItem Value="0">Select</asp:ListItem>
                                                <asp:ListItem Value="All">All</asp:ListItem>
                                                <asp:ListItem Value="Name">Name</asp:ListItem>
                                                <asp:ListItem Value="Code">Code Id</asp:ListItem>
                                            </asp:DropDownList>
                                        </td>
                                        <td>
                                            <asp:TextBox ID="txtSearch" runat="server"></asp:TextBox><asp:Button ID="butGO" runat="server"
                                                Text="GO" Width="30px" Height="30px" />
                                        </td>
                                        <td>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                            <asp:GridView ID="grdView"  runat="server"  AutoGenerateColumns="False" 
        DataSourceID="ods1" AllowSorting="True" AllowPaging="True" CssClass="datatable"  
        CellPadding="0" BorderWidth="0px" AlternatingRowStyle-BackColor="#CDE4F1" GridLines="None" HeaderStyle-BackColor="#099cc" ShowFooter="True" >
        <PagerStyle CssClass="pager-row" />
           <RowStyle CssClass="row" />
              <PagerSettings Mode="NumericFirstLast" PageButtonCount="7"  FirstPageText="«" LastPageText="»" />      
          <Columns>
                                    <asp:TemplateField>
                                        <AlternatingItemTemplate>
                                            <asp:Button ID="butSelect" runat="server" Height="20px" Width="20px" />
                                        </AlternatingItemTemplate>
                                        <ItemTemplate>
                                            <asp:Button ID="butSelect" runat="server" Height="20px" Width="20px" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="PolicyNo" HeaderText="Policy Number" HeaderStyle-CssClass="first"
                                        ItemStyle-CssClass="first">
                                        <HeaderStyle CssClass="first"></HeaderStyle>
                                        <ItemStyle CssClass="first"></ItemStyle>
                                    </asp:BoundField>
                                    <asp:BoundField DataField="DrCrNo" HeaderText="DrCr Note" HeaderStyle-CssClass="first"
                                        ItemStyle-CssClass="first">
                                        <HeaderStyle CssClass="first"></HeaderStyle>
                                        <ItemStyle CssClass="first"></ItemStyle>
                                    </asp:BoundField>
                                     <asp:BoundField DataField="BrokerCode" HeaderText="Broker Code" />
                                    <asp:BoundField DataField="ProcessingDate" HeaderText="Process Date" />
                                    <asp:BoundField DataField="BatchNo" HeaderText="Batch No" />
                                   <%-- <asp:BoundField DataField="AccountLedgerType" HeaderText="LTyp" />--%>
                                </Columns>
            
        <HeaderStyle HorizontalAlign="Justify" VerticalAlign="Top" />
                <AlternatingRowStyle BackColor="#CDE4F1" />
        </asp:GridView>
<%-- <asp:GridView ID="grdView1"  runat="server"  AutoGenerateColumns="False" 
        DataSourceID="ods2" AllowSorting="True" AllowPaging="True" CssClass="datatable"  
        CellPadding="0" BorderWidth="0px" AlternatingRowStyle-BackColor="#CDE4F1" GridLines="None" HeaderStyle-BackColor="#099cc" ShowFooter="True" >
        <PagerStyle CssClass="pager-row" />
           <RowStyle CssClass="row" />
              <PagerSettings Mode="NumericFirstLast" PageButtonCount="7"  FirstPageText="«" LastPageText="»" />      
            <Columns>
                                    <asp:TemplateField>
                                        <AlternatingItemTemplate>
                                            <asp:Button ID="butSelect" runat="server" Height="20px" Width="20px" />
                                        </AlternatingItemTemplate>
                                        <ItemTemplate>
                                            <asp:Button ID="butSelect" runat="server" Height="20px" Width="20px" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="PolicyNo" HeaderText="Policy Number" HeaderStyle-CssClass="first"
                                        ItemStyle-CssClass="first">
                                        <HeaderStyle CssClass="first"></HeaderStyle>
                                        <ItemStyle CssClass="first"></ItemStyle>
                                    </asp:BoundField>
                                    <asp:BoundField DataField="DrCrNo" HeaderText="DrCr Note" HeaderStyle-CssClass="first"
                                        ItemStyle-CssClass="first">
                                        <HeaderStyle CssClass="first"></HeaderStyle>
                                        <ItemStyle CssClass="first"></ItemStyle>
                                    </asp:BoundField>
                                    <asp:BoundField DataField="ProcessingDate" HeaderText="Process Date" />
                                    <asp:BoundField DataField="BatchNo" HeaderText="Batch No" />
                                
            
        <HeaderStyle HorizontalAlign="Justify" VerticalAlign="Top" />
                <AlternatingRowStyle BackColor="#CDE4F1" />
        </asp:GridView>--%>
                            
                            
                        </div>
                    </div>
                </div>
                <div class="bottom-outer">
                    <div class="bottom-inner">
                        <div class="bottom">
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <asp:ObjectDataSource ID="ods1" runat="server" SelectMethod="GetDebitNoteList"
            TypeName="CustodianLife.Data.GroupDRCRNoteRepository">
            <SelectParameters>
                <asp:ControlParameter ControlID="cmbChoice" Name="_key" PropertyName="SelectedValue"
                    Type="String" />
                <asp:ControlParameter ControlID="txtSearch" Name="_value" PropertyName="Text" Type="String" />
            </SelectParameters>
        </asp:ObjectDataSource>
         <%--<asp:ObjectDataSource ID="ods2" runat="server" SelectMethod="GetDebitNoteList"
            TypeName="CustodianLife.Data.GroupDRCRNoteRepository">
            <SelectParameters>
                <asp:ControlParameter ControlID="cmbChoice" Name="_key" PropertyName="SelectedValue"
                    Type="String" />
                <asp:ControlParameter ControlID="txtSearch" Name="_value" PropertyName="Text" Type="String" />
            </SelectParameters>
        </asp:ObjectDataSource>--%>
    </div>
    <div>
        <asp:TextBox runat="server" ID="txtValue" CssClass="popupOffset"></asp:TextBox>
        <asp:TextBox runat="server" ID="txtDesc" CssClass="popupOffset"></asp:TextBox>
        <asp:TextBox runat="server" ID="txtValue1" CssClass="popupOffset"></asp:TextBox>
        <asp:TextBox runat="server" ID="txtDesc1" CssClass="popupOffset"></asp:TextBox>
        <asp:TextBox runat="server" ID="txtDesc2" CssClass="popupOffset"></asp:TextBox>
        <asp:TextBox runat="server" ID="txtParentCode" CssClass="popupOffset"></asp:TextBox>
    </div>
    </form>
</body>
</html>
