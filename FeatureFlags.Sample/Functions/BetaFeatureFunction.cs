namespace FeatureFlags.Sample.Functions;

public class BetaFeatureFunction(IFeatureManagerSnapshot featureManager)
{
    private readonly IFeatureManagerSnapshot _featureManager = featureManager;

    [FunctionName("BetaFeatureFunction")]
    [OpenApiOperation(operationId: "Run", tags: new[] { "feature" })]
    [OpenApiParameter(name: "userId", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "User ID for targeting (optional)")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(string))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
        ILogger log)
    {
        string userId = req.Query["userId"];
        
        // Create targeting context - if no userId provided, use a random one for percentage calculation
        var context = new TargetingContext 
        { 
            UserId = !string.IsNullOrWhiteSpace(userId) ? userId : Guid.NewGuid().ToString()
        };

        // Optional logging for Microsoft.Percentage bucket analysis
        string userForLogging = context.UserId.ToLowerInvariant();
        int hash = Math.Abs(userForLogging.GetHashCode());
        int bucket = hash % 100;
        log.LogInformation($"🧮 User '{userForLogging}' falls into Microsoft.Percentage bucket: {bucket}");

        bool isEnabled = await _featureManager.IsEnabledAsync("BetaFeature", context);

        var message = isEnabled
            ? $"✅ User '{context.UserId}' has access to BetaFeature (bucket: {bucket})."
            : $"🚫 User '{context.UserId}' does NOT have access to BetaFeature (bucket: {bucket}).";

        return new OkObjectResult(new { 
            userId = context.UserId, 
            hasAccess = isEnabled,
            bucket,
            message
        });
    }
}


