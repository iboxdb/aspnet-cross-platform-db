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

            var task = Task.Run<(AutoBox, Guid, AutoBox, Guid)>(() =>
               {
                   #region Path 
                   String dir = "iwebapi_data";
                   String path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), dir);
                   Directory.CreateDirectory(path);
                   Console.WriteLine("DBPath=" + path);
                   DB.Root(path);
                   #endregion

                   Guid[] dbids = new Guid[2];
                   DB[] dbs = new DB[2] { new DB(1), new DB(2) };
                   #region Config

                   for (var i = 0; i < dbs.Length; i++)
                   {
                       var recy = new LogRecycler(i + 11);

                       dbs[i].SetBoxRecycler(recy);
                       var cfg = dbs[i].GetConfig().DBConfig;
                       cfg.CacheLength = cfg.MB(256);

                       cfg.EnsureTable<Var>("Var", "Id");
                       cfg.EnsureTable<Confirm>("Confirm", "DatabaseId");
                       cfg.EnsureTable<Article>("Article", "Id", "DatabaseId");
                   }





                   #endregion

                   return (dbs[0].Open(), dbids[0], dbs[1].Open(), dbids[1]);
               });


            var host = CreateWebHostBuilder(args).Build();

            (App.MasterA, App.MasterADatabaseId, App.MasterB, App.MasterBDatabaseId) = task.GetAwaiter().GetResult();

            using (App.MasterA.GetDatabase())
            {
                using (App.MasterB.GetDatabase())
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
