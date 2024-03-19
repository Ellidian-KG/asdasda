using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private TcpClient tcpClient;
        private NetworkStream stream;
        private System.Timers.Timer timer;

        public Form1()
        {
            InitializeComponent();
        }

        public async Task ListenForMessages()
        {
            try
            {
                while (true)
                {
                    byte[] data = new byte[1024];
                    int bytesRead = await stream.ReadAsync(data, 0, data.Length);
                    string message = Encoding.UTF8.GetString(data, 0, bytesRead);

                    AppendMessage(message); 
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при чтении сообщений: " + ex.Message);
            }
        }

        private void AppendMessage(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => textBox2.AppendText(message + Environment.NewLine)));
            }
            else
            {
                textBox2.AppendText(message + Environment.NewLine);
            }
        }

        private void StartReceivingMessages()
        {
            Task.Run(async () =>
            {
                await ListenForMessages();
            });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var port = 12345;
            string ip = textBox1.Text;
            string ip2 = textBox4.Text;

            tcpClient = new TcpClient(ip2, port);
            stream = tcpClient.GetStream();

            textBox2.AppendText("Пользователь " + ip + " подключился к чату");
            textBox2.AppendText(Environment.NewLine);

            timer = new System.Timers.Timer(5);
            timer.Elapsed += (s, ev) => StartReceivingMessages();
            timer.AutoReset = false; 
            timer.Start();
        }

        private async Task ReceiveMessage()
        {
            try
            {
                byte[] data = new byte[1024];
                int bytesRead = await stream.ReadAsync(data, 0, data.Length);
                string message = Encoding.UTF8.GetString(data, 0, bytesRead);

                textBox2.Invoke((MethodInvoker)delegate
                {
                    textBox2.Text += message + Environment.NewLine;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при чтении сообщения: " + ex.Message);
            }
        }

        public async Task SendMessage(string message)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(data, 0, data.Length);

                string formattedMessage = $"{message}\n";
                textBox2.AppendText(formattedMessage);
                textBox2.AppendText(Environment.NewLine);
                textBox2.ScrollToCaret();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(textBox1.Text))
                {

                    string message = $"{textBox1.Text}: {textBox3.Text}\n";
                    await SendMessage(message);
                    await ReceiveMessage();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при отправке сообщения: " + ex.Message);
            }
        }



    }
}
