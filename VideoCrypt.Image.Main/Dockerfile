# Use the official .NET SDK image as the base build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the project file and restore dependencies
COPY VideoCrypt.Image.Main.csproj ./
RUN dotnet restore

# Copy the remaining source code
COPY . ./

# Build the project
RUN dotnet build -c Release -o /app/build

# Final image stage for execution
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/build .

# Set the entry point for the application
ENTRYPOINT ["dotnet", "VideoCrypt.Image.Main.dll"]
