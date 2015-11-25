Imports System.Drawing
Imports System.Drawing.Printing
Public Class FrmPrint
    Dim i As Integer = 15
    Dim ready As Boolean = True
    Dim img As Image
    Dim photoBoothPaper As New PaperSize("Photo Booth", 413, 950)
    Private Sub save()
        Dim tmpimg As New Bitmap(CInt(CInt(Me.Width / 2) / 2), Me.Height)

        Using g As Graphics = Graphics.FromImage(tmpimg)
            g.CopyFromScreen(Me.PointToScreen(New Point(0, 0)), New Point(0, 0), New Size(Me.Width, Me.Height))
        End Using

        tmpimg.Save("D:\tmp.jpg", System.Drawing.Imaging.ImageFormat.Jpeg)
    End Sub
    Private Sub FrmPrint_MouseClick(sender As Object, e As MouseEventArgs) Handles Me.MouseClick
        save()
        PrintFrm()
        exitFrm()
        frmWait.Show()
        frmWait.Timer1.Enabled = True
    End Sub
    Private Sub FrmPrint_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        PictureBox7.BringToFront()
        Me.FormBorderStyle = Windows.Forms.FormBorderStyle.None
        Me.WindowState = FormWindowState.Maximized
        Me.TopMost = True
        Cursor.Hide()
    End Sub
    Function PrintFrm()
        Dim Printer As PrintDocument = New PrintDocument

        AddHandler Printer.PrintPage, AddressOf PrintImage
        Printer.Print()

        Printer.DefaultPageSettings.PaperSize = photoBoothPaper

        exitFrm()

        Return Nothing
    End Function
    Private Sub PrintImage(ByVal sender As Object, ByVal e As PrintPageEventArgs)
        img = Image.FromFile("D:\tmp.jpg")

        e.Graphics.DrawImage(img, New Rectangle(0, 0, 950, 413))

        img.RotateFlip(RotateFlipType.Rotate90FlipNone)

        e.Graphics.DrawImage(img, New Rectangle(0, 0, 950, 413))
    End Sub
    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        i = i - 1

        If i = 0 Then
            PhotoBooth.Show()
            exitFrm()
            i = 15
            Timer1.Enabled = False
        Else
            Label4.Text = i & " seconds"
        End If
    End Sub
    Private Sub exitFrm()
        i = 15
        Timer1.Enabled = False
        Me.Hide()
        PhotoBooth.btnAddHandler.PerformClick()
        PhotoBooth.PictureBox1.Image = Nothing
        PhotoBooth.PictureBox2.Image = Nothing
        PhotoBooth.PictureBox3.Image = Nothing
        ' PhotoBooth.PictureBox4.Image = Nothing
    End Sub
    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
        If Label1.ForeColor = Color.Red Then
            Label1.ForeColor = Color.Black
            Label4.ForeColor = Color.Black
        Else
            Label1.ForeColor = Color.Red
            Label4.ForeColor = Color.Red
        End If
    End Sub
End Class