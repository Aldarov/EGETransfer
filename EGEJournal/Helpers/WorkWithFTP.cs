using Crypto;
using EGEJournal.Infrustructure;
using EGEJournal.ServiceEGE;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace EGEJournal.Helpers
{
    //public class FTPFile
    //{
    //    public string file_folder { get; set; }
    //    public string file_name { get; set; }
    //    public int size { get; set; }
    //}

    public class WorkWithFTP
    {
        private readonly byte[] _ftp_access;

        public WorkWithFTP(byte[] ftp_access)
        {
            _ftp_access = ftp_access;
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
        }

        private FtpWebRequest GetFtpWebRequest(string relativePath)
        {
            string[] ftp_access = AES.DecryptStringFromBytes(_ftp_access, CKeys.key1, CKeys.key2).Split('#');

            string full_path = string.Format("ftp://{0}", ftp_access[0]) + "/" + relativePath;
            FtpWebRequest reqFTP = (FtpWebRequest)WebRequest.Create(full_path);
            reqFTP.UseBinary = true;
            reqFTP.Proxy = null;
            reqFTP.KeepAlive = false;
            reqFTP.UsePassive = true;
            reqFTP.EnableSsl = false;
            reqFTP.Timeout = -1;
            reqFTP.Credentials = new NetworkCredential(ftp_access[1], ftp_access[2]);
            return reqFTP;
        }

        //public bool FtpDirectoryExists(string directoryPath)
        //{
        //    bool IsExists = false;
        //    FtpWebResponse response = null;
        //    try
        //    {
        //        FtpWebRequest reqFTP = GetFtpWebRequest(directoryPath);
        //        reqFTP.Method = WebRequestMethods.Ftp.ListDirectory;
        //        response = (FtpWebResponse)reqFTP.GetResponse();
        //        IsExists = true;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    finally
        //    {
        //        if (response != null)
        //            response.Close();
        //    }
        //    return IsExists;
        //}

        public void FTPMakeDir(string pathToCreate)
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
                }
                catch (Exception)
                {
                }
                finally
                {
                    if (ftpStream != null)
                        ftpStream.Close();
                    if (response != null)
                        response.Close();
                }
            }
        }

        public List<FTPFile> GetListFilesFromDir(string dir)
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
                    string[] file_info = line.Substring(19, line.Length - 19).Trim().Split(' ');
                    int file_size;
                    int.TryParse(file_info[0], out file_size);
                    files.Add(new FTPFile()
                    {
                        file_folder = dir,
                        size = file_size,
                        file_name = file_info[1]
                    });
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                if (response != null)
                    response.Close();
            }
            return files.OrderBy(x => x.file_name).ToList();
        }

        public long FtpFileSize(string dir)
        {
            long size = 0;
            FtpWebResponse respSize = null;
            try
            {
                FtpWebRequest reqFTP = GetFtpWebRequest(dir);
                reqFTP.Method = WebRequestMethods.Ftp.GetFileSize;
                respSize = (FtpWebResponse)reqFTP.GetResponse();
                size = respSize.ContentLength;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (respSize != null)
                    respSize.Close();
            }
            return size;
        }

        public Boolean FTPDownloadFile(string targetFilePath, FTPFile file, Action<int> ProgressChange)
        {
            bool result = false;
            FileStream outputStream = new FileStream(targetFilePath + "\\" + file.file_name, FileMode.Create);
            Stream ftpStream = null;
            FtpWebResponse response = null;
            try
            {
                long filesize = FtpFileSize(file.file_folder + "/" + file.file_name);

                FtpWebRequest reqFTP = GetFtpWebRequest(file.file_folder + "/" + file.file_name);
                reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                response = (FtpWebResponse)reqFTP.GetResponse();
                ftpStream = response.GetResponseStream();
                int bufferSize = 16384;
                int readCount;
                byte[] buffer = new byte[bufferSize];

                long countWriteBites = 0;
                readCount = ftpStream.Read(buffer, 0, bufferSize);
                while (readCount > 0)
                {
                    outputStream.Write(buffer, 0, readCount);
                    countWriteBites = countWriteBites + readCount;
                    ProgressChange((int)(countWriteBites * 100 / filesize));
                    readCount = ftpStream.Read(buffer, 0, bufferSize);
                }
                result = true;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (ftpStream != null)
                    ftpStream.Close();
                if (outputStream != null)
                    outputStream.Close();
                if (response != null)
                    response.Close();
            }
            return result;
        }

        public Boolean FTPUploadFile(string FileName, string destinationPath, Action<int> ProgressChange)
        {
            bool result = false;
            FileInfo fileInf = new FileInfo(FileName);

            FtpWebRequest reqFTP = GetFtpWebRequest(destinationPath + "/" + fileInf.Name);
            reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
            reqFTP.ContentLength = fileInf.Length;

            FileStream fs = null;
            Stream stream = null;
            try
            {
                fs = fileInf.OpenRead();
                stream = reqFTP.GetRequestStream();

                int buffLength = 16384;
                byte[] buff = new byte[buffLength];

                long countWriteBites = 0;
                int contentLen = fs.Read(buff, 0, buffLength);
                while (contentLen > 0)
                {
                    stream.Write(buff, 0, contentLen);
                    countWriteBites = countWriteBites + contentLen;
                    ProgressChange((int)(countWriteBites * 100 / fileInf.Length));
                    contentLen = fs.Read(buff, 0, buffLength);
                }
                result = true;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
                if (fs != null)
                    fs.Close();
            }
            return result;
        }

        public Boolean DeleteFile(string fileName)
        {
            bool result = false;
            FtpWebResponse response = null;
            Stream ftpStream = null;
            try
            {
                FtpWebRequest reqFTP = GetFtpWebRequest(fileName);
                reqFTP.Method = WebRequestMethods.Ftp.DeleteFile;
                response = (FtpWebResponse)reqFTP.GetResponse();
                ftpStream = response.GetResponseStream();
                result = true;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (ftpStream != null)
                    ftpStream.Close();
                if (response != null)
                    response.Close();
            }
            return result;
        }

    }
}