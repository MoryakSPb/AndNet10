#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Server/AndNet.Manager.Server.csproj", "src/Server/"]
COPY ["src/Client/AndNet.Manager.Client.csproj", "src/Client/"]
COPY ["src/Shared/AndNet.Manager.Shared.csproj", "src/Shared/"]
COPY ["src/Database/AndNet.Manager.Database.csproj", "src/Database/"]
COPY ["src/Integration/AndNet.Integration.Discord/AndNet.Integration.Discord.csproj", "src/Integration/AndNet.Integration.Discord/"]
COPY ["src/Integration/AndNet.Integration.Steam/AndNet.Integration.Steam.csproj", "src/Integration/AndNet.Integration.Steam/"]
RUN dotnet restore "src/Server/AndNet.Manager.Server.csproj"
COPY . .
WORKDIR "/src/src/Server"
RUN dotnet build "AndNet.Manager.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AndNet.Manager.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AndNet.Manager.Server.dll"]