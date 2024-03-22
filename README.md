# dotnet uint128_t

## Introduction

Implements 128-bit integers for dotnet.  This is generally intended to
operate as an integer type, so notably, they are mutable.  This is
unusual for struct types but normal for integers, which are also
struct types.

## Thoughts on hardware implementations

The initial implementation is pure C#, but these can potentially
be hardware-accelerated.  The developer would then import
`BrickAbode.UInt128_x86` or `BrickAbode.UInt128_arm`, and the UInt128
class would work the same.

| Operation                        | SSE2 (x86/x64)                                              | ARM64                                                       |
|----------------------------------|-------------------------------------------------------------|-------------------------------------------------------------|
| Addition (`+`)                   | `System.Runtime.Intrinsics.X86.Sse2.Add`                    | `System.Runtime.Intrinsics.Arm.Arm64.Add`                   |
| Subtraction (`-`)                | `System.Runtime.Intrinsics.X86.Sse2.Subtract`               | `System.Runtime.Intrinsics.Arm.Arm64.Subtract`              |
| Multiplication (`*`)             | `System.Runtime.Intrinsics.X86.Sse2.MultiplyLow`            | `System.Runtime.Intrinsics.Arm.Arm64.Multiply`              |
| Bitwise AND (`&`)                | `System.Runtime.Intrinsics.X86.Sse2.And`                    | `System.Runtime.Intrinsics.Arm.Arm64.And`                   |
| Bitwise OR (`|`)                 | `System.Runtime.Intrinsics.X86.Sse2.Or`                     | `System.Runtime.Intrinsics.Arm.Arm64.Or`                    |
| Bitwise XOR (`^`)                | `System.Runtime.Intrinsics.X86.Sse2.Xor`                    | `System.Runtime.Intrinsics.Arm.Arm64.Xor`                   |
| Bitwise NOT (`~`)                | `System.Runtime.Intrinsics.X86.Sse2.Not`                    | `System.Runtime.Intrinsics.Arm.Arm64.Not`                   |


## Testing

### High-level test objectives

    - public static UInt128 operator +(UInt128 a, UInt128 b)
    - public static UInt128 operator -(UInt128 a, UInt128 b)
    - public static UInt128 operator *(UInt128 a, UInt128 b)
    - public static UInt128 operator /(UInt128 a, UInt128 b)
    - public static UInt128 operator %(UInt128 a, UInt128 b)
    - public static UInt128 operator +(UInt128 a) => a;
    - public static UInt128 operator -(UInt128 a) => throw new NotImplementedException("Unary negation is not implemented for UInt128 due to its unsigned nature.");
    - public static UInt128 operator ++(UInt128 a)
    - public static UInt128 operator --(UInt128 a)
    - public static UInt128 operator &(UInt128 a, UInt128 b)
    - public static UInt128 operator |(UInt128 a, UInt128 b)
    - public static UInt128 operator ^(UInt128 a, UInt128 b)
    - public static UInt128 operator ~(UInt128 a)
    - public static UInt128 operator <<(UInt128 a, int shift)
    - public static UInt128 operator >>(UInt128 a, int shift)
    - public int CompareTo(object? obj)
    - public int CompareTo(UInt128 other)
    - public bool Equals(UInt128 other)
    - public override bool Equals(object? obj)
    - public static bool operator ==(UInt128 left, UInt128 right) => left.Equals(right);
    - public static bool operator !=(UInt128 left, UInt128 right) => !(left == right);
    - public override int GetHashCode()
    - public override string ToString() => ToString(null, null); // default
    - public string ToString(string? format, IFormatProvider? formatProvider)
    - string IConvertible.ToString(IFormatProvider? provider) => ToString();
    - TypeCode IConvertible.GetTypeCode() => TypeCode.Object;
    - char IConvertible.ToChar(IFormatProvider? provider) => (char)low;
    - DateTime IConvertible.ToDateTime(IFormatProvider? provider)
    - bool IConvertible.ToBoolean(IFormatProvider? provider) => high != 0 || low != 0;
    - byte IConvertible.ToByte(IFormatProvider? provider) => (byte)low;
    - decimal IConvertible.ToDecimal(IFormatProvider? provider) => (decimal)ToBigInteger(this);
    - float IConvertible.ToSingle(IFormatProvider? provider) => (float)ToBigInteger(this);
    - double IConvertible.ToDouble(IFormatProvider? provider) => (double)ToBigInteger(this);
    - short IConvertible.ToInt16(IFormatProvider? provider) => (short)low;
    - sbyte IConvertible.ToSByte(IFormatProvider? provider) => (sbyte)low;
    - int IConvertible.ToInt32(IFormatProvider? provider) => (int)low;
    - long IConvertible.ToInt64(IFormatProvider? provider) => (long)low;
    - ushort IConvertible.ToUInt16(IFormatProvider? provider) => (ushort)low;
    - uint IConvertible.ToUInt32(IFormatProvider? provider) => (uint)low;
    - ulong IConvertible.ToUInt64(IFormatProvider? provider) => low;
    - object IConvertible.ToType(Type conversionType, IFormatProvider? provider)

### Test plan outline

| Test Case Description                   | Method | Input Values                                           | Expected Outcome                                    |
|-----------------------------------------|--------|--------------------------------------------------------|-----------------------------------------------------|
| Add leading to high increment           | `+`    | `a = (1<<63) + 1, b = (1<<63) - 1`                     | `1<<64`                                             |
| Add resulting in max value              | `+`    | `a = UInt128.MaxValue - 1, b = 1`                      | `UInt128.MaxValue`                                  |
| Add overflow (wrap around)              | `+`    | `a = UInt128.MaxValue, b = 1`                          | `0`                                                 |
| Subtract with zero result               | `-`    | `a = 1<<64, b = 1<<64`                                 | `0`                                                 |
| Subtract leading to high decrement      | `-`    | `a = 1<<64, b = 1`                                     | `(1<<64) - 1`                                       |
| Subtract underflow (wrap around)        | `-`    | `a = 0, b = 1`                                         | `UInt128.MaxValue`                                  |
| Multiply leading to high component      | `*`    | `a = 1<<32, b = 1<<32`                                 | `1<<64`                                             |
| Multiply with overflow (wrap around)    | `*`    | `a = (1<<64) - 1, b = 2`                               | `2*(1<<64) - 2` (with potential overflow handling)  |
| Divide with no remainder                | `/`    | `a = 1<<64, b = 1<<32`                                 | `1<<32`                                             |
| Divide with maximum value               | `/`    | `a = UInt128.MaxValue, b = 1`                          | `UInt128.MaxValue`                                  |
| Divide resulting in minimum value       | `/`    | `a = UInt128.MaxValue, b = UInt128.MaxValue`           | `1`                                                 |
| Modulus with no remainder               | `%`    | `a = 1<<64, b = 1<<32`                                 | `0`                                                 |
| Modulus with remainder                  | `%`    | `a = (1<<65) + 1, b = 1<<64`                           | `1`                                                 |
| Unary Plus with max value                | `+`    | `a = UInt128.MaxValue`                                 | `UInt128.MaxValue`                                  |
| Unary Negation not implemented           | `-`    | `a = 123`                                              | `NotImplementedException`                           |
| Increment just below high component      | `++`   | `a = (1<<64) - 1`                                      | `1<<64`                                             |
| Increment at max value (wrap around)     | `++`   | `a = UInt128.MaxValue`                                 | `0`                                                 |
| Decrement from high component            | `--`   | `a = 1<<64`                                            | `(1<<64) - 1`                                       |
| Decrement at zero (wrap around)          | `--`   | `a = 0`                                                | `UInt128.MaxValue`                                  |
| Bitwise AND with differing bits          | `&`    | `a = 0xF0F0F0F0F0F0F0F0, b = 0x0F0F0F0F0F0F0F0F`       | `0`                                                 |
| Bitwise OR with complementary bits       | `|`    | `a = 0xF0F0F0F0F0F0F0F0, b = 0x0F0F0F0F0F0F0F0F`       | `0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF`               |
| Bitwise XOR with identical values        | `^`    | `a = 0xAAAAAAAABBBBBBBB, b = 0xAAAAAAAABBBBBBBB`        | `0`                                                 |
| Bitwise NOT on max value                 | `~`    | `a = UInt128.MaxValue`                                 | `0`                                                 |
| Left shift across boundary               | `<<`   | `a = 1, shift = 64`                                    | `1<<64`                                             |
| Right shift across boundary              | `>>`   | `a = 1<<64, shift = 64`                                | `1`                                                 |
| CompareTo identical values               | `CompareTo` | `a = UInt128.MaxValue, obj = UInt128.MaxValue` | `0`                      |
| CompareTo less than                      | `CompareTo` | `a = 0, other = UInt128.MaxValue`        | `<0` (negative value)        |
| Equals with identical objects            | `Equals` | `a = UInt128.MaxValue, obj = UInt128.MaxValue` | `true`                   |
| Operator equality on identical values    | `==`   | `left = UInt128.MaxValue, right = UInt128.MaxValue` | `true`                   |
| Operator inequality on different values  | `!=`   | `left = UInt128.MaxValue, right = 0`    | `true`                        |
| Convert to byte with truncation          | `IConvertible.ToByte`        | `a = 256`        | `0` (or specific handling of truncation)            |
| Convert to DateTime from Unix epoch      | `IConvertible.ToDateTime`    | `a = 0`          | `DateTime` corresponding to Unix epoch              |
| Convert to bool from zero                | `IConvertible.ToBoolean`     | `a = 0`          | `false`                                             |
| Convert to bool from non-zero            | `IConvertible.ToBoolean`     | `a = 123`        | `true`                                              |
| Convert to double with large value       | `IConvertible.ToDouble`      | `a = (1<<64)`    | Equivalent `double` value                           |
| Convert to BigInteger with max value     | `ToBigInteger`               | `a = UInt128.MaxValue` | `BigInteger` equivalent to `UInt128.MaxValue`  |
| Convert to unsupported type              | `IConvertible.ToType`        | `a = 123, type = typeof(SomeUnsupportedType)` | `InvalidCastException` |
| Hash code consistency                   | `GetHashCode`                | `a = UInt128.MaxValue`      | Consistent hash code for equal values        |
| String representation for zero          | `ToString`                   | `a = 0`                     | `"0"`                                         |
| Parse method validity with max value    | `Parse` (if applicable)      | `str = "UInt128.MaxValue"`  | `UInt128.MaxValue`                            |
| Parse method error on invalid input     | `Parse` (if applicable)      | `str = "invalid"`           | Throw format exception                        |
| TypeCode verification                   | `IConvertible.GetTypeCode`   | `a = any UInt128 value`     | `TypeCode.Object`                             |
| Conversion to unsupported type          | `IConvertible.ToType`        | `conversionType = typeof(UnsupportedType)` | `InvalidCastException` |
| Convert to char with truncation         | `IConvertible.ToChar`        | `a = 256`                   | Resulting char or exception on invalid conversion |
| Convert to DateTime beyond range        | `IConvertible.ToDateTime`    | `a = UInt128.MaxValue`      | Exception due to out-of-range value          |
| Conversion to double precision loss     | `IConvertible.ToDouble`      | `a = (1<<100)`              | Double value with precision loss             |
| Convert to sbyte with truncation        | `IConvertible.ToSByte`       | `a = 256`                   | Truncated value or exception                  |
| Increment from max low to min high       | `++`                            | `a = (1UL << 63) - 1`                        | `1UL << 63`                             |
| Decrement across high-low boundary       | `--`                            | `a = 1UL << 63`                              | `(1UL << 63) - 1`                       |
| Bitwise AND with zero                    | `&`                             | `a = UInt128.MaxValue, b = 0`                | `0`                                     |
| Bitwise OR to max value                  | `|`                             | `a = 0, b = UInt128.MaxValue`                | `UInt128.MaxValue`                      |
| Bitwise XOR self to zero                 | `^`                             | `a = UInt128.MaxValue, b = UInt128.MaxValue` | `0`                                     |
| Bitwise NOT to invert                    | `~`                             | `a = 0`                                      | `UInt128.MaxValue`                      |
| Left shift by zero                       | `<<`                            | `a = 1, shift = 0`                           | `1`                                     |
| Right shift by zero                      | `>>`                            | `a = 1 << 64, shift = 0`                     | `1 << 64`                               |
| Left shift by max bits                   | `<<`                            | `a = 1, shift = 127`                         | `1UL << 127`                            |
| Right shift by max bits                  | `>>`                            | `a = 1UL << 127, shift = 127`                | `1`                                     |
| Compare identical values                 | `CompareTo(UInt128)`           | `a = UInt128.MaxValue, other = UInt128.MaxValue` | `0`                          |
| Compare different values                 | `CompareTo(UInt128)`           | `a = 1, other = 2`                           | `<0`                                    |
| Equality of identical values             | `Equals(UInt128)`              | `a = UInt128.MaxValue, other = UInt128.MaxValue` | `true`                         |
| Inequality of different values           | `!=`                            | `left = 1, right = 2`                        | `true`                                  |
| Convert to byte without loss             | `IConvertible.ToByte`          | `a = 255`                                    | `255`                                   |
| Convert to decimal large value           | `IConvertible.ToDecimal`       | `a = UInt128.MaxValue`                       | Decimal equivalent of `UInt128.MaxValue`|
| Convert to float with precision loss     | `IConvertible.ToSingle`        | `a = UInt128.MaxValue`                       | Float with precision loss               |
| Convert to double with precision loss    | `IConvertible.ToDouble`        | `a = UInt128.MaxValue`                       | Double with precision loss              |
| Convert to int without loss              | `IConvertible.ToInt32`         | `a = 2147483647`                             | `2147483647`                            |
| Convert to int with overflow             | `IConvertible.ToInt32`         | `a = UInt128.MaxValue`                       | Exception or truncated value            |
| Convert to ulong without loss            | `IConvertible.ToUInt64`        | `a = UInt64.MaxValue`                        | `UInt64.MaxValue`                       |
| Convert to unsupported type              | `IConvertible.ToType`          | `a = 123, conversionType = typeof(DateTime)` | `InvalidCastException`                  |
| Convert to DateTime within range         | `IConvertible.ToDateTime`      | `a = 621355968000000000` (Unix epoch)        | `DateTime` equivalent to Unix epoch     |
| Convert to bool from maximum value       | `IConvertible.ToBoolean`       | `a = UInt128.MaxValue`                       | `true`                                  |
| Convert to char from single digit        | `IConvertible.ToChar`          | `a = 65`                                     | `'A'`                                   |
| Conversion leads to precision loss in decimal | `IConvertible.ToDecimal` | `a = (1UL << 127) + (1UL << 126)`           | Decimal with potential precision loss  |
| Convert to short with overflow           | `IConvertible.ToInt16`         | `a = 32768`                                  | Exception or truncated value            |
| Convert to sbyte with overflow           | `IConvertible.ToSByte`         | `a = 128`                                    | Exception or truncated value            |
| Convert to int16 without loss            | `IConvertible.ToInt16`         | `a = 32767`                                  | `32767`                                 |
| Convert to int64 from high value         | `IConvertible.ToInt64`         | `a = 1UL << 63`                              | `1UL << 63`                             |
| Convert to uint16 without loss           | `IConvertible.ToUInt16`        | `a = 65535`                                  | `65535`                                 |
| Convert to uint32 without loss           | `IConvertible.ToUInt32`        | `a = UInt32.MaxValue`                        | `UInt32.MaxValue`                       |
| Convert from high boundary to ulong      | `IConvertible.ToUInt64`        | `a = UInt64.MaxValue`                        | `UInt64.MaxValue`                       |
| Convert to BigInteger without loss       | Custom conversion to `BigInteger` | `a = UInt128.MaxValue`                | `BigInteger` equivalent of `UInt128.MaxValue` |
| Convert to type, expecting same instance | `IConvertible.ToType`          | `a = 123, conversionType = typeof(UInt128)`  | Instance of `UInt128` with value `123`  |
| Convert to float from zero value         | `IConvertible.ToSingle`        | `a = 0`                                      | `0.0f`                                  |
| Convert to double from zero value        | `IConvertible.ToDouble`        | `a = 0`                                      | `0.0`                                   |
| Convert to ulong max without loss        | `IConvertible.ToUInt64`        | `a = UInt64.MaxValue`                        | `UInt64.MaxValue`                       |
| GetHashCode consistency with same value  | `GetHashCode`                  | `a = UInt128.MaxValue, b = UInt128.MaxValue` | Same hash code for both                 |
| GetHashCode difference for distinct values | `GetHashCode`                | `a = 0, b = 1`                               | Different hash codes                    |
| ToString default format                   | `ToString`                     | `a = 123`                                    | `"123"`                                 |
| ToString with format specifier           | `ToString(string, IFormatProvider)` | `a = 123, format = "X"`               | Hexadecimal string representation       |

