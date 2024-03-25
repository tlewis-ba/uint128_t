using System;
using System.Numerics;
using Xunit;
using Xunit.Abstractions;
using BrickAbode.UInt128;
using BrickAbode.UInt128.Tests;

namespace BrickAbode.UInt128.Tests.Arithmetic {

    public class UInt128Tests_Arithmetic : UInt128TestBase
    {

        public UInt128Tests_Arithmetic(ITestOutputHelper output) : base(output)
        {
        }

#if UNDEFINED
        [Fact]
            public void AddLeadingToHighIncrement()
            {
                helper.AssertOperation(
                        new UInt128(1UL << 63, 1),
                        new UInt128(1UL << 63, ulong.MaxValue - 1),
                        UInt128TestHelper.Operation.Add);
            }

        [Fact]
            public void AddResultingInMaxValue()
            {
                helper.AssertOperation(
                        UInt128.MaxValue - 1,
                        1,
                        UInt128TestHelper.Operation.Add);
            }

        [Fact]
            public void AddOverflowWrapAround()
            {
                helper.AssertOperation(
                        UInt128.MaxValue,
                        1,
                        UInt128TestHelper.Operation.Add);
            }

        [Fact]
            public void SubtractWithZeroResult()
            {
                helper.AssertOperation(
                        new UInt128(1, 0), // 1<<64
                        new UInt128(1, 0), // 1<<64
                        UInt128TestHelper.Operation.Subtract);
            }

        [Fact]
            public void SubtractLeadingToHighDecrement()
            {
                helper.AssertOperation(
                        new UInt128(1, 0), // 1<<64
                        1,
                        UInt128TestHelper.Operation.Subtract);
            }

        [Fact]
            public void SubtractUnderflowWrapAround()
            {
                helper.AssertOperation(
                        0,
                        1,
                        UInt128TestHelper.Operation.Subtract);
            }

#else

        [Fact]
        public void AddTest() => helper.TestOperation(UInt128TestHelper.Operation.Add);

        [Fact]
        public void SubtractTest() => helper.TestOperation(UInt128TestHelper.Operation.Subtract);

        [Fact]
        public void MultiplyTest() => helper.TestOperation(UInt128TestHelper.Operation.Multiply);

        [Fact]
            public void MultiplyLeadingToHighComponent()
            {
                helper.AssertOperation(
                        new UInt128(0, 1UL << 32),
                        new UInt128(0, 1UL << 32),
                        UInt128TestHelper.Operation.Multiply);
            }

        [Fact]
            public void MultiplyWithOverflowWrapAround()
            {
                helper.AssertOperation(
                        new UInt128(0, (1UL << 64) - 1),
                        2,
                        UInt128TestHelper.Operation.Multiply);
            }

        [Fact]
        public void DivideTest() => helper.TestOperation(UInt128TestHelper.Operation.Divide);

        [Fact]
            public void DivideWithNoRemainder()
            {
                helper.AssertOperation(
                        new UInt128(1, 0), // 1<<64
                        new UInt128(0, 1UL << 32),
                        UInt128TestHelper.Operation.Divide);
            }

        [Fact]
            public void DivideWithMaximumValue()
            {
                helper.AssertOperation(
                        UInt128.MaxValue,
                        1,
                        UInt128TestHelper.Operation.Divide);
            }

        [Fact]
            public void DivideResultingInMinimumValue()
            {
                helper.AssertOperation(
                        UInt128.MaxValue,
                        UInt128.MaxValue,
                        UInt128TestHelper.Operation.Divide);
            }

        [Fact]
            public void ModulusWithNoRemainder()
            {
                helper.AssertOperation(
                        new UInt128(1, 0), // 1<<64
                        new UInt128(0, 1UL << 32),
                        UInt128TestHelper.Operation.Modulus);
            }

        [Fact]
            public void ModulusWithRemainder()
            {
                helper.AssertOperation(
                        new UInt128(2, 1), // (1<<65) + 1
                        new UInt128(1, 0), // 1<<64
                        UInt128TestHelper.Operation.Modulus);
            }

        [Fact]
            public void IncrementJustBelowHighComponent()
            {
                var a = new UInt128(0, UInt64.MaxValue);
                a++;

                helper.AssertOperation(
                        a,
                        new UInt128(1, 0),
                        UInt128TestHelper.Operation.Equals);
            }

        [Fact]
            public void IncrementAtMaxValueWrapAround()
            {
                var a = UInt128.MaxValue;
                a++;
                helper.AssertOperation(
                        a,
                        new UInt128(0, 0),
                        UInt128TestHelper.Operation.Equals);
            }

        [Fact]
            public void DecrementFromHighComponent()
            {
                var a = new UInt128(1, 0);
                a--;
                helper.AssertOperation(
                        a,
                        new UInt128(0, UInt64.MaxValue),
                        UInt128TestHelper.Operation.Equals);
            }

        [Fact]
            public void DecrementAtZeroWrapAround()
            {
                var a = new UInt128(0, 0);
                a--;
                helper.AssertOperation(
                        a,
                        UInt128.MaxValue,
                        UInt128TestHelper.Operation.Equals);
            }

#endif

    }
}
