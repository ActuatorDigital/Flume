using AIR.Flume;
using NUnit.Framework;
using System;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture]
public class ServiceInstallerTests
{
    private GameObject go;

    [SetUp]
    public void Setup()
    {
        go = new GameObject(nameof(ServiceInstallerTests));
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(go);
    }

    [Test]
    public void RegisterTypeImplementation_NoCollisisons_AddsServiceImplementation()
    {
        // Arrange
        var service = default(IMockService);
        go.AddComponent<MockServiceInstaller>();

        // Act
        var mockDep = new MockDependent();
        service = mockDep.MockService;

        // Assert
        Assert.AreEqual(typeof(MockService), service.GetType());
    }

    [Test]
    public void Resolve_WhenThrowsDuringInject_ShouldWrapAndRethrow()
    {
        // Arrange
        go.AddComponent<MockServiceInstaller>();

        // Act
        TestDelegate td = () => new MockInjectThrowsDependent();

        // Assert
        Assert.Throws<DuringInjectException>(td);
    }

    [Test]
    public void InstallServices_WhenThrowsDuringInject_ShouldCallOnException()
    {
        // Arrange
        LogAssert.ignoreFailingMessages = true;
        var exceptionThrown = default(Exception);

        // Act
        go.AddComponent<ThrowServiceInstaller>();
        exceptionThrown = go.GetComponent<ThrowServiceInstaller>().ThrownEx;

        // Assert
        Assert.IsNotNull(exceptionThrown);
        Assert.IsTrue(exceptionThrown is NotImplementedException);
    }

    private class MockServiceInstaller : ServiceInstaller
    {
        protected override void InstallServices(FlumeServiceContainer container)
            => container.Register<IMockService, MockService>();
    }

    private class ThrowServiceInstaller : ServiceInstaller
    {
        public Exception ThrownEx;

        protected override void InstallServices(FlumeServiceContainer container)
            => throw new NotImplementedException();

        protected override void OnException(Exception ex)
            => ThrownEx = ex;
    }

    private class MockService : IMockService
    { }

    private class MockServiceAlternate : IMockService
    { }

    private interface IMockService
    { }

    private class MockDependent : Dependent
    {
        public IMockService MockService { get; private set; }

        public void Inject(IMockService mockService) => MockService = mockService;
    }

    private class MockInjectThrowsDependent : Dependent
    {
        public IMockService MockService { get; private set; }

        public void Inject(IMockService mockService) => throw new NotImplementedException();
    }
}