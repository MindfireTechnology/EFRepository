

dotnet tool install --global dotnet-ef
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet ef dbcontext scaffold "Server=XXX;User Id=XXX;Password=XXX;Database=XXX" Microsoft.EntityFrameworkCore.SqlServer --context-dir Data --output-dir Data --data-annotations