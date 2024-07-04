# Use the official .NET ASP.NET image as the base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:7003

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY VideoCrypt.Image.Api/VideoCrypt.Image.Api.csproj VideoCrypt.Image.Api/
RUN dotnet restore "VideoCrypt.Image.Api/VideoCrypt.Image.Api.csproj"

COPY VideoCrypt.Image.Api/ VideoCrypt.Image.Api/
WORKDIR "/src/VideoCrypt.Image.Api"
RUN dotnet build "VideoCrypt.Image.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VideoCrypt.Image.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VideoCrypt.Image.Api.dll"]
