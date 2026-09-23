FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY POSApp.Data/POSApp.Data.csproj POSApp.Data/
COPY POSApp.Web/POSApp.Web.csproj POSApp.Web/
RUN dotnet restore POSApp.Web/POSApp.Web.csproj

COPY POSApp.Data/ POSApp.Data/
COPY POSApp.Web/ POSApp.Web/
RUN dotnet publish POSApp.Web/POSApp.Web.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_EnableDiagnostics=0

EXPOSE 5000

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:5000/ || exit 1

ENTRYPOINT ["dotnet", "POSApp.Web.dll"]
