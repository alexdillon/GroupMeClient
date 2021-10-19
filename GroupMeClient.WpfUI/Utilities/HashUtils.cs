using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace GroupMeClient.WpfUI.Utilities
{
    /// <summary>
    /// Utilities for hashing data.
    /// </summary>
    public class HashUtils
    {
        /// <summary>
        /// Computes the SHA1 hash for a string.
        /// </summary>
        /// <param name="input">The string to hash.</param>
        /// <returns>The hex encoded SHA1 hash result.</returns>
        public static string SHA1Hash(string input)
        {
            using (var sha1Managed = new SHA1Managed())
            {
                var hash = sha1Managed.ComputeHash(Encoding.UTF8.GetBytes(input));
                return string.Concat(hash.Select(b => b.ToString("x2")));
            }
        }
    }
}
