# Use the .NET SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Copy the source code into the container
COPY . /src/dotnet-function-app

# Build the project
RUN cd /src/dotnet-function-app && \
    mkdir -p /home/site/wwwroot && \
    dotnet publish *.csproj --output /home/site/wwwroot

# Use the Azure Functions base image
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0

# Set environment variables
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

# Copy the build output from the build stage
COPY --from=build /home/site/wwwroot /home/site/wwwroot

# Set the command to run the Azure Functions host
CMD ["func", "host", "start", "--verbose"]
