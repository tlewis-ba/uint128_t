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
            //  if (operation == Operation.Not) expectedBig = ~bigA;

            // Use the existing FromBigInteger conversion method for all except NOT, which doesn't need it
            //UInt128 expected = (operation != Operation.Not) ? UInt128.FromBigInteger(expectedBig) : new UInt128();
            UInt128 expected = UInt128.FromBigInteger(expectedBig);

            // Assert
            if (expected != result)
            {
                if (operation == Operation.Not)
                {
                    throw new InvalidOperationException($"Assertion Failed: Expected result was {expected}, but got ~{a} = {result}.");
                }
                throw new InvalidOperationException($"Assertion Failed: Expected result was {expected}, but got {a} {op} {b} = {result}.");
            }
        }


        public static void TestOperation(UInt128TestHelper.Operation operation, int? shiftAmount = null)
        {
            foreach (var a in testValues)
            {
                foreach (var b in testValues)
                {
                    if ( (operation == UInt128TestHelper.Operation.RightShift) || (operation == UInt128TestHelper.Operation.LeftShift))
                    {
                        foreach (var shiftAmt in shiftAmounts)
                        {
                            UInt128TestHelper.AssertOperation(a, b, operation, shiftAmt);
                        }
                    }
                    else
                    {
                        UInt128TestHelper.AssertOperation(a, b, operation);
                    }
                }
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

        public static UInt128[] testValues = new UInt128[]
        {
            new UInt128(0, 0), // Zero
            UInt128.MaxValue, // Maximum value
            new UInt128(0, 1), // Near Zero
            new UInt128(0, 15), // Near Zero
            new UInt128(0, 16), // Near Zero
            new UInt128(ulong.MaxValue - 1, 0), // Near Max
            new UInt128(ulong.MaxValue - 1, 1), // Near Max
            new UInt128(ulong.MaxValue - 1, ulong.MaxValue), // Near Max
            new UInt128(0, ulong.MaxValue), // Equal to ulong.MaxValue
            new UInt128(0, ulong.MaxValue) + 1, // ulong.MaxValue + 1
            new UInt128(0, 12345678901234567890UL), // A random large value
            new UInt128(0, 18446744073709551557UL) // A prime number
        };

        public static readonly int[] shiftAmounts = new int[] { 0, 1, 7, 8, 9, 63, 64, 65, 119, 127, 128 };

    }

    public class UInt128Tests_Arithmetic
    {
        // Test values

        // Tests

        [Fact]
        public void BitwiseNotOnMaxValue()
        {
            foreach (var a in UInt128TestHelper.testValues)
            {
                UInt128 b = ~a;
                UInt128 expected = UInt128.MaxValue;
                UInt128 result = a ^ b;
                if ((a == b) || (result != expected))
                {
                    throw new InvalidOperationException($"Assertion Failed: negation of {a:X} yielded {b:X}; xor'ing them gave {result:X} instead of {expected:X}.  Negation is broken");
                }
            }
        }

        [Fact]
        public void BitwiseAndWithDifferingBits() => UInt128TestHelper.TestOperation(UInt128TestHelper.Operation.And);

        [Fact]
        public void BitwiseOrWithComplementaryBits() => UInt128TestHelper.TestOperation(UInt128TestHelper.Operation.Or);

        [Fact]
        public void BitwiseXorWithIdenticalValues() => UInt128TestHelper.TestOperation(UInt128TestHelper.Operation.Xor);

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

#if UNDEFINED
        // Less good implementations
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

        [Fact]
        public void LeftShiftAcrossBoundary() => UInt128TestHelper.TestOperation(UInt128TestHelper.Operation.LeftShift);

        [Fact]
        public void RightShiftAcrossBoundary() => UInt128TestHelper.TestOperation(UInt128TestHelper.Operation.RightShift);

#endif
    }

}
