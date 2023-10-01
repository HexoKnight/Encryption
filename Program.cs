using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Numerics;
using System.IO;

namespace Encryption
{
    class Program
    {
        public const int fullprime = 97;

        public static RandomBigInteger random = new RandomBigInteger();

        public static readonly string[] helpCommand = new string[] { "help", "h", "H", "-help", "-h", "-H", "/help", "/h", "/H" };

        static void Main(string[] args)
        {
            if (args.Length == 0) UI();
            else if (helpCommand.Contains(args[0])) Help();
            else if (args[0] == "ENCRYPT") Encrypt(args.Skip(1));
            else if (args[0] == "DECRYPT") Decrypt(args.Skip(1));

            //for (int i = 0; i < args.Length; i++) Console.WriteLine(args[i]);
            Console.WriteLine("done");
            return;
        }

        #region help
        public static void Help()
        {
            Console.WriteLine("A program that encrypts and decrypts text that can be written with the average QWERTY UK keyboard using Shamir's secret sharing algorithm"
                     + "\n" + ""
                     + "\n" + ""
                     + "\n" + "COMMAND : ENCRYPT"
                     + "\n" + "Encrypts text using Shamir's Secret Sharing"
                     + "\n" + ""
                     + "\n" + "USAGE : Encryption.exe ENCRYPT -S:{secret} [/F] -R:{keys_required} -K:{keys_generated}"
                     + "\n" + "                               [-E:{extra1}[;{extra2}][;{extra3}]...] [-D:{destinationdir}[:{fileprefix}]]"
                     + "\n" + ""
                     + "\n" + "[] : optional    ,    {} : to be replaced"
                     + "\n" + ""
                     + "\n" + "secret :            Indicates what text should be encrypted,"
                     + "\n" + "                    eg. Encryption.exe ENCRYPT -S:\"foobar\" ..."
                     + "\n" + "/F :                Indicates that the source is a file path,"
                     + "\n" + "                    eg. Encryption.exe ENCRYPT /F -S:\"C:/path/file.txt\" ..."
                     + "\n" + "keys_required :     Indicates how many keys should be required to decrypt the secret,"
                     + "\n" + "                    eg. Encryption.exe ENCRYPT ... -R:5 ..."
                     + "\n" + "keys_generated :    Indicates how many keys should be generated,"
                     + "\n" + "                    eg. Encryption.exe ENCRYPT ... -K:10 ..."
                     + "\n" + "extra1, extra2... : Indicates (keys_required - 1) manually chosen extra integers"
                     + "\n" + "                    (which are otherwise randomly generated),"
                     + "\n" + "                    eg. Encryption.exe ENCRYPT  ... -E:134;378;127 ..."
                     + "\n" + "destinationdir :    Indicates where the keys will be outputted (printed in console if -D ommited"
                     + "\n" + "                    and outputted in current directory if destinationdir omitted),"
                     + "\n" + "                    eg. Encryption.exe ENCRYPT  ... -D:\"C:/path/to/directory/\" ..."
                     + "\n" + "fileprefix :        Indicates what the files will be prefixed with,"
                     + "\n" + "                    eg. Encryption.exe ENCRYPT ... -D:\"...\":\"key_\" ..."
                     + "\n" + ""
                     + "\n" + "For Example:"
                     + "\n" + "Encryption.exe ENCRYPT -S:\"a_big_secret\" -R:10 -K:20"
                     + "\n" + "Encryption.exe ENCRYPT -S:\"a_big_secret\" -R:10 -K:100 -D:\"C:/destination/\""
                     + "\n" + "Encryption.exe ENCRYPT -S:\"C:/path/to/secret.txt\" /F -R:5 -K:30"
                     + "\n" + "                       -E:2637;173468;124364;50485 -D:\"C:/path/to/destination/\":\"key_\""
                     + "\n" + ""
                     + "\n" + ""
                     + "\n" + ""
                     + "\n" + "COMMAND : DECRYPT"
                     + "\n" + "Decrypts keys using Shamir's Secret Sharing"
                     + "\n" + ""
                     + "\n" + "USAGE : Encryption.exe DECRYPT (-K:{key1},{value1}[;{key2},{value2}][;{key3},{value3}]... | -KD:{keysdirectory})"
                     + "\n" + "                               [-SD:{secretdestination}]"
                     + "\n" + ""
                     + "\n" + "(x | y) : compulsory either or    ,    [] : optional    ,    {} : to be replaced"
                     + "\n" + ""
                     + "\n" + "key1,value1, key2,value2... : Indicates what keys and values should be used to decrypt the secret,"
                     + "\n" + "                              eg. Encryption.exe DECRYPT -K:3,\"t9ks¬\";1,\"lW+o4\" ..."
                     + "\n" + "keysdirectory :               Indicates what directory the key files (ending '.key') are in"
                     + "\n" + "                              (uses current directory if keysdirectory omitted),"
                     + "\n" + "                              eg. Encryption.exe DECRYPT -KD:\"C:/path/to/keys/\" ..."
                     + "\n" + "secretdestination :           Indicates where the secret will be outputted (printed in console if -SD ommited),"
                     + "\n" + "                              eg. Encryption.exe DECRYPT ... -SD:\"C:/path/to/secret.txt\" ..."
                     + "\n" + ""
                     + "\n" + "For Example:"
                     + "\n" + "Encryption.exe DECRYPT -K:3,\"t9ks¬\";1,\"lW+o4\""
                     + "\n" + "Encryption.exe DECRYPT -KD:\"C:/path/to/keys/\""
                     + "\n" + "Encryption.exe DECRYPT -KD:\"C:/path/to/keys/\" -SD:\"C:/path/to/secret.txt\""
                     );
        }
        #endregion

        #region Encrypt
        public static void Encrypt(IEnumerable<string> args)
        {
            string stringsecret = null;
            bool pathsecret = false;
            int? requiredkeys = null, numkeys = null;
            BigInteger[] extra = null;
            string destinationdir = null, fileprefix = null;
            foreach (string arg in args)
            {
                string str = arg.Trim();
                if (str.StartsWith("-S:"))
                {
                    str = str.Substring(3);
                    if (string.IsNullOrEmpty(str)) throw new ArgumentException("secret must exist");
                    if (str[0] == '"' && str[str.Length - 1] == '"') str = str.Substring(1, str.Length - 2);
                    stringsecret = str;
                }
                else if (str == "/F") pathsecret = true;
                else if (str.StartsWith("-R:"))
                {
                    if (!int.TryParse(str.Substring(3), out int tempint)) throw new ArgumentException("keys_required must be an integer");
                    requiredkeys = tempint;
                    if (extra != null && extra.Length != requiredkeys - 1) throw new ArgumentException("there must be (keys_required - 1) extras");
                }
                else if (str.StartsWith("-K:"))
                {
                    if (!int.TryParse(str.Substring(3), out int tempint)) throw new ArgumentException("keys_generated must be an integer");
                    numkeys = tempint;
                }
                else if (str.StartsWith("-E:"))
                {
                    string[] stringextra = str.Substring(3).Split(';');
                    if (requiredkeys.HasValue && stringextra.Length != requiredkeys.Value - 1) throw new ArgumentException("there must be (keys_required - 1) extras");
                    extra = stringextra.Select(strextra => BigInteger.Parse(strextra)).ToArray();
                }
                else if (str.StartsWith("-D:"))
                {
                    if (str.Length > 3)
                    {
                        string[] parts = str.Substring(3).Split(':');
                        if (parts.Length > 2) throw new ArgumentException("what?  -  use 'Encryption.exe help'");

                        if (parts[0].Length >= 2 && parts[0][0] == '"' && parts[0][parts[0].Length - 1] == '"') destinationdir = parts[0].Substring(1, parts[0].Length - 2);
                        else destinationdir = parts[0];

                        if (parts.Length == 2)
                        {
                            if (parts[1].Length >= 2 && parts[1][0] == '"' && parts[1][parts[1].Length - 1] == '"') fileprefix = parts[1].Substring(1, parts[1].Length - 2);
                            else fileprefix = parts[1];
                        }
                    }
                    else destinationdir = "";
                }
            }
            if (stringsecret == null || !requiredkeys.HasValue || !numkeys.HasValue)
                throw new ArgumentException("-S:{secret}, -R:{keys_required}, -K:{key_generated} are all required  -  use 'Encryption.exe help'");

            if (pathsecret)
            {
                if (!File.Exists(stringsecret)) throw new ArgumentException("secret file must exist");
                stringsecret = File.ReadAllText(stringsecret);
            }
            if (!stringsecret.All(chr => IsValidChar(chr))) throw new ArgumentException("secret must only contain valid characters");
            if (destinationdir != null)
            {
                if (string.IsNullOrEmpty(destinationdir)) destinationdir = Directory.GetCurrentDirectory();
                else if (!Directory.Exists(destinationdir)) throw new ArgumentException("destinationdir path must exist");
            }

            BigInteger prime = PerfectPrime(stringsecret);

            //if (extra == null)
            //{
            //    extra = new BigInteger[requiredkeys.Value];
            //    for (int i = 1; i < extra.Length; i++) extra[i] = random.NextBigInteger(0, prime);
            //}

            Polynomial polynomial = new Polynomial((extra ?? Enumerable.Repeat(0, requiredkeys.Value - 1).Select(_ => random.NextBigInteger(1, prime)).ToArray()).Prepend(ToFullInt(stringsecret)), prime);
            Dictionary<int, BigInteger> points = new Dictionary<int, BigInteger>();
            for (int i = 1; i < numkeys + 1; i++) points.Add(i, polynomial.Evaluate(i));
            if (destinationdir == null)
            {
                int fill = (int)Math.Floor(Math.Log10(numkeys.Value)) + 1;
                foreach (KeyValuePair<int, BigInteger> keyValuePair in points)
                    Console.WriteLine($"{keyValuePair.Key.ToString().PadLeft(fill)} : '{ToString(keyValuePair.Value)}'");
            }
            else
            {
                foreach (KeyValuePair<int, BigInteger> keyValuePair in points)
                    using (StreamWriter writer = File.CreateText(Path.Combine(destinationdir, (fileprefix ?? string.Empty) + keyValuePair.Key + ".key")))
                        writer.Write(keyValuePair.Key + "\n" + ToString(keyValuePair.Value));
            }
        }
        #endregion

        #region Decrypt
        public static void Decrypt(IEnumerable<string> args)
        {
            string[][] stringkeys = null;
            string keysdirectory = null, secretdestination = null;
            foreach (string arg in args)
            {
                string str = arg.Trim();
                if (str.StartsWith("-K:"))
                {
                    if (keysdirectory != null) throw new ArgumentException("there cannot be both keys and a keysdirectory");
                    str = str.Substring(3);
                    string[][] stringkeysarray = str.Split(';').Select(key => key.Split(':')).ToArray();
                    if (stringkeysarray.Any(strs => strs.Length != 2)) throw new ArgumentException("keys and values must come in pairs");
                }
                if (str.StartsWith("-KD:"))
                {
                    if (stringkeys != null) throw new ArgumentException("there cannot be both keys and a keysdirectory");
                    keysdirectory = (str[4] == '"' && str[str.Length - 1] == '"') ? str.Substring(5, str.Length - 5) : str.Substring(4);
                    if (keysdirectory.StartsWith("/")) keysdirectory = Directory.GetCurrentDirectory() + keysdirectory;
                    else if (!Directory.Exists(keysdirectory)) throw new ArgumentException("keysdirectory path must exist");
                }
                if (str.StartsWith("-SD:"))
                {
                    secretdestination = (str[4] == '"' && str[str.Length - 1] == '"') ? str.Substring(5, str.Length - 5) : str.Substring(4);
                    if (!File.Exists(secretdestination)) throw new ArgumentException("secretdestination path must exist");
                }
            }
            List<Tuple<string, BigInteger, BigInteger>> keys = new List<Tuple<string, BigInteger, BigInteger>>();
            if (stringkeys == null)
            {
                IEnumerable<string> keyfiles = Directory.EnumerateFiles(keysdirectory, "*.key");
                if (!keyfiles.Any()) throw new ArgumentException("keysdirectory must contain key files (files that end in '.key')");
                IEnumerable<IEnumerable<string>> keyvalues = keyfiles.Select(keyfile => File.ReadLines(keyfile));
                if (keyvalues.Any(strs => strs.Count() != 2)) throw new ArgumentException("key files in keysdirectory must be formatted correctly: '{key}\\n{value}', eg.:\n5\nqwerty");
                foreach (IEnumerable<string> keyvalue in keyvalues)
                {
                    if (!int.TryParse(keyvalue.First(), out int key)) throw new ArgumentException("key keys must be integers");
                    string stringvalue = keyvalue.Last();
                    if (!stringvalue.All(chr => IsValidChar(chr))) throw new ArgumentException("key values must only contain valid characters");
                    keys.Add(new Tuple<string, BigInteger, BigInteger>(stringvalue, key, ToFullInt(stringvalue)));
                }
            }
            else
            {
                foreach (string[] pair in stringkeys)
                {
                    if (!int.TryParse(pair[0], out int key)) throw new ArgumentException("key keys must be integers");
                    if (!pair[1].All(chr => IsValidChar(chr))) throw new ArgumentException("key values must only contain valid characters");
                    keys.Add(new Tuple<string, BigInteger, BigInteger>(pair[1], key, ToFullInt(pair[1])));
                }
            }

            string secret = ToString(Polynomial.Lagrange(0, keys.Select(tuple => new KeyValuePair<BigInteger, BigInteger>(tuple.Item2, tuple.Item3)),
                BigInteger.Pow(97, keys.Select(tuple => tuple.Item1).Max(str => str.Length))));

            if (secretdestination == null) Console.WriteLine($"The secret was:\n{secret}\n");
            else File.WriteAllText(secretdestination, secret);
        }
        #endregion

        #region UI
        public static void UI()
        {
            ConsoleColourEnabler.EnableConsoleColour(); // maybe use Console.ForeGroundColor/Console.BackGroundColor, but I like this

            //System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            //stopwatch.Restart();
            //Console.WriteLine("prime : " + BigIntegerMaths.NextPrime(bigInteger));
            //stopwatch.Stop();
            //Console.WriteLine((double)stopwatch.ElapsedTicks / System.Diagnostics.Stopwatch.Frequency);

            //while (true)
            //{
            //    string input = GetInput("> ");
            //    try
            //    {
            //        Console.WriteLine(ToString(BigInteger.Parse(input)));
            //    }
            //    catch
            //    {
            //        BigInteger num = ToFullInt(input);
            //        Console.WriteLine(num);
            //        Console.WriteLine(PerfectPrime(input));
            //    }
            //}
            //Console.WriteLine("\u001b[93m Colour \u001b[m");

            string prev_string = "";
            BigInteger prev_secret = 0;
            while (true)
            {
                if (GetInput("\u001b[mEnter 'y' to encode: \u001b[93m") == "y")
                {
                    #region Encoding
                    Console.WriteLine("\n\r---ENCODING---");
                    bool strings = true;
                    BigInteger secret, prime;
                    if (GetInput("\u001b[mEnter 'y' to encode a number instead of a string: \u001b[93m") == "y")
                    {
                        strings = false;
                        if (!EmptyableIntInput(out secret,
                            "\u001b[mEnter the secret to be encoded(leave empty for previous value):\n\r> \u001b[93m"))
                            Console.WriteLine("\u001b[F\u001b[2C" + (secret = prev_secret));
                        if (!EmptyableIntInput(out prime,
                            "\u001b[mEnter the universal key\n\r(must be higher than the secret and prime or weird stuff happens)\n\r(leave empty for next highest prime):\n\r> \u001b[93m"))
                            Console.WriteLine("\u001b[F\u001b[2C" + (prime = BigIntegerMaths.NextPrime(secret)));
                    }
                    else
                    {
                        if (EmptyableInput(out string str, "\u001b[mEnter the secret to be encoded(leave empty for previous value):\n\r> \u001b[93m")) Console.WriteLine(secret = ToFullInt(str));
                        else Console.WriteLine($"\u001b[F\u001b[2C{prev_string}\n\r{secret = prev_secret}");
                        if (!EmptyableIntInput(out prime,
                            "\u001b[mEnter the universal key\n\r(must be higher than the secret and prime or weird stuff happens)\n\r(leave empty for perfect prime):\n\r> \u001b[93m"))
                            Console.WriteLine("\u001b[F\u001b[2C" + (prime = PerfectPrime(str)));
                    }
                    int requiredkeys = IntInput("\u001b[mEnter the number of keys required:\n\r> \u001b[93m");
                    int numkeys = IntInput("\u001b[mEnter the number of keys to make\n\r(preferably more than the amount required):\n\r> \u001b[93m");
                    BigInteger[] extra = new BigInteger[requiredkeys];
                    extra[0] = secret;
                    if (GetInput("\u001b[mEnter 'y' to manually enter other integer values: \u001b[93m") == "y")
                    {
                        Console.WriteLine("\u001b[mEnter other integer values one at a time\n\r(leave empty for random value):");
                        for (int i = 1; i < extra.Length; i++)
                        {
                            if (EmptyableIntInput(out BigInteger input, "\u001b[m> \u001b[93m")) extra[i] = input;
                            else Console.WriteLine("\u001b[F\u001b[2C" + (extra[i] = random.NextBigInteger(1, prime)));
                        }
                    }
                    else
                    {
                        for (int i = 1; i < extra.Length; i++) extra[i] = random.NextBigInteger(1, prime);
                    }
                    Polynomial polynomial = new Polynomial(extra, prime);
                    Dictionary<int, BigInteger> points = new Dictionary<int, BigInteger>();
                    for (int i = 1; i < numkeys + 1; i++)
                    {
                        points.Add(i, polynomial.Evaluate(i));
                    }
                    int fill = (int)Math.Floor(Math.Log10(numkeys)) + 1;
                    foreach (KeyValuePair<int, BigInteger> keyValuePair in points)
                    {
                        Console.WriteLine($"\u001b[96m{keyValuePair.Key.ToString().PadLeft(fill)} : {(strings ? "'" + ToString(keyValuePair.Value) + "'" : keyValuePair.Value.ToString())}\u001b[m");
                    }
                    Console.WriteLine();
                    #endregion
                }
                else
                {
                    #region Decoding
                    Console.WriteLine("\n\r---DECODING---");
                    bool strings = true;
                    BigInteger prime;
                    IEnumerable<KeyValuePair<BigInteger, BigInteger>> keys = new Dictionary<BigInteger, BigInteger>();
                    if (GetInput("\u001b[mEnter 'y' to decode a number instead of a string: \u001b[93m") == "y")
                    {
                        Dictionary<BigInteger, BigInteger> numkeys = new Dictionary<BigInteger, BigInteger>();
                        strings = false;
                        prime = BigIntInput("\u001b[mEnter the universal key:\n\r> \u001b[93m");
                        Console.WriteLine("\u001b[mEnter keys(first smaller numbers) and values(second larger numbers) one at a time(leave empty to stop):");
                        while (true)
                        {
                            if (!EmptyableIntInput(out int key, "\u001b[mkey  > \u001b[93m")) break;
                            numkeys.Add(key, BigIntInput("\u001b[m  value> \u001b[93m"));
                        }
                        keys = numkeys;
                    }
                    else
                    {
                        Console.WriteLine("\u001b[mEnter keys(numbers) and values(strings) one at a time(leave empty to stop):");
                        Dictionary<int, string> stringkeys = new Dictionary<int, string>();
                        while (true)
                        {
                            if (!EmptyableIntInput(out int key, "\u001b[mkey  > \u001b[93m")) break;
                            stringkeys.Add(key, GetInput("\u001b[m  value> \u001b[93m"));
                        }
                        if (!EmptyableIntInput(out prime,
                            "\u001b[mEnter the universal key\n\r(leave empty for automatic, which should be correct\n\runless keys have been altered or whitespace omitted):\n\r> \u001b[93m"))
                            Console.WriteLine("\u001b[F\u001b[2C" + (prime = BigInteger.Pow(97, stringkeys.Values.Max(str => str.Length))));
                        keys = stringkeys.Select(pair => new KeyValuePair<BigInteger, BigInteger>(pair.Key, ToFullInt(pair.Value)));
                    }
                    Console.WriteLine($"\u001b[mThe secret was:\n\r\u001b[96m{(strings ? ToString(Polynomial.Lagrange(0, keys, prime)) : Polynomial.Lagrange(0, keys, prime).ToString())}\u001b[m\n\r");
                    #endregion
                }
            }
        }
        #endregion

        #region get input
        public static string GetInput(string start = "")
        {
            Console.Write(start);
            return Console.ReadLine();
        }
        public static int IntInput(string start = "")
        {
            int result;
            while (!int.TryParse(GetInput(start), out result)) Console.WriteLine("Enter an integer");
            return result;
        }
        public static BigInteger BigIntInput(string start = "")
        {
            BigInteger result;
            while (!BigInteger.TryParse(GetInput(start), out result)) Console.WriteLine("Enter an integer");
            return result;
        }

        /*public static BigInteger? EmptyableIntInput(string start = "")
        {
            BigInteger result = 0;
            string input;
            bool empty = false;
            while (true)
            {
                if (string.IsNullOrEmpty(input = GetInput(start)))
                {
                    empty = true;
                    break;
                }
                if (BigInteger.TryParse(input, out result)) break;
                Console.WriteLine("Enter an integer");
            }
            return empty? null : (BigInteger?)result;
        }*/

        /// <summary>
        /// gets an input that can be empty and returns whether it is not empty
        /// </summary>
        /// <param name="input">the string entered</param>
        /// <param name="start"></param>
        /// <returns>true if it was not empty, false if it was empty</returns>
        public static bool EmptyableInput(out string input, string start = "") => string.IsNullOrEmpty(input = GetInput(start)) ? false : true;

        #region emptyable input
        /// <summary>
        /// gets an input that can be empty or integer and returns whether it is an integer
        /// </summary>
        /// <param name="input">the integer entered</param>
        /// <param name="start"></param>
        /// <returns>true if it was an integer, false if it was empty</returns>
        public static bool EmptyableIntInput(out int input, string start = "")
        {
            while (true)
            {
                string stringinput = GetInput(start);
                if (string.IsNullOrEmpty(stringinput))
                {
                    input = default;
                    return false;
                }
                if (int.TryParse(stringinput, out input)) return true;
                Console.WriteLine("Enter an integer");
            }
        }
        /// <summary>
        /// gets an input that can be empty or integer and returns whether it is an integer
        /// </summary>
        /// <param name="input">the integer entered</param>
        /// <param name="start"></param>
        /// <returns>true if it was an integer, false if it was empty</returns>
        public static bool EmptyableIntInput(out BigInteger input, string start = "")
        {
            while (true)
            {
                string stringinput = GetInput(start);
                if (string.IsNullOrEmpty(stringinput))
                {
                    input = default;
                    return false;
                }
                if (BigInteger.TryParse(stringinput, out input)) return true;
                Console.WriteLine("Enter an integer");
            }
        }
        #endregion
        #endregion

        public static bool IsValidChar(char chr) => (chr >= 32 && chr <= 126/*94 + 32*/) || chr == '£' || chr == '¬';
        public static char ToChar(int num) => (num = fullprime) == 95 ? '£' : (num == 96 ? '¬' : (char)(32 + num));
        public static char ToChar(BigInteger num) => (num = num.Mod(fullprime)) == 95 ? '£' : (num == 96 ? '¬' : (char)(32 + num));
        public static int ToInt(char chr) => chr == '£' ? 95 : (chr == '¬' ? 96 : (chr - 32));

        public static string ToString(BigInteger fullnum)
        {
            if (fullnum == 0) return " ";

            char[] chrs = new char[(int)BigInteger.Log(fullnum, fullprime) + 1];
            for ((int i, BigInteger primepower) = (chrs.Length - 1, 1); i >= 0; i--, primepower *= fullprime)
                chrs[i] = ToChar(fullnum / primepower);
            return new string(chrs);
        }
        public static BigInteger ToFullInt(string str)
        {
            BigInteger fullint = 0;
            for ((int i, BigInteger primepower) = (str.Length - 1, 1); i >= 0; i--, primepower *= fullprime)
                fullint += ToInt(str[i]) * primepower;
            return fullint;
        }

        public static BigInteger PerfectPrime(string str)
        {
            return BigInteger.Pow(fullprime, str.Length);
        }
    }
}
