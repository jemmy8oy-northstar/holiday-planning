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
COPY HolidayPlanning.Tests/*.csproj ./HolidayPlanning.Tests/
# Restore dependencies
# NOTE: `dotnet restore` with no argument restores HolidayPlanning.slnx, so EVERY project
# listed there must have its .csproj copied above — including test projects, which are
# not published. Adding a project to the solution without a COPY line here breaks the
# image build only, and only on a push to main, long after CI has gone green.
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
