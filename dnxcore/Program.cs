using System;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello iBoxDB!");
            //iBoxDB.LocalServer.DB.Root("/tmp/");
            var text = iBoxDB.TestHelper.RunALL();
            Console.WriteLine(text); 
        }
    }
}
