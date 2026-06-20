# Stage 1: Build .NET app
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /source

# Copy solution and project files
COPY Balenthiran.slnx .
COPY Balenthiran.WebApi/*.csproj ./Balenthiran.WebApi/
COPY Balenthiran.Abstractions/*.csproj ./Balenthiran.Abstractions/
COPY Balenthiran.DataModels/*.csproj ./Balenthiran.DataModels/
COPY Balenthiran.DomainModels/*.csproj ./Balenthiran.DomainModels/
COPY Balenthiran.Services/*.csproj ./Balenthiran.Services/
COPY Balenthiran.EntityModels/*.csproj ./Balenthiran.EntityModels/ 
COPY Balenthiran.Database/*.csproj ./Balenthiran.Database/
# Restore dependencies
RUN dotnet restore

# Copy all source files
COPY . .

# Build and publish
RUN dotnet publish Balenthiran.WebApi/Balenthiran.WebApi.csproj -c Release -o /app/publish

# Stage 2: Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine
WORKDIR /app
COPY --from=build /app/publish .

# Expose port (default ASP.NET Core port is 8080 in .NET 8+)
EXPOSE 8080

ENTRYPOINT ["dotnet", "Balenthiran.WebApi.dll"]
