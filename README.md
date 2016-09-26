## ASP.NET Cross Platform Database Test Project
Used to check if the components([iBoxDB](http://www.iboxdb.com/), [Nancy](http://nancyfx.org/),  [Bootstrap](http://getbootstrap.com/)) are supported on Windows and Linux,

And [DNXCore](https://github.com/iboxdb/aspnet-cross-platform-db/tree/master/dnxcore/project.json) ,
[DNXMono](https://github.com/iboxdb/aspnet-cross-platform-db/tree/master/dnxmono/project.json) .

### Windows
    Download
    Copy to ASP.NET Server
    
### Linux

```
 sudo apt-get update
 sudo apt-get install mono-xsp4
 
 git clone https://github.com/iboxdb/aspnet-cross-platform-db.git 
 cd aspnet-cross-platform-db
 xsp4
```

### NETCoreApp

```
 git clone https://github.com/iboxdb/aspnet-cross-platform-db.git 
 cd aspnet-cross-platform-db/netcoreapp/hosting
 dotnet restore 
 dotnet run
```

### DNX Core

```
 git clone https://github.com/iboxdb/aspnet-cross-platform-db.git 
 cd aspnet-cross-platform-db/dnxcore
 dotnet restore
 //ignore System.* dependencies
 dotnet run
```

### DNX Mono

```
 git clone https://github.com/iboxdb/aspnet-cross-platform-db.git 
 cd aspnet-cross-platform-db/dnxmono
 export DOTNET_REFERENCE_ASSEMBLIES_PATH="/usr/lib/mono/xbuild-frameworks"
 dotnet restore 
 dotnet run
```

### Exe Mono

```
//copy dnxcore to Home
mcs  /r:/usr/lib/mono/4.5/Facades/System.Runtime.dll /r:/usr/lib/mono/4.5/Facades/System.IO.dll /r:../.dnx/packages/iBoxDB.DNX/2.6.2.16/lib/iBoxDB.DNX.dll  Program.cs iBoxDB26.cs
export MONO_PATH=../.dnx/packages/iBoxDB.DNX/2.6.2.16/lib
mono Program.exe
```

### Open Browser
![https://github.com/iboxdb/aspnet-cross-platform-db/raw/master/Content/pic.png](https://github.com/iboxdb/aspnet-cross-platform-db/raw/master/Content/pic.png "ASP.NET Cross Platform NoSQL Database iBoxDB")  


