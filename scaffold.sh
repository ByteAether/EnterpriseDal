dotnet build DAL.ScaffoldInterceptor --configuration Release
dotnet tool restore --add-source ./nuget
dotnet linq2db scaffold -i scaffold.json
