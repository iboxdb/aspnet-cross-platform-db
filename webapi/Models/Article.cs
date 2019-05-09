
using System;

namespace WebApi.Models
{
    public class Article
    {
        public long Id;
        public string Text;
        public string Ip;

        private static readonly DateTime Base = new DateTime(2019, 1, 1);
        private static long lastTimeId = 0;
        public static long NextTimeId()
        {
            lock (typeof(GlobalObject))
            {
                int count = -1;
                long nextId = -1;
                while (nextId <= lastTimeId)
                {
                    count++;

                    nextId = DateTime.Now.Ticks - Base.Ticks;
                    nextId *= 100;
                    nextId += count;
                }
                lastTimeId = nextId;
                return nextId;
            }
        }
    }
}