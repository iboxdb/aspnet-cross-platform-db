using System;

namespace hwapp
{
    class Program
    {
        static void Main(string[] args)
        {
   iBoxDB.LocalServer.DB.Root("/tmp/");
  var text =  iBoxDB.NDB.RunALL(true);
  Console.WriteLine(text);
            Console.WriteLine("Hello World! NET1.0");
        }
    }
}
