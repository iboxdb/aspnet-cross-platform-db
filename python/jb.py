print("iBoxDB Java Python!")

# pip install jpype1
# Download openjdk

import os;
print(os.environ.get("JAVA_HOME"))
os.environ["JAVA_HOME"] = "/home/user/Software/openjdk-25_linux-x64_bin/jdk-25"
print(os.environ.get("JAVA_HOME"))


import jpype
# ModuleNotFoundError if not imported
import jpype.imports
 
jpype.startJVM()
print("Version: ", jpype.getJVMVersion(), jpype.getDefaultJVMPath())
# Load Database, the same Directory or set Path
jpype.addClassPath("iboxdb-3.9.5.jar")

from java.lang import Long, Double, String
from java.math import BigDecimal          
from iboxdb.localserver import Ason,DB

print("CRUD",Ason.class_,DB.class_)

# Prototype
proto = Ason("id:",Long(0), "name:",String("_"), "val:",Double(0.0), "star", BigDecimal('0'))

DB.root("../DBRoot")
db = DB(1)
cfg = db.getConfig()
cfg.ensureTable(proto,"table","id")
isUnique = False
cfg.ensureIndex(proto,"table",isUnique,"name")
auto = db.open()

#print(dir(auto))
#print(dir(proto))
#print(proto.clone.__doc__)

# clone for insert
doc = proto.clone()
# s() == set()
doc.s("id", auto.newId()+0)
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
doc.s("id", auto.newId())
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

auto.getDatabase().close()
jpype.shutdownJVM()

del jpype