Imports System.IO.Ports
Imports EasyModbus
Public Class Form1
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
        If Button1.Text = "Сканировать" Then
            Dim h, l As Integer
            Try
                Button5.Text = "Остановить"
                For i As Integer = 1 To 50
                    ProgressBar1.Visible = True
                    ProgressBar1.Value = i
                    Try
                        modbusClient = New ModbusClient(ComboBox1.SelectedItem) With {
                        .UnitIdentifier = i,
                        .Parity = 0,
                        .StopBits = StopBits.Two,
                        .ConnectionTimeout = 200
                        }
                        modbusClient.Connect()
                        Dim ReadValues() As Integer = modbusClient.ReadHoldingRegisters(40021, 1)
                        ListBox1.Items.Add(ReadValues(0))
                        modbusClient.Disconnect()
                    Catch ex As Exception
                        'MessageBox.Show(ex.Message)
                        modbusClient.Disconnect()
                    End Try
                Next
                ProgressBar1.Visible = False
            Catch ex As Exception
                MessageBox.Show(ex.Message)
            End Try
        Else
            Button1.Text = "Сканировать"
        End If
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
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
End Class
