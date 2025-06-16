global using FeatureFlags.Sample.Functions;

namespace FeatureFlags.Sample.Test.FunctionTests;

public class BeatFeatureFunctionTest
{
    [Theory]
    [InlineData("alpha@demo.com", true)] // known email
    [InlineData("random@demo.com", false)] // not in list, percentage logic assumed false
    public async Task EmailBasedTargeting_Works(string email, bool expectedEnabled)
    {
        // Arrange
        var mockFeatureManager = new Mock<IFeatureManagerSnapshot>();
        var logger = new Mock<ILogger<BetaFeatureFunction>>();
        mockFeatureManager
            .Setup(x => x.IsEnabledAsync("BetaFeature", It.Is<TargetingContext>(c => c.UserId == email)))
            .ReturnsAsync(expectedEnabled);

        var function = new BetaFeatureFunction(mockFeatureManager.Object);

        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString($"?email={email}");

        // Act
        var result = await function.Run(context.Request, logger.Object) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        var body = result.Value.ToString();
        if (expectedEnabled)
            Assert.Contains("✅", body);
        else
            Assert.Contains("🚫", body);
    }
}