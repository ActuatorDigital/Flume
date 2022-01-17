using AIR.Flume;
using NUnit.Framework;
using UnityEngine;


[TestFixture]
public class ServiceInstallerTests
{
    [Test]
    public void RegisterTypeImplementation_NoCollisisons_AddsServiceImplementation()
    {
        // Arrange
        var go = new GameObject(nameof(ServiceInstallerTests));
        var service = default(IMockService);

        // Act
        var mockInstaller = go.AddComponent<MockServiceInstaller>();
        var mockDep = new MockDependent();

        // Assert
        service = mockDep.MockService;
        Assert.AreEqual(typeof(MockService), service.GetType());
        Object.DestroyImmediate(go);
    }

    private class MockServiceInstaller : ServiceInstaller
    {
        protected override void InstallServices(FlumeServiceContainer container)
            => container.Register<IMockService, MockService>();
    }

    private class MockService : IMockService { }

    private class MockServiceAlternate : IMockService { }

    private interface IMockService { }

    private class MockDependent : Dependent
    {
        public IMockService MockService { get; private set; }

        public void Inject(IMockService mockService) => MockService = mockService;
    }
}