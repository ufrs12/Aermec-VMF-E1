using System;
using System.ComponentModel;
using System.Windows.Forms;
using EasyModbus;
using System.IO.Ports;
namespace ModbusCS
{
    public partial class Form1 : Form
    {
        int lo;
        int hi;
        int kol;
        private ModbusClient modbusClient;
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            RadioButton1.Checked = true;
            TextBox3.Text = "1";
            TextBox4.Text = "50";
            TextBox2.TextAlign = HorizontalAlignment.Center;
            TextBox3.TextAlign = HorizontalAlignment.Center;
            TextBox4.TextAlign = HorizontalAlignment.Center;
            dataGridView1.Rows.Add("Температура воздуха на фанкойле", "", "");
            dataGridView1.Rows.Add("Температура воздуха на панеле", "", "");
            dataGridView1.Rows.Add("Температура теплоносителя (хладагента)", "", "");
            dataGridView1.Rows.Add("Температура дополнительного датчика", "", "");
            dataGridView1.Rows.Add("Действующая уставка температуры", "", "");
            dataGridView1.Rows.Add("Уставка скорости вентилятора", "", "");
            dataGridView1.Rows.Add("Реальная скорость вентилятора", "", "");
            dataGridView1.Rows.Add("Уставка запуска вентилятора", "", "");
            dataGridView1.Rows.Add("Код аварии", "", "");
            dataGridView1.Rows.Add("Положение DIP-переключателей", "", "");
            dataGridView1.Rows.Add("", "", "");
            dataGridView1.Rows.Add("", "", "");
            dataGridView1.Rows.Add("", "", "");
            dataGridView1.Rows.Add("Манипуляции с адресом", "", "");
            dataGridView1.Rows.Add("", "", "");
            dataGridView1.Rows.Add("Уставка полученная по удаленке", "", "");
            dataGridView1.Rows.Add("", "", "");
            dataGridView1.Rows.Add("", "", "");
            dataGridView1.Rows.Add("", "", "");
            dataGridView1.Rows.Add("", "", "");
            dataGridView1.Rows.Add("Адрес", "", "");
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        }
        private void ComboBox1_Enter(object sender, EventArgs e)
        {
            ComboBox1.Items.Clear();
            foreach (String sp in SerialPort.GetPortNames())
            {
                ComboBox1.Items.Add(sp);
            }
        }
        private void RadioButton1_CheckedChanged(object sender, EventArgs e)
        {
            GroupBox1.Enabled = true;
            GroupBox2.Enabled = false;
            GroupBox3.Enabled = false;
        }
        private void RadioButton2_CheckedChanged(object sender, EventArgs e)
        {
            GroupBox1.Enabled = false;
            GroupBox2.Enabled = true;
            GroupBox3.Enabled = false;
        }
        private void RadioButton3_CheckedChanged(object sender, EventArgs e)
        {
            GroupBox1.Enabled = false;
            GroupBox2.Enabled = false;
            GroupBox3.Enabled = true;
        }
        private void Button5_Click(object sender, EventArgs e)
        {
            try
            {
                if (backgroundWorker1.IsBusy != true)
            {
                listBox2.Items.Clear();
                Button5.Text = "Остановить";
                if (RadioButton1.Checked == true)
                {
                    hi = Convert.ToInt32(TextBox2.Text);
                    lo = Convert.ToInt32(TextBox2.Text);
                }
                else if (RadioButton2.Checked == true)
                {
                    lo = Convert.ToInt32(TextBox3.Text);
                    hi = Convert.ToInt32(TextBox4.Text);
                }
                else if (RadioButton3.Checked == true)
                {
                    lo = 1;
                    hi = 247;
                }
                else
                {
                    hi = 0;
                    lo = 0;
                }
                ProgressBar1.Value = 0;
                ProgressBar1.Visible = true;
                ProgressBar1.Maximum = hi - lo + 1;
                backgroundWorker1.RunWorkerAsync();
                kol = 0;
            }
            else
            {
                backgroundWorker1.CancelAsync();
                Button5.Text = "Сканировать";
                Label7.Text = "";
                Label6.Text = "";
            }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Button5.Text = "Сканировать";
            }
        }
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            worker.WorkerSupportsCancellation = true;
            for (int i = lo; i <= hi; i++)
            {
                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    try
                    {
                        modbusClient = new ModbusClient(Convert.ToString(ComboBox1.SelectedItem))
                        {
                            UnitIdentifier = Convert.ToByte(i),
                            Baudrate = 19200,
                            Parity = System.IO.Ports.Parity.None,
                            StopBits = System.IO.Ports.StopBits.Two,
                            ConnectionTimeout = 200
                        };
                        modbusClient.Connect();
                        int[] adr = modbusClient.ReadHoldingRegisters(20, 1);
                        modbusClient.Disconnect();
                        if (adr[0] != 0)
                        {
                            listBox2.Items.Add(adr[0]);
                        }
                        kol += 1;
                        ProgressBar1.Value = kol;
                    }
                    catch (Exception ex)
                    {
                        if (modbusClient != null)
                        {
                            modbusClient.Disconnect();
                        }
                        MessageBox.Show(ex.Message);
                        break;
                    }
                }
            }
        }
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Button5.Text = "Сканировать";
            ProgressBar1.Visible = false;
            Label6.Text = "Опрошено адресов: " + Convert.ToString(kol) + ".";
            if (kol == 0)
            {
                Label7.Text = "Проверьте параметры";
            }
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            groupBox5.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            groupBox4.Enabled = false;
        }
    }
}