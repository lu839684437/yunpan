using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace yunpan
{
    class globalTools
    {
        //global
        private static string localDir;
        public static void setLocalDir(string dir)
        {
            localDir = dir;
        }
        public static string getLocalDir()
        {
            return localDir;
        }
        private static string remoteDir;
        public static void setRemoteDir(string dir)
        {
            remoteDir = dir;
        }
        public static string getRemoteDir()
        {
            return remoteDir;
        }
        private static string recycleDir;
        public static void setRecycleDir(string dir)
        {
            recycleDir = dir;
        }
        public static string getRecycleDir()
        {
            return recycleDir;
        }
        private static string User;
        public static void setUser(string user)
        {
            User = user;
        }
        public static string getUser()
        {
            return User;
        }
        public static string localFileToRemoteFile(string tarFile) 
        {
            return remoteDir+ tarFile.Substring(localDir.Length, tarFile.Length-localDir.Length);
        }
        public static string remoteFileToLocalFile(string filename,string dir) 
        {
            string tmpKey = dir.ToString().Trim().Replace("&", "\\") + "\\" + filename.ToString();
            tmpKey = localDir + tmpKey;
            return tmpKey;
        }
        private static int maxThreadNum=5;
        public static void setMaxThreadNum(int Threadnum)
        {
            maxThreadNum = Threadnum;
        }
        public static int getMaxThreadNum()
        {
            return maxThreadNum;
        }
        public static int runningThreadNum = 0;

    }
}
