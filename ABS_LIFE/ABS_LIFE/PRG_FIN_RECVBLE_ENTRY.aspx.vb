﻿Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports CustodianLife.Data
Imports CustodianLife.Model
Imports CustodianLife.Repositories
Imports System.Xml.Serialization
Imports System.Xml
Imports System.IO
Imports System.Globalization

Partial Public Class PRG_FIN_RECVBLE_ENTRY
    Inherits System.Web.UI.Page

    Dim gtRepo As GLTransRepository
    Dim indLifeEnq As IndLifeCodesRepository
    Dim transTypeEnq As TransactionTypesRepository
    '    Dim hHelper As HashHelper
    Dim polinfo As PolicyInfo
    Dim updateFlag As Boolean
    Dim strKey As String
    Dim prgKey As String
    Dim Save_Amount As Decimal = 0
    Dim Cum_Detail_Amount As Decimal = 0
    Dim Gtrans As CustodianLife.Model.GLTrans
    Dim swEntry As String = "H"
    Dim detailEdit As String = "N"
    Dim TotTransAmt As Decimal = 0
    Dim TransAmt As Decimal = 0
    Dim TransType As String
    Dim newDocRefNo As String
    Protected publicMsgs As String = String.Empty
    Protected ci As CultureInfo
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        txtSubSerialNo.Attributes.Add("disabled", "disabled")
        txtRefAmt.Attributes.Add("disabled", "disabled")
        txtRefDate.Attributes.Add("disabled", "disabled")
        txtLedgerType.Attributes.Add("disabled", "disabled")

        If Not Page.IsPostBack Then
            'set globalization and culture -- date issues

            ci = New CultureInfo("en-GB")
            Session("ci") = ci

            gtRepo = New GLTransRepository
            indLifeEnq = New IndLifeCodesRepository
            transTypeEnq = New TransactionTypesRepository

            Session("gtRepo") = gtRepo
            updateFlag = False
            Session("updateFlag") = updateFlag
            swEntry = "H"
            Session("swEntry") = swEntry

            strKey = Request.QueryString("idd")
            prgKey = Request.QueryString("prgKey")
            Session("prgKey") = prgKey
            txtProgType.Text = prgKey
            Session("gtId") = strKey
            Session("Save_Amount") = Save_Amount
            Session("Cum_Detail_Amount") = Cum_Detail_Amount
            'Company code value to be filled from login
            txtCompanyCode.Text = "001"
            'txtEntryDate.Text = Now.Date.ToString()
            txtEntryDate.Text = Format(Now, "dd/MM/yyyy")
            txtEntryDate.ReadOnly = True
            lblError.Visible = False
            'If prgKey = "journal" Or prgKey = "payment" Then
            '    txtReceiptNo.Enabled = True
            'End If


            SetComboBinding(cmbBranchCode, indLifeEnq.GetById("L02", "003"), "CodeItem_CodeLongDesc", "CodeItem")
            SetComboBinding(cmbCurrencyType, indLifeEnq.GetById("L02", "017"), "CodeItem_CodeLongDesc", "CodeItem")
            SetComboBinding(cmbDept, indLifeEnq.GetById("L02", "005"), "CodeItem_CodeLongDesc", "CodeItem")
            SetComboBinding(cmbTransDetailType, transTypeEnq.TransactionTypesDetails, "TransactionCode_Description", "TransactionCode")

            cmbTransType.Attributes.Add("readonly", "readonly")
            txtSubAcct.Text = "000000"

            If strKey IsNot Nothing Then
                fillValues()
            Else
                gtRepo = CType(Session("gtRepo"), GLTransRepository)
            End If

        Else 'post back

            Me.Validate()
            If (Not Me.IsValid) Then
                Dim msg As String
                ' Loop through all validation controls to see which 
                ' generated the error(s).
                Dim oValidator As IValidator
                For Each oValidator In Validators
                    If oValidator.IsValid = False Then
                        msg = msg & "\n" & oValidator.ErrorMessage
                    End If
                Next

                lblError.Text = msg
                lblError.Visible = True
                publicMsgs = "javascript:alert('" + msg + "')"
            End If



            ci = CType(Session("ci"), CultureInfo)


        End If


    End Sub


    Protected Sub butSave_Click(ByVal sender As Object, ByVal e As EventArgs) Handles butSave.Click, butSaveDetail.Click
        lblError.Text = ""
        Dim msg As String = String.Empty
        Dim docMonth As String = String.Empty
        Dim docYear As String = String.Empty
        Dim Err = ""
        ValidateFields(Err)
        If Err = "Y" Then
            Exit Sub
        End If
        Try
            If Me.IsValid Then

                'this routine will persist only one object. 
                '1. The GlTrans object

                updateFlag = CType(Session("updateFlag"), Boolean)


                If Not updateFlag Then 'if new record

                    'create a new instance of the GLTrans object
                    Gtrans = New CustodianLife.Model.GLTrans()
                    gtRepo = New GLTransRepository()
                    lblError.Visible = False

                    With Gtrans
                        .BatchDate = CType(txtBatchDate.Text, Integer)
                        .BatchNo = CType(txtBatchNo.Text, Integer)
                        .BranchCode = cmbBranchCode.SelectedValue.ToString()

                        If Trim(txtChequeDate.Text).Length > 0 Then
                            .ChequeDate = ValidDate(txtChequeDate.Text)
                        Else
                            .ChequeDate = #1/1/2014#
                        End If

                        .ChequeNo = txtChequeNo.Text
                        .ClientName = txtPayeeName.Text
                        .CompanyCode = txtCompanyCode.Text
                        .CurrencyType = cmbCurrencyType.SelectedValue.ToString()
                        .DeptCode = cmbDept.SelectedValue.ToString()
                        .DocNo = txtReceiptNo.Text
                        .EntryDate = Now.Date

                        .MainAccount = txtMainAcct.Text
                        .OperatorId = "001"
                        .PostStatus = "U" 'Unposted
                        .ApprovalStatus = "N"
                        .RecordStatus = "A"
                        .Remarks = txtRemarks.Text

                        .SubAccount = txtSubAcct.Text
                        .TempBrokerName = txtTempBrokerName.Text

                        If Trim(txtTellerDate.Text).Length > 0 Then
                            .TellerDate = ValidDate(txtTellerDate.Text)
                        Else
                            .TellerDate = #1/1/2014#
                        End If

                        .TellerNo = txtTellerNo.Text
                        If Trim(txtTellerDate.Text).Length > 0 Then
                            .TransDate = ValidDate(txtEffectiveDate.Text)
                        Else
                            .TransDate = #1/1/2014#
                        End If

                        .TransDescription = txtTransDesc.Text
                        .TransId = "H" 'header
                        .TransType = cmbTransType.SelectedValue.ToString()
                        '.TotalAmt = CType(txtTotalAmt.Text, Decimal)  'detail amounts must total to this
                        Save_Amount = CType(txtTotalAmt.Text, Decimal)
                        Session("Save_Amount") = Save_Amount

                        Cum_Detail_Amount = 0
                        Session("Cum_Detail_Amount") = Cum_Detail_Amount

                        docMonth = Right(txtBatchDate.Text, 2)
                        docYear = Left(txtBatchDate.Text, 4)
                        'get new serial number
                        'L02(other receipts) , 001 -- for main serial number 
                        'L02(other receipts), 002 -- for Sub serial number 
                        If .TransId = "H" Then
                            Dim newSerialNum As String = gtRepo.GetNextSerialNumber("L02", "001", docMonth, docYear, " ", "12", "11")

                            prgKey = Session("prgKey")
                            If prgKey = "journal" Or prgKey = "JV" Then
                                .TransType = "JV"
                                'get new journal number
                                newDocRefNo = gtRepo.GetNextSerialNumber("JNL", _
                                                                         "001", _
                                                                         "001", _
                                                                       docYear, _
                                                                      "JV/", _
                                                                          "13", _
                                                                           "12")
                            ElseIf prgKey = "payment" Or prgKey = "PV" Then
                                .TransType = "PV"
                                'get new Payment voucher number
                                newDocRefNo = gtRepo.GetNextSerialNumber("PMT", _
                                                                         "001", _
                                                                         "001", _
                                                                       docYear, _
                                                                      "PV/", _
                                                                          "13", _
                                                                           "12")
                            Else
                                .TransType = "R"
                                'get new receipt number
                                newDocRefNo = gtRepo.GetNextSerialNumber("RCN", _
                                                                         "001", _
                                                                         "001", _
                                                                       docYear, _
                                                                      "BRHO/" + docYear, _
                                                                          "13", _
                                                                           "12")
                            End If
                            txtReceiptNo.Text = newDocRefNo
                            txtSerialNo.Text = newSerialNum
                            .SerialNo = CType(txtSerialNo.Text, Long)
                            .DocNo = Trim(txtReceiptNo.Text)

                        End If
                        .TransMode = cmbMode.SelectedValue.ToString

                        'initialize details part
                        .DetailTransType = String.Empty 'D
                        .DRCR = String.Empty 'D
                        .GLAmountLC = 0.0 'D
                        .GLAmountFC = 0.0 'D
                        .RefAmount = 0.0 'D
                        .RefDate = #1/1/2014# 'D
                        .RefNo1 = String.Empty 'D
                        .RefNo2 = String.Empty 'D
                        .RefNo3 = String.Empty 'D
                        .RefAmount = 0.0  'D
                        .SubSerialNo = 0 'D

                        'save
                        gtRepo.Save(Gtrans)
                        grdData.DataBind()
                        Session("Gtrans") = Gtrans
                        updateFlag = True
                        Session("updateFlag") = updateFlag
                        detailEdit = "N"
                        Session("detailEdit") = detailEdit
                        ' initializeDetailFields()
                    End With
                Else
                    swEntry = CType(Session("swEntry"), String)
                    If swEntry = "D" Then
                        Gtrans = New CustodianLife.Model.GLTrans() 'a new object but filled with old values with the only changes from the detail part
                        detailEdit = "N"
                        Session("detailEdit") = detailEdit ' detail records in new rec mode

                    Else
                        detailEdit = Session("detailEdit") ' detail records in edit mode
                        Gtrans = CType(Session("Gtrans"), CustodianLife.Model.GLTrans)
                    End If
                    gtRepo = CType(Session("gtRepo"), GLTransRepository)


                    With Gtrans
                        .BatchDate = CType(txtBatchDate.Text, Integer)
                        .BatchNo = CType(txtBatchNo.Text, Integer)
                        .BranchCode = cmbBranchCode.SelectedValue.ToString()

                        If Trim(txtChequeDate.Text).Length > 0 Then
                            .ChequeDate = ValidDate(txtChequeDate.Text)
                        Else
                            .ChequeDate = #1/1/2014#
                        End If

                        .ChequeNo = txtChequeNo.Text
                        .ClientName = txtPayeeName.Text
                        .CompanyCode = txtCompanyCode.Text
                        .CurrencyType = cmbCurrencyType.SelectedValue.ToString()
                        .DeptCode = cmbDept.SelectedValue.ToString()
                        .DocNo = txtReceiptNo.Text
                        .EntryDate = Now.Date
                        '.TotalAmt = Math.Round(CType(txtTotalAmt.Text, Decimal), 2)
                        .DetailTransType = cmbTransDetailType.SelectedValue.ToString 'D
                        .DRCR = cmbDRCR.SelectedValue.ToString 'D
                        '.GLAmountLC = Math.Round(CType(txtTransAmt.Text, Decimal), 2) 'D
                        .GLAmountLC = Format(txtTransAmt.Text, "Standard")
                        '.RefAmount = CType(txtRefAmt.Text, Decimal) 'D
                        .RefAmount = Format(txtRefAmt.Text, "Standard") 'D


                        If Trim(txtRefDate.Text).Length > 0 Then
                            .RefDate = ValidDate(txtRefDate.Text)

                        Else
                            .RefDate = #1/1/2014#
                        End If
                        .RefNo1 = txtReceiptRefNo1.Text 'D
                        .RefNo2 = txtReceiptRefNo2.Text 'D
                        .RefNo3 = txtReceiptRefNo3.Text 'D
                        .MainAccount = txtMainAcct.Text
                        '.OperatorId = "001"
                        .PostStatus = "U" 'Unposted
                        .ApprovalStatus = "N"
                        .RecordStatus = "A"
                        .Remarks = txtRemarks.Text
                        .SubAccount = txtSubAcct.Text
                        .TempBrokerName = txtTempBrokerName.Text
                        docMonth = Right(txtBatchDate.Text, 2)
                        docYear = Left(txtBatchDate.Text, 4)

                        If Trim(txtTellerDate.Text).Length > 0 Then
                            .TellerDate = ValidDate(txtTellerDate.Text)

                        Else
                            .TellerDate = #1/1/2014#
                        End If

                        .TellerNo = txtTellerNo.Text

                        If Trim(txtEffectiveDate.Text).Length > 0 Then
                            .TransDate = ValidDate(txtEffectiveDate.Text)

                        Else
                            .TransDate = #1/1/2014#
                        End If
                        .TransDescription = txtTransDesc.Text
                        .TransId = "D" 'Detail
                        .SerialNo = CType(txtSerialNo.Text, Long)
                        .DocNo = Trim(txtReceiptNo.Text)
                        .LedgerTypeCode = Trim(txtLedgerType.Text)
                        .TransMode = cmbMode.SelectedValue.ToString
                        swEntry = CType(Session("swEntry"), String)
                        If swEntry = "H" Or swEntry = "D" Then
                            swEntry = "D"
                            Session("swEntry") = swEntry
                            'get sub serial number
                            'L02, 002 -- for Sub serial number 
                            detailEdit = CType(Session("detailEdit"), String)
                            If detailEdit = "N" Then
                                Dim newSerialSubNum As String = gtRepo.GetNextSerialNumber("L02", "002", docMonth, docYear, " ", "12", "11")
                                txtSubSerialNo.Text = newSerialSubNum
                                .SubSerialNo = CType(txtSubSerialNo.Text, Integer)
                            End If
                        End If

                        prgKey = Session("prgKey")
                        If prgKey = "journal" Or prgKey = "JV" Then
                            .TransType = "JV"
                        ElseIf prgKey = "payment" Or prgKey = "PV" Then
                            .TransType = "PV"
                        Else
                            .TransType = "R"
                        End If

                        Cum_Detail_Amount = Cum_Detail_Amount + .GLAmountLC
                        Session("Cum_Detail_Amount") = Cum_Detail_Amount

                        ' If .PostStatus = "U" And .ApprovalStatus = "N" Then  'Unposted and Not Approved -- change is possible

                        gtRepo.Save(Gtrans)
                        msg = "Save Operation Successful"

                        lblError.Text = msg
                        lblError.Visible = True
                        publicMsgs = "javascript:alert('" + msg + "')"

                        'End If
                        grdData.DataBind()

                        Session("Gtrans") = Gtrans
                        initializeDetailFields()
                    End With


                End If
                ' initializeFields()


            End If
        Catch ex As Exception
            msg = ex.Message
            lblError.Text = msg
            lblError.Visible = True
            publicMsgs = "javascript:alert('" + msg + "')"

        End Try
    End Sub
    Private Sub SetComboBinding(ByVal toBind As ListControl, ByVal dataSource As Object, ByVal displayMember As String, ByVal valueMember As String)
        toBind.DataTextField = displayMember
        toBind.DataValueField = valueMember
        toBind.DataSource = dataSource
        toBind.DataBind()
        toBind.Items.Insert(0, New ListItem("Select", "NA"))
    End Sub

    Private Sub fillValues()

        strKey = CType(Session("gtId"), String)
        gtRepo = CType(Session("gtRepo"), GLTransRepository)
        Gtrans = gtRepo.GetById(strKey)

        Session("Gtrans") = Gtrans
        If Gtrans IsNot Nothing Then
            With Gtrans
                txtBatchDate.Text = .BatchDate
                txtBatchNo.Text = .BatchNo
                cmbBranchCode.SelectedValue = .BranchCode
                txtBranchCode.Text = cmbBranchCode.SelectedValue
                txtChequeDate.Text = ValidDateFromDB(.ChequeDate)
                txtChequeNo.Text = .ChequeNo
                txtPayeeName.Text = .ClientName
                txtCompanyCode.Text = .CompanyCode
                cmbCurrencyType.SelectedValue = .CurrencyType
                txtCurrencyCode.Text = cmbCurrencyType.SelectedValue
                cmbDept.SelectedValue = .DeptCode
                txtDeptCode.Text = cmbDept.SelectedValue
                txtReceiptNo.Text = .DocNo
                txtEntryDate.Text = .EntryDate
                cmbTransDetailType.SelectedValue = Trim(.DetailTransType)
                txtTransTypeCode.Text = cmbTransDetailType.SelectedValue
                cmbDRCR.SelectedValue = .DRCR
                txtDRCR.Text = cmbDRCR.SelectedValue
                'txtTotalAmt.Text = Math.Round(.TotalAmt, 2)
                ' txtRefAmt.Text = Math.Round(.RefAmount, 2)
                txtRefAmt.Text = Format(.RefAmount, "Standard")
                txtRefDate.Text = ValidDateFromDB(.RefDate)
                txtReceiptRefNo1.Text = .RefNo1
                txtReceiptRefNo2.Text = .RefNo2
                txtReceiptRefNo3.Text = .RefNo3
                txtMainAcct.Text = .MainAccount
                '.OperatorId = "001"
                ' .PostStatus = "U" 'Unposted
                txtRemarks.Text = .Remarks
               
                If .SubAccount = "" Then
                    txtSubAcct.Text = "000000"
                Else
                    txtSubAcct.Text = .SubAccount

                End If

                'Fill Account Description
                If txtMainAcct.Text <> "" Then
                    GetAcctDescription()
                End If

                txtTempBrokerName.Text = .TempBrokerName
                If .TempBrokerName <> "" Then
                    HidShowHide.Value = "show"
                End If

                txtSubSerialNo.Text = .SubSerialNo
                txtTellerDate.Text = ValidDateFromDB(.TellerDate)
                txtTellerNo.Text = .TellerNo
                txtEffectiveDate.Text = ValidDateFromDB(.TransDate)
                txtTransDesc.Text = .TransDescription
                txtSerialNo.Text = .SerialNo
                txtReceiptNo.Text = .DocNo
                cmbTransType.SelectedValue = .TransType

                ' txtTransAmt.Text = Math.Round(.GLAmountLC, 2)
                txtTransAmt.Text = Format(.GLAmountLC, "Standard")

                cmbMode.SelectedValue = .TransMode
                txtMode.Text = cmbMode.SelectedValue
                txtLedgerType.Text = .LedgerTypeCode
                txtProgType.Text = CType(Session("prgKey"), String)

                Session("Gtrans") = Gtrans
            End With

            updateFlag = True
            Session("updateFlag") = updateFlag
            detailEdit = "Y"
            Session("detailEdit") = detailEdit
            grdData.DataBind()


        End If

    End Sub
    Protected Sub initializeFields()
        txtMode.Text = String.Empty
        txtBranchCode.Text = String.Empty
        txtCurrencyCode.Text = String.Empty
        txtDeptCode.Text = String.Empty
        txtBatchDate.Text = String.Empty
        txtBatchNo.Text = String.Empty
        'cmbBranchCode.SelectedIndex = 0
        cmbBranchCode.Text = "1501"
        txtChequeDate.Text = String.Empty
        txtChequeNo.Text = String.Empty
        txtPayeeName.Text = String.Empty
        txtCompanyCode.Text = "001"
        cmbCurrencyType.SelectedIndex = 0
        cmbDept.SelectedIndex = 0
        txtReceiptNo.Text = String.Empty
        txtTotalAmt.Text = 0.0
        '.OperatorId = "001"
        '.PostStatus = "U" 'Unposted
        txtRemarks.Text = String.Empty
        txtTellerDate.Text = String.Empty
        txtTellerNo.Text = String.Empty
        txtEffectiveDate.Text = String.Empty
        txtTransDesc.Text = String.Empty
        txtSerialNo.Text = String.Empty
        txtReceiptNo.Text = String.Empty
        cmbMode.SelectedIndex = 0

        cmbTransDetailType.SelectedIndex = 0
        cmbDRCR.SelectedIndex = 1
        'txtRefAmt.Text = String.Empty
        txtRefDate.Text = String.Empty
        'txtReceiptRefNo1.Text = String.Empty
        txtReceiptRefNo2.Text = String.Empty
        txtReceiptRefNo3.Text = String.Empty
        txtRefAmt.Text = 0.0
        txtMainAcct.Text = String.Empty
        txtSubAcct.Text = String.Empty
        txtSubAcctDesc.Text = String.Empty
        txtMainAcctDesc.Text = String.Empty
        txtLedgerType.Text = String.Empty
        txtTransAmt.Text = 0.0


        txtSubSerialNo.Text = 0
        txtTempBrokerName.Text = String.Empty

        updateFlag = False
        Session("updateFlag") = updateFlag 'ready for a new record
        detailEdit = "N"
        Session("detailEdit") = detailEdit

        swEntry = "H"
        Session("swEntry") = swEntry

        prgKey = Session("prgKey")

    End Sub
    Protected Sub initializeDetailFields()

        cmbTransDetailType.SelectedIndex = 0
        txtTransTypeCode.Text = String.Empty
        cmbDRCR.SelectedIndex = 1
        txtRefAmt.Text = String.Empty
        txtRefDate.Text = String.Empty
        'txtReceiptRefNo1.Text = String.Empty
        txtReceiptRefNo2.Text = String.Empty
        txtReceiptRefNo3.Text = String.Empty
        txtRefAmt.Text = 0.0
        txtMainAcct.Text = String.Empty
        txtSubAcct.Text = String.Empty
        txtSubAcctDesc.Text = String.Empty
        txtMainAcctDesc.Text = String.Empty
        txtLedgerType.Text = String.Empty
       
        txtSubSerialNo.Text = 0
        txtTempBrokerName.Text = String.Empty
        detailEdit = "N"
        Session("detailEdit") = detailEdit
        swEntry = "D"
        Session("swEntry") = swEntry
    End Sub
    Protected Sub butDelete_Click(ByVal sender As Object, ByVal e As EventArgs) Handles butDelete.Click, butDeleteDetail.Click
        Dim msg As String = String.Empty
        Gtrans = CType(Session("Gtrans"), CustodianLife.Model.GLTrans)
        gtRepo = CType(Session("gtRepo"), GLTransRepository)
        Try
            gtRepo.Delete(Gtrans)
            msg = "Delete Successful"
            lblError.Text = msg
            grdData.DataBind()

        Catch ex As Exception
            msg = ex.Message
            lblError.Text = msg
            lblError.Visible = True
            publicMsgs = "javascript:alert('" + msg + "')"
        End Try
        initializeDetailFields()



    End Sub

    Protected Sub butClose_Click(ByVal sender As Object, ByVal e As EventArgs) Handles butClose.Click
        Dim msg As String = String.Empty

        Cum_Detail_Amount = CType(Session("Cum_Detail_Amount"), Decimal)
        Save_Amount = CType(Session("Save_Amount"), Decimal)


        'If (Save_Amount <> Cum_Detail_Amount) Then
        '    msg = "Total Amount " & Save_Amount & "is not equal to Detail Amounts!: " & Cum_Detail_Amount & "Pls Review before Closing"
        '    publicMsgs = "javascript:alert('" + msg + "')"

        '    Return 'do nothing
        'End If
        prgKey = Session("prgKey")


        Response.Redirect("ReceiptOthersList.aspx?prgKey=" & prgKey)
    End Sub

    Private Sub grdData_RowDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewRowEventArgs) Handles grdData.RowDataBound

        If (e.Row.RowType = DataControlRowType.DataRow) Then
            Dim lblPrice As Label = CType(e.Row.FindControl("lblTransAmt"), Label)
            TransType = (DataBinder.Eval(e.Row.DataItem, "DRCR"))
            TransAmt = (Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "GLAmountLC")))
            If TransType = "C" Then
                TransAmt = (TransAmt * -1)
            End If
            TotTransAmt = (TotTransAmt + TransAmt)

        End If
        If (e.Row.RowType = DataControlRowType.Footer) Then
            Dim lblTotal As Label = CType(e.Row.FindControl("lbltxtTotal"), Label)
            lblTotal.Text = Math.Round(TotTransAmt, 2).ToString
        End If

        'format fields
        Dim ea As GridViewRowEventArgs = CType(e, GridViewRowEventArgs)
        If (ea.Row.RowType = DataControlRowType.DataRow) Then
            Dim drv As GLTrans = CType(ea.Row.DataItem, GLTrans)
            ' Dim ob As Object = drv("GLAmountLC")
            If Not Convert.IsDBNull(drv.GLAmountLC) Then
                Dim iParsedValue As Decimal = 0
                If Decimal.TryParse(drv.GLAmountLC.ToString, iParsedValue) Then
                    Dim cell As TableCell = ea.Row.Cells(14)
                    cell.Text = String.Format(System.Globalization.CultureInfo.CurrentCulture, "{0:N}", New Object() {iParsedValue})
                End If
            End If
        End If
    End Sub

    Protected Sub butNew_Click(ByVal sender As Object, ByVal e As EventArgs) Handles butNew.Click
        initializeFields()
    End Sub

    Protected Sub butNewDetail_Click(ByVal sender As Object, ByVal e As EventArgs) Handles butNewDetail.Click
        initializeDetailFields()
    End Sub
    Private Function ValidDate(ByVal DateValue As String) As DateTime
        Dim dateparts() As String = DateValue.Split(Microsoft.VisualBasic.ChrW(47))
        Dim strDateTest As String = dateparts(1) & "/" & dateparts(0) & "/" & dateparts(2)
        Dim dateIn As Date = Format(CDate(strDateTest), "MM/dd/yyyy")
        Return dateIn
    End Function
    Private Function ValidDateFromDB(ByVal DateValue As Date) As String
        Dim dateparts() As String = DateValue.Date.ToString.Split(Microsoft.VisualBasic.ChrW(47))
        Dim strDateTest As String = dateparts(1) & "/" & dateparts(0) & "/" & Left(dateparts(2), 4)
        Return strDateTest
    End Function

    <System.Web.Services.WebMethod()> _
 Public Shared Function GetGrpDNoteInfo(ByVal _brokercode As String, ByVal _policynum As String, ByVal _transno As String) As String
        Dim polinfos As String = String.Empty
        Dim recRepo As New GLTransRepository()

        Try
            polinfos = recRepo.GetGroupDRNoteInfo(_brokercode, _policynum, _transno)

        Catch ex As ApplicationException

        Finally
            If polinfos = "<NewDataSet />" Then
                Throw New Exception()
            End If



        End Try
        Return polinfos
    End Function
    <System.Web.Services.WebMethod()> _
 Public Shared Function GetClaimsDNoteInfo(ByVal _brokercode As String, ByVal _claimsno As String, ByVal _transno As String) As String
        Dim cinfos As String = String.Empty
        Dim recRepo As New GLTransRepository()

        Try
            cinfos = recRepo.GetClaimsDRNoteInfo(_brokercode, _claimsno, _transno)

        Catch ex As ApplicationException

        Finally
            If cinfos = "<NewDataSet />" Then
                Throw New Exception()
            End If
        End Try
        Return cinfos
    End Function
    <System.Web.Services.WebMethod()> _
    Public Shared Function GetInvoiceInfo(ByVal _creditorcode As String, ByVal _invoiceno As String) As String
        Dim cinfos As String = String.Empty
        Dim recRepo As New GLTransRepository()

        Try
            cinfos = recRepo.GetInvoiceInfo(_creditorcode, _invoiceno)

        Catch ex As ApplicationException

        Finally
            If cinfos = "<NewDataSet />" Then
                Throw New Exception()
            End If
        End Try


        Return cinfos
    End Function
    <System.Web.Services.WebMethod()> _
    Public Shared Function GetBrokerInfo(ByVal _brokercode As String) As String
        Dim brkinfo As String = String.Empty
        Dim recRepo As New GLTransRepository()

        Try
            brkinfo = recRepo.GetBrokerInfo(_brokercode)

        Catch ex As ApplicationException

        Finally
            If brkinfo = "<NewDataSet />" Then
                Throw New Exception()
            End If
        End Try


        Return brkinfo
    End Function

    Protected Sub csValidateCurrencyType_ServerValidate(ByVal source As Object, ByVal args As System.Web.UI.WebControls.ServerValidateEventArgs) Handles csValidateCurrencyType.ServerValidate

        If cmbCurrencyType.SelectedValue = "0" Then
            args.IsValid = False

        End If
    End Sub

    Protected Sub CustomValidator1_ServerValidate(ByVal source As Object, ByVal args As System.Web.UI.WebControls.ServerValidateEventArgs) Handles CustomValidator1.ServerValidate
        If cmbDRCR.SelectedValue = "0" Then
            args.IsValid = False
        End If
    End Sub

    Protected Sub butPrintRecpt_Click(ByVal sender As Object, ByVal e As EventArgs) Handles butPrintRecpt.Click
        If Len(Trim(txtRecptNo.Text)) = 0 Then
            publicMsgs = "javascript:alert('Error! Please enter a valid receipt number')"
        Else
            Session("orcPrintNo") = txtRecptNo.Text
            Response.Redirect("OtherReceiptPrint.aspx")
        End If
    End Sub

    Protected Sub grdData_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles grdData.SelectedIndexChanged

    End Sub


    Protected Sub butPrint_Click(ByVal sender As Object, ByVal e As EventArgs) Handles butPrint.Click
        Response.Redirect("prg_fin_othr_recpt_print.aspx?id=" & txtReceiptNo.Text)

    End Sub

    Protected Sub cmbMode_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles cmbMode.SelectedIndexChanged
        lblError.Text = ""
        If cmbMode.SelectedIndex <> 0 Then
            txtMode.Text = cmbMode.SelectedValue
        End If
    End Sub

    <System.Web.Services.WebMethod()> _
        Public Shared Function GetDeptInformation(ByVal _deptcode As String) As String
        Dim deptinfo As String = String.Empty
        Dim codesRepo As New IndLifeCodesRepository()
        'Dim crit As String = 

        Try
            deptinfo = codesRepo.GetDeptInfo(_deptcode)
            Return deptinfo
        Finally
            If deptinfo = "<NewDataSet />" Then
                Throw New Exception()
            End If
        End Try

    End Function

    <System.Web.Services.WebMethod()> _
        Public Shared Function GetTransType(ByVal _transcode As String) As String
        Dim transinfo As String = String.Empty
        Dim transRepo As New TransactionTypesRepository()
        'Dim crit As String = 

        Try
            transinfo = transRepo.GetTransInfo(_transcode)
            Return transinfo
        Finally
            If transinfo = "<NewDataSet />" Then
                Throw New Exception()
            End If
        End Try

    End Function

    'Protected Sub cmbBranchCode_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles cmbBranchCode.SelectedIndexChanged
    '    lblError.Text = ""

    '    If cmbBranchCode.SelectedIndex <> 0 Then
    '        txtBranchCode.Text = cmbBranchCode.SelectedValue
    '    End If
    'End Sub

    'Protected Sub cmbDept_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles cmbDept.SelectedIndexChanged
    '    lblError.Text = ""

    '    If cmbDept.SelectedIndex <> 0 Then
    '        txtDeptCode.Text = cmbDept.SelectedValue
    '    End If
    'End Sub

    'Protected Sub cmbCurrencyType_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles cmbCurrencyType.SelectedIndexChanged
    '    lblError.Text = ""

    '    If cmbCurrencyType.SelectedIndex <> 0 Then
    '        txtCurrencyCode.Text = cmbCurrencyType.SelectedValue
    '    End If
    'End Sub

    'Protected Sub cmbTransDetailType_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles cmbTransDetailType.SelectedIndexChanged
    '    lblError.Text = ""

    '    If cmbTransDetailType.SelectedIndex <> 0 Then
    '        txtTransTypeCode.Text = cmbTransDetailType.SelectedValue
    '    End If
    'End Sub

    'Protected Sub cmbDRCR_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles cmbDRCR.SelectedIndexChanged
    '    lblError.Text = ""
    '    If cmbDRCR.SelectedIndex <> 0 Then
    '        txtDRCR.Text = cmbDRCR.SelectedValue
    '    Else
    '        txtDRCR.Text = ""
    '    End If
    'End Sub

    Private Sub ValidateFields(ByRef ErrorInd)
        'Dim lblError.Text
        'If txtReceiptNo.Text = "" Then
        '    lblError.Text = "Receipt number must not be empty"
        '    ErrorInd = "Y"
        '    publicMsgs = "javascript:alert('" + lblError.Text + "')"
        '    txtReceiptNo.Focus()
        '    lblError.Visible = True
        '    Exit Sub
        'End If
        If txtBatchNo.Text = "" Then
            lblError.Text = "Batch number must not be empty"
            ErrorInd = "Y"
            publicMsgs = "javascript:alert('" + lblError.Text + "')"
            txtBatchNo.Focus()
            lblError.Visible = True
            Exit Sub
        End If
        If txtBatchDate.Text = "" Then
            lblError.Text = "Batch date must not be empty"
            ErrorInd = "Y"
            publicMsgs = "javascript:alert('" + lblError.Text + "')"
            txtBatchDate.Focus()
            lblError.Visible = True
            Exit Sub
        End If
        If cmbMode.SelectedIndex = 0 Then
            lblError.Text = "Please select receipt mode"
            ErrorInd = "Y"
            publicMsgs = "javascript:alert('" + lblError.Text + "')"
            cmbMode.Focus()
            lblError.Visible = True
            Exit Sub
        End If
        'If cmbTransType.SelectedIndex = 0 Then
        '    lblError.Text = "Please select transaction type"
        '    ErrorInd = "Y"
        '    publicMsgs = "javascript:alert('" + lblError.Text + "')"
        '    cmbTransType.Focus()
        '    Exit Sub
        'End If
        If cmbDept.SelectedIndex = 0 Then
            lblError.Text = "Please select a department"
            ErrorInd = "Y"
            publicMsgs = "javascript:alert('" + lblError.Text + "')"
            cmbDept.Focus()
            lblError.Visible = True
            Exit Sub
        End If

        If txtEffectiveDate.Text = "" Then
            lblError.Text = "Receipt date must not be empty"
            ErrorInd = "Y"
            publicMsgs = "javascript:alert('" + lblError.Text + "')"
            txtEffectiveDate.Focus()
            lblError.Visible = True
            Exit Sub
        End If

        Dim str() As String
        str = DoDate_Process(txtEffectiveDate.Text, txtEffectiveDate)
        If (str(2) = Nothing) Then
            Dim errMsg = str(0).Insert(18, " receipt date, ")
            publicMsgs = errMsg.Replace("Javascript:alert('", "").Replace("');", "")
            lblError.Text = publicMsgs
            lblError.Visible = True
            txtEffectiveDate.Focus()
            ErrorInd = "Y"
            Exit Sub
        Else
            txtEffectiveDate.Text = str(2).ToString()
        End If

        If cmbMode.SelectedValue = "T" Then
            If txtTellerNo.Text = "" Then
                lblError.Text = "Teller number  must not be empty"
                ErrorInd = "Y"
                publicMsgs = "javascript:alert('" + lblError.Text + "')"
                txtTellerNo.Focus()
                lblError.Visible = True
                Exit Sub
            End If
            If txtTellerDate.Text = "" Then
                lblError.Text = "Teller date must not be empty"
                ErrorInd = "Y"
                publicMsgs = "javascript:alert('" + lblError.Text + "')"
                txtEffectiveDate.Focus()
                lblError.Visible = True
                Exit Sub
            End If
            str = DoDate_Process(txtTellerDate.Text, txtTellerDate)
            If (str(2) = Nothing) Then
                Dim errMsg = str(0).Insert(18, " Teller date, ")
                publicMsgs = errMsg.Replace("Javascript:alert('", "").Replace("');", "")
                lblError.Text = publicMsgs
                lblError.Visible = True
                txtTellerDate.Focus()
                ErrorInd = "Y"
                Exit Sub
            Else
                txtTellerDate.Text = str(2).ToString()
            End If
        End If
        If cmbMode.SelectedValue = "Q" Then
            If txtChequeNo.Text = "" Then
                lblError.Text = "Cheque number must not be empty"
                ErrorInd = "Y"
                publicMsgs = "javascript:alert('" + lblError.Text + "')"
                txtChequeNo.Focus()
                lblError.Visible = True
                Exit Sub
            End If

            If txtChequeDate.Text = "" Then
                lblError.Text = "Cheque date must not be empty"
                ErrorInd = "Y"
                publicMsgs = "javascript:alert('" + lblError.Text + "')"
                txtChequeDate.Focus()
                lblError.Visible = True
                Exit Sub
            End If

            str = DoDate_Process(txtChequeDate.Text, txtChequeDate)
            If (str(2) = Nothing) Then
                Dim errMsg = str(0).Insert(18, " Cheque date, ")
                publicMsgs = errMsg.Replace("Javascript:alert('", "").Replace("');", "")
                lblError.Text = publicMsgs
                lblError.Visible = True
                txtChequeDate.Focus()
                ErrorInd = "Y"
                Exit Sub
            Else
                txtChequeDate.Text = str(2).ToString()
            End If
        End If
        If cmbCurrencyType.SelectedIndex = 0 Then
            lblError.Text = "Please select currency type"
            ErrorInd = "Y"
            publicMsgs = "javascript:alert('" + lblError.Text + "')"
            cmbCurrencyType.Focus()
            lblError.Visible = True
            Exit Sub
        End If
        If txtTotalAmt.Text = "0.0" Then
            lblError.Text = "Total Amount must not be equal to 0.00"
            ErrorInd = "Y"
            publicMsgs = "javascript:alert('" + lblError.Text + "')"
            txtTotalAmt.Focus()
            lblError.Visible = True
            Exit Sub
        End If
        'If txtPayeeName.Text = "" Then
        '    lblError.Text = "Payee name must not be empty"
        '    ErrorInd = "Y"
        '    publicMsgs = "javascript:alert('" + lblError.Text + "')"
        '    txtPayeeName.Focus()
        '    lblError.Visible = True
        '    Exit Sub
        'End If
        'If cmbBranchCode.SelectedIndex = 0 Then
        '    lblError.Text = "Please select a branch"
        '    ErrorInd = "Y"
        '    publicMsgs = "javascript:alert('" + lblError.Text + "')"
        '    cmbBranchCode.Focus()
        '    lblError.Visible = True
        '    Exit Sub
        'End If
        If txtTransDesc.Text = "" Then
            lblError.Text = "Transaction description must not be empty"
            ErrorInd = "Y"
            publicMsgs = "javascript:alert('" + lblError.Text + "')"
            txtTransDesc.Focus()
            lblError.Visible = True
            Exit Sub
        End If
        If Not IsNumeric(txtTransAmt.Text) Then
            lblError.Text = "Trans Amount must be numeric"
            ErrorInd = "Y"
            publicMsgs = "javascript:alert('" + lblError.Text + "')"
            txtTransAmt.Focus()
            lblError.Visible = True
            Exit Sub
        End If
        If Not IsNumeric(txtTotalAmt.Text) Then
            lblError.Text = "Total Amount must be numeric"
            ErrorInd = "Y"
            publicMsgs = "javascript:alert('" + lblError.Text + "')"
            txtTotalAmt.Focus()
            lblError.Visible = True
            Exit Sub
        End If


        'If txtMainAcct.Text = "1020080010" Or txtMainAcct.Text = "1020080015" Or txtMainAcct.Text = "1020080020" Then
        '    If txtSubAcct.Text = "" Or txtSubAcct.Text = "000000" Then
        '        lblError.Text = "Total Amount must be numeric"
        '        ErrorInd = "Y"
        '        publicMsgs = "javascript:alert('" + lblError.Text + "')"
        '        txtTotalAmt.Focus()
        '        lblError.Visible = True
        '        Exit Sub
        '    End If
        'End If
    End Sub

    Protected Sub txtTransAmt_TextChanged(ByVal sender As Object, ByVal e As EventArgs) Handles txtTransAmt.TextChanged
        If IsNumeric(txtTransAmt.Text) Then
            txtTransAmt.Text = Format(txtTransAmt.Text, "Standard")
        End If
    End Sub

    Private Sub GetAcctDescription()
        Dim dt As DataSet = New DataSet()
        Dim recRep As New ReceiptsRepository()
        dt = recRep.GetAccountChartDetailsDataSet(txtSubAcct.Text, txtMainAcct.Text)
        If dt.Tables(0).Rows().Count <> 0 Then
            txtMainAcctDesc.Text = dt.Tables(0).Rows(0).Item("sMainDesc")
            txtSubAcctDesc.Text = dt.Tables(0).Rows(0).Item("sSubDesc")
            HidShowHide.Value = "hide"
        End If
    End Sub

    Function DoDate_Process(ByVal dateValue As String, ByVal ctrlId As Control) As String()
        Dim rtnMsg(3) As String
        Dim rtnMsg_ As String = Nothing

        'Checking fields for empty values
        If dateValue = "" Then
            rtnMsg_ = " Field is required!"
            rtnMsg(0) = rtnMsg_
            rtnMsg(1) = ctrlId.ID
            Return rtnMsg
        Else
            'Validate date
            Dim myarrData = Split(dateValue, "/")
            'If myarrData.Count <> 3 Then
            If myarrData.Length <> 3 Then
                rtnMsg_ = " Expecting full date in ddmmyyyy format ..."
                rtnMsg_ = "Javascript:alert('" & rtnMsg_ & "')"
                rtnMsg(0) = rtnMsg_
                rtnMsg(1) = ctrlId.ID
                Return rtnMsg
                'Exit Function
            End If
            Dim strMyDay = myarrData(0)
            Dim strMyMth = myarrData(1)
            Dim strMyYear = myarrData(2)

            strMyDay = CType(Format(Val(strMyDay), "00"), String)
            strMyMth = CType(Format(Val(strMyMth), "00"), String)
            strMyYear = CType(Format(Val(strMyYear), "0000"), String)

            Dim strMyDte = Trim(strMyDay) & "/" & Trim(strMyMth) & "/" & Trim(strMyYear)
            'dateValue = Trim(strMyDte)


            Dim blnStatusX = gnTest_TransDate(strMyDte)
            If blnStatusX = False Then
                rtnMsg_ = " is not a valid date..."
                rtnMsg_ = "Javascript:alert('" & rtnMsg_ & "');"
                rtnMsg(0) = rtnMsg_
                rtnMsg(1) = ctrlId.ID
                Return rtnMsg
                'Exit Function
            Else
                rtnMsg(2) = CType(strMyDte, String)
                Return rtnMsg
            End If
            dateValue = RTrim(strMyDte)
            'Exit Sub
        End If



        Return rtnMsg
    End Function

    Public Function gnTest_TransDate(ByVal MyFunc_Date As String) As Boolean

        On Error GoTo MyTestDate_Err1

        Dim pvbln As Boolean

        gnTest_TransDate = False
        pvbln = False

        'If Len(MyFunc_Date) = 10 And Mid(MyFunc_Date, 3, 1) = "/" And Mid(MyFunc_Date, 6, 1) = "/" Then
        'Else
        '    Return pvbln
        '    Exit Function
        'End If

        If (Len(MyFunc_Date) = 10) And _
           (Mid(MyFunc_Date, 3, 1) = "-" Or Mid(MyFunc_Date, 3, 1) = "/") And _
           (Mid(MyFunc_Date, 6, 1) = "-" Or Mid(MyFunc_Date, 6, 1) = "/") Then
        Else
            Return pvbln
            Exit Function
        End If

        Dim strDteMsg As String = "Invalid Date"
        Dim strDteErr As String = "0"
        Dim DteTst As Date

        Dim strDte_Start As String
        Dim strDte_End As String

        Dim strDteYY As String
        Dim strDteMM As String
        Dim strDteDD As String

        strDteMsg = ""
        strDteErr = "0"

        strDteMsg = ""
        strDteErr = "0"

        'MsgBox _
        ' "Left Xter. :" & Left(MyFunc_Date, 2) & vbCrLf & _
        ' "Mid Xter. :" & Mid(MyFunc_Date, 4, 2) & vbCrLf & _
        ' "Right Xter. :" & Right(MyFunc_Date, 4)

        'If MyFunc_Date = "__/__/____" Or _
        '   MyFunc_Date = "" Then
        '    MyTestDate_Trans = True
        '    Exit Function
        'End If

        strDteDD = Left(MyFunc_Date, 2)
        strDteMM = Mid(MyFunc_Date, 4, 2)
        strDteYY = Right(MyFunc_Date, 4)

        strDteDD = Trim(strDteDD)
        strDteMM = Trim(strDteMM)
        strDteYY = Trim(strDteYY)

        'If strDteDD = "" And _
        '   strDteMM = "" And _
        '   strDteYY = "" Then
        '    MyTestDate_Trans = True
        '    Exit Function
        'End If

        'If Val(Left(MyFunc_Date, 2)) = 0 And _
        '   Val(Mid(MyFunc_Date, 4, 2)) = 0 And _
        '   Val(Right(MyFunc_Date, 4)) = 0 Then
        '   MyTestDate_Trans = True
        '   Exit Function
        'End If

        If Trim(strDteDD) < "01" Or _
           Trim(strDteDD) > "31" Then
            strDteMsg = _
              "  -> Day < 01 or Day > 31 ..." & vbCrLf
            strDteErr = "1"
            'MsgBox "Day date error..."
        End If
        If Trim(strDteMM) < "01" Or _
           Trim(strDteMM) > "12" Then
            strDteMsg = strDteMsg & _
              "  -> Month < 01 or Month > 12 ..." & vbCrLf
            strDteErr = "1"
            'MsgBox "Month date error..."
        End If

        If Len(Trim(strDteYY)) < 4 Then
            strDteMsg = strDteMsg & _
              "  -> Year = 0 digit or Year < 4 digits..." & vbCrLf
            strDteErr = "1"
            'MsgBox "Year date error..." & Year(Now)
        End If


        strDte_Start = ""
        strDte_End = ""
        strDte_Start = MyFunc_Date
        strDte_End = MyFunc_Date

        Select Case Trim(strDteMM)
            Case "01", "03", "05", "07", "08", "10", "12"
                If Val(strDteDD) > 31 Then
                    strDteMsg = strDteMsg & _
                    "  -> Invalid day in month. Month <" & strDteMM & ">" & _
                    " ends in <" & " 31 " & ">" & _
                    ". Full Date: " & strDte_End & vbCrLf
                    strDteErr = "1"
                End If

            Case "02"
                If (Val(strDteYY) Mod 4) = 0 Then
                    If Val(strDteDD) > 29 Then
                        strDteMsg = strDteMsg & _
                            "  -> Invalid day in month. Month <" & strDteMM & ">" & _
                            " ends in <" & " 29 " & ">" & _
                            ". Full Date: " & strDte_End & vbCrLf
                        strDteErr = "1"
                    End If
                Else
                    If Val(strDteDD) > 28 Then
                        strDteMsg = strDteMsg & _
                            "  -> Invalid day in month. Month <" & strDteMM & ">" & _
                            " ends in <" & " 28 " & ">" & _
                            ". Full Date: " & strDte_End & vbCrLf
                        strDteErr = "1"
                    End If

                End If

            Case "04", "06", "09", "11"
                If Val(strDteDD) > 30 Then
                    strDteMsg = strDteMsg & _
                    "  -> Invalid day in month. Month <" & strDteMM & ">" & _
                    " ends in <" & " 30 " & ">" & _
                    ". Full Date: " & strDte_End & vbCrLf
                    strDteErr = "1"
                End If
        End Select


MyTestDate_01:
        If strDteErr <> "0" Then
            GoTo MyTestDate_Msg
        End If

        gnTest_TransDate = True
        pvbln = True

        Return pvbln
        Exit Function

MyTestDate_Msg:

        'Call gnASPNET_MsgBox(strDteMsg)
        'Call gnASPNET_MsgBox("Invalid date...")
        'Call gnASPNET_MsgBox_VB(strDteMsg)

        gnTest_TransDate = False
        pvbln = False

        Return pvbln
        Exit Function

MyTestDate_Err1:
        gnTest_TransDate = False
        pvbln = False

        Return pvbln

    End Function
End Class