
using System;
using System.IO;

namespace IISHostLeadTools
{
    public class LibUtilities
    {
        public static string TheEnvironmentVariablePath { get; set; }
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
                return false;
            }
        }
        public static bool IsAccess(string DirPath)
        {
            try
            {
                if (Directory.GetAccessControl(DirPath).AreAccessRulesProtected) return false;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public struct TheEnvVarSubDir
    {
        public static string License = "License";
        public static string OCRArchived = "OCRArchived";
        public static string OCRInput = "OCRInput";
        public static string OCRMasterFormSets = "OCRMasterFormSets";
        public static string OCRNotRecognized = "OCRNotRecognized";
        public static string OCROutput = "OCROutput";
    }
}