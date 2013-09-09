using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySQLDriverCS;
using System.Data.Odbc;
using System.Data;
using System.Windows.Forms;
namespace yunpan
{
    class sql
    {
        MySQLConnection conn = null;

        public sql() 
        {

            conn = new MySQLConnection((new MySQLConnectionString("192.168.1.153", "clouddiskv", "xxx", "xxx").AsString));
            conn.Open();
            MySQLCommand setformat = new MySQLCommand("set names gb2312", conn);
            setformat.ExecuteNonQuery();
            setformat.Dispose();
        }
        /// <summary>
        /// 执行sql语句
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public bool sqlCommand(String sql)
        {
            try
            {
                MySQLCommand commn = new MySQLCommand(sql, conn);
                commn.ExecuteNonQuery();
                return true;
            }
            catch (Exception)
            {
                throw;
                return false;
            }
        }
        public DateTime getTimeNow()
        {
            string sql_now = "select now() as now";
            DataTable dt = readDt(sql_now);
            string now = dt.Rows[0][0].ToString();
            return DateTime.Parse(now);
        }
        public DataTable readDt(String sql) 
        {


            try
            {
                MySQLDataAdapter mda = new MySQLDataAdapter(sql, conn);
                DataSet ds = new DataSet();
                mda.Fill(ds, "table1");
                return ds.Tables["table1"];
            }
            catch (Exception)
            {
               // MessageBox.Show(sql);
                throw;
            }
        }
        ~sql() 
        {
            conn.Close();
        }
    }
}
