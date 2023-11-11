#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["NeoCMS/NeoCMS/NeoCMS.csproj", "NeoCMS/NeoCMS/"]
COPY ["NeoCommon/NeoCommon.csproj", "NeoCommon/"]
COPY ["NeoCMS/NeoCMS.Client/NeoCMS.Client.csproj", "NeoCMS/NeoCMS.Client/"]
RUN dotnet restore "./NeoCMS/NeoCMS/./NeoCMS.csproj"
COPY . .
WORKDIR "/src/NeoCMS/NeoCMS"
RUN dotnet build "./NeoCMS.csproj" -c $BUILD_CONFIGURATION -o /app/build



FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./NeoCMS.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV MONGO_URI=mongodb://10.0.52.208,10.0.52.245,10.0.52.226/NeoAlexandria?replicaSet=repA
ENTRYPOINT ["dotnet", "NeoCMS.dll"]