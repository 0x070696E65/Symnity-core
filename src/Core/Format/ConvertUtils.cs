using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Symnity.Core.Format
{
    [Serializable]
    public class ConvertUtils
    {
        
        public  static List<byte> Xor(List<byte> currentMetadataValueBytes, List<byte> newMetadataValueBytes)
        {
            var length = Math.Max(currentMetadataValueBytes.Count, newMetadataValueBytes.Count);
            for (var i = currentMetadataValueBytes.Count; i < length; i++) currentMetadataValueBytes.Add(0);
            for (var i = newMetadataValueBytes.Count; i < length; i++) newMetadataValueBytes.Add(0);
            var metadataNewLifelist = new List<byte>();
            for (var i = 0; i < length; i++) metadataNewLifelist.Add((byte)(currentMetadataValueBytes.ToArray()[i] ^ newMetadataValueBytes.ToArray()[i]));
            return metadataNewLifelist;
        }
        
        /**
        * Converts a hex string to a byte array.
        *
        * @param hexString The input hex string.
        * @return The output byte array.
        */
        public static byte[] GetBytes(string hexString)
        {
            try
            {
                return GetBytesInternal(hexString);
            }
            catch (Exception e)
            {
                throw new ArgumentException(e.Message);
            }
        }
        
        /**
         * Convert UTF-8 string to Uint8Array
         * @param {string} input - An string with UTF-8 encoding
         * @return {Uint8Array}
         */
        public static byte[] Utf8ToByteArray(string input)
        {
            return GetBytes(Utf8ToHex(input));
        }

        private static byte[] GetBytesInternal(string hexString)
        {
            try
            {
                var bs = new List<byte>();
                for (var i = 0; i < hexString.Length / 2; i++)
                {
                    bs.Add(Convert.ToByte(hexString.Substring(i * 2, 2), 16));
                }
                return bs.ToArray();
            }
            catch (Exception e)
            {
                throw new ArgumentException(e.Message);
            }
        }
        
        /**
         * Converts a byte array to a hex string.
         *
         * @param bytes The input byte array.
         * @return The output hex string.
         */
        public static string ToHex(byte[] bytes)
        {
            var str = BitConverter.ToString(bytes);
            str = str.Replace("-", string.Empty);
            return str;
        }
        
        /**
         * Convert UTF-8 to hex
         * @param {string} input - An UTF-8 string
         * @return {string}
         */
        public static string Utf8ToHex(string input) {
            var bytes = Encoding.UTF8.GetBytes(input);
            var hexString = BitConverter.ToString(bytes);
            hexString = hexString.Replace("-", "");
            return hexString;
        }

        public static string HexToChar(string hex, bool zero = false)
        {
            var byteList = new List<byte>(GetBytes(hex));
            var result = "";
            byteList.ForEach(b=>
            {
                if (zero) result += Convert.ToChar(b);
                else if (b != 0)
                {
                    result += Convert.ToChar(b);
                    zero = true;
                }
            });
            return result;
        }

        /**
        * Determines whether or not a string is a hex string.
        *
        * @param input The string to test.
        * @return boolean true if the input is a hex string, false otherwise.
        */
        public static bool IsHexString(string s)
        {
            try
            {
                if (string.IsNullOrEmpty(s))
                {
                    return false;
                }

                var bs = new List<byte>();
                for (var i = 0; i < s.Length / 2; i++)
                {
                    bs.Add(Convert.ToByte(s.Substring(i * 2, 2), 16));
                }
                return bs.Count != 0;
            }
            catch 
            {
                return false;
            }
        }
        /**
        * Validates that an input is a valid hex . If not, it raises a {@link IllegalArgumentException}
        *
        * @param input the string input
        * @throws IllegalArgumentException if the input is null or not an hex.
        */
        public static void ValidateIsHexString(string input) {
            if (input == null) {
                throw new ArgumentNullException("Null is not a valid hex");
            }
            if (!IsHexString(input)) {
                throw new Exception(input + " is not a valid hex");
            }
        }
    }
}