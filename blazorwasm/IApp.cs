using System;
using iBoxDB.LocalServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

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
