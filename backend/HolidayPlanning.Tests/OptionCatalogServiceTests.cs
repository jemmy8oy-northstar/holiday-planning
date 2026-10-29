using HolidayPlanning.Services;
using Xunit;

namespace HolidayPlanning.Tests;

public class OptionCatalogServiceTests
{
    [Fact]
    public async Task Catalog_HasExactlyTenExampleDestinations()
    {
        var options = await new OptionCatalogService().GetOptionsAsync();

        Assert.Equal(10, options.Count);
    }

    [Fact]
    public async Task Catalog_DestinationsHaveDistinctSlugsAndNames()
    {
        var options = await new OptionCatalogService().GetOptionsAsync();

        Assert.Equal(10, options.Select(o => o.Slug).Distinct().Count());
        Assert.Equal(10, options.Select(o => o.Name).Distinct().Count());
    }

    [Fact]
    public async Task Catalog_EveryDestinationIsFullyDescribed()
    {
        var options = await new OptionCatalogService().GetOptionsAsync();

        Assert.All(options, o =>
        {
            Assert.False(string.IsNullOrWhiteSpace(o.Country));
            Assert.False(string.IsNullOrWhiteSpace(o.Description));
            Assert.True(o.CostPerPersonGbp > 0);
            Assert.True(o.Nights > 0);
            Assert.NotEmpty(o.VibeIntensities);
        });
    }
}
