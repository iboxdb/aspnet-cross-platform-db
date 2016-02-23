using System.IO;
using iBoxDB.LocalServer;
using iBoxDB.LocalServer.IO;
using System;

namespace iBoxDB
{
    // An example for encryption.
    // iBoxDB.EncryptDatabaseConfig.ResetStorage();   var server = new DB();
    public class EncryptDatabaseConfig : DatabaseConfig
    {
        public static void ResetStorage()
        {
            BoxFileStreamConfig.AdapterType = typeof(EncryptDatabaseConfig);
        }

        private static byte[] ReadPasswordFromServer()
        {
            return new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 11, 12 };
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
            FileStream fs = new FileStream(LocalRoot + path, FileMode.OpenOrCreate,
              access == StreamAccess.Read ? FileAccess.Read : FileAccess.ReadWrite,
              access == StreamAccess.Read ? FileShare.ReadWrite : FileShare.Read, 1);
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
