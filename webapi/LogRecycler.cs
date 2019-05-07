using System;
using System.IO;
using iBoxDB.LocalServer;
using iBoxDB.LocalServer.Replication;

namespace WebApi
{
    public class LogRecycler : IBoxRecycler
    {
        public AutoBox Auto;
        public App App;
        public long DatabaseId;
        public LogRecycler(long logDbId, long databaseId, App app)
        {
            App = app;
            DatabaseId = databaseId;
            DB db = new DB(logDbId);
            var cfg = db.GetConfig();
            cfg.DBConfig.CacheLength = cfg.DBConfig.MB(32);
            cfg.EnsureTable<Log>("Log", "Id");
            Auto = db.Open();
        }

        public void Dispose()
        {
            Auto.GetDatabase().Dispose();
        }

        public void OnReceived(Socket socket, BoxData outBox, bool normal)
        {
            if (socket.DestAddress == long.MaxValue) { return; }
            using (var box = Auto.Cube())
            {
                foreach (var oldlog in box.Select<Log>("from Log limit 0,1"))
                {
                    //after poweroff,  re-check 
                    if (oldlog.BoxDataId == socket.ID)
                    {
                        return;
                    }
                }

                Log log = new Log
                {
                    DatabaseId = DatabaseId,
                    Id = box.NewId(),
                    BoxDataId = socket.ID,
                    Data = new MemoryStream(outBox.ToBytes())
                };

                box["Log"].Insert(log);
                box.Commit();
            }
        }
    }
    public class Log : GlobalObject
    {
        public Guid BoxDataId;
        public MemoryStream Data;

    }

    public class Confirm : GlobalObject
    {

    }
}