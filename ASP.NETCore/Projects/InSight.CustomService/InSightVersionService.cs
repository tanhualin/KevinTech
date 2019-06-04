using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Generic;
using System.ServiceProcess;

namespace InSight.CustomService
{
    public partial class InSightVersionService : ServiceBase
    {
        public InSightVersionService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {

        }

        protected override void OnStop()
        {
        }

        private static void RabbitMqRecv()
        {
            // 获取到连接以及mq通道
            ConnectionFactory factory = new ConnectionFactory();
            factory.Port = 5672;    //服务器端口号：默认端口号为：5672
            factory.Endpoint = new AmqpTcpEndpoint(new System.Uri("amqp://172.21.26.120/"));//服务器IP：172.21.26.120;
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

                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);//告诉broker同一时间只处理一个消息                                                           //channel.QueueBind(QueueName, ExchangeName, routingKey: QueueName);
                    var consumer = new EventingBasicConsumer(channel);
                    var cusList = new List<Models.CustomVersionModel>();
                    bool pullFiles = false;

                    consumer.Received += (model, ea) =>
                    {
                        //当处理最后一个版本时
                        if (channel.MessageCount(Queue_Name) == 0)
                        {
                            var msgBody = System.Text.Encoding.UTF8.GetString(ea.Body);
                            var entity = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.CustomVersionModel>(msgBody);
                            if (!cusList.Contains(entity))
                            {
                                cusList.Add(entity);
                                //拉取文件,

                            }
                            else
                            {
                                //1、文件已拉取，但覆盖不成功
                                if (pullFiles)
                                {

                                }
                            }
                            
                            //
                        }
                        else
                        {
                            var msgBody = System.Text.Encoding.UTF8.GetString(ea.Body);
                            var entity = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.CustomVersionModel>(msgBody);
                            cusList.Add(entity);
                            //回复确认处理成功
                            channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                            //文件未拉取
                            pullFiles = false;
                            //暂停1秒，防止CPU爆满的问题
                            System.Threading.Thread.Sleep(1000);
                        }
                            
                        


                        //Console.WriteLine(string.Format("**【{0}】**接收时间:{1}，消息内容：{2}", index.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msgBody));
                        //int dots = msgBody.Split('.').Length - 1;

                        //处理完成，告诉Broker可以服务端可以删除消息，分配新的消息过来
                        if (channel.MessageCount(Queue_Name) == 0)
                        {
                            channel.BasicNack(ea.DeliveryTag, false, true);
                        }
                        else
                        {
                            channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                        }
                    };
                    //noAck设置false,告诉broker，发送消息之后，消息暂时不要删除，等消费者处理完成再说
                    channel.BasicConsume(Queue_Name, autoAck: false, consumer: consumer);

                }
            }
        }
    }
}
