using System;
using System.Runtime.InteropServices;

namespace BrickAbode.Int128
{

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    [ComVisible(true)]
    public struct UInt128 : IComparable, IFormattable, IConvertible,
                             IComparable<UInt128>, IEquatable<UInt128>
    {
        private readonly ulong high;
        private readonly ulong low;

        public UInt128(ulong high, ulong low)
        {
            this.high = high;
            this.low = low;
        }

        // Arithmetic operators
        public static UInt128 operator +(UInt128 a, UInt128 b)
        {
            ulong lowSum = a.low + b.low;
            // Calculate carry without a branch
            bool carry = (lowSum < a.low) | (lowSum < b.low);
            // Add the high parts with the carry. Overflow in high parts exceeds UInt128 range and is ignored.
            ulong highSum = a.high + b.high + Convert.ToUInt64(carry);

            return new UInt128(highSum, lowSum);
        }

        public static UInt128 operator -(UInt128 a, UInt128 b)
        {
            ulong lowDiff = a.low - b.low;
            bool borrow = a.low < b.low;
            ulong highDiff = a.high - b.high - Convert.ToUInt64(borrow);

            return new UInt128(highDiff, lowDiff);
        }

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

        public static UInt128 operator /(UInt128 a, UInt128 b) => throw new NotImplementedException();
        public static UInt128 operator %(UInt128 a, UInt128 b) => throw new NotImplementedException();

        // Unary negation and plus
        public static UInt128 operator -(UInt128 a) => throw new NotImplementedException();
        public static UInt128 operator +(UInt128 a) => a;  // Unary plus does not change the value

        // Increment and decrement
        public static UInt128 operator ++(UInt128 a) => throw new NotImplementedException();
        public static UInt128 operator --(UInt128 a) => throw new NotImplementedException();

        // Bitwise operators
        public static UInt128 operator &(UInt128 a, UInt128 b)
        {
            ulong h = a.high & b.high;
            ulong l = a.low & b.low;
            return new UInt128(h, l);
        }

        public static UInt128 operator |(UInt128 a, UInt128 b) => throw new NotImplementedException();
        public static UInt128 operator ^(UInt128 a, UInt128 b) => throw new NotImplementedException();
        public static UInt128 operator ~(UInt128 a) => throw new NotImplementedException();

        // Shift operators
        public static UInt128 operator <<(UInt128 a, int shift) => throw new NotImplementedException();
        public static UInt128 operator >>(UInt128 a, int shift) => throw new NotImplementedException();


        // IComparable.CompareTo(object? obj)
        int IComparable.CompareTo(object? obj) => throw new NotImplementedException();

        public int CompareTo(UInt128 other) => throw new NotImplementedException();

        public bool Equals(UInt128 other)
        {
            // Use XOR to find differences between high and low parts, then OR the results.
            // The outcome is zero if and only if there are no differences.
            return ((this.high ^ other.high) | (this.low ^ other.low)) == 0;
        }

        // Override object.Equals(object obj) to ensure consistency
        public override bool Equals(object? obj)
        {
            return obj is UInt128 other && Equals(other);
        }

        // Remember to override GetHashCode as well, when overriding Equals
        public override int GetHashCode()
        {
            // Example hash code implementation; there are many ways to do this
            int hashHigh = high.GetHashCode();
            int hashLow = low.GetHashCode();

            // Combine the hash codes of the high and low parts
            return hashHigh ^ hashLow;
        }

        // IFormattable.ToString(string? format, IFormatProvider? formatProvider)
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            string fmt = "X";
            if (format != null)
            {
                fmt = format.ToUpperInvariant();
            }
            switch (fmt)
            {
                case "X":
                    return $"0x{high:X16}{low:X16}"; // Concatenate the hex representation of high and low parts.
                // case "O": // WTF, dotnet?
                    // return $"{high:O22}{low:O22}"; // Concatenate the octal representation of high and low parts.
                case "B":
                    // Return a binary representation. This is a placeholder.
                    return ConvertToBinaryString();
                // Implement "O" case for octal representation.
                default:
                    throw new FormatException($"The {format} format string is not supported.");
            }
        }
        private string ConvertToBinaryString()
        {
            // Convert the high part to binary, ensuring it's padded to 64 characters.
            string binaryHigh = Convert.ToString((long)high, 2).PadLeft(64, '0');

            // Convert the low part to binary. No need to pad the low part as it naturally fills its space.
            string binaryLow = Convert.ToString((long)low, 2).PadLeft(64, '0');

            // Concatenate the high and low parts.
            return binaryHigh + binaryLow;
        }
        public override string ToString() => ToString(null, null);

        // IConvertible implementations with corrected nullability annotations
        TypeCode IConvertible.GetTypeCode() => throw new NotImplementedException();
        bool IConvertible.ToBoolean(IFormatProvider? provider) => throw new NotImplementedException();
        char IConvertible.ToChar(IFormatProvider? provider) => throw new NotImplementedException();
        sbyte IConvertible.ToSByte(IFormatProvider? provider) => throw new NotImplementedException();
        byte IConvertible.ToByte(IFormatProvider? provider) => throw new NotImplementedException();
        short IConvertible.ToInt16(IFormatProvider? provider) => throw new NotImplementedException();
        ushort IConvertible.ToUInt16(IFormatProvider? provider) => throw new NotImplementedException();
        int IConvertible.ToInt32(IFormatProvider? provider) => throw new NotImplementedException();
        uint IConvertible.ToUInt32(IFormatProvider? provider) => throw new NotImplementedException();
        long IConvertible.ToInt64(IFormatProvider? provider) => throw new NotImplementedException();
        ulong IConvertible.ToUInt64(IFormatProvider? provider) => throw new NotImplementedException();
        float IConvertible.ToSingle(IFormatProvider? provider) => throw new NotImplementedException();
        double IConvertible.ToDouble(IFormatProvider? provider) => throw new NotImplementedException();
        decimal IConvertible.ToDecimal(IFormatProvider? provider) => throw new NotImplementedException();
        DateTime IConvertible.ToDateTime(IFormatProvider? provider) => throw new NotImplementedException();
        string IConvertible.ToString(IFormatProvider? provider) => throw new NotImplementedException();
        object IConvertible.ToType(Type conversionType, IFormatProvider? provider) => throw new NotImplementedException();

        // Correct operator implementations if necessary
        public static bool operator ==(UInt128 left, UInt128 right) => left.Equals(right);
        public static bool operator !=(UInt128 left, UInt128 right) => !(left == right);
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

