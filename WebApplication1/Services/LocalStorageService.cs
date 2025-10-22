using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace WebApplication1.Services
{
    /// <summary>
    /// Service for interacting with browser's LocalStorage.
    /// Used for persisting user preferences like "Remember Me" functionality.
    /// </summary>
    public class LocalStorageService
    {
        private readonly IJSRuntime _jsRuntime;
        private const string UsernameKey = "christmas_list_username";

        public LocalStorageService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        /// <summary>
        /// Saves the username to browser's LocalStorage.
        /// Used when "Remember Me" is checked during login.
        /// </summary>
        public async Task SaveUsernameAsync(string username)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", UsernameKey, username);
        }

        /// <summary>
        /// Retrieves the saved username from browser's LocalStorage.
        /// Returns null if no username was saved.
        /// </summary>
        public async Task<string> GetUsernameAsync()
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", UsernameKey);
            }
            catch
            {
                // If JavaScript is not ready or LocalStorage is unavailable, return null
                return null;
            }
        }

        /// <summary>
        /// Clears the saved username from browser's LocalStorage.
        /// Used during logout.
        /// </summary>
        public async Task ClearUsernameAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", UsernameKey);
        }
    }
}
