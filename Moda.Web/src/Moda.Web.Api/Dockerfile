FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 5150

ENV ASPNETCORE_URLS=http://+:5150

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
# RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
# USER appuser

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Moda.Web/src/Moda.Web.Api/Moda.Web.Api.csproj", "Moda.Web/src/Moda.Web.Api/"]
RUN dotnet restore "Moda.Web/src/Moda.Web.Api/Moda.Web.Api.csproj"
COPY . .
WORKDIR "/src/Moda.Web/src/Moda.Web.Api"
RUN dotnet build "Moda.Web.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Moda.Web.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Moda.Web.Api.dll"]
