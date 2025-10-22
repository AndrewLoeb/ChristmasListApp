using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace WebApplication1.Services
{
    public class GoogleImageSearchService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _searchEngineId;
        private const string GoogleCustomSearchApiUrl = "https://www.googleapis.com/customsearch/v1";

        public GoogleImageSearchService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _searchEngineId = configuration["GoogleCustomSearch:SearchEngineId"];

            // Load API key from app_client_secret.json
            var secretJson = File.ReadAllText("app_client_secret.json");
            var secretDoc = JsonDocument.Parse(secretJson);
            _apiKey = secretDoc.RootElement.GetProperty("google_custom_search_api_key").GetString();
        }

        /// <summary>
        /// Searches Google Images and returns the first image URL.
        /// Priority order:
        /// 1. Amazon ASIN (most accurate for Amazon)
        /// 2. Product title from OpenGraph (accurate, site-provided metadata)
        /// 3. Domain + product name parsed from URL (fallback for generic sites)
        /// 4. Full URL (last resort)
        /// </summary>
        /// <param name="productUrl">The product URL</param>
        /// <param name="productTitle">The product title from OpenGraph (if available)</param>
        /// <returns>Image URL or null if not found</returns>
        public async Task<string> SearchProductImageAsync(string productUrl, string productTitle = null)
        {
            try
            {
                string searchQuery = null;

                // Strategy 1: Amazon-specific ASIN extraction (highly accurate for Amazon)
                if (!string.IsNullOrWhiteSpace(productUrl) &&
                    productUrl.Contains("amazon", StringComparison.OrdinalIgnoreCase))
                {
                    var asin = ExtractAmazonAsin(productUrl);
                    if (!string.IsNullOrEmpty(asin))
                    {
                        searchQuery = $"Amazon {asin}";
                        Console.WriteLine($"Using Amazon ASIN search: {searchQuery}");
                    }
                }

                // Strategy 2: Product title from OpenGraph (site-provided, accurate)
                if (searchQuery == null && !string.IsNullOrWhiteSpace(productTitle))
                {
                    searchQuery = productTitle;
                    Console.WriteLine($"Using product title search: {searchQuery}");
                }

                // Strategy 3: Generic site - extract domain + product name + ID from URL
                if (searchQuery == null && !string.IsNullOrWhiteSpace(productUrl))
                {
                    var (domain, productName, productId) = ExtractDomainAndProduct(productUrl);
                    if (!string.IsNullOrEmpty(domain) && !string.IsNullOrEmpty(productName))
                    {
                        // Build search query with domain, product name, and ID (if available)
                        searchQuery = productId != null
                            ? $"{domain} {productName} item {productId}"
                            : $"{domain} {productName}";
                        Console.WriteLine($"Using domain + product name search: {searchQuery}");
                    }
                }

                // Strategy 4: Last resort - use the URL itself
                if (searchQuery == null && !string.IsNullOrWhiteSpace(productUrl))
                {
                    searchQuery = productUrl;
                    Console.WriteLine($"Using full URL search: {searchQuery}");
                }

                if (string.IsNullOrWhiteSpace(searchQuery))
                {
                    return null;
                }

                // Build Google Custom Search API request
                var requestUrl = $"{GoogleCustomSearchApiUrl}?key={_apiKey}&cx={_searchEngineId}&q={Uri.EscapeDataString(searchQuery)}&searchType=image&num=1";

                var response = await _httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var searchResult = JsonDocument.Parse(jsonResponse);

                // Extract the first image URL from the response
                if (searchResult.RootElement.TryGetProperty("items", out var items) && items.GetArrayLength() > 0)
                {
                    var firstItem = items[0];
                    if (firstItem.TryGetProperty("link", out var link))
                    {
                        return link.GetString();
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching for product image: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Extracts domain, product name, and product ID from a generic e-commerce URL.
        /// Examples:
        ///   nordstrom.com/s/straw-shoulder-bag/8461451 → ("nordstrom", "straw shoulder bag", "8461451")
        ///   etsy.com/listing/123456/vintage-ceramic-mug → ("etsy", "vintage ceramic mug", "123456")
        ///   target.com/p/kitchen-towel-set/-/A-12345 → ("target", "kitchen towel set", "12345")
        /// </summary>
        /// <param name="url">Product URL</param>
        /// <returns>Tuple of (domain, product name, product ID) or (null, null, null) if extraction fails</returns>
        private (string domain, string productName, string productId) ExtractDomainAndProduct(string url)
        {
            try
            {
                var uri = new Uri(url);

                // Extract domain name (e.g., "nordstrom.com" → "nordstrom")
                var domain = uri.Host;
                if (domain.StartsWith("www."))
                {
                    domain = domain.Substring(4);
                }
                // Get the main domain part (e.g., "nordstrom.com" → "nordstrom")
                var domainParts = domain.Split('.');
                var mainDomain = domainParts.Length > 0 ? domainParts[0] : domain;

                // Extract product name from URL path
                var path = uri.AbsolutePath;

                // Remove common URL patterns and extract the product slug
                // Examples:
                //   /s/straw-shoulder-bag/8461451 → straw-shoulder-bag
                //   /listing/123456/vintage-ceramic-mug → vintage-ceramic-mug
                //   /p/kitchen-towel-set/-/A-12345 → kitchen-towel-set

                // Split path by '/' and look for the product slug and ID
                var pathSegments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                string productSlug = null;
                string productId = null;

                for (int i = 0; i < pathSegments.Length; i++)
                {
                    var segment = pathSegments[i];

                    // Skip common path prefixes (s, p, listing, item, product, etc.)
                    if (segment.Length <= 2 ||
                        segment.Equals("s", StringComparison.OrdinalIgnoreCase) ||
                        segment.Equals("p", StringComparison.OrdinalIgnoreCase) ||
                        segment.Equals("item", StringComparison.OrdinalIgnoreCase) ||
                        segment.Equals("listing", StringComparison.OrdinalIgnoreCase) ||
                        segment.Equals("product", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    // Check if this is a numeric ID (likely product ID)
                    if (Regex.IsMatch(segment, @"^\d+$"))
                    {
                        // Save as potential product ID
                        productId = segment;
                        continue;
                    }

                    // Skip segments that start with special characters or single letters
                    if (segment.StartsWith("-") || segment.StartsWith("A-"))
                    {
                        continue;
                    }

                    // Found a candidate - typically the product slug with dashes
                    if (segment.Contains("-") && segment.Length > 3)
                    {
                        productSlug = segment;

                        // Check if the next segment is a numeric ID
                        if (i + 1 < pathSegments.Length && Regex.IsMatch(pathSegments[i + 1], @"^\d+$"))
                        {
                            productId = pathSegments[i + 1];
                        }

                        break; // Take the first good match
                    }
                }

                if (string.IsNullOrEmpty(productSlug))
                {
                    return (null, null, null);
                }

                // Convert slug to readable name: "straw-shoulder-bag" → "straw shoulder bag"
                var productName = productSlug.Replace("-", " ").Replace("_", " ").Trim();

                // Clean up any remaining special characters
                productName = Regex.Replace(productName, @"[^\w\s]", " ");
                productName = Regex.Replace(productName, @"\s+", " ").Trim();

                return (mainDomain, productName, productId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting domain and product from URL: {ex.Message}");
                return (null, null, null);
            }
        }

        /// <summary>
        /// Extracts the Amazon ASIN (10-character product identifier) from an Amazon URL.
        /// Supports multiple Amazon URL patterns.
        /// </summary>
        /// <param name="url">Amazon product URL</param>
        /// <returns>ASIN or null if not found</returns>
        private string ExtractAmazonAsin(string url)
        {
            if (string.IsNullOrWhiteSpace(url) || !url.Contains("amazon", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            // Common Amazon URL patterns:
            // https://www.amazon.com/dp/B08N5WRWNW
            // https://www.amazon.com/gp/product/B08N5WRWNW
            // https://www.amazon.com/Product-Title/dp/B08N5WRWNW
            // https://a.co/d/B08N5WRWNW (short links)

            var patterns = new[]
            {
                @"/dp/([A-Z0-9]{10})",           // /dp/ASIN
                @"/gp/product/([A-Z0-9]{10})",   // /gp/product/ASIN
                @"/d/([A-Z0-9]{10})",            // a.co/d/ASIN
                @"ASIN=([A-Z0-9]{10})",          // Query parameter
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(url, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }

            return null;
        }

        /// <summary>
        /// Test method to check if the API credentials are working.
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var testUrl = $"{GoogleCustomSearchApiUrl}?key={_apiKey}&cx={_searchEngineId}&q=test&searchType=image&num=1";
                var response = await _httpClient.GetAsync(testUrl);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
