using System;
using System.Numerics;
using Xunit;
using Xunit.Abstractions;
using BrickAbode.UInt128;
using BrickAbode.UInt128.Tests;


namespace BrickAbode.UInt128.Tests
{
    public class TestLogger
    {
        private readonly ITestOutputHelper outputHelper;
        private readonly LogLevel currentLogLevel;

        public TestLogger(ITestOutputHelper outputHelper)
        {
            this.outputHelper = outputHelper;
            var envLogLevel = Environment.GetEnvironmentVariable("XUNIT_DEBUG");
            currentLogLevel = LogLevel.Warning;
            if (!string.IsNullOrWhiteSpace(envLogLevel))
            {
                if(Enum.TryParse(envLogLevel, true, out LogLevel parsedLevel))
                {
                    currentLogLevel = parsedLevel;
                }
            }
        }

        public enum LogLevel
        {
            All = 0,
            Debug = 1,
            Info = 2,
            Warning = 3,
            Error = 4,
            Critical = 5,
            Off = 1<<16 - 1,
        }

        private void Log(LogLevel logLevel, string message)
        {
            if (logLevel >= currentLogLevel && outputHelper != null)
            {
                Console.WriteLine($"LOG({logLevel}/{currentLogLevel}/{outputHelper}): {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} [{logLevel}] {message}");
                // outputHelper.WriteLine($"LOG {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} [{logLevel}] {message}");
            }
        }

        public void Debug(string message) => Log(LogLevel.Debug, message);
        public void Info(string message) => Log(LogLevel.Info, message);
        public void Warning(string message) => Log(LogLevel.Warning, message);
        public void Error(string message) => Log(LogLevel.Error, message);
        public void Critical(string message) => Log(LogLevel.Critical, message);

    }

    public class UInt128TestHelper
    {
        private TestLogger logger;
        public UInt128TestHelper(TestLogger tlogger)
        {
            logger = tlogger;
        }

        public void AssertOperation(UInt128 a, UInt128 b, Operation operation, int shiftAmount = 0)
        {
            // Use the existing ToBigInteger conversion method
            var bigA = UInt128.ToBigInteger(a);
            var bigB = UInt128.ToBigInteger(b);
            var sub_amt = bigA >= bigB ? bigA - bigB : BigInteger.Pow(2, 128) - (bigB - bigA); // Handle underflow

            if((operation == Operation.Divide) && (b==0)) // we just silently skip these
            {
                return;
            }

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
                    logger.Error($"FAILURE: UInt128 (~{a:X} = {result:X}) != BigInteger(~{bigA:X} = {expectedBig:X})");
                    throw new InvalidOperationException($"Assertion Failed: Expected result was {expected}, but got ~{a} = {result}.");
                }
                logger.Error($"FAILURE: UInt128 ({a:X} {op} {b:X} = {result:X}) != BigInteger({bigA:X} {op} {bigB:X} = {expectedBig:X})");
                throw new InvalidOperationException($"Assertion Failed: Expected result was {expected}, but got {a} {op} {b} = {result}.");
            }
            logger.Debug($"SUCCESS: UInt128 (0x{a:X} {op} 0x{b:X} = 0x{result:X}) == BigInteger(0x{bigA:X} {op} 0x{bigB:X} = 0x{expectedBig:X})");
        }


        public void TestOperation(UInt128TestHelper.Operation operation, int? shiftAmount = null)
        {
            foreach (var a in testValues)
            {
                foreach (var b in testValues)
                {
                    if ( (operation == UInt128TestHelper.Operation.RightShift) || (operation == UInt128TestHelper.Operation.LeftShift))
                    {
                        foreach (var shiftAmt in shiftAmounts)
                        {
                            AssertOperation(a, b, operation, shiftAmt);
                        }
                    }
                    else
                    {
                        AssertOperation(a, b, operation);
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
        public readonly UInt128[] testValues = new UInt128[]
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

            new UInt128(0, (1UL<<63)),          // 2**63
            new UInt128(0, ulong.MaxValue - 1), // Mid - 2
            new UInt128(0, ulong.MaxValue),     // Mid - 1
            new UInt128(1, 0),                  // Mid (2**64)
            new UInt128(1, 1),                  // Near mid
            new UInt128(1, 2),                  // Near mid
            new UInt128(2, 0),                  // 2**65

            new UInt128((1UL<<31), 0),                    // 2**95
            new UInt128((1UL<<31), 1),                    // 2**95 + 1
            new UInt128((1UL<<31), 2),                    // 2**95 + 2
            new UInt128((1UL<<32)-1, 0xFFFFFFFFFFFFFFFF), // 2**96 - 1
            new UInt128((1UL<<32)-1, 0xFFFFFFFFFFFFFFFE), // 2**96 - 2
            new UInt128((1UL<<32), 0),                    // 2**96
            new UInt128((1UL<<32), 1),                    // 2**96 + 1
            new UInt128((1UL<<32), 2),                    // 2**96 + 2
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

        public readonly int[] shiftAmounts = new int[] { 0, 1, 7, 8, 9, 63, 64, 65, 119, 127, 128 };

    }

    public class UInt128TestBase
    {
        public TestLogger logger;
        public UInt128TestHelper helper;
        public UInt128TestBase(ITestOutputHelper output)
        {
            logger = new TestLogger(output); // Initialize the logger with an ITestOutputHelper instance
            helper = new UInt128TestHelper(logger); // Initialize the logger with an ITestOutputHelper instance
            logger.Debug($"Starting test run: {this.GetType().Name}");
        }
    }
}
