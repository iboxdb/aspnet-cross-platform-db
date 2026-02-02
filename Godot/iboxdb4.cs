using Godot;
using Godot.Collections;
using System.IO;
using System.Linq;
using IBoxDB.LocalServer;


//var iboxdb4 = preload("res://iboxdb4.cs")
public partial class iboxdb4 : GodotObject
{

    private DB _db;
    private AutoBoxDoc _auto = null;
    private long _db_id;
    static iboxdb4()
    {
        //Path
        var dir = OS.GetUserDataDir();
        dir = Path.Combine(dir, "DBROOT");
        Directory.CreateDirectory(dir);
        DB.Root(dir);
    }

    public iboxdb4(int db_id)
    {
        _db_id = db_id;
        _db = new DB(db_id);
    }
    public iboxdb4() : this(1)
    {

    }
    private Dictionary wrap(Local s)
    {
        if (s == null) { return null; }
        Dictionary d = new Dictionary();
        foreach (var kv in s)
        {
            //GD.Print(kv.Key, kv.Value.GetType());
            if (kv.Value is System.Int64)
            {
                d[kv.Key] = (System.Int64)kv.Value;
            }
            else if (kv.Value is System.Double)
            {
                d[kv.Key] = (System.Double)kv.Value;
            }
            else
                d[kv.Key] = kv.Value?.ToString();
        }
        return d;
    }
    private Ason wrap(Dictionary s)
    {
        if (s == null) { return null; }
        Ason a = new Ason();
        foreach (var e in s)
        {
            a[e.Key.AsString()] = e.Value.Obj;
        }
        return a;
    }

    private object[] wrap(Array s)
    {
        if (s == null) { return []; }
        return s.Select(c => c.Obj).ToArray();
    }

    public void ensure_table(Dictionary protoType, string tableName, string[] key)
    {

        try
        {
            lock (typeof(iboxdb4))
            {
                var w = wrap(protoType);
                _db.get_config().ensure_table(w, tableName, key);
            }
        }
        catch (System.Exception ex)
        {
            GD.Print(ex.Message);
        }
    }

    public void ensure_index(Dictionary protoType, string tableName, bool isUnique, string[] key)
    {
        try
        {
            lock (typeof(iboxdb4))
            {
                var w = wrap(protoType);
                _db.get_config().ensure_index(w, tableName, isUnique, key);
            }
        }
        catch (System.Exception ex)
        {
            GD.Print(ex.Message);
        }
    }


    public string open()
    {
        try
        {
            lock (typeof(iboxdb4))
            {
                if (_auto == null)
                {
                    _db.MinConfig();
                    _auto = _db.open_doc();
                }
            }
            return DB.Root(null);
        }
        catch (System.Exception ex)
        {
            return ex.Message;
        }
    }

    public Dictionary get(string table, Array key)
    {
        var l = _auto.get(table, wrap(key));
        return wrap(l);
    }

    public Array<Dictionary> select(string ql, Array args)
    {
        var r = _auto.select(ql, wrap(args));
        return new Array<Dictionary>(r.Select(c => wrap(c)));
    }

    public long count(string ql, Array args)
    {
        return _auto.count(ql, wrap(args));
    }

    public long new_id()
    {
        return _auto.new_id();
    }

    public bool insert(string tableName, Dictionary obj)
    {
        try
        {
            var w = wrap(obj);
            return _auto.insert(tableName, w);
        }
        catch (System.Exception ex)
        {
            GD.Print(ex.Message);
            return false;
        }
    }
    public bool update(string tableName, Dictionary obj)
    {
        var w = wrap(obj);
        return _auto.update(tableName, w);
    }
    public bool replace(string tableName, Dictionary obj)
    {
        var w = wrap(obj);
        return _auto.replace(tableName, w);
    }
    public bool delete(string tableName, Array key)
    {
        return _auto.delete(tableName, wrap(key));
    }


    public void close_db()
    {
        _auto.close_db();
        _auto = null;
        _db = null;
    }

    public void debug_clear()
    {
        BoxSystem.DBDebug.DeleteDBFiles(_db_id);
    }

    public static void print(Array objs)
    {
        foreach (var o in objs)
        {
            GD.Print(o.Obj);
            GD.Print(o.Obj.GetType());
        }
    }

}
