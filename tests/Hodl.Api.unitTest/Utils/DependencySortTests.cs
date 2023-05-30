namespace Hodl.Api.UnitTest.Utils;
internal class DependencySortTests
{
    [Test]
    public void TestDependencySort()
    {
        List<int> list = new()
        {
            1,2,3,4,5,6,7,8,9,10
        };
        List<(int, int)> dependencies = new()
        {
            ( 1, 2 ),
            ( 2, 3 ),
            ( 4, 3 ),
            ( 2, 4 ),
            ( 5, 1 ),
            ( 6, 1 ),
            ( 7, 1 )
        };
        var sorted = DependencySort.Sort(list, dependencies);
        Console.WriteLine(sorted);
        Assert.AreEqual(sorted[0], 3);
        Assert.AreEqual(sorted[1], 4);
        Assert.AreEqual(sorted[2], 2);
        Assert.AreEqual(sorted[3], 1);
        Assert.AreEqual(sorted[4], 5);
    }
}
