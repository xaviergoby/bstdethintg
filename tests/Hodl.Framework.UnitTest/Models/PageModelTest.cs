using Hodl.Models;

namespace Hodl.Api.UnitTest.Models;

public class PageModelTest
{
    [Test]
    public void PageModelPageLimitTest()
    {
        var pm = new PagingModel<Object>
        {
            PageSize = PagingModel<Object>.MIN_PAGE_SIZE - 1
        };
        Assert.That(pm.PageSize, Is.GreaterThanOrEqualTo(PagingModel<Object>.MIN_PAGE_SIZE), "Minimum page size is exceeded.");

        pm.PageSize = PagingModel<Object>.MAX_PAGE_SIZE + 1;
        Assert.That(pm.PageSize, Is.LessThanOrEqualTo(PagingModel<Object>.MAX_PAGE_SIZE), "Maximum page size is exceeded.");
    }

    [Test]
    [TestCase(10, 0, 0)]
    [TestCase(10, 5, 1)]
    [TestCase(10, 10, 1)]
    [TestCase(10, 11, 2)]
    [TestCase(11, 11, 1)]
    [TestCase(11, 12, 2)]
    [TestCase(10, 30, 3)]
    [TestCase(10, 1000, 100)]
    public void PageModelPageTotalPagesTest(int pageSize, int itemCount, int expected)
    {
        var pm = new PagingModel<Object>
        {
            PageSize = pageSize,
            TotalItems = itemCount,
        };
        Assert.That(pm.TotalPages, Is.EqualTo(expected), "Input: PageSize: \"{0}\", Itemcount: \"{1}\"", pageSize, itemCount);
    }

    [Test]
    [TestCase(10, 0, 0, 1)]
    [TestCase(10, 0, 1, 1)]
    [TestCase(10, 5, 0, 1)]
    [TestCase(10, 5, 1, 1)]
    [TestCase(10, 5, 2, 1)]
    [TestCase(10, 10, 1, 1)]
    [TestCase(10, 10, 2, 1)]
    [TestCase(10, 11, 1, 1)]
    [TestCase(10, 11, 2, 2)]
    [TestCase(10, 11, 3, 2)]
    [TestCase(10, 20, 1, 1)]
    [TestCase(10, 20, 2, 2)]
    [TestCase(10, 20, 3, 2)]
    [TestCase(10, 1000, 51, 51)]
    public void PageModelPageSetCurrentPageTest(int pageSize, int itemCount, int page, int expected)
    {
        var pm = new PagingModel<Object>
        {
            PageSize = pageSize,
            TotalItems = itemCount,
            CurrentPage = page
        };
        Assert.That(pm.CurrentPage, Is.EqualTo(expected), "Input: PageSize: \"{0}\", Itemcount: \"{1}\"", pageSize, itemCount);
    }

}
