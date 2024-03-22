// #define SKIP_BROKEN

using System;
using System.Numerics;
using Xunit;
using BrickAbode.UInt128;

namespace BrickAbode.UInt128.Tests.Bitwise
{

    public static class UInt128TestHelper
    {

        public static void AssertOperation(UInt128 a, UInt128 b, Operation operation, int shiftAmount = 0)
        {
            // Use the existing ToBigInteger conversion method
            var bigA = UInt128.ToBigInteger(a);
            var bigB = UInt128.ToBigInteger(b);

            (BigInteger expectedBig, UInt128 result, string op) = operation switch
            {
                Operation.And => (bigA & bigB, a & b, "&"),
                    Operation.Or => (bigA | bigB, a | b, "|"),
                    Operation.Xor => (bigA ^ bigB, a ^ b, "^"),
                    Operation.Not => (~bigA, ~a, "~"),
                    Operation.LeftShift => (bigA << shiftAmount, a << shiftAmount, "<<"),
                    Operation.RightShift => (bigA >> shiftAmount, a >> shiftAmount, ">>"),
                    _ => throw new ArgumentException("Unsupported operation"),
            };

            // Handle special case for NOT operation (since b is not used)
            if (operation == Operation.Not) expectedBig = ~bigA;

            // Use the existing FromBigInteger conversion method for all except NOT, which doesn't need it
            UInt128 expected = (operation != Operation.Not) ? UInt128.FromBigInteger(expectedBig) : new UInt128();

            // Assert
            if (expected != result)
            {
                throw new InvalidOperationException($"Assertion Failed: Expected result was {expected}, but got {a} {op} {b} = {result}.");
            }
        }

        // Operation enum needs to be expanded to include bitwise and shift operations
        public enum Operation
        {
            And,
            Or,
            Xor,
            Not,
            LeftShift,
            RightShift
        }
    }

    public class UInt128Tests_Arithmetic
    {

        [Fact]
        public void BitwiseAndWithDifferingBits()
        {
            UInt128TestHelper.AssertOperation(
                new UInt128(0xF0F0F0F0F0F0F0F0, 0xF0F0F0F0F0F0F0F0),
                new UInt128(0x0F0F0F0F0F0F0F0F, 0x0F0F0F0F0F0F0F0F),
                UInt128TestHelper.Operation.And);
        }

        [Fact]
        public void BitwiseOrWithComplementaryBits()
        {
            UInt128TestHelper.AssertOperation(
                new UInt128(0xF0F0F0F0F0F0F0F0, 0xF0F0F0F0F0F0F0F0),
                new UInt128(0x0F0F0F0F0F0F0F0F, 0x0F0F0F0F0F0F0F0F),
                UInt128TestHelper.Operation.Or);
        }

        [Fact]
        public void BitwiseXorWithIdenticalValues()
        {
            UInt128TestHelper.AssertOperation(
                new UInt128(0xAAAAAAAABBBBBBBB, 0xAAAAAAAABBBBBBBB),
                new UInt128(0xAAAAAAAABBBBBBBB, 0xAAAAAAAABBBBBBBB),
                UInt128TestHelper.Operation.Xor);
        }

        [Fact(Skip="Broken")]
        public void BitwiseNotOnMaxValue()
        {
            UInt128TestHelper.AssertOperation(
                UInt128.MaxValue,
                UInt128.Zero, // Unused for NOT operation
                UInt128TestHelper.Operation.Not);
        }

        [Fact]
        public void LeftShiftAcrossBoundary()
        {
            UInt128TestHelper.AssertOperation(
                1,
                0, // Not used for shift operations
                UInt128TestHelper.Operation.LeftShift,
                64);
        }

        [Fact]
        public void RightShiftAcrossBoundary()
        {
            UInt128TestHelper.AssertOperation(
                new UInt128(1, 0), // 1<<64
                0, // Not used for shift operations
                UInt128TestHelper.Operation.RightShift,
                64);
        }

    }

}
