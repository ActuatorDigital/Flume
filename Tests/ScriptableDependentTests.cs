// Copyright (c) AIR Pty Ltd. All rights reserved.

using AIR.Flume;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

[TestFixture]
public class ScriptableDependentTests
{
    private FlumeServiceContainer _container;

    [SetUp]
    public void SetUp()
    {
        _container = new GameObject("Container")
            .AddComponent<FlumeServiceContainer>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_container);
    }

    [Test]
    public void Creation_NoInjectMethodOrContainer_ThrowsNoErrors()
    {
        // Act
        TestDelegate td = () => new GameObject(
            nameof(Creation_NoInjectMethodOrContainer_ThrowsNoErrors),
            typeof(MockBehaviourWithScriptableObjectField));

        // Assert
        Assert.DoesNotThrow(td);
    }

    private class MockIndependentScriptableDependent : ScriptableDependent
    {
        public bool InjectCalled;
        private void Inject() => InjectCalled = true;
    }

    [Test]
    public void AddComponent_WithInjectionInScriptableObjectField_CallsInjectOnInstance()
    {
        // Act
        var waker = new GameObject(
                nameof(AddComponent_WithInjectionInScriptableObjectField_CallsInjectOnInstance))
            .AddComponent<MockBehaviourWithScriptableObjectField>();

        Assert.IsTrue(waker.Injected);
    }

    private class MockBehaviourWithScriptableObjectField : MonoBehaviour
    {
        private MockIndependentScriptableDependent _dependentScriptableObject;

        public void Awake() => _dependentScriptableObject = ScriptableObject
            .CreateInstance<MockIndependentScriptableDependent>();

        public bool Injected => _dependentScriptableObject.InjectCalled;
    }

    [Test]
    public void CreateInstance_WithMissingDependents_LogsMissingDependencyException()
    {
        // Arrange
        bool thrown = false;
        Application.logMessageReceived += LogAssert();

        // Act
        ScriptableObject.CreateInstance<MockScriptableDependent>();

        // Assert
        Assert.IsTrue(thrown);
        Application.logMessageReceived -= LogAssert();
        Application.LogCallback LogAssert()
        {
            return (condition, trace, type) =>
            {
                thrown =
                    type == LogType.Exception &&
                    condition.Contains(nameof(MissingDependencyException));
                if (thrown)
                    UnityEngine.TestTools.LogAssert.Expect(type, condition);
            };
        }
    }

    [Test]
    public void CreateInstance_WithDependencies_InjectsDependencies()
    {
        // Arrange
        _container.Register<MockService>();

        // Act
        var dependent = ScriptableObject.CreateInstance<MockScriptableDependent>();

        // Assert
        Assert.IsTrue(dependent.InjectionCalled);
    }

    private class MockScriptableDependent : ScriptableDependent
    {
        public bool InjectionCalled = false;

        public void Inject(MockService mockService) =>
            InjectionCalled = true;
    }

    [Test]
    public void CreateInstance_WithDependencies_DeliversDependenciesBeforeUnityLifecycle()
    {
        // Arrange
        _container.Register<MockService>();

        // Act
        var lifecycle = ScriptableObject
            .CreateInstance<MockLifecycleTrackingScriptableDependent>();

        // Assert
        Assert.IsTrue(lifecycle.InjectRanFirst);
    }

    private class MockLifecycleTrackingScriptableDependent : ScriptableDependent
    {
        public bool InjectRanFirst = true;
        private bool _injected;

        void Inject() => _injected = true;

        public void OnEnable() => FlagInjectRanLate();

        private void FlagInjectRanLate()
        {
            if (!_injected) InjectRanFirst = false;
        }
    }

    [Test]
    public void InjectInBaseAndChild_DifferentInjectDependencies_CallsAllInjects()
    {
        // Arrange
        _container.Register<MockService>();
        _container.Register<DifferentMockService>();

        // Act
        var differentInjectedDecedent = ScriptableObject
            .CreateInstance<DifferentMockInjectedAncestor>();

        // Assert
        Assert.IsTrue(differentInjectedDecedent.AncestorInjected);
        Assert.IsTrue(differentInjectedDecedent.DifferentAncestorInjected);
    }

    private class MockInjectedAncestor : ScriptableDependent
    {
        public bool AncestorInjected = false;

        public void Inject(MockService mock) => AncestorInjected = true;
    }

    private class DifferentMockInjectedAncestor : MockInjectedAncestor
    {
        public bool DifferentAncestorInjected = false;

        public void Inject(DifferentMockService mock) => DifferentAncestorInjected = true;
    }

    private class MockService { }

    private class DifferentMockService { }

    [Test]
    public void Dependent_WithSOServiceDependencies_CreatesInstanceAndInjectsDependencies()
    {
        // Arrange
        _container.Register<IMockScriptableObjectService, MockScriptableObjectService>();

        // Act
        var dependent = new MockSOServiceDependent();

        // Assert
        Assert.IsTrue(dependent.InjectionCalled);
    }

    private interface IMockScriptableObjectService { }

    private class MockScriptableObjectService : ScriptableObject, IMockScriptableObjectService { }

    private class MockSOServiceDependent : Dependent
    {
        public bool InjectionCalled = false;

        public void Inject(IMockScriptableObjectService mockService) =>
            InjectionCalled = true;
    }

    [Test]
    public void Dependent_WithPreMadeSOServiceDependencies_InjectsDependencies()
    {
        // Arrange
        var SOService = ScriptableObject.CreateInstance<MockScriptableObjectService>();
        _container.Register<IMockScriptableObjectService>(SOService);

        // Act
        var dependent = new MockSOServiceDependent();

        // Assert
        Assert.IsTrue(dependent.InjectionCalled);
    }
}