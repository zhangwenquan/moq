﻿using System;
using System.ComponentModel;
using Xunit;
using Moq.Sdk;
using Moq.Proxy;
using static Moq.Syntax;

namespace Moq.Tests
{
    public class MoqTests
    {
        [Fact]
        public void CanRaiseEvents()
        {
            var calculator = Mock.Of<ICalculator>();
            calculator.InsertBehavior(0, new EventBehavior());

            var raised = false;

            EventHandler handler = (sender, args) => raised = true;
            calculator.TurnedOn += handler;

            calculator.TurnedOn += Raise.Event();

            Assert.True(raised);

            raised = false;
            calculator.TurnedOn -= handler;
            calculator.TurnedOn -= handler;

            calculator.TurnedOn += Raise();

            Assert.False(raised);
        }

        [Fact]
        public void CanRaiseEventsWithArgs()
        {
            var mock = Mock.Of<INotifyPropertyChanged>();
            mock.InsertBehavior(0, new EventBehavior());

            var property = "";
            mock.PropertyChanged += (sender, args) => property = args.PropertyName;

            mock.PropertyChanged += Raise<PropertyChangedEventHandler>(new PropertyChangedEventArgs("Mode"));

            Assert.Equal("Mode", property);
        }

        [Fact]
        public void CanSetupPropertyViaReturns()
        {
            var calculator = Mock.Of<ICalculator>();

            calculator.Mode.Returns(CalculatorMode.Standard);

            var mode = calculator.Mode;
            
            Assert.Equal(CalculatorMode.Standard, mode);
        }

        [Fact]
        public void CanSetupPropertyTwiceViaReturns()
        {
            var calculator = Mock.Of<ICalculator>();

            calculator.Mode.Returns(CalculatorMode.Standard);
            calculator.Mode.Returns(CalculatorMode.Scientific);

            var mode = calculator.Mode;

            Assert.Equal(CalculatorMode.Scientific, mode);
        }

        [Fact]
        public void CanSetupMethodWithArgumentsViaReturns()
        {
            var calculator = Mock.Of<ICalculator>();

            calculator.Add(2, 3).Returns(5);

            var result = calculator.Add(2, 3);

            Assert.Equal(5, result);
        }

        [Fact]
        public void CanSetupMethodWithDifferentArgumentsViaReturns()
        {
            var calculator = Mock.Of<ICalculator>();

            calculator.Add(2, 2).Returns(4);
            calculator.Add(2, 3).Returns(5);

            calculator.Add(10, Any<int>()).Returns(10);

            calculator.Add(Any<int>(i => i > 20), Any<int>()).Returns(20);

            Assert.Equal(5, calculator.Add(2, 3));
            Assert.Equal(4, calculator.Add(2, 2));
            Assert.Equal(10, calculator.Add(10, 2));
            Assert.Equal(20, calculator.Add(25, 20));
        }

        [Fact]
        public void CanReturnFunction()
        {
            var calculator = Mock.Of<ICalculator>();

            calculator.Add(2, 2).Returns(() => 4);

            Assert.Equal(4, calculator.Add(2, 2));
        }

        [Fact]
        public void CanReturnFunctionWithArgs()
        {
            var calculator = Mock.Of<ICalculator>();

            calculator.Add(Any<int>(), Any<int>()).Returns((int x, int y) => x + y);

            Assert.Equal(4, calculator.Add(2, 2));
            Assert.Equal(5, calculator.Add(2, 3));
        }

        [Fact]
        public void CanInvokeCallback()
        {
            var calculator = Mock.Of<ICalculator>();
            var called = false;

            calculator.Add(Any<int>(), Any<int>())
                .Callback(() => called = true)
                .Returns((int x, int y) => x + y);

            calculator.Add(2, 2);

            Assert.True(called);
        }

        [Fact]
        public void CanInvokeTwoCallbacks()
        {
            var calculator = Mock.Of<ICalculator>();
            var called1 = false;
            var called2 = false;

            calculator.Add(Any<int>(), Any<int>())
                .Callback(() => called1 = true)
                .Callback(() => called2 = true)
                .Returns((int x, int y) => x + y)
                ;

            calculator.Add(2, 2);

            Assert.True(called1);
            Assert.True(called2);
        }

        [Fact]
        public void CanInvokeCallbackAfterReturn()
        {
            var calculator = Mock.Of<ICalculator>();
            var called = false;

            calculator.Add(Any<int>(), Any<int>())
                .Returns((int x, int y) => x + y)
                // The reason this can't work is that Returns does not keep 
                // invoking the next handler in the behavior pipeline, and 
                // therefore short-circuits the behaviors at this point.
                .Callback(() => called = true);

            calculator.Add(2, 2);

            Assert.True(called);
        }
    }
}