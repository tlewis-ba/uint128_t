using System;
using System.Numerics;
using Xunit;
using Xunit.Abstractions;
using BrickAbode.UInt128;
using BrickAbode.UInt128.Tests;

namespace BrickAbode.UInt128.Tests.Bitwise
{

    public class UInt128Tests_Bitwise : UInt128TestBase
    {

        public UInt128Tests_Bitwise(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void BitwiseAndWithDifferingBits() => helper.TestOperation(UInt128TestHelper.Operation.And);

        [Fact]
        public void BitwiseOrWithComplementaryBits() => helper.TestOperation(UInt128TestHelper.Operation.Or);

        [Fact]
        public void BitwiseXorWithIdenticalValues() => helper.TestOperation(UInt128TestHelper.Operation.Xor);

        [Fact]
        public void BitwiseNotOnMaxValue()
        {
            foreach (var a in helper.testValues)
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
            helper.AssertOperation(
                1,
                0, // Not used for shift operations
                UInt128TestHelper.Operation.LeftShift,
                64);
        }

        [Fact]
        public void RightShiftAcrossBoundary()
        {
            helper.AssertOperation(
                new UInt128(1, 0), // 1<<64
                0, // Not used for shift operations
                UInt128TestHelper.Operation.RightShift,
                64);
        }
    }
}
