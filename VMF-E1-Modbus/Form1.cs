using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using EasyModbus;
using System.IO.Ports;
namespace ModbusCS
{
    public partial class Form1 : Form
    {
        int lo;
        int hi;
        int kol;
        int adr_ch;
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
            dataGridView1.Rows.Add(0,"Температура воздуха на фанкойле", "", "");
            dataGridView1.Rows.Add(1,"Температура воздуха на панеле", "", "");
            dataGridView1.Rows.Add(2,"Температура теплоносителя (хладагента)", "", "");
            dataGridView1.Rows.Add(3,"Температура дополнительного датчика", "", "");
            dataGridView1.Rows.Add(4,"Действующая уставка температуры", "", "");
            dataGridView1.Rows.Add(5,"Уставка скорости вентилятора", "", "");
            dataGridView1.Rows.Add(6,"Реальная скорость вентилятора", "", "");
            dataGridView1.Rows.Add(7,"Уставка запуска вентилятора", "", "");
            dataGridView1.Rows.Add(8,"Код аварии", "", "");
            dataGridView1.Rows.Add(9,"Положение DIP-переключателей", "", "");
            dataGridView1.Rows.Add(10,"", "", "");
            dataGridView1.Rows.Add(11,"", "", "");
            dataGridView1.Rows.Add(12,"", "", "");
            dataGridView1.Rows.Add(13,"Манипуляции с адресом", "", "");
            dataGridView1.Rows.Add(14,"", "", "");
            dataGridView1.Rows.Add(15,"Уставка полученная по удаленке", "", "");
            dataGridView1.Rows.Add(16,"", "", "");
            dataGridView1.Rows.Add(17,"", "", "");
            dataGridView1.Rows.Add(18,"", "", "");
            dataGridView1.Rows.Add(19,"", "", "");
            dataGridView1.Rows.Add(20,"Адрес", "", "");
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
                            СountRetries = 0
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
            if (checkBox1.Checked == true && listBox2.Items.Count == 1)
            {
                groupBox7.Enabled = true;
            }
        }
        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            groupBox5.Enabled = true;
            groupBox6.Enabled = true;
            adr_ch = Convert.ToInt32(listBox2.SelectedItem);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (backgroundWorker2.IsBusy != true)
            {
                groupBox4.Enabled = false;
                groupBox6.Enabled = false;
                groupBox7.Enabled = false;
                label14.Visible = true;
                button1.Text = "Остановить опрос";
                backgroundWorker2.RunWorkerAsync();
            }
            else
            {
                backgroundWorker2.CancelAsync();
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                modbusClient = new ModbusClient(Convert.ToString(ComboBox1.SelectedItem))
                {
                    UnitIdentifier = Convert.ToByte(adr_ch),
                };
                modbusClient.Connect();
                modbusClient.WriteMultipleRegisters(13, new int[] { 4 });
                modbusClient.Disconnect();
            }
            catch (Exception ex)
            {
                if (modbusClient != null)
                {
                    modbusClient.Disconnect();
                }
                MessageBox.Show(ex.Message);
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
                try
                {
                    modbusClient = new ModbusClient(Convert.ToString(ComboBox1.SelectedItem))
                    {
                        UnitIdentifier = Convert.ToByte(adr_ch),
                    };
                    modbusClient.Connect();
                    modbusClient.WriteMultipleRegisters(13, new int[] {0});
                    modbusClient.Disconnect();
                }
                catch (Exception ex)
                {
                    if (modbusClient != null)
                    {
                        modbusClient.Disconnect();
                    }
                    MessageBox.Show(ex.Message);
                }
        }
        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            worker.WorkerSupportsCancellation = true;
            modbusClient = new ModbusClient(Convert.ToString(ComboBox1.SelectedItem))
            {
                UnitIdentifier = Convert.ToByte(adr_ch),
                СountRetries = Convert.ToInt16(textBox8.Text),
                ConnectionTimeout = Convert.ToInt16(textBox7.Text)
            };
            modbusClient.Connect();

            while (true)
            {
                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    modbusClient.Disconnect();
                    break;
                }
                else
                {
                    //try
                    //{
                        int[] regs = modbusClient.ReadHoldingRegisters(0, 21);
                        if (regs[20] != 0)
                        {

                            int a = 0;
                            foreach (int i in regs)
                            {
                                dataGridView1[3, a].Value = i;
                                a++;
                            }
                            if (Convert.ToInt32(dataGridView1[3, 0].Value) > 1500)
                            {
                                dataGridView1[2, 0].Value = "Трабл датчика";
                            }
                            else
                            {
                                dataGridView1[2, 0].Value = Convert.ToDecimal(dataGridView1[3, 0].Value) / 10;
                            }
                        if (Convert.ToInt32(dataGridView1[3, 1].Value) > 1500)
                        {
                            dataGridView1[2, 1].Value = "Трабл датчика";
                        }
                        else
                        {
                            dataGridView1[2, 1].Value = Convert.ToDecimal(dataGridView1[3, 1].Value) / 10;
                        }
                        if (Convert.ToInt32(dataGridView1[3, 2].Value) > 1500)
                        {
                            dataGridView1[2, 2].Value = "Трабл датчика";
                        }
                        else
                        {
                            dataGridView1[2, 2].Value = Convert.ToDecimal(dataGridView1[3, 2].Value) / 10;
                        }
                        if (Convert.ToInt32(dataGridView1[3, 3].Value) > 1500)
                        {
                            dataGridView1[2, 3].Value = "Трабл датчика";
                        }
                        else
                        {
                            dataGridView1[2, 3].Value = Convert.ToDecimal(dataGridView1[3, 3].Value) / 10;
                        }
                        if (Convert.ToInt32(dataGridView1[3, 4].Value) > 1500)
                        {
                            dataGridView1[2, 4].Value = "Трабл датчика";
                        }
                        else
                        {
                            dataGridView1[2, 4].Value = Convert.ToDecimal(dataGridView1[3, 4].Value) / 10;
                        }
                        if (label14.Text == "---")
                        {
                            label14.Text = @" \";
                        }
                        else
                        {
                            if (label14.Text == @" \")
                            {
                                label14.Text = " |";
                            }
                            else
                            {
                                if (label14.Text == " |")
                                {
                                    label14.Text = " /";
                                }
                                else
                                {
                                    label14.Text = "---";
                                }
                            }
                        }
                        }

                        else
                        {
                            groupBox5.Enabled = false;
                            groupBox6.Enabled = false;
                            groupBox7.Enabled = false;
                            break;
                        }
                    //}
                    //catch (Exception ex)
                    //{
                    //    if (modbusClient != null)
                    //    {
                    //        modbusClient.Disconnect();
                    //    }
                    //    MessageBox.Show(ex.Message);
                    //    modbusClient.Disconnect();
                    //    break;
                    //}
                    Thread.Sleep(Convert.ToInt16(textBox6.Text));
                }
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                modbusClient = new ModbusClient(Convert.ToString(ComboBox1.SelectedItem))
                {
                    UnitIdentifier = Convert.ToByte(0),
                };
                modbusClient.Connect();
                modbusClient.WriteMultipleRegisters(13, new int[] { 8 });
                modbusClient.Disconnect();
            }
            catch (Exception ex)
            {
                if (modbusClient != null)
                {
                    modbusClient.Disconnect();
                }
                MessageBox.Show(ex.Message);
            }
        }
        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                modbusClient = new ModbusClient(Convert.ToString(ComboBox1.SelectedItem))
                {
                    UnitIdentifier = Convert.ToByte(0),
                };
                modbusClient.Connect();
                modbusClient.WriteMultipleRegisters(20, new int[] { Convert.ToInt16(textBox1.Text) });
                modbusClient.WriteMultipleRegisters(13, new int[] { 4 });
                modbusClient.Disconnect();
            }
            catch (Exception ex)
            {
                if (modbusClient != null)
                {
                    modbusClient.Disconnect();
                }
                MessageBox.Show(ex.Message);
            }
        }
        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                modbusClient = new ModbusClient(Convert.ToString(ComboBox1.SelectedItem))
                {
                    UnitIdentifier = Convert.ToByte(0),
                };
                modbusClient.Connect();
                modbusClient.WriteMultipleRegisters(13, new int[] { 0 });
                modbusClient.Disconnect();
            }
            catch (Exception ex)
            {
                if (modbusClient != null)
                {
                    modbusClient.Disconnect();
                }
                MessageBox.Show(ex.Message);
            }
        }
        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                modbusClient = new ModbusClient(Convert.ToString(ComboBox1.SelectedItem))
                {
                    UnitIdentifier = Convert.ToByte(0),
                };
                modbusClient.Connect();
                modbusClient.WriteMultipleRegisters(13, new int[] { 4 });
                modbusClient.Disconnect();
            }
            catch (Exception ex)
            {
                if (modbusClient != null)
                {
                    modbusClient.Disconnect();
                }
                MessageBox.Show(ex.Message);
            }
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            button1.Text = "Начать опрос";
            label14.Visible = false;
            groupBox4.Enabled = true;
            groupBox6.Enabled = true;

        }
    }
}