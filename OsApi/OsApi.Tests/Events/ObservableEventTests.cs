using AppManager.OsApi.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AppManager.OsApi.Tests.Events
{
    [TestClass]
    public class ObservableEventTests
    {
        [TestMethod]
        public void Constructor_ShouldInitializeWithZeroCount()
        {
            // Act
            var observableEvent = new ObservableEvent<object?, string>();

            // Assert
            Assert.AreEqual(0, observableEvent.Count);
        }

        [TestMethod]
        public void AddHandler_ShouldIncreaseCount()
        {
            // Arrange
            var observableEvent = new ObservableEvent<object?, string>();
            bool handlerCalled = false;
            TypedEvent<object?, string> handler = (sender, args) => { handlerCalled = true; };

            // Act
            observableEvent += handler;

            // Assert
            Assert.AreEqual(1, observableEvent.Count);
        }

        [TestMethod]
        public void AddMultipleHandlers_ShouldIncreaseCount()
        {
            // Arrange
            var observableEvent = new ObservableEvent<object?, string>();
            TypedEvent<object?, string> handler1 = (sender, args) => { };
            TypedEvent<object?, string> handler2 = (sender, args) => { };

            // Act
            observableEvent += handler1;
            observableEvent += handler2;

            // Assert
            Assert.AreEqual(2, observableEvent.Count);
        }

        [TestMethod]
        public void CountChangedEvent_ShouldFireWhenHandlerAdded()
        {
            // Arrange
            var observableEvent = new ObservableEvent<object?, string>();
            int countChangedFires = 0;
            int newCount = -1;

            observableEvent.CountChangedEvent += (sender, count) => 
            { 
                countChangedFires++; 
                newCount = count; 
            };

            TypedEvent<object?, string> handler = (sender, args) => { };

            // Act
            observableEvent += handler;

            // Assert
            Assert.AreEqual(1, countChangedFires);
            Assert.AreEqual(1, newCount);
        }

        [TestMethod]
        public void RemoveHandler_ShouldDecreaseCount()
        {
            // Arrange
            var observableEvent = new ObservableEvent<object?, string>();
            TypedEvent<object?, string> handler = (sender, args) => { };
            observableEvent += handler;

            // Act
            observableEvent -= handler;

            // Assert
            Assert.AreEqual(0, observableEvent.Count);
        }
    }
}