Imports System.Runtime.InteropServices
Public Class PhotoBooth
    Const WM_CAP As Short = &H400S
    Const WM_CAP_DRIVER_CONNECT As Integer = WM_CAP + 10
    Const WM_CAP_DRIVER_DISCONNECT As Integer = WM_CAP + 11
    Const WM_CAP_EDIT_COPY As Integer = WM_CAP + 30
    Public Const WM_CAP_GET_STATUS As Integer = WM_CAP + 54
    Public Const WM_CAP_DLG_VIDEOFORMAT As Integer = WM_CAP + 41
    Const WM_CAP_SET_PREVIEW As Integer = WM_CAP + 50
    Const WM_CAP_SET_PREVIEWRATE As Integer = WM_CAP + 52
    Const WM_CAP_SET_SCALE As Integer = WM_CAP + 53
    Const WS_CHILD As Integer = &H40000000
    Const WS_VISIBLE As Integer = &H10000000
    Const SWP_NOMOVE As Short = &H2S
    Const SWP_NOSIZE As Short = 1
    Const SWP_NOZORDER As Short = &H4S
    Const HWND_BOTTOM As Short = 1
    Private DeviceID As Integer = 0
    Private hHwnd As Integer
    Dim MyForm As Form
    Dim DesignX As Integer
    Dim DesignY As Integer
    Declare Function SendMessage Lib "user32" Alias "SendMessageA" _
        (ByVal hwnd As Integer, ByVal wMsg As Integer, ByVal wParam As Integer, _
        ByRef lParam As CAPSTATUS) As Boolean
    Declare Function SendMessage Lib "user32" Alias "SendMessageA" _
       (ByVal hwnd As Integer, ByVal wMsg As Integer, ByVal wParam As Boolean, _
       ByRef lParam As Integer) As Boolean
    Declare Function SendMessage Lib "user32" Alias "SendMessageA" _
         (ByVal hwnd As Integer, ByVal wMsg As Integer, ByVal wParam As Integer, _
         ByRef lParam As Integer) As Boolean
    Declare Function SetWindowPos Lib "user32" Alias "SetWindowPos" (ByVal hwnd As Integer, _
        ByVal hWndInsertAfter As Integer, ByVal x As Integer, ByVal y As Integer, _
        ByVal cx As Integer, ByVal cy As Integer, ByVal wFlags As Integer) As Integer

    Dim seconds As Integer = 6
    Declare Function DestroyWindow Lib "user32" (ByVal hndw As Integer) As Boolean
    Dim sfdImage As SaveFileDialog
    Dim pictures(4) As PictureBox
    Dim i As Integer
    Dim data As IDataObject
    Dim bmap(10000) As Bitmap
    Dim numPics As Integer
    Dim ready As Boolean = True
    Structure POINTAPI
        Dim x As Integer
        Dim y As Integer
    End Structure
    Public Structure CAPSTATUS
        Dim uiImageWidth As Integer                    '// Width of the image
        Dim uiImageHeight As Integer                   '// Height of the image
        Dim fLiveWindow As Integer                     '// Now Previewing video?
        Dim fOverlayWindow As Integer                  '// Now Overlaying video?
        Dim fScale As Integer                          '// Scale image to client?
        Dim ptScroll As POINTAPI                    '// Scroll position
        Dim fUsingDefaultPalette As Integer            '// Using default driver palette?
        Dim fAudioHardware As Integer                  '// Audio hardware present?
        Dim fCapFileExists As Integer                  '// Does capture file exist?
        Dim dwCurrentVideoFrame As Integer             '// # of video frames cap'td
        Dim dwCurrentVideoFramesDropped As Integer     '// # of video frames dropped
        Dim dwCurrentWaveSamples As Integer            '// # of wave samples cap'td
        Dim dwCurrentTimeElapsedMS As Integer          '// Elapsed capture duration
        Dim hPalCurrent As Integer                     '// Current palette in use
        Dim fCapturingNow As Integer                   '// Capture in progress?
        Dim dwReturn As Integer                        '// Error value after any operation
        Dim wNumVideoAllocated As Integer              '// Actual number of video buffers
        Dim wNumAudioAllocated As Integer              '// Actual number of audio buffers
    End Structure
    Declare Function capCreateCaptureWindowA Lib "avicap32.dll" _
         (ByVal lpszWindowName As String, ByVal dwStyle As Integer, _
         ByVal x As Integer, ByVal y As Integer, ByVal nWidth As Integer, _
         ByVal nHeight As Short, ByVal hWndParent As Integer, _
         ByVal nID As Integer) As Integer
    Declare Function capGetDriverDescriptionA Lib "avicap32.dll" (ByVal wDriver As Short, _
        ByVal lpszName As String, ByVal cbName As Integer, ByVal lpszVer As String, _
        ByVal cbVer As Integer) As Boolean
    Private Sub PhotoBooth_Click(sender As Object, e As EventArgs) Handles Me.Click
        If ready = True Then
            Label1.Visible = False
            i = 0
            btnTakePics.PerformClick()
        End If
    End Sub
    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        LoadDeviceList()
        If lstDevices.Items.Count > 0 Then
            lstDevices.SelectedIndex = 0
        Else
            lstDevices.Items.Add("No Capture Device")
        End If

        btnStart.PerformClick()

        pictures(0) = PictureBox1
        pictures(1) = PictureBox2
        pictures(2) = PictureBox3
        pictures(3) = PictureBox4

        Me.FormBorderStyle = Windows.Forms.FormBorderStyle.None
        Me.WindowState = FormWindowState.Maximized
        Me.TopMost = True
        Cursor.Hide()

        Me.AutoScrollMinSize = New Size(100, 100)
        picCapture.SizeMode = PictureBoxSizeMode.StretchImage
        picCapture.BringToFront()
    End Sub
    Private Sub LoadDeviceList()
        Dim strName As String = Space(100)
        Dim strVer As String = Space(100)
        Dim bReturn As Boolean
        Dim x As Short = 0

        Do
            bReturn = capGetDriverDescriptionA(x, strName, 100, strVer, 100)

            If bReturn Then lstDevices.Items.Add(strName.Trim)
            x += CType(1, Short)
        Loop Until bReturn = False
    End Sub
    Private Sub OpenPreviewWindow()
        Dim iHeight As Integer = picCapture.Height
        Dim iWidth As Integer = picCapture.Width

        hHwnd = capCreateCaptureWindowA(DeviceID.ToString, WS_VISIBLE Or WS_CHILD, 0, 0, 1920, _
            1080, picCapture.Handle.ToInt32, 0)

        If SendMessage(hHwnd, WM_CAP_DRIVER_CONNECT, DeviceID, 0) Then
            SendMessage(hHwnd, WM_CAP_SET_SCALE, True, 0)

            SendMessage(hHwnd, WM_CAP_SET_PREVIEWRATE, 66, 0)

            SendMessage(hHwnd, WM_CAP_SET_PREVIEW, True, 0)

            SetWindowPos(hHwnd, HWND_BOTTOM, 0, 0, picCapture.Width, picCapture.Height, _
                    SWP_NOMOVE Or SWP_NOZORDER)

        Else
            DestroyWindow(hHwnd)
        End If
    End Sub
    Private Sub btnStart_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnStart.Click
        DeviceID = lstDevices.SelectedIndex
        OpenPreviewWindow()
        Dim bReturn As Boolean
        Dim s As CAPSTATUS
        bReturn = SendMessage(hHwnd, WM_CAP_GET_STATUS, Marshal.SizeOf(s), s)
        Debug.WriteLine(String.Format("Video Size {0} x {1}", s.uiImageWidth, s.uiImageHeight))
    End Sub
    Private Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave.Click
        SendMessage(hHwnd, WM_CAP_EDIT_COPY, 0, 0)

        data = Clipboard.GetDataObject()
        If data.GetDataPresent(GetType(System.Drawing.Bitmap)) Then
            bmap(numPics) = CType(data.GetData(GetType(System.Drawing.Bitmap)), Bitmap)
            Trace.Assert(Not (bmap(numPics) Is Nothing))
            pictures(i).Image = bmap(numPics)
            bmap(numPics).Save("D:\Pictures\Pic" & numPics & ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg)
            numPics += 1
        End If

        On Error Resume Next
    End Sub

    Private Sub btnTakePics_Click(sender As Object, e As EventArgs) Handles btnTakePics.Click
        ready = False

        For l = 1 To 3
            Timer1.Enabled = True

            My.Computer.Audio.Play("D:\Countdown.wav", AudioPlayMode.Background)

            For i As Integer = 0 To 20
                System.Threading.Thread.Sleep(100)
                Application.DoEvents()
            Next

            My.Computer.Audio.Play("D:\Shutter.wav", AudioPlayMode.WaitToComplete)

            i = l - 1

            btnSave.PerformClick()
        Next

        FrmPrint.PictureBox9.Image = bmap(numPics - 3)
        FrmPrint.PictureBox10.Image = bmap(numPics - 2)
        FrmPrint.PictureBox11.Image = bmap(numPics - 1)
        '   FrmPrint.PictureBox12.Image = bmap(numPics - 1)

        FrmPrint.Timer1.Enabled = True

        Me.Hide()
        FrmPrint.Show()
    End Sub
    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        If seconds > 1 Then

            seconds -= 1

            Label2.Text = seconds
        Else
            seconds = 6

            Label2.Text = ""

            Timer1.Enabled = False
        End If
    End Sub

    Private Sub btnAddHandler_Click(sender As Object, e As EventArgs) Handles btnAddHandler.Click
        ready = True
        Label1.Visible = True
    End Sub
End Class