using ArcGIS.Desktop.Framework;
using CCTool.Scripts.Manager;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CCTool.Scripts.ToolManagers
{
    public class BaseTool
    {
        // 从嵌入资源中复制文件
        public static void CopyResourceFile(string resourceName, string filePath)
        {
            // 获取当前程序集的实例
            Assembly assembly = Assembly.GetExecutingAssembly();
            // 从嵌入资源中读取文件
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                // 创建目标文件
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    // 将文件从嵌入资源复制到目标文件
                    stream.CopyTo(fileStream);
                }
            }
        }

        // 从嵌入资源中复制压缩包
        public static void CopyResourceRar(string resourceName, string filePath)
        {
            // 获取当前程序集的实例
            Assembly assembly = Assembly.GetExecutingAssembly();
            // 从嵌入资源中读取文件
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                // 创建目标文件
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    // 将文件从嵌入资源复制到目标文件
                    stream.CopyTo(fileStream);
                }
            }
            // 解压缩
            using (Stream stream2 = File.OpenRead(filePath))
            {
                using (var reader = RarArchive.Open(stream2))
                {
                    foreach (var entry in reader.Entries)
                    {
                        if (!entry.IsDirectory)
                        {
                            string to_path = filePath[..filePath.LastIndexOf(@"\")];    // 解压位置
                            entry.WriteToDirectory(to_path, new ExtractionOptions()
                            {
                                ExtractFullPath = true,
                                Overwrite = true
                            });
                        }
                    }
                }
            }
            // 删除压缩包
            File.Delete(filePath);
        }

        // 复制文件夹下的所有文件到新的位置
        public static void CopyAllFiles(string sourceDir, string destDir)
        {
            //目标目录不存在则创建
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }
            DirectoryInfo sourceDireInfo = new DirectoryInfo(sourceDir);
            List<FileInfo> fileList = new List<FileInfo>();
            GetFileList(sourceDireInfo, fileList); // 获取源文件夹下的所有文件
            List<DirectoryInfo> dirList = new List<DirectoryInfo>();
            GetDirList(sourceDireInfo, dirList); // 获取源文件夹下的所有子文件夹
            // 创建目标文件夹结构
            foreach (DirectoryInfo dir in dirList)
            {
                string sourcePath = dir.FullName;
                string destPath = sourcePath.Replace(sourceDir, destDir); // 替换源文件夹路径为目标文件夹路径
                if (!Directory.Exists(destPath))
                {
                    Directory.CreateDirectory(destPath); // 创建目标文件夹
                }
            }
            // 复制文件到目标文件夹
            foreach (FileInfo fileInfo in fileList)
            {
                string sourceFilePath = fileInfo.FullName;
                string destFilePath = sourceFilePath.Replace(sourceDir, destDir); // 替换源文件夹路径为目标文件夹路径
                File.Copy(sourceFilePath, destFilePath, true); // 复制文件，允许覆盖目标文件
            }
        }

        // 递归获取文件列表
        public static void GetFileList(DirectoryInfo dir, List<FileInfo> fileList)
        {
            fileList.AddRange(dir.GetFiles()); // 添加当前文件夹下的所有文件
            foreach (DirectoryInfo directory in dir.GetDirectories())
            {
                GetFileList(directory, fileList); // 递归获取子文件夹下的文件
            }
        }

        // 递归获取子文件夹列表
        public static void GetDirList(DirectoryInfo dir, List<DirectoryInfo> dirList)
        {
            dirList.AddRange(dir.GetDirectories()); // 添加当前文件夹下的所有子文件夹

            foreach (DirectoryInfo directory in dir.GetDirectories())
            {
                GetDirList(directory, dirList); // 递归获取子文件夹下的子文件夹
            }
        }
    }
}
