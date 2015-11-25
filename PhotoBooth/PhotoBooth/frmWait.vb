Public Class frmWait
    Dim i As Integer = 20
    Private Sub frmWait_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.FormBorderStyle = Windows.Forms.FormBorderStyle.None
        Me.WindowState = FormWindowState.Maximized
        Me.TopMost = True
        Cursor.Hide()

        Label1.Left = Me.Width / 2 - (Label1.Width / 2)
        Label1.Top = Me.Height / 2 - (Label1.Height / 2)
    End Sub
    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        If i > 1 Then
            i = i - 1
        Else
            i = 20
            Timer1.Enabled = False
            Me.Hide()
            PhotoBooth.Show()
            PhotoBooth.btnAddHandler.PerformClick()
        End If
    End Sub
End Class