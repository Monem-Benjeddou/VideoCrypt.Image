FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY VideoCrypt.Image.Main.csproj ./
RUN dotnet restore "VideoCrypt.Image.Main.csproj"

COPY . .
WORKDIR "/src"
RUN dotnet build "VideoCrypt.Image.Main.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VideoCrypt.Image.Main.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VideoCrypt.Image.Main.dll"]
