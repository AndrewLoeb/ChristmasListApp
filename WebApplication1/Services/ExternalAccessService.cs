using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.Services
{
    /// <summary>
    /// Scoped service to manage external user access context.
    /// Handles token validation and external user identity.
    /// </summary>
    public class ExternalAccessService
    {
        // Token to external user name mapping
        private static readonly Dictionary<string, string> TokenMapping = new Dictionary<string, string>
        {
            { "lisa", "Lisa" }
            // Future external users can be added here:
            // { "john", "John" },
        };

        public string ExternalUserName { get; private set; }
        public bool IsExternalUser => !string.IsNullOrEmpty(ExternalUserName);

        /// <summary>
        /// Validates token and sets external user context.
        /// Returns true if token is valid, false otherwise.
        /// </summary>
        public bool SetExternalUser(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            // Case-insensitive token lookup
            var normalizedToken = token.ToLowerInvariant();

            if (TokenMapping.TryGetValue(normalizedToken, out string userName))
            {
                ExternalUserName = userName;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets all valid tokens (for admin/debugging purposes).
        /// </summary>
        public IEnumerable<string> GetAllowedTokens()
        {
            return TokenMapping.Keys;
        }

        /// <summary>
        /// Clears external user context.
        /// </summary>
        public void ClearContext()
        {
            ExternalUserName = null;
        }
    }
}
