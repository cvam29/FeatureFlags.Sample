🧪 Feature Flags Sample (.NET 8 Azure Functions)  
This project showcases an Azure Function (In-Process) built with .NET 8, implementing feature flag management using the Microsoft.FeatureManagement library. It includes OpenAPI/Swagger support for easy API exploration and a clear, testable structure.  

📂 Project Structure  
FeatureFlags.Sample/  
│  
├── Configuration/  
│   └── FeatureFlagSettings.cs      # (Optional) Strongly-typed feature flag settings  
│  
├── FeatureFlags/  
│   └── FeatureNames.cs            # Constants for feature flag names (e.g., "BetaFeature")  
│  
├── Functions/  
│   └── BetaFeatureFunction.cs     # HTTP-triggered Azure Function with feature flag logic  
│  
├── Services/  
│   └── UserContextService.cs      # Helper to build TargetingContext from request  
│  
├── Startup.cs                     # Configures Feature Management and OpenAPI  
├── appsettings.json               # Feature flag configuration  
├── host.json                      # Azure Functions host settings  
├── local.settings.json            # Local environment settings  
├── FeatureFlags.Sample.csproj     # Project file  
│  
FeatureFlags.Sample.Test/  
│  
├── FunctionTests/  
│   └── BetaFeatureFunctionTest.cs # xUnit tests for feature rollouts  
│  
├── Mocks/  
│   └── FeatureManagerMock.cs      # (Optional) Reusable mocks for testing  
│  
├── FeatureFlags.Sample.Test.csproj # Test project file  

🚀 Features  

- **HTTP-triggered Azure Function** with feature flag checks.  
- **Feature Management** using Microsoft.FeatureManagement.  
- **Microsoft.Percentage**: Enables the "BetaFeature" for 20% of users (based on email hash).  
- **Microsoft.Targeting**: Enables the feature for specific email addresses (e.g., `alpha@demo.com`).  
- **Diagnostic Logging**: Logs the percentage bucket for each user to debug rollout behavior.  
- **OpenAPI/Swagger**: API documentation at `/api/swagger/ui`.  
- **xUnit Tests**: Validates feature access for targeted emails and percentage-based rollouts.  

⚙️ Feature Flag Configuration  
The feature flag settings are defined in `appsettings.json`:
- **Microsoft.Percentage**: Randomly enables the `BetaFeature` for 20% of users based on their email's hash.  
- **Microsoft.Targeting**: Always enables the `BetaFeature` for specified email addresses.  

🔍 Diagnostic Logging  
The function logs the percentage bucket for each user to help debug why a user does or doesn't get the feature:
If `bucket < 20`, the user qualifies for the 20% rollout (unless they're in the targeted email list).  

🛠️ How to Run  

1. **Clone the repository**:
2. **Install dependencies**:  
   Ensure you have .NET 8 SDK installed, then restore packages:
3. **Run locally**:  
   Use Visual Studio (F5) or the CLI:
4. **Access the API**:  
   - **Swagger UI**: Open `http://localhost:7125/api/swagger/ui` in your browser.  
   - **Test Endpoint**: Call the function with an email query parameter:  
     ```bash  
     curl "http://localhost:7125/api/BetaFeatureFunction?email=test@example.com"  
     ```  

   Example responses:  
   - ✅ `test@example.com` has access to BetaFeature. (if enabled)  
   - 🚫 `test@example.com` does NOT have access to BetaFeature. (if disabled)  

5. **Check Logs**:  
   View logs in the console or Application Insights to see percentage bucket assignments:
🧪 How to Test  

1. **Run xUnit Tests**:  
   Navigate to the solution directory and execute:
2. **What the Tests Cover**:  
   - Verifies the function returns correct responses when the feature is enabled for targeted emails (e.g., `alpha@demo.com`).  
   - Verifies the function returns correct responses when the feature is disabled for non-targeted emails.  
   - Ensures proper handling of invalid requests (missing email parameter).  

3. **Test File**:  
   `FeatureFlags.Sample.Test/FunctionTests/BetaFeatureFunctionTest.cs`  

📦 Dependencies  

### Function Project  
- `Microsoft.FeatureManagement`  
- `Microsoft.FeatureManagement.FeatureFilters`  
- `Microsoft.Azure.Functions.Extensions`  
- `Microsoft.Azure.WebJobs.Extensions.OpenApi`  

### Test Project  
- `xunit`  
- `Moq`  
- `Microsoft.NET.Test.Sdk`  
- `Microsoft.Extensions.DependencyInjection`  

Install via:
🧑‍💻 Development Notes  

- **Feature Names**: Use `FeatureFlags/FeatureNames.cs` to define constants for feature flags to avoid typos:
- **Extending Filters**: If deterministic testing of percentage rollouts is needed, add a custom filter in `Filters/`.  
- **Configuration**: Use `Configuration/FeatureFlagSettings.cs` for strongly-typed access to feature settings.  
- **Services**: `Services/UserContextService.cs` can be extended for more complex user context logic.  

📖 Additional Resources  

- [Microsoft Feature Management Docs](https://learn.microsoft.com/en-us/azure/azure-app-configuration/use-feature-flags)  
- [Azure Functions In-Process Model](https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide)  
- [OpenAPI in Azure Functions](https://learn.microsoft.com/en-us/azure/azure-functions/functions-openapi-definition)
