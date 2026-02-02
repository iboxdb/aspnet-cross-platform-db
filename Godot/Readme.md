## Godot Wrapper

[iBoxDB](https://www.iboxdb.com) is a Lightweight Embedded NoSQL Database, running in many platforms, easy to deploy.

This directory includes a GDScript Wrapper.

### Prepare

#### Add iBoxDB to Mono Project (.csproj)

```xml
  <ItemGroup>  
    <PackageReference Include="iBoxDB" Version="" /> 
  </ItemGroup>
```
or

```xml
  <ItemGroup>  
    <Reference Include="/localpath/NET2/iBoxDB.NET2.dll"  /> 
  </ItemGroup>
```

#### Add iBoxDB Wrap to Godot Project
One csharp script file only,  
[v3 download iboxdb.cs](iboxdb.cs)
[v4 download iboxdb4.cs](iboxdb4.cs)


#### Use preload(..).new() to import
[v3 Usage AMeshInstance.gd](AMeshInstance.gd)
[v4 Usage accept_dialog4.gd](accept_dialog4.gd)


##### Create Database
````
var idb = preload("res://iboxdb.cs").new()
````

##### Create Table
````
idb.ensure_table({"id":0}, "table" , [])
````

##### Insert Record
````
var id = idb.new_id()
var obj = {"id": id }
obj.name = "myname_" + id as String
obj.value = "myvalue_" + id as String
idb.insert("table",obj)	
````

##### Select Records
````
sel = idb.select("table") 
for i in sel :
  print(i.id, " ", i.name, "  " , i.value, " ")
````
