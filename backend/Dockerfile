# Stage 1: Build .NET app
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /source

# Copy solution and project files
COPY HolidayPlanning.slnx .
COPY HolidayPlanning.WebApi/*.csproj ./HolidayPlanning.WebApi/
COPY HolidayPlanning.Abstractions/*.csproj ./HolidayPlanning.Abstractions/
COPY HolidayPlanning.DataModels/*.csproj ./HolidayPlanning.DataModels/
COPY HolidayPlanning.DomainModels/*.csproj ./HolidayPlanning.DomainModels/
COPY HolidayPlanning.Services/*.csproj ./HolidayPlanning.Services/
COPY HolidayPlanning.EntityModels/*.csproj ./HolidayPlanning.EntityModels/ 
COPY HolidayPlanning.Database/*.csproj ./HolidayPlanning.Database/
# Restore dependencies
RUN dotnet restore

# Copy all source files
COPY . .

# Build and publish
RUN dotnet publish HolidayPlanning.WebApi/HolidayPlanning.WebApi.csproj -c Release -o /app/publish

# Stage 2: Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine
WORKDIR /app
COPY --from=build /app/publish .

# Expose port (default ASP.NET Core port is 8080 in .NET 8+)
EXPOSE 8080

ENTRYPOINT ["dotnet", "HolidayPlanning.WebApi.dll"]
