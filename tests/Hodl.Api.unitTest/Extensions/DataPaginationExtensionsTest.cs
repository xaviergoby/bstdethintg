namespace Hodl.Api.Extensions;

internal class DataPaginationExtensionsTest
{
    private readonly Mock<HodlDbContext> contextMock = new();

    [SetUp]
    public void Setup()
    {
        List<CryptoCurrency> data = new();
        // Add test data
        for (int i = 0; i < 100; i++)
        {
            data.Add(new CryptoCurrency()
            {
                Symbol = i.ToString(),
                Name = i.ToString(),
            });
        }

        contextMock.Setup(x => x.CryptoCurrencies).ReturnsDbSet(data);
    }

    [Test]
    public void PaginateTest()
    {
        // Create a paged resultset
        var _query = contextMock.Object.CryptoCurrencies.Where(_ => true);
        var pageResult = _query.Paginate(1, 10);

        Assert.NotNull(pageResult, "Page result is null");
        Assert.NotNull(pageResult.Items, "Page items result is null");
        Assert.AreEqual(10, pageResult.Items.Count, "Expected number of items on the page is wrong");
        Assert.AreEqual(100, pageResult.TotalItems, "Total number of items is wrong");
        Assert.AreEqual(10, pageResult.TotalPages, "Total number of pages is wrong");
        Assert.AreEqual(1, pageResult.CurrentPage, "Current page is wrong");
    }

    [Test]
    [TestCase(0, 10, 1, 10, 10)]
    [TestCase(1, 100, 1, 100, 1)]
    [TestCase(2, 100, 1, 100, 1)]
    [TestCase(2, 80, 2, 20, 2)]
    [TestCase(2, 8, 2, 8, 13)]
    [TestCase(13, 8, 13, 4, 13)]
    [TestCase(14, 8, 13, 4, 13)]
    public void PaginateQueryTest(int page, int pageSize, int expectedPage, int expectedItemCount, int expectedTotalPages)
    {
        // Create a paged resultset
        var _query = contextMock.Object.CryptoCurrencies.Where(_ => true);
        var pageResult = _query.Paginate(page, pageSize);

        Assert.NotNull(pageResult, "Page result is null");
        Assert.NotNull(pageResult.Items, "Page items result is null");
        Assert.AreEqual(expectedPage, pageResult.CurrentPage, "Current page is wrong");
        Assert.AreEqual(expectedItemCount, pageResult.Items.Count, "Expected number of items on the page is wrong");
        Assert.AreEqual(expectedTotalPages, pageResult.TotalPages, "Total number of pages is wrong");
    }

    [Test]
    public async Task PaginateAsyncTest()
    {
        // Create a paged resultset
        var _query = contextMock.Object.CryptoCurrencies.Where(_ => true);
        var pageResult = await _query.PaginateAsync(1, 10);

        Assert.NotNull(pageResult, "Page result is null");
        Assert.NotNull(pageResult.Items, "Page items result is null");
        Assert.AreEqual(10, pageResult.Items.Count, "Expected number of items on the page is wrong");
        Assert.AreEqual(100, pageResult.TotalItems, "Total number of items is wrong");
        Assert.AreEqual(10, pageResult.TotalPages, "Total number of pages is wrong");
        Assert.AreEqual(1, pageResult.CurrentPage, "Current page is wrong");
    }

    [Test]
    [TestCase(0, 10, 1, 10, 10)]
    [TestCase(1, 100, 1, 100, 1)]
    [TestCase(2, 100, 1, 100, 1)]
    [TestCase(2, 80, 2, 20, 2)]
    [TestCase(2, 8, 2, 8, 13)]
    [TestCase(13, 8, 13, 4, 13)]
    [TestCase(14, 8, 13, 4, 13)]
    public async Task PaginateAsyncQueryTest(int page, int pageSize, int expectedPage, int expectedItemCount, int expectedTotalPages)
    {
        // Create a paged resultset
        var _query = contextMock.Object.CryptoCurrencies.Where(_ => true);
        var pageResult = await _query.PaginateAsync(page, pageSize);

        Assert.NotNull(pageResult, "Page result is null");
        Assert.NotNull(pageResult.Items, "Page items result is null");
        Assert.AreEqual(expectedPage, pageResult.CurrentPage, "Current page is wrong");
        Assert.AreEqual(expectedItemCount, pageResult.Items.Count, "Expected number of items on the page is wrong");
        Assert.AreEqual(expectedTotalPages, pageResult.TotalPages, "Total number of pages is wrong");
    }
}
