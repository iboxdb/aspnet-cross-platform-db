using System;
using iBoxDB.LocalServer;

namespace WebApi
{
    public class App
    {
        public static AutoBox Auto;
        public static Box Cube() => Auto.Cube();
    }

    public class GlobalObject
    {
        public long Id;
        public long DatabaseId;

        public DateTime Time = DateTime.Now;


    }



}