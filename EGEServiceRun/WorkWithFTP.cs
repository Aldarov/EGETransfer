using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace EGEServiceRun
{
    public static class WorkWithFTP
    {
        private static FtpWebRequest GetFtpWebRequest(string relativePath)
        {
            string full_path = string.Format("ftp://{0}", ConfigurationManager.AppSettings["ftp_address"]) + "/" + relativePath;
            FtpWebRequest reqFTP = (FtpWebRequest)WebRequest.Create(full_path);
            reqFTP.UseBinary = true;
            reqFTP.Proxy = null;
            reqFTP.KeepAlive = false;
            reqFTP.UsePassive = true;
            reqFTP.EnableSsl = false;
            reqFTP.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["ftp_login"],
                ConfigurationManager.AppSettings["ftp_password"]);
            return reqFTP;
        }

        //public static bool FtpDirectoryExists(string directoryPath)
        //{
        //    bool IsExists = false;
        //    FtpWebResponse response = null;
        //    try
        //    {
        //        FtpWebRequest reqFTP = GetFtpWebRequest(directoryPath);
        //        reqFTP.Method = WebRequestMethods.Ftp.PrintWorkingDirectory;
        //        response = (FtpWebResponse)reqFTP.GetResponse();
        //        IsExists = true;
        //        response.Close();
        //    }
        //    catch (Exception e)
        //    {
        //        if (response != null)
        //            response.Close();
        //    }
        //    return IsExists;
        //}

        public static void FTPMakeDir(string pathToCreate)
        {
            string[] subDirs = pathToCreate.Split('/');
            string cur_dir = "";
            foreach (string subDir in subDirs)
            {
                cur_dir = cur_dir + "/" + subDir;
                //if (FtpDirectoryExists(cur_dir))
                //    continue;

                FtpWebResponse response = null;
                Stream ftpStream = null;
                try
                {
                    FtpWebRequest reqFTP = GetFtpWebRequest(cur_dir);
                    reqFTP.Method = WebRequestMethods.Ftp.MakeDirectory;
                    response = (FtpWebResponse)reqFTP.GetResponse();
                    ftpStream = response.GetResponseStream();
                    ftpStream.Close();
                    response.Close();
                }
                catch (Exception)
                {
                    if (ftpStream != null)
                        ftpStream.Close();
                    if (response != null)
                        response.Close();
                    //throw;
                }
            }
        }

        public static List<FTPFile> GetListFilesFromDir(string dir)
        {
            List<FTPFile> files = new List<FTPFile>();
            FtpWebResponse response = null;
            StreamReader reader = null;
            try
            {
                FtpWebRequest reqFTP = GetFtpWebRequest(dir);
                reqFTP.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                response = (FtpWebResponse)reqFTP.GetResponse();
                reader = new StreamReader(response.GetResponseStream());

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] file_info = line.Substring(19,line.Length-19).Trim().Split(' ');
                    int file_size;
                    int.TryParse(file_info[0],out file_size);
                    files.Add(new FTPFile() 
                    { 
                        file_folder = dir,
                        size =  file_size,
                        file_name = file_info[1]
                    });
                }

                reader.Close();
                response.Close();
                return files.OrderBy(x => x.file_name).ToList();
            }
            catch (Exception)
            {
                if (reader != null)
                    reader.Close();
                if (response != null)
                    response.Close();
                throw;
            }
        }

    }
}