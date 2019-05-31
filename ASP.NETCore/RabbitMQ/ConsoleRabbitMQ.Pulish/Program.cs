using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleRabbitMQ.Pulish
{
    class Program
    {
        static void Main(string[] args)
        {
            fanoutPublic();
        }

        private static void fanoutPublic()
        {
            // 获取到连接以及mq通道
            ConnectionFactory factory = new ConnectionFactory();
            factory.Port = 5672;    //服务器端口号：默认端口号为：5672
            factory.Endpoint = new AmqpTcpEndpoint(new Uri("amqp://172.21.26.120/"));//服务器IP：172.21.26.120;
            factory.UserName = "admin"; //服务器登录账号
            factory.Password = "2020"; //服务器密码账号
            factory.VirtualHost = "testhost"; //

            using (IConnection conn = factory.CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    string ExchangeName = "test_exchange_fanout";
                    // 声明exchange及交换器类型:fanout
                    channel.ExchangeDeclare(ExchangeName, "fanout");

                    for (int i = 0; i < 100; i++)
                    {
                        // 消息内容
                        //消息内容
                        string message = "测试交换器下多消息者调用：【" + i.ToString() + "】";
                        //推送消息
                        byte[] bytes = Encoding.UTF8.GetBytes(message);
                        channel.BasicPublish(ExchangeName, "", null, bytes);
                        Console.WriteLine("消息已发送：" + message);
                        System.Threading.Thread.Sleep(500);
                    }
                }
            }
        }
    }
}
