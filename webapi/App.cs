using System;
using iBoxDB.LocalServer;

namespace WebApi
{
    public class App
    {
        public static App MasterA;
        public static App MasterB;

        public AutoBox Auto;
        public long DatabaseId;

    }

    public class GlobalObject
    {
        public long Id;
        public long DatabaseId;

        public DateTime Time = DateTime.Now;


        private static readonly DateTime Base = new DateTime(2018, 1, 1);
        private static long lastTimeId = 0;
        public static long NextTimeId()
        {
            lock (typeof(GlobalObject))
            {
                int count = 0;
                long nextId = 0;
                while (nextId <= lastTimeId)
                {
                    nextId = DateTime.Now.Ticks - Base.Ticks;
                    nextId *= 100;
                    nextId += count;
                    count++;
                }
                lastTimeId = nextId;
                return nextId;
            }
        }
    }



}