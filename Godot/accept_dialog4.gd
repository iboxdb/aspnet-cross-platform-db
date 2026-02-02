extends AcceptDialog

var iboxdb4 = preload("res://iboxdb4.cs")
# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	print('begin')
	#iboxdb4.print([1,1.1,"1.1"])
	var db = iboxdb4.new()
	db.debug_clear()
	
	db.ensure_table({"id":0},"table",["id"]);
	db.ensure_index({"name":"any"},"table",false,["name"]);
	print(db.open())
	print(db.insert("table",{"id":db.new_id(),
		"name":"Bee", "val":100.1 }))
	var l = db.select("from table id>=?", [-1]);
	print(l)
	var doc = l[0]
	doc["val"] += 22.1
	print(db.update("table",doc))
	var doc2 = db.get("table",[doc["id"]]);
	print(doc2)
	print( db.count("from table id>=?", [-1]) )
	db.delete("table",[doc2["id"]])
	print( db.count("table",[]) )
	print(db.replace("table",{"id":db.new_id(),
		"name":"NoName", "val":1.1 }))
	print(db.select("table",[]))
	db.close_db()		
	db.debug_clear()
	print('end')


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass
