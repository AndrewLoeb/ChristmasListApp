using System;
using System.Collections.Generic;
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

                // Strategy 3: Generic site - extract domain + product name + ID + query params from URL
                // We'll try this with progressive fallback if initial search fails
                string fallbackDomain = null;
                string fallbackProductName = null;
                string fallbackProductId = null;
                List<string> fallbackQueryParams = null;

                if (searchQuery == null && !string.IsNullOrWhiteSpace(productUrl))
                {
                    var (domain, productName, productId, queryParams) = ExtractDomainAndProduct(productUrl);
                    if (!string.IsNullOrEmpty(domain) && !string.IsNullOrEmpty(productName))
                    {
                        // Store for potential fallback attempts
                        fallbackDomain = domain;
                        fallbackProductName = productName;
                        fallbackProductId = productId;
                        fallbackQueryParams = queryParams;

                        // Build search query with domain, product name, ID, and relevant params
                        var queryParts = new List<string> { domain, productName };

                        if (productId != null)
                        {
                            queryParts.Add($"item {productId}");
                        }

                        // Add query parameters (color, size, sku, etc.)
                        if (queryParams != null && queryParams.Count > 0)
                        {
                            queryParts.AddRange(queryParams);
                        }

                        searchQuery = string.Join(" ", queryParts);
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

                // Try search with progressive fallback
                string imageUrl = await TryGoogleImageSearch(searchQuery);

                // If no results and we have fallback data, try simpler queries
                if (imageUrl == null && fallbackDomain != null && fallbackProductName != null)
                {
                    // Fallback 1: Remove query params and product ID, keep domain + product name
                    var simplifiedQuery = $"{fallbackDomain} {fallbackProductName}";
                    Console.WriteLine($"Initial search failed. Trying simplified query: {simplifiedQuery}");
                    imageUrl = await TryGoogleImageSearch(simplifiedQuery);

                    // Fallback 2: Just domain + first 3-4 words of product name
                    if (imageUrl == null)
                    {
                        var words = fallbackProductName.Split(' ');
                        if (words.Length > 4)
                        {
                            var shortenedName = string.Join(" ", words.Take(4));
                            var shortenedQuery = $"{fallbackDomain} {shortenedName}";
                            Console.WriteLine($"Simplified search failed. Trying shortened query: {shortenedQuery}");
                            imageUrl = await TryGoogleImageSearch(shortenedQuery);
                        }
                    }
                }

                return imageUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching for product image: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Executes a single Google Image Search query and returns the first image URL.
        /// Returns null if no results found.
        /// </summary>
        private async Task<string> TryGoogleImageSearch(string searchQuery)
        {
            try
            {
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
                        var imageUrl = link.GetString();
                        Console.WriteLine($"✓ Found image for query: {searchQuery}");
                        return imageUrl;
                    }
                }

                // No results found
                if (searchResult.RootElement.TryGetProperty("searchInformation", out var searchInfo))
                {
                    if (searchInfo.TryGetProperty("totalResults", out var totalResults))
                    {
                        Console.WriteLine($"✗ No results for query (total: {totalResults.GetString()}): {searchQuery}");
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing Google search for '{searchQuery}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Extracts domain, product name, product ID, and query parameters from a generic e-commerce URL.
        /// Examples:
        ///   nordstrom.com/s/straw-shoulder-bag/8461451 → ("nordstrom", "straw shoulder bag", "8461451", [])
        ///   lululemon.com/.../prod11400110?color=71300 → ("lululemon", "pace breaker short...", "prod11400110", ["color 71300"])
        ///   sephora.com/.../P502185?skuId=2597045 → ("sephora", "triclone skin tech...", "P502185", ["skuid 2597045"])
        /// </summary>
        /// <param name="url">Product URL</param>
        /// <returns>Tuple of (domain, product name, product ID, query params) or (null, null, null, null) if extraction fails</returns>
        private (string domain, string productName, string productId, List<string> queryParams) ExtractDomainAndProduct(string url)
        {
            try
            {
                var uri = new Uri(url);

                // Extract brand name from domain
                // Examples:
                //   www.nordstrom.com → "nordstrom"
                //   shop.lululemon.com → "lululemon"
                //   m.nike.com → "nike"
                //   etsy.com → "etsy"
                var domain = uri.Host;

                // Remove www. prefix
                if (domain.StartsWith("www."))
                {
                    domain = domain.Substring(4);
                }

                // Split by dots and find the brand name
                var domainParts = domain.Split('.');
                string mainDomain;

                if (domainParts.Length >= 2)
                {
                    // Get the second-to-last part (brand name before TLD)
                    // shop.lululemon.com → ["shop", "lululemon", "com"] → "lululemon"
                    // nordstrom.com → ["nordstrom", "com"] → "nordstrom"
                    mainDomain = domainParts[domainParts.Length - 2];
                }
                else
                {
                    // Fallback to first part if unusual structure
                    mainDomain = domainParts[0];
                }

                // Extract relevant query parameters (color, size, SKU, etc.)
                // Whitelist of known product-related parameters (excludes tracking/analytics)
                var relevantParams = new[] { "color", "colour", "size", "style", "sku", "skuid", "variant", "option" };
                var queryParams = new List<string>();

                if (!string.IsNullOrWhiteSpace(uri.Query))
                {
                    var queryString = uri.Query.TrimStart('?');
                    var parameters = queryString.Split('&');

                    foreach (var param in parameters)
                    {
                        var parts = param.Split('=');
                        if (parts.Length == 2)
                        {
                            var key = parts[0].ToLower();
                            var value = parts[1];

                            // Check if this is a relevant product parameter
                            if (relevantParams.Contains(key) && !string.IsNullOrWhiteSpace(value))
                            {
                                // Decode URL-encoded values and clean up
                                var decodedValue = Uri.UnescapeDataString(value);

                                // Skip purely numeric values for color/style (they're cryptic codes, not helpful for search)
                                // Example: color=71300 doesn't help, but color=black does
                                // SKU/skuId can be numeric (that's useful for product identification)
                                bool isNumericValue = Regex.IsMatch(decodedValue, @"^\d+$");
                                bool isIdentifier = key.Contains("sku") || key.Contains("id");

                                if (!isNumericValue || isIdentifier)
                                {
                                    queryParams.Add($"{key} {decodedValue}");
                                }
                                else
                                {
                                    Console.WriteLine($"Skipping numeric {key} parameter: {decodedValue}");
                                }
                            }
                        }
                    }
                }

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

                // Strategy: Prefer longer, more specific slugs (likely product names)
                // rather than short category slugs (like "men-shorts")
                // Also look for product IDs (numeric or patterns like "prod12345")

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

                    // Check if this is a product ID pattern (numeric or "prod12345")
                    if (Regex.IsMatch(segment, @"^\d+$") || Regex.IsMatch(segment, @"^prod\d+", RegexOptions.IgnoreCase))
                    {
                        // Save as product ID
                        productId = segment;
                        continue;
                    }

                    // Skip segments that start with special characters or underscores (like "_")
                    if (segment.StartsWith("-") || segment.StartsWith("_") || segment.StartsWith("A-"))
                    {
                        continue;
                    }

                    // Found a candidate slug with dashes
                    if (segment.Contains("-") && segment.Length > 3)
                    {
                        // Prefer longer slugs (product names) over shorter ones (categories)
                        // E.g., "Pace-Breaker-Short-NF-7-Lined-Update" (38 chars) over "men-shorts" (10 chars)
                        if (productSlug == null || segment.Length > productSlug.Length)
                        {
                            productSlug = segment;
                        }

                        // Check if the next segment is a product ID
                        if (i + 1 < pathSegments.Length)
                        {
                            var nextSegment = pathSegments[i + 1];
                            if (Regex.IsMatch(nextSegment, @"^\d+$") || Regex.IsMatch(nextSegment, @"^prod\d+", RegexOptions.IgnoreCase))
                            {
                                productId = nextSegment;
                            }
                        }

                        // Don't break - continue looking for longer/better slugs
                    }
                }

                if (string.IsNullOrEmpty(productSlug))
                {
                    return (null, null, null, null);
                }

                // Convert slug to readable name: "straw-shoulder-bag" → "straw shoulder bag"
                var productName = productSlug.Replace("-", " ").Replace("_", " ").Trim();

                // Clean up any remaining special characters
                productName = Regex.Replace(productName, @"[^\w\s]", " ");
                productName = Regex.Replace(productName, @"\s+", " ").Trim();

                return (mainDomain, productName, productId, queryParams);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting domain and product from URL: {ex.Message}");
                return (null, null, null, null);
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
