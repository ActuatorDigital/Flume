using UnityEngine.TestTools;
using NUnit.Framework;
using UnityEngine;
using AIR.Flume;

[TestFixture]
public class DependentBehaviourTests {
    
    private FlumeServiceContainer _container;
    
    [SetUp]
    public void SetUp() {
        _container = new GameObject("Container")
            .AddComponent<FlumeServiceContainer>();
    }

    [TearDown]
    public void TearDown() {
        Object.DestroyImmediate(_container);
    }

    [Test]
    public void Instantiation_NoInjectMethodOrContainer_ThrowsNoErrors() {

        // Act
        TestDelegate td = () => new GameObject("Test", typeof(MockIndependentBehaviour));

        // Assert
        Assert.DoesNotThrow(td);

    }
    
    private class MockIndependentBehaviour : DependentBehaviour { }
    
    [Test]
    public void Instantiation_WithInjectionInChild_CallsInjectOnInstance() {
        
        // Act
        var waker = new GameObject("AwakeTest")
            .AddComponent<MockDependentBehaviourWithAwakeAndPrivateInject>();
        
        Assert.IsTrue(waker.Injected);
        
    }
    
    [Test]
    public void Instantiation_WithStartInChild_StartsChildClass() {
        
        // Act
        var waker = new GameObject("AwakeTest")
            .AddComponent<MockDependentBehaviourWithAwakeAndPrivateInject>();
        
        Assert.IsTrue(waker.Injected);
        
    }

    private class MockDependentBehaviourWithAwakeAndPrivateInject : DependentBehaviour {
        public bool Started = false, Injected = false;
        void Inject() => Injected = true;
        void Start() => Started = true;
    }
    
    [Test]
    public void Instantiation_WithMissingDependents_LogsMissingDependencyException() {
        
        // Arrange
        bool thrown = false;
        Application.logMessageReceived += (condition, trace, type) => {
            thrown = 
                type == LogType.Exception && 
                condition.Contains(typeof(MissingDependencyException).Name);
            if (thrown) 
                LogAssert.Expect(LogType.Exception, condition);
        };

        // Act
        new GameObject("MissingDependency")
            .AddComponent<MockDependentBehaviour>();
        
        // Assert
        Assert.IsTrue(thrown);
        
    }
    
    [Test]
    public void Instantiation_WithDependencies_InjectsDependencies() {
        // Arrange
        _container.Register<MockService>();
        
        // Act
        var dependent = new GameObject("Dependent")
            .AddComponent<MockDependentBehaviour>();
        
        // Assert
        Assert.IsTrue(dependent.InjectionCalled);
    }
    
    private class MockDependentBehaviour : DependentBehaviour {
        
        public bool InjectionCalled = false;

        public void Inject(MockService mockService) =>
            InjectionCalled = true;
        
    }

    [Test]
    public void Instantiation_WithDependencies_DeliversDependenciesBeforeUnityLifecycle() {
        // Arrange
        _container.Register<MockService>();
        
        // Act
        var lifecycle = new GameObject("LifecycleMock")
            .AddComponent<MockLifecycleTrackingDependentBehaviour>();
        
        // Assert
        Assert.IsTrue(lifecycle.InjectRanFirst);

    }

    private class MockLifecycleTrackingDependentBehaviour : DependentBehaviour {
        public bool InjectRanFirst = true;
        private bool _injected;

        void Inject() => _injected = true;
        
        public void OnEnable() => FlagInjectRanLate();
        public void Start() => FlagInjectRanLate();

        private void FlagInjectRanLate() {
            if (!_injected) InjectRanFirst = false;
        }
        
    }
    
    [Test]
    public void InjectInBaseAndChild_DifferentInjectDependencies_CallsAllInjects() {
        
        // Arrange
        _container.Register<MockService>();
        _container.Register<DifferentMockService>();
        
        // Act
        var differentInjectedDecedent = new GameObject("Decedent")
            .AddComponent<DifferentMockInjectedAncestor>();
        
        // Assert
        Assert.IsTrue(differentInjectedDecedent.AncestorInjected);
        Assert.IsTrue(differentInjectedDecedent.DifferentAncestorInjected);
    }
    
    private class MockInjectedAncestor : DependentBehaviour {

        public bool AncestorInjected = false;

        public void Inject(MockService mock) => AncestorInjected = true;
        
    }
    
    private class DifferentMockInjectedAncestor : MockInjectedAncestor {

        public bool DifferentAncestorInjected = false;

        public void Inject(DifferentMockService mock) => DifferentAncestorInjected = true;
        
    }

    private class MockService { }

    private class DifferentMockService { }


}


