using System;
using System.IO;
using IBoxDB.LocalServer;
using IBoxDB.LocalServer.IO;


namespace IBoxDB.IO
{
    // An example for encryption.
    // EncryptDatabaseConfig.ResetStorage();  
    // var db = new DB();
    // <PackageReference Include="iBoxDB" Version="" />	
    public class EncryptDatabaseConfig : DatabaseConfig
    {
        public static void ResetStorage()
        {
            BoxFileStreamConfig.AdapterType = typeof(EncryptDatabaseConfig);
        }

        private static byte[] ReadPasswordFromServer()
        {
            //the key at least 8K
            byte[] key = new byte[1024 * 8 + 479];
            for (int i = 0; i < key.Length; i++)
            {
                key[i] = (byte)(i + i / 3 + i % 5);
            }
            return key;
        }

        private readonly byte[] password;
        public string LocalRoot;
        public EncryptDatabaseConfig()
        {
            password = ReadPasswordFromServer();
            this.LocalRoot = BoxFileStreamConfig.RootPath;
        }
        public override void Dispose()
        {
            base.Dispose();
        }

        public override IBStream CreateStream(string path, StreamAccess access)
        {
            int noBuffer = 1;
            FileStream fs = new FileStream(LocalRoot + path, FileMode.OpenOrCreate,
              access == StreamAccess.Read ? FileAccess.Read : FileAccess.ReadWrite,
              access == StreamAccess.Read ? FileShare.ReadWrite : FileShare.Read, noBuffer);
            return new LocalStream(fs, password);
        }
        public override bool ExistsStream(string path)
        {
            return File.Exists(LocalRoot + path);
        }


        class LocalStream : IBStream
        {
            private FileStream fs;
            private byte[] password;
            public LocalStream(FileStream _fs, byte[] pw)
            {
                this.fs = _fs;
                this.password = pw;
            }


            public void SetLength(long value)
            {
                fs.SetLength(value);
            }

            public long Length
            {
                get { return fs.Length; }
            }

            public int Read(long position, byte[] buffer, int offset, int count)
            {
                fs.Position = position;
                count = fs.Read(buffer, offset, count);

                for (int i = 0; i < count; i++)
                {
                    long pos = position + i;
                    buffer[offset + i] ^= password[pos % password.Length];
                }
                return count;
            }

            public void Write(long position, byte[] buffer, int offset, int count)
            {
                byte[] buf = new byte[count];
                Buffer.BlockCopy(buffer, offset, buf, 0, count);

                for (int i = 0; i < count; i++)
                {
                    long pos = position + i;
                    buf[i] ^= password[pos % password.Length];
                }
                fs.Position = position;
                fs.Write(buf, 0, buf.Length);
            }

            public void Flush()
            {
                fs.Flush();
            }

            public void Dispose()
            {
                fs.Dispose();
            }

            public void BeginWrite(long appID, int maxLen)
            {

            }

            public void EndWrite()
            {

            }

        }
    }
}

namespace IBoxDB.IO.Test
{
    //this ZIP-IO not for encryption,
    //just to show how small the database is.
    using System.IO.Compression;

    public class EncryptDatabaseConfigTest
    {

        public static void Test()
        {
            EncryptDatabaseConfig.ResetStorage();
            DB.Root("../Encrypted_Database");
            long fileAddress = 1L;
            DB db = new DB(fileAddress);

            var cfg = db.MinConfig();
            cfg.FileIncSize = 1;
            cfg.EnsureTable<Record>("Record", "Id");
            var auto = db.Open();
            auto.Insert("Record", new Record
            {
                Id = auto.NewId(),
                Value = DateTime.Now.ToString()
            });
            Console.WriteLine("File:");
            Console.WriteLine(auto.Select("from Record"));
            auto.GetDatabase().Dispose();

            Console.WriteLine("File(Re-Open):");
            db = new DB(fileAddress);
            auto = db.Open();
            Console.WriteLine(auto.Select("from Record"));

            Console.WriteLine("Memory:");
            var bs = auto.GetDatabase().CopyTo();

            using (var mm = new MemoryStream())
            {
                using (var zip = new GZipStream(mm, CompressionMode.Compress))
                {
                    zip.Write(bs, 0, bs.Length);
                }
                bs = mm.ToArray();
            }
            Console.WriteLine("Database Size is : " + bs.Length / 1024 + " (KB) ");

            using (var mm = new MemoryStream())
            {
                using (var zip = new GZipStream(new MemoryStream(bs), CompressionMode.Decompress))
                {
                    zip.CopyTo(mm);
                }
                bs = mm.ToArray();
            }

            var memAuto = new DB(bs).Open();
            Console.WriteLine(memAuto.Select("from Record"));
            memAuto.GetDatabase().Dispose();
            auto.GetDatabase().Dispose();
        }

        public class Record
        {
            public long Id;
            public String Value;
        }
    }

}
