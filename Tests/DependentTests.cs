﻿
using Actuator.Flume;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class DependentTests
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
    public void Construction_NoInjectMethodOrContainer_ThrowsNoErrors()
    {
        // Act
        TestDelegate td = () => new MockBlankDependent();

        // Assert
        Assert.DoesNotThrow(td);
    }

    private class MockBlankDependent : Dependent { }

    [Test]
    public void Construction_WithInjectionInChild_InjectsChildClass()
    {
        // Act
        var awaker = new MockDependentWithAwakeAndPrivateInject();

        // Assert
        Assert.IsTrue(awaker.Injected);
    }

    [Test]
    public void Construction_WithInjectionInChild_CallsChildConstructor()
    {
        // Act
        var awaker = new MockDependentWithAwakeAndPrivateInject();

        // Assert
        Assert.IsTrue(awaker.Constructed);
    }

    private class MockDependentWithAwakeAndPrivateInject : Dependent
    {
        public bool Injected = false, Constructed = false;
        void Inject() => Injected = true;
        public MockDependentWithAwakeAndPrivateInject() => Constructed = true;
    }

    [Test]
    public void Construction_WithMissingDependents_LogsMissingDependencyException()
    {
        // Act
        TestDelegate td = () => new MockDependent();

        // Assert
        Assert.Throws<MissingDependencyException>(td);
    }

    [Test]
    public void Construction_WithDependencies_InjectsDependencies()
    {
        // Arrange
        _container.Register<MockService>();

        // Act
        var dependent = new MockDependent();

        // Assert
        Assert.IsTrue(dependent.InjectionCalled);
    }

    private class MockDependent : Dependent
    {
        public bool InjectionCalled = false;

        public void Inject(MockService mockService) =>
            InjectionCalled = true;
    }

    private class MockService { }

    [Test]
    public void Construction_WithMultipleDependencies_InjectsAllDependencies()
    {
        // Arrange
        _container
            .Register<MockDependentOne>()
            .Register<MockDependentTwo>()
            .Register<MockDependentThree>()
            .Register<MockDependentFour>()
            .Register<MockDecedentFive>()
            .Register<MockDependentSix>();

        // Act
        var dependent = new MockMultipleDependent();

        // Assert
        Assert.IsTrue(dependent.FirstDependentInjected);
        Assert.IsTrue(dependent.SecondDependentInjected);
        Assert.IsTrue(dependent.ThirdDependentInjected);
        Assert.IsTrue(dependent.FourthDependentInjected);
        Assert.IsTrue(dependent.FifthDependentInjected);
        Assert.IsTrue(dependent.SixthDependentInjected);
    }

    private class MockMultipleDependent : Dependent
    {
        public bool
            FirstDependentInjected,
            SecondDependentInjected,
            ThirdDependentInjected,
            FourthDependentInjected,
            FifthDependentInjected,
            SixthDependentInjected;

        public void Inject(
            MockDependentOne one,
            MockDependentTwo two,
            MockDependentThree three,
            MockDependentFour four,
            MockDecedentFive five,
            MockDependentSix six
        )
        {
            FirstDependentInjected = one != null;
            SecondDependentInjected = two != null;
            ThirdDependentInjected = three != null;
            FourthDependentInjected = four != null;
            FifthDependentInjected = five != null;
            SixthDependentInjected = six != null;
        }
    }

    private class MockDependentOne { }

    private class MockDependentTwo { }

    private class MockDependentThree { }

    private class MockDependentFour { }

    private class MockDecedentFive { }

    private class MockDependentSix { }


    [Test]
    public void InjectInBaseAndChild_DifferentInjectDependencies_CallsAllInjects()
    {
        // Arrange
        _container.Register<MockService>();
        _container.Register<DifferentMockService>();

        // Act
        var differentInjectedDecedent = new DifferentMockInjectedAncestor();

        // Assert
        Assert.IsTrue(differentInjectedDecedent.AncestorInjected);
        Assert.IsTrue(differentInjectedDecedent.DifferentAncestorInjected);
    }

    private class MockInjectedAncestor : Dependent
    {
        public bool AncestorInjected = false;

        public void Inject(MockService mock) => AncestorInjected = true;
    }

    private class DifferentMockInjectedAncestor : MockInjectedAncestor
    {
        public bool DifferentAncestorInjected = false;

        public void Inject(DifferentMockService mock) => DifferentAncestorInjected = true;
    }

    private class DifferentMockService { }
}