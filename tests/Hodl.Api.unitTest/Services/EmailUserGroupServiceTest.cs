using Hodl.Api.Services.Notifications;
using Microsoft.Extensions.Options;

namespace Hodl.Api.UnitTest.Services;

internal class EmailUserGroupServiceTest
{
    private static EmailUserGroupService CreateService()
    {
        var _emailService = new Mock<IEmailService>();
        var appOptions = Options.Create(new AppDefaults()
        {
            RunningEnvironment = "TEST"
        });

        return new EmailUserGroupService(
            appOptions,
            null,
            _emailService.Object);
    }


    [Test]
    public void RegisterErrorFor()
    {
        var emailUserGroupService = CreateService();

        Assert.DoesNotThrow(() => emailUserGroupService.RegisterErrorFor("Test", "123"));
        Assert.IsTrue(emailUserGroupService.IsRegisteredAsError("Test", "123"));
    }

    [Test]
    public void ResetErrorFor()
    {
        var emailUserGroupService = CreateService();

        Assert.DoesNotThrow(() => emailUserGroupService.ResetErrorFor("Test", "123"));

        emailUserGroupService.RegisterErrorFor("Test", "123");
        emailUserGroupService.ResetErrorFor("Test", "123");
        Assert.IsFalse(emailUserGroupService.IsRegisteredAsError("Test", "123"));
    }

    [Test]
    public void IsRegisteredAsError()
    {
        var emailUserGroupService = CreateService();

        Assert.Multiple(() =>
        {
            Assert.DoesNotThrow(() => emailUserGroupService.RegisterErrorFor("Test1", "123"));
            Assert.IsTrue(emailUserGroupService.IsRegisteredAsError("Test1", "123"));
            Assert.IsFalse(emailUserGroupService.IsRegisteredAsError("Test2", "123"));
            Assert.IsFalse(emailUserGroupService.IsRegisteredAsError("Test1", "321"));
        });
    }
}
