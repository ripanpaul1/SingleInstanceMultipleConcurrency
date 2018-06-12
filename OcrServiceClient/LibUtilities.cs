/*
++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    Description: General Utilities
    Programmer: Ripan Paul
    Start Date: 29th May 2018
    End Date:
++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
*/
using System;
using System.IO;
using System.Net;

namespace OCR_DLL_Invoker
{
    public class LibUtilities
    {
        public static string GetEnvironmentVariablePath(string EnvironmentVariable)
        {
            if (string.IsNullOrWhiteSpace(EnvironmentVariable)) return null;
            return Environment.GetEnvironmentVariable(EnvironmentVariable);
        }
        public static bool IsDirectoryExists(string DirPath)
        {
            try
            {
                if (!Directory.Exists(DirPath)) Directory.CreateDirectory(DirPath);
                return true;
            }
            catch (IOException err)
            {
                WriteFile("IsDirectoryExists - " + err.Message);
                return false;
            }
        }
        public static bool IsFileExists(string FilePath)
        {
            try
            {
                if (!File.Exists(FilePath)) File.Create(FilePath).Dispose();
                return true;
            }
            catch (IOException err)
            {
                WriteFile("IsFileExists - " + err.Message);
                return false;
            }
        }
        public static bool WriteFile(string Content)
        {
            return WriteFile("TheLogFile.txt", Content);
        }
        public static bool WriteFile(string FilePath, string Content)
        {
            try
            {
                if (!IsFileExists(FilePath)) return false;
                using (StreamWriter sw = File.AppendText(FilePath))
                {
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt") + " " + Content);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static string ExecuteAPIRequest(string apiUrl)
        {
            using (var client = new WebClient())
                return client.DownloadString(apiUrl);
        }
    }
}