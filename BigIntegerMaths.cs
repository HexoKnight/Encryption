using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Encryption
{
    public static class BigIntegerMaths
    {
        public static BigInteger IntPow(BigInteger x, BigInteger pow)
        {
            BigInteger result = 1;
            while (pow != 0)
            {
                if ((pow & 1) == 1) result *= x;
                x *= x;
                pow >>= 1;
            }
            return result;
        }

        #region primes
        public static SortedSet<ulong> primes = new SortedSet<ulong>();
        public static readonly ulong min_prime = 50000000;

        public static BigInteger NextPrime(this BigInteger num)
        {
            if (num.Sign < 0) throw new ArgumentOutOfRangeException("num cannot be negative");
            if (num < ulong.MaxValue && num >= min_prime - 1)// && (num < max_prime))
            {
                ulong prime = primes.GetViewBetween((ulong)(num + 1), ulong.MaxValue).FirstOrDefault();
                if (prime != default) return prime;
            }

            int mod = (int)(num % 6);
            if (mod == 5)
            {
                if (IsPrime(num + 2, true)) return num + 2;
                num += 7;
            }
            else
            {
                if (mod == 0 && IsPrime(num + 1, true)) return num + 1;
                num += 6 - mod;
            }

            while (true)
            {
                BigInteger numminusone = default;
                BigInteger numplusone = default;
                Task<bool> oneless = Task.Run(() => IsPrime(numminusone = num - 1, true));
                Task<bool> onemore = Task.Run(() => IsPrime(numplusone = num + 1, true));

                num += 6;
                oneless.Wait();
                if (oneless.Result) return numminusone; // num - 1;
                onemore.Wait();
                if (onemore.Result) return numplusone; // num + 1;
            }
        }

        public static bool IsPrime(BigInteger num, bool fromNextPrime = false)
        {
            if (num.Sign < 0) throw new ArgumentOutOfRangeException("num cannot be negative");
            if (num <= ulong.MaxValue && num >= min_prime)
            {
                bool contains = primes.Contains((ulong)num);
                if (contains) return true;
            }

            if (fromNextPrime)
            {
                if (num == 2 || num == 3) return true;
                if (num.IsEven || num % 3 == 0) return false;
            }

            BigInteger divisor = 5;
            BigInteger sqrt = SqrtFast(num);
            while (divisor <= sqrt)
            {
                if ((num % divisor).IsZero)
                {
                    return false;
                }
                divisor += 2;
                if ((num % divisor).IsZero)
                {
                    return false;
                }
                divisor += 4;
            }
            if (num <= ulong.MaxValue && num >= min_prime)
            {
                lock (primes) { primes.Add((ulong)num); }
            }
            return true;
        }
        #endregion

        #region fast square root
        private static readonly BigInteger FastSqrtSmallNumber = 4503599761588223UL; // as static readonly = reduce compare overhead

        public static BigInteger SqrtFast(BigInteger value)
        {
            if (value <= FastSqrtSmallNumber) // small enough for Math.Sqrt() or negative?
            {
                if (value.Sign < 0) throw new ArgumentException("Negative argument.");
                return (ulong)Math.Sqrt((ulong)value);
            }

            BigInteger root; // now filled with an approximate value
            int byteLen = value.ToByteArray().Length;
            if (byteLen < 128) // small enough for direct double conversion?
            {
                root = (BigInteger)Math.Sqrt((double)value);
            }
            else // large: reduce with bitshifting, then convert to double (and back)
            {
                root = (BigInteger)Math.Sqrt((double)(value >> (byteLen - 127) * 8)) << (byteLen - 127) * 4;
            }

            for (; ; )
            {
                BigInteger root2 = value / root + root >> 1;
                if ((root2 == root || root2 == root + 1) && IsSqrt(value, root)) return root;
                root = value / root2 + root2 >> 1;
                if ((root == root2 || root == root2 + 1) && IsSqrt(value, root2)) return root2;
            }
        }

        public static bool IsSqrt(BigInteger value, BigInteger root)
        {
            var lowerBound = root * root;

            return value >= lowerBound && value <= lowerBound + (root << 1);
        }
        #endregion

        public static BigInteger Mod(this BigInteger dividend, BigInteger divisor) => dividend.Sign < 0 ? divisor + dividend % divisor : dividend % divisor;

        public static BigInteger ModularInverse(this BigInteger num, BigInteger prime)
            // https://en.wikipedia.org/wiki/Extended_Euclidean_algorithm#Computing_multiplicative_inverses_in_modular_structures
        {
            num = num.Mod(prime);
            BigInteger old_s = 1, s = 0;
            BigInteger old_r = num, r = prime;
            while (!r.IsZero)
            {
                BigInteger q = old_r / r; // quotient

                //BigInteger new_r = old_r - q * r;
                //old_r = r;
                //r = new_r;
                (old_r, r) = (r, old_r - q * r);

                //BigInteger new_s = old_s - q * s;
                //old_s = s;
                //s = new_s;
                (old_s, s) = (s, old_s - q * s);
            }
            return old_s < 0 ? prime + old_s : old_s;
        }

        public static int GetBitlength(this int num)
        {
            int size = 0;
            for (; num != 0; num >>= 1) size++;
            return size;
        }
        public static int GetBitlength(this BigInteger num)
        {
            int size = 0;
            for (; num != 0; num >>= 1) size++;
            return size;
        }
    }
}
