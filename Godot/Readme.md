### Godot Wrap

#### Prepare

##### Add iBoxDB to Mono Project (.csproj)

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

##### Add iBoxDB Wrap to Godot Project
[iboxdb.cs](iboxdb.cs)

##### Use preload(..).new() to import
[usage](AMeshInstance.gd)
