using System;
using Nancy;
using Nancy.Hosting.Self;
using iBoxDB.LocalServer;

namespace ConsoleApplication
{
    /*
    export DOTNET_REFERENCE_ASSEMBLIES_PATH="/usr/lib/mono/xbuild-frameworks"
    dotnet restore
    dotnet run
    */
    public class Program
    {
        public static void Main(string[] args)
        {
  	        var server = new DB(1, "/tmp/");
            server.GetConfig().EnsureTable<Record>("Record", "ID");
            var autoBox = server.Open();
            HomeModule.auto = autoBox;
       
            var url = "http://localhost:1234";
            HostConfiguration fig = new HostConfiguration();
            fig.UrlReservations.CreateAutomatically = true;
            using (var host = new NancyHost(fig,new Uri(url) ))
            {
               host.Start();
               Console.WriteLine( url + " Started");
               Console.ReadLine();
            }
            
            server.Dispose();
        }
    }

   public class Record
   {
       public long ID;
       public string Name;
    }
    public class HomeModule : Nancy.NancyModule
    {
        public static AutoBox auto;
        
        public HomeModule()
        {                                   

            Get["/" ] =  (x) => { return "Hello World!"; };
            Get["/hello/{name}" ] =  (x) => { return "Hello " + x.name; };
            
            Get["/select/{id:long}" ] =  (x) => { 
                 var r = auto.SelectKey<Record>("Record", (long)x.id);
                 return this.Response.AsJson(r);
            };
                        
            Get["/insert"  ] =  (x) => {       
                var r = new Record{ ID=auto.NewId(0) };
                r.Name= "N-" + r.ID;
                if ( auto.Insert("Record", r) )  return r.ID.ToString();
                return "-1";
            };
        }
    }
}
