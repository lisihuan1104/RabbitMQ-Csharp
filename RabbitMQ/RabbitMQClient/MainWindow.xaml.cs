using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RabbitMQClient
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isRunning = true;
        private bool synchronization = false;
        public MainWindow()
        {
            InitializeComponent();
        }

        private static readonly ConnectionFactory rabbitMqFactory = new ConnectionFactory()
        {
            HostName = "127.0.0.1",
            UserName = "garea",
            Password = "garea",
            Port = 5672,
            VirtualHost = "testhost"
        };
        /// <summary>
        /// 路由名称
        /// </summary>
        const string ExchangeName = "Jent.Exchange";
        /// <summary>
        /// 队列名称
        /// </summary>
        const string QueueName = "Jent.Queue";
        private void DirectAcceptExchange()
        {
            using (IConnection conn = rabbitMqFactory.CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    channel.ExchangeDeclare(ExchangeName, "fanout", durable: false, autoDelete: true, arguments: null);
                    if (!synchronization)
                    {
                        channel.QueueDeclare(QueueName, durable: false, exclusive: false, autoDelete: true, arguments: null);
                        channel.QueueBind(QueueName, ExchangeName, routingKey: QueueName);
                        while (isRunning)
                        {
                            BasicGetResult msgResponse = channel.BasicGet(QueueName, autoAck: true);
                            if (msgResponse != null)
                            {
                                var msgBody = Encoding.UTF8.GetString(msgResponse.Body);
                                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    lbContent.Items.Insert(0, string.Format("发送内容：{0}  发送时间：{1}", msgBody, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                                }));
                            }
                        }
                    }
                    else
                    {
                        var queueok = channel.QueueDeclare();
                        channel.QueueBind(queueok.QueueName, ExchangeName, routingKey: "");
                        var conumer = new QueueingBasicConsumer(channel);
                        channel.BasicConsume(queueok.QueueName, true, conumer);
                        while (isRunning)
                        {
                            var _result = (BasicDeliverEventArgs)conumer.Queue.Dequeue();//5                        
                            var body = _result.Body;
                            var message = Encoding.UTF8.GetString(body);
                            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                lbContent.Items.Insert(0, string.Format("发送内容：{0}  发送时间：{1}", message, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                            }));
                        }
                    }
                }
            }
        }

        private void Cb1_Click(object sender, RoutedEventArgs e)
        {
            cb1.IsChecked = true;
            cb2.IsChecked = false;
        }

        private void Cb2_Click(object sender, RoutedEventArgs e)
        {
            cb2.IsChecked = true;
            cb1.IsChecked = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (cb1.IsChecked.Value)
            {
                synchronization = false;
            }
            else
            {
                synchronization = true;
            }
            btnStart.IsEnabled = false;
            Thread thread = new Thread(DirectAcceptExchange);
            thread.Start();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            isRunning = false;
        }
    }
}
