using NLog;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;

namespace ConsoleRabbitMQ.Receive
{
    /// <summary>
    /// 该项目为RabbitMQ的接收信息（消息者端）
    /// 1、直接接收队列的信息（Work模式）
    /// 2、Work模式包含：轮询分发与公平分发
    /// 3、轮询分发与公平分发区别：设置basicQos与确认模式
    /// </summary>
    class Program
    {
        public static Logger logger = LogManager.GetLogger("logFiles");
        static void Main(string[] args)
        {
            Recv2();
        }


        private static void Recv()
        {
            // 获取到连接以及mq通道
            ConnectionFactory factory = new ConnectionFactory();
            factory.Port = 5672;    //服务器端口号：默认端口号为：5672
            factory.Endpoint = new AmqpTcpEndpoint(new Uri("amqp://172.21.26.120/"));//服务器IP：172.21.26.120;
            factory.UserName = "admin"; //服务器登录账号
            factory.Password = "2020"; //服务器密码账号
            factory.VirtualHost = "testhost"; //
            //factory.RequestedHeartbeat = heartbeat;
            using (IConnection conn = factory.CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    string Queue_Name = "q_test_01";
                    //声明消息队列
                    channel.QueueDeclare(Queue_Name, false, false, false, null);
                    // 同一时刻服务器只会发一条消息给消费者
                    channel.BasicQos(0, 1, false);     //BasicQos(1);
                                                       // 定义队列的消费者
                    var consumer = new QueueingBasicConsumer(channel);
                    //监听队列，false表示手动返回完成状态，true表示自动
                    channel.BasicConsume(Queue_Name, false, consumer);

                    #region 获取消息
                    while (true)
                    {
                        ulong deliveryTag = 0;
                        try
                        {
                            Thread.Sleep(1000);//暂停1秒，防止CPU爆满的问题

                            //获取信息
                            var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
                            deliveryTag = ea.DeliveryTag;
                            byte[] bytes = ea.Body;
                            string str = Encoding.UTF8.GetString(bytes);

                            logger.Info($"获取到服务器队列{Queue_Name}的信息:{str}");
                            //回复确认处理成功
                            channel.BasicAck(deliveryTag, false);
                            //发生错误了，但是还可以重新提交给队列重新分配
                            //channel.BasicNack(deliveryTag, false, true);
                            //发生严重错误，无法继续进行，这种情况应该写日志或者是发送消息通知管理员
                            //channel.BasicNack(deliveryTag, false, false);
                        }
                        catch (Exception ex)
                        {
                            //channel.BasicNack(deliveryTag, false, false);
                            //logger.Info($"获取到服务器队列{Queue_Name}的信息失败:{ex.Message}");
                        }
                    }
                    #endregion
                }
            }
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
                    string Queue_Name = "q_test_02";
                    //channel.ExchangeDeclare(ExchangeName, "direct", durable: true, autoDelete: false, arguments: null);
                    channel.QueueDeclare(Queue_Name, durable: false, autoDelete: false, exclusive: false, arguments: null);
                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);//告诉broker同一时间只处理一个消息
                    int index = 1;                                                                 //channel.QueueBind(QueueName, ExchangeName, routingKey: QueueName);
                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var msgBody = Encoding.UTF8.GetString(ea.Body);
                        Console.WriteLine(string.Format("**【{0}】**接收时间:{1}，消息内容：{2}",index.ToString(),DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msgBody));
                        //int dots = msgBody.Split('.').Length - 1;
                        System.Threading.Thread.Sleep(2000);
                        Console.WriteLine(" [x] Done");
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
