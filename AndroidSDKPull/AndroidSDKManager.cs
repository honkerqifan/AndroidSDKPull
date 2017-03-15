using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace AndroidSDKPull
{
    public class AndroidSDKManager
    {
        ConfigInfo config;
        public AndroidSDKManager(ConfigInfo info)
        {
            config = info;
        }
        public void Start()
        {
            var log = Utility.ReadLog(config);
            if (log != null)
            {
                log.Status = "running";
                log.BeginTime = DateTime.Now;
            }
            else
            {
                log = new LogInfo { Status = "running", BeginTime = DateTime.Now, EndTime = DateTime.Now };
            }
            Utility.WriteLog(log, config);
            foreach (var item in config.XmlList)
            {
                Fetch(item);
            }
            var log2 = new LogInfo { Status = "success", BeginTime = log.BeginTime, EndTime = DateTime.Now };
            Utility.WriteLog(log2, config);
        }
        private void Fetch(string file)
        {
            string dir = "/";
            if (file.StartsWith("http"))
            {
                return;
            }
            Process(dir + file);//"/glass/addon.xml"

            //string base_dir = new DirectoryInfo("/" + file).FullName;
            //if (base_dir != "/") base_dir += '/';
            //判断多级路径的时候
            string base_dir = "/";
            if (file.Contains("/"))
            {
                base_dir = file.Substring(0, file.LastIndexOf("/"));
            }
            
            //xml读取要namespace，换成正则匹配
            string xmlstr = File.ReadAllText(config.OutDir + dir + file);
            string pat = "<sdk:url>.*</sdk:url>";
            var col = Regex.Matches(xmlstr, pat);
            string fname = "";
            foreach (Match item in col)
            {
                fname = item.Value.Replace("<sdk:url>", "").Replace("</sdk:url>", "");
                if (!fname.EndsWith(".xml"))
                {
                    if (!fname.StartsWith("http"))
                    {
                        Process(base_dir+"/"+fname);
                    }
                }
                else
                {
                    Fetch(fname);
                }
            }
        }
        private void Process(string filename, double size = -1)
        {
            try
            {
                string file = config.OutDir + filename;//"/data1/android/repository/glass/addon.xml"

                if (File.Exists(file) && new FileInfo(file).Length <= size)
                {
                    Console.WriteLine("Skipping: " + filename + " " + DateTime.Now.ToString()); return;
                }
                Console.WriteLine("Processing: " + filename + " " + DateTime.Now.ToString());
                var headers = GetHTTPResponseHeaders(config.BaseUrl+"/" + filename);
                double content_length = Convert.ToDouble(headers["Content-Length"]);
                DateTime last_modified = Convert.ToDateTime(headers["Last-Modified"]);
                string dir = config.OutDir;
                if (filename.Contains("/"))
                {
                    dir += filename.Substring(0, filename.LastIndexOf("/"));
                }
                if (!Directory.Exists(dir))
                {
                    Console.WriteLine("Creating " + dir + " " + DateTime.Now.ToString());
                    Directory.CreateDirectory(dir);
                }
                if (!File.Exists(file))//不存在就下载
                {
                    Download(filename, last_modified);
                }
                else//存在则比对时间和大小
                {
                    FileInfo finfo = new FileInfo(file);
                    if (finfo.LastWriteTime != last_modified || finfo.Length != content_length)
                    {
                        Download(filename, last_modified);
                    }
                    else
                    {
                        Console.WriteLine("Skipping: " + filename + "  [NOT MODIFIED]" + " " + DateTime.Now.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(AppContext.BaseDirectory + "//androiderror.log", 
                    DateTime.Now.ToString() + " " +config.BaseUrl+ filename +" "+ ex.Message + " " + ex.StackTrace + "\r\n\r\n");
            }
            
        }
        public void Download(string filename, DateTime last_modified)
        {

            string file = config.OutDir + filename;
            Console.WriteLine("Downloading " + filename + " " + DateTime.Now.ToString());

            WebRequest req = WebRequest.Create(config.BaseUrl+"/" + filename);
            WebResponse resp = req.GetResponseAsync().Result;
            using (Stream responseStream = resp.GetResponseStream())
            {
                //创建本地文件写入流
                using (Stream stream = new FileStream(file, FileMode.Create))
                {
                    byte[] bArr = new byte[1024];
                    int size = responseStream.Read(bArr, 0, (int)bArr.Length);
                    while (size > 0)
                    {
                        stream.Write(bArr, 0, size);
                        size = responseStream.Read(bArr, 0, (int)bArr.Length);
                    }
                }
            }
            FileInfo finfo = new FileInfo(file);
            finfo.LastWriteTime = last_modified;
            Console.WriteLine("Download Complete: " + filename + " " + DateTime.Now.ToString());
            Process(filename);
        }
        private Dictionary<string, string> GetHTTPResponseHeaders(string Url)
        {

            Dictionary<string, string> headerList = new Dictionary<string, string>();

            WebRequest req = WebRequest.Create(Url);
            WebResponse resp = req.GetResponseAsync().Result;
            foreach (string key in resp.Headers)
            {
                headerList.Add(key, resp.Headers[key]);
            }

            return headerList;

        }

    }

}
