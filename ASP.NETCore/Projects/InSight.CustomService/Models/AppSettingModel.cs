using System;
using System.Configuration;

namespace InSight.CustomService.Models
{
    public class AppSettingModel
    {
        public static Configuration configInstance = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap()
            {
                ExeConfigFilename = AppDomain.CurrentDomain.BaseDirectory+@"\App.config"
        }, ConfigurationUserLevel.None);
        public static int mqPort = Convert.ToInt32(configInstance.AppSettings.Settings["mqPort"].Value);
        public static string mqEndpointIP = configInstance.AppSettings.Settings["mqEndpointIP"].Value;
        public static string mqUserName = configInstance.AppSettings.Settings["mqUserName"].Value;
        public static string mqPassword = configInstance.AppSettings.Settings["mqPassword"].Value;
        public static string mqVirtualHost = configInstance.AppSettings.Settings["mqVirtualHost"].Value;
        public static string mqQueueName = configInstance.AppSettings.Settings["mqQueueName"].Value;
        public static string mqExchangeName = configInstance.AppSettings.Settings["mqExchangeName"].Value;
        public static int mqTimeout = Convert.ToInt32(configInstance.AppSettings.Settings["mqTimeout"].Value);
        public static int mqWaitTime = Convert.ToInt32(configInstance.AppSettings.Settings["mqWaitTime"].Value);
        public static string fileRemoteHost = configInstance.AppSettings.Settings["fileRemoteHost"].Value;
        public static string fileUserName = configInstance.AppSettings.Settings["fileUserName"].Value;
        public static string filePassword = configInstance.AppSettings.Settings["filePassword"].Value;
        public static string fileDirName = configInstance.AppSettings.Settings["fileDirName"].Value;
        public static string targetAppName = configInstance.AppSettings.Settings["targetAppName"].Value;
        public static string targetAppDir = configInstance.AppSettings.Settings["targetAppDir"].Value;
        public static string inSightAlert = configInstance.AppSettings.Settings["InSightAlert"].Value;
        public static int inSightWaitTime = Convert.ToInt32(configInstance.AppSettings.Settings["InSightWaitTime"].Value);
        /// <summary>
        /// 拉取后文件存在目录
        /// </summary>
        public static string filePullSaveDirName = "PullFiles";
        /// <summary>
        /// 拉取后文件解压目录
        /// </summary>
        public static string fileUnZipDirName = "UnZipFiles";
        /// <summary>
        /// 覆盖之前，文件备份目录
        /// </summary>
        public static string fileBackupDirName = "BackupFiles";
    } 
}
