using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Windows.Forms;
namespace yunpan
{
    class delectFile
    {
        FtpWeb fw = new FtpWeb("202.118.75.113", "", "daidai", "1");
        bool flagOpen = true;
        sql db = new sql();
        public void delectFiles(progress pr)
        {
            pr.deleteFunctionFlag = 0;
            if (flagOpen)
            {
                flagOpen = false;
                while (checkfile.deleteFiles.Count > 0)
                {
                    
                    fileValue fileToDelete = (fileValue)checkfile.deleteFiles[0];
                    pr.trackProcess ("删除"+fileToDelete.name);
                    //1,delete local files,and delete remote files
                    if (File.Exists(fileToDelete.pwd)) 
                    {
                        try
                        {
                            MessageBox.Show("delete"+fileToDelete.pwd);
                            File.Delete(fileToDelete.pwd);

                        }
                        catch (Exception)
                        {
                            //文件可能被占用
                            
                            continue;
                        }
                    }
                    string remoteFile = globalTools.localFileToRemoteFile(fileToDelete.pwd);
                    string recycleFile=globalTools.getRecycleDir()+ remoteFile.Substring(globalTools.getRemoteDir().Length,remoteFile.Length-globalTools.getRemoteDir().Length);
                    fw.MovieFile(remoteFile, recycleFile);
                    try
                    {
                        
                    }
                    catch (Exception)
                    {
                        checkfile.deleteFiles.Remove(fileToDelete);
                        MessageBox.Show("删除失败");
                        return;
                    }
                    //2,refresh remote database and update recycle table
                    if (!refreshDB(fileToDelete)) 
                    {
                         //log  fail tp refresh remote database
                    }
    
                    //3,refresh dbFilesQueue,localfilesQueue,deleteFiles.count
                    if (!refreshQueue(fileToDelete)) 
                    {
                        //log  fail tp refresh Queue In memery
                    }
                    pr.trackProcess("删除" + fileToDelete.name+"成功");
                   // pr.trackProcess("");
                }
                flagOpen = true;
                
            }
            pr.trackProcess("删除完成");
            pr.deleteFunctionFlag = 1;
           
        }
        private bool refreshDB(fileValue tarFile) 
        {
          
            //update recyclebox
            String sql_string1 = "select id from recyclebox where filename='" + tarFile.name + "'and dir='" + tarFile.dir.Substring(globalTools.getLocalDir().Length, tarFile.dir.Length - globalTools.getLocalDir().Length).Replace("\\", "&") + "' and user='"+globalTools.getUser()+"'";

            DataTable dt = db.readDt(sql_string1);
            if (dt.Rows.Count > 0)
            {
                //有问题
                String sql_string2 = "update recyclebox set lastWT=" + tarFile.lastWriteTime.ToFileTime() + " where id=" + int.Parse(dt.Rows[0][0].ToString());
                if (!db.sqlCommand(sql_string2))
                {
                    MessageBox.Show("更新错误" + sql_string2);
                    return false;
                }
            }
            else
            {
                String sql_string = "insert into recyclebox (filename,lastWT,user,dir)values('"
                 + tarFile.name + "',"
                 + tarFile.lastWriteTime.ToFileTime() + ",'"
                 + globalTools.getUser() + "','"
                 + tarFile.dir.Replace(globalTools.getLocalDir(), "").Replace("\\", "&") +
                  "')";
                if (!db.sqlCommand(sql_string))
                {
                    MessageBox.Show("插入错误" + sql_string);
                    return false;
                }        
            }
            //remove fileindex deleted row
            string sql_del = "delete from filesindex where filename='" + tarFile.name + "' and dir='" + tarFile.dir.Replace(globalTools.getLocalDir(), "").Replace("\\", "&") + "'";
            if (!db.sqlCommand(sql_del))
            {
                MessageBox.Show("删除错误" + sql_del);
                return false;
            }  
            return true;
        }
        private bool refreshQueue(fileValue tarFile)
        {
            //refresh dbFilesQueue,localfilesQueue,deleteFiles.count
            lock(this)
            {
                checkfile.deleteFiles.Remove(tarFile);
                checkfile.DBfiles.Remove(tarFile);
            }
            
            //再想想
            return true;
        }
    }
}
