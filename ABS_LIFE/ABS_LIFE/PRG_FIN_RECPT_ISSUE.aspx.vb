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


Partial Public Class PRG_FIN_RECPT_ISSUE
    Inherits System.Web.UI.Page
    Dim rcRepo As ReceiptsRepository
    Dim indLifeEnq As IndLifeCodesRepository
    Dim prodEnq As ProductDetailsRepository
    Dim prodCatRepo As ProductCatRepository
    Dim polinfo As PolicyInfo
    Dim updateFlag As Boolean
    Dim strKey As String
    Dim strSchKey As String
    Dim newReceiptNo As String
    Dim newSerialNum As String
    Dim Rceipt As CustodianLife.Model.Receipts
    Protected publicMsgs As String = String.Empty
    Dim Err As String
    Dim strTmp_Value As String = ""
    Dim myarrData() As String
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        txtAssuredAddress.Attributes.Add("disabled", "disabled")
        txtMOP.Attributes.Add("disabled", "disabled")
        txtMOPDesc.Attributes.Add("disabled", "disabled")
        txtTransDesc2.Attributes.Add("disabled", "disabled")
        txtAgentCode.Attributes.Add("disabled", "disabled")
        txtPolRegularContrib.Attributes.Add("disabled", "disabled")
        txtInsuredCode.Attributes.Add("disabled", "disabled")
        txtAgentName.Attributes.Add("disabled", "disabled")
        txtMainAcctDebitDesc.Attributes.Add("disabled", "disabled")
        txtMainAcctCreditDesc.Attributes.Add("disabled", "disabled")
        txtSubAcctDebitDesc.Attributes.Add("disabled", "disabled")
        txtSubAcctCreditDesc.Attributes.Add("disabled", "disabled")
        txtAssuredName.Attributes.Add("disabled", "disabled")
        txtAgentName.Attributes.Add("disabled", "disabled")

        If Not Page.IsPostBack Then
            rcRepo = New ReceiptsRepository
            indLifeEnq = New IndLifeCodesRepository
            prodEnq = New ProductDetailsRepository
            prodCatRepo = New ProductCatRepository
            Session("rcRepo") = rcRepo
            updateFlag = False
            Session("updateFlag") = updateFlag

            strKey = Request.QueryString("idd")
            Session("rcId") = strKey

            'Company code value to be filled from login
            txtCompanyCode.Text = "001"
            '  txtEntryDate.Text = Now.Date.ToString()
            txtEntryDate.Text = Format(Now, "dd/MM/yyyy")
            txtEntryDate.ReadOnly = True
            lblError.Visible = False


            SetComboBinding(cmbBranchCode, indLifeEnq.GetById("L02", "003"), "CodeItem_CodeLongDesc", "CodeItem")
            SetComboBinding(cmbCurrencyType, indLifeEnq.GetById("L02", "017"), "CodeItem_CodeLongDesc", "CodeItem")
            'SetComboBinding(cboProductClass, prodCatRepo.ProductCategories(), "PrdtCode_Desc", "ProductCatCode")
            txtSubAcctDebit.Text = "000000"
            txtSubAcctCredit.Text = "000000"
            cmbBranchCode.SelectedValue = "1501"
            txtBranchCode.Text = cmbBranchCode.SelectedValue
            txtCurrencyCode.Text = cmbCurrencyType.SelectedValue
            If strKey IsNot Nothing Then
                fillValues()
            Else
                rcRepo = CType(Session("rcRepo"), ReceiptsRepository)
            End If

        Else 'post back
            txtPolRegularContrib.Text = txtPolRegularContribH.Value
            txtMainAcctDebitDesc.Text = txtMainAcctDebitDescH.Value
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
        End If

    End Sub

    Protected Sub butSave_Click(ByVal sender As Object, ByVal e As EventArgs) Handles butSave.Click, butSaveN.Click
        Dim msg As String = String.Empty
        lblError.Text = ""
        Dim Err = ""
        ValidateFields(Err)
        If Err = "Y" Then
            Exit Sub
        End If
        Try
            If Me.IsValid Then

                'this routine will persist only one object. 
                '1. The Receipt object

                updateFlag = CType(Session("updateFlag"), Boolean)
                If Not updateFlag Then 'if new record

                    'create a new instance of the Receipt object
                    Rceipt = New CustodianLife.Model.Receipts()
                    rcRepo = New ReceiptsRepository()
                    lblError.Visible = False
                    txtAgentCode.Enabled = True
                    Rceipt.AgentCode = txtAgentCode.Text
                    'txtAgentCode.Enabled = False

                    Rceipt.AmountFC = CType(txtReceiptAmtFC.Text, Decimal)
                    Rceipt.AmountLC = CType(txtReceiptAmtLC.Text, Decimal)

                    Rceipt.BankCode = txtBankGLCode.Text
                    Rceipt.BatchNo = CType(txtBatchNo.Text, Int32)
                    Rceipt.BranchCode = cmbBranchCode.SelectedValue.ToString
                    If Trim(txtChequeDate.Text).Length() > 0 Then
                        Rceipt.ChequeDate = ValidDate(txtChequeDate.Text)
                    Else
                        Rceipt.ChequeDate = #1/1/2014#
                    End If

                    Rceipt.ChequeInwardNo = txtChequeNo.Text
                    Rceipt.ChequeTellerNo = txtTellerNo.Text
                    Rceipt.CommissionApplicable = cmbCommissions.SelectedValue
                    Rceipt.CompanyCode = txtCompanyCode.Text

                    Rceipt.CurrencyType = cmbReceiptType.SelectedValue
                    Rceipt.EntryDate = Now.Date
                    Rceipt.InsuredCode = txtInsuredCode.Text

                    Rceipt.MainAccountCredit = txtMainAcctCredit.Text
                    Rceipt.MainAccountDebit = txtMainAcctDebit.Text
                    Rceipt.PayeeName = txtPayeeName.Text
                    Rceipt.PolicyPaymentMode = cmbMode.SelectedValue
                    Rceipt.PolicyRegularContribution = CType(txtPolRegularContrib.Text, Decimal)

                    Rceipt.PolicyPaymentMode = txtMOP.Text
                    Rceipt.ReceiptType = cmbReceiptType.SelectedValue
                    Rceipt.ReferenceNo = txtReceiptRefNo.Text

                    Dim docMonth As String = Right(txtBatchNo.Text, 2)
                    Dim docYear As String = Left(txtBatchNo.Text, 4)

                    'get new serial number
                    newSerialNum = rcRepo.GetNextSerialNumber("L01", "001", docMonth, docYear, " ", "12", "11")


                    'get new receipt number
                    newReceiptNo = rcRepo.GetNextSerialNumber("RCN", "002", "001", docYear, "IL-BR-", "12", "11")
                    txtReceiptNo.Text = Trim(newReceiptNo)
                    txtSerialNo.Text = newSerialNum
                    Rceipt.SerialNo = CType(newSerialNum, Int64)
                    Rceipt.SubAccountCredit = txtSubAcctCredit.Text
                    Rceipt.SubAccountDebit = txtSubAcctDebit.Text
                    Rceipt.DocNo = Trim(newReceiptNo)

                    Rceipt.TempTransNo = txtTempReceiptNo.Text
                    Rceipt.TranDescription1 = txtTransDesc1.Text
                    Rceipt.TranDescription2 = txtTransDesc2.Text
                    Rceipt.TransDate = ValidDate(txtEffectiveDate.Text)
                    Rceipt.TransMode = cmbMode.SelectedValue
                    Rceipt.TransType = cmbTransType.SelectedValue
                    Rceipt.CurrencyType = cmbCurrencyType.SelectedValue
                    Rceipt.LedgerTypeCredit = "T"
                    Rceipt.FileNo = txtFileNo.Text.Trim()
                    Rceipt.ProductCode = txtProductCode.Text.Trim()
                    Rceipt.Flag = "A"
                    Rceipt.OperId = "001"
                    Rceipt.ProcDate = txtBatchNo.Text.Trim
                    Rceipt.TempPolicyNo = txtTempPolNo.Text.Trim()
                    Rceipt.TempInsuredName = txtTempInsName.Text.Trim()
                    Rceipt.TempProdCode = cboProduct.SelectedValue
                    Rceipt.TempProdCat = cboProductClass.SelectedValue


                    rcRepo.Save(Rceipt)
                    Session("Rceipt") = Rceipt
                    msg = "Save Operation Successful"
                    lblError.Text = msg
                    lblError.Visible = True
                    publicMsgs = "javascript:alert('" + msg + "')"

                Else
                    Rceipt = CType(Session("Rceipt"), CustodianLife.Model.Receipts)
                    rcRepo = CType(Session("rcRepo"), ReceiptsRepository)

                    txtAgentCode.Enabled = True
                    Rceipt.AgentCode = txtAgentCode.Text
                    txtAgentCode.Enabled = False
                    Rceipt.AmountFC = CType(txtReceiptAmtFC.Text, Decimal)
                    Rceipt.AmountLC = CType(txtReceiptAmtLC.Text, Decimal)

                    Rceipt.BankCode = txtBankGLCode.Text
                    Rceipt.BatchNo = CType(txtBatchNo.Text, Int32)
                    Rceipt.BranchCode = cmbBranchCode.SelectedValue.ToString
                    If Trim(txtChequeDate.Text).Length() > 0 Then
                        Rceipt.ChequeDate = ValidDate(txtChequeDate.Text)
                    Else
                        Rceipt.ChequeDate = #1/1/2014#
                    End If


                    Rceipt.ChequeInwardNo = txtChequeNo.Text
                    Rceipt.ChequeTellerNo = txtTellerNo.Text
                    Rceipt.CommissionApplicable = cmbCommissions.SelectedValue
                    Rceipt.CompanyCode = txtCompanyCode.Text

                    Rceipt.CurrencyType = cmbCurrencyType.SelectedValue
                    '  Rceipt.EntryDate = CType(txtEntryDate.Text, Date)
                    Rceipt.InsuredCode = txtInsuredCode.Text


                    Rceipt.MainAccountCredit = txtMainAcctCredit.Text
                    Rceipt.MainAccountDebit = txtMainAcctDebit.Text
                    Rceipt.PayeeName = txtPayeeName.Text
                    Rceipt.PolicyPaymentMode = cmbMode.SelectedValue
                    Rceipt.PolicyRegularContribution = CType(txtPolRegularContrib.Text, Decimal)

                    Rceipt.PolicyPaymentMode = txtMOP.Text
                    Rceipt.ReceiptType = cmbReceiptType.SelectedValue
                    Rceipt.ReferenceNo = txtReceiptRefNo.Text
                    Rceipt.SubAccountCredit = txtSubAcctCredit.Text
                    Rceipt.SubAccountDebit = txtSubAcctDebit.Text

                    Rceipt.TempTransNo = txtTempReceiptNo.Text
                    Rceipt.TranDescription1 = txtTransDesc1.Text
                    Rceipt.TranDescription2 = txtTransDesc2.Text
                    Rceipt.TransDate = ValidDate(txtEffectiveDate.Text)
                    Rceipt.TransMode = cmbMode.SelectedValue
                    Rceipt.TransType = cmbTransType.SelectedValue
                    Rceipt.CurrencyType = cmbCurrencyType.SelectedValue
                    Rceipt.LedgerTypeCredit = "T"
                    Rceipt.FileNo = txtFileNo.Text.Trim()
                    Rceipt.ProductCode = txtProductCode.Text.Trim()
                    Rceipt.Flag = "A"
                    Rceipt.OperId = "001"
                    Rceipt.ProcDate = txtBatchNo.Text.Trim
                    Rceipt.TempPolicyNo = txtTempPolNo.Text.Trim()
                    Rceipt.TempInsuredName = txtTempInsName.Text.Trim()
                    Rceipt.TempProdCode = cboProduct.SelectedValue
                    Rceipt.TempProdCat = cboProductClass.SelectedValue

                   

                    rcRepo.Save(Rceipt)
                    msg = "Save Operation Successful"
                    lblError.Text = msg
                    lblError.Visible = True
                    publicMsgs = "javascript:alert('" + msg + "')"
                End If
                
                initializeFields()
                txtReceiptNo.Enabled = False
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
    End Sub


    Private Sub fillValues()

        strKey = CType(Session("rcId"), String)
        rcRepo = CType(Session("rcRepo"), ReceiptsRepository)
        Rceipt = rcRepo.GetById(strKey)

        Session("Rceipt") = Rceipt
        If Rceipt IsNot Nothing Then
            'txtReceiptNo.Enabled = True

            txtAgentCode.Text = Rceipt.AgentCode
            'txtReceiptAmtFC.Text = Math.Round(Rceipt.AmountFC, 2).ToString()
            'txtReceiptAmtLC.Text = Math.Round(Rceipt.AmountLC, 2).ToString()

            txtReceiptAmtFC.Text = Format(Rceipt.AmountFC, "Standard")
            txtReceiptAmtLC.Text = Format(Rceipt.AmountLC, "Standard")

            txtBankGLCode.Text = Rceipt.BankCode
            txtBatchNo.Text = Rceipt.BatchNo
            cmbBranchCode.SelectedValue = Rceipt.BranchCode
            txtChequeDate.Text = ValidDateFromDB(Rceipt.ChequeDate)


            txtChequeNo.Text = Rceipt.ChequeInwardNo
            txtTellerNo.Text = Rceipt.ChequeTellerNo
            cmbCommissions.SelectedValue = Rceipt.CommissionApplicable
            txtCompanyCode.Text = Rceipt.CompanyCode

            cmbCurrencyType.SelectedValue = Rceipt.CurrencyType
            txtEntryDate.Text = ValidDateFromDB(Rceipt.EntryDate)
            txtInsuredCode.Text = Rceipt.InsuredCode

            txtMainAcctCredit.Text = Rceipt.MainAccountCredit
            txtMainAcctDebit.Text = Rceipt.MainAccountDebit
            txtPayeeName.Text = Rceipt.PayeeName
            cmbMode.SelectedValue = Rceipt.PolicyPaymentMode
            'txtPolRegularContrib.Text = Math.Round(Rceipt.PolicyRegularContribution, 2)
            txtPolRegularContrib.Text = Format(Rceipt.PolicyRegularContribution, "Standard")


            txtMOP.Text = Rceipt.PolicyPaymentMode
            cmbReceiptType.SelectedValue = Rceipt.ReceiptType
            txtReceiptRefNo.Text = Rceipt.ReferenceNo
            txtReceiptNo.Text = Rceipt.DocNo
            Err = ""
            txtSerialNo.Text = Rceipt.SerialNo
            If Rceipt.SubAccountCredit = "" Then
                txtSubAcctCredit.Text = "000000"
            Else
                txtSubAcctCredit.Text = Rceipt.SubAccountCredit
            End If
            If Rceipt.SubAccountDebit = "" Then
                txtSubAcctDebit.Text = "000000"
            Else
                txtSubAcctDebit.Text = Rceipt.SubAccountDebit
            End If

            If txtMainAcctDebit.Text <> "" Then
                GetAcctDescriptionDR()
            End If

            If txtMainAcctCredit.Text <> "" Then
                GetAcctDescriptionCR()
            End If

            txtTempReceiptNo.Text = Rceipt.TempTransNo
            txtTransDesc1.Text = Rceipt.TranDescription1
            txtTransDesc2.Text = Rceipt.TranDescription2
            txtEffectiveDate.Text = ValidDateFromDB(Rceipt.TransDate)
            cmbMode.SelectedValue = Rceipt.TransMode
            cmbTransType.SelectedValue = Rceipt.TransType
            cmbCurrencyType.SelectedValue = Rceipt.CurrencyType

            txtBranchCode.Text = cmbBranchCode.SelectedValue
            txtCurrencyCode.Text = cmbCurrencyType.Text
            txtReceiptCode.Text = cmbReceiptType.SelectedValue
            txtMode.Text = cmbMode.SelectedValue

            txtFileNo.Text = Rceipt.FileNo
            txtProductCode.Text = Rceipt.ProductCode
            updateFlag = True
            Session("updateFlag") = updateFlag
            If (txtReceiptNo.Text <> "" And (cmbReceiptType.SelectedValue = "P" Or cmbReceiptType.SelectedValue = "D")) Then
                GetPolicyInfos(Err)
                If Err = "Y" Then
                    Exit Sub
                End If
            End If
        End If

    End Sub
    Protected Sub initializeFields()
        'txtReceiptNo.Enabled = False
        txtPolRegularContribH.Value = "0.00"
        txtMainAcctDebitDescH.Value = ""
        txtAgentCode.Text = String.Empty
        txtReceiptAmtFC.Text = String.Empty
        txtReceiptAmtLC.Text = String.Empty

        txtBankGLCode.Text = String.Empty
        txtBatchNo.Text = String.Empty
        'cmbBranchCode.SelectedIndex = 0
        cmbBranchCode.Text = "1501"
        txtChequeDate.Text = String.Empty


        txtChequeNo.Text = String.Empty
        txtTellerNo.Text = String.Empty
        cmbCommissions.SelectedIndex = 0
        txtCompanyCode.Text = "001"

        cmbReceiptType.SelectedIndex = 0
        'cmbReceiptCode.Text = String.Empty
        txtReceiptRefNo.Text = String.Empty
        ' txtEntryDate.Text = String.Empty
        txtInsuredCode.Text = String.Empty

        txtMainAcctCredit.Text = String.Empty
        txtMainAcctDebit.Text = String.Empty
        txtPayeeName.Text = String.Empty
        cmbMode.SelectedIndex = 0
        txtPolRegularContrib.Text = String.Empty

        txtMOP.Text = String.Empty
        cmbReceiptType.SelectedIndex = 0
        txtReceiptRefNo.Text = String.Empty
        txtSerialNo.Text = String.Empty
        txtSubAcctCredit.Text = String.Empty
        txtSubAcctDebit.Text = String.Empty

        txtTempReceiptNo.Text = String.Empty
        txtTransDesc1.Text = String.Empty
        txtTransDesc2.Text = String.Empty
        txtEffectiveDate.Text = String.Empty
        cmbTransType.SelectedIndex = 0
        cmbCurrencyType.SelectedIndex = 0

        'txtReceiptNo.Text = String.Empty
        txtSubAcctDebitDesc.Text = String.Empty
        txtSubAcctCreditDesc.Text = String.Empty

        txtMode.Text = String.Empty
        'txtBranchCode.Text = String.Empty
        txtReceiptCode.Text = String.Empty
        'txtCurrencyCode.Text = String.Empty
        txtMainAcctDebitDesc.Text = String.Empty
        txtAssuredName.Text = String.Empty
        txtAgentName.Text = String.Empty
        txtAssuredAddress.Text = String.Empty
        txtMOPDesc.Text = String.Empty
        cmbBranchCode.SelectedValue = "1501"
        txtBranchCode.Text = cmbBranchCode.SelectedValue
        txtCurrencyCode.Text = cmbCurrencyType.SelectedValue

        txtTempInsName.Text = String.Empty
        txtTempPolNo.Text = String.Empty
        cboProductClass.SelectedIndex = 0
        cboProduct.SelectedIndex = 0
        txtProductClass.Text = cboProductClass.SelectedValue
        txtProduct.Text = cboProduct.SelectedValue
        txtEntryDate.Text = Format(Now, "dd/MM/yyyy")
        'notFound.Visible = True
        updateFlag = False
        Session("updateFlag") = updateFlag 'ready for a new record

        ' grdData.DataBind()

    End Sub



    Protected Sub butDelete_Click(ByVal sender As Object, ByVal e As EventArgs) Handles butDelete.Click
        Dim msg As String = String.Empty
        Rceipt = CType(Session("Rceipt"), CustodianLife.Model.Receipts)
        rcRepo = CType(Session("rcRepo"), ReceiptsRepository)
        Try
            rcRepo.Delete(Rceipt)
        Catch ex As Exception
            msg = ex.Message
            lblError.Text = msg
            lblError.Visible = True
            publicMsgs = "javascript:alert('" + msg + "')"
        End Try
        initializeFields()
        txtReceiptNo.Enabled = False


    End Sub

    Protected Sub txtReceiptRefNo_TextChanged(ByVal sender As Object, ByVal e As EventArgs) Handles txtReceiptRefNo.TextChanged

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
    Public Shared Function PaymentsPeriodCover(ByVal _polnum As String, ByVal _mop As String, ByVal _effdate As String, ByVal _contrib As String, ByVal _amtpaid As String) As String
        Dim paycover As String = String.Empty
        Dim rRepo As New ReceiptsRepository()

        Try
            paycover = rRepo.GetPaymentCover(_polnum, _mop, _effdate, _contrib, CDbl(_amtpaid))
            Return paycover
        Finally
            If paycover = "<NewDataSet />" Then
                Throw New Exception()
            End If
        End Try

    End Function
    <System.Web.Services.WebMethod()> _
    Public Shared Function GetBranchInformation(ByVal _branchcode As String) As String
        Dim branchinfo As String = String.Empty
        Dim recRepo As New ReceiptsRepository()
        'Dim crit As String = 

        Try
            branchinfo = recRepo.GetBranchInfo(_branchcode)
            Return branchinfo
        Finally
            If branchinfo = "<NewDataSet />" Then
                Throw New Exception()
            End If
        End Try

    End Function

    <System.Web.Services.WebMethod()> _
     Public Shared Function GetCurrencyInformation(ByVal _currencycode As String) As String
        Dim currencycode As String = String.Empty
        Dim recRepo As New ReceiptsRepository()
        'Dim crit As String = 

        Try
            currencycode = recRepo.GetCurrencyType(_currencycode)
            Return currencycode
        Finally
            If currencycode = "<NewDataSet />" Then
                Throw New Exception()
            End If
        End Try

    End Function

    <System.Web.Services.WebMethod()> _
    Public Shared Function GetPolicyInformation(ByVal _polnum As String, ByVal _type As String) As String
        Dim polinfos As String = String.Empty
        Dim recRepo As New ReceiptsRepository()
        'Dim crit As String = 

        Try
            polinfos = recRepo.GetPolicyInfo(_polnum, _type)
            Return polinfos
        Finally
            If polinfos = "<NewDataSet />" Then
                Throw New Exception()
            End If
        End Try

    End Function

    <System.Web.Services.WebMethod()> _
    Public Shared Function GetAccountChartDetails(ByVal _accountsubcode As String, ByVal _accountmaincode As String) As String
        Dim acodes As String = String.Empty
        Dim recRepo As New ReceiptsRepository()
        'Dim crit As String = 

        Try
            acodes = recRepo.GetAccountChartDetails(_accountsubcode, _accountmaincode)
            Return acodes
        Finally
            If acodes = "<NewDataSet />" Then
                Throw New Exception()
            End If
        End Try

    End Function

    <System.Web.Services.WebMethod()> _
     Public Shared Function GetProductByCatCodeClient(ByVal _catCode As String) As String
        Dim ProductDetails As String = String.Empty
        Dim prodEnq As New ProductDetailsRepository()
        'Dim crit As String = 

        Try
            ProductDetails = prodEnq.GetProductByCatCodeClient(_catCode)
            Return ProductDetails
        Finally
            If ProductDetails = "<NewDataSet />" Then
                Throw New Exception()
            End If
        End Try
    End Function

    'Protected Sub csValidateCommissions_ServerValidate(ByVal source As Object, _
    '                                                   ByVal args As System.Web.UI.WebControls.ServerValidateEventArgs) _
    '                                                   Handles csValidateCommissions.ServerValidate
    '    If (cmbReceiptType.SelectedValue = "P" Or cmbReceiptType.SelectedValue = "D") Then
    '        If cmbCommissions.SelectedValue = "0" Then
    '            args.IsValid = False
    '        End If
    '    End If
    'End Sub

    Protected Sub txtPolRegularContrib_TextChanged(ByVal sender As Object, ByVal e As EventArgs) Handles txtPolRegularContrib.TextChanged

    End Sub

    Protected Sub txtTransDesc1_TextChanged(ByVal sender As Object, ByVal e As EventArgs) Handles txtTransDesc1.TextChanged

    End Sub

    Protected Sub butClose_Click(ByVal sender As Object, ByVal e As EventArgs) Handles butClose.Click
        Response.Redirect("ReceiptsList.aspx")
    End Sub

    'Protected Sub csValidateCurrencyType_ServerValidate(ByVal source As Object, _
    '                                                    ByVal args As System.Web.UI.WebControls.ServerValidateEventArgs) _
    '                                                    Handles csValidateCurrencyType.ServerValidate
    '    If (cmbCurrencyType.SelectedValue = "0") Then
    '        args.IsValid = False
    '    End If
    'End Sub

    Protected Sub butPrintReceipt_Click(ByVal sender As Object, ByVal e As EventArgs) Handles butPrintReceipt.Click
        If Len(Trim(txtRecptNo.Text)) = 0 Then
            publicMsgs = "javascript:alert('Error! Please enter a valid receipt number')"
        Else
            Session("rcPrintNo") = txtRecptNo.Text
            Response.Redirect("ReceiptPrint.aspx")
        End If
    End Sub

    Protected Sub txtMainAcctCreditDesc_TextChanged(ByVal sender As Object, ByVal e As EventArgs) Handles txtMainAcctCreditDesc.TextChanged

    End Sub

    Protected Sub txtMainAcctDebitDesc_TextChanged(ByVal sender As Object, ByVal e As EventArgs) Handles txtMainAcctDebitDesc.TextChanged

    End Sub

    Protected Sub txtReceiptNo_TextChanged(ByVal sender As Object, ByVal e As EventArgs) Handles txtReceiptNo.TextChanged
        If txtReceiptNo.Text <> "" Then
            initializeFields()
            lblError.Text = ""
            rcRepo = CType(Session("rcRepo"), ReceiptsRepository)
            Rceipt = rcRepo.GetByReceiptNo(Trim(txtReceiptNo.Text))

            Session("Rceipt") = Rceipt
            If Rceipt IsNot Nothing Then
                'txtReceiptNo.Enabled = True
                txtReceiptNo.Enabled = False
                chkReceiptNo.Checked = False
                txtAgentCode.Text = Rceipt.AgentCode
                'txtReceiptAmtFC.Text = Math.Round(Rceipt.AmountFC, 2).ToString()
                'txtReceiptAmtLC.Text = Math.Round(Rceipt.AmountLC, 2).ToString()

                txtReceiptAmtFC.Text = Format(Rceipt.AmountFC, "Standard")
                txtReceiptAmtLC.Text = Format(Rceipt.AmountLC, "Standard")

                txtBankGLCode.Text = Rceipt.BankCode
                txtBatchNo.Text = Rceipt.BatchNo
                cmbBranchCode.SelectedValue = Rceipt.BranchCode
                txtChequeDate.Text = ValidDateFromDB(Rceipt.ChequeDate)


                txtChequeNo.Text = Rceipt.ChequeInwardNo
                txtTellerNo.Text = Rceipt.ChequeTellerNo
                cmbCommissions.SelectedValue = Rceipt.CommissionApplicable
                txtCompanyCode.Text = Rceipt.CompanyCode

                cmbCurrencyType.SelectedValue = Rceipt.CurrencyType
                txtEntryDate.Text = ValidDateFromDB(Rceipt.EntryDate)
                txtInsuredCode.Text = Rceipt.InsuredCode

                txtMainAcctCredit.Text = Rceipt.MainAccountCredit
                txtMainAcctDebit.Text = Rceipt.MainAccountDebit
                txtPayeeName.Text = Rceipt.PayeeName
                'cmbMode.SelectedValue = Rceipt.PolicyPaymentMode
                'cmbMode.Text = Rceipt.PolicyPaymentMode
                ' txtPolRegularContrib.Text = Math.Round(Rceipt.PolicyRegularContribution, 2)
                txtPolRegularContrib.Text = Format(Rceipt.PolicyRegularContribution, "Standard")

                txtMOP.Text = Rceipt.PolicyPaymentMode
                cmbReceiptType.SelectedValue = Rceipt.ReceiptType
                txtReceiptRefNo.Text = Rceipt.ReferenceNo
                txtReceiptNo.Text = Rceipt.DocNo


                txtSerialNo.Text = Rceipt.SerialNo
                'txtSubAcctCredit.Text = Rceipt.SubAccountCredit
                'txtSubAcctDebit.Text = Rceipt.SubAccountDebit

                If Rceipt.SubAccountCredit = "" Then
                    txtSubAcctCredit.Text = "000000"
                Else
                    txtSubAcctCredit.Text = Rceipt.SubAccountCredit
                End If
                If Rceipt.SubAccountDebit = "" Then
                    txtSubAcctDebit.Text = "000000"
                Else
                    txtSubAcctDebit.Text = Rceipt.SubAccountDebit
                End If

                txtTempReceiptNo.Text = Rceipt.TempTransNo
                txtTransDesc1.Text = Rceipt.TranDescription1
                txtTransDesc2.Text = Rceipt.TranDescription2
                txtEffectiveDate.Text = ValidDateFromDB(Rceipt.TransDate)
                cmbMode.SelectedValue = Rceipt.TransMode
                cmbTransType.SelectedValue = Rceipt.TransType
                cmbCurrencyType.SelectedValue = Rceipt.CurrencyType

                txtBranchCode.Text = cmbBranchCode.SelectedValue
                txtCurrencyCode.Text = cmbCurrencyType.SelectedValue
                txtReceiptCode.Text = cmbReceiptType.SelectedValue
                txtMode.Text = cmbMode.SelectedValue

                txtFileNo.Text = Rceipt.FileNo
                txtProductCode.Text = Rceipt.ProductCode
                txtTempPolNo.Text = Rceipt.TempPolicyNo
                txtTempInsName.Text = Rceipt.TempInsuredName
                cboProductClass.SelectedValue = Rceipt.TempProdCat
                txtProductClass.Text = cboProductClass.SelectedValue
                prodEnq = New ProductDetailsRepository
                SetComboBinding(cboProduct, prodEnq.GetProductByCatCode(cboProductClass.SelectedValue), "ProductDesc", "ProductCode")
                cboProduct.SelectedValue = Rceipt.TempProdCode
                txtProduct.Text = cboProduct.SelectedValue
                'If txtTempInsName.Text = "" Then
                '    notFound.Visible = False
                'End If

                updateFlag = True
                Session("updateFlag") = updateFlag

                If txtMainAcctDebit.Text <> "" Then
                    GetAcctDescriptionDR()
                End If

                If txtMainAcctCredit.Text <> "" Then
                    GetAcctDescriptionCR()
                End If
                Err = ""
                If (txtReceiptNo.Text <> "" And (cmbReceiptType.SelectedValue = "P" Or cmbReceiptType.SelectedValue = "D")) Then
                    GetPolicyInfos(Err)
                    If Err = "Y" Then
                        Exit Sub
                    End If
                End If
            End If
        End If
    End Sub

    Protected Sub butPrint_Click(ByVal sender As Object, ByVal e As EventArgs) Handles butPrint.Click

    End Sub

    Protected Sub butNew_Click(ByVal sender As Object, ByVal e As EventArgs) Handles butNew.Click
        txtReceiptNo.Text = String.Empty
        initializeFields()
        txtReceiptNo.Enabled = False
    End Sub
    Protected Sub cmbCurrencyType_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles cmbCurrencyType.SelectedIndexChanged
        
    End Sub

    Protected Sub cmbBranchCode_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles cmbBranchCode.SelectedIndexChanged
       
    End Sub

    Protected Sub cmbMode_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles cmbMode.SelectedIndexChanged
       
    End Sub
    Protected Sub cmbReceiptType_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles cmbReceiptType.SelectedIndexChanged
       
    End Sub

    Protected Sub chkReceiptNo_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs) Handles chkReceiptNo.CheckedChanged
        If chkReceiptNo.Checked Then
            txtReceiptNo.Enabled = True
        Else
            txtReceiptNo.Enabled = False
        End If
    End Sub
    Protected Sub txtSerialNo_TextChanged(ByVal sender As Object, ByVal e As EventArgs) Handles txtSerialNo.TextChanged

    End Sub

    Protected Sub txtReceiptAmtLC_TextChanged(ByVal sender As Object, ByVal e As EventArgs) Handles txtReceiptAmtLC.TextChanged
        If ((txtReceiptAmtLC.Text <> "") And IsNumeric(txtReceiptAmtLC.Text)) Then
            txtReceiptAmtLC.Text = Format(txtReceiptAmtLC.Text, "Standard")
            txtReceiptAmtFC.Text = txtReceiptAmtLC.Text
            txtTransDesc2.Text = ""
            If cmbReceiptType.SelectedValue = "P" Or cmbReceiptType.SelectedValue = "D" Then
                Dim dt As DataSet = New DataSet()
                Dim recRep1 As New ReceiptsRepository()
                If txtPolRegularContrib.Text = "" Or txtPolRegularContrib.Text = "0.00" Then
                    'lblError.Text = "Regular Contrib cannot be 0 or empty, contact technical dept"
                    'lblError.Visible = True
                    'publicMsgs = "javascript:alert('" + lblError.Text + "')"
                    'Exit Sub
                Else
                    dt = recRep1.GetPaymentCoverDataSet(txtReceiptRefNo.Text, txtMOP.Text, txtPolicyEffDate.Text, CDbl(txtPolRegularContrib.Text), CDbl(txtReceiptAmtLC.Text))
                    If dt.Tables(0).Rows().Count <> 0 Then
                        If Not IsDBNull(dt.Tables(0).Rows(0).Item("sPeriodsCoverRange")) Then _
                                                        txtTransDesc2.Text = dt.Tables(0).Rows(0).Item("sPeriodsCoverRange")
                    Else
                        lblError.Text = "Load Periods Cover Not Found. Parameters Empty or Invalid. Please Re-Confirm"
                        lblError.Visible = True
                        publicMsgs = "javascript:alert('" + lblError.Text + "')"
                        Exit Sub
                    End If
                End If
            End If

        End If
    End Sub

    Private Sub ValidateFields(ByRef ErrorInd)
        Dim msg
        'If txtReceiptNo.Text = "" Then
        '    msg = "Receipt number must not be empty"
        '    ErrorInd = "Y"
        '    lblError.Text = msg
        '    lblError.Visible = True
        '    publicMsgs = "javascript:alert('" + msg + "')"
        '    txtReceiptNo.Focus()
        '    Exit Sub
        'End If
        If txtBatchNo.Text = "" Then
            msg = "Batch date must not be empty"
            ErrorInd = "Y"
            lblError.Text = msg
            lblError.Visible = True
            publicMsgs = "javascript:alert('" + msg + "')"
            txtBatchNo.Focus()
            Exit Sub
        End If
        If txtEffectiveDate.Text = "" Then
            msg = "Effective date must not be empty"
            ErrorInd = "Y"
            lblError.Text = msg
            lblError.Visible = True
            publicMsgs = "javascript:alert('" + msg + "')"
            txtEffectiveDate.Focus()
            Exit Sub
        End If

        Dim str() As String
        str = DoDate_Process(txtEffectiveDate.Text, txtEffectiveDate)
        If (str(2) = Nothing) Then
            Dim errMsg = str(0).Insert(18, " Effective date, ")
            msg = errMsg.Replace("Javascript:alert('", "").Replace("');", "")
            lblError.Text = msg
            publicMsgs = "javascript:alert('" + msg + "')"
            lblError.Visible = True
            txtEffectiveDate.Focus()
            ErrorInd = "Y"
            Exit Sub
        Else
            txtEffectiveDate.Text = str(2).ToString()
        End If
        If cmbMode.SelectedIndex = 0 Then
            msg = "Please select receipt mode"
            ErrorInd = "Y"
            lblError.Text = msg
            lblError.Visible = True
            publicMsgs = "javascript:alert('" + msg + "')"
            cmbMode.Focus()
            Exit Sub
        End If
        If cmbReceiptType.SelectedIndex = 0 Then
            msg = "Please select receipt type"
            ErrorInd = "Y"
            lblError.Text = msg
            lblError.Visible = True
            publicMsgs = "javascript:alert('" + msg + "')"
            cmbReceiptType.Focus()
            Exit Sub
        End If
        If txtReceiptRefNo.Text = "" Then
            msg = lblRefNo.Text & " must not be empty"
            ErrorInd = "Y"
            lblError.Text = msg
            lblError.Visible = True
            publicMsgs = "javascript:alert('" + msg + "')"
            txtReceiptRefNo.Focus()
            Exit Sub
        End If
        'If cmbReceiptType.SelectedValue = "P" Or cmbReceiptType.SelectedValue = "D" Then
        '    If txtInsuredCode.Text = "" Then
        '        msg = "Insured code must not be empty, Please contact technical dept to update record"
        '        ErrorInd = "Y"
        '        lblError.Text = msg
        '        lblError.Visible = True
        '        publicMsgs = "javascript:alert('" + msg + "')"
        '        txtInsuredCode.Focus()
        '        Exit Sub
        '    End If
        'End If
        If cmbMode.SelectedValue = "T" Then
            If txtTellerNo.Text = "" Then
                msg = "Teller no must not be empty"
                ErrorInd = "Y"
                lblError.Text = msg
                lblError.Visible = True
                publicMsgs = "javascript:alert('" + msg + "')"
                txtTellerNo.Focus()
                Exit Sub
            End If
        End If
        'If cmbCurrencyType.SelectedIndex = 0 Then
        '    msg = "Please select currency type"
        '    ErrorInd = "Y"
        '    publicMsgs = "javascript:alert('" + msg + "')"
        '    cmbCurrencyType.Focus()
        '    Exit Sub
        'End If
        'If cmbReceiptType.SelectedValue = "P" Or cmbReceiptType.SelectedValue = "D" Then
        '    If txtAgentCode.Text = "" Then
        '        msg = "Agent Code must not be empty, Please contact technical dept to update record"
        '        ErrorInd = "Y"
        '        lblError.Text = msg
        '        lblError.Visible = True
        '        publicMsgs = "javascript:alert('" + msg + "')"
        '        txtAgentCode.Focus()
        '        Exit Sub
        '    End If
        'End If

        If cmbMode.SelectedValue = "Q" Then
            If txtChequeNo.Text = "" Then
                msg = "Cheque no must not be empty"
                ErrorInd = "Y"
                lblError.Text = msg
                lblError.Visible = True
                publicMsgs = "javascript:alert('" + msg + "')"
                txtChequeNo.Focus()
                Exit Sub
            End If
            If txtChequeDate.Text = "" Then
                msg = "Cheque date must not be empty"
                ErrorInd = "Y"
                lblError.Text = msg
                lblError.Visible = True
                publicMsgs = "javascript:alert('" + msg + "')"
                txtChequeDate.Focus()
                Exit Sub
            End If
        End If
        If txtPayeeName.Text = "" Then
            msg = "Payee name must not be empty"
            ErrorInd = "Y"
            lblError.Text = msg
            lblError.Visible = True
            publicMsgs = "javascript:alert('" + msg + "')"
            txtPayeeName.Focus()
            Exit Sub
        End If
        If cmbReceiptType.SelectedValue = "P" Or cmbReceiptType.SelectedValue = "D" Then
            If txtTransDesc1.Text = "" Then
                msg = "Trans desc 1 must not be empty"
                ErrorInd = "Y"
                lblError.Text = msg
                lblError.Visible = True
                publicMsgs = "javascript:alert('" + msg + "')"
                txtTransDesc1.Focus()
                Exit Sub
            End If
        End If
        'If cmbReceiptType.SelectedValue = "P" Or cmbReceiptType.SelectedValue = "D" Then
        '    If txtPolRegularContrib.Text = "0.00" Then
        '        msg = "Policy Regular Contrib must not be equal to 0.00, Please contact technical dept to update record"
        '        ErrorInd = "Y"
        '        lblError.Text = msg
        '        lblError.Visible = True
        '        publicMsgs = "javascript:alert('" + msg + "')"
        '        txtPolRegularContrib.Focus()
        '        Exit Sub
        '    End If
        'End If
        If cmbCommissions.SelectedIndex = 0 Then
            msg = "Please select commission applicable"
            ErrorInd = "Y"
            lblError.Text = msg
            lblError.Visible = True
            publicMsgs = "javascript:alert('" + msg + "')"
            cmbCommissions.Focus()
            Exit Sub
        End If
        'If cmbReceiptType.SelectedValue = "P" Or cmbReceiptType.SelectedValue = "D" Then
        '    If txtMOP.Text = "" Then
        '        msg = "Mode of payment must not be empty, Please contact technical dept to update record"
        '        ErrorInd = "Y"
        '        lblError.Text = msg
        '        lblError.Visible = True
        '        publicMsgs = "javascript:alert('" + msg + "')"
        '        txtMOP.Focus()
        '        Exit Sub
        '    End If
        'End If
        If Not IsNumeric(txtReceiptAmtLC.Text) Then
            msg = "Receipt Amount LC must be numeric"
            ErrorInd = "Y"
            lblError.Text = msg
            lblError.Visible = True
            publicMsgs = "javascript:alert('" + msg + "')"
            txtReceiptAmtLC.Focus()
            Exit Sub
        End If
        If Not IsNumeric(txtReceiptAmtFC.Text) Then
            msg = "Receipt Amount FC must be numeric"
            ErrorInd = "Y"
            lblError.Text = msg
            lblError.Visible = True
            publicMsgs = "javascript:alert('" + msg + "')"
            txtReceiptAmtFC.Focus()
            Exit Sub
        End If

    End Sub

    Private Sub GetPolicyInfos(ByRef ErrorInd)
        Dim dt As DataSet = New DataSet()
        Dim recRep As New ReceiptsRepository()
        dt = recRep.GetPolicyInfoDataSet(txtReceiptRefNo.Text, cmbReceiptType.SelectedValue)
        If dt.Tables(0).Rows().Count <> 0 Then
            If Not IsDBNull(dt.Tables(0).Rows(0).Item("TBIL_POLY_ASSRD_CD")) Then _
                                            txtInsuredCode.Text = dt.Tables(0).Rows(0).Item("TBIL_POLY_ASSRD_CD")
            If Not IsDBNull(dt.Tables(0).Rows(0).Item("TBIL_POLY_AGCY_CODE")) Then _
                                            txtAgentCode.Text = dt.Tables(0).Rows(0).Item("TBIL_POLY_AGCY_CODE")
            If Not IsDBNull(dt.Tables(0).Rows(0).Item("Insured_Name")) Then _
                                            txtAssuredName.Text = dt.Tables(0).Rows(0).Item("Insured_Name")
            If Not IsDBNull(dt.Tables(0).Rows(0).Item("Insured_Address")) Then _
                                            txtAssuredAddress.Text = dt.Tables(0).Rows(0).Item("Insured_Address")
            If Not IsDBNull(dt.Tables(0).Rows(0).Item("Insured_Name")) Then _
                                            txtPayeeName.Text = dt.Tables(0).Rows(0).Item("Insured_Name")
            If Not IsDBNull(dt.Tables(0).Rows(0).Item("TBIL_POL_PRM_DTL_MOP_PRM_LC")) Then
                txtPolRegularContrib.Text = Format(dt.Tables(0).Rows(0).Item("TBIL_POL_PRM_DTL_MOP_PRM_LC"), "Standard")
                txtPolRegularContribH.Value = txtPolRegularContrib.Text
            End If
            If Not IsDBNull(dt.Tables(0).Rows(0).Item("Agent_Name")) Then _
                                            txtAgentName.Text = dt.Tables(0).Rows(0).Item("Agent_Name")
            If Not IsDBNull(dt.Tables(0).Rows(0).Item("Payment_Mode")) Then _
                                            txtMOP.Text = dt.Tables(0).Rows(0).Item("Payment_Mode")
            If Not IsDBNull(dt.Tables(0).Rows(0).Item("Payment_Mode_Desc")) Then _
                                            txtMOPDesc.Text = dt.Tables(0).Rows(0).Item("Payment_Mode_Desc")
            If Not IsDBNull(dt.Tables(0).Rows(0).Item("File_No")) Then _
                                            txtFileNo.Text = dt.Tables(0).Rows(0).Item("File_No")
            If Not IsDBNull(dt.Tables(0).Rows(0).Item("Product_Code")) Then _
                                            txtProductCode.Text = dt.Tables(0).Rows(0).Item("Product_Code")
            If Not IsDBNull(dt.Tables(0).Rows(0).Item("TBIL_POLICY_EFF_DT")) Then _
                                            txtPolicyEffDate.Text = dt.Tables(0).Rows(0).Item("TBIL_POLICY_EFF_DT")
            'If cmbReceiptType.SelectedValue = "P" Or cmbReceiptType.SelectedValue = "D" Then
            '    If (txtInsuredCode.Text = "" Or txtAgentCode.Text = "" Or txtPolRegularContrib.Text = "0.00" Or txtMOP.Text = "") Then
            '        Dim message = "Please contact technical department, record not completed for " & lblRefNo.Text & "no " & txtReceiptRefNo.Text
            '        ErrorInd = "Y"
            '        publicMsgs = "javascript:alert('" + message + "')"
            '        txtReceiptRefNo.Focus()
            '        Exit Sub
            '    End If
            'End If
        End If

    End Sub

    Private Sub GetAcctDescriptionDR()
        Dim dt As DataSet = New DataSet()
        Dim recRep As New ReceiptsRepository()
        dt = recRep.GetAccountChartDetailsDataSet(txtSubAcctDebit.Text, txtMainAcctDebit.Text)
        If dt.Tables(0).Rows().Count <> 0 Then
            txtMainAcctDebitDesc.Text = dt.Tables(0).Rows(0).Item("sMainDesc")
            txtSubAcctDebitDesc.Text = dt.Tables(0).Rows(0).Item("sSubDesc")
        End If
    End Sub

    Private Sub GetAcctDescriptionCR()
        Dim dt As DataSet = New DataSet()
        Dim recRep As New ReceiptsRepository()
        dt = recRep.GetAccountChartDetailsDataSet(txtSubAcctCredit.Text, txtMainAcctCredit.Text)
        If dt.Tables(0).Rows().Count <> 0 Then
            txtMainAcctCreditDesc.Text = dt.Tables(0).Rows(0).Item("sMainDesc")
            txtSubAcctCreditDesc.Text = dt.Tables(0).Rows(0).Item("sSubDesc")
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


        'If strDteYY < "1990" Then
        '   strDteMsg = strDteMsg & _
        '     "  -> Year < 1990..." & vbCrLf
        '   strDteErr = "1"
        '   'MsgBox "Year date error..." & Year(Now)
        'End If
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


        'Get the first day of a month
        '----------------------------
        'strDte_Start = DateSerial( _
        '  Format(Val(strDteYY), "0000"), _
        '  Format(Val(strDteMM), "00"), _
        '  Format(Val(1), "00"))

        'Get the end day of a month
        '--------------------------
        'strDte_End = DateSerial( _
        '  Format(Val(strDteYY), "0000"), _
        '  Format(Val(strDteMM) + 1, "00"), _
        '  Format(Val(0), "00"))


        'If Val(strDteDD) > Val(Mid(strDte_End, 4, 2)) Then
        '   strDteMsg = strDteMsg & _
        '     "  -> Invalid day in month. Month <" & strDteMM & ">" & _
        '     " ends in <" & Mid(strDte_End, 4, 2) & ">" & _
        '     ". Full Date: " & strDte_End & vbCrLf
        '   strDteErr = "1"
        '   'MsgBox "Day date error..."
        'End If


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

        'MsgBox( _
        '  "Error: Incorrect or Incomplete date... " & vbCrLf & vbCrLf & _
        '  "Check the following:" & vbCrLf & vbCrLf & _
        '  strDteMsg & vbCrLf & vbCrLf & _
        '  "Please enter correct date or pick from <DatePicker>.", , "Date" & " - " & MyFunc_Date)

        Return pvbln
        Exit Function

MyTestDate_Err1:
        gnTest_TransDate = False
        pvbln = False

        'If Err.Number = 13 Then
        '    MsgBox("Error: Invalid date...", , "Incorrect date")
        'Else
        '    MsgBox("Error: " & Err.Description & _
        '      "(" & Err.Number & ")", vbCritical, "Error " & " - " & MyFunc_Date)
        'End If

        Return pvbln

    End Function

    Protected Sub cboProductClass_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles cboProductClass.SelectedIndexChanged
        cboProduct.Items.Clear()
        Try
            If Me.cboProductClass.SelectedIndex = -1 Or Me.cboProductClass.SelectedIndex = 0 Or _
               Me.cboProductClass.SelectedItem.Value = "" Or Me.cboProductClass.SelectedItem.Value = "*" Then
                Exit Sub
            Else
                strTmp_Value = Me.cboProductClass.SelectedItem.Value
                txtProductClass.Text = Me.cboProductClass.SelectedItem.Value
                prodEnq = New ProductDetailsRepository
                'If myarrData.Count <> 2 Then
                '    Me.lblError.Text = "Missing or Invalid " & Me.lblProductClass.Text
                '    publicMsgs = "Javascript:alert('" & Me.lblError.Text & "')"
                '    Exit Sub
                'End If
                Dim li As ListItem
                li = New ListItem
                li.Text = "Select"
                li.Value = "0"
                cboProduct.Items.Add(li)
                SetComboBinding(cboProduct, prodEnq.GetProductByCatCode(strTmp_Value), "ProductDesc", "ProductCode")
            End If
        Catch ex As Exception

        End Try
    End Sub

    Protected Sub cboProduct_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles cboProduct.SelectedIndexChanged
        Try
            If Me.cboProduct.SelectedIndex = -1 Or Me.cboProduct.SelectedIndex = 0 Or _
               Me.cboProduct.SelectedItem.Value = "" Or Me.cboProduct.SelectedItem.Value = "*" Then
                Exit Sub
            Else
                strTmp_Value = Me.cboProduct.SelectedItem.Value
                txtProduct.Text = Me.cboProduct.SelectedItem.Value
            End If
        Catch ex As Exception

        End Try
    End Sub

    'Protected Sub txtProductClass_TextChanged(ByVal sender As Object, ByVal e As EventArgs) Handles txtProductClass.TextChanged
    '    If txtProductClass.Text <> "" Then
    '        cboProductClass.SelectedValue = txtProductClass.Text
    '        prodEnq = New ProductDetailsRepository
    '        SetComboBinding(cboProduct, prodEnq.GetProductByCatCode(txtProduct.Text), "ProductDesc", "ProductCode")
    '    End If
    'End Sub
End Class