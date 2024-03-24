using System;
using System.Numerics;
using Xunit;
using BrickAbode.UInt128;
using BrickAbode.UInt128.Tests;

namespace BrickAbode.UInt128.Tests.Bitwise
{

    public class UInt128Tests_Bitwise
    {

        [Fact]
        public void BitwiseAndWithDifferingBits() => UInt128TestHelper.TestOperation(UInt128TestHelper.Operation.And);

        [Fact]
        public void BitwiseOrWithComplementaryBits() => UInt128TestHelper.TestOperation(UInt128TestHelper.Operation.Or);

        [Fact]
        public void BitwiseXorWithIdenticalValues() => UInt128TestHelper.TestOperation(UInt128TestHelper.Operation.Xor);

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
