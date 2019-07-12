using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Generic;
using System.ServiceProcess;
using NLog;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using InSight.CustomService.Common;

namespace InSight.CustomService
{
    public partial class InSightVersionService : ServiceBase
    {
        public static Logger logger = LogManager.GetLogger("InSightLogFiles");
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
                    //获取配置路径
                    string basePath = Path.Combine(Models.AppSettingModel.targetAppDir, Models.AppSettingModel.fileDirName);
                    if (!Directory.Exists(basePath))
                        Directory.CreateDirectory(basePath);
                    string sPath = Path.Combine(basePath, Models.AppSettingModel.filePullSaveDirName);
                    string uPath = Path.Combine(basePath, Models.AppSettingModel.fileUnZipDirName);
                    string pPath = Path.Combine(basePath, Models.AppSettingModel.fileBackupDirName);

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
                                var version = cusList.OrderBy(p => p.version).FirstOrDefault().version;
                                var entitylist = cusList.Where(p => p.isNeed == true || p.version == version);
                                //连接文件服务器
                                var msg = string.Empty;
                                if (CommonHelper.ConnectFileRemoteHost(Models.AppSettingModel.fileRemoteHost,
                                    Models.AppSettingModel.fileUserName, Models.AppSettingModel.filePassword,ref msg))
                                {

                                    try
                                    {
                                        #region 拉取文件
                                        foreach (var cv in entitylist)
                                        {
                                            if (!string.IsNullOrEmpty(cv.verFilePath))
                                            {
                                                var fileExt = Path.GetExtension(cv.verFilePath).ToLower();
                                                if (fileExt == "dll")
                                                {
                                                    #region 拉取DLL文件
                                                    try
                                                    {
                                                        var sFilePath = Path.Combine(sPath, cv.version,
                                                            Path.GetFileName(cv.verFilePath));
                                                        if (!File.Exists(sFilePath))
                                                        {
                                                            File.Copy(cv.verFilePath, sFilePath, true);
                                                        }
                                                    }
                                                    catch (Exception err)
                                                    {
                                                        throw new Exception(
                                                            $"拉取【{cv.verFilePath}】文件失败，" + err.Message);
                                                    }

                                                    #endregion
                                                }
                                                else if (fileExt == "rar" || fileExt == "zip")
                                                {
                                                    #region 拉取压缩包
                                                    try
                                                    {
                                                        var sFilePath = Path.Combine(sPath,
                                                            Path.GetFileName(cv.verFilePath));
                                                        if (!File.Exists(sFilePath))
                                                        {
                                                            File.Copy(cv.verFilePath, sFilePath, true);
                                                        }
                                                    }
                                                    catch (Exception err)
                                                    {
                                                        throw new Exception(
                                                            $"拉取【{cv.verFilePath}】文件失败，" + err.Message);
                                                    }

                                                    #endregion
                                                }
                                            }
                                        }

                                        #endregion

                                        pullFiles = true; //文件已下载

                                        #region 解压
                                        foreach (var cv in entitylist)
                                        {
                                            if (!string.IsNullOrEmpty(cv.verFilePath))
                                            {
                                                var fileExt = Path.GetExtension(cv.verFilePath).ToLower();
                                                if (fileExt == "rar" || fileExt == "zip")
                                                {
                                                    #region 解压
                                                    try
                                                    {
                                                        var sFileName = Path.Combine(sPath,
                                                            Path.GetFileName(cv.verFilePath));
                                                        var unZipPath = Path.Combine(uPath, cv.version.ToString());
                                                        ZipHelper.UnZip(sFileName, unZipPath);
                                                    }
                                                    catch (Exception err)
                                                    {
                                                        throw new Exception(
                                                            $"解压文件{cv.verFilePath}出错，" + err.Message);
                                                    }
                                                    #endregion
                                                }
                                            }
                                        }
                                        #endregion

                                        //判断InSight程序是否运行
                                        var processName = Process.GetProcessesByName("InSight");
                                        if (processName != null)
                                        {
                                            //
                                        }
                                        else
                                        {
                                            #region 更新程序DLL
                                            foreach (var cv in entitylist)
                                            {
                                                if (!string.IsNullOrEmpty(cv.verFilePath))
                                                {
                                                    var fileExt = Path.GetExtension(cv.verFilePath).ToLower();
                                                    if (fileExt == "dll")
                                                    {
                                                        var fileName = Path.Combine(basePath,
                                                            Path.GetFileName(cv.verFilePath));
                                                        //若存在该DLL，则将该DLL复制到备份文件夹
                                                        if (File.Exists(fileName))
                                                        {
                                                            var targetFilePath = Path.Combine(pPath, cv.version,
                                                                Path.GetFileName(cv.verFilePath));
                                                            File.Move(fileName, targetFilePath);
                                                        }

                                                        //将下载的DLL，复制到安装目录下
                                                        var sFilePath = Path.Combine(sPath, cv.version,
                                                            Path.GetFileName(cv.verFilePath));
                                                        File.Move(sFilePath, fileName);
                                                    }
                                                    else if (fileExt == "rar" || fileExt == "zip")
                                                    {
                                                        #region

                                                        try
                                                        {
                                                            DirectoryInfo unZipFolder =
                                                                new DirectoryInfo(Path.Combine(uPath, cv.version));
                                                            FileInfo[] unZipFiles = unZipFolder.GetFiles();
                                                            foreach (var file in unZipFiles)
                                                            {
                                                                var fileName = Path.Combine(basePath, file.Name);
                                                                //若存在该DLL，则将该DLL复制到备份文件夹
                                                                if (File.Exists(fileName))
                                                                {
                                                                    var targetFilePath = Path.Combine(pPath, cv.version,
                                                                        file.Name);
                                                                    File.Move(fileName, targetFilePath);
                                                                }

                                                                //将下载的DLL，复制到安装目录下
                                                                var sFilePath = Path.Combine(uPath, cv.version,
                                                                    file.Name);
                                                                File.Move(sFilePath, fileName);
                                                            }
                                                        }
                                                        catch (Exception err)
                                                        {
                                                            throw new Exception(
                                                                $"解压文件{cv.verFilePath}出错，" + err.Message);
                                                        }

                                                        #endregion
                                                    }
                                                }
                                            }
                                            #endregion
                                        }

                                        //文件拉完后清空文件列表
                                        cusList.Clear();

                                        //回复确认处理成功
                                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                                    }
                                    catch (Exception ex)
                                    {//发生错误了，但是还可以重新提交给队列重新分配
                                        channel.BasicNack(ea.DeliveryTag, false, true);
                                        throw new Exception(ex.Message);
                                    }
                                }
                                else
                                {//连接文件服务器异常
                                    //发生错误了，但是还可以重新提交给队列重新分配
                                    channel.BasicNack(ea.DeliveryTag, false, true);
                                    //暂停N秒
                                    System.Threading.Thread.Sleep(Models.AppSettingModel.mqWaitTime);
                                }
                            }
                            else
                            {
                                //1、文件已拉取，但覆盖不成功
                                if (!pullFiles)
                                {
                                    var msg = string.Empty;
                                    if (CommonHelper.ConnectFileRemoteHost(Models.AppSettingModel.fileRemoteHost,
                                        Models.AppSettingModel.fileUserName, Models.AppSettingModel.filePassword,
                                        ref msg))
                                    {
                                        //pullRemoteFiles(entitylist,)
                                    }
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
                            System.Threading.Thread.Sleep(Models.AppSettingModel.mqTimeout);
                        }
                    };
                    //noAck设置false,告诉broker，发送消息之后，消息暂时不要删除，等消费者处理完成再说
                    channel.BasicConsume(Queue_Name, autoAck: false, consumer: consumer);

                }
            }
        }

        private static void RabbitMqSubscribe()
        {
            logger.Info("服务开启，开始初始化mq通道！");
            // 获取到连接以及mq通道
            ConnectionFactory factory = new ConnectionFactory();
            factory.Port = Models.AppSettingModel.mqPort;    //服务器端口号：默认端口号为：5672
            factory.Endpoint = new AmqpTcpEndpoint(new System.Uri($"amqp://{Models.AppSettingModel.mqEndpointIP}/"));//服务器IP：172.21.26.120;
            factory.UserName = Models.AppSettingModel.mqUserName; //服务器登录账号
            factory.Password = Models.AppSettingModel.mqPassword; //服务器密码账号 
            factory.VirtualHost = Models.AppSettingModel.mqVirtualHost; //
            using (IConnection conn = factory.CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    logger.Info("开始获取配置文件参数!");
                    #region 配置文件参数
                    string Queue_Name = Models.AppSettingModel.mqQueueName;
                    string ExchangeName = Models.AppSettingModel.mqExchangeName;
                    string fileRemoteHost = Models.AppSettingModel.fileRemoteHost;
                    string targetAppName = Models.AppSettingModel.targetAppName;
                    string baseDir = Models.AppSettingModel.targetAppDir;
                    string inSightAlert = Models.AppSettingModel.inSightAlert;
                    int inSightWaitTime = Models.AppSettingModel.inSightWaitTime;

                    string customDir = Path.Combine(baseDir, Models.AppSettingModel.fileDirName);
                    if (!Directory.Exists(customDir))
                        Directory.CreateDirectory(customDir);
                    string downFileDir = Path.Combine(customDir, Models.AppSettingModel.filePullSaveDirName);
                    if (!Directory.Exists(downFileDir))
                        Directory.CreateDirectory(downFileDir);
                    string unZipDir = Path.Combine(customDir, Models.AppSettingModel.fileUnZipDirName);
                    if (!Directory.Exists(unZipDir))
                        Directory.CreateDirectory(unZipDir);
                    string backupDir = Path.Combine(customDir, Models.AppSettingModel.fileBackupDirName);
                    if (!Directory.Exists(backupDir))
                        Directory.CreateDirectory(backupDir);
                    #endregion

                    #region 初始化队列及绑定交换机
                    var cusList = new List<Models.CustomVersionModel>();
                    bool isNewTask = true;//判断是否新任务
                    string msg = string.Empty;
                    bool isPullFile = true; //文件是否拉取成功
                    bool isUnZip = true;// 解压是否成功
                    bool isUtl = true;// 更新是否成功
                    logger.Info("开始初始化队列及绑定交换机！");
                    channel.QueueDeclare(Queue_Name, durable: false, autoDelete: false, exclusive: false, arguments: null);
                    // 绑定队列到交换机
                    channel.QueueBind(Queue_Name, ExchangeName, "");
                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);//告诉broker同一时间只处理一个消息                                                           //channel.QueueBind(QueueName, ExchangeName, routingKey: QueueName);
                    var consumer = new EventingBasicConsumer(channel);
                    #endregion

                    logger.Info("消费者开始监控！");
                    consumer.Received += (model, ea) =>
                    { //当处理最后一个版本时,判断是否新任务
                        if (channel.MessageCount(Queue_Name) == 0)
                        { //新任务
                            if (isNewTask)
                            {
                                #region 处理最后一个版本的新任务
                                try
                                {
                                    logger.Info($"处理本次最后的新任务:{ea.DeliveryTag}");
                                    var msgBody = System.Text.Encoding.UTF8.GetString(ea.Body);
                                    var entity = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.CustomVersionModel>(msgBody);
                                    cusList.Add(entity);
                                    //获取最新程序集与必需覆盖的程序集,
                                    var version = cusList.OrderByDescending(p => p.verIdx).First().version;
                                    cusList = cusList.Where(p => p.isNeed == true || p.version == version).ToList();
                                    //连接文件服务器
                                    logger.Info($"任务:{ea.DeliveryTag}的程序集版本:{entity.version}转换成功！开始连接拉取服务器文件!");
                                    
                                    if (CommonHelper.ConnectFileRemoteHost(Models.AppSettingModel.fileRemoteHost,
                                        Models.AppSettingModel.fileUserName, Models.AppSettingModel.filePassword, ref msg))
                                    { //开始拉取文件
                                        #region 开始拉取文件
                                        try
                                        {
                                            logger.Info($"开始拉取服务器文件!");
                                            isPullFile = CommonHelper.pullRemoteHostFiles(cusList, fileRemoteHost, downFileDir, ref msg);
                                        }
                                        catch (Exception err)
                                        {
                                            isPullFile = false;
                                            msg = err.Message;
                                            //logger.Info($"拉取服务器文件失败!"+err.Message);
                                        }
                                        #endregion
                                        if (isPullFile)
                                        {
                                            #region 开始解压文件
                                            try
                                            {
                                                logger.Info($"开始解压文件!");
                                                isUnZip = CommonHelper.UnZipDownFiles(cusList, downFileDir, unZipDir, ref msg);
                                            }
                                            catch (Exception err)
                                            {
                                                isUnZip = false;
                                                msg = err.Message;
                                            }
                                            #endregion
                                            if (isUnZip)
                                            {
                                                try
                                                {
                                                    var proAppName = Process.GetProcessesByName(targetAppName);
                                                    if (proAppName != null && proAppName.Length > 0)
                                                    {
                                                        logger.Info($"目标程序:{targetAppName}已启动时，开始处理更新程序集!");
                                                        //当程序在运行时，判断是否存在必需要覆盖的程序集
                                                        var entityList = cusList.Where(p => p.isNeed == true);
                                                        if (entityList != null && entityList.ToList().Count > 0)
                                                        {
                                                            //当存在时，服务弹出提示窗口，并在10秒后关闭程序
                                                            SendMessageHelper.ShowMessageBox("InSight定制版本更新提示", inSightAlert);
                                                            //暂停N秒，
                                                            Thread.Sleep(inSightWaitTime);
                                                            logger.Info($"等待N秒，后再查看目标程序是否打开，若打开则杀死！");
                                                            proAppName = Process.GetProcessesByName(targetAppName);
                                                            foreach (var pro in proAppName)
                                                            {
                                                                pro.Kill();
                                                            }
                                                            logger.Info($"目标程序:{targetAppName}未启动，开始处理更新程序集!");
                                                            isUtl = CommonHelper.UtlInSightCustomDll(cusList, baseDir, downFileDir, unZipDir, backupDir, ref msg);
                                                        }
                                                        else
                                                        {
                                                            logger.Info($"更新版本不存在必需覆盖版本，暂时不更新");
                                                            isUtl = false;
                                                            msg = string.Empty;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        logger.Info($"目标程序:{targetAppName}未启动，开始处理更新程序集!");
                                                        isUtl = CommonHelper.UtlInSightCustomDll(cusList, baseDir, downFileDir,unZipDir, backupDir, ref msg);
                                                    }
                                                }
                                                catch (Exception err)
                                                {
                                                    isUtl = false;
                                                    msg = err.Message;
                                                }
                                                if (isUtl)
                                                {
                                                    logger.Info($"任务:{ea.DeliveryTag}的程序集更新成功!清空处理列表");
                                                    cusList.Clear();
                                                    //回复确认处理成功
                                                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                                                }
                                                else
                                                {
                                                    if (string.IsNullOrEmpty(msg))
                                                    {
                                                        logger.Info($"目标程序已打开，且该任务不存在必需更新!，重新提交任务，下次更新");
                                                    }
                                                    else
                                                    {
                                                        logger.Info($"更新程序集失败!" + msg);
                                                    }
                                                    isNewTask = false;
                                                    //发生错误了，但是还可以重新提交给队列重新分配
                                                    channel.BasicNack(ea.DeliveryTag, false, true);
                                                    //暂停1秒，防止CPU爆满的问题
                                                    System.Threading.Thread.Sleep(Models.AppSettingModel.mqWaitTime);
                                                }
                                            }
                                            else
                                            {
                                                logger.Info($"处理拉取的文件失败!" + msg);
                                                isNewTask = false;
                                                //发生错误了，但是还可以重新提交给队列重新分配
                                                channel.BasicNack(ea.DeliveryTag, false, true);
                                                //暂停1秒，防止CPU爆满的问题
                                                System.Threading.Thread.Sleep(Models.AppSettingModel.mqWaitTime);
                                            }
                                        }
                                        else
                                        {
                                            logger.Info($"拉取服务器文件失败!" + msg);
                                            isNewTask = false;
                                            //发生错误了，但是还可以重新提交给队列重新分配
                                            channel.BasicNack(ea.DeliveryTag, false, true);
                                            //暂停1秒，防止CPU爆满的问题
                                            Thread.Sleep(Models.AppSettingModel.mqWaitTime);
                                        }
                                    }
                                    else
                                    {
                                        logger.Info($"连接文件服务器失败!" + msg);
                                        isNewTask = false;
                                        isPullFile = false;
                                        //发生错误了，但是还可以重新提交给队列重新分配
                                        channel.BasicNack(ea.DeliveryTag, false, true);
                                        //暂停N秒
                                        Thread.Sleep(Models.AppSettingModel.mqWaitTime);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Info($"处理新任务：{ea.DeliveryTag} 失败！" + ex.Message);
                                    isNewTask = false;
                                    isPullFile = false;
                                    //发生错误了，但是还可以重新提交给队列重新分配
                                    channel.BasicNack(ea.DeliveryTag, false, true);
                                    //暂停1秒，防止CPU爆满的问题
                                    System.Threading.Thread.Sleep(Models.AppSettingModel.mqWaitTime);
                                }
                                #endregion
                            }
                            else
                            {
                                #region 处理重新提交的任务
                                //1、判断文件是否已拉取，若拉取，则判断文件是否已解压，若解压，则判断是否已更新
                                if (!isPullFile)
                                {
                                    #region 若文件没拉取，则先拉取
                                    if (CommonHelper.ConnectFileRemoteHost(Models.AppSettingModel.fileRemoteHost,
                                        Models.AppSettingModel.fileUserName, Models.AppSettingModel.filePassword, ref msg))
                                    { //开始拉取文件
                                        #region 开始拉取文件
                                        try
                                        {
                                            logger.Info($"开始拉取服务器文件!");
                                            isPullFile = CommonHelper.pullRemoteHostFiles(cusList, fileRemoteHost, downFileDir, ref msg);
                                        }
                                        catch (Exception err)
                                        {
                                            isPullFile = false;
                                            msg = err.Message;
                                            //logger.Info($"拉取服务器文件失败!"+err.Message);
                                        }
                                        #endregion
                                        if (isPullFile)
                                        {
                                            #region 开始解压文件
                                            try
                                            {
                                                logger.Info($"开始解压文件!");
                                                isUnZip = CommonHelper.UnZipDownFiles(cusList, downFileDir, unZipDir, ref msg);
                                            }
                                            catch (Exception err)
                                            {
                                                isUnZip = false;
                                                msg = err.Message;
                                            }
                                            #endregion
                                            if (isUnZip)
                                            {
                                                #region 开始更新前操作
                                                try
                                                {
                                                    var proAppName = Process.GetProcessesByName(targetAppName);
                                                    if (proAppName != null && proAppName.Length > 0)
                                                    {
                                                        #region 当打开了目标程序且必需更新，则先提示关闭，
                                                        logger.Info($"目标程序:{targetAppName}已启动时，开始处理更新程序集!");
                                                        //当程序在运行时，判断是否存在必需要覆盖的程序集
                                                        var entityList = cusList.Where(p => p.isNeed == true);
                                                        if (entityList != null && entityList.ToList().Count > 0)
                                                        {
                                                            //当存在时，服务弹出提示窗口，并在10秒后关闭程序
                                                            SendMessageHelper.ShowMessageBox("InSight定制版本更新提示", inSightAlert);
                                                            //暂停N秒，
                                                            Thread.Sleep(inSightWaitTime);
                                                            logger.Info($"等待N秒，后再查看目标程序是否打开，若打开则杀死！");
                                                            proAppName = Process.GetProcessesByName(targetAppName);
                                                            foreach (var pro in proAppName)
                                                            {
                                                                pro.Kill();
                                                            }
                                                            logger.Info($"目标程序:{targetAppName}未启动，开始处理更新程序集!");
                                                            isUtl = CommonHelper.UtlInSightCustomDll(cusList, baseDir, downFileDir, unZipDir, backupDir, ref msg);
                                                        }
                                                        else
                                                        {
                                                            logger.Info($"更新版本不存在必需覆盖版本，暂时不更新");
                                                            isUtl = false;
                                                            msg = string.Empty;
                                                        }
                                                        #endregion
                                                    }
                                                    else
                                                    {
                                                        logger.Info($"目标程序:{targetAppName}未启动，开始处理更新程序集!");
                                                        isUtl = CommonHelper.UtlInSightCustomDll(cusList, baseDir, downFileDir, unZipDir, backupDir, ref msg);
                                                    }
                                                }
                                                catch (Exception err)
                                                {
                                                    isUtl = false;
                                                    msg = err.Message;
                                                }
                                                #endregion

                                                if (isUtl)
                                                {
                                                    logger.Info($"任务:{ea.DeliveryTag}的程序集更新成功!清空处理列表");
                                                    cusList.Clear();
                                                    isNewTask = true;
                                                    //回复确认处理成功
                                                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                                                }
                                                else
                                                {
                                                    if (string.IsNullOrEmpty(msg))
                                                    {
                                                        logger.Info($"目标程序已打开，且该任务不存在必需更新!，重新提交任务，下次更新");
                                                    }
                                                    else
                                                    {
                                                        logger.Info($"更新程序集失败!" + msg);
                                                    }
                                                    isNewTask = false;
                                                    //发生错误了，但是还可以重新提交给队列重新分配
                                                    channel.BasicNack(ea.DeliveryTag, false, true);
                                                    //暂停1秒，防止CPU爆满的问题
                                                    Thread.Sleep(Models.AppSettingModel.mqWaitTime);
                                                }
                                            }
                                            else
                                            {
                                                logger.Info($"处理拉取的文件失败!" + msg);
                                                isNewTask = false;
                                                //发生错误了，但是还可以重新提交给队列重新分配
                                                channel.BasicNack(ea.DeliveryTag, false, true);
                                                //暂停1秒，防止CPU爆满的问题
                                                System.Threading.Thread.Sleep(Models.AppSettingModel.mqWaitTime);
                                            }
                                        }
                                        else
                                        {
                                            logger.Info($"拉取服务器文件失败!" + msg);
                                            isNewTask = false;
                                            //发生错误了，但是还可以重新提交给队列重新分配
                                            channel.BasicNack(ea.DeliveryTag, false, true);
                                            //暂停1秒，防止CPU爆满的问题
                                            System.Threading.Thread.Sleep(Models.AppSettingModel.mqWaitTime);
                                        }
                                    }
                                    else
                                    {
                                        logger.Info($"连接文件服务器失败!" + msg);
                                        isNewTask = false;
                                        isPullFile = false;
                                        //发生错误了，但是还可以重新提交给队列重新分配
                                        channel.BasicNack(ea.DeliveryTag, false, true);
                                        //暂停N秒
                                        Thread.Sleep(Models.AppSettingModel.mqWaitTime);
                                    }
                                    #endregion
                                }
                                else  //当文件已拉取，判断是否解压
                                {
                                    if (!isUnZip)
                                    {
                                        #region 开始解压文件
                                        try
                                        {
                                            logger.Info($"开始解压文件!");
                                            isUnZip = CommonHelper.UnZipDownFiles(cusList, downFileDir, unZipDir, ref msg);
                                        }
                                        catch (Exception err)
                                        {
                                            isUnZip = false;
                                            msg = err.Message;
                                        }
                                        #endregion
                                        if (isUnZip)
                                        {
                                            #region 开始更新前操作
                                            try
                                            {
                                                var proAppName = Process.GetProcessesByName(targetAppName);
                                                if (proAppName != null && proAppName.Length > 0)
                                                {
                                                    #region 当打开了目标程序且必需更新，则先提示关闭，
                                                    logger.Info($"目标程序:{targetAppName}已启动时，开始处理更新程序集!");
                                                    //当程序在运行时，判断是否存在必需要覆盖的程序集
                                                    var entityList = cusList.Where(p => p.isNeed == true);
                                                    if (entityList != null && entityList.ToList().Count > 0)
                                                    {
                                                        //当存在时，服务弹出提示窗口，并在10秒后关闭程序
                                                        SendMessageHelper.ShowMessageBox("InSight定制版本更新提示", inSightAlert);
                                                        //暂停N秒，
                                                        Thread.Sleep(inSightWaitTime);
                                                        logger.Info($"等待N秒，后再查看目标程序是否打开，若打开则杀死！");
                                                        proAppName = Process.GetProcessesByName(targetAppName);
                                                        foreach (var pro in proAppName)
                                                        {
                                                            pro.Kill();
                                                        }
                                                        logger.Info($"目标程序:{targetAppName}未启动，开始处理更新程序集!");
                                                        isUtl = CommonHelper.UtlInSightCustomDll(cusList, baseDir, downFileDir, unZipDir, backupDir, ref msg);
                                                    }
                                                    else
                                                    {
                                                        logger.Info($"更新版本不存在必需覆盖版本，暂时不更新");
                                                        isUtl = false;
                                                        msg = string.Empty;
                                                    }
                                                    #endregion
                                                }
                                                else
                                                {
                                                    logger.Info($"目标程序:{targetAppName}未启动，开始处理更新程序集!");
                                                    isUtl = CommonHelper.UtlInSightCustomDll(cusList, baseDir, downFileDir, unZipDir, backupDir, ref msg);
                                                }
                                            }
                                            catch (Exception err)
                                            {
                                                isUtl = false;
                                                msg = err.Message;
                                            }
                                            #endregion

                                            if (isUtl)
                                            {
                                                logger.Info($"任务:{ea.DeliveryTag}的程序集更新成功!清空处理列表");
                                                cusList.Clear();
                                                isNewTask = true;
                                                //回复确认处理成功
                                                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                                            }
                                            else
                                            {
                                                if (string.IsNullOrEmpty(msg))
                                                {
                                                    logger.Info($"目标程序已打开，且该任务不存在必需更新!，重新提交任务，下次更新");
                                                }
                                                else
                                                {
                                                    logger.Info($"更新程序集失败!" + msg);
                                                }
                                                isNewTask = false;
                                                //发生错误了，但是还可以重新提交给队列重新分配
                                                channel.BasicNack(ea.DeliveryTag, false, true);
                                                //暂停1秒，防止CPU爆满的问题
                                                Thread.Sleep(Models.AppSettingModel.mqWaitTime);
                                            }
                                        }
                                        else
                                        {
                                            logger.Info($"处理拉取的文件失败!" + msg);
                                            isNewTask = false;
                                            //发生错误了，但是还可以重新提交给队列重新分配
                                            channel.BasicNack(ea.DeliveryTag, false, true);
                                            //暂停1秒，防止CPU爆满的问题
                                            System.Threading.Thread.Sleep(Models.AppSettingModel.mqWaitTime);
                                        }
                                    }
                                    else  //当文件已解压，再次更新
                                    {
                                        #region 开始更新前操作
                                        try
                                        {
                                            var proAppName = Process.GetProcessesByName(targetAppName);
                                            if (proAppName != null && proAppName.Length > 0)
                                            {
                                                #region 当打开了目标程序且必需更新，则先提示关闭，
                                                logger.Info($"目标程序:{targetAppName}已启动时，开始处理更新程序集!");
                                                //当程序在运行时，判断是否存在必需要覆盖的程序集
                                                var entityList = cusList.Where(p => p.isNeed == true);
                                                if (entityList != null && entityList.ToList().Count > 0)
                                                {
                                                    //当存在时，服务弹出提示窗口，并在10秒后关闭程序
                                                    SendMessageHelper.ShowMessageBox("InSight定制版本更新提示", inSightAlert);
                                                    //暂停N秒，
                                                    Thread.Sleep(inSightWaitTime);
                                                    logger.Info($"等待N秒，后再查看目标程序是否打开，若打开则杀死！");
                                                    proAppName = Process.GetProcessesByName(targetAppName);
                                                    foreach (var pro in proAppName)
                                                    {
                                                        pro.Kill();
                                                    }
                                                    logger.Info($"目标程序:{targetAppName}未启动，开始处理更新程序集!");
                                                    isUtl = CommonHelper.UtlInSightCustomDll(cusList, baseDir, downFileDir, unZipDir, backupDir, ref msg);
                                                }
                                                else
                                                {
                                                    logger.Info($"更新版本不存在必需覆盖版本，暂时不更新");
                                                    isUtl = false;
                                                    msg = string.Empty;
                                                }
                                                #endregion
                                            }
                                            else
                                            {
                                                logger.Info($"目标程序:{targetAppName}未启动，开始处理更新程序集!");
                                                isUtl = CommonHelper.UtlInSightCustomDll(cusList, baseDir, downFileDir, unZipDir, backupDir, ref msg);
                                            }
                                        }
                                        catch (Exception err)
                                        {
                                            isUtl = false;
                                            msg = err.Message;
                                        }
                                        #endregion

                                        if (isUtl)
                                        {
                                            logger.Info($"任务:{ea.DeliveryTag}的程序集更新成功!清空处理列表");
                                            cusList.Clear();
                                            isNewTask = true;
                                            //回复确认处理成功
                                            channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                                        }
                                        else
                                        {
                                            if (string.IsNullOrEmpty(msg))
                                            {
                                                logger.Info($"目标程序已打开，且该任务不存在必需更新!，重新提交任务，下次更新");
                                            }
                                            else
                                            {
                                                logger.Info($"更新程序集失败!" + msg);
                                            }
                                            isNewTask = false;
                                            //发生错误了，但是还可以重新提交给队列重新分配
                                            channel.BasicNack(ea.DeliveryTag, false, true);
                                            //暂停1秒，防止CPU爆满的问题
                                            Thread.Sleep(Models.AppSettingModel.mqWaitTime);
                                        }
                                    }
                                }
                                #endregion
                            }
                        }
                        else
                        {
                            logger.Info($"拉取任务:{ea.DeliveryTag}");
                            var msgBody = System.Text.Encoding.UTF8.GetString(ea.Body);
                            var entity = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.CustomVersionModel>(msgBody);
                            cusList.Add(entity);
                            //回复确认处理成功
                            channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                            logger.Info($"将任务:{ea.DeliveryTag}的程序集版本:{entity.version}添加到待处理列表");
                            //暂停1秒，防止CPU爆满的问题
                            System.Threading.Thread.Sleep(Models.AppSettingModel.mqTimeout);
                        }
                    };
                    //noAck设置false,告诉broker，发送消息之后，消息暂时不要删除，等消费者处理完成再说
                    channel.BasicConsume(Queue_Name, autoAck: false, consumer: consumer);
                }
            }
        }
    }
}
