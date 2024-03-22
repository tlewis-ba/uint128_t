using System;
using System.Numerics;
using Xunit;

using BrickAbode.UInt128;

public static class UInt128TestHelper
{
    public enum Operation
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Modulus
        // Extend with other operations as needed
    }

    public static void AssertOperation(UInt128 a, UInt128 b, Operation operation)
    {
        // Use the existing ToBigInteger conversion method
        var bigA = UInt128.ToBigInteger(a);
        var bigB = UInt128.ToBigInteger(b);

        // Perform operation using BigInteger
        BigInteger expectedBig = operation switch
        {
            Operation.Add => bigA + bigB,
            Operation.Subtract => bigA - bigB,
            Operation.Multiply => bigA * bigB,
            Operation.Divide => bigA / bigB,
            Operation.Modulus => bigA % bigB,
            _ => throw new ArgumentException("Unsupported operation"),
        };

        // Use the existing FromBigInteger conversion method
        UInt128 expected = UInt128.FromBigInteger(expectedBig);

        // Perform the same operation on UInt128
        UInt128 result = operation switch
        {
            Operation.Add => a + b,
            Operation.Subtract => a - b,
            Operation.Multiply => a * b,
            Operation.Divide => a / b,
            Operation.Modulus => a % b,
            _ => throw new InvalidOperationException("Unsupported operation"),
        };

        // Assert
        if (expected != result)
        {
            throw new InvalidOperationException($"Assertion Failed: Expected result was {expected}, but got {result}.");
        }
    }
}

public class UInt128Tests
{
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

    // Note: Unary Plus and Negation tests don't fit the AssertOperation pattern
    // Increment and Decrement tests require a slightly different approach

    [Fact]
    public void IncrementJustBelowHighComponent()
    {
        var a = new UInt128(0, UInt64.MaxValue);
        var expected = new UInt128(1, 0);
        a++;
        Assert.Equal(expected, a);
    }

//    [Fact]
//    public void IncrementAtMaxValueWrapAround()
//    {
//        var a = UInt128.MaxValue;
//        a++;
//        Assert.Equal(new UInt128(0, 0), a);
//    }
//
//    [Fact]
//    public void DecrementFromHighComponent()
//    {
//        var a = new UInt128(1, 0);
//        var expected = new UInt128(0, UInt64.MaxValue);
//        a--;
//        Assert.Equal(expected, a);
//    }
//
//    [Fact]
//    public void DecrementAtZeroWrapAround()
//    {
//        var a = new UInt128(0, 0);
//        a--;
//        Assert.Equal(UInt128.MaxValue, a);
//    }

}

