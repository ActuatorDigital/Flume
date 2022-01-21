using AIR.Flume;
using NUnit.Framework;
using System;
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

    [Test]
    public void Dispose_WhenNoRegisters_ShouldNotThrow()
    {
        // Arrange
        var register = new ServiceRegister();

        // Act
        void Act() => register.Dispose();

        // Assert
        Assert.DoesNotThrow(Act);
    }

    [Test]
    public void Dispose_WhenNoIDisposablesRegistered_ShouldNotThrow()
    {
        // Arrange
        var register = new ServiceRegister();
        register.Register<IMockService>(new MockService());

        // Act
        void Act() => register.Dispose();

        // Assert
        Assert.DoesNotThrow(Act);
    }

    [Test]
    public void Dispose_WhenIDisposableRegistered_ShouldCallServiceDispose()
    {
        // Arrange
        var register = new ServiceRegister();
        register.Register<IMockService, MockDisposeThrowService>();

        // Act
        void Act() => register.Dispose();

        // Assert
        Assert.Throws<MockDisposeThrowService.MockDisposeThrowServiceException>(Act);
    }

    [Test]
    public void Dispose_WhenIDisposableRegisteredManual_ShouldCallServiceDispose()
    {
        // Arrange
        var register = new ServiceRegister();
        register.Register<IMockService>(new MockDisposeThrowService());

        // Act
        void Act() => register.Dispose();

        // Assert
        Assert.Throws<MockDisposeThrowService.MockDisposeThrowServiceException>(Act);
    }

    [Test]
    public void Dispose_WhenMultipleIDisposableRegistered_ShouldCallAllServiceDisposes()
    {
        // Arrange
        const int EXPECTED_COUNT = 1;
        var register = new ServiceRegister();
        var disposedCount = 0;
        var delegateService = new MockDisposeDelegateService(() => disposedCount++);
        register.Register<IMockService>(delegateService);
        var anotherDisposedCount = 0;
        var anotherDelegateService = new MockDisposeDelegateService(() => anotherDisposedCount++);
        register.Register<IAnotherMockService>(anotherDelegateService);

        // Act
        register.Dispose();

        // Assert
        Assert.AreEqual(EXPECTED_COUNT, disposedCount);
        Assert.AreEqual(EXPECTED_COUNT, anotherDisposedCount);
    }

    [Test]
    public void Dispose_WhenSingleIDisposableConcretionRegisteredAsMultipleInterfaces_ShouldCallServiceDisposeOnce()
    {
        // Arrange
        const int EXPECTED_COUNT = 1;
        var register = new ServiceRegister();
        var disposedCount = 0;
        var delegateService = new MockDisposeDelegateService(() => disposedCount++);
        register.Register<IMockService>(delegateService);
        register.Register<IAnotherMockService>(delegateService);

        // Act
        register.Dispose();

        // Assert
        Assert.AreEqual(EXPECTED_COUNT, disposedCount);
    }

    [Test]
    public void Resolve_WhenSameConcretionRegisteredForTwoService_ShouldReturnSameObjectForBothResolves()
    {
        // Arrange
        var register = new ServiceRegister();
        var comboService = new MockComboService();
        register.Register<IMockService>(comboService);
        register.Register<IAnotherMockService>(comboService);

        // Act
        var mock = register.Resolve<IMockService>();
        var anotherMock = register.Resolve<IAnotherMockService>();

        // Assert
        Assert.AreSame(comboService, mock);
        Assert.AreSame(comboService, anotherMock);
        Assert.AreSame(mock, anotherMock);
    }

    internal class MockDisposeThrowService : IMockService, IDisposable
    {
        [Serializable]
        public class MockDisposeThrowServiceException : Exception
        {
            public MockDisposeThrowServiceException()
            { }
        }

        public void Dispose()
        {
            throw new MockDisposeThrowServiceException();
        }
    }

    internal class MockDisposeDelegateService : IMockService, IAnotherMockService, IDisposable
    {
        private System.Action _action;

        public MockDisposeDelegateService(Action action) => _action = action;

        public void Dispose() => _action?.Invoke();
    }

    internal class AnotherMockDisposeDelegateService : IAnotherMockService, IDisposable
    {
        private System.Action _action;

        public AnotherMockDisposeDelegateService(Action action) => _action = action;

        public void Dispose() => _action?.Invoke();
    }

    private class MockService : IMockService
    { }

    private class MockServiceAlternate : IMockService
    { }

    private class MockComboService : IMockService, IAnotherMockService
    { }

    private interface IMockService
    { }

    private interface IAnotherMockService
    { }
}