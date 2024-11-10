# Stage 1: Build the Blazor UI
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS blazor-build
WORKDIR /src

# Copy and restore dependencies for Blazor
COPY UI/UI.csproj ./UI/
RUN dotnet restore ./UI/UI.csproj
COPY UI/ ./UI/
RUN dotnet publish ./UI/UI.csproj -c Release -o /src/UI/dist

# Stage 2: Build the .NET Core API
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS api-build
WORKDIR /src

# Copy and restore dependencies for the API
COPY API/API.csproj ./API/
RUN dotnet restore ./API/API.csproj
COPY API/ ./API/

# Publish the API to the /out directory
RUN dotnet publish ./API/API.csproj -c Release -o /out

# Stage 3: Runtime environment for the API
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the published .NET Core API and Blazor UI
COPY --from=api-build /out .
COPY --from=blazor-build /src/UI/dist ./wwwroot

EXPOSE 80

# Entry point for the backend API
ENTRYPOINT ["dotnet", "API.dll"]
