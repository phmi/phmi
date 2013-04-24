rmdir /s /q "bin\PHmi"
del BuildPHmi.log
"%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" PHmi.sln /t:ReBuild /m /p:Configuration=Release >> BuildPHmi.log
start BuildPHmi.log