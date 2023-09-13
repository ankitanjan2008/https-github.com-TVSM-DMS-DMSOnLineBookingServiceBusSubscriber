using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Online_leads_WalkinTelephone_service_bus
{
    public class CreateLogFiles
    {
        public static string strLogFilePath = string.Empty;
        private static StreamWriter sw = null;

        public static string LogFilePath
        {
            set
            {
                strLogFilePath = value;
            }
            get
            {
                return strLogFilePath;
            }
        }

        public static bool WriteFile(Exception objException)
        {
            string strPathName = string.Empty;

            if (strLogFilePath.Equals(string.Empty))
            {
                //Get Default log file path "LogFile.txt"
                strPathName = GetLogFilePath();
            }
            else
            {
                //If the log file path is not empty but the file
                //is not available it will create it
                if (true != File.Exists(strLogFilePath))
                {
                    if (false == CheckDirectory(strLogFilePath))
                        return false;

                    FileStream fs = new FileStream(strLogFilePath,
                            FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    fs.Close();
                }
                strPathName = strLogFilePath;
            }
            bool bReturn = true;

            // write the error log to that text file
            if (true != WriteErrorLog(strPathName, objException))
            {
                bReturn = false;
            }
            return bReturn;
        }

        public static bool WriteFile(String objException)
        {
            string strPathName = string.Empty;

            if (strLogFilePath.Equals(string.Empty))
            {
                //Get Default log file path "LogFile.txt"
                strPathName = GetLogFilePath();
            }
            else
            {
                //If the log file path is not empty but the file
                //is not available it will create it
                if (true != File.Exists(strLogFilePath))
                {
                    if (false == CheckDirectory(strLogFilePath))
                        return false;

                    FileStream fs = new FileStream(strLogFilePath,
                            FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    fs.Close();
                }
                strPathName = strLogFilePath;
            }
            bool bReturn = true;

            // write the error log to that text file
            if (true != WriteErrorLog(strPathName, objException))
            {
                bReturn = false;
            }
            return bReturn;
        }

        private static bool WriteErrorLog(string strPathName, Exception objException)
        {
            bool bReturn = false;
            string strException = string.Empty;
            try
            {
                sw = new StreamWriter(strPathName, true);
                sw.WriteLine("Date          : " +
                        DateTime.Now.ToString("dd-MM-yyyy"));
                sw.WriteLine("Time          : " +
                        DateTime.Now.ToShortTimeString());
                sw.WriteLine(objException);
                sw.Flush();
                sw.Close();
                bReturn = true;
            }
            catch (Exception)
            {
                bReturn = false;
            }
            return bReturn;
        }

        private static bool WriteErrorLog(string strPathName, String objException)
        {
            bool bReturn = false;
            string strException = string.Empty;
            try
            {
                sw = new StreamWriter(strPathName, true);
                sw.WriteLine("Date        : " +
                        DateTime.Now.ToString("dd-MM-yyyy"));
                sw.WriteLine("Time        : " +
                        DateTime.Now.ToShortTimeString());
                sw.WriteLine(objException);
                sw.Flush();
                sw.Close();
                bReturn = true;
            }
            catch (Exception)
            {
                bReturn = false;
            }
            return bReturn;
        }

        private static string GetLogFilePath()
        {
            try
            {
                // get the base directory
                string baseDir = AppDomain.CurrentDomain.BaseDirectory +
                               AppDomain.CurrentDomain.RelativeSearchPath;

                string logFile = DateTime.Now.ToString("dd-MM-yyyy") + ".txt";
                // search the file below the current directory
                string TargetFile = baseDir + "LogFile\\" + logFile;
                string retFilePath = TargetFile;
                strLogFilePath = TargetFile;
                // if exists, return the path
                if (File.Exists(retFilePath) == true)
                    return retFilePath;
                //create a text file
                else
                {
                    if (false == CheckDirectory(strLogFilePath))
                    {
                        return string.Empty;
                    }
                    else
                    {
                        StreamWriter TextStream = new StreamWriter(retFilePath);
                        TextStream.Close();
                    }
                }

                return retFilePath;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private static bool CheckDirectory(string strLogPath)
        {
            try
            {
                int nFindSlashPos = strLogPath.Trim().LastIndexOf("\\");
                string strDirectoryname =
                           strLogPath.Trim().Substring(0, nFindSlashPos);

                if (false == Directory.Exists(strDirectoryname))
                    Directory.CreateDirectory(strDirectoryname);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
