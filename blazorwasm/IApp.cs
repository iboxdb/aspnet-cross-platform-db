using System;
using iBoxDB.LocalServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using System.IO.Compression;
using System.IO;
using iBoxDB.LocalServer.Replication;
/*
<PackageReference Include="iBoxDB" Version="2.21.0" />
<PackageReference Include="System.IO.Compression" Version="4.3"/>
*/

public interface IApp
{
    AutoBox Auto { get; }
    IBox Cube();

    string Msg { get; }
}

public class AppClient : IApp
{
    string msg;
    IJSInProcessRuntime runtime;
    DB db;
    AutoBox auto;

    public AppClient(IJSRuntime _runtime)
    {
        try
        {
            runtime = (IJSInProcessRuntime)_runtime;

            var bs = runtime.Invoke<byte[]>("localStorage.getItem", typeof(DB).FullName);
            if (bs != null)
            {
                var mm = new MemoryStream();
                var zip = new GZipStream(new MemoryStream(bs), CompressionMode.Decompress);
                zip.CopyTo(mm);
                zip.Dispose();
                bs = mm.ToArray();
            }
            db = new DB(bs ?? new byte[0]);
            var cfg = db.GetConfig();
            cfg.EnsureTable<Record>("Table", "Id");

            db.MinConfig().FileIncSize = 1;
            db.SetBoxRecycler(new TSave(this));
            auto = db.Open();

        }
        catch (Exception ex)
        {
            msg = ex.ToString();
        }
    }

    public AutoBox Auto => auto;

    public IBox Cube()
    {
        return Auto.Cube();
    }

    private int Save()
    {
        var bs = db.GetBuffer();

        var mm = new MemoryStream();
        var zip = new GZipStream(mm, CompressionMode.Compress);
        zip.Write(bs, 0, bs.Length);
        zip.Dispose();
        bs = mm.ToArray();

        runtime.Invoke<byte[]>("localStorage.setItem", typeof(DB).FullName, bs);
        return bs.Length;
    }
    private class TSave : IBoxRecycler3
    {
        private AppClient app;
        public TSave(AppClient _app)
        {
            app = _app;
        }
        public void Dispose()
        {
        }

        public bool Enabled()
        {
            //disable OnReceived();
            return false;
        }

        public void OnReceived(Socket socket, BoxData outBox, bool normal)
        {
        }
        public void OnReceiving(Socket socket)
        {
        }

        private long time = 0;
        public void OnFlushed(Socket socket)
        {
            if (app.auto != null)
            {
                app.msg = $"{app.Save()} ({++time})";
            }
        }
    }
    public string Msg => msg;

}

/*
public class Startup
public void ConfigureServices(IServiceCollection services)
services.AddDatabase();
 */
public static class AppClientExtension
{
    public static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        return services.AddScoped<IApp, AppClient>();
    }
}

public class Record
{
    public long Id;

    public string Name;
    public long Value;
}
