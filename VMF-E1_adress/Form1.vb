Imports System.IO.Ports
Public Class Form1
    Private Sub ComboBox1_Enter(sender As Object, e As EventArgs) Handles ComboBox1.Enter
        ComboBox1.Items.Clear()
        For Each sp As String In My.Computer.Ports.SerialPortNames
            ComboBox1.Items.Add(sp)
        Next
    End Sub
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If Button1.Text = "Открыть порт" Then
            Try
                SerialPort1.PortName = ComboBox1.SelectedItem
                SerialPort1.BaudRate = 19200
                SerialPort1.Parity = 0
                SerialPort1.DataBits = 8
                SerialPort1.StopBits = StopBits.Two
                SerialPort1.Handshake = 0
                SerialPort1.Open()
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
End Class
