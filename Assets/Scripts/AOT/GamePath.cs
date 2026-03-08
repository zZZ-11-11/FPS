using System.IO;
using UnityEngine;

namespace AOT
{
    public static class GamePath
    {
        public enum DataType
        {
            Save, // 存档
            Config, // 配置
            Cache, // 缓存
            Settings, // 设置
            Logs, // 日志
            Tmp // 临时
        }

        private static readonly string s_Root = Application.persistentDataPath;

        public static string GetDirPath(DataType type)
        {
            string subDir = type switch
            {
                DataType.Save => "Save",
                DataType.Config => "Config",
                DataType.Cache => "Cache",
                DataType.Settings => "Settings",
                DataType.Logs => "Logs",
                DataType.Tmp => "Tmp",
                _ => ""
            };
            string fullPath = Path.Combine(s_Root, subDir);
            EnsureDirectory(fullPath);
            return fullPath;
        }

        public static string GetFilePath(DataType type, string fileName) => Path.Combine(GetDirPath(type), fileName);

        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}