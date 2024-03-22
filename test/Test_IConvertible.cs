using System;
using System.Numerics;
using Xunit;
using BrickAbode.UInt128;

namespace BrickAbode.UInt128.Tests.IConvertible
{

    public static class UInt128TestHelper
    {

        public static void AssertConversion<T>(UInt128 val)
        {
            // Convert UInt128 value to BigInteger for comparison
            BigInteger bigIntEquivalent = UInt128.ToBigInteger(val);

            // Dynamically determine the target type from T
            Type targetType = typeof(T);

            // Use Convert.ChangeType to convert UInt128 and BigInteger to the target type (T)
            T convertedFromUInt128 = (T)Convert.ChangeType(val, targetType);
            T convertedFromBigInteger = (T)Convert.ChangeType(bigIntEquivalent, targetType);

            // Assert that the converted values are equal
            Assert.Equal(convertedFromBigInteger, convertedFromUInt128);
        }


    }

    public class UInt128Tests_Arithmetic
    {
        [Fact(Skip="BigInteger is stupid")]
        public void ConvertToDouble_ComparisonWithBigInteger()
        {
            // Create a UInt128 value for testing
            var value = new UInt128(0, 1) << 100; // Example value

            // Test conversion to double using the generic type parameter
            UInt128TestHelper.AssertConversion<double>(value);
        }

    }

}
