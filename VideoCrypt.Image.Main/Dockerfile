# Use the official .NET ASP.NET image as the base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

USER app
WORKDIR /app
EXPOSE 8080

# Use the official .NET SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["VideoCrypt.Image.Main.csproj", "VideoCrypt.Image.Main/"]
RUN dotnet restore "VideoCrypt.Image.Main.csproj"

COPY . .
WORKDIR "/src/VideoCrypt.Image.Main"
RUN dotnet dev-certs https
RUN dotnet build "VideoCrypt.Image.Main.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "VideoCrypt.Image.Main.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "VideoCrypt.Image.Main.dll"]
