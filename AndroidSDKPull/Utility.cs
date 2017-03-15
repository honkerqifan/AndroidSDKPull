using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AndroidSDKPull
{
    public class Utility
    {
        public static ConfigInfo InitConfig()
        {
            string json = File.ReadAllText(AppContext.BaseDirectory + "/config.json");
            var config = JsonConvert.DeserializeObject<ConfigInfo>(json);
            if (config != null)
            {
                return config;
            }
            else
            {
                throw new Exception("config.json 配置文件读取出错！");
            }
        }
        public static void WriteLog(LogInfo log, ConfigInfo config)
        {
           
            File.WriteAllText(config.LogPath, JsonConvert.SerializeObject(log));
        }
        public static LogInfo ReadLog(ConfigInfo config)
        {
            if (File.Exists(config.LogPath))
            {
                return JsonConvert.DeserializeObject<LogInfo>(File.ReadAllText(config.LogPath) ?? "");
            }
            return null;
        }
    }
}
