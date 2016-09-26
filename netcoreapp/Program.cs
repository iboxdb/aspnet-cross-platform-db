using System;

//Test version sudo apt-get install dotnet-dev-1.0.0-preview2-003131
// user@ubuntu:~/.nuget/packages/iBoxDB/2.9.1/content$ cp iBoxDB291.cs /home/user/dnxcore003131
namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
			iBoxDB.LocalServer.DB.Root("/tmp/");
			var text =  iBoxDB.NDB.RunALL();
			Console.WriteLine (text);
        }
    }
}
