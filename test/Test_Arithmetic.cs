using System;
using System.Numerics;
using Xunit;
using BrickAbode.UInt128;
using BrickAbode.UInt128.Tests;

namespace BrickAbode.UInt128.Tests.Arithmetic {

#if UNDEFINED
    public static class UInt128TestHelperOrig
    {
        public enum Operation
        {
            Add,
            Subtract,
            Multiply,
            Divide,
            Modulus,
            Equals
                // Extend with other operations as needed
        }

        public static void AssertOperation(UInt128 a, UInt128 b, Operation operation)
        {
            // Use the existing ToBigInteger conversion method
            var bigA = UInt128.ToBigInteger(a);
            var bigB = UInt128.ToBigInteger(b);

            (BigInteger expectedBig, UInt128 result, string op) = operation switch
            {
                Operation.Add => (bigA + bigB, a + b, "+"),
                    Operation.Subtract => (bigA >= bigB ? bigA - bigB : BigInteger.Pow(2, 128) - (bigB - bigA), a - b, "-"), // Handle underflow
                    Operation.Multiply => (bigA * bigB, a * b, "*"),
                    Operation.Divide => (bigA / bigB, a / b, "/"),
                    Operation.Modulus => (bigA % bigB, a % b, "%"),
                    Operation.Equals => ((bigA == bigB) ? 1 : 0, (a == b) ? 1 : 0, "=="),
                    _ => throw new ArgumentException("Unsupported operation"),
            };

            // Use the existing FromBigInteger conversion method
            UInt128 expected = UInt128.FromBigInteger(expectedBig);

            // Assert
            if (expected != result)
            {
                throw new InvalidOperationException($"Assertion Failed: Expected result was {expected}, but got {a} {op} {b} = {result}.");
            }
        }
    }
#endif

    public class UInt128Tests_Arithmetic
    {
        [Fact]
            public void AddLeadingToHighIncrement()
            {
                UInt128TestHelperOrig.AssertOperation(
                        new UInt128(1UL << 63, 1),
                        new UInt128(1UL << 63, ulong.MaxValue - 1),
                        UInt128TestHelperOrig.Operation.Add);
            }

        [Fact]
            public void AddResultingInMaxValue()
            {
                UInt128TestHelperOrig.AssertOperation(
                        UInt128.MaxValue - 1,
                        1,
                        UInt128TestHelperOrig.Operation.Add);
            }

        [Fact]
            public void AddOverflowWrapAround()
            {
                UInt128TestHelperOrig.AssertOperation(
                        UInt128.MaxValue,
                        1,
                        UInt128TestHelperOrig.Operation.Add);
            }

        [Fact]
            public void SubtractWithZeroResult()
            {
                UInt128TestHelperOrig.AssertOperation(
                        new UInt128(1, 0), // 1<<64
                        new UInt128(1, 0), // 1<<64
                        UInt128TestHelperOrig.Operation.Subtract);
            }

        [Fact]
            public void SubtractLeadingToHighDecrement()
            {
                UInt128TestHelperOrig.AssertOperation(
                        new UInt128(1, 0), // 1<<64
                        1,
                        UInt128TestHelperOrig.Operation.Subtract);
            }

        [Fact]
            public void SubtractUnderflowWrapAround()
            {
                UInt128TestHelperOrig.AssertOperation(
                        0,
                        1,
                        UInt128TestHelperOrig.Operation.Subtract);
            }

        [Fact]
            public void MultiplyLeadingToHighComponent()
            {
                UInt128TestHelperOrig.AssertOperation(
                        new UInt128(0, 1UL << 32),
                        new UInt128(0, 1UL << 32),
                        UInt128TestHelperOrig.Operation.Multiply);
            }

        [Fact]
            public void MultiplyWithOverflowWrapAround()
            {
                UInt128TestHelperOrig.AssertOperation(
                        new UInt128(0, (1UL << 64) - 1),
                        2,
                        UInt128TestHelperOrig.Operation.Multiply);
            }

        [Fact]
            public void DivideWithNoRemainder()
            {
                UInt128TestHelperOrig.AssertOperation(
                        new UInt128(1, 0), // 1<<64
                        new UInt128(0, 1UL << 32),
                        UInt128TestHelperOrig.Operation.Divide);
            }

        [Fact]
            public void DivideWithMaximumValue()
            {
                UInt128TestHelperOrig.AssertOperation(
                        UInt128.MaxValue,
                        1,
                        UInt128TestHelperOrig.Operation.Divide);
            }

        [Fact]
            public void DivideResultingInMinimumValue()
            {
                UInt128TestHelperOrig.AssertOperation(
                        UInt128.MaxValue,
                        UInt128.MaxValue,
                        UInt128TestHelperOrig.Operation.Divide);
            }

        [Fact]
            public void ModulusWithNoRemainder()
            {
                UInt128TestHelperOrig.AssertOperation(
                        new UInt128(1, 0), // 1<<64
                        new UInt128(0, 1UL << 32),
                        UInt128TestHelperOrig.Operation.Modulus);
            }

        [Fact]
            public void ModulusWithRemainder()
            {
                UInt128TestHelperOrig.AssertOperation(
                        new UInt128(2, 1), // (1<<65) + 1
                        new UInt128(1, 0), // 1<<64
                        UInt128TestHelperOrig.Operation.Modulus);
            }

        [Fact]
            public void IncrementJustBelowHighComponent()
            {
                var a = new UInt128(0, UInt64.MaxValue);
                a++;

                UInt128TestHelperOrig.AssertOperation(
                        a,
                        new UInt128(1, 0),
                        UInt128TestHelperOrig.Operation.Equals);
            }

        [Fact]
            public void IncrementAtMaxValueWrapAround()
            {
                var a = UInt128.MaxValue;
                a++;
                UInt128TestHelperOrig.AssertOperation(
                        a,
                        new UInt128(0, 0),
                        UInt128TestHelperOrig.Operation.Equals);
            }

        [Fact]
            public void DecrementFromHighComponent()
            {
                var a = new UInt128(1, 0);
                a--;
                UInt128TestHelperOrig.AssertOperation(
                        a,
                        new UInt128(0, UInt64.MaxValue),
                        UInt128TestHelperOrig.Operation.Equals);
            }

        [Fact]
            public void DecrementAtZeroWrapAround()
            {
                var a = new UInt128(0, 0);
                a--;
                UInt128TestHelperOrig.AssertOperation(
                        a,
                        UInt128.MaxValue,
                        UInt128TestHelperOrig.Operation.Equals);
            }

    }
}
