using System;
using System.IO;

namespace AndroidSDKPull
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine(">>>>>>>Android SDK Mirrors Pull. By QiFan 2017.03<<<<<<<");
            Console.WriteLine("DateTime:" + DateTime.Now.ToString());
            var config = Utility.InitConfig();
            try
            {
                new AndroidSDKManager(config).Start();
                Console.WriteLine("over "+DateTime.Now.ToString());
            }
            catch (Exception ex)
            {
                File.AppendAllText(AppContext.BaseDirectory + "//androiderror.log", DateTime.Now.ToString() + " " + ex.Message + " " + ex.StackTrace + "\r\n\r\n");
                var log = Utility.ReadLog(config);
                if (log != null)
                {
                    log.Status = "failed";
                }
                else
                {
                    log = new LogInfo { Status = "failed", BeginTime = DateTime.Now, EndTime = DateTime.Now };
                }
                
                Utility.WriteLog(log, config);
                Console.WriteLine("Error:" + ex.Message);

            }
             
        } 
    }
}