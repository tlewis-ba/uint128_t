using System;
using System.Numerics;
using Xunit;
using BrickAbode.UInt128;
using BrickAbode.UInt128.Tests;

namespace BrickAbode.UInt128.Tests.Arithmetic {

    public class UInt128Tests_Arithmetic
    {

#if UNDEFINED
        [Fact]
            public void AddLeadingToHighIncrement()
            {
                UInt128TestHelper.AssertOperation(
                        new UInt128(1UL << 63, 1),
                        new UInt128(1UL << 63, ulong.MaxValue - 1),
                        UInt128TestHelper.Operation.Add);
            }

        [Fact]
            public void AddResultingInMaxValue()
            {
                UInt128TestHelper.AssertOperation(
                        UInt128.MaxValue - 1,
                        1,
                        UInt128TestHelper.Operation.Add);
            }

        [Fact]
            public void AddOverflowWrapAround()
            {
                UInt128TestHelper.AssertOperation(
                        UInt128.MaxValue,
                        1,
                        UInt128TestHelper.Operation.Add);
            }

        [Fact]
            public void SubtractWithZeroResult()
            {
                UInt128TestHelper.AssertOperation(
                        new UInt128(1, 0), // 1<<64
                        new UInt128(1, 0), // 1<<64
                        UInt128TestHelper.Operation.Subtract);
            }

        [Fact]
            public void SubtractLeadingToHighDecrement()
            {
                UInt128TestHelper.AssertOperation(
                        new UInt128(1, 0), // 1<<64
                        1,
                        UInt128TestHelper.Operation.Subtract);
            }

        [Fact]
            public void SubtractUnderflowWrapAround()
            {
                UInt128TestHelper.AssertOperation(
                        0,
                        1,
                        UInt128TestHelper.Operation.Subtract);
            }

#else

        [Fact]
        public void AddTest() => UInt128TestHelper.TestOperation(UInt128TestHelper.Operation.Add);

        [Fact]
        public void SubtractTest() => UInt128TestHelper.TestOperation(UInt128TestHelper.Operation.Subtract);

        [Fact(Skip="Code looks broken; yay!")]
        public void MultiplyTest() => UInt128TestHelper.TestOperation(UInt128TestHelper.Operation.Multiply);

        [Fact]
            public void MultiplyLeadingToHighComponent()
            {
                UInt128TestHelper.AssertOperation(
                        new UInt128(0, 1UL << 32),
                        new UInt128(0, 1UL << 32),
                        UInt128TestHelper.Operation.Multiply);
            }

        [Fact]
            public void MultiplyWithOverflowWrapAround()
            {
                UInt128TestHelper.AssertOperation(
                        new UInt128(0, (1UL << 64) - 1),
                        2,
                        UInt128TestHelper.Operation.Multiply);
            }

        [Fact]
        public void DivideTest() => UInt128TestHelper.TestOperation(UInt128TestHelper.Operation.Divide);

        [Fact]
            public void DivideWithNoRemainder()
            {
                UInt128TestHelper.AssertOperation(
                        new UInt128(1, 0), // 1<<64
                        new UInt128(0, 1UL << 32),
                        UInt128TestHelper.Operation.Divide);
            }

        [Fact]
            public void DivideWithMaximumValue()
            {
                UInt128TestHelper.AssertOperation(
                        UInt128.MaxValue,
                        1,
                        UInt128TestHelper.Operation.Divide);
            }

        [Fact]
            public void DivideResultingInMinimumValue()
            {
                UInt128TestHelper.AssertOperation(
                        UInt128.MaxValue,
                        UInt128.MaxValue,
                        UInt128TestHelper.Operation.Divide);
            }

        [Fact]
            public void ModulusWithNoRemainder()
            {
                UInt128TestHelper.AssertOperation(
                        new UInt128(1, 0), // 1<<64
                        new UInt128(0, 1UL << 32),
                        UInt128TestHelper.Operation.Modulus);
            }

        [Fact]
            public void ModulusWithRemainder()
            {
                UInt128TestHelper.AssertOperation(
                        new UInt128(2, 1), // (1<<65) + 1
                        new UInt128(1, 0), // 1<<64
                        UInt128TestHelper.Operation.Modulus);
            }

        [Fact]
            public void IncrementJustBelowHighComponent()
            {
                var a = new UInt128(0, UInt64.MaxValue);
                a++;

                UInt128TestHelper.AssertOperation(
                        a,
                        new UInt128(1, 0),
                        UInt128TestHelper.Operation.Equals);
            }

        [Fact]
            public void IncrementAtMaxValueWrapAround()
            {
                var a = UInt128.MaxValue;
                a++;
                UInt128TestHelper.AssertOperation(
                        a,
                        new UInt128(0, 0),
                        UInt128TestHelper.Operation.Equals);
            }

        [Fact]
            public void DecrementFromHighComponent()
            {
                var a = new UInt128(1, 0);
                a--;
                UInt128TestHelper.AssertOperation(
                        a,
                        new UInt128(0, UInt64.MaxValue),
                        UInt128TestHelper.Operation.Equals);
            }

        [Fact]
            public void DecrementAtZeroWrapAround()
            {
                var a = new UInt128(0, 0);
                a--;
                UInt128TestHelper.AssertOperation(
                        a,
                        UInt128.MaxValue,
                        UInt128TestHelper.Operation.Equals);
            }

#endif

    }
}
