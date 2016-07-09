src\.nuget\nuget.exe pack Impromptu.csproj -IncludeReferencedProjects -Symbols -OutputDirectory src\core\Impromptu\bin\Release\ -Properties Configuration=Release -Verbosity detailed
src\.nuget\nuget.exe push src\core\Impromptu\bin\Release\impromptu.1.1.0.0.nupkg -Source https://www.nuget.org/api/v2/package
