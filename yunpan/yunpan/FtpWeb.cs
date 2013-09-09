using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Globalization;
using System.Windows.Forms;
using System.Threading;
namespace yunpan
{
    public class FtpWeb
    {
        string ftpServerIP;
        string ftpRemotePath;
        string ftpUserID;
        string ftpPassword;
        string ftpURI;

        /// <summary>
        /// 连接FTP
        /// </summary>
        /// <param name="FtpServerIP">FTP连接地址</param>
        /// <param name="FtpRemotePath">指定FTP连接成功后的当前目录, 如果不指定即默认为根目录</param>
        /// <param name="FtpUserID">用户名</param>
        /// <param name="FtpPassword">密码</param>
        public FtpWeb(string FtpServerIP, string FtpRemotePath, string FtpUserID, string FtpPassword)
        {
            ftpServerIP = FtpServerIP;
            ftpRemotePath = FtpRemotePath;
            ftpUserID = FtpUserID;
            ftpPassword = FtpPassword;
            ftpURI = "ftp://" + ftpServerIP + "/" + ftpRemotePath + "/";
        }

        /// <summary>
        /// 上传
        /// </summary>
        /// <param name="filename"></param>
        public bool Upload(string filename, String dir,progress pr)
        {
            ListViewItem lvTmp = null;
           
            FileInfo fileInf = new FileInfo(filename);
            lock (this) 
            {
                try
                {
                    for (int i = 0; i < pr.listView1.Items.Count; i++)
                    {

                        if (fileInf.ToString() == pr.listView1.Items[i].SubItems[0].Text)
                        {

                            lvTmp = pr.listView1.Items[i];
                            break;
                        }
                    }
                }
                catch (Exception)
                {
                    
                   
                }
            }          
            string uri = "ftp://" + ftpServerIP + "/" + fileInf.Name;
            FtpWebRequest reqFTP;
            // Console.WriteLine(uri);
            // 根据uri创建FtpWebRequest对象 
            reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + ftpServerIP + "/" + dir + "/" + fileInf.Name));
            // ftp用户名和密码 
            reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
            // 默认为true，连接不会被关闭 
            // 在一个命令之后被执行 
          //  reqFTP.KeepAlive = false;
            reqFTP.Proxy = null;
            // 指定执行什么命令 
            reqFTP.Method = WebRequestMethods.Ftp.UploadFile;

            // 指定数据传输类型 
          //  reqFTP.UseBinary = true;

            // 上传文件时通知服务器文件的大小 
            reqFTP.ContentLength = fileInf.Length;

            // 缓冲大小设置为2kb 
            int buffLength = 2048*4;

            byte[] buff = new byte[buffLength];
            int contentLen;

          
           
            try
            {
                // 打开一个文件流 (System.IO.FileStream) 去读上传的文件 
                FileStream fs = fileInf.OpenRead();
                // 把上传的文件写入流 
                Stream strm = reqFTP.GetRequestStream();

                // 每次读文件流的2kb 
                contentLen = fs.Read(buff, 0, buffLength);

                // 流内容没有结束 
                long totalLen = 0;
                int last = 0;
                int now;
                while (contentLen != 0)
                {
                    // 把内容从file stream 写入 upload stream 
                    strm.Write(buff, 0, contentLen);

                    contentLen = fs.Read(buff, 0, buffLength);
                    totalLen += contentLen;
                    now = (int)((double)(((double)totalLen) / fileInf.Length) * 100);
                    if (  now> last) 
                    {
                        if (lvTmp != null)
                        {
                            lvTmp.SubItems[3].Text = now + "%";
                        }
                        
                        last = now;
                    }
                  
                }

                // 关闭两个流 
                strm.Close();
                fs.Close();

                FtpWebResponse myResponse = (FtpWebResponse)reqFTP.GetResponse();
                myResponse.Close();

                if (lvTmp != null) {
                    pr.addCompletedFile(lvTmp, "上传");
                }

                return true;
            }
            catch (Exception ex)
           {
                pr.addFailedFile(lvTmp);
                return false;
                throw;
            }
        }
       
        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileName"></param>
     
        public bool Download(fileValue localFile, string path,progress pr)
        {
            ListViewItem lvTmp = null;

            FileInfo fileInf = new FileInfo(localFile.pwd);
            for (int i = 0; i < pr.listView1.Items.Count; i++)
            {

                try
                {
                    if (fileInf.ToString() == pr.listView1.Items[i].SubItems[0].Text)
                    {

                        lvTmp = pr.listView1.Items[i];
                    }
                }
                catch (Exception)
                {
                    
                   
                }
            }
            FtpWebRequest reqFTP;
           // try
            //{
                FileStream outputStream = new FileStream(fileInf.ToString(), FileMode.Create);
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpURI + path));
              
                reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                long cl = response.ContentLength;
                int bufferSize = 2048;
                int readCount;
                byte[] buffer = new byte[bufferSize];

                readCount = ftpStream.Read(buffer, 0, bufferSize);
                while (readCount > 0)
                {
                    outputStream.Write(buffer, 0, readCount);
                    readCount = ftpStream.Read(buffer, 0, bufferSize);
                }
                //可以添加进度条
                ftpStream.Close();
                outputStream.Close();
                response.Close();
                fileInf.LastWriteTime = localFile.lastWriteTime;
                if (lvTmp != null) 
                {
                    pr.addCompletedFile(lvTmp, "下载");
                }
                return true;
           // }
          //  catch (Exception ex)
           // {

              //  Insert_Standard_ErrorLog.Insert("FtpWeb", "Download Error --> " + ex.Message);
              //  return false;
           // }
        }


        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="fileName"></param>
        public void Delete(string fileName)
        {
            try
            {
                string uri = ftpURI + fileName;
                FtpWebRequest reqFTP;
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));

                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                reqFTP.KeepAlive = false;
                reqFTP.Method = WebRequestMethods.Ftp.DeleteFile;

                string result = String.Empty;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                long size = response.ContentLength;
                Stream datastream = response.GetResponseStream();
                StreamReader sr = new StreamReader(datastream);
                result = sr.ReadToEnd();
                sr.Close();
                datastream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                Insert_Standard_ErrorLog.Insert("FtpWeb", "Delete Error --> " + ex.Message + "  文件名:" + fileName);
            }
        }

        /// <summary>
        /// 删除文件夹
        /// </summary>
        /// <param name="folderName"></param>
        public void RemoveDirectory(string folderName)
        {
            try
            {
                string uri = ftpURI + folderName;
                FtpWebRequest reqFTP;
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));

                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                reqFTP.KeepAlive = false;
                reqFTP.Method = WebRequestMethods.Ftp.RemoveDirectory;

                string result = String.Empty;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                long size = response.ContentLength;
                Stream datastream = response.GetResponseStream();
                StreamReader sr = new StreamReader(datastream);
                result = sr.ReadToEnd();
                sr.Close();
                datastream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                Insert_Standard_ErrorLog.Insert("FtpWeb", "Delete Error --> " + ex.Message + "  文件名:" + folderName);
            }
        }

        /// <summary>
        /// 获取当前目录下明细(包含文件和文件夹)
        /// </summary>
        /// <returns></returns>
        public string[] GetFilesDetailList()
        {
            string[] downloadFiles;
            try
            {
                StringBuilder result = new StringBuilder();
                FtpWebRequest ftp;
                ftp = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpURI));
              
                ftp.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                ftp.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                WebResponse response = ftp.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.Default);

                //while (reader.Read() > 0)
                //{

                //}
                string line = reader.ReadLine();
                //line = reader.ReadLine();
                //line = reader.ReadLine();

                while (line != null)
                {
                    result.Append(line);
                    result.Append("\n");
                    line = reader.ReadLine();
                }
                result.Remove(result.ToString().LastIndexOf("\n"), 1);
                reader.Close();
                response.Close();
                return result.ToString().Split('\n');
            }
            catch (Exception ex)
            {
                downloadFiles = null;
                Insert_Standard_ErrorLog.Insert("FtpWeb", "GetFilesDetailList Error --> " + ex.Message);
                return downloadFiles;
            }
        }

        /// <summary>
        /// 获取当前目录下文件列表(仅文件)
        /// </summary>
        /// <returns></returns>
        public string[] GetFileList(string mask)
        {
            string[] downloadFiles;
            StringBuilder result = new StringBuilder();
            FtpWebRequest reqFTP;
            try
            {
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpURI));
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                reqFTP.Method = WebRequestMethods.Ftp.ListDirectory;
                WebResponse response = reqFTP.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.Default);

                string line = reader.ReadLine();
                while (line != null)
                {
                    if (mask.Trim() != string.Empty && mask.Trim() != "*.*")
                    {

                        string mask_ = mask.Substring(0, mask.IndexOf("*"));
                        if (line.Substring(0, mask_.Length) == mask_)
                        {
                            result.Append(line);
                            result.Append("\n");
                        }
                    }
                    else
                    {
                        result.Append(line);
                        result.Append("\n");
                    }
                    line = reader.ReadLine();
                }
                result.Remove(result.ToString().LastIndexOf('\n'), 1);
                reader.Close();
                response.Close();
                return result.ToString().Split('\n');
            }
            catch (Exception ex)
            {
                downloadFiles = null;
                if (ex.Message.Trim() != "远程服务器返回错误: (550) 文件不可用(例如，未找到文件，无法访问文件)。")
                {
                    Insert_Standard_ErrorLog.Insert("FtpWeb", "GetFileList Error --> " + ex.Message.ToString());
                }
                return downloadFiles;
            }
        }

        /// <summary>
        /// 获取当前目录下所有的文件夹列表(仅文件夹)
        /// </summary>
        /// <returns></returns>
        public string[] GetDirectoryList()
        {
            string[] drectory = GetFilesDetailList();
            string m = string.Empty;
            foreach (string str in drectory)
            {
                int dirPos = str.IndexOf("<DIR>");
                if (dirPos > 0)
                {
                    /*判断 Windows 风格*/
                    m += str.Substring(dirPos + 5).Trim() + "\n";
                }
                else if (str.Trim().Substring(0, 1).ToUpper() == "D")
                {
                    /*判断 Unix 风格*/
                    string dir = str.Substring(54).Trim();
                    if (dir != "." && dir != "..")
                    {
                        m += dir + "\n";
                    }
                }
            }

            char[] n = new char[] { '\n' };
            return m.Split(n);
        }

        /// <summary>
        /// 判断当前目录下指定的子目录是否存在
        /// </summary>
        /// <param name="RemoteDirectoryName">指定的目录名</param>
        public bool DirectoryExist(string RemoteDirectoryName)
        {
            string[] dirList = GetDirectoryList();
            foreach (string str in dirList)
            {
                if (str.Trim() == RemoteDirectoryName.Trim())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 判断当前目录下指定的文件是否存在
        /// </summary>
        /// <param name="RemoteFileName">远程文件名</param>
        public bool FileExist(string RemoteFileName)
        {
            string[] fileList = GetFileList("*.*");
            foreach (string str in fileList)
            {
                if (str.Trim() == RemoteFileName.Trim())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 创建文件夹
        /// </summary>
        /// <param name="dirName"></param>
        /// 
        public void MakeDirV2(string dirName)
        {
            String[] filename = dirName.Split('/');
            String file = "";
            if (!dirName.Contains("/"))
            {
                filename = dirName.Split('\\');
            }
            for (int i = 0; i < filename.Length; i++)
            {
                file += "/" + filename[i];
                //if (!DirectoryExist(file))
               // {
                    MakeDir(file);
               // }
            }
        }
        public void MakeDir(string dirName)
        {
            FtpWebRequest reqFTP;
            try
            {
                // dirName = name of the directory to create.
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpURI + dirName));
                reqFTP.Method = WebRequestMethods.Ftp.MakeDirectory;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();

                ftpStream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                Insert_Standard_ErrorLog.Insert("FtpWeb", "MakeDir Error --> " + ex.Message);
            }
        }

        /// <summary>
        /// 获取指定文件大小
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public long GetFileSize(string filename)
        {
            FtpWebRequest reqFTP;
            long fileSize = 0;
            try
            {
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpURI + filename));
                reqFTP.Method = WebRequestMethods.Ftp.GetFileSize;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                fileSize = response.ContentLength;

                ftpStream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                Insert_Standard_ErrorLog.Insert("FtpWeb", "GetFileSize Error --> " + ex.Message);
            }
            return fileSize;
        }

        /// <summary>
        /// 改名
        /// </summary>
        /// <param name="currentFilename"></param>
        /// <param name="newFilename"></param>
        public void ReName(string currentFilename, string newFilename)
        {
            FtpWebRequest reqFTP;
            try
            {
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpURI + currentFilename));
              //  MessageBox.Show(ftpURI + currentFilename);
                reqFTP.Method = WebRequestMethods.Ftp.Rename;
                reqFTP.RenameTo = newFilename;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();

                ftpStream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                Insert_Standard_ErrorLog.Insert("FtpWeb", "ReName Error --> " + ex.Message);
            }
        }
        /*string[] dirs=currentFilename.Split('\\');
           string dir = "";
           string filename="";
           int i = 0;
           for ( i = 0; i < dirs.Length - 1; i++) 
           {
               dir += "\\" + dirs[i];
           }
           filename=dirs[i];
           MessageBox.Show(dir + " "+filename);
           GotoDirectory(dir,true);
           if (FileExist(filename)) 
           {
               MessageBox.Show(dir + " " + filename);
           }*/

        /// <summary>
        /// 移动文件
        /// </summary>
        /// <param name="currentFilename"></param>
        /// <param name="newFilename"></param>
        public void MovieFile(string currentFilename, string newDirectory)
        {
          
          string[] dirs=currentFilename.Split('\\');
          string dir = "";
          string filename="";
          int i = 0;
          for ( i = 0; i < dirs.Length - 1; i++) 
          {
              dir += "\\" + dirs[i];
          }
          filename = dirs[i];
          string[] dirs2 = newDirectory.Split('\\');
          string dirTmp="";
          for (i = 0; i < dirs2.Length - 1; i++) 
          {
              dirTmp += "/" + dirs2[i];
              
              MakeDir(dirTmp);
             
          }
          GotoDirectory(dir, true);
         
          ReName(filename, newDirectory);
        }

        /// <summary>
        /// 切换当前目录
        /// </summary>
        /// <param name="DirectoryName"></param>
        /// <param name="IsRoot">true 绝对路径   false 相对路径</param>
        public void GotoDirectory(string DirectoryName, bool IsRoot)
        {
            if (IsRoot)
            {
                ftpRemotePath = DirectoryName;
            }
            else
            {
                ftpRemotePath += DirectoryName + "/";
            }
            ftpURI = "ftp://" + ftpServerIP + "/" + ftpRemotePath + "/";
        }
        public bool Upload(string filename, String dir)
        {


            FileInfo fileInf = new FileInfo(filename);

            string uri = "ftp://" + ftpServerIP + "/" + fileInf.Name;
            FtpWebRequest reqFTP;
            // Console.WriteLine(uri);
            // 根据uri创建FtpWebRequest对象 
            reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + ftpServerIP + "/" + dir + "/" + fileInf.Name));

            // ftp用户名和密码 
            reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);

            // 默认为true，连接不会被关闭 
            // 在一个命令之后被执行 
            reqFTP.KeepAlive = false;

            // 指定执行什么命令 
            reqFTP.Method = WebRequestMethods.Ftp.UploadFile;

            // 指定数据传输类型 
            reqFTP.UseBinary = true;

            // 上传文件时通知服务器文件的大小 
            reqFTP.ContentLength = fileInf.Length;

            // 缓冲大小设置为2kb 
            int buffLength = 2048;

            byte[] buff = new byte[buffLength];
            int contentLen;

            // 打开一个文件流 (System.IO.FileStream) 去读上传的文件 
            FileStream fs = fileInf.OpenRead();
            try
            {
                // 把上传的文件写入流 
                Stream strm = reqFTP.GetRequestStream();

                // 每次读文件流的2kb 
                contentLen = fs.Read(buff, 0, buffLength);

                // 流内容没有结束 
                while (contentLen != 0)
                {
                    // 把内容从file stream 写入 upload stream 
                    strm.Write(buff, 0, contentLen);

                    contentLen = fs.Read(buff, 0, buffLength);
                }

                // 关闭两个流 
                strm.Close();
                fs.Close();
                return true;
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Upload Error");
                return false;
                throw;
                //MessageBox.Show(ex.Message, "Upload Error");
            }
        }

        /// <summary>
        /// 删除订单目录
        /// </summary>
        /// <param name="ftpServerIP">FTP 主机地址</param>
        /// <param name="folderToDelete">FTP 用户名</param>
        /// <param name="ftpUserID">FTP 用户名</param>
        /// <param name="ftpPassword">FTP 密码</param>
        public static void DeleteOrderDirectory(string ftpServerIP, string folderToDelete, string ftpUserID, string ftpPassword)
        {
            try
            {
                if (!string.IsNullOrEmpty(ftpServerIP) && !string.IsNullOrEmpty(folderToDelete) && !string.IsNullOrEmpty(ftpUserID) && !string.IsNullOrEmpty(ftpPassword))
                {
                    FtpWeb fw = new FtpWeb(ftpServerIP, folderToDelete, ftpUserID, ftpPassword);
                    //进入订单目录
                    fw.GotoDirectory(folderToDelete, true);
                    //获取规格目录
                    string[] folders = fw.GetDirectoryList();
                    foreach (string folder in folders)
                    {
                        if (!string.IsNullOrEmpty(folder) || folder != "")
                        {
                            //进入订单目录
                            string subFolder = folderToDelete + "/" + folder;
                            fw.GotoDirectory(subFolder, true);
                            //获取文件列表
                            string[] files = fw.GetFileList("*.*");
                            if (files != null)
                            {
                                //删除文件
                                foreach (string file in files)
                                {
                                    fw.Delete(file);
                                }
                            }
                            //删除冲印规格文件夹
                            fw.GotoDirectory(folderToDelete, true);
                            fw.RemoveDirectory(folder);
                        }
                    }

                    //删除订单文件夹
                    string parentFolder = folderToDelete.Remove(folderToDelete.LastIndexOf('/'));
                    string orderFolder = folderToDelete.Substring(folderToDelete.LastIndexOf('/') + 1);
                    fw.GotoDirectory(parentFolder, true);
                    fw.RemoveDirectory(orderFolder);
                }
                else
                {
                    throw new Exception("FTP 及路径不能为空！");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("删除订单时发生错误，错误信息为：" + ex.Message);
            }
        }
    }


    public class Insert_Standard_ErrorLog
    {
        public static void Insert(string x, string y)
        {

        }
    }
}
