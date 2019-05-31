using RabbitMQ.Client;
using System;
using System.Text;

namespace ConsoleRabbitMQ.Send
{
    /// <summary>
    /// 项目为：RabbitMQ的生产者端
    /// 直接将消息发送到队列（普通模式）
    /// 生产者发送消息分为：普通械、确认模式、事务模式
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // 获取到连接以及mq通道
            ConnectionFactory factory = new ConnectionFactory();
            factory.Port = 5672;    //服务器端口号：默认端口号为：5672
            factory.Endpoint = new AmqpTcpEndpoint(new Uri("amqp://172.21.26.120/"));//服务器IP：172.21.26.120;
            factory.UserName = "admin"; //服务器登录账号
            factory.Password = "2020"; //服务器密码账号
            factory.VirtualHost = "testhost"; //
            //factory.RequestedHeartbeat = heartbeat;

            // 从连接中创建通道
            using (var channel = factory.CreateConnection().CreateModel())
            {
                string Queue_Name = "q_test_02";
                //声明消息队列
                channel.QueueDeclare(Queue_Name, false, false, false, null);

                for (int i = 0; i < 100; i++)
                {
                    // 消息内容
                    //消息内容
                    string message = "测试多消息者调用：【" + i.ToString()+"】";
                    //推送消息
                    byte[] bytes = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish("", Queue_Name, null, bytes);
                    Console.WriteLine("消息已发送：" + message);
                    System.Threading.Thread.Sleep(500);
                }

                
            }
        }
    }
}
