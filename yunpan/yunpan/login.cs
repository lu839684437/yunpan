using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
namespace yunpan
{
    public partial class login : Form
    {
        public login()
        {
            InitializeComponent();
        }
        string user = "";
        string dir="";
        string localdir = "";
        sql db = new sql();
        private bool loadUserData(string user,string pwd)
        {
           
            string sqlString = "select * from user where user='"+user+"' and pwd='"+pwd+"'";
            DataTable dt = db.readDt(sqlString);
            if (dt.Rows.Count == 1)
            {
                dir=dt.Rows[0][2].ToString().Trim();
                return true;
            }
            else 
            {
                MessageBox.Show("密码输入错误");
                return false;
            }
            
        }
        private void button1_Click(object sender, EventArgs e)
        {
           
            user = textBox1.Text.Trim();
            string pwd = textBox2.Text.Trim();
            if (loadUserData(user,pwd)) 
            {
                string localdir1 = loadLocalDir();
                if (localdir1 == "") 
                {
                    MessageBox.Show("路径未设置");
                    return;
                }
                globalTools.setLocalDir(@localdir1);
                globalTools.setRecycleDir(@"\recycle\"+dir);
                globalTools.setRemoteDir(dir);
                globalTools.setUser(user);
                globalTools.setMaxThreadNum(5);
                progress pr = new progress();
                pr.Show();
                this.Hide();
            }
        }
        private string loadLocalDir()
        {
            StreamReader objReader = new StreamReader("localdir.txt");
            string sLine = "";

            while (sLine != null)
            {
                sLine = objReader.ReadLine();
                return sLine;
              
            }
            return "";
            objReader.Close();
        }
        [DllImport("Kernel32.dll")]
        public static extern void SetLocalTime(SystemTime st);

        [StructLayout(LayoutKind.Sequential)]
        public class SystemTime
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMilliseconds;
        }
        private void login_Load(object sender, EventArgs e)
        {
            DateTime remoteTime=db.getTimeNow();
            try
            {
                if (remoteTime != DateTime.Now)
                {
                    SystemTime st = new SystemTime();
                    st.wYear = (ushort)remoteTime.Year;
                    st.wMonth = (ushort)remoteTime.Month;
                    st.wDay = (ushort)remoteTime.Day;
                    st.wHour = (ushort)remoteTime.Hour;
                    st.wMinute = (ushort)remoteTime.Minute;
                    st.wSecond = (ushort)remoteTime.Second;
                    SetLocalTime(st);
                }
            }
            catch (Exception)
            {

                //MessageBox.Show("设置失败");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            label3.Visible = true;
            button3.Visible = true;
            if (textBox1.Text != "" && textBox2.Text != "" && localdir != "")
            {

                string sqlString = "select userid from pd_users where username='" + textBox1.Text.Trim() + "' and password='" + textBox2.Text.Trim() + "'";
                DataTable dt = db.readDt(sqlString);
                if (dt.Rows.Count > 0)
                {
                    MessageBox.Show("此账号已经被注册了");
                    return;
                }
                string sqlInsert = "insert into user (user,pwd,dir)values('" + textBox1.Text.Trim() + "','" + textBox2.Text.Trim() + "','" + textBox1.Text.Trim() + "')";
                db.sqlCommand(sqlInsert);
                //注册
                globalTools.setLocalDir(localdir);
                FileStream fs = new FileStream("localdir.txt", FileMode.OpenOrCreate);
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(localdir);
                //清空缓冲区  
                sw.Flush();
                //关闭流  
                sw.Close();
                fs.Close();
                MessageBox.Show("注册成功");


            }
            else 
            {
                MessageBox.Show("注册条件达不到");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            localdir = folderBrowserDialog1.SelectedPath.ToString();
           // globalTools.setLocalDir(folderBrowserDialog1.SelectedPath.ToString());
        }

        private void 设置本地目录ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            string dirTmp = folderBrowserDialog1.SelectedPath.ToString();
            FileStream fs = new FileStream("localdir.txt", FileMode.OpenOrCreate);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(dirTmp);
            //清空缓冲区  
            sw.Flush();
            //关闭流  
            sw.Close();
            fs.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            MessageBox.Show(DateTime.Now.ToFileTime()+"  "+DateTime.Now.AddMilliseconds(1).ToFileTime());
           // string str = "insert into pd_folders (folder_id,folder_name,folder_description)values(-1,'','')";
            //db.sqlCommand(str);
            //string str_insert = "select pd_files.file_name,pd_users.username,pd_folders.folder_pwd from pd_files,pd_users,pd_folders where pd_files.userid=pd_users.userid and pd_files.folder_id=pd_folders.folder_id";
           // DataTable dt = db.readDt(str_insert);

            /*string sql_string = "select folder_id,folder_name,parent_id from pd_files";
            sql db = new sql();
            DataTable dt = db.readDt(sql_string);
            MessageBox.Show(""+dt.Rows.Count);
            foreach(DataRow dr in dt.Rows)
            {
                string pwd = "&"+dr[1].ToString().Trim();
                int p_id=int.Parse( dr[2].ToString());
                while (p_id != -1) 
                {
                    DataRow dr1=seach(p_id);
                    pwd += "&" + dr1[1].ToString().Trim();
                    p_id = int.Parse(dr1[2].ToString());

                }

                string str_insert = "update pd_folders set folder_pwd='" + pwd + "' where folder_id=" + int.Parse(dr[0].ToString());
                db.sqlCommand(str_insert);
            }*/
            //MessageBox.Show(dt.Rows.Count+"");
        }
        private DataRow seach(int id) 
        {
            string s = "select folder_id,folder_name,parent_id from pd_folders where folder_id=" + id;
            DataTable dt = db.readDt(s);
            return dt.Rows[0];
            
        }
    }
}
