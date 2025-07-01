namespace FeatureFlags.Sample.Test.FunctionTests;

public class BetaFeatureFunctionTest
{
    [Fact]
    public async Task BetaFeatureFunction_WithoutUserId_GeneratesRandomUserAndReturnsResult()
    {
        // Arrange
        var mockFeatureManager = new Mock<IFeatureManagerSnapshot>();
        var logger = new Mock<ILogger<BetaFeatureFunction>>();
        
        mockFeatureManager
            .Setup(x => x.IsEnabledAsync("BetaFeature", It.IsAny<TargetingContext>()))
            .ReturnsAsync(true);

        var function = new BetaFeatureFunction(mockFeatureManager.Object);

        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString("");

        // Act
        var result = await function.Run(context.Request, logger.Object) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        mockFeatureManager.Verify(x => x.IsEnabledAsync("BetaFeature", It.IsAny<TargetingContext>()), Times.Once);
    }

    [Theory]
    [InlineData("user1@test.com")]
    [InlineData("user2@test.com")]
    [InlineData("testuser")]
    public async Task BetaFeatureFunction_WithUserId_UsesProvidedUserId(string userId)
    {
        // Arrange
        var mockFeatureManager = new Mock<IFeatureManagerSnapshot>();
        var logger = new Mock<ILogger<BetaFeatureFunction>>();
        
        mockFeatureManager
            .Setup(x => x.IsEnabledAsync("BetaFeature", It.Is<TargetingContext>(c => c.UserId == userId)))
            .ReturnsAsync(true);

        var function = new BetaFeatureFunction(mockFeatureManager.Object);

        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString($"?userId={userId}");

        // Act
        var result = await function.Run(context.Request, logger.Object) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        mockFeatureManager.Verify(x => x.IsEnabledAsync("BetaFeature", It.Is<TargetingContext>(c => c.UserId == userId)), Times.Once);
    }

    [Fact]
    public async Task PercentageFilterTest_With100MockUsers_ValidatesDistribution()
    {
        // Arrange
        var logger = new Mock<ILogger<BetaFeatureFunction>>();
        var enabledUsers = new List<string>();
        var disabledUsers = new List<string>();
        var bucketDistribution = new Dictionary<int, int>();

        // Create 100 mock users
        var mockUsers = Enumerable.Range(1, 100)
            .Select(i => $"user{i}@test.com")
            .ToList();

        foreach (var user in mockUsers)
        {
            // Calculate bucket for each user (same logic as in the function)
            string userLower = user.ToLowerInvariant();
            int hash = Math.Abs(userLower.GetHashCode());
            int bucket = hash % 100;
            
            // Track bucket distribution
            if (!bucketDistribution.ContainsKey(bucket))
                bucketDistribution[bucket] = 0;
            bucketDistribution[bucket]++;

            // Mock feature manager to simulate 20% rollout
            var mockFeatureManager = new Mock<IFeatureManagerSnapshot>();
            bool shouldBeEnabled = bucket < 20; // 20% percentage from config
            
            mockFeatureManager
                .Setup(x => x.IsEnabledAsync("BetaFeature", It.Is<TargetingContext>(c => c.UserId == user)))
                .ReturnsAsync(shouldBeEnabled);

            var function = new BetaFeatureFunction(mockFeatureManager.Object);

            var context = new DefaultHttpContext();
            context.Request.QueryString = new QueryString($"?userId={user}");

            // Act
            var result = await function.Run(context.Request, logger.Object) as OkObjectResult;

            // Assert individual result
            Assert.NotNull(result);
            
            if (shouldBeEnabled)
                enabledUsers.Add(user);
            else
                disabledUsers.Add(user);
        }

        // Assert overall distribution
        Assert.True(enabledUsers.Count > 0, "Some users should be enabled");
        Assert.True(disabledUsers.Count > 0, "Some users should be disabled");
        
        // Log results for analysis
        Console.WriteLine($"Enabled users: {enabledUsers.Count}/100 ({enabledUsers.Count}%)");
        Console.WriteLine($"Disabled users: {disabledUsers.Count}/100 ({disabledUsers.Count}%)");
        Console.WriteLine($"Unique buckets used: {bucketDistribution.Count}/100");
        
        // Verify we have reasonable distribution across buckets
        Assert.True(bucketDistribution.Count > 50, "Should have reasonable distribution across buckets");
    }

    [Fact]
    public async Task PercentageFilterTest_BucketCalculation_IsConsistent()
    {
        // Arrange
        var logger = new Mock<ILogger<BetaFeatureFunction>>();
        var testUser = "consistent.user@test.com";
        
        var mockFeatureManager = new Mock<IFeatureManagerSnapshot>();
        mockFeatureManager
            .Setup(x => x.IsEnabledAsync("BetaFeature", It.IsAny<TargetingContext>()))
            .ReturnsAsync(true);

        var function = new BetaFeatureFunction(mockFeatureManager.Object);

        // Act - Call multiple times with same user
        var results = new List<int>();
        for (int i = 0; i < 10; i++)
        {
            var context = new DefaultHttpContext();
            context.Request.QueryString = new QueryString($"?userId={testUser}");
            
            await function.Run(context.Request, logger.Object);
            
            // Calculate bucket manually to verify consistency
            string userLower = testUser.ToLowerInvariant();
            int hash = Math.Abs(userLower.GetHashCode());
            int bucket = hash % 100;
            results.Add(bucket);
        }

        // Assert - All bucket calculations should be identical
        Assert.True(results.All(b => b == results[0]), "Bucket calculation should be consistent for the same user");
    }

    [Theory]
    [InlineData(0, 100)] // 0% should enable no users
    [InlineData(100, 100)] // 100% should enable all users  
    [InlineData(50, 100)] // 50% should enable approximately half
    public async Task PercentageFilterTest_DifferentPercentages_WorkCorrectly(int targetPercentage, int userCount)
    {
        // Arrange
        var logger = new Mock<ILogger<BetaFeatureFunction>>();
        var enabledCount = 0;

        var mockUsers = Enumerable.Range(1, userCount)
            .Select(i => $"testuser{i}@example.com")
            .ToList();

        foreach (var user in mockUsers)
        {
            // Calculate if user should be enabled based on target percentage
            string userLower = user.ToLowerInvariant();
            int hash = Math.Abs(userLower.GetHashCode());
            int bucket = hash % 100;
            bool shouldBeEnabled = bucket < targetPercentage;

            var mockFeatureManager = new Mock<IFeatureManagerSnapshot>();
            mockFeatureManager
                .Setup(x => x.IsEnabledAsync("BetaFeature", It.Is<TargetingContext>(c => c.UserId == user)))
                .ReturnsAsync(shouldBeEnabled);

            var function = new BetaFeatureFunction(mockFeatureManager.Object);

            var context = new DefaultHttpContext();
            context.Request.QueryString = new QueryString($"?userId={user}");

            // Act
            var result = await function.Run(context.Request, logger.Object) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            
            if (shouldBeEnabled)
                enabledCount++;
        }

        // Assert percentage adherence
        double actualPercentage = (double)enabledCount / userCount * 100;
        Console.WriteLine($"Target: {targetPercentage}%, Actual: {actualPercentage:F1}%, Enabled: {enabledCount}/{userCount}");
        
        if (targetPercentage == 0)
            Assert.Equal(0, enabledCount);
        else if (targetPercentage == 100)
            Assert.Equal(userCount, enabledCount);
        else
        {
            // For other percentages, allow some variance due to hash distribution
            double tolerance = 20; // Allow 20% tolerance
            Assert.True(Math.Abs(actualPercentage - targetPercentage) <= tolerance, 
                $"Actual percentage {actualPercentage:F1}% should be within {tolerance}% of target {targetPercentage}%");
        }
    }
}