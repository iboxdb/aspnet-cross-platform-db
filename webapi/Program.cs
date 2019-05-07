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

namespace WebApi
{


    public class Program
    {
        public static void Main(string[] args)
        {

            var task = Task.Run<(App, App)>(() =>
                {
                    #region Path 
                    String dir = "iwebapi_data";
                    String path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), dir);
                    Directory.CreateDirectory(path);
                    Console.WriteLine("DBPath=" + path);
                    DB.Root(path);
                    #endregion

                    DB[] dbs = new DB[2] { new DB(1), new DB(2) };

                    App[] apps = new App[dbs.Length];

                    #region Config
                    for (var i = 0; i < dbs.Length; i++)
                    {
                        apps[i] = new App();
                        var databaseId = i + 1;
                        var recy = new LogRecycler(databaseId + 100, databaseId, apps[i]);
                        dbs[i].SetBoxRecycler(recy);

                        var cfg = dbs[i].GetConfig().DBConfig;
                        cfg.CacheLength = cfg.MB(256);

                        cfg.EnsureTable<Confirm>("Confirm", "DatabaseId");
                        cfg.EnsureTable<Article>("Article", "Id", "DatabaseId");

                        apps[i].Auto = dbs[i].Open();
                        apps[i].DatabaseId = databaseId;
                    }
                    #endregion

                    return (apps[0], apps[1]);
                });


            var host = CreateWebHostBuilder(args).Build();

            (App.MasterA, App.MasterB) = task.GetAwaiter().GetResult();

            using (App.MasterA.Auto.GetDatabase())
            {
                using (App.MasterB.Auto.GetDatabase())
                {
                    host.Run();
                }
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
