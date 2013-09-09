using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Data;
using System.Windows.Forms;
namespace yunpan
{
    public class checkfile
    {
        public static ArrayList DBfiles = new ArrayList();
        private System.Collections.ArrayList arrayFile = new System.Collections.ArrayList();
        static public ArrayList downFiles = new ArrayList();
        static public ArrayList upFiles = new ArrayList();
        static public ArrayList deleteFiles = new ArrayList();
        public ArrayList localfiles = new ArrayList();
        private ArrayList recyclefiles = new ArrayList();
        private ArrayList DBTmpFiles = new ArrayList();
        public checkfile()
        {
           
      
        }
        sql db = new sql();
        private void LoadDB()
        {
            
            
            String sql_string = "select filename,lastWT,dir from filesindex where user='"+globalTools.getUser()+"'";//可以改进为存储过程
            DataTable dt = db.readDt(sql_string);
            String sql_recycle = "select filename,lastWT,dir from recyclebox where  user='"+globalTools.getUser()+"'";//可以改进为存储过程
            DataTable dt_re = db.readDt(sql_recycle);
            //load recyclebox data from remoto database
            recyclefiles.Clear();
            for (int i = 0; i < dt_re.Rows.Count; i++) 
            {
                DataRow dr = dt_re.Rows[i];
                DateTime lastWriteTime = DateTime.FromFileTime(long.Parse(dr[1].ToString()));
                String tmpKey = globalTools.remoteFileToLocalFile(dr[0].ToString(),dr[2].ToString());
                fileValue f = new fileValue(tmpKey, lastWriteTime);
                recyclefiles.Add(f);
            }
           // MessageBox.Show(""+dt.Rows.Count);
            DBTmpFiles.Clear();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                    
                    DataRow dr = dt.Rows[i];

                    DateTime lastWriteTime = DateTime.FromFileTime(long.Parse(dr[1].ToString()));
                    String tmpKey = globalTools.remoteFileToLocalFile(dr[0].ToString(), dr[2].ToString());
                    fileValue f = new fileValue(tmpKey, lastWriteTime);
                    updatelist(DBfiles,f);
                    DBTmpFiles.Add(f);
                    //fileTools.newFile(tmpKey);
                    
            }
            //remote deletefiles
            for (int i = 0; i < DBfiles.Count; i++) 
            {
                
                if (!DBTmpFiles.Contains((fileValue)DBfiles[i])) 
                {
                    fileValue file = (fileValue)DBfiles[i];
                    string sql_compare = "select id from filesindex where filename='" + file.name + "'and dir='" + file.dir.Replace("\\", "&") + "' and user='" + globalTools.getUser() + "' and lastWT=" + file.lastWriteTime.ToFileTime() ;
                    DataTable dt1 = db.readDt(sql_compare);
                    if (dt1.Rows.Count > 0)
                    {
                        deleteFiles.Add(DBfiles[i]);
                        DBfiles.Remove(DBfiles[i]);
                    }
                }
            }
        }
        public void updateDBTmplist(fileValue fileTarget) 
        {
            if (!DBTmpFiles.Contains(fileTarget))
            {
                DBTmpFiles.Add(fileTarget);
            }
        }
        public void updatelist(ArrayList tarList,fileValue fileTarget) 
        {
          
            int flag = 1;
            for (int j = 0; j < tarList.Count; j++)
            {
                fileValue dbfileTmp = (fileValue)tarList[j];
                if (dbfileTmp.pwd == fileTarget.pwd && dbfileTmp.lastWriteTime < fileTarget.lastWriteTime)
                {
                    tarList.Remove(dbfileTmp);
                    tarList.Add(fileTarget);
                    flag = 0;
                    break;
                }
                else if (dbfileTmp.pwd == fileTarget.pwd)
                {
                    flag = 0;
                    break;
                }
            }
            if (flag == 1)
            {
                
                tarList.Add(fileTarget);

            }
            

        } 
        public void scheduling(progress pr) 
        {
            pr.checkFunctionFlag = 0;
            //add localfiles
            //scan local directory
            readDir(globalTools.getLocalDir());
            //add new files
            for (int i = 0; i < arrayFile.Count; i++) 
            {
                fileValue fileTmp =(fileValue)arrayFile[i];
                updatelist(localfiles,fileTmp);
            }
            //scan remote directory
            LoadDB();
           
            //localfiles compare with dbfiles(remoteData) to add waiting for upload files
          
            for (int i = 0; i < localfiles.Count; i++)
            {
                fileValue fileToUpload = (fileValue)localfiles[i];
                if (!DBfiles.Contains(fileToUpload))
                {
                  
                    if (isInRecyclyBox(fileToUpload)) 
                    {
                        //remove the file
                        MessageBox.Show("recycle");
                        File.Delete(fileToUpload.pwd);
                        localfiles.Remove(fileToUpload);
                        continue;
                    }
                    //add to upload files if localfile_lastwritetime >_DBfiles_lastwritetiem
                    if (isInRemoteFiles(fileToUpload))
                    {
                        if (addFiletoArray(upFiles, fileToUpload,1))
                        {
                           
                            pr.addloadingFile(fileToUpload, "上传");
                        }

                    }
                    else 
                    {
                        //to down new version files
                        //cover the old version files
                    }
                   
                }
              
            }
          
            for (int i = 0; i < DBfiles.Count; i++)
            {
                if (!localfiles.Contains(DBfiles[i]) && !deleteFiles.Contains(DBfiles[i]))
                {
                    //add to download files
                   
                    fileValue fileTodownLoad = (fileValue)DBfiles[i];
                    FileInfo fileTmp = new FileInfo(fileTodownLoad.pwd);
                    if (addFiletoArray(downFiles, fileTodownLoad,2))
                    {
                        if (!File.Exists(fileTodownLoad.pwd))
                        {
                            pr.addloadingFile(fileTodownLoad, "下载");
                        }
                        else if (fileTmp.LastWriteTime < fileTodownLoad.lastWriteTime) 
                        {
                            pr.addloadingFile(fileTodownLoad, "下载");
                        }
                    }

                }
                
            }
          
       
          
            //compare localfiles and waiting for delecting files
     
            //add waiting for delete files
            
            for (int i = 0; i < localfiles.Count; i++)
            {
                if (!arrayFile.Contains(localfiles[i]) && !upFiles.Contains(localfiles[i]))
                {
                    if (!deleteFiles.Contains(localfiles[i])&&!File.Exists(((fileValue)localfiles[i]).pwd))
                    {
                       // FileInfo f=new FileInfo(((fileValue)localfiles[i]).pwd);
                        //需要修改
                        fileValue f = (fileValue)localfiles[i];
                        deleteFiles.Add(localfiles[i]);
                      //  MessageBox.Show("add" + ((fileValue)localfiles[i]).pwd + "time  " + ((fileValue)localfiles[i]).lastWriteTime);
                        localfiles.Remove(f);
                        if (DBfiles.Contains(f)) 
                        {
                            DBfiles.Remove(f);
                        }
                    }
                }
            }
          
           
            pr.checkFunctionFlag = 1;
        }
        // 
        private bool isInRecyclyBox(fileValue tarFile) 
        {
            if (recyclefiles.Contains(tarFile)) 
            {
                return true;
            }
            for (int i = 0; i < recyclefiles.Count; i++) 
            {
                fileValue tmp=(fileValue)recyclefiles[i];
                if (tmp.name == tarFile.name&&tmp.dir==tarFile.dir && tmp.lastWriteTime > tarFile.lastWriteTime) 
                {
                    return true;
                }
            }
            return false;
        }
         /// <summary>
         /// 判断是否是一个远程文件
         /// </summary>
         /// <param name="tarFile"></param>
         /// <returns></returns>
        private bool isInRemoteFiles(fileValue tarFile)
        {
            for (int i = 0; i < DBfiles.Count; i++)
            {
                fileValue tmp = (fileValue)DBfiles[i];
                if (tmp.pwd == tarFile.pwd &&  tmp.lastWriteTime < tarFile.lastWriteTime)
                {
                    //if (tmp.lastWriteTime < tarFile.lastWriteTime)
                       //  MessageBox.Show(tarFile.name + "  " + tarFile.dir + "  " + tarFile.lastWriteTime.ToString() + "**" + tmp.name + "  " + tmp.dir + "  " + tmp.lastWriteTime.ToString());
                    //MessageBox.Show("xx");
                    //should upload
                    return true;
                }
                else if (tmp.pwd == tarFile.pwd  && tmp.lastWriteTime > tarFile.lastWriteTime) 
                {
                    //should download
                    return false;
                }
            }
           
            //should upload
            return true;
        }
        private bool addFiletoArray(ArrayList tar, fileValue tarFile,int typeFlag)
        {
            
            int flag=0;
            int i = 0;
            if (tar.Contains(tarFile)) 
            {
                return false;
            }
            if (typeFlag == 1 && upLoadToServer.uploadingfile.Contains(tarFile)) 
            {        
                return false;
            }
            if (typeFlag == 2 && downFromServer.downLoadingfile.Contains(tarFile))
            {
                return false;
            }

            for (i = 0; i < tar.Count; i++) 
            {
               
                fileValue fileTmp = (fileValue)tar[i];

                if (fileTmp == tarFile )
                {
                    flag = 1;
                    break;
                }
                else if(fileTmp.pwd==tarFile.pwd&& fileTmp.lastWriteTime<tarFile.lastWriteTime)
                {
                    flag = 2;
                    break;
                }
            }
            if (flag == 0) 
            {
                if (!tar.Contains(tarFile)) 
                {
                    
                    tar.Add(tarFile);
                    return true;
                }
               
            }
            else if (flag == 2) 
            {
               
                tar.Remove(tar[i]);
                tar.Add(tarFile);
            }
           
            return false;
           
        }
     
    
        private void readDir(String dir) 
        {
            arrayFile.Clear();
            arraylist.Clear();
            GetSubDir(dir);
            foreach (var s in arraylist)
            {
                // Console.WriteLine(s);
                FileInfo fileInfo = new FileInfo(s.ToString());

                arrayFile.Add(new fileValue(fileInfo));

            }
           
        }
        private  System.Collections.ArrayList arraylist = new System.Collections.ArrayList();
        private  void GetSubDir(string s1)//递归获取文件夹中所有的文件名，并存入数组 
        {
            string[] sDir = Directory.GetDirectories(s1);//获取子目录的名称 
            string[] sFile = Directory.GetFiles(s1);//获取文件 
 
            for (int i = 0; i < sFile.Length; i++)
            {
                //如果在下载队列里面
                if (!isInDownLoadfiles(sFile[i]))
                {
                    FileInfo ftmp=new FileInfo(sFile[i]);
                    if (!(ftmp.CreationTime == ftmp.LastWriteTime && ftmp.CreationTime>DateTime.Now.AddSeconds(-10)))
                    {
                        arraylist.Add(sFile[i]);  
                    }
                   
                } //将文件加入数组 

            }
            for (int i = 0; i < sDir.Length; i++)
            {
                //arraylist.Add(sDir[i]);
                GetSubDir(sDir[i]);
            }
        }
        private bool isInDownLoadfiles(string tarFile) 
        {
            
            for (int i = 0; i < downFiles.Count; i++) 
            {

                if (tarFile == ((fileValue)downFiles[i]).pwd) 
                {
                    return true;
                }
            }
            for(int i = 0; i <  downFromServer.downLoadingfile.Count; i++)
            {
                 if (tarFile == ((fileValue)downFromServer.downLoadingfile[i]).pwd) 
                {
                    return true;
                }
            }
            return false;
        }
    }
}
