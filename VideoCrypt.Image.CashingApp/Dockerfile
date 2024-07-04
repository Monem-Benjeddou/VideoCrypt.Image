# Use the official .NET ASP.NET image as the base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:4000

RUN mkdir -p /path/to/shared/cache/directory

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY VideoCrypt.Image.CashingApp/VideoCrypt.Image.CashingApp.csproj VideoCrypt.Image.CashingApp/
RUN dotnet restore "VideoCrypt.Image.CashingApp/VideoCrypt.Image.CashingApp.csproj"

COPY VideoCrypt.Image.CashingApp/ VideoCrypt.Image.CashingApp/
WORKDIR "/src/VideoCrypt.Image.CashingApp"
RUN dotnet build "VideoCrypt.Image.CashingApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VideoCrypt.Image.CashingApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VideoCrypt.Image.CashingApp.dll"]
