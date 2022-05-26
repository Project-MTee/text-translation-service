# ---------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# ---------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

ARG CONFIGURATION=Release

WORKDIR /src

COPY ["TranslationAPIService/TranslationAPIService.csproj", "TranslationAPIService/"]
RUN dotnet restore "TranslationAPIService/TranslationAPIService.csproj"
COPY TranslationAPIService TranslationAPIService
# build service
WORKDIR "/src/TranslationAPIService"
RUN dotnet build "TranslationAPIService.csproj" -c $CONFIGURATION -o /app/build

# ---------------------------------------------------
FROM build AS tested-build

ARG CONFIGURATION=Release

WORKDIR /src
COPY ["TranslationAPIService.Tests/TranslationAPIService.Tests.csproj", "TranslationAPIService.Tests/"]
RUN dotnet restore "TranslationAPIService.Tests/TranslationAPIService.Tests.csproj"
COPY TranslationAPIService.Tests TranslationAPIService.Tests
WORKDIR "/src/TranslationAPIService.Tests"
RUN dotnet test -c $CONFIGURATION

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