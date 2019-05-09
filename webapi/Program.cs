using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using iBoxDB.LocalServer;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebApi.Models;

namespace WebApi
{


    public class Program
    {
        public static void Main(string[] args)
        {

            var task = Task.Run<AutoBox>(() =>
                {
                    #region Path 
                    String dir = "iwebapi_data";
                    String path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), dir);
                    Directory.CreateDirectory(path);
                    Console.WriteLine("DBPath=" + path);
                    DB.Root(path);
                    #endregion

                    DB db = new DB(1);
                    var cfg = db.GetConfig();
                    cfg.EnsureTable<Article>("Article", "Id");
                    return db.Open();
                });


            var host = CreateWebHostBuilder(args).Build();

            (App.Auto) = task.GetAwaiter().GetResult();

            using (App.Auto.GetDatabase())
            {
                host.Run();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
