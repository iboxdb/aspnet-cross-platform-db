print("iBoxDB .NET Python!")

# pip install pythonnet

# sudo dnf remove mono-devel-6.12.0.107-0.xamarin.9.epel8.x86_64
# sudo dnf install mono-complete

import sys
import os;
#AddReference Path
#sys.path.append("./lib")

from pythonnet import load
# ldconfig -p | grep 'mono'
# find -L /lib64 -name 'libmonosgen*.*'
# libmono = Path/"Mono/bin/mono-2.0-sgen.dll" , "libmonosgen-2.0.so.1"
load("mono")
import clr # as IronPython
# Load Database (ver 4.0), the same Directory or set sys.path
clr.AddReference("iBoxDB.NET2")

from System import Int64,Double,String,Decimal
from IBoxDB.LocalServer import Ason, DB
print("CRUD",clr.GetClrType(Ason),clr.GetClrType(DB))


# Prototype
proto = Ason("id:",Int64(0), "name:",String("_"), "val:",Double(0.0), "star", Decimal(0))
 
DB.Root("../DBRoot")
db = DB(1)
cfg = db.get_config()
cfg.ensure_table(proto,"table","id")
isUnique = False
cfg.ensure_index(proto,"table",isUnique,"name")
auto = db.open_doc()

#print(dir(auto))
#print(dir(proto))
#print(proto.clone.__doc__)


# clone for insert
doc = proto.clone()
# s() == set()
doc.s("id", auto.new_id()+0)
doc.s("name","super_bee")
doc.s("val",100.88)
doc.s("star","123456789123456789")
auto.insert("table",doc)

# +0, +1 to show compatible with python
l = auto.select("from table where id<=? limit 0,3",doc["id"]+1)
print(l)


# clone for update
doc1 = l[0].clone() 
doc1.s("val", doc["val"]+15 )
doc1.s("star","987654321987654321.54321")
auto.update("table",doc1)

doc2 = auto.get("table", doc1["id"]+0)
print(doc2)
doc = None
doc1 = None

print( auto.count("from table where id>=?", -1) )
auto.delete("table",doc2["id"])
print( auto.count("table") )
doc2 = None

doc = proto.clone()
doc.s("id", auto.new_id())
doc.s("name","one_bee")
doc.s("val",-1)
doc.s("star",'-2')
# as insert()
auto.replace("table",doc)

doc2 = auto.get("table", doc["id"])
print(doc2)


# clone for update
doc2 = doc2.clone()
doc2.s( "val", doc2["val"]-1 )
# as update()
auto.replace("table",doc2)

doc3 = auto.get("table", doc["id"])
print(doc3)
#print(type(doc3["star"]))

auto.close_db()
del clr