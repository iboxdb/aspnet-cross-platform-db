using System;
using System.IO;
using System.IO.Compression;
using Microsoft.JSInterop;
using Microsoft.Extensions.DependencyInjection;
using IBoxDB.LocalServer;

/*
dotnet new blazorwasm
<PackageReference Include="iBoxDB" Version="3.5.0" /> 
 
builder.Services.AddDatabase();
public void ConfigureServices(IServiceCollection services)
   services.AddDatabase();
*/

/*
<button class="btn btn-primary" @onclick="IncrementCount">Click me</button>
<p role="status">Current count: @currentCount</p>
<p role="status">Database size: @App.Msg</p>

@code
{
    [Inject]
    IApp App { get; set; } = default!;

    private int currentCount = 0;

    private void IncrementCount()
    {
        using var box = App.Auto.Cube();
        var record = box["Table", 0L].Replace<Record>();
        record.Value++;
        box.Commit();
        currentCount = record.Value;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        IncrementCount();
    }
}
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
    string msg = String.Empty;
    IJSInProcessRuntime runtime;
    DB db;
    AutoBox auto;

    public AppClient(IJSRuntime _runtime)
    {
        runtime = (IJSInProcessRuntime)_runtime;

        var s = runtime.Invoke<string>("localStorage.getItem", typeof(DB).FullName);
        var bs = s != null ? Convert.FromBase64String(s) : null;
        if (bs != null)
        {
            using (var mm = new MemoryStream())
            {
                using (var zip = new GZipStream(new MemoryStream(bs), CompressionMode.Decompress))
                {
                    zip.CopyTo(mm);
                }
                bs = mm.ToArray();
            }
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

    public AutoBox Auto => auto;
    public string Msg => msg;

    public IBox Cube()
    {
        return Auto.Cube();
    }

    private int Save()
    {
        var bs = db.GetBuffer();
        using (var mm = new MemoryStream())
        {
            using (var zip = new GZipStream(mm, CompressionMode.Compress))
            {
                zip.Write(bs, 0, bs.Length);
                zip.Flush();
            }
            bs = mm.ToArray();
        }
        runtime.Invoke<string>("localStorage.setItem", typeof(DB).FullName, Convert.ToBase64String(bs));
        return bs.Length;
    }

    private long time = 0;

}



public class Record
{
    public long Id;

    public string Name = String.Empty;
    public int Value = 0;
}
