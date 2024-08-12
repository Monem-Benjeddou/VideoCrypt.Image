# Use the official .NET ASP.NET image as the base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Use the official .NET SDK image for the build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY VideoCrypt.Image.CashingApp/VideoCrypt.Image.CashingApp.csproj VideoCrypt.Image.CashingApp/
COPY ../VideoCrypt.Image.Data/VideoCrypt.Image.Data.csproj VideoCrypt.Image.Data/
RUN dotnet restore "VideoCrypt.Image.CashingApp/VideoCrypt.Image.CashingApp.csproj"

COPY VideoCrypt.Image.CashingApp/ VideoCrypt.Image.CashingApp/
COPY ../VideoCrypt.Image.Data/ VideoCrypt.Image.Data/
WORKDIR "/src/VideoCrypt.Image.CashingApp"
RUN dotnet build "VideoCrypt.Image.CashingApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VideoCrypt.Image.CashingApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY VideoCrypt.Image.CashingApp/clear_cache.sh /app/clear_cache.sh
RUN chmod +x /app/clear_cache.sh
RUN mkdir -p /cache
ENTRYPOINT ["dotnet", "VideoCrypt.Image.CashingApp.dll"]
