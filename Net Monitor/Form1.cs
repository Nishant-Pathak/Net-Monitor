using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        private byte[] byteData = new byte[4096];
        private BackgroundWorker backgroundWorker1;
        private int inBytes;
        private int outBytes;
        private IPEndPoint ipEpOut;
        private string srcIp = "";
        private Socket socketIn;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string strIP = null;
            inBytes = 0;

            IPHostEntry HosyEntry = Dns.GetHostEntry((Dns.GetHostName()));
            if (HosyEntry.AddressList.Length > 0)
            {
                foreach (IPAddress ip in HosyEntry.AddressList)
                {
                    strIP = ip.ToString();
                    dropDowmInterface.Items.Add(strIP);
                }

            }
        }

        private void dropDown_IndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (socketIn != null)
                {
                    if (socketIn.IsBound)
                    {
                        socketIn.Close();

                    }
                }
                srcIp = dropDowmInterface.Text;
                IPEndPoint ipEp1 = new IPEndPoint(IPAddress.Parse(dropDowmInterface.Text), 0);
                ipEpOut = new IPEndPoint(IPAddress.Parse(dropDowmInterface.Text), 0);
                socketIn = new Socket(ipEp1.AddressFamily, SocketType.Raw, ProtocolType.IP);
                socketIn.Bind(ipEp1);
                byte[] byTrue = new byte[4] { 1, 0, 0, 0 };
                byte[] byOut = new byte[4] { 1, 0, 0, 0 };
                socketIn.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None,
                    new AsyncCallback(OnReceiveIn), null);

                socketIn.IOControl(IOControlCode.ReceiveAll, byTrue, byOut);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error" + ex);
            }
        }

        private void OnReceiveIn(IAsyncResult ar)
        {

            int nReceived = 0;
            IPHeader ipHeader = null;
            try
            {
                nReceived = socketIn.EndReceive(ar);
                ipHeader = new IPHeader(ref byteData, nReceived);
            }
            
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return;
            }
            System.Console.WriteLine("out" + ipHeader.SourceAddress.ToString());
            if (ipHeader.SourceAddress.ToString() == srcIp) {
                outBytes += nReceived;
            }
            else
            {
                inBytes += nReceived;
            }
            if (!backgroundWorker1.IsBusy)
            {
                this.backgroundWorker1.RunWorkerAsync();
            }
            socketIn.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None,
                new AsyncCallback(OnReceiveIn), null);
        }
        private void backgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (chart1.Series.Count != 2) return;
            System.Windows.Forms.DataVisualization.Charting.Series inSeries = chart1.Series[0];
            System.Windows.Forms.DataVisualization.Charting.Series outSeries = chart1.Series[1];

            inSeries.Points.AddXY(DateTime.Now, inBytes);
            outSeries.Points.AddXY(DateTime.Now, outBytes);

            inBytes = 0;
            outBytes = 0;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
           // MessageBox.Show("Good bye" + e.ToString());
            if (socketIn != null && socketIn.IsBound)
            {
                socketIn.Close();
            }
        }
    }
}
