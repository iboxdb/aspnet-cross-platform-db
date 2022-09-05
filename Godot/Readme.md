## Godot Wrap

### Prepare

#### Add iBoxDB to Mono Project (.csproj)

```xml
  <ItemGroup>  
    <PackageReference Include="iBoxDB" Version="3.5.0" /> 
  </ItemGroup>
```
or

```xml
  <ItemGroup>  
    <Reference Include="/localpath/NET2/iBoxDB.NET2.dll"  /> 
  </ItemGroup>
```

#### Add iBoxDB Wrap to Godot Project
only one csharp script file,  
[download iboxdb.cs](iboxdb.cs)

#### Use preload(..).new() to import
[Usage AMeshInstance.gd)](AMeshInstance.gd)


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
var id = idb.newId()
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
