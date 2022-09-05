extends MeshInstance

var idb = preload("res://iboxdb.cs").new()
#var idb = preload("res://iboxdb.cs").create(2)

func _ready():	
	print(idb.get_os_name());
	#idb.debug_clear()
	
	idb.ensure_table({"id":0}, "table" , [])
	idb.ensure_index({"name(32)":"myname"}, "table", false, [])
	var path = idb.open()
	print("db_path:", path)
  

func _process(delta):
	if (idb.is_opening()):
		test()
		idb.exit()
	
	translation.z += delta 


func test():	
	var size = idb.count("table")	
	print("count: ", size)
	
	var id = idb.new_id()
	print("new id:", id)
	
	var obj = {"id": id }
	obj.name = "myname_" + id as String
	obj.value = "myvalue_" + id as String
	
	print(idb.insert("table",obj))	
	print(idb.count("table id==?", [id]) == 1)
	
	var sel = idb.select("table name==?", ["myname_" + id as String])
	print(sel.size() == 1)
	
	sel = idb.select("table") 
	print(sel.size() == size+1)
	for i in sel :
		print(i.id, " ", i.name, "  " , i.value, " ")
	
	var o = idb.find("table",id)  
	print(o != obj)
	print(o.id == obj.id)
	print(o.name == obj.name)
	
	#nosql, adding a 'ex' field automatically
	o.ex = "ex_" + o.id as String 
	o.value = o.value + "_ex" 
	print(idb.update("table", o)) 
	
	var o2 = idb.find("table",id)  
	print(o2 != o)
	print(o2.id == o.id)
	print(o2.value == o.value)
	
	o2.ex2 = "ex2_" + o.id as String 
	print(idb.replace("table", o2)) 

	var o3 = idb.find("table",id) 
	print(o3 != o2)
	print(o3.ex2 == o2.ex2);
	print(o3);
	
#	print(idb.delete("table", id))	
#	var o4 = idb.find("table",id)
#	print(o4==null)
	 
	idb.save_web() 



