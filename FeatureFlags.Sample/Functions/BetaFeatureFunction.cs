namespace FeatureFlags.Sample.Functions;

public class BetaFeatureFunction(IFeatureManagerSnapshot featureManager)
{
    private readonly IFeatureManagerSnapshot _featureManager = featureManager;

    [FunctionName("BetaFeatureFunction")]
    [OpenApiOperation(operationId: "Run", tags: new[] { "feature" })]
    [OpenApiParameter(name: "email", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "Email of the user")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(string))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
        ILogger log)
    {
        string email = req.Query["email"];
        if (string.IsNullOrWhiteSpace(email))
        {
            return new BadRequestObjectResult("Please provide 'email' as query parameter.");
        }

        // Optional logging for Microsoft.Percentage bucket analysis
        string userId = email.ToLowerInvariant();
        int hash = Math.Abs(userId.GetHashCode());
        int bucket = hash % 100;
        log.LogInformation($"🧮 User '{userId}' falls into Microsoft.Percentage bucket: {bucket}");

        var context = new TargetingContext { UserId = email };
        bool isEnabled = await _featureManager.IsEnabledAsync("BetaFeature", context);

        var message = isEnabled
            ? $"✅ {email} has access to BetaFeature."
            : $"🚫 {email} does NOT have access to BetaFeature.";

        return new OkObjectResult(message);
    }
}


