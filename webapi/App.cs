using System;
using iBoxDB.LocalServer;

namespace WebApi
{
    public class App
    {
        public static AutoBox MasterA;
        public static Guid MasterADatabaseId;

        public static AutoBox MasterB;
        public static Guid MasterBDatabaseId;
    }

    public class GlobalObject
    {
        public Guid DatabaseId;
        public long Id;

        public DateTime Time = DateTime.Now;
    }

    public class Var
    {
        public long Id;
        public String Value;
    }

}