using System;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;

namespace task5
{
    public partial class Form1 : Form
    {
        private delegate void SafeCallDelegate(string text);
        private Socket McastSendSocket;
        private Socket McastRecvSocket;
        private IPEndPoint RemoteEndPoint;
        private CancellationTokenSource TokenSource;
        private Thread Listener;
        private string UserName;
        public Form1()
        {
            InitializeComponent();
            RemoteEndPoint = new IPEndPoint(new IPAddress(new byte[] { 255, 255, 255, 255 }), 8888);
            TokenSource = new CancellationTokenSource();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox2.Text != "")
            {
                label3.Visible = false;
                UserName = textBox2.Text;
            }
            else
            {
                label3.Visible = true;
                return;
            }
            try
            {
                McastSendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                McastSendSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
                McastSendSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            }
            catch (SocketException se)
            {
                richTextBox1.AppendText(se.Message + '\n');
                return;
            }
            Send("User " + UserName + " has connected", true);
            Listener = new Thread(() => Listen(TokenSource.Token));
            Listener.Start();
            textBox1.Enabled = true;
            button3.Enabled = true;
            button1.Enabled = false;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                Send(textBox1.Text, false);
                textBox1.Text = "";
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
        }

        private void WriteTextSafe(string text)     //ThreadSafe richTextBox usage
        {
            if (textBox1.InvokeRequired)
            {
                var del = new SafeCallDelegate(WriteTextSafe);
                richTextBox1.Invoke(del, new object[] { text });
            }
            else
            {
                richTextBox1.AppendText(text);
            }
        }

        private void Listen(CancellationToken token)        //Cancellable Listener Thread
        {
            try
            {
                McastRecvSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                McastRecvSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
                McastRecvSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                var broadcastEndpoint = new IPEndPoint(IPAddress.Any, 8888);
                McastRecvSocket.Bind(broadcastEndpoint);
            }
            catch (SocketException se)
            {
                WriteTextSafe(se.Message + "FromListener\n");
                return;
            }
            while (!token.IsCancellationRequested)
            {
                if (McastRecvSocket.Available != 0)
                {
                    try
                    {
                        byte[] b = new byte[1024];
                        McastRecvSocket.Receive(b);
                        WriteTextSafe(Encoding.UTF8.GetString(b, 0, b.Length) + '\n');
                    }
                    catch (SocketException se)
                    {
                        WriteTextSafe(se.Message + "FromListener\n");
                        return;
                    }
                    catch(ObjectDisposedException)
                    {
                        return;
                    }
                }
                Thread.Yield();
            }
        }
        private void Send(string message, bool system)       //Send in main thread
        {
            try
            {
                byte[] byteMessage;
                if (system)
                    byteMessage = Encoding.UTF8.GetBytes(message + '\n');
                else
                    byteMessage = Encoding.UTF8.GetBytes(UserName + ": " + message + '\n');
                McastSendSocket.SendTo(byteMessage, RemoteEndPoint);
            }
            catch (SocketException se)
            {
                WriteTextSafe(se.Message + "FromSend\n");
                return;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Visible = false;
            if (McastRecvSocket != null)
                McastRecvSocket.Shutdown(SocketShutdown.Both);
            if (Listener != null)
            {
                TokenSource.Cancel();
                Listener.Join();
            }
            if (McastRecvSocket != null)
                McastRecvSocket.Close();
            if (McastSendSocket != null)
            {
                Send("User " + UserName + " has disconnected", true);
                McastSendSocket.Shutdown(SocketShutdown.Both);
                McastSendSocket.Close();
            }
            TokenSource.Dispose();
            base.OnFormClosing(e);
        }
    }
}
