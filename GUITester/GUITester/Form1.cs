using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace GUITester
{
    public partial class Form1 : Form
    {

        delegate void SetChartCallback(int count);

        private BackgroundWorker _bw = new BackgroundWorker();
        private BackgroundWorker _bw1 = new BackgroundWorker();
        private BackgroundWorker th = new BackgroundWorker();
        private BackgroundWorker th1 = new BackgroundWorker();

        public int count = 0;
        public int fibRess = 0;
        
        public Form1()
        {

            InitializeComponent();
            _bw = new BackgroundWorker();
            _bw.DoWork += new DoWorkEventHandler(_bw_DoWork);

            _bw1.DoWork += new DoWorkEventHandler(_bw1_DoWork);
            _bw1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_bw1_RunWorkerCompleted);

            th.DoWork += new DoWorkEventHandler(th_DoWork);
            th.RunWorkerCompleted += new RunWorkerCompletedEventHandler(th_RunWorkerCompleted);

            th1.DoWork += new DoWorkEventHandler(th1_DoWork);
            th1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(th1_RunWorkerCompleted);
            
            chart1.Series.Add("Series1");
            chart1.Series.Add("Series2");
            chart1.ChartAreas[0].AxisY.Maximum = 12;
            chart1.ChartAreas[0].AxisY.Minimum = -12;

        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void _bw_DoWork(object sender, DoWorkEventArgs e)
        {
            do
            {
                System.Threading.Thread.Sleep(100);
                count += 1;

                SetGraph(count);
            } while (checkBoxInteractive.Checked && checkBoxInteractive.Checked);
        }

        private void _bw1_DoWork(object sender, DoWorkEventArgs e)
        {
            loadingImage.Enabled = true;
            
            if (th.IsBusy || th1.IsBusy)
            {
                return;
            }
            else
            {
                th.RunWorkerAsync();
                th1.RunWorkerAsync();
            }

            while (th.IsBusy || th1.IsBusy)
            {
                Thread.Sleep(100);
            }
        }

        private void _bw1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("All threads are done");
            loadingImage.Enabled = false;
            textPrintBox.Text += fibRess;
        }

        private void th_DoWork(object sender, DoWorkEventArgs e)
        {
            Work.Calculate(getTextBoxValue(fibNumber));
        }

        private void th_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
        }

        private void th1_DoWork(object sender, DoWorkEventArgs e)
        {
            fibRess = Work.Calculate(getTextBoxValue(fibNumber));
        }

        private void th1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (_bw.IsBusy)
            {
                return;
            }
            else
            {
                _bw.RunWorkerAsync();
            }
        }

        private void SetGraph(int count)
        {
            if (chart1.InvokeRequired)
            {
                SetChartCallback d = new SetChartCallback(SetGraph);
                Invoke(d, new object[] { count });
            }
            else
            {
                MakeGraph(count);
            }
        }

        private void MakeGraph(int shift)
        {
            int n = 100;
            int k = 0;

            foreach (var series in chart1.Series)
            {
                series.Points.Clear();
                if (series.Name == "Series1")
                {
                    for (int i = 0; i < n; i++)
                    {
                        double alpha = Math.PI * i * ((double)trackBar1.Value / 50.0) / ((double)n / 4) + (double)count / 2;
                        series.Points.AddXY(i * 1 + shift, Math.Sin(alpha + k) * trackBar3.Value + trackBar2.Value);
                    }

                    series.Color = Color.Red;
                }
                else
                {
                    for (int i = 0; i < n; i++)
                    {
                        double alpha = Math.PI * i * ((double)trackBar1.Value / 50.0) / ((double)n / 4) + (double)count / 2;
                        series.Points.AddXY(i * 1 + shift, Math.Sin(alpha + k+3) * trackBar3.Value + trackBar2.Value);
                    }

                    series.Color = Color.Blue;
                }
                series.ChartType = SeriesChartType.FastLine;
                k += 1;
            }

        }

        private void printTest_Click(object sender, EventArgs e)
        {
            int processors = 1;
            string processorsStr = System.Environment.GetEnvironmentVariable("NUMBER_OF_PROCESSORS");
            if (processorsStr != null)
            {
                processors = int.Parse(processorsStr);
            }
            textPrintBox.Text += "Nr. Proc: " + processors + "\r\n";
        }

        public string _textPrint
        {
            set { textPrintBox.Text += value; }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            RunFibonacci();
        }

        private void RunFibonacci()
        {
            const int FibonacciCalculations = 10;

            // Event for each fib object
            ManualResetEvent[] doneEvents = new ManualResetEvent[FibonacciCalculations];
            Fibonacci[] fibArray = new Fibonacci[FibonacciCalculations];
            Random r = new Random();

            // Start threadpool
            _textPrint = "Launching " + FibonacciCalculations + " tasks \r\n";
            
            for (int i = 0; i < FibonacciCalculations; i++)
            {
                doneEvents[i] = new ManualResetEvent(false);
                Fibonacci f = new Fibonacci(r.Next(20, 40), doneEvents[i]);
                fibArray[i] = f;

                ThreadPool.QueueUserWorkItem(f.ThreadPoolCallback, i);
            }

            // Wait for pool to be done
            WaitHandle.WaitAll(doneEvents);
            textPrintBox.Text += "Done with all calcs \r\n";

            // Display results
            for (int i = 0; i < FibonacciCalculations; i++)
            {
                textPrintBox.Text += "Fibonacci " + fibArray[i] + "\r\n";
            }
        }

        private int getTextBoxValue(TextBox textBox)
        {
            int x = 0;
            if (!Int32.TryParse(textBox.Text, out x))
            {
                x = 0;
            }

            return x;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textPrintBox.Clear();
            // Test test = new Test();
            Work work = new Work();

            int x = getTextBoxValue(fibNumber);

            loadingImage.Enabled = true;
            
            if (_bw1.IsBusy)
            {
                return;
            }
            else
            {
                _bw1.RunWorkerAsync();
            }
            /***
            _textPrint = "\r\n MC res: " + t.Result.ToString();

            _textPrint = "Result from fibonacci";***/
        }

        public void printStuff(string print)
        {
            textPrintBox.Text += print + "\r\n";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textPrintBox.Text = "hello";
        }
    }
    public class Fibonacci
    {

        private int _n;
        private int _fibOfn;
        private ManualResetEvent _doneEvent;
        public int N { get { return _n; } }
        public int FibOfN { get { return _fibOfn; } }


        // Constructor
        public Fibonacci(int n, ManualResetEvent doneEvent)
        {
            _n = n;
            _doneEvent = doneEvent;
        }

        // Wrapper method for use with thread pool
        public void ThreadPoolCallback(Object threadContext)
        {
            int threadIndex = (int)threadContext;
            _fibOfn = Calculate(_n);
            _doneEvent.Set();
        }

        public int Calculate(int n)
        {
            if (n <= 1)
            {
                return n;
            }

            return Calculate(n - 1) + Calculate(n - 2);
        }


    }
}

class Test
{
    static void Run()
    {
        /***
        ThreadStart threadDelegate = new ThreadStart(Work.DoWork);
        Thread newThread = new Thread(threadDelegate);
        newThread.Start();

        Work w = new Work();
        w.Data = 42;
        threadDelegate = new ThreadStart(w.DoMoreWork);
        newThread = new Thread(threadDelegate);
        newThread.Start();
        ***/
    }
}

public class Work
{

    public static void Write(TextBox textBox)
    {
        textBox.Text += "Hello from class";
        
    }

    public static void sleepIn()
    {
        for (int i = 0; i< 20; i++)
        {
            Thread.Sleep(100);
            
        }
    }

    public static int Calculate(int n)
    {
        if (n <= 1)
        {
            return n;
        }

        return Calculate(n - 1) + Calculate(n - 2);
    }
}