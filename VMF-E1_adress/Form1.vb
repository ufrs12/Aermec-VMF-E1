Imports System.IO.Ports
Imports EasyModbus
Imports System.ComponentModel
Public Class Form1

    Public Sub New()
        InitializeComponent()
        BackgroundWorker1.WorkerReportsProgress = True
        BackgroundWorker1.WorkerSupportsCancellation = True
    End Sub
    Dim modbusClient As ModbusClient
    Private Sub ComboBox1_Enter(sender As Object, e As EventArgs) Handles ComboBox1.Enter
        ComboBox1.Items.Clear()
        For Each sp As String In My.Computer.Ports.SerialPortNames
            ComboBox1.Items.Add(sp)
        Next
    End Sub
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        If Button1.Text = "Открыть порт" Then
            Try
                'SerialPort1.PortName = ComboBox1.SelectedItem
                'SerialPort1.BaudRate = 19200
                'SerialPort1.Parity = 0
                'SerialPort1.DataBits = 8
                'SerialPort1.StopBits = StopBits.Two
                'SerialPort1.Handshake = 0
                'SerialPort1.Open()
                Button1.Text = "Освободить порт"
                Button1.BackColor = Color.LightGreen
                ComboBox1.Enabled = False
            Catch ex As Exception
                MessageBox.Show(ex.Message)
            End Try
        Else
            If SerialPort1 IsNot Nothing AndAlso SerialPort1.IsOpen Then
                SerialPort1.Close()
                Button1.Text = "Открыть порт"
                Button1.BackColor = Color.Moccasin
                ComboBox1.Enabled = True
            End If
        End If
    End Sub
    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If SerialPort1 IsNot Nothing AndAlso SerialPort1.IsOpen Then
            SerialPort1.Close()
        End If
    End Sub
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        TextBox1.Text = SerialPort1.StopBits.ToString & SerialPort1.BaudRate.ToString & SerialPort1.Parity.ToString & SerialPort1.DataBits.ToString & SerialPort1.Handshake.ToString
    End Sub
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        'SerialPort1.Write("1710000D000102000440EE")
        Dim bytes() As Byte = FromHex("17 10 00 0D 00 01 02 00 04 40 EE")
        SerialPort1.Write(bytes, 0, bytes.Length)
    End Sub
    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        'SerialPort1.Write("1710000D0001020000412D")
        Dim bytes() As Byte = FromHex("17 10 00 0D 00 01 02 00 00 41 2D")
        SerialPort1.Write(bytes, 0, bytes.Length)
    End Sub
    Public Shared Function FromHex(ByVal hex As String) As Byte()
        hex = hex.Replace(" ", "")
        Dim raw((hex.Length \ 2) - 1) As Byte
        For i As Integer = 0 To raw.Length - 1
            raw(i) = Convert.ToByte(hex.Substring(i * 2, 2), 16)
        Next i
        Return raw
    End Function
    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        Label7.Text = ""
        If BackgroundWorker1.IsBusy <> True Then
            BackgroundWorker1.RunWorkerAsync()
            Button5.Text = "Остановить"
        Else
            BackgroundWorker1.CancelAsync()
            Button5.Text = "Сканировать"
        End If
    End Sub
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        CheckForIllegalCrossThreadCalls = False
        RadioButton1.Checked = True
        TextBox3.Text = 1
        TextBox4.Text = 50
        TextBox1.TextAlign = HorizontalAlignment.Center
        TextBox2.TextAlign = HorizontalAlignment.Center
        TextBox3.TextAlign = HorizontalAlignment.Center
        TextBox4.TextAlign = HorizontalAlignment.Center
    End Sub

    Private Sub RadioButton1_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton1.CheckedChanged
        GroupBox1.Enabled = True
        GroupBox2.Enabled = False
        GroupBox3.Enabled = False
    End Sub

    Private Sub RadioButton2_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton2.CheckedChanged
        GroupBox1.Enabled = False
        GroupBox2.Enabled = True
        GroupBox3.Enabled = False
    End Sub

    Private Sub RadioButton3_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton3.CheckedChanged
        GroupBox1.Enabled = False
        GroupBox2.Enabled = False
        GroupBox3.Enabled = True
    End Sub
    Private Sub backgroundWorker1_DoWork(ByVal sender As System.Object,
    ByVal e As DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        Dim worker As BackgroundWorker = CType(sender, BackgroundWorker)
        Dim hi, lo, kol As Integer
        Try
            If RadioButton1.Checked = True Then
                hi = TextBox2.Text
                lo = TextBox2.Text
            End If
            If RadioButton2.Checked = True Then
                lo = TextBox3.Text
                hi = TextBox4.Text
            End If
            If RadioButton3.Checked = True Then
                lo = 1
                hi = 247
            End If
            ProgressBar1.Value = 0
            ProgressBar1.Visible = True
            ProgressBar1.Maximum = hi - lo + 1
            kol = 0
        Catch ex As Exception
            BackgroundWorker1.CancelAsync()
            'Button5.Text = "Сканировать"
            'ProgressBar1.Visible = False
            'MessageBox.Show(ex.Message)
        End Try
        For i = lo To hi
            If (worker.CancellationPending = True) Then
                e.Cancel = True
                Exit For
            Else
                Try
                    modbusClient = New ModbusClient(ComboBox1.SelectedItem)
                Catch ex As Exception
                    Exit For
                End Try
                Try
                    modbusClient.UnitIdentifier = i
                    modbusClient.Parity = 0
                    modbusClient.StopBits = StopBits.Two
                    modbusClient.ConnectionTimeout = 200
                    modbusClient.Connect()
                    Dim ReadValues() As Integer = modbusClient.ReadHoldingRegisters(40021, 1)
                    ListBox1.Items.Add(ReadValues(0))
                    modbusClient.Disconnect()
                Catch ex As Exception
                    'MessageBox.Show(ex.Message)
                    If modbusClient IsNot Nothing AndAlso modbusClient.Connected Then
                        modbusClient.Disconnect()
                    End If
                End Try
                kol += 1
                ProgressBar1.Value = kol
            End If
        Next
        Button5.Text = "Сканировать"
        ProgressBar1.Visible = False
        Label6.Text = "Опрошено адресов: " & kol & "."
        If kol = 0 Then
            Label7.Text = "Проверьте параметры"
        End If
    End Sub
End Class
