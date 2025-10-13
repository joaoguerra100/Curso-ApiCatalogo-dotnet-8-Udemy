------------Descrição---------------
Curso de dotnet 8 webapi de catalogos na udemy pelo jose carlos macoratti
curso: O essencial para criar APIs da Web na plataforma .NET

---------------Pacotes necessarios para instalação---------------------
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL

dotnet add package Microsoft.EntityFrameworkCore.Tools

dotnet add package Microsoft.EntityFrameworkCore.Design

dotnet tool install --global dotnet-ef // dotnet tool update --global dotnet-ef

dotnet ef migrations add Inicial
dotnet ef database update

dotnet add package AutoMapper
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection

dotnet add package Microsoft.AspNetCore.JsonPatch
dotnet add package Microsoft.AspNetCore.Mvc.NewtonsoftJson
dotnet add package Newtonsoft.Json

dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.2
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 8.0.5
dotnet add package Asp.versioning.Mvc.ApiExplorer
dotnet add package Asp.versioning.Http

dotnet add package  --source https://api.nuget.org/v3/index.json --version 8.0.5

------------Comandos Para Iniciar---------------
dotnet run
