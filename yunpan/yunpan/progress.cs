using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Net;
using System.Globalization;
using System.Threading;
namespace yunpan
{
    public partial class progress : Form
    {
        public checkfile checker = new checkfile();
        upLoadToServer uploader = new upLoadToServer();
        downFromServer downer = new downFromServer();
        delectFile deleter = new delectFile();
        FtpWeb fw = new FtpWeb("192.168.1.153", "", "daidai", "1");
        public int checkFunctionFlag=1;
        public int upLoadFunctionFlag = 1;
        public int downLoadFunctionFlag = 1;
        public int deleteFunctionFlag = 1;
        public progress()
        {
            InitializeComponent();

        }
        public  void addloadingFile(fileValue file,String state)
        {
            FileInfo fileInf =new FileInfo(file.pwd);
            ListViewItem lv = new ListViewItem(file.pwd);
            lock (this) 
            {
                lv.SubItems.Add(state + "同步");
            }
           
            String len = "";
      
            if (state == "下载")
            {
                long length=fw.GetFileSize(globalTools.localFileToRemoteFile(file.pwd));
                if (length > 1024 * 1024)
                {
                    len = (int)(length / (1024 * 1024)) + "MB";

                }
                else if (length < 1024)
                {
                    len = ((int)((length))) + "B";
                }
                else len = ((double)((length) / 1024)).ToString("0.0") + "KB";
            }
            else 
            {
                if (fileInf.Length > 1024 * 1024)
                {
                    len = (int)(fileInf.Length / (1024 * 1024)) + "MB";

                }
                else if (fileInf.Length < 1024)
                {
                    len = ((int)((fileInf.Length))) + "B";
                }
                else len = ((double)((fileInf.Length) / 1024)).ToString("0.0") + "KB";
            }
            lv.SubItems.Add(len);
            lv.SubItems.Add("0%"); 
            lv.ImageIndex = 0;
            listView1.Items.Add(lv);
          
         
          
        }
       
        public void addCompletedFile(ListViewItem listitem,String state) 
        {
            lock (this) 
            {
                listView1.Items.Remove(listitem);
            }
            
            ListViewItem lv = new ListViewItem(listitem.SubItems[0].Text);
            
            lv.SubItems.Add(state+"完成");

            lv.SubItems.Add(listitem.SubItems[2].Text);
            lv.SubItems.Add(DateTime.Now.ToString());
            lv.ImageIndex = 0;
            listView2.Items.Add(lv);
        }
        public void addFailedFile(ListViewItem listitem) 
        {

            ListViewItem lv = new ListViewItem(listitem.SubItems[0].Text);
            lv.SubItems.Add("失败");
            String len = "";          
            lv.SubItems.Add("传输失败");
            lv.SubItems.Add("查看");
            lv.ImageIndex = 0;
            listView3.Items.Add(lv);
            listView1.Items.Remove(listitem);
        }
        private void Form1_Load(object sender, EventArgs e)
        {        
            listviewLoader();
            //globalValueLoader();
    
            
            
            startCheckClock();           
        }
        /// <summary>
        /// 跟踪程序进度函数
        /// </summary>
        /// <param name="signs">显示的信号</param>
        public void trackProcess(string signs) 
        {
            label1.Text = signs;
        }
        private void startCheckClock() 
        {
            CheckForIllegalCrossThreadCalls = false;

            System.Timers.Timer checkfieClock = new System.Timers.Timer(2000);
            checkfieClock.Elapsed +=new System.Timers.ElapsedEventHandler(checkDir);
            checkfieClock.AutoReset = true;
            checkfieClock.Enabled = true;

           
        }
        private void globalValueLoader() 
        {
            //需要读文件载入数据
            globalTools.setLocalDir(@"D:\pan");
            globalTools.setRecycleDir(@"\recycle\1");
            globalTools.setRemoteDir("1");
            globalTools.setUser("1");
  
        }
        Thread downLoadTread;
        Thread upLoadThread;
        Thread deleteThread;
        /// <summary>
        /// 扫描用户文件
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void checkDir(object source, System.Timers.ElapsedEventArgs e)
        {
            if (checkFunctionFlag == 1) 
            {
               // loader.getDate(c.checkDir(checkdir,this), this);
               //如何让其并行
                checker.scheduling(this);
            }
            if (downLoadFunctionFlag == 1 && checkfile.downFiles.Count > 0) 
            {
                downLoadTread = new Thread(new ThreadStart(download));
                downLoadTread.Start();
            }
            if (upLoadFunctionFlag == 1 && checkfile.upFiles.Count > 0) 
            {
                upLoadThread = new Thread(new ThreadStart(upload));
                upLoadThread.Start();
            }
            if (deleteFunctionFlag == 1 && checkfile.deleteFiles.Count > 0) 
            {
                deleteThread = new Thread(new ThreadStart(delete));
                deleteThread.Start();
            }
            
        }
        private void download() 
        {
            downer.downLoadFiles(this);
        }
        private void upload() 
        {
            uploader.upLoadfiles(this);
        }
        private void delete() 
        {
            deleter.delectFiles(this);
        }
        private void listviewLoader()
        {
            ColumnHeader h1 = new ColumnHeader();
            h1.Width = 80;
            h1.Text = "状态";
            ColumnHeader h2 = new ColumnHeader();
            h2.Width = 210;
            h2.Text = "文件名";
            ColumnHeader h3 = new ColumnHeader();
            h3.Width =50;
            h3.Text = "大小";
            ColumnHeader h4 = new ColumnHeader();
            h4.Width = 150;
            h4.Text = "进度";
            listView1.Columns.Add(h2);
            listView1.Columns.Add(h1);
            listView1.Columns.Add(h3);
            listView1.Columns.Add(h4);
            listView1.View = View.Details;
            listView1.GridLines = true;
            listView1.FullRowSelect = true;
            ColumnHeader h21 = new ColumnHeader();
            h21.Width = 50;
            h21.Text = "状态";
            ColumnHeader h22 = new ColumnHeader();
            h22.Width = 190;
            h22.Text = "文件名";
            ColumnHeader h23 = new ColumnHeader();
            h23.Width = 80;
            h23.Text = "大小";
            ColumnHeader h24 = new ColumnHeader();
            h24.Width = 130;
            h24.Text = "时间";
            ColumnHeader h25 = new ColumnHeader();
            h25.Width = 50;
            h25.Text = "查看";
            listView2.Columns.Add(h22);
            listView2.Columns.Add(h21);
            
            listView2.Columns.Add(h23);
            listView2.Columns.Add(h24);
            listView2.Columns.Add(h25);
            listView2.View = View.Details;
            listView2.GridLines = true;
            listView2.FullRowSelect = true;

            ColumnHeader h31 = new ColumnHeader();
            h31.Width = 50;
            h31.Text = "状态";
            ColumnHeader h32 = new ColumnHeader();
            h32.Width = 220;
            h32.Text = "文件名";
            ColumnHeader h33 = new ColumnHeader();
            h33.Width = 150;
            h33.Text = "不同步的原因";
      
            ColumnHeader h35 = new ColumnHeader();
            h35.Width = 80;
            h35.Text = "查看";
            listView3.Columns.Add(h32);
            listView3.Columns.Add(h31);    
            listView3.Columns.Add(h33);
            listView3.Columns.Add(h35);
            listView3.View = View.Details;
            listView3.GridLines = true;
            listView3.FullRowSelect = true;

            
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void 设置同步文件夹ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            globalTools.setLocalDir(folderBrowserDialog1.SelectedPath.ToString());
            //MessageBox.Show(folderBrowserDialog1.SelectedPath.ToString());
        }
        private void close() 
        {
            for (int i = 0; i < Application.OpenForms.Count; i++)
            {
                // MessageBox.Show(Application.OpenForms[i].GetType().ToString());
                //if (Application.OpenForms[i].GetType().ToString().Equals("test4.show"))
                    Application.OpenForms[i].Close();
            }
            if (upLoadThread!=null)
                 upLoadThread.Abort();
            if (downLoadTread != null)
                downLoadTread.Abort();
            if (deleteThread != null)
                deleteThread.Abort();
            
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            // 这里写关闭窗体要执行的代码
            close();
            base.OnClosing(e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
           // base.OnClosing(e);
        }

    }
}
