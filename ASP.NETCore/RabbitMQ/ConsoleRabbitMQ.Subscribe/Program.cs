using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleRabbitMQ.Subscribe
{
    class Program
    {
        static void Main(string[] args)
        {
            Recv2();
        }

        private static void Recv2()
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
                    string Queue_Name = "test_queue_work1";
                    string ExchangeName = "test_exchange_fanout";
                    //channel.ExchangeDeclare(ExchangeName, "direct", durable: true, autoDelete: false, arguments: null);
                    channel.QueueDeclare(Queue_Name, durable: false, autoDelete: false, exclusive: false, arguments: null);
                    // 绑定队列到交换机
                    channel.QueueBind(Queue_Name, ExchangeName, "");

                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);//告诉broker同一时间只处理一个消息
                    int index = 1;                                                                 //channel.QueueBind(QueueName, ExchangeName, routingKey: QueueName);
                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var msgBody = Encoding.UTF8.GetString(ea.Body);
                        Console.WriteLine(string.Format("**【{0}】**接收时间:{1}，消息内容：{2}", index.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msgBody));
                        //int dots = msgBody.Split('.').Length - 1;
                        System.Threading.Thread.Sleep(2000);
                        Console.WriteLine(" ---------");
                        //处理完成，告诉Broker可以服务端可以删除消息，分配新的消息过来
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                        index++;
                    };
                    //noAck设置false,告诉broker，发送消息之后，消息暂时不要删除，等消费者处理完成再说
                    channel.BasicConsume(Queue_Name, autoAck: false, consumer: consumer);

                    Console.WriteLine("按任意值，退出程序");
                    Console.ReadKey();
                }
            }
        }
    }
}
