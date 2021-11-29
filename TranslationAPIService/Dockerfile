#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

# ---------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# ---------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

ARG CONFIGURATION=Release

WORKDIR /src
COPY ["TranslationAPIService.Tests/TranslationAPIService.Tests.csproj", "TranslationAPIService.Tests/"]
COPY ["TranslationAPIService/TranslationAPIService.csproj", "TranslationAPIService/"]
RUN dotnet restore "TranslationAPIService.Tests/TranslationAPIService.Tests.csproj"
RUN dotnet restore "TranslationAPIService/TranslationAPIService.csproj"
COPY . .
# build service
WORKDIR "/src/TranslationAPIService"
RUN dotnet build "TranslationAPIService.csproj" -c $CONFIGURATION -o /app/build
# build tests
WORKDIR "/src/TranslationAPIService.Tests"
RUN dotnet build "TranslationAPIService.Tests.csproj" -c $CONFIGURATION

# ---------------------------------------------------
FROM build AS tested-build

WORKDIR "/src/TranslationAPIService.Tests"
RUN dotnet test

# ---------------------------------------------------
FROM tested-build AS publish

ARG CONFIGURATION=Release

WORKDIR "/src/TranslationAPIService"
RUN dotnet publish "TranslationAPIService.csproj" -c $CONFIGURATION -o /app/publish

# ---------------------------------------------------
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tilde.MT.TranslationAPIService.dll"]