
using Nancy;
using Nancy.ModelBinding;
using Nancy.Hosting.Aspnet;
using System;
using System.Collections.Generic;
using System.Collections;

public class IndexModule : NancyModule
{
    public IndexModule()
    {
        Get["/"] = parameters =>
        {
            if (MyDB.DB.SelectCount("from Test") == 0)
            {
                MyDB.DB.Insert("Test",
                 new Dictionary<string, object>
             {
                { "ID" ,  MyDB.DB.NewId(0)} ,
                {"Name" ,  "ASP.NET DB" },
                {"Value" ,  "SUPPORTED" }
             },

                new Dictionary<string, object>
            {
                { "ID" ,  MyDB.DB.NewId(0)} ,
                {"Name" ,  "Platform" },
                {"Value" ,  Environment.OSVersion.Platform.ToString() }
            },

                new Dictionary<string, object>
            {
                { "ID" ,  MyDB.DB.NewId(0)} ,
                {"Name" ,  "Version" },
                {"Value" ,  Environment.OSVersion.VersionString }
            }
               );
            }
            else
            {                
                foreach (DictionaryEntry e in Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process))
                {
                    MyDB.DB.Insert("Test",
                 new Dictionary<string, object>
             {
                { "ID" ,  MyDB.DB.NewId(0)} ,
                {"Name" ,  e.Key.ToString() },
                {"Value" ,  e.Value.ToString() }
             });
                }
                MyDB.DB.Insert("Test",
                 new Dictionary<string, object>
             {
                { "ID" ,  MyDB.DB.NewId(0)} ,
                {"Name" ,  "AccessTime" },
                {"Value" ,  DateTime.Now }
             });
                
            }
            return View["index", MyDB.DB.Select("from Test limit 0,100")];
        };

        Get["/test"] = parameters =>
       {
           return Response.AsJson(MyDB.DB.Select("from Test"));
       };
    }
}
