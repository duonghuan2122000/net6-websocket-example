#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
RUN apt-get update && apt-get install -y apt-utils libgdiplus libc6-dev
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["TestSignalR/TestSignalR.csproj", "TestSignalR/"]
RUN dotnet restore "TestSignalR/TestSignalR.csproj"
COPY . .
WORKDIR "/src/TestSignalR"
RUN dotnet build "TestSignalR.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TestSignalR.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TestSignalR.dll"]