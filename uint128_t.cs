using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace BrickAbode.Int128
{

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    [ComVisible(true)]
    public struct UInt128 : IComparable, IFormattable, IConvertible,
                             IComparable<UInt128>, IEquatable<UInt128>
    {
        private ulong high;
        private ulong low;

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

        private static BigInteger ToBigInteger(UInt128 value)
        {
            return new BigInteger(value.low) + (new BigInteger(value.high) << 64);
        }

        // Helper method to convert BigInteger to UInt128
        private static UInt128 FromBigInteger(BigInteger value)
        {
            byte[] bytes = value.ToByteArray();
            ulong low = BitConverter.ToUInt64(bytes, 0);
            ulong high = bytes.Length > 8 ? BitConverter.ToUInt64(bytes, 8) : 0;
            return new UInt128(high, low);
        }

        public static UInt128 operator /(UInt128 a, UInt128 b)
        {
            BigInteger bigA = ToBigInteger(a);
            BigInteger bigB = ToBigInteger(b);
            BigInteger result = bigA / bigB;
            return FromBigInteger(result);
        }

        public static UInt128 operator %(UInt128 a, UInt128 b)
        {
            BigInteger bigA = ToBigInteger(a);
            BigInteger bigB = ToBigInteger(b);
            BigInteger result = bigA % bigB;
            return FromBigInteger(result);
        }

        // Unary negation and plus
        public static UInt128 operator -(UInt128 a) => throw new NotImplementedException();
        public static UInt128 operator +(UInt128 a) => a;  // Unary plus does not change the value

        // Increment and decrement
        public static UInt128 operator ++(UInt128 a)
        {
            a.low++;
            if (a.low == 0)
            {
                a.high++;
            }
            return a;
        }

        public static UInt128 operator --(UInt128 a)
        {
            if (a.low == 0)
            {
                a.high--;
            }
            a.low--;
            return a;
        }

        // Bitwise operators
        public static UInt128 operator &(UInt128 a, UInt128 b)
        {
            ulong h = a.high & b.high;
            ulong l = a.low & b.low;
            return new UInt128(h, l);
        }

        public static UInt128 operator |(UInt128 a, UInt128 b)
        {
            ulong h = a.high | b.high;
            ulong l = a.low | b.low;
            return new UInt128(h, l);
        }

        public static UInt128 operator ^(UInt128 a, UInt128 b)
        {
            ulong h = a.high ^ b.high;
            ulong l = a.low ^ b.low;
            return new UInt128(h, l);
        }

        public static UInt128 operator ~(UInt128 a)
        {
            ulong h = ~a.high;
            ulong l = ~a.low;
            return new UInt128(h, l);
        }

        // Shift operators
        // FIXME: both of these should be fixed to the mutable style
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

        // IComparable.CompareTo(object? obj)
        public int CompareTo(object? obj)
        {
            if (obj == null) return 1; // Consider null to be less than any instance of UInt128

            if (!(obj is UInt128))
                throw new ArgumentException("Object must be of type UInt128.", nameof(obj));

            return CompareTo((UInt128)obj);
        }

        public int CompareTo(UInt128 other)
        {
            if (high > other.high) return 1;
            if (high < other.high) return -1;

            if (low > other.low) return 1;
            if (low < other.low) return -1;

            return 0;
        }

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
        TypeCode IConvertible.GetTypeCode() => TypeCode.Object;
        char IConvertible.ToChar(IFormatProvider? provider) => (char)low;


        DateTime IConvertible.ToDateTime(IFormatProvider? provider)
        {
            if ((high != 0) || (low > long.MaxValue))
            {
                throw new InvalidCastException("UInt128 contains a value that cannot be represented as a DateTime.");
            }

            return new DateTime((long)low);
        }

        // IConvertible implementations
        bool IConvertible.ToBoolean(IFormatProvider? provider) => high != 0 || low != 0;
        byte IConvertible.ToByte(IFormatProvider? provider) => (byte)low;

        // Other conversions...
        decimal IConvertible.ToDecimal(IFormatProvider? provider) => (decimal)ToBigInteger(this);
        float IConvertible.ToSingle(IFormatProvider? provider) => (float)ToBigInteger(this);
        double IConvertible.ToDouble(IFormatProvider? provider) => (double)ToBigInteger(this);

        sbyte IConvertible.ToSByte(IFormatProvider? provider) => (sbyte)low;
        string IConvertible.ToString(IFormatProvider? provider) => ToString();

        short IConvertible.ToInt16(IFormatProvider? provider) => (short)low;
        int IConvertible.ToInt32(IFormatProvider? provider) => (int)low;
        long IConvertible.ToInt64(IFormatProvider? provider) => (long)low;

        ushort IConvertible.ToUInt16(IFormatProvider? provider) => (ushort)low;
        uint IConvertible.ToUInt32(IFormatProvider? provider) => (uint)low;
        ulong IConvertible.ToUInt64(IFormatProvider? provider) => low;

        object IConvertible.ToType(Type conversionType, IFormatProvider? provider)
        {
            if (conversionType == typeof(BigInteger))
                return ToBigInteger(this);
            throw new InvalidCastException($"Cannot convert to {conversionType.FullName}");
        }

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

