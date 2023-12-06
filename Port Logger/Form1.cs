using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UdemyEgitim
{
    public partial class Form1 : Form
    {
        private TcpListener tcpListener;
        private bool isListening = false;

        private Thread listenerThread;

        public Form1()
        {
            InitializeComponent();
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (int.TryParse(textBox1.Text, out int port))
            {
                if (port > 65535)
                {
                    MessageBox.Show("Invalid port. Port value cannot be greater than 65535.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    listBox1.Items.Add($"Port {port} is listening...");
                    textBox1.Enabled = false;
                    label3.Text = port.ToString();
                    button1.Enabled = false;
                    listBox1.HorizontalScrollbar = true;
                    listBox1.Visible = true;
                    button2.Enabled = true;

                    StartListening(port);
                }
            }
            else
            {
                MessageBox.Show("Invalid port number, please enter a number.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            StopListening();
        }

        private void StartListening(int port)
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, port);
                listenerThread = new Thread(new ThreadStart(ListenForClients));
                listenerThread.Start();
                isListening = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void StopListening()
        {
            if (tcpListener != null)
            {
                isListening = false;
                tcpListener.Stop();
                listBox1.Items.Add("Port listening has been stopped.");
                textBox1.Enabled = true;
                button2.Enabled = false;
                button1.Enabled = true;
                label3.Text = "";
            }
        }

        private void ListenForClients()
        {
            try
            {
                tcpListener?.Start();

                while (isListening)
                {
                    if (tcpListener != null)
                    {

                        TcpClient client = tcpListener.AcceptTcpClient();

                        if (client != null)
                        {
                            Thread clientThread = new Thread(() => HandleClientComm(client));
                            clientThread.Start();
                        }
                    }
                }
            }
            catch (ObjectDisposedException)
            {

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bağlantı hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void HandleClientComm(TcpClient client)
        {

            IPEndPoint remoteEndPoint = (IPEndPoint)client.Client.RemoteEndPoint;
            string clientIPAddress = remoteEndPoint.Address.ToString();

            DateTime connectionTime = DateTime.Now;

            string clientMachineName;
            try
            {
                clientMachineName = Dns.GetHostEntry(clientIPAddress).HostName;
            }
            catch (Exception)
            {
                clientMachineName = "Unknown";
            }

            listBox1.Invoke((MethodInvoker)delegate
            {
                listBox1.Items.Add($"'{clientMachineName}' The client named is connected. '{clientIPAddress}' | {connectionTime:yyyy-MM-dd HH:mm}");
            });


            client.Close();

            listBox1.Invoke((MethodInvoker)delegate
            {
                listBox1.Items.Add($"'{clientMachineName}' The client named has logged out. He disconnected.'{clientIPAddress}' | {DateTime.Now:yyyy-MM-dd HH:mm}");
            });
        }


    }
}
