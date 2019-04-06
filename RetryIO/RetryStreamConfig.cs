using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using iBoxDB.LocalServer;
using iBoxDB.LocalServer.IO;

//Retry IO, for unstable remote IO.
//Test By Yourself.

public class RetryStreamConfig : DatabaseConfig
{
    public int Timeout = 30;

    public static void ResetStorage()
    {
        BoxFileStreamConfig.AdapterType = typeof(RetryStreamConfig);
    }
    public static Stream CreateNetworkStream(string path)
    {
        //Create Remote Connection, change to your own network stream
        return new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
    }
    public override bool ExistsStream(string path)
    {
        //Change to your own network
        return File.Exists(BoxFileStreamConfig.RootPath + path);
    }

    Dictionary<string, RetryStream> dict = new Dictionary<string, RetryStream>();
    public override IBStream CreateStream(string path, StreamAccess access)
    {
        //one remote connection each file; each remote connection has its own inner cache, 
        //two connections for one file, some cache will not update automatically.
        RetryStream o;
        if (dict.TryGetValue(path, out o))
        {
            return o;
        }
        o = new RetryStream(this, BoxFileStreamConfig.RootPath + path, access);
        dict.Add(path, o);
        return o;
    }


    public class RetryStream : IBStream
    {
        RetryStreamConfig owner;
        string path;
        StreamAccess access;

        Stream str;

        public RetryStream(RetryStreamConfig _owner, string _path, StreamAccess _access)
        {
            owner = _owner;
            path = _path;
            access = _access;
            str = CreateNetworkStream();
        }
        public Stream CreateNetworkStream()
        {
            return RetryStreamConfig.CreateNetworkStream(path);
        }

        private R Retry<R>(Func<R> act, Action err, string msg)
        {
            //LOCK, retry can't simultaneously
            lock (owner)
            {
                int retry = owner.Timeout;

                while (retry > 0)
                {
                    retry--;
                    try
                    {
                        return act();
                    }
                    catch
                    {
                        Console.WriteLine($"Retry {msg} , {retry}");
                        Thread.Sleep(1000);
                        if (err != null)
                        {
                            err();
                        }
                    }
                }
                return act();
            }
        }

        private void ReCreate()
        {
            Retry(() =>
            {
                foreach (var e in owner.dict.Values)
                {
                    e.Dispose();
                    e.str = e.CreateNetworkStream();
                    //Read Test
                    e.str.Position = 0;
                    e.str.ReadByte();
                }
                return 0;
            }, null, "Create");
        }

        public void Dispose()
        {
            try
            {
                if (str != null)
                {
                    str.Dispose();
                }
            }
            catch
            {
            }
            str = null;
        }

        public long Length
        {
            get
            {
                return Retry(() =>
                {
                    return str.Length;
                }, ReCreate, "Length");
            }
        }

        public void BeginWrite(long appID, int maxLen)
        {
            Retry(() =>
                {
                    //str.BeginWrite(appID, maxLen);
                    return 0;
                }, ReCreate, "BeginWrite");

        }

        public void EndWrite()
        {
            Retry(() =>
                   {
                       //str.EndWrite();
                       return 0;
                   }, ReCreate, "EndWrite");
        }

        public void Flush()
        {
            Retry(() =>
                {
                    str.Flush();
                    return 0;
                }, ReCreate, "Flush");
        }

        public int Read(long position, byte[] buffer, int offset, int count)
        {
            return Retry(() =>
               {
                   str.Position = position;
                   return str.Read(buffer, offset, count);
               }, ReCreate, "Read");
        }

        public void SetLength(long value)
        {
            Retry(() =>
                     {
                         str.SetLength(value);
                         return 0;
                     }, ReCreate, "SetLength");
        }

        public void Write(long position, byte[] buffer, int offset, int count)
        {
            Retry(() =>
              {
                  str.Position = position;
                  str.Write(buffer, offset, count);
                  return 0;
              }, ReCreate, "Write");
        }
    }
}