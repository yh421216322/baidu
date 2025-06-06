using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace MyGameNamespace
{
    public static class FileHelper
    {
        public static void TryCreateDirectory(string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }
        // public static void CreateOrWrite(string filePath, string msg)
        // {
        //     if (msg == null) throw new ArgumentNullException("字符串为空");
        //     using (FileStream fs = File.Create(filePath))
        //     {
        //         byte[] buff = msg.ToCharArray().CharsToBytes();
        //         fs.Write(buff, 0, buff.Length);
        //     }
        // }
        /// <summary>
        /// 写入文件
        /// </summary>
        /// <param name="path">带文件的完整路径</param>
        public static void WriteFile(string path, Action<FileStream> callback)
        {
            if (callback == null) throw new Exception("回调函数为空 该方法无意义");
            var file = new FileInfo(path);
            // 如果当前目录存在该文件 打开并写入文件 否则 创建存档文件
            using (FileStream fs = file.Exists ? file.OpenWrite() : file.Create())
            {
                if (file.Exists)
                {
                    fs.Seek(0, SeekOrigin.Begin);
                    fs.SetLength(0);
                }
                callback.Invoke(fs);
            }
        }

                /// <summary>
        /// 类需要序列化
        /// </summary>
        /// <param name="savedata">需要保存到的类</param>
        /// <param name="FileName">文件名字</param>
        /// <param name="FilePath">文件路径</param>
        public static void JsonSaveData(object savedata, string FileName, string FilePath)
        {
            // 检查文件夹是否存在，不存在则创建
            if (!Directory.Exists(FilePath))
            {
                Directory.CreateDirectory(FilePath);
            }

            // 将对象序列化为 JSON 格式的字符串
            string SaveJson = JsonConvert.SerializeObject(savedata);

            // 调用创建或打开文件的方法，将 JSON 字符串写入文件
            CreateOrOpenFile(FilePath, FileName, SaveJson);
        }

        /// <summary>
        /// 从指定文件路径加载数据并反序列化为指定类型的对象
        /// </summary>
        /// <typeparam name="T">要反序列化的对象类型</typeparam>
        /// <param name="FilePath">文件路径</param>
        /// <returns>反序列化后的对象</returns>
        public static T JsonLoadData<T>(string FilePath) where T : class, new()
        {
            // 检查文件是否存在
            if (!File.Exists(FilePath))
            {
                // 文件不存在，返回空
                return null;
            }

            // 读取文件流
            using (StreamReader streamReader = new StreamReader(FilePath))
            {
                // 读取文件内容
                string str = streamReader.ReadToEnd();

                // 将文件内容反序列化为指定类型的对象
                T json = JsonConvert.DeserializeObject<T>(str);

                // 返回反序列化后的对象
                return json;
            }
        }

        /// <summary>
        /// 创建或打开指定路径下的文件，并将指定信息写入文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="name">文件名</param>
        /// <param name="info">要写入文件的信息</param>
        static void CreateOrOpenFile(string path, string name, string info)
        {
            // 创建文件写入流
            StreamWriter sw;
            FileInfo fi = new FileInfo(Path.Combine(path, name));

            // 创建或打开文件，并写入信息
            sw = fi.CreateText();
            sw.WriteLine(info);

            // 释放资源并关闭文件流
            sw.Dispose();
            sw.Close();
        }
        
        // 调用这个方法来写入日志
        public static void WriteLogMessage(string message,string filePath = "log.txt")
        {
            
            // 添加时间戳
            string logMessage = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + message + "\n";

            // 写入日志到文件
            File.AppendAllText(filePath, logMessage);

            // 也输出到Unity控制台
           //Debug.Log(logMessage);
        }
        
        // ExportToCSV 方法，用于将数据导出到CSV文件中  
        // 参数 data: 需要导出的数据，每行是一个字符串数组  
        // 参数 filePath: 导出文件的路径（包含文件名）  
        public static void ExportToCSV(List<string[]> data, string filePath)  
        {  
            // 使用 StreamWriter 类创建文件写入器，并指定文件路径  
            // StreamWriter 会在 using 语句块结束时自动关闭和释放资源  
            using (StreamWriter writer = new StreamWriter(filePath))  
            {  
                // 遍历每一行数据  
                foreach (string[] row in data)  
                {  
                    // 将每行数据中的字符串用逗号连接成一个字符串，并写入文件  
                    // 每个字符串数组代表一行，数组中的元素用逗号分隔  
                    writer.WriteLine(string.Join(",", row));  
                }  
            }  
          
            // 在控制台输出成功导出的信息  
            //Debug.Log("CSV file exported successfully.");  
        }  
    }
}