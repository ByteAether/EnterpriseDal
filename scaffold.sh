dotnet build DAL.ScaffoldInterceptor --configuration Release
dotnet tool restore
dotnet linq2db scaffold -i scaffold.json
