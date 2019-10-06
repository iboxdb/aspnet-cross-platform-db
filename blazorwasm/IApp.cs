using System;
using iBoxDB.LocalServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using System.IO.Compression;
using System.IO;
/*
<PackageReference Include="iBoxDB" Version="2.21.0" />
<PackageReference Include="System.IO.Compression" Version="4.3"/>
*/

public interface IApp
{
    AutoBox Auto { get; }
    IBox Cube();

    int Save();
    string Msg { get; }
}

public class AppClient : IApp
{
    string msg;
    IJSInProcessRuntime runtime;


    public AppClient(IJSRuntime _runtime)
    {
        try
        {
            runtime = (IJSInProcessRuntime)_runtime;
            lock (typeof(DB))
            {
                if (DB.Tag == null)
                {
                    var bs = runtime.Invoke<byte[]>("localStorage.getItem", typeof(DB).FullName);
                    if (bs != null)
                    {
                        var mm = new MemoryStream();
                        var zip = new GZipStream(new MemoryStream(bs), CompressionMode.Decompress);
                        zip.CopyTo(mm);
                        zip.Dispose();
                        bs = mm.ToArray();
                    }
                    var db = new DB(bs ?? new byte[0]);
                    var cfg = db.GetConfig();
                    cfg.EnsureTable<Record>("Table", "Id");

                    db.MinConfig().FileIncSize = 1;
                    DB.Tag = new Object[] { db.Open(), db };
                }
            }
        }
        catch (Exception ex)
        {
            msg = ex.ToString();
        }
    }

    public AutoBox Auto => (AutoBox)(((Object[])DB.Tag)[0]);

    public IBox Cube()
    {
        return Auto.Cube();
    }

    public int Save()
    {
        var db = (DB)(((Object[])DB.Tag)[1]);
        var bs = db.GetBuffer();

        var mm = new MemoryStream();
        var zip = new GZipStream(mm, CompressionMode.Compress);
        zip.Write(bs, 0, bs.Length);
        zip.Dispose();
        bs = mm.ToArray();

        runtime.Invoke<byte[]>("localStorage.setItem", typeof(DB).FullName, bs);
        return bs.Length;
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
