#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

ARG CONFIGURATION=Release

WORKDIR /src
COPY ["TranslationAPIService/TranslationAPIService.csproj", "TranslationAPIService/"]
RUN dotnet restore "TranslationAPIService/TranslationAPIService.csproj"
COPY . .
WORKDIR "/src/TranslationAPIService"
RUN dotnet build "TranslationAPIService.csproj" -c $CONFIGURATION -o /app/build

FROM build AS publish

ARG CONFIGURATION=Release

RUN dotnet publish "TranslationAPIService.csproj" -c $CONFIGURATION -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tilde.MT.TranslationAPIService.dll"]