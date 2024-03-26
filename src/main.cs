using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace BrickAbode.UInt128
{

    class Program
    {
        static void TestCarryBitComputation(ulong aLow, ulong bLow)
        {
            ulong lowSum = aLow + bLow;

            // Boolean-less alternative calculation
            ulong carry_bit = ((lowSum ^ aLow) & (lowSum ^ bLow)) >> (int)(sizeof(uint) * 8 - 1);

            Console.WriteLine($"Testing with aLow = {aLow:X}, bLow = {bLow:X}:");
            Console.WriteLine($"    lowSum = {lowSum:X}, carry_bit (alternative) = {carry_bit}");
            Console.WriteLine();
        }

        static int Main(string[] args)
        {
            UInt128 a = new UInt128(1, 2);
            UInt128 b = new UInt128(0, 3);
            ulong x = 0;
            ulong y = 1;
            Console.WriteLine($"C#: (ulong)0 - (ulong)1 = {(ulong)(x-y)}");
            Console.WriteLine($"a({a:X}) == b({b:B})?  {a == b}");
#pragma warning disable CS1718
            Console.WriteLine($"a({a:X}) == a({a:B})?  {a == a}");
#pragma warning restore CS1718

            ulong z = 0; // Test with zero
            ulong notZero = (ulong)((z | (~z + 1)) >> 63 & 1);
            ulong isZero = 1 - notZero;
            Console.WriteLine($"For z = {z}, isZero = {isZero}, notZero = {notZero}");

            z = 18446744073709551615; // Maximum ulong value, definitely not zero
            notZero = (ulong)((z | (~z + 1)) >> 63 & 1);
            isZero = 1 - notZero;
            Console.WriteLine($"For z = {z}, isZero = {isZero}, notZero = {notZero}");

            TestCarryBitComputation(0x00000000FFFFFFFF, 0x0000000000000001); // No carry expected
            TestCarryBitComputation(0xFFFFFFFFFFFFFFFF, 0x0000000000000001); // Carry expected
            TestCarryBitComputation(0x00000001FFFFFFFF, 0x00000000FFFFFFFF); // No carry expected
            TestCarryBitComputation(0x7FFFFFFFFFFFFFFF, 0x7FFFFFFFFFFFFFFF); // Carry expected
            TestCarryBitComputation(0x8000000000000000, 0x8000000000000000); // Carry expected
            TestCarryBitComputation(0x0000000000000000, 0xFFFFFFFFFFFFFFFF); // No carry expected

            Console.WriteLine($"Let's see if we underflow like UL does on subtraction:");
            foreach (var w in new ulong[] {1UL, 2UL})
            {
                Console.WriteLine($"\tulong underflow, 0x1 UL - 0x{w:X} UL = 0x{1UL - w:X} UL");
            }

            var one = new UInt128(0, 1);
            foreach (var ww in new UInt128[] { new UInt128(0, 1), new UInt128(0, 2) })
            {
                Console.WriteLine($"\tulonglong underflow, 0x1 ULL - 0x{ww:X} ULL = 0x{(one - ww):X} ULL");
            }

            return 0;
        }
    }

}
