using System;

using IBoxDB.LocalServer;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using System.IO.Compression;
using System.IO;

/*
<PackageReference Include="iBoxDB" Version="3.0" />
*/

/* 
public void ConfigureServices(IServiceCollection services)
   services.AddDatabase();
*/
public static class AppClientExtension
{
    public static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        return services.AddSingleton<IApp, AppClient>();
    }
}

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
            db.SetBoxRecycler((socket, outBox, normal) =>
            {
                time++;
                msg = $"{Save()},  Time:({time})";
            });
            auto = db.Open();

        }
        catch (Exception ex)
        {
            msg = ex.ToString();
        }
    }

    public AutoBox Auto => auto;
    public string Msg => msg;

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

    private long time = 0;

}



public class Record
{
    public long Id;

    public string Name;
    public long Value;
}
