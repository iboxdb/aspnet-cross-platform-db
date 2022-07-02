## ASP.NET Cross Platform Database Test Project

Used to check if [iBoxDB](http://www.iboxdb.com/) is supported on Windows and Linux,

[Blazor Wasm Client App](https://github.com/iboxdb/aspnet-cross-platform-db/blob/master/blazorwasm/IApp.cs)

[Encrypted IO](https://github.com/iboxdb/aspnet-cross-platform-db/blob/master/IO/EncryptDatabaseConfig.cs)

[iBoxDB ORM eXpress Persistent Objects XPO](https://sourceforge.net/p/datastorexpo/code/)

[WebAPI & SSL Linux](https://github.com/iboxdb/aspnet-cross-platform-db/tree/master/webapi)


### Add assembly to project
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>netcoreapp3.1</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <Reference Include="\home\user\Downloads\iBoxDB\NETDB\iBoxDB.dll">
    </Reference> 

    <!--  
    <Reference Include="\home\user\Downloads\iBoxDB\NETDB\NET2\iBoxDB.NET2.dll">
    </Reference> 
    -->

  </ItemGroup>

</Project>
```




## History...

 [RetryIO for Network IO](https://github.com/iboxdb/aspnet-cross-platform-db/blob/master/RetryIO/RetryStreamConfig.cs)

 [NETCoreApp 1.1](https://github.com/iboxdb/aspnet-cross-platform-db/tree/master/netcoreapp11) ,
 [NETCoreApp](https://github.com/iboxdb/aspnet-cross-platform-db/tree/master/netcoreapp/hosting) ,
 [DNXCore](https://github.com/iboxdb/aspnet-cross-platform-db/tree/master/dnxcore/project.json) ,
 [DNXMono](https://github.com/iboxdb/aspnet-cross-platform-db/tree/master/dnxmono/project.json)
 

### ASP.NET WebForm

#### Windows WebForm
```
    Download
    Copy /OriginVer/ to ASP.NET Server
```

#### Linux Mono WebForm

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

  
