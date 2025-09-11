using RJCPPort = RJCP.IO.Ports;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace ilkADCreadFramework
{
    public partial class Form1 : Form
    {
        RJCPPort.SerialPortStream serialPort;
        private Chart chart;
        private Label statusLabel;
        private Queue<int> last20Values = new Queue<int>();

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Status Label
            statusLabel = new Label
            {
                Text = "Port status: Closed",
                Dock = DockStyle.Top,
                Height = 30
            };
            this.Controls.Add(statusLabel);

            // Chart oluştur
            chart = new Chart
            {
                Height = 400,
                Width = 1600,
                Location = new Point(10, 40),
            };
            chart.Anchor = AnchorStyles.Left;

            ChartArea area = new ChartArea("MainArea");
            area.AxisY.Minimum = 0;
            area.AxisY.Maximum = 1023;
            area.AxisX.Title = "Time";
            area.AxisX.TitleFont = new Font("Arial", 12, FontStyle.Bold);
            area.AxisX.TitleForeColor = Color.Blue;
            area.AxisY.TitleFont = new Font("Arial", 12, FontStyle.Bold);
            area.AxisY.TitleForeColor = Color.Red;
            area.AxisY.Title = "Value";
            chart.ChartAreas.Add(area);

            // ADC Series
            Series adcSeries = new Series("ADC")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.Blue
            };
            chart.Series.Add(adcSeries);

            // ECG Series
            Series ecgSeries = new Series("ECG")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.Green
            };
            chart.Series.Add(ecgSeries);

            this.Controls.Add(chart);

            // --- COM portu manuel belirtiyoruz ---
            OpenSerialPort("COM11", 115200);
        }

        private void OpenSerialPort(string portName, int baudRate)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                int retry = 0;
                bool success = false;

                while (!success && retry < 5)
                {
                    try
                    {
                        if (serialPort != null)
                        {
                            try { serialPort.Close(); } catch { }
                            serialPort.Dispose();
                        }

                        serialPort = new RJCPPort.SerialPortStream(portName, baudRate);
                        serialPort.DataReceived += SerialPort_DataReceived;
                        serialPort.Open();

                        Thread.Sleep(250); // ESP reset beklemesi

                        success = true;
                        this.Invoke((MethodInvoker)(() =>
                        {
                            statusLabel.Text = $"Port {portName} opened successfully at {baudRate} baud!";
                        }));
                    }
                    catch (Exception ex)
                    {
                        retry++;
                        this.Invoke((MethodInvoker)(() =>
                        {
                            statusLabel.Text = $"Port opening error: {ex.Message} Retry {retry}/5";
                        }));
                        Thread.Sleep(500);
                    }
                }

                if (!success)
                {
                    this.Invoke((MethodInvoker)(() =>
                    {
                        statusLabel.Text = $"Port {portName} couldn't open.";
                    }));
                }
            });
        }

        private void SerialPort_DataReceived(object sender, RJCPPort.SerialDataReceivedEventArgs e)
        {
            try
            {
                if (serialPort != null && serialPort.IsOpen)
                {
<<<<<<< HEAD
                    string AllData = serialPort.ReadExisting();
                    string[] lines = AllData.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
=======
                    string AllData = serialPort.ReadExisting();  // receive all the data
                    string[] lines = AllData.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);  // split the lines
>>>>>>> 1adff10fa07f49bbc00f5887ccec231b1a8e7ef8
                    string AllData = serialPort.ReadExisting();  // receive all the data
                    string[] lines = AllData.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);  // split the lines

                    this.Invoke((MethodInvoker)(() =>
                    {
                        foreach (var line in lines)
                        {
                            if (line.StartsWith("ADC:"))
                            {
                                string ADCvalue = line.Substring(4).Trim();
                                if (int.TryParse(ADCvalue, out int value))
                                {
                                    int scaledValue = value * 1023 / 4095;
                                    int nextX = chart.Series["ADC"].Points.Count;
                                    chart.Series["ADC"].Points.AddXY(nextX, scaledValue);

                                    if (scaledValue > 500)
                                        chart.Series["ADC"].Points[nextX].Color = Color.Red;
                                    else if (scaledValue > 400)
                                        chart.Series["ADC"].Points[nextX].Color = Color.Yellow;

                                    var area = chart.ChartAreas["MainArea"];
                                    area.AxisX.Minimum = Math.Max(0, nextX - 100);
                                    area.AxisX.Maximum = nextX;

                                    last20Values.Enqueue(scaledValue);
                                    if (last20Values.Count > 20) last20Values.Dequeue();

                                    listBox1.Items.Clear();
                                    foreach (var v in last20Values) listBox1.Items.Add(v);
                                    listBox1.TopIndex = listBox1.Items.Count - 1;
                                }
                            }
                            else if (line.StartsWith("ECG:"))
                            {
                                string ECGvalue = line.Substring(4).Trim();
                                if (int.TryParse(ECGvalue, out int ecgValue))
                                {
                                    int nextX = chart.Series["ECG"].Points.Count;
                                    chart.Series["ECG"].Points.AddXY(nextX, ecgValue);

                                    if (ecgValue > 500)
                                        chart.Series["ECG"].Points[nextX].Color = Color.Red;
                                    else if (ecgValue > 400)
                                        chart.Series["ECG"].Points[nextX].Color = Color.Yellow;

                                    var area = chart.ChartAreas["MainArea"];
                                    area.AxisX.Minimum = Math.Max(0, nextX - 100);
                                    area.AxisX.Maximum = nextX;
                                }
                            }
                            else if (line.StartsWith("BTN:"))
                            {
                                string butonCount = line.Substring(4).Trim();
                                listBox2.Items.Add(butonCount);
                                listBox2.TopIndex = listBox2.Items.Count - 1;
                            }
                            else if (line.StartsWith("LED:"))  
                            {
                                string LEDstate = line.Substring(4).Trim();
                                label3.Text = "LED is " + LEDstate;
                                label3.ForeColor = (LEDstate == "ON") ? Color.Green : Color.Red;
                            }
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Serial read error: " + ex.Message);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (serialPort != null)
            {
                try { serialPort.Close(); } catch { }
                serialPort.Dispose();
            }
            base.OnFormClosing(e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnLEDon_Click(object sender, EventArgs e)
        {
            serialPort.Write("1");
            label3.Text = "LED is ON";
            label3.ForeColor = Color.Green;
        }

        private void btnLEDoff_Click(object sender, EventArgs e)
        {
            serialPort.Write("0");
            label3.Text = "LED is OFF";
            label3.ForeColor = Color.Red;
        }
    }
}
