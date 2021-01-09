using AIR.Flume;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class ServiceRegisterTests
{
    [Test]
    public void RegisterTypeImplementation_NoCollisisons_AddsServiceImplementation()
    {
        // Arrange
        var register = new ServiceRegister();

        // Act
        register.Register<IMockService, MockService>();

        // Assert
        var registeredService = register.Resolve<IMockService>();
        Assert.AreEqual(typeof(MockService), registeredService.GetType());
    }

    [Test]
    public void RegisterType_NoServicesRegistered_AddsNewService()
    {
        // Arrange
        var register = new ServiceRegister();

        // Act
        register.Register<MockService>();

        // Assert
        MockService service = null;
        TestDelegate td = () => service = register.Resolve<MockService>();
        Assert.DoesNotThrow(td);
        Assert.IsNotNull(service);
    }

    [Test]
    public void RegisterInstance_NoServicesRegistered_AddsInstance()
    {
        // Arrange
        var register = new ServiceRegister();

        // Act
        register.Register(new MockService());

        // Assert
        MockService service = null;
        TestDelegate td = () => service = register.Resolve<MockService>();
        Assert.DoesNotThrow(td);
        Assert.IsNotNull(service);
    }

    [Test]
    public void Register_SameTypeRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        var register = new ServiceRegister();
        register.Register(new MockService());

        // Act
        TestDelegate td = () => register.Register<MockService>();

        // Assert
        Assert.Throws<ServiceCollisionException<MockService>>(td);
    }

    [Test]
    public void Replace_SameTypeRegistered_ReplacesServiceWithWarning()
    {
        // Arrange
        bool warningLogged = false;
        var register = new ServiceRegister();
        register.Register<IMockService>(new MockService());
        Application.logMessageReceived += (condition, trace, type) =>
            warningLogged = type == LogType.Warning && condition.Contains(typeof(MockService).Name);

        // Act
        register.Replace<IMockService>(new MockServiceAlternate());

        // Assert
        var service = register.Resolve<IMockService>();
        Assert.AreEqual(typeof(MockServiceAlternate), service.GetType());
        Assert.True(warningLogged);
    }

    [Test]
    public void Replace_ServiceNotRegistered_WarnsUserOfMisuse()
    {
        // Arrange
        bool warningLogged = false;
        var register = new ServiceRegister();
        Application.logMessageReceived += (condition, trace, type) =>
            warningLogged = type == LogType.Warning && condition.Contains("you might be doing something wrong");

        // Act
        register.Replace<IMockService, MockService>();

        // Assert
        var service = register.Resolve<IMockService>();
        Assert.True(warningLogged);
        Assert.AreEqual(typeof(MockService), service.GetType());
    }


    [Test]
    public void Resolve_ServiceNotRegistered_ThrowsMissingServiceException()
    {
        // Arrange
        var register = new ServiceRegister();

        // Act
        TestDelegate td = () => register.Resolve<MockService>();

        // Assert
        Assert.Throws<MissingServiceException>(td);
    }

    [Test]
    public void Resolve_ServiceRegistered_ReturnsService()
    {
        // Arrange
        var register = new ServiceRegister();

        // Act
        register.Register<IMockService>(new MockService());

        // Assert
        var service = register.Resolve<IMockService>();
        Assert.IsNotNull(service);
    }

    private class MockService : IMockService { }

    private class MockServiceAlternate : IMockService { }

    private interface IMockService { }
}