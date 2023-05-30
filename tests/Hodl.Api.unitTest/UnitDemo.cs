namespace Hodl.Api.UnitTest;

public class DemoTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestTheTest()
    {
        //Arrange

        //Act

        //Assert
        Assert.Pass();
    }

    [Test]
    public void TestTheTestAsync()
    {
        //Arrange

        //Act

        //Assert
        Assert.DoesNotThrowAsync(async () => await Task.Delay(100));
    }
}
