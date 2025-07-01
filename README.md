# FeatureFlags.Sample

A sample Azure Functions application demonstrating Feature Management with Microsoft Feature Flags, including percentage-based feature rollouts and user targeting.

## Overview

This project showcases how to implement feature flags in Azure Functions using:
- **Microsoft.FeatureManagement** for feature flag management
- **Percentage Filter** for gradual rollouts
- **Targeting Filter** for user-specific feature control
- **.NET 8** as the target framework

## Project Structure
FeatureFlags.Sample/
├── Functions/
│   └── BetaFeatureFunction.cs    # Main HTTP trigger function
├── appsettings.json              # Feature flag configuration
├── local.settings.json           # Local development settings
├── host.json                     # Azure Functions host configuration
├── Startup.cs                    # Dependency injection setup
└── GlobalUsings.cs               # Global using statements
## Local Development Setup

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Azure Functions Core Tools](https://docs.microsoft.com/azure/azure-functions/functions-run-local)
- [Azurite](https://docs.microsoft.com/azure/storage/common/storage-use-azurite) or Azure Storage Account

### Local Settings Configuration

The `local.settings.json` file contains essential configuration for local development:
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_INPROC_NET8_ENABLED": "1", 
        "FUNCTIONS_WORKER_RUNTIME": "dotnet"
    }
}
#### Configuration Explained

| Setting | Value | Description |
|---------|-------|-------------|
| `IsEncrypted` | `false` | Indicates if the settings values are encrypted (typically false for local development) |
| `AzureWebJobsStorage` | `UseDevelopmentStorage=true` | Uses Azurite storage emulator for local development. For production, use actual Azure Storage connection string |
| `FUNCTIONS_INPROC_NET8_ENABLED` | `1` | Enables .NET 8 in-process hosting model for Azure Functions |
| `FUNCTIONS_WORKER_RUNTIME` | `dotnet` | Specifies the language runtime for Azure Functions |

#### Additional Local Settings (Optional)

You can extend `local.settings.json` with additional settings as needed:
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_INPROC_NET8_ENABLED": "1",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet",
        "APPINSIGHTS_INSTRUMENTATIONKEY": "your-app-insights-key",
        "AZURE_CLIENT_ID": "your-client-id",
        "AZURE_CLIENT_SECRET": "your-client-secret"
    }
}
### Running Locally

1. **Start Azurite** (if using local storage emulator):azurite --silent --location c:\azurite --debug c:\azurite\debug.log
2. **Run the Functions app**:cd FeatureFlags.Sample
   func start
3. **Test the endpoint**:# Test without userId (generates random user)
curl http://localhost:7071/api/BetaFeatureFunction

# Test with specific userId
curl "http://localhost:7071/api/BetaFeatureFunction?userId=user1@example.com"
## Feature Configuration

Feature flags are configured in `appsettings.json`:
{
  "FeatureManagement": {
    "BetaFeature": {
      "EnabledFor": [
        {
          "Name": "Microsoft.Percentage",
          "Parameters": {
            "Value": 20
          }
        }
      ]
    }
  }
}
This configuration enables the `BetaFeature` for 20% of users based on a consistent hash of their user ID.

## API Endpoints

### GET /api/BetaFeatureFunction

Tests whether a user has access to the beta feature.

**Query Parameters:**
- `userId` (optional): User identifier for targeting. If not provided, a random GUID is generated.

**Response:**{
  "userId": "user1@example.com",
  "hasAccess": true,
  "bucket": 15,
  "message": "✅ User 'user1@example.com' has access to BetaFeature (bucket: 15)."
}
## Testing

The project includes comprehensive unit tests in `FeatureFlags.Sample.Test` that validate:
- Feature flag evaluation with and without user IDs
- Percentage distribution accuracy
- Bucket calculation consistency
- Various percentage scenarios (0%, 50%, 100%)

Run tests with:dotnet test
## Deployment

For deployment to Azure:

1. Update `local.settings.json` values as Azure Function App Configuration settings
2. Replace `UseDevelopmentStorage=true` with actual Azure Storage connection string
3. Configure Application Insights if needed
4. Deploy using Azure CLI, GitHub Actions, or Visual Studio

## Security Notes

- `local.settings.json` is automatically excluded from source control via `.gitignore`
- Never commit sensitive connection strings or secrets to version control
- Use Azure Key Vault for production secrets
- The `CopyToPublishDirectory` is set to `Never` for `local.settings.json` in the project file

## Dependencies

- **Microsoft.Azure.Functions.Extensions** - Dependency injection for Azure Functions
- **Microsoft.Azure.WebJobs.Extensions.OpenApi** - OpenAPI/Swagger support
- **Microsoft.FeatureManagement** - Feature flag management
- **Microsoft.NET.Sdk.Functions** - Azure Functions SDK for .NET

## Learn More

- [Azure Functions documentation](https://docs.microsoft.com/azure/azure-functions/)
- [Feature Management documentation](https://docs.microsoft.com/azure/azure-app-configuration/use-feature-flags-dotnet-core)
- [.NET 8 documentation](https://docs.microsoft.com/dotnet/core/whats-new/dotnet-8)
