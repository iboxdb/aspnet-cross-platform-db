//#define WINDOWS_PHONE
//#define NET2

using System;
using System.Text;
using System.Threading;
using System.Collections.Generic;

using iBoxDB.LocalServer;
using iBoxDB.LocalServer.IO;
using iBoxDB.LocalServer.Replication;
using iBoxDB.DBDebug;


/*
Copy to  "static void Main(string[] args)"
  iBoxDB.LocalServer.DB.Root("/tmp/");
  var text =  iBoxDB.NDB.RunALL();
  Console.WriteLine(text);
*/
namespace iBoxDB
{
    public class NDB
    {
        public static String RunALL(bool speedTest = false)
        {
            try
            {
                //DB.Root("/tmp/");
                DBPlatform.SetStorage();
                strout = new StringBuilder();

                WriteLine("");
                GettingStarted();

                WriteLine("");
                IsolatedSpace();

                WriteLine("");
                BeyondSQL();

                WriteLine("");
                MasterSlave();

                WriteLine("");
                MasterMaster();

                if (speedTest)
                {
                    GC.Collect();
                    WriteLine("");
                    SpeedTest(DBPlatform.Speed_ThreadCount);

                    GC.Collect();
                    WriteLine("");
                    ReplicationSpeed(DBPlatform.Replication_ThreadCount, DBPlatform.Replication_Time);
                }

                DBPlatform.DeleteDB();
                return WriteLine("").ToString();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public static void GettingStarted()
        {
            WriteLine("Getting Started");
            DBPlatform.DeleteDB();

            var db = new DB(1);
            db.GetConfig().EnsureTable<Member>("Table", "ID");
            var auto = db.Open();

            long key = 1;
            auto.Insert("Table", new Member { ID = key, Name = "Andy" });
            var o1 = auto.Get<Member>("Table", key);
            WriteLine(o1.Name);

            o1.Name = "Kelly";
            auto.Update("Table", o1);
            o1 = null;

            var o2 = auto.Get<Member>("Table", key);
            WriteLine(o2.Name);

            db.Dispose();

        }

        public static void IsolatedSpace()
        {
            WriteLine("Isolated Space");
            DBPlatform.DeleteDB();

            var db = new DB(1);
            db.GetConfig().EnsureTable<Member>("Member", "ID");
            // StringColumnName(Length) , default length is 32
            db.GetConfig().EnsureIndex<Member>("Member", "Name(20)");
            // Particular index for class MemberVIP.VIP
            db.GetConfig().EnsureIndex<MemberVIP>("Member", "VIP");

            // Composite Key Supported
            db.GetConfig().EnsureTable<Product>("Product", "Type", "UID");

            var auto = db.Open();

            long andyId, kellyId;
            //Creating Isolated Space, the Box.
            using (var box = auto.Cube())
            {
                andyId = box.NewId();
                kellyId = box.NewId();
                // insert members, two different classes with different index setting.
                box["Member"].Insert(new Member()
                {
                    ID = andyId,
                    Name = "Andy",
                    RegTime = new DateTime(2013, 1, 2),
                    Tags = new string[] { "Nice", "Player" }
                });

                box["Member"].Insert(new MemberVIP()
                {
                    ID = kellyId,
                    Name = "Kelly",
                    RegTime = new DateTime(2013, 1, 3),
                    Tags = new string[] { "Player" },
                    VIP = 3
                });


                Product game = new Product()
                {
                    Type = 8,
                    UID = new Guid("{22222222-0000-0000-0000-000000000000}"),
                    Name = "MoonFlight"
                };
                // Dynamic Column 
                game["GameType"] = "ACT";

                box["Product"].Insert(game);

                var cr = box.Commit();
                //cr.Equals(CommitResult.OK)
                cr.Assert();
            }


            using (var box = auto.Cube())
            {
                // Query Object, SQL Style
                //  > < >= <=  == != 
                //  & | () 
                //  []
                foreach (var m in box.Select<MemberVIP>("from Member where VIP>? ", 1))
                {
                    WriteLine("Member: " + m.Name + " RegTime: " + m.RegTime);
                }

                // Key-Value Style, Composite-Key Supported
                var cs = box["Product", 8, new Guid("{22222222-0000-0000-0000-000000000000}")].Select<Product>();
                WriteLine("Product: " + cs.Name + "  Type: " + cs["GameType"]);
            }

            using (var box = auto.Cube())
            {
                var m = box["Member", kellyId].Select<MemberVIP>();
                // Update Amount and Name
                m.Name = "Kelly J";
                m.Amount = 100;
                box["Member", m.ID].Update(m);
                box.Commit().Assert();
            }
            using (var box = auto.Cube())
            {
                foreach (var m in box.Select<Member>("from Member where Name==?", "Kelly J"))
                {
                    WriteLine("Updated: " + m.Name + "  Amount: " + m.Amount);
                }
            }
            db.Dispose();
        }

        public static void BeyondSQL()
        {
            DBPlatform.DeleteDB();

            var db = new DB(1);
            db.GetConfig().EnsureTable<MemberInc>("MemberInc", "ID");
            db.GetConfig().EnsureUpdateIncrementIndex<MemberInc>("MemberInc", "Version");
            var auto = db.Open();

            WriteLine("Update Increment");
            Write("Number increasing: ");

            MemberInc m = new MemberInc();
            m.ID = 1;
            m.Name = "Andy";

            auto.Insert("MemberInc", m);
            var mg = auto.Get<MemberInc>("MemberInc", 1L);
            Write(mg.Version + ".  ");

            auto.Update("MemberInc", mg);
            mg = auto.Get<MemberInc>("MemberInc", 1L);
            Write(mg.Version + ".  ");

            auto.Update("MemberInc", mg);
            mg = auto.Get<MemberInc>("MemberInc", 1L);
            WriteLine(mg.Version + ".  ");

            WriteLine("Selecting Tracer");
            using (var boxTracer = auto.Cube())
            {
                bool keepTrace = true;
                Member tra = boxTracer["MemberInc", 1L].Select<Member>(keepTrace);
                String currentName = tra.Name;

                {
                    // another box changes the name
                    MemberInc mm = new MemberInc();
                    mm.ID = 1;
                    mm.Name = "Kelly";
                    auto.Update("MemberInc", mm.ID, mm);
                }

                // Change detected
                if (!boxTracer.Commit().Equals(CommitResult.OK))
                {
                    Write("Detected '" + currentName + "' has changed, ");
                }
            }
            Member nm = auto.Get<Member>("MemberInc", 1L);
            WriteLine("new name is '" + nm.Name + "'");

            db.Dispose();
        }

        public static void MasterSlave()
        {
            DBPlatform.DeleteDB();

            long MasterA_DBAddress = 10;
            //  negative number
            long SlaveA_DBAddress = -10;

            var db = new DB(MasterA_DBAddress);
            db.GetConfig().EnsureTable<Member>("Member", "ID");
            db.SetBoxRecycler(new MemoryBoxRecycler());
            var auto = db.Open();

            var db_slave = new DB(SlaveA_DBAddress);
            var auto_slave = db_slave.Open();

            WriteLine("MasterSlave Replication");
            using (var box = auto.Cube())
            {
                for (var i = 0; i < 3; i++)
                {
                    box["Member"].Insert(new Member()
                    {
                        ID = box.NewId(),
                        Name = "LN " + i
                    });
                }
                box.Commit().Assert();
            }

            // Database Serialization
            var recycler = (MemoryBoxRecycler)auto.GetDatabase().GetBoxRecycler();
            lock (recycler.Packages)
            {
                foreach (var p in recycler.Packages)
                {
                    if (p.Socket.SourceAddress == MasterA_DBAddress)
                    {
                        (new BoxData(p.OutBox)).SlaveReplicate(auto_slave.GetDatabase()).Assert();
                    }
                }
                recycler.Packages.Clear();
            }

            WriteLine("Master Address: " + auto.GetDatabase().LocalAddress + ", Data:");
            using (var box = auto.Cube())
            {
                foreach (var o in box.Select<Member>("from Member", null))
                {
                    Write(o.Name + ".  ");
                }
            }
            WriteLine("");
            WriteLine("Slave Address: " + auto_slave.GetDatabase().LocalAddress + ", Data:");
            using (var box = auto_slave.Cube())
            {
                foreach (var o in box.Select<Member>("from Member", null))
                {
                    Write(o.Name + ".  ");
                }
            }
            WriteLine("");
            db.Dispose();
            db_slave.Dispose();
        }


        public static void MasterMaster()
        {
            DBPlatform.DeleteDB();
            long MasterA_DBAddress = 10;
            long MasterB_DBAddress = 20;

            var db_masterA = new DB(MasterA_DBAddress);
            db_masterA.GetConfig().EnsureTable<Member>("Member", "ID");
            db_masterA.SetBoxRecycler(new MemoryBoxRecycler());
            //send to MasterB_Address
            var auto_masterA = db_masterA.Open(MasterB_DBAddress);


            var db_masterB = new DB(MasterB_DBAddress);
            db_masterB.GetConfig().EnsureTable<Member>("Member", "ID");
            db_masterB.SetBoxRecycler(new MemoryBoxRecycler());
            // send to MasterA_Address
            var auto_masterB = db_masterB.Open(MasterA_DBAddress);


            WriteLine("MasterMaster Replication");
            byte IncTableID = 1;
            using (var box = auto_masterA.Cube())
            {
                for (var i = 0; i < 3; i++)
                {
                    box["Member"].Insert(new Member()
                    {
                        ID = box.NewId(IncTableID, 1) * 1000 + MasterA_DBAddress,
                        Name = "A" + i
                    });
                }
                box.Commit().Assert();
            }
            using (var box = auto_masterB.Cube())
            {
                for (var i = 0; i < 3; i++)
                {
                    box["Member"].Insert(new Member()
                    {
                        ID = box.NewId(IncTableID, 1) * 1000 + MasterB_DBAddress,
                        Name = "B" + i
                    });
                }
                box.Commit().Assert();
            }

            //Do Replication
            List<Package> buffer;
            var recycler = (MemoryBoxRecycler)auto_masterA.GetDatabase().GetBoxRecycler();
            lock (recycler.Packages)
            {
                buffer = new List<Package>(recycler.Packages);
                recycler.Packages.Clear();
            }
            recycler = (MemoryBoxRecycler)auto_masterB.GetDatabase().GetBoxRecycler();
            lock (recycler.Packages)
            {
                buffer.AddRange(recycler.Packages);
                recycler.Packages.Clear();
            }
            foreach (var p in buffer)
            {
                if (p.Socket.DestAddress == MasterA_DBAddress)
                {
                    (new BoxData(p.OutBox)).MasterReplicate(auto_masterA.GetDatabase());
                }
                if (p.Socket.DestAddress == MasterB_DBAddress)
                {
                    (new BoxData(p.OutBox)).MasterReplicate(auto_masterB.GetDatabase());
                }
            }

            WriteLine("MasterA Address: " + auto_masterA.GetDatabase().LocalAddress);
            using (var box = auto_masterA.Cube())
            {
                foreach (var o in box.Select<Member>("from Member", null))
                {
                    Write(o.Name + ". ");
                }
            }
            WriteLine("");
            WriteLine("MasterB Address: " + auto_masterB.GetDatabase().LocalAddress);
            using (var box = auto_masterB.Cube())
            {
                foreach (var o in box.Select<Member>("from Member", null))
                {
                    Write(o.Name + ". ");
                }
            }
            WriteLine("");
            db_masterA.Dispose();
            db_masterB.Dispose();
            /* another replication config
             Key = [ID,Address]
             m.ID =  box.NewId(IncTableID, 1) ;
             m.Address = box.LocalAddress;
             box.Bind("Member").Insert(m);
             */
        }


        public static void SpeedTest(int threadCount)
        {
            DBPlatform.DeleteDB();

            var db = new DB(1);
            db.GetConfig().EnsureTable<Member>("TSpeed", "ID");
            var dbConfig = db.GetConfig().DBConfig;
            //Cache
            //dbConfig.CacheLength = dbConfig.MB(512);
            //File
            //dbConfig.FileIncSize = (int)dbConfig.MB(4);
            //Thread 
            //dbConfig.ReadStreamCount = 8;
            var auto = db.Open();

            WriteLine("Speed");
            var objectCount = 10;
            WriteLine("Begin Insert " + (threadCount * objectCount).ToString("#,#"));

            DDebug.StartWatch();
            DBPlatform.For(0, threadCount, (i) =>
            {
                using (var box = auto.Cube())
                {
                    for (var o = 0; o < objectCount; o++)
                    {
                        var m = new Member
                        {
                            ID = box.NewId(0, 1),
                            Name = i.ToString() + "_" + o.ToString(),
                            Age = 1
                        };
                        box["TSpeed"].Insert(m);
                    }
                    box.Commit().Assert();
                }
            });

            var sec = DDebug.StopWatch().TotalSeconds;
            var avg = (threadCount * objectCount) / sec;
            WriteLine("Elapsed " + sec + "s, AVG Insert " + avg.ToString("#,#") + " o/sec");

            DDebug.StartWatch();
            DBPlatform.For(0, threadCount, (i) =>
            {
                using (var box = auto.Cube())
                {
                    for (var o = 0; o < objectCount; o++)
                    {
                        long ID = i * objectCount + o + 1;
                        var mem = box["TSpeed", ID].Select<Member>();
                        if (mem.ID != ID) { throw new Exception(); }
                    }
                }
            });

            sec = DDebug.StopWatch().TotalSeconds;
            avg = (threadCount * objectCount) / sec;
            WriteLine("Elapsed " + sec + "s, AVG Lookup " + avg.ToString("#,#") + " o/sec");

            DDebug.StartWatch();
            int count = 0;
            DBPlatform.For(0, threadCount, (i) =>
            {
                using (var box = auto.Cube())
                {
                    var tspeed = box.Select<Member>("from TSpeed where ID>=? & ID<=?",
                        (long)(i * objectCount + 1), (long)(i * objectCount + objectCount));
                    foreach (var m in tspeed)
                    {
                        // age == 1
                        Interlocked.Add(ref count, m.Age);
                    }
                }
            });
            if (count != (threadCount * objectCount)) { throw new Exception(count.ToString()); }

            sec = DDebug.StopWatch().TotalSeconds;
            avg = count / sec;
            WriteLine("Elapsed " + sec + "s, AVG Query " + avg.ToString("#,#") + " o/sec");

            db.Dispose();
        }


        public static void ReplicationSpeed(int threadCount, int time)
        {
            DBPlatform.DeleteDB();
            int MasterA_DBAddress = 10;
            int MasterB_DBAddress = 20;

            int SlaveA_DBAddress = -10;

            var db_masterA = new DB(MasterA_DBAddress);
            db_masterA.GetConfig().EnsureTable<Member>("TSpeed", "ID");
            db_masterA.SetBoxRecycler(new MemoryBoxRecycler());
            var auto_masterA = db_masterA.Open(MasterB_DBAddress);

            var db_slave = new DB(SlaveA_DBAddress);
            var auto_slave = db_slave.Open();

            var db_masterB = new DB(MasterB_DBAddress);
            db_masterB.GetConfig().EnsureTable<Member>("TSpeed", "ID");
            var auto_masterB = db_masterB.Open();

            var data = ((MemoryBoxRecycler)auto_masterA.GetDatabase().GetBoxRecycler()).AsBoxData();
            BoxData.SlaveReplicate(auto_slave.GetDatabase(), data).Assert();
            BoxData.MasterReplicate(auto_masterB.GetDatabase(), data).Assert();

            var objectCount = 10;

            double slaveSec = 0;
            double masterSec = 0;
            for (var t = 0; t < time; t++)
            {
                DBPlatform.For(0, threadCount, (i) =>
                {
                    using (var box = auto_masterA.Cube())
                    {
                        for (var o = 0; o < objectCount; o++)
                        {
                            var m = new Member
                            {
                                ID = box.NewId(0, 1),
                                Name = i.ToString() + "_" + o.ToString(),
                                Age = 1
                            };
                            box["TSpeed"].Insert(m);
                        }
                        box.Commit().Assert();
                    }
                });


                data = ((MemoryBoxRecycler)auto_masterA.GetDatabase().GetBoxRecycler()).AsBoxData();
                DDebug.StartWatch();
                BoxData.SlaveReplicate(auto_slave.GetDatabase(), data).Assert();
                slaveSec += DDebug.StopWatch().TotalSeconds;


                DDebug.StartWatch();
                BoxData.MasterReplicate(auto_masterB.GetDatabase(), data).Assert();
                masterSec += DDebug.StopWatch().TotalSeconds;

            }
            WriteLine("Replicate " + (threadCount * time).ToString("#,#") + " transactions, totals " + (threadCount * objectCount * time).ToString("#,#") + " objects");
            var avg = (threadCount * objectCount * time) / slaveSec;
            WriteLine("SlaveSpeed " + slaveSec + "s, AVG " + avg.ToString("#,#") + " o/sec");

            avg = (threadCount * objectCount * time) / masterSec;
            WriteLine("MasterSpeed " + masterSec + "s, AVG " + avg.ToString("#,#") + " o/sec");

            int count = 0;

            DDebug.StartWatch();
            for (var t = 0; t < time; t++)
            {
                DBPlatform.For(0, threadCount, (i) =>
                {
                    for (var dbc = 0; dbc < 2; dbc++)
                    {
                        using (var box = dbc == 0 ? auto_slave.Cube() : auto_masterB.Cube())
                        {
                            for (var o = 0; o < objectCount; o++)
                            {
                                long ID = i * objectCount + o + 1;
                                ID += (t * threadCount * objectCount);
                                var mem = box["TSpeed", ID].Select<Member>();
                                if (mem.ID != ID) { throw new Exception(); }
                                Interlocked.Add(ref count, mem.Age);
                            }
                        }
                    }
                });
            }

            if (count != (threadCount * objectCount * time * 2))
            {
                throw new Exception();
            }
            masterSec = DDebug.StopWatch().TotalSeconds;
            avg = count / masterSec;
            WriteLine("Lookup after replication " + masterSec + "s, AVG " + avg.ToString("#,#") + " o/sec");

            auto_masterA.GetDatabase().Dispose();
            auto_slave.GetDatabase().Dispose();
            auto_masterB.GetDatabase().Dispose();
        }


        private static StringBuilder strout = new StringBuilder();
        private static StringBuilder WriteLine(string str)
        {
            strout.Append(str + "\r\n");
            return strout;
        }
        private static StringBuilder Write(string str)
        {
            strout.Append(str);
            return strout;
        }

        public abstract class IDClass
        {
            public long ID;
        }

        public class Member : IDClass
        {
            public string Name;

            public DateTime RegTime;

            public string[] Tags;

            public decimal Amount;

            public int Age;
        }

        public class MemberVIP : Member
        {
            public int VIP;
        }

        public class MemberInc : Member
        {
            // UpdateIncrement, type is long
            public long Version;
        }

        public class Product : Dictionary<string, object>
        {

            public int Type
            {
                get
                {
                    return (int)this["Type"];
                }
                set
                {
                    this["Type"] = value;
                }
            }

            public Guid UID
            {
                get
                {
                    return (Guid)this["UID"];
                }
                set
                {
                    this["UID"] = value;
                }
            }


            public string Name
            {
                get
                {
                    return (string)this["Name"];
                }
                set
                {
                    this["Name"] = value;
                }
            }
        }
    }

    public class DBPlatform
    {
        public static void SetStorage()
        {
            //DB.Root(path); DB Files Path
#if (WINDOWS_PHONE || UNITY_WP8) && (!UNITY_EDITOR)
            DB.Root(Windows.Storage.ApplicationData.Current.LocalFolder.Path);
#else
#if (NETFX_CORE || UNITY_METRO) && (!UNITY_EDITOR)
            iBoxDB.WSDatabaseConfig.ResetStorage();
#else
            Xamarin_IL2CPP_MakeSureClassesAreLinked();
#endif
#endif

        }

        public delegate void FRun(int i);


#if WINDOWS_PHONE || NET2 || UNITY_EDITOR || UNITY_WP8 || UNITY_ANDROID
        public static void For(int start, int end, FRun run)
        {
            for (var i = start; i < end; i++)
            {
                run(i);
            }
        }
#else
        public static void For(int start, int end, FRun run)
        {
            System.Threading.Tasks.Parallel.For(start, end,
                          (i) =>
                          {
                              run(i);
                          });
        }
#endif

#if WINDOWS_PHONE || NET2 || UNITY_EDITOR || NETFX_CORE || UNITY_WP8 || UNITY_METRO || UNITY_ANDROID
        public static int Speed_ThreadCount = 200;
        public static int Replication_ThreadCount = 40;
        public static int Replication_Time = 4;
#else
        public static int Speed_ThreadCount = 20000;
        public static int Replication_ThreadCount = 200;
        public static int Replication_Time = 10;

#endif

        public static void DeleteDB()
        {
            DDebug.DeleteDBFiles(new long[] { 1, 10, 20, -10 });
        }
        private static void Xamarin_IL2CPP_MakeSureClassesAreLinked()
        {
#if !NETFX_CORE && !UNITY_METRO && !WINDOWS_PHONE && !UNITY_WP8 && !UNITY_EDITOR
            if (Object.ReferenceEquals(1, 2))
            {
                //ensure the methods 
                new System.IO.FileStream("", System.IO.FileMode.OpenOrCreate,
                   System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite, 1);
            }
#endif
        }
    }

    public class Package
    {
        public Socket Socket;
        public byte[] OutBox;
    }
    // recycle boxes
    public class MemoryBoxRecycler : IBoxRecycler //,IBoxRecycler3
    {
        public List<Package> Packages = new List<Package>();

        public MemoryBoxRecycler() { }
        public MemoryBoxRecycler(long name, DatabaseConfig config) : this()
        {
        }

        public void OnReceived(Socket socket, BoxData outBox, bool normal)
        {
            if (socket.DestAddress == long.MaxValue)
            {
                // default replicate address
                return;
            }
            lock (Packages)
            {
                Packages.Add(new Package { Socket = socket, OutBox = outBox.ToBytes() });
            }
        }

        public BoxData[] AsBoxData()
        {
            List<BoxData> list = new List<BoxData>();
            lock (Packages)
            {
                foreach (var p in Packages)
                {
                    list.Add(new BoxData(p.OutBox));
                }
                Packages.Clear();
            }
            return list.ToArray();
        }
        public void Dispose()
        {
            Packages = null;
        }
    }

    //ALL in One Config, var server = new ApplicationServer();
    public class ApplicationServer : LocalDatabaseServer
    {
        public class Config : BoxFileStreamConfig
        {
            public Config()
                : base()
            {
                CacheLength = MB(512);
                FileIncSize = (int)MB(4);
                ReadStreamCount = 8;
            }
        }
        class MyConfig : Config
        {
            public MyConfig()
                : base()
            {
                EnsureTable<NDB.Member>("Member", "ID");
                EnsureIndex<NDB.Member>("Member", "Name(20)");
                EnsureIndex<NDB.Member>("Member", "VIP");

                EnsureTable<NDB.Product>("Product", "Type", "UID");

                EnsureTable<NDB.Member>("TSpeed", "ID");

                EnsureTable<NDB.MemberInc>("MemberInc", "ID");
                EnsureUpdateIncrementIndex<NDB.MemberInc>("MemberInc", "Version");

                //KeyOnly Table, StartsWith '/', only read/write ID and Name
                EnsureTable<NDB.Member>("/M", "ID", "Name");
            }
        }
        public const int MasterA_DBAddress = 10;
        public const int MasterB_DBAddress = 20;
        public const int SlaveA_DBAddress = -10;

        protected override DatabaseConfig BuildDatabaseConfig(long name)
        {
            if (name == MasterB_DBAddress || name == MasterA_DBAddress)
            {
                return new MyConfig();
            }
            if (name == SlaveA_DBAddress)
            {
                return new Config();
            }
            throw new NotImplementedException();
        }

        protected override IBoxRecycler BuildBoxRecycler(long name, DatabaseConfig config)
        {
            if (name == MasterA_DBAddress || name == MasterB_DBAddress)
            {
                return new MemoryBoxRecycler(name, config);
            }
            return base.BuildBoxRecycler(name, config);
        }
    }
}
