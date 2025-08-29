using System;
using System.Drawing;
using System.Linq.Expressions;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using RJCP.IO.Ports;

namespace ilkADCreadFramework
{
    public partial class Form1 : Form
    {
        private SerialPortStream serialPort;
        private Chart chart;
        private Label statusLabel;

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
            chart = new Chart { Dock = DockStyle.Fill };           
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
            OpenSerialPort("COM7", 9600);
            
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
                            Thread.Sleep(1000);
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
                    string line = serialPort.ReadLine();
                    if (int.TryParse(line, out int value))
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            int nextX = chart.Series["ADC"].Points.Count;
                            chart.Series["ADC"].Points.AddXY(nextX, value);
                            if (value > 500)
                                chart.Series["ADC"].Points[nextX].Color = Color.Red;
                            else if(value>400 && value<501)
                                chart.Series["ADC"].Points[nextX].Color = Color.Yellow;
                            ChartArea area = chart.ChartAreas["MainArea"];
                            area.AxisX.Minimum = Math.Max(0, nextX - 100);
                            area.AxisX.Maximum = nextX;
                        });
                    }
                }
            }
            catch { }
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
    }
}