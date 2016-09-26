using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace WebApplication1
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.Run(async (context) =>
            {
	        iBoxDB.LocalServer.DB.Root("/tmp/");
	        var text =  iBoxDB.NDB.RunALL();
                await context.Response.WriteAsync(text);
            });
        }
    }
}
