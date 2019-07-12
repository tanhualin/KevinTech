using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InSight.CustomService
{
    public class CommonHelper
    {
        public static bool ConnectFileRemoteHost(string remoteHost, string userName, string passWord, ref string msg)
        {
            bool Flag = true;
            Process proc = new Process();
            proc.StartInfo.FileName = "cmd.exe";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.CreateNoWindow = true;
            try
            {
                proc.Start();
                string command = @"net  use  " + remoteHost + "  " + passWord + "  " + "  /user:" + userName + ">NUL";
                proc.StandardInput.WriteLine(command);
                command = "exit";
                proc.StandardInput.WriteLine(command);
                while (proc.HasExited == false)
                {
                    proc.WaitForExit(1000);
                }
                msg = proc.StandardError.ReadToEnd();
                if (msg != "")
                    Flag = false;
                proc.StandardError.Close();
            }
            catch (Exception ex)
            {
                Flag = false;
                msg = "连接远程文件服务器错误：" + ex.Message;
            }
            finally
            {
                proc.Close();
                proc.Dispose();
            }
            return Flag;
        }
        /// <summary>
        /// 拉取服务器文件
        /// </summary>
        /// <param name="cusList">待拉取的文件列表</param>
        /// <param name="downFileDir">存放文件路径</param>
        /// <param name="msg">错误信息</param>
        /// <returns></returns>
        public static bool pullRemoteHostFiles(List<Models.CustomVersionModel> cusList,string fileRemoteHost, string downFileDir, ref string msg)
        {
            bool result = true;
            foreach (var cv in cusList)
            {
                if (!string.IsNullOrEmpty(cv.verFilePath))
                {
                    var remoteFile = fileRemoteHost + cv.verFilePath;
                    if (cv.verFileExt == "zip")
                    {
                        #region 拉取压缩包
                        try
                        {
                            var sFilePath = Path.Combine(downFileDir, Path.GetFileName(cv.verFilePath));
                            if (!File.Exists(sFilePath))
                            {
                                File.Copy(remoteFile, sFilePath, true);
                            }
                        }
                        catch (Exception err)
                        {
                            result = false;
                            msg = $"拉取版本【{cv.version}】文件失败，" + err.Message;
                            break;
                        }
                        #endregion
                    }
                    else
                    {
                        #region 拉取DLL文件
                        try
                        {
                            var sDir = Path.Combine(downFileDir, cv.version);
                            if (!Directory.Exists(sDir))
                            {
                                Directory.CreateDirectory(sDir);
                            }
                            var sFilePath = Path.Combine(sDir, Path.GetFileName(cv.verFilePath));
                            if (!File.Exists(sFilePath))
                            {
                                File.Copy(remoteFile, sFilePath, true);
                            }
                        }
                        catch (Exception err)
                        {
                            result = false;
                            msg = $"拉取版本【{cv.version}】文件失败，" + err.Message;
                            break;
                        }
                        #endregion
                    }
                }
            }
            return result;
        }

        public static bool UnZipDownFiles(List<Models.CustomVersionModel> cusList,string downFileDir,string unZipFileDir,ref string msg)
        {
            bool result = true;
            foreach (var cv in cusList)
            {
                if (!string.IsNullOrEmpty(cv.verFilePath))
                {
                    if (cv.verFileExt == "zip")
                    {
                        #region 解压
                        try
                        {
                            var sFileName = Path.Combine(downFileDir, Path.GetFileName(cv.verFilePath));
                            var unZipPath = Path.Combine(unZipFileDir, cv.version);
                            ZipHelper.UnZip(sFileName, unZipPath);
                        }
                        catch (Exception err)
                        {
                            result = false;
                            msg = $"解压文件版本：{cv.version} 的文件：{cv.verFilePath} 出错，" + err.Message;
                        }
                        #endregion
                    }
                }
            }
            return result;
        }

        public static bool UtlInSightCustomDll(List<Models.CustomVersionModel> cusList, string baseDir, string downFileDir, string unZipFileDir, string backupFileDir, ref string msg)
        {
            bool result = true;
            //更新程序集
            foreach (var cv in cusList)
            {
                if (!string.IsNullOrEmpty(cv.verFilePath))
                {
                    if (cv.verFileExt == "zip")
                    {
                        #region 处理Zip文件
                        try
                        {
                            DirectoryInfo unZipFolder = new DirectoryInfo(Path.Combine(unZipFileDir, cv.version));
                            FileInfo[] unZipFiles = unZipFolder.GetFiles();
                            foreach (var file in unZipFiles)
                            {
                                var fileName = Path.Combine(baseDir, file.Name);
                                //若存在该DLL，则将该DLL复制到备份文件夹
                                if (File.Exists(fileName))
                                {
                                    var targetDir = Path.Combine(backupFileDir, cv.version);
                                    if (!Directory.Exists(targetDir))
                                    {
                                        Directory.CreateDirectory(targetDir);
                                    }
                                    var targetFilePath = Path.Combine(backupFileDir, cv.version, file.Name);
                                    File.Move(fileName, targetFilePath);
                                }
                                //将下载的DLL，复制到安装目录下
                                var sFilePath = Path.Combine(unZipFileDir, cv.version,
                                    file.Name);
                                File.Copy(sFilePath, fileName);
                            }
                        }
                        catch (Exception err)
                        {
                            result = false;
                            msg = $"更新文件版本:{cv.version}出错，" + err.Message;
                        }

                        #endregion
                    }
                    else
                    {
                        try
                        {
                            var fileName = Path.Combine(baseDir, Path.GetFileName(cv.verFilePath));
                            //若存在该DLL，则将该DLL复制到备份文件夹
                            if (File.Exists(fileName))
                            {
                                var targetDir = Path.Combine(backupFileDir, cv.version);
                                if (!Directory.Exists(targetDir))
                                    Directory.CreateDirectory(targetDir);
                                var targetFilePath = Path.Combine(targetDir, Path.GetFileName(cv.verFilePath));
                                File.Move(fileName, targetFilePath);
                            }

                            //将下载的DLL，复制到安装目录下
                            var sFilePath = Path.Combine(downFileDir, cv.version, Path.GetFileName(cv.verFilePath));
                            File.Copy(sFilePath, fileName, true);
                        }
                        catch (Exception err)
                        {
                            result = false;
                            msg = $"更新文件版本:{cv.version}出错，" + err.Message;
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 替换InSight定制功能版本
        /// </summary>
        /// <returns></returns>
        public static bool ReplaceInSightCustomDll(IList<Models.CustomVersionModel> cusList,string targetAppName,string baseDir,string downFileDir, string unZipFileDir,string backupFileDir,ref string msg)
        {
            bool result = true;
            var proAppName = Process.GetProcessesByName(targetAppName);
            if (proAppName != null && proAppName.Length > 0)
            {
                //当程序在运行时，判断是否存在必需要覆盖的程序集
                var entityList= cusList.Where(p => p.isNeed == true);
                if (entityList != null && entityList.ToList().Count > 0)
                {
                    //当存在时，服务弹出提示窗口，并在10秒后关闭程序

                }
            }
            else
            {
                #region 更新程序DLL
                foreach (var cv in cusList)
                {
                    if (!string.IsNullOrEmpty(cv.verFilePath))
                    {
                        if (cv.verFileExt == "zip")
                        {
                            #region 处理Zip文件
                            try
                            {
                                DirectoryInfo unZipFolder = new DirectoryInfo(Path.Combine(unZipFileDir, cv.version));
                                FileInfo[] unZipFiles = unZipFolder.GetFiles();
                                foreach (var file in unZipFiles)
                                {
                                    var fileName = Path.Combine(baseDir, file.Name);
                                    //若存在该DLL，则将该DLL复制到备份文件夹
                                    if (File.Exists(fileName))
                                    {
                                        var targetDir = Path.Combine(backupFileDir, cv.version);
                                        if (!Directory.Exists(targetDir))
                                        {
                                            Directory.CreateDirectory(targetDir);
                                        }
                                        var targetFilePath = Path.Combine(backupFileDir, cv.version, file.Name);
                                        File.Move(fileName, targetFilePath);
                                    }
                                    //将下载的DLL，复制到安装目录下
                                    var sFilePath = Path.Combine(unZipFileDir, cv.version,
                                        file.Name);
                                    File.Copy(sFilePath, fileName);
                                }
                            }
                            catch (Exception err)
                            {
                                result = false;
                                msg = $"更新文件版本:{cv.version}出错，" + err.Message;
                            }

                            #endregion
                        }
                        else
                        {
                            try
                            {
                                var fileName = Path.Combine(baseDir, Path.GetFileName(cv.verFilePath));
                                //若存在该DLL，则将该DLL复制到备份文件夹
                                if (File.Exists(fileName))
                                {
                                    var targetDir = Path.Combine(backupFileDir, cv.version);
                                    if (!Directory.Exists(targetDir))
                                        Directory.CreateDirectory(targetDir);
                                    var targetFilePath = Path.Combine(targetDir, Path.GetFileName(cv.verFilePath));
                                    File.Move(fileName, targetFilePath);
                                }

                                //将下载的DLL，复制到安装目录下
                                var sFilePath = Path.Combine(downFileDir, cv.version, Path.GetFileName(cv.verFilePath));
                                File.Copy(sFilePath, fileName, true);
                            }
                            catch (Exception err)
                            {
                                result = false;
                                msg = $"更新文件版本:{cv.version}出错，" + err.Message;
                            }
                        }
                    }
                }
                #endregion
            }
            return result;
        }
    }
}
