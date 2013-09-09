using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using System.IO;
using System.Windows.Forms;
using System.Threading;
namespace yunpan
{
    class downFromServer
    {
        public downFromServer() 
        {

        }
        FtpWeb fw = new FtpWeb("192.168.1.153", "", "daidai", "1");
        bool flagOpen = true;
        progress prtmp;
        public static ArrayList downLoadingfile = new ArrayList();
        int Count1=0;
        public void downLoadFiles(progress pr)
        {
            prtmp = pr;
            pr.downLoadFunctionFlag = 0;
            //MessageBox.Show("start" + checkfile.downFiles.Count);
            if (flagOpen) 
            {
                flagOpen = false;
                while (checkfile.downFiles.Count > 0)
                {

                    fileValue fileToDownload = (fileValue)checkfile.downFiles[0];
                    if (!Directory.Exists(fileToDownload.dir))
                    {
                        Directory.CreateDirectory(fileToDownload.dir);
                    }
                    if (globalTools.runningThreadNum <= globalTools.getMaxThreadNum())
                    {

                      //修改
                        lock (this)
                        {
                            checkfile.downFiles.Remove(fileToDownload);
                            downLoadingfile.Add(fileToDownload);
                        }
                        Count1++;
                        Interlocked.Increment(ref globalTools.runningThreadNum);
                        ThreadPool.QueueUserWorkItem(new WaitCallback(downLoadOneFile), fileToDownload);

                    }
                    
                    
                    // MessageBox.Show(filesList[i].ToString());
                }
                flagOpen = true;
            }
            pr.trackProcess("下载完成");
            pr.downLoadFunctionFlag = 1;
           // MessageBox.Show("over" + Count1);
           
        }
        private void downLoadOneFile(object filetmp)
        {
            fileValue fileToDownload = (fileValue)filetmp;
            prtmp.trackProcess("正在下载" + fileToDownload.name + "@@@" + Thread.CurrentThread.GetHashCode());
            if (fw.Download(fileToDownload, globalTools.localFileToRemoteFile(fileToDownload.pwd), prtmp))
             {

                 downLoadingfile.Remove(fileToDownload);
                //该覆盖
                 
                 for (int i = 0; i < prtmp.checker.localfiles.Count; i++) 
                 {
                     fileValue fileT=(fileValue)prtmp.checker.localfiles[i];
                     if (fileT.pwd == fileToDownload.pwd) 
                     {

                         prtmp.checker.localfiles.Remove(fileT);
                         break;
                     }
                 }
                 prtmp.checker.localfiles.Add(fileToDownload);

              //  prtmp.checker.updatelist(prtmp.checker.localfiles, fileToDownload);
                //对arraylist做改动
                 prtmp.trackProcess("");
                 Thread.Sleep(1);
             }
             else
             {

                        checkfile.downFiles.Remove(fileToDownload);
                        checkfile.downFiles.Add(fileToDownload);
                        prtmp.trackProcess("");
                        MessageBox.Show(fileToDownload.pwd + "下载错误");
             }
            Interlocked.Decrement(ref globalTools.runningThreadNum);
        }
        
    }
}
