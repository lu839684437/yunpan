using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace yunpan
{
    public class fileValue
    {
        public string dir;
        public string name;
        public string pwd;
        public int flag;
        public DateTime lastWriteTime;
        public fileValue(string path,DateTime lastwriteTime) 
        {
            FileInfo file = new FileInfo(path);
            dir = file.DirectoryName;
            name = file.Name;
            pwd = file.ToString();
            lastWriteTime = lastwriteTime;
            flag = 0;
        }
        public fileValue(FileInfo file)
        {
            dir = file.DirectoryName;
            name = file.Name;
            pwd = file.ToString();
            lastWriteTime = file.LastWriteTime;
            flag = 0;
        }
        public static bool operator ==(fileValue t1, fileValue t2)
        {
            if (t1.dir == t2.dir && t1.lastWriteTime == t2.lastWriteTime && t1.name == t2.name && t1.pwd == t2.pwd)
                return true;
            return false;
        }
        public static bool operator !=(fileValue t1, fileValue t2)
        {
            if (t1.dir == t2.dir && t1.lastWriteTime == t2.lastWriteTime && t1.name == t2.name && t1.pwd == t2.pwd)
                return false;
            return true;
        }
        override public   bool   Equals(object a)
        {
            if (this.dir == ((fileValue)a).dir && this.lastWriteTime == ((fileValue)a).lastWriteTime && this.name == ((fileValue)a).name && this.pwd == ((fileValue)a).pwd)
                return true;
            return false;
        }
    }
}
