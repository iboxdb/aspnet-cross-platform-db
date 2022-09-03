//using Godot;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using IBoxDB.LocalServer;


//var idb = preload("res://iboxdb.cs").new()
public class iboxdb : Godot.Object
{
    private DB _db;
    private AutoBox _auto = null;
    private int _db_id;

    static iboxdb()
    {
        //Path
        var t = new Godot.File();
        t.Open("user://DBROOT.tmp", Godot.File.ModeFlags.Write);
        var dir = Path.GetDirectoryName(t.GetPathAbsolute());
        t.Close();

        dir = Path.Combine(dir, "DBROOT");
        Directory.CreateDirectory(dir);
        DB.Root(dir);
    }

    public iboxdb(int db_id)
    {
        _db_id = db_id;
        _db = new DB(db_id);
    }
    public iboxdb() : this(1)
    {

    }
    private D wrap<D>(IDictionary s) where D : class, IDictionary, new()
    {
        if (s == null) { return null; }
        D d = new D();
        foreach (DictionaryEntry e in s)
        {
            d[e.Key] = e.Value;
        }
        return d;
    }


    public bool is_opening()
    {
        return _auto != null;
    }


    //idb.ensure_table({"id":0}, "table" , [])
    public void ensure_table(IDictionary protoType, string tableName, string[] key)
    {
        lock (typeof(iboxdb))
        {
            var w = wrap<Dictionary<String, Object>>(protoType);
            _db.GetConfig().EnsureTable(w, tableName, key);
        }
    }


    //idb.ensure_index({"name":"myname"}, "table", false, [])
    public void ensure_index(IDictionary protoType, string tableName, bool isUnique, string[] key)
    {
        lock (typeof(iboxdb))
        {
            var w = wrap<Dictionary<String, Object>>(protoType);
            _db.GetConfig().EnsureIndex(w, tableName, isUnique, key);
        }
    }


    public string open()
    {
        try
        {
            lock (typeof(iboxdb))
            {
                if (_auto == null)
                {
                    _db.MinConfig();
                    _auto = _db.Open();
                }
            }
            return DB.Root(null);
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    public Godot.Collections.Dictionary find(string table, Object key)
    {
        var l = _auto.Get<Dictionary<String, Object>>(table, key);
        return wrap<Godot.Collections.Dictionary>(l);
    }

    public Godot.Collections.Array<Godot.Collections.Dictionary> select(String ql, Object[] args)
    {
        var l = new Godot.Collections.Array<Godot.Collections.Dictionary>();
        foreach (var e in _auto.Select<Dictionary<String, Object>>(ql, args))
        {
            var w = wrap<Godot.Collections.Dictionary>(e);
            l.Add(w);
        }
        return l;
    }

    public Godot.Collections.Array<Godot.Collections.Dictionary> select(String ql)
    {
        return select(ql, new object[0]);
    }

    public int count(String ql, Object[] args)
    {
        return (int)_auto.Count(ql, args);
    }

    public int count(String ql)
    {
        return count(ql, new object[0]);
    }

    public int newId()
    {
        return (int)_auto.NewId();
    }

    public bool insert(string tableName, IDictionary obj)
    {
        var w = wrap<Dictionary<String, Object>>(obj);
        return _auto.Insert(tableName, w);
    }
    public bool update(string tableName, IDictionary obj)
    {
        var w = wrap<Dictionary<String, Object>>(obj);
        return _auto.Update(tableName, w);
    }
    public bool replace(string tableName, IDictionary obj)
    {
        var w = wrap<Dictionary<String, Object>>(obj);
        return _auto.Replace(tableName, w);
    }
    public bool delete(string tableName, object key)
    {
        return _auto.Delete(tableName, key);
    }
    public bool delete(string tableName, object[] key)
    {
        return _auto.Delete(tableName, key);
    }

    // HTML5 Save
    //idb.save_web( JavaScript )
    public object save_web(Godot.Object JavaScript)
    {
        String script = "FS.syncfs(false, function (err) {});";
        try
        {
           return JavaScript.Call("eval", script);
        }
        catch (Exception ex)
        {
            var msg = ex.Message;
            return msg;
        }
    }
    public void exit()
    {
        _auto.GetDatabase().Dispose();
        _auto = null;
        _db = null;
    }

    public void debug_clear()
    {
        BoxSystem.DBDebug.DeleteDBFiles(_db_id);
    }

}
