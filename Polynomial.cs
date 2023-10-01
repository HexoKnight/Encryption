using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Encryption
{
    public class Polynomial
    {
        public IEnumerable<BigInteger> coefficients;

        public BigInteger prime;

        public Polynomial(IEnumerable<BigInteger> coefficients, BigInteger prime)
        {
            if (coefficients.Any(num => num.Sign < 0)) throw new ArgumentOutOfRangeException("coefficients cannot be negative");
            this.coefficients = coefficients;
            this.prime = prime;
        }

        public BigInteger Evaluate(BigInteger num)
        {
            //BigInteger result = 0;
            //int i = 0;
            //foreach (BigInteger coefficient in coefficients)
            //{
            //    result += (coefficient * BigInteger.ModPow(num, i++, prime)) % prime;
            //}
            //return result % prime;
            return coefficients.Select((coefficient, i) => (coefficient * BigInteger.ModPow(num, i, prime)) % prime).Aggregate((a, b) => a + b) % prime;
        }

        public static BigInteger Lagrange(BigInteger x, IEnumerable<KeyValuePair<BigInteger, BigInteger>> keys, BigInteger prime)
        {
            if (keys.Any(pair => pair.Key.Sign < 0 || pair.Value.Sign < 0)) throw new ArgumentOutOfRangeException("keys cannot be negative");

            BigInteger result = 0;
            foreach (KeyValuePair<BigInteger, BigInteger> key1 in keys)
            {
                BigInteger l/*product*/ = 1; // lagrange basis polynomial
                foreach (KeyValuePair<BigInteger, BigInteger> key2 in keys)
                {
                    if (key1.Key == key2.Key) continue;
                    l *= ((x - key2.Key).Mod(prime) * (key1.Key - key2.Key).ModularInverse(prime)) % prime;
                }
                result += (l * key1.Value) % prime;
            }
            return result % prime;
        }

        // https://math.stackexchange.com/questions/944465/coefficients-of-lagrange-polynomials
        // https://stackoverflow.com/questions/9860937/how-to-calculate-coefficients-of-polynomial-using-lagrange-interpolation
        // https://math.stackexchange.com/questions/384470/lagrange-interpolation-polynomial-code-for-coefficients
        // https://stackoverflow.com/questions/14536927/function-interpolation-c-sharp
    }
}
