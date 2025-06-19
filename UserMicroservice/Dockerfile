# Use the official ASP.NET Core runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["UserMicroservice.csproj", "./"]
RUN dotnet restore "UserMicroservice.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "UserMicroservice.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UserMicroservice.csproj" -c Release -o /app/publish

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "UserMicroservice.dll"]