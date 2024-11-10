# Stage 1: Build the Blazor UI
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS blazor-build
WORKDIR /app

# Copy the Blazor project file and restore dependencies
COPY UI/UI.csproj ./UI/
RUN dotnet restore ./UI/UI.csproj

# Copy the entire UI directory and publish it to a known directory
COPY UI/ ./UI/
RUN dotnet publish ./UI/UI.csproj -c Release -o /app/publish

# List the files in the output directory to confirm the structure
RUN ls -R /app/publish  # Debugging output structure

# Stage 2: Build the .NET Core API
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS api-build
WORKDIR /src/api

COPY API/API.csproj ./API/
RUN dotnet restore ./API/API.csproj
COPY API/ ./API/

RUN dotnet publish ./API/API.csproj -c Release -o /src/api/out
RUN ls -la /src/api/out

# Stage 3: Runtime environment
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the API and Blazor UI published output to the final image
COPY --from=api-build /src/api/out ./  # Copy API output

# Make sure this points to the right location based on the publish structure:
COPY --from=blazor-build /app/publish/wwwroot /wwwroot  # Copy Blazor UI wwwroot output

EXPOSE 80
ENTRYPOINT ["dotnet", "API.dll"]
