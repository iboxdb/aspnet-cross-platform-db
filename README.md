## ASP.NET Cross Platform Database Test Project




### XSP4 ASP.NET
Used to check if [iBoxDB](http://www.iboxdb.com/) is supported on Windows and Linux,

And [NETCoreApp 1.1](https://github.com/iboxdb/aspnet-cross-platform-db/tree/master/netcoreapp11) ,
 [NETCoreApp](https://github.com/iboxdb/aspnet-cross-platform-db/tree/master/netcoreapp/hosting) ,
 [DNXCore](https://github.com/iboxdb/aspnet-cross-platform-db/tree/master/dnxcore/project.json) ,
 [DNXMono](https://github.com/iboxdb/aspnet-cross-platform-db/tree/master/dnxmono/project.json),

And More Code ...



#### Windows
```
    Download
    Copy /OriginVer/ to ASP.NET Server
```

#### Linux Mono

```
 sudo apt-get update
 sudo apt-get install mono-xsp4
 
 git clone https://github.com/iboxdb/aspnet-cross-platform-db.git 
 cd aspnet-cross-platform-db/OriginVer
 xsp4
```
 

### NETCoreApp 1.1

```
 git clone https://github.com/iboxdb/aspnet-cross-platform-db.git 
 cd aspnet-cross-platform-db/netcoreapp11
 dotnet restore 
 dotnet run
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

  
### IO
 **/IO/, Encrypted IO.**


### Retry IO
 **/RetryIO/, Example for Network IO.**


### WebAPI
 **/webapi/, Example for Exchanging data. **
 