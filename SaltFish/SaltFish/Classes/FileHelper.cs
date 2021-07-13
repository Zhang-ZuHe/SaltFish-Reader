using SaltFish.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SaltFish.Classes
{
    public class FileHelper
    {
        /// <summary>
        /// 版本号
        /// </summary>
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return System.IO.Path.GetDirectoryName(path);
                //return Directory.GetParent(System.IO.Path.GetDirectoryName(path)).FullName;
            }
        }
        /// <summary>
        /// 检测文件是否占用
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool IsFileInUse(string fileName)
        {
            bool inUse = true;

            FileStream fs = null;
            try
            {
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None);
                inUse = false;
            }
            catch
            {
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
            return inUse;//true表示正在使用,false没有使用  
        }
        /// <summary>
        /// 保存当前观看位置
        /// </summary>
        /// <param name="window"></param>
        public static void SaveProgress(string filename)
        {
            double offset = 0;
            foreach (Window window in System.Windows.Application.Current.Windows)
            {
                if (window.GetType().Name == "ReaderView")
                {
                    ReaderView readerview = window as ReaderView;
                    offset = readerview.richbox.VerticalOffset;
                }
            }
            if (AppConfig.History.ContainsKey(filename))
                AppConfig.History[filename] = offset;
            else
                AppConfig.History.Add(filename, offset);
            string configpath = AssemblyDirectory + "/config.json";
            File.WriteAllText(configpath, JSON.stringify(AppConfig.History));
        }
    }
}
