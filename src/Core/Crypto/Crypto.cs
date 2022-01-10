using System;
using System.Security.Cryptography;

namespace Symnity.Core.Crypto
{
    [Serializable]
    public class Crypto
    {
        /**
         * Generate random bytes by length
         * @param {byte} length - The length of the random bytes
         *
         * @return {byte[]}
         */
        public static byte[] RandomBytes(byte length) {
            var rngCsp = new RNGCryptoServiceProvider();
            var randomBytes = new byte[length];
            rngCsp.GetBytes(randomBytes);
            return randomBytes;
        }
    }
}