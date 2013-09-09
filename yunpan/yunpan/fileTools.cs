using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace yunpan
{
    class fileTools
    {
        public static void newFile(string path, DateTime lastWriteTime) 
        {
            FileInfo file = new FileInfo(path);
            if (!Directory.Exists(file.DirectoryName)) 
            {
                Directory.CreateDirectory(file.DirectoryName);    
            }
             FileStream tt = new FileStream(path, FileMode.Create);
             tt.Close();
                
            file.LastWriteTime = lastWriteTime;
            
        }
    }
}
