using System;
using System.Numerics;
using Xunit;
using BrickAbode.UInt128;
using BrickAbode.UInt128.Tests;

namespace BrickAbode.UInt128.Tests
{

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

    public static class UInt128TestHelper
    {

        public static void AssertOperation(UInt128 a, UInt128 b, Operation operation, int shiftAmount = 0)
        {
            // Use the existing ToBigInteger conversion method
            var bigA = UInt128.ToBigInteger(a);
            var bigB = UInt128.ToBigInteger(b);
            var sub_amt = bigA >= bigB ? bigA - bigB : BigInteger.Pow(2, 128) - (bigB - bigA); // Handle underflow

            (BigInteger expectedBig, UInt128 result, string op) = operation switch
            {
                Operation.Add         => (bigA + bigB,            a + b,             "+"),
                Operation.Subtract    => (sub_amt,                a - b,             "-"), 
                Operation.Multiply    => (bigA * bigB,            a * b,             "*"),
                Operation.Divide      => (bigA / bigB,            a / b,             "/"),
                Operation.Modulus     => (bigA % bigB,            a % b,             "%"),
                Operation.Equals      => ((bigA == bigB) ? 1 : 0, (a == b) ? 1 : 0,  "=="),
                Operation.And         => (bigA & bigB,            a & b,             "&"),
                Operation.Or          => (bigA | bigB,            a | b,             "|"),
                Operation.Xor         => (bigA ^ bigB,            a ^ b,             "^"),
                Operation.Not         => (~bigA,                  ~a,                "~"),
                Operation.LeftShift   => (bigA << shiftAmount,    a << shiftAmount,  "<<"),
                Operation.RightShift  => (bigA >> shiftAmount,    a >> shiftAmount,  ">>"),
                _                     => throw new ArgumentException("Unsupported operation"),
            };

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
            Add,
            Subtract,
            Multiply,
            Divide,
            Modulus,
            Equals,
            And,
            Or,
            Xor,
            Not,
            LeftShift,
            RightShift
        }

        // Standard test values
        public static UInt128[] testValues = new UInt128[]
        {
            new UInt128(0, 0),  // Zero
            new UInt128(0, 1),  // Near Zero
            new UInt128(0, 15), // Near Zero
            new UInt128(0, 16), // Near Zero

            new UInt128(0, (ulong)(1UL<<31)),   // 2**31
            new UInt128(0, (ulong)(1UL<<32)-1), // 2**32 - 1
            new UInt128(0, (ulong)(1UL<<32)),   // 2**32
            new UInt128(0, (ulong)(1UL<<32)+1), // 2**32 + 1
            new UInt128(0, (ulong)(1UL<<33)),   // 2**33

            new UInt128(0, (1UL<<63)),   // 2**63
            new UInt128(0, (1UL<<64)-1), // Near mid
            new UInt128(1, 0),           // Mid (2**64)
            new UInt128(1, 1),           // Near mid
            new UInt128(2, 0),           // 2**65

            new UInt128((1UL<<31), 0),                    // 2**95
            new UInt128((1UL<<32)-1, 0xFFFFFFFFFFFFFFFF), // 2**96 - 1
            new UInt128((1UL<<32), 0),                    // 2**96
            new UInt128((1UL<<32), 1),                    // 2**96 + 1
            new UInt128((1UL<<33), 0),                    // 2**97

            new UInt128(ulong.MaxValue - 1, 0),              // Near Max
            new UInt128(ulong.MaxValue - 1, 1),              // Near Max
            new UInt128(ulong.MaxValue - 1, ulong.MaxValue), // Near Max
            new UInt128(ulong.MaxValue, ulong.MaxValue - 1), // Near Max
            new UInt128(ulong.MaxValue, ulong.MaxValue),     // Max
            UInt128.MaxValue,                                // Max, expressed differently

            new UInt128(0, 12345678901234567890UL),                      // A random large value
            new UInt128(11675585457592138324UL, 0),                      // A random large value
            new UInt128(11675585457592138324UL, 1),                      // A random large value
            new UInt128(11675585457592138324UL, 12345678901234567890UL), // A random large value
            new UInt128(0x7FFFFFFFFFFFFFFFUL, 0xFFFFFFFFFFFFFFFFUL)     // 2**127-1, a Mersenne prime
        };

        public static readonly int[] shiftAmounts = new int[] { 0, 1, 7, 8, 9, 63, 64, 65, 119, 127, 128 };

    }
}
