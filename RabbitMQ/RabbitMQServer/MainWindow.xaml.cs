using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace RabbitMQServer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
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

        public MainWindow()
        {
            InitializeComponent();
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
            using (IConnection conn = rabbitMqFactory.CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    channel.ExchangeDeclare(ExchangeName, "fanout", durable: false, autoDelete: true, arguments: null);
                    if (cb1.IsChecked.Value)
                    {
                        channel.QueueDeclare(QueueName, durable: false, exclusive: false, autoDelete: true, arguments: null);
                        channel.QueueBind(QueueName, ExchangeName, routingKey: QueueName);
                    }

                    var props = channel.CreateBasicProperties();
                    props.Persistent = true;
                    props.DeliveryMode = 2;
                    string vadata = Guid.NewGuid().ToString();
                    var msgBody = Encoding.UTF8.GetBytes(vadata);
                    if (cb1.IsChecked.Value)
                    {
                        channel.BasicPublish(exchange: ExchangeName, routingKey: QueueName, basicProperties: props, body: msgBody);
                    }
                    else
                    {
                        channel.BasicPublish(exchange: ExchangeName, routingKey: "", basicProperties: props, body: msgBody);
                    }
                    lbContent.Items.Insert(0, string.Format("发送内容：{0}  发送时间：{1}", vadata, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                }
            }
        }
    }
}
