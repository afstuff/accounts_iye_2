Public Partial Class RPT_FIN_MISSING_BROKER_CODE
    Inherits System.Web.UI.Page
    Dim rParams As String() = {"nw", "nw", "new", "new", "new", "new"}

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    Private Sub butOK_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles butOK.Click
        If (txtStartBatchNo.Text = "") Then
            Status.Text = "Start batch date must not be empty"
            Exit Sub
        End If
        If (txtEndBatchNo.Text = "") Then
            Status.Text = "End batch date must not be empty"
            Exit Sub
        End If

        Dim startBatchNo = Convert.ToInt32(txtStartBatchNo.Text)
        Dim endBatchNo = Convert.ToInt32(txtEndBatchNo.Text)

        If startBatchNo > endBatchNo Then
            Status.Text = "Start batch date must not be greater than end batch date"
            Exit Sub
        End If

        Dim url As String = HttpContext.Current.Request.Url.AbsoluteUri
        rParams(0) = "rptGetMissingBrokerCode"
        rParams(1) = "pStart_BatchDate="
        rParams(2) = startBatchNo.ToString() + "&"
        rParams(3) = "pEnd_BatchDate="
        rParams(4) = endBatchNo.ToString() + "&"
        rParams(5) = url

        Session("ReportParams") = rParams
        Response.Redirect("PrintView.aspx")
    End Sub

    Private Sub butClose_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles butClose.Click
        Response.Redirect("ReceiptOthersList.aspx")
    End Sub
End Class