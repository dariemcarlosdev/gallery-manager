FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/GalleryManager.Api/GalleryManager.Api.csproj src/GalleryManager.Api/
RUN dotnet restore src/GalleryManager.Api/GalleryManager.Api.csproj
COPY src/GalleryManager.Api/ src/GalleryManager.Api/
RUN dotnet publish src/GalleryManager.Api/GalleryManager.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "GalleryManager.Api.dll"]
