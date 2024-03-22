using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace BrickAbode.UInt128
{

    /// <summary>
    /// Represents a 128-bit unsigned integer. This is a mutable struct type,
    /// inspired by the design of .NET's built-in integer types. It supports arithmetic,
    /// bitwise, and comparison operations. For division, modulo, and some conversions,
    /// it relies on <see cref="BigInteger"/> for handling large numbers.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    [ComVisible(true)]
    public struct UInt128 : IComparable, IFormattable, IConvertible,
                             IComparable<UInt128>, IEquatable<UInt128>
    {
        private ulong high;
        private ulong low;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="UInt128"/> struct using
        /// specified high and low parts of the 128-bit integer.
        /// </summary>
        /// <param name="high">The high 64 bits of the 128-bit integer.</param>
        /// <param name="low">The low 64 bits of the 128-bit integer.</param>
        public UInt128(ulong high, ulong low)
        {
            this.high = high;
            this.low = low;
        }
        #endregion

        #region Arithmetic Operators

        /// <summary>
        /// Adds two <see cref="UInt128"/> values and returns the result.
        /// </summary>
        /// <param name="a">The first value to add.</param>
        /// <param name="b">The second value to add.</param>
        /// <returns>The sum of <paramref name="a"/> and <paramref name="b"/>.</returns>
        public static UInt128 operator +(UInt128 a, UInt128 b)
        {
            ulong lowSum = a.low + b.low;
            // Calculate carry without a branch
            bool carry = (lowSum < a.low) | (lowSum < b.low);
            // Add the high parts with the carry. Overflow in high parts exceeds UInt128 range and is ignored.
            ulong highSum = a.high + b.high + Convert.ToUInt64(carry);

            return new UInt128(highSum, lowSum);
        }

        /// <summary>
        /// Subtracts one <see cref="UInt128"/> value from another and returns the result.
        /// </summary>
        /// <param name="a">The value to subtract from.</param>
        /// <param name="b">The value to subtract.</param>
        /// <returns>The result of subtracting <paramref name="b"/> from <paramref name="a"/>.</returns>
        public static UInt128 operator -(UInt128 a, UInt128 b)
        {
            ulong lowDiff = a.low - b.low;
            bool borrow = a.low < b.low;
            ulong highDiff = a.high - b.high - Convert.ToUInt64(borrow);

            return new UInt128(highDiff, lowDiff);
        }

        /// <summary>
        /// Multiplies two <see cref="UInt128"/> values and returns the result.
        /// </summary>
        /// <param name="a">The first value to multiply.</param>
        /// <param name="b">The second value to multiply.</param>
        /// <returns>The product of <paramref name="a"/> and <paramref name="b"/>.</returns>
        public static UInt128 operator *(UInt128 a, UInt128 b)
        {
            ulong a0 = a.low & 0xffffffff;
            ulong a1 = a.low >> 32;
            ulong a2 = a.high & 0xffffffff;
            ulong a3 = a.high >> 32;
            ulong b0 = b.low & 0xffffffff;
            ulong b1 = b.low >> 32;
            ulong b2 = b.high & 0xffffffff;
            ulong b3 = b.high >> 32;

            ulong p0 = a0 * b0;
            ulong p1 = a0 * b1;
            ulong p2 = a0 * b2;
            ulong p3 = a0 * b3;
            ulong p4 = a1 * b0;
            ulong p5 = a1 * b1;
            ulong p6 = a1 * b2;
            // ulong p7 = a1 * b3;
            ulong p8 = a2 * b0;
            ulong p9 = a2 * b1;
            // ulong p10 = a2 * b2;
            // ulong p11 = a2 * b3;
            ulong p12 = a3 * b0;
            // ulong p13 = a3 * b1;
            // ulong p14 = a3 * b2;
            // ulong p15 = a3 * b3;

            ulong low = 0;
            ulong high = 0;

            low += p0;
            low += (p1 << 32);
            high += (p1 >> 32);
            high += p2;
            high += (p3 << 32);
            low += (p4 << 32);
            high += (p4 >> 32);
            high += p5;
            high += (p6 << 32);
            // skip: p7 << 128
            high += p8;
            high += (p9 << 32);
            // skip: p10 << 128
            // skip: p11 << 160
            high += (p12 << 32);
            // skip: p13 << 128
            // skip: p14 << 160
            // skip: p15 << 192

            return new UInt128(high, low);
        }


        /// <summary>
        /// Divides two <see cref="UInt128"/> values and returns the quotient.
        /// </summary>
        /// <param name="a">The dividend.</param>
        /// <param name="b">The divisor.</param>
        /// <returns>The quotient of <paramref name="a"/> divided by <paramref name="b"/>.</returns>
        public static UInt128 operator /(UInt128 a, UInt128 b)
        {
            BigInteger bigA = ToBigInteger(a);
            BigInteger bigB = ToBigInteger(b);
            BigInteger result = bigA / bigB;
            return FromBigInteger(result);
        }

        /// <summary>
        /// Divides two <see cref="UInt128"/> values and returns the remainder.
        /// </summary>
        /// <param name="a">The dividend.</param>
        /// <param name="b">The divisor.</param>
        /// <returns>The remainder of <paramref name="a"/> divided by <paramref name="b"/>.</returns>
        public static UInt128 operator %(UInt128 a, UInt128 b)
        {
            BigInteger bigA = ToBigInteger(a);
            BigInteger bigB = ToBigInteger(b);
            BigInteger result = bigA % bigB;
            return FromBigInteger(result);
        }

        // Unary negation and plus
        /// <summary>
        /// Returns the value of the <see cref="UInt128"/> operand (the sign is unchanged).
        /// This operation has no effect and exists for compatibility with numeric operations.
        /// </summary>
        /// <param name="a">The operand.</param>
        /// <returns>The value of the operand, <paramref name="a"/>.</returns>
        public static UInt128 operator +(UInt128 a) => a;

        /// <summary>
        /// Negates a specified <see cref="UInt128"/> value; meaningless for unsigned integers, so throws an exception.
        /// </summary>
        /// <param name="a">The value to negate.</param>
        /// <returns>(Throws NotImplementedException.)</returns>
        public static UInt128 operator -(UInt128 a) => throw new NotImplementedException("Unary negation is not implemented for UInt128 due to its unsigned nature.");

        // Increment and decrement
        /// <summary>
        /// Increments the <see cref="UInt128"/> operand by 1.
        /// </summary>
        /// <param name="a">The value to increment.</param>
        /// <returns>The value of <paramref name="a"/> incremented by 1.</returns>
        public static UInt128 operator ++(UInt128 a)
        {
            a.low++;
            if (a.low == 0)
            {
                a.high++;
            }
            return a;
        }

        /// <summary>
        /// Decrements the <see cref="UInt128"/> operand by 1.
        /// </summary>
        /// <param name="a">The value to decrement.</param>
        /// <returns>The value of <paramref name="a"/> decremented by 1.</returns>
        public static UInt128 operator --(UInt128 a)
        {
            if (a.low == 0)
            {
                a.high--;
            }
            a.low--;
            return a;
        }

        #endregion

        #region Bitwise Operators

        /// <summary>
        /// Performs a bitwise And operation on two <see cref="UInt128"/> values.
        /// </summary>
        /// <param name="a">The first operand.</param>
        /// <param name="b">The second operand.</param>
        /// <returns>The result of the bitwise And operation.</returns>
        public static UInt128 operator &(UInt128 a, UInt128 b)
        {
            ulong h = a.high & b.high;
            ulong l = a.low & b.low;
            return new UInt128(h, l);
        }

        /// <summary>
        /// Performs a bitwise Or operation on two <see cref="UInt128"/> values.
        /// </summary>
        /// <param name="a">The first operand.</param>
        /// <param name="b">The second operand.</param>
        /// <returns>The result of the bitwise Or operation.</returns>
        public static UInt128 operator |(UInt128 a, UInt128 b)
        {
            ulong h = a.high | b.high;
            ulong l = a.low | b.low;
            return new UInt128(h, l);
        }

        /// <summary>
        /// Performs a bitwise Exclusive Or (Xor) operation on two <see cref="UInt128"/> values.
        /// </summary>
        /// <param name="a">The first operand.</param>
        /// <param name="b">The second operand.</param>
        /// <returns>The result of the bitwise Xor operation.</returns>
        public static UInt128 operator ^(UInt128 a, UInt128 b)
        {
            ulong h = a.high ^ b.high;
            ulong l = a.low ^ b.low;
            return new UInt128(h, l);
        }

        /// <summary>
        /// Inverts all the bits of the specified <see cref="UInt128"/> value.
        /// </summary>
        /// <param name="a">The value to invert.</param>
        /// <returns>The result of inverting all bits in <paramref name="a"/>.</returns>
        public static UInt128 operator ~(UInt128 a)
        {
            ulong h = ~a.high;
            ulong l = ~a.low;
            return new UInt128(h, l);
        }

        #endregion

        #region Shift Operators

        // FIXME: both of these should be fixed to the mutable style

        /// <summary>
        /// Shifts a <see cref="UInt128"/> value a specified number of bits to the left.
        /// </summary>
        /// <param name="a">The value to shift.</param>
        /// <param name="shift">The number of bits to shift <paramref name="a"/> to the left.</param>
        /// <returns>The result of shifting <paramref name="a"/> to the left by <paramref name="shift"/> bits.</returns>
        public static UInt128 operator <<(UInt128 a, int shift)
        {
            ulong h = 0;
            ulong l = 0;
            if(shift < 64)
            {
                l = a.low << shift;
                h = (a.low >> (64-shift)) | (a.high << shift);
            }
            else if(shift == 64)
            {
                h = a.low;
            }
            else if(shift < 128)
            {
                h = a.low << (shift - 64);
            }
            else
            {
                // h = 0;
                // l = 0;
            }

            return new UInt128(h, l);
        }

        /// <summary>
        /// Shifts a <see cref="UInt128"/> value a specified number of bits to the right.
        /// </summary>
        /// <param name="a">The value to shift.</param>
        /// <param name="shift">The number of bits to shift <paramref name="a"/> to the right.</param>
        /// <returns>The result of shifting <paramref name="a"/> to the right by <paramref name="shift"/> bits.</returns>
        public static UInt128 operator >>(UInt128 a, int shift)
        {
            ulong h = 0;
            ulong l = 0;
            if(shift < 64)
            {
                h = a.high >> shift;
                l = (a.high << (64-shift)) | (a.low >> shift);
            }
            else if(shift == 64)
            {
                l = a.high;
            }
            else if(shift < 128)
            {
                l = a.high >> (shift - 64);
            }
            else
            {
                // h = 0;
                // l = 0;
            }

            return new UInt128(h, l);
        }

        #endregion

        #region Comparison Operators and Methods

        /// <summary>
        /// Compares this instance to a specified object and returns an indication of their relative values.
        /// Implements IComparable.CompareTo(object? obj).
        /// </summary>
        /// <param name="obj">An object to compare, or null.</param>
        /// <returns>A signed number indicating the relative values of this instance and <paramref name="obj"/>.</returns>
        public int CompareTo(object? obj)
        {
            if (obj == null) return 1; // Consider null to be less than any instance of UInt128

            if (!(obj is UInt128))
                throw new ArgumentException("Object must be of type UInt128.", nameof(obj));

            return CompareTo((UInt128)obj);
        }

        /// <summary>
        /// Compares this instance to a specified <see cref="UInt128"/> object and returns an indication of their relative values.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>A signed number indicating the relative values of this instance and <paramref name="other"/>.</returns>
        public int CompareTo(UInt128 other)
        {
            if (high > other.high) return 1;
            if (high < other.high) return -1;

            if (low > other.low) return 1;
            if (low < other.low) return -1;

            return 0;
        }

        /// <summary>
        /// Indicates whether this instance and a specified UInt128 are equal.
        /// </summary>
        /// <param name="other">The UInt128 to compare with the current object.</param>
        /// <returns>true if <paramref name="other"/> and this instance are the same type and represent the same value; otherwise, false.</returns>
        public bool Equals(UInt128 other)
        {
            // Use XOR to find differences between high and low parts, then OR the results.
            // The outcome is zero if and only if there are no differences.
            return ((this.high ^ other.high) | (this.low ^ other.low)) == 0;
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>true if <paramref name="other"/> and this instance are the same type and represent the same value; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            return obj is UInt128 other && Equals(other);
        }

        public static bool operator ==(UInt128 left, UInt128 right) => left.Equals(right);
        public static bool operator !=(UInt128 left, UInt128 right) => !(left == right);

        #endregion

        #region Conversion Methods

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            int hashHigh = high.GetHashCode();
            int hashLow = low.GetHashCode();
            return hashHigh ^ hashLow;
        }

        /// <summary>
        /// Converts the numeric value of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>A string representation of the value of this instance.</returns>
        public override string ToString() => ToString(null, null); // default
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            // Handle null or empty format by using the "G" (General) format.
            string fmt = string.IsNullOrEmpty(format) ? "G" : format.ToUpperInvariant();

            switch (fmt)
            {
                case "X": // Hexadecimal format
                    return $"0x{high:X16}{low:X16}";
                case "B": // Binary format - a custom implementation might be needed here
                    return ConvertToBinaryString();
                // case "D": // Decimal format
                // case "G": // General format - similar to "D", but can be overridden for different types
                // case "P": // Percent format
                // case "N": // Number format
                // case "E": // Exponential format (scientific notation)
                // case "F": // Fixed-point format
                default:
                    // For unsupported formats, fall back to BigInteger's implementation.
                    // This includes scientific, fixed-point, number, percent, and custom numeric formats.
                    return ToBigInteger(this).ToString(format, formatProvider);
            }
        }

        #endregion

        #region IConvertible Implementations

        /// <summary>
        /// Converts a <see cref="UInt128"/> value to a <see cref="BigInteger"/>.
        /// </summary>
        /// <param name="value">The <see cref="UInt128"/> value to convert.</param>
        /// <returns>A <see cref="BigInteger"/> that represents the converted <see cref="UInt128"/>.</returns>
        private static BigInteger ToBigInteger(UInt128 value)
        {
            return new BigInteger(value.low) + (new BigInteger(value.high) << 64);
        }

        /// <summary>
        /// Converts a <see cref="BigInteger"/> value to a <see cref="UInt128"/>.
        /// </summary>
        /// <param name="value">The <see cref="BigInteger"/> value to convert.</param>
        /// <returns>A <see cref="UInt128"/> that represents the converted <see cref="BigInteger"/>.</returns>
        private static UInt128 FromBigInteger(BigInteger value)
        {
            byte[] bytes = value.ToByteArray();
            ulong low = BitConverter.ToUInt64(bytes, 0);
            ulong high = bytes.Length > 8 ? BitConverter.ToUInt64(bytes, 8) : 0;
            return new UInt128(high, low);
        }

        /// <summary>
        /// Converts the numeric value of this instance to its binary string representation.
        /// </summary>
        /// <returns>A binary string representation of the value of this instance.</returns>
        private string ConvertToBinaryString()
        {
            // Convert both high and low parts to binary strings and concatenate them.
            // Padding left with zeros to ensure the strings represent the full 64 bits of each part.
            return Convert.ToString((long)high, 2).PadLeft(64, '0') + Convert.ToString((long)low, 2).PadLeft(64, '0');
        }

        /// <summary>
        /// Converts the value of this instance to its equivalent string representation using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that supplies culture-specific formatting information. This parameter is not used.</param>
        /// <returns>A string representation of the value of this instance.</returns>
        string IConvertible.ToString(IFormatProvider? provider) => ToString();

	    /// <summary>
	    /// Returns the <see cref="TypeCode"/> for this instance.
	    /// </summary>
	    /// <returns>
	    /// Returns the constant value <see cref="TypeCode.Object"/>.  N.b.
	    /// that UInt128 is a struct type, but dotnet TypeCodes do not
	    /// differentiate between struct and object types; the generic
	    /// type for non-builtin types is just <see cref="TypeCode.Object"/>.
	    /// </returns>
        TypeCode IConvertible.GetTypeCode() => TypeCode.Object;

        /// <summary>
        /// Converts the value of this instance to an equivalent Unicode character using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>A Unicode character equivalent to the value of this instance.</returns>
        char IConvertible.ToChar(IFormatProvider? provider) => (char)low;

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="DateTime"/> using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>A <see cref="DateTime"/> instance equivalent to the value of this instance.</returns>
        DateTime IConvertible.ToDateTime(IFormatProvider? provider)
        {
            if ((high != 0) || (low > long.MaxValue))
            {
                throw new InvalidCastException("UInt128 contains a value that cannot be represented as a DateTime.");
            }

            return new DateTime((long)low);
        }

        // IConvertible implementations

        /// <summary>
        /// Converts the value of this instance to an equivalent Boolean value using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>A Boolean value equivalent to the value of this instance.</returns>
        bool IConvertible.ToBoolean(IFormatProvider? provider) => high != 0 || low != 0;

        /// <summary>
        /// Converts the value of this instance to an equivalent 8-bit unsigned integer using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>An 8-bit unsigned integer equivalent to the value of this instance.</returns>
        byte IConvertible.ToByte(IFormatProvider? provider) => (byte)low;

        /// <summary>
        /// Converts the value of this instance to an equivalent decimal number using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>A decimal number equivalent to the value of this instance.</returns>
        decimal IConvertible.ToDecimal(IFormatProvider? provider) => (decimal)ToBigInteger(this);

        /// <summary>
        /// Converts the value of this instance to an equivalent single-precision floating-point number using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>A single-precision floating-point number equivalent to the value of this instance.</returns>
        float IConvertible.ToSingle(IFormatProvider? provider) => (float)ToBigInteger(this);

        /// <summary>
        /// Converts the value of this instance to an equivalent double-precision floating-point number using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>A double-precision floating-point number equivalent to the value of this instance.</returns>
        double IConvertible.ToDouble(IFormatProvider? provider) => (double)ToBigInteger(this);

        /// <summary>
        /// Converts the value of this instance to an equivalent 16-bit signed integer using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>A 16-bit signed integer equivalent to the value of this instance.</returns>
        short IConvertible.ToInt16(IFormatProvider? provider) => (short)low;

        /// <summary>
        /// Converts the value of this instance to an equivalent 8-bit signed integer using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>An 8-bit signed integer equivalent to the value of this instance.</returns>
        sbyte IConvertible.ToSByte(IFormatProvider? provider) => (sbyte)low;

        /// <summary>
        /// Converts the value of this instance to an equivalent 32-bit signed integer using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>A 32-bit signed integer equivalent to the value of this instance.</returns>
        int IConvertible.ToInt32(IFormatProvider? provider) => (int)low;

        /// <summary>
        /// Converts the value of this instance to an equivalent 64-bit signed integer using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>A 64-bit signed integer equivalent to the value of this instance.</returns>
        long IConvertible.ToInt64(IFormatProvider? provider) => (long)low;

        /// <summary>
        /// Converts the value of this instance to an equivalent 16-bit unsigned integer using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>A 16-bit unsigned integer equivalent to the value of this instance.</returns>
        ushort IConvertible.ToUInt16(IFormatProvider? provider) => (ushort)low;

        /// <summary>
        /// Converts the value of this instance to an equivalent 32-bit unsigned integer using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>A 32-bit unsigned integer equivalent to the value of this instance.</returns>
        uint IConvertible.ToUInt32(IFormatProvider? provider) => (uint)low;

        /// <summary>
        /// Converts the value of this instance to an equivalent 64-bit unsigned integer using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>A 64-bit unsigned integer equivalent to the value of this instance.</returns>
        ulong IConvertible.ToUInt64(IFormatProvider? provider) => low;

        /// <summary>
        /// Converts the value of this instance to an object of the specified <see cref="Type"/> that has an equivalent value, using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="conversionType">The <see cref="Type"/> to which the value of this instance is converted.</param>
        /// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>An object instance of type <paramref name="conversionType"/> whose value is equivalent to the value of this instance.</returns>
        /// <exception cref="InvalidCastException">Thrown when the conversion cannot be performed.</exception>
        object IConvertible.ToType(Type conversionType, IFormatProvider? provider)
        {
            if (conversionType == typeof(BigInteger))
                return ToBigInteger(this);
            throw new InvalidCastException($"Cannot convert to {conversionType.FullName}");
        }

        #endregion

    }

    class Program
    {
        static int Main(string[] args)
        {
            UInt128 a = new UInt128(1, 2);
            UInt128 b = new UInt128(0, 3);
            Console.WriteLine($"a({a:X}) == b({b:B})?  {a == b}");
#pragma warning disable CS1718
            Console.WriteLine($"a({a:X}) == a({a:B})?  {a == a}");
#pragma warning restore CS1718
            return 0;
        }
    }

}

