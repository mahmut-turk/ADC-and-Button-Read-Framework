using RJCP.IO.Ports;
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
        private SerialPortStream serialPort;
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

            // create a Chart
            chart = new Chart
            {
                //Dock = DockStyle.Top,
                Height = 400,
                Width = 1600,
                Location = new Point(10, 40),
            };
            chart.Anchor = AnchorStyles.Left;
            ChartArea area = new ChartArea("MainArea");
            area.AxisY.Minimum = 0;
            area.AxisY.Maximum = 1023;  
            area.AxisX.Title = "Time";
            area.AxisX.TitleFont= new Font("Arial", 12, FontStyle.Bold);
            area.AxisX.TitleForeColor = Color.Blue;
            area.AxisY.TitleFont = new Font("Arial", 12, FontStyle.Bold);
            area.AxisY.TitleForeColor = Color.Red;
            area.AxisY.Title = "ADC Value";
            chart.ChartAreas.Add(area);
            Series series = new Series("ADC")
            {
                ChartType = SeriesChartType.Line
            };
            chart.Series.Add(series);
            this.Controls.Add(chart);

            // try to open the COM port
            OpenSerialPort("COM10", 115200);
            
        }
  
        private void OpenSerialPort(string portName, int baudRate)      
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                int retry = 0;
                bool success = false;

                while (!success && retry < 2)
                {
                    try
                    {
                        if (serialPort != null)
                        {
                            try { serialPort.Close(); } catch { }
                            serialPort.Dispose();
                        }

                        serialPort = new SerialPortStream(portName, baudRate);
                        serialPort.DataReceived += SerialPort_DataReceived;
                        serialPort.Open();

                        // wait for Arduino reset
                        Thread.Sleep(250);

                        success = true;
                        this.Invoke((MethodInvoker)(() =>
                        {
                            statusLabel.Text = $"Port {portName} ist opened succesfull";
                            
                        }));
                    }
                    catch (Exception ex)
                    {
                        retry++;
                        this.Invoke((MethodInvoker)(() =>
                        {
                            statusLabel.Text = $"Port openning error: {ex.Message} Retry {retry}/2";
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

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    string data = serialPort.ReadExisting(); // tüm mevcut veriyi al
                    string[] lines = data.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

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
                            else if (line.StartsWith("BTN:"))
                            {
                                string butonCount = line.Substring(4).Trim();
                                listBox2.Items.Add(butonCount);
                                listBox2.TopIndex = listBox2.Items.Count - 1;
                            }
                            else if (line.StartsWith("LED:"))
                            {
                                string payload = line.Substring(4).Trim();
                                label3.Text = "LED is " + payload;
                                label3.ForeColor = (payload == "ON") ? Color.Green : Color.Red;
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
                serialPort.Dispose();   // free all resources
            }
            base.OnFormClosing(e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();     // button1 located on the form to close the application (location 1650;50) (size 240;80)
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
            label3.Text="LED is OFF";
            label3.ForeColor = Color.Red;
        }
    }
}