# Use the official .NET ASP.NET image as the base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Use the official .NET SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY VideoCrypt.Image.Main.csproj ./
RUN dotnet restore "VideoCrypt.Image.Main.csproj"

# Copy the entire directory contents
COPY . .
WORKDIR "/src"
RUN dotnet build "VideoCrypt.Image.Main.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VideoCrypt.Image.Main.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VideoCrypt.Image.Main.dll"]
