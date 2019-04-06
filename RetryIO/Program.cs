using System;
using System.Threading;
using iBoxDB.LocalServer;

//Network IO Test
namespace RetryIO
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Started");
            RetryStreamConfig.ResetStorage();
            DB.Root("/mnt/hgfs/Share/db");


            {/* 
                DB.Root("/tmp");
                var ra = iBoxDB.NDB.RunALL(true);
                Console.WriteLine(ra);
                return; */
            }

            var auto = new Func<AutoBox>(() =>
            {

                //iBoxDB.DBDebug.DDebug.DeleteDBFiles(1);
                DB db = new DB(1);

                db.GetConfig().EnsureTable<TestObject>(nameof(TestObject), "Id");

                //Disable Cache, for Test only. it should be big
                db.GetConfig().DBConfig.CacheLength = 1;

                return db.Open();
            })();
            using (auto.GetDatabase())
            {
                string r = "Begin";
                var thread1 = new Thread(() =>
{
    while (r != "exit")
    {
        try
        {
            Console.WriteLine("Count: " + auto.SelectCount($"from {nameof(TestObject)}"));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Thread1  {ex.Message}");
        }
        Thread.Sleep(1);
    }
}
                );


                var thread2 = new Thread(() =>
    {
        while (r != "exit")
        {
            try
            {
                using (var box = auto.Cube())
                {
                    box[nameof(TestObject)].Insert(
                        new TestObject
                        {
                            Id = Guid.NewGuid(),
                            Value = $"{r} ,{DateTime.Now}"
                        }
                        );
                    Console.WriteLine($"Commit {box.Commit()}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Thread2  {ex.Message}");
            }
            Thread.Sleep(1);
        }
    }
                );
                thread1.Start();
                thread2.Start();
                while ((r = Console.ReadLine()) != "exit")
                {
                    Console.WriteLine($"Input {r}");

                }
                thread1.Join();
                thread2.Join();
            }
            Console.WriteLine("End.");
        }
    }

    public class TestObject
    {
        public Guid Id;
        public String Value;
    }
}
