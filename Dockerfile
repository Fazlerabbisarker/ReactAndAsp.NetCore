# Use ASP.NET Core 9 runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

# Use .NET SDK to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["MyApiApp.csproj", "./"]
RUN dotnet restore "./MyApiApp.csproj"

# Copy everything else and build
COPY . .
RUN dotnet publish "MyApiApp.csproj" -c Release -o /app/publish

# Final stage: runtime
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MyApiApp.dll"]
