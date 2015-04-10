
using iBoxDB.LocalServer;
using Nancy;
using Nancy.Conventions;
using System.Collections.Generic;
using System.IO;

public class Bootstrapper : DefaultNancyBootstrapper
{
    protected override void ApplicationStartup(Nancy.TinyIoc.TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines)
    {
        base.ApplicationStartup(container, pipelines);

        var dbpath = Path.Combine(RootPathProvider.GetRootPath(), "App_Data");

        if (!Directory.Exists(dbpath))
        {
            Directory.CreateDirectory(dbpath);
        }

        DB.Root(dbpath);
        var server = new DB(1);
        var config = server.GetConfig();
        config.EnsureTable("Test", new Dictionary<string, object> { { "ID", 0L } });
        MyDB.DB = server.Open();

    }

    protected override void ConfigureConventions(Nancy.Conventions.NancyConventions nancyConventions)
    {
        base.ConfigureConventions(nancyConventions);
        nancyConventions.StaticContentsConventions.Add(
             StaticContentConventionBuilder.AddDirectory("Scripts", @"Scripts")
            );
        nancyConventions.StaticContentsConventions.Add(
             StaticContentConventionBuilder.AddDirectory("fonts", @"fonts")
            );
    }
    protected override byte[] FavIcon
    {
        get
        {
            return null;
        }
    }
}
