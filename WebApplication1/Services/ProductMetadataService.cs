using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace WebApplication1.Services
{
    public class ProductMetadata
    {
        public string ImageUrl { get; set; }
        public decimal? Price { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class ProductMetadataService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ProductMetadataService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ProductMetadata> FetchMetadataAsync(string url)
        {
            var result = new ProductMetadata { Success = false };

            if (string.IsNullOrWhiteSpace(url))
            {
                result.ErrorMessage = "URL is empty";
                return result;
            }

            try
            {
                Console.WriteLine($"Fetching metadata for URL: {url}");

                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10);

                // Set user agent to avoid bot blocking
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var html = await response.Content.ReadAsStringAsync();

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // Try OpenGraph tags first
                result.ImageUrl = GetMetaProperty(doc, "og:image")
                               ?? GetMetaProperty(doc, "twitter:image");

                result.Title = GetMetaProperty(doc, "og:title")
                            ?? GetMetaProperty(doc, "twitter:title")
                            ?? doc.DocumentNode.SelectSingleNode("//title")?.InnerText?.Trim();

                result.Description = GetMetaProperty(doc, "og:description")
                                  ?? GetMetaProperty(doc, "twitter:description")
                                  ?? GetMetaName(doc, "description");

                // Price detection disabled for now - will be enhanced later with Amazon PA-API
                // result.Price = ExtractPrice(doc);

                result.Success = true;
                Console.WriteLine($"Successfully fetched metadata: Title={result.Title}, ImageUrl={result.ImageUrl}");
            }
            catch (HttpRequestException ex)
            {
                result.ErrorMessage = $"HTTP error: {ex.Message}";
                Console.WriteLine($"Error fetching metadata: {result.ErrorMessage}");
            }
            catch (TaskCanceledException ex)
            {
                result.ErrorMessage = "Request timed out";
                Console.WriteLine($"Error fetching metadata: {result.ErrorMessage}");
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Unexpected error: {ex.Message}";
                Console.WriteLine($"Error fetching metadata: {result.ErrorMessage}");
            }

            return result;
        }

        private string GetMetaProperty(HtmlDocument doc, string property)
        {
            var node = doc.DocumentNode.SelectSingleNode($"//meta[@property='{property}']");
            return node?.GetAttributeValue("content", null);
        }

        private string GetMetaName(HtmlDocument doc, string name)
        {
            var node = doc.DocumentNode.SelectSingleNode($"//meta[@name='{name}']");
            return node?.GetAttributeValue("content", null);
        }

        private decimal? ExtractPrice(HtmlDocument doc)
        {
            // Try various common price meta tags
            string priceStr = null;

            // OpenGraph product price
            priceStr = GetMetaProperty(doc, "product:price:amount")
                    ?? GetMetaProperty(doc, "og:price:amount")
                    ?? GetMetaProperty(doc, "price");

            // Schema.org price in meta tags
            if (priceStr == null)
            {
                var schemaNode = doc.DocumentNode.SelectSingleNode("//meta[@itemprop='price']");
                priceStr = schemaNode?.GetAttributeValue("content", null);
            }

            // Try JSON-LD structured data (common on e-commerce sites)
            if (priceStr == null)
            {
                priceStr = ExtractPriceFromJsonLd(doc);
            }

            // If we found a price string, try to parse it
            if (!string.IsNullOrWhiteSpace(priceStr))
            {
                // Remove currency symbols and clean up
                var cleanPrice = Regex.Replace(priceStr, @"[^\d.,]", "");
                cleanPrice = cleanPrice.Replace(",", ""); // Remove commas for thousands

                if (decimal.TryParse(cleanPrice, out decimal price))
                {
                    return price;
                }
            }

            // Fallback: search for common price patterns in visible HTML
            // Look for patterns like: $29.99, $29, USD 29.99, etc.
            var htmlText = doc.DocumentNode.InnerText;
            var priceMatch = Regex.Match(htmlText, @"[\$€£¥]\s*(\d{1,6}(?:[.,]\d{2})?)");
            if (priceMatch.Success)
            {
                var priceValue = priceMatch.Groups[1].Value.Replace(",", ".");
                if (decimal.TryParse(priceValue, out decimal fallbackPrice))
                {
                    return fallbackPrice;
                }
            }

            return null;
        }

        private string ExtractPriceFromJsonLd(HtmlDocument doc)
        {
            try
            {
                // Look for JSON-LD script tags
                var jsonLdNodes = doc.DocumentNode.SelectNodes("//script[@type='application/ld+json']");
                if (jsonLdNodes == null) return null;

                foreach (var node in jsonLdNodes)
                {
                    var json = node.InnerText;

                    // Look for price patterns in JSON (Product, Offer, AggregateOffer schemas)
                    // Simple regex approach - not parsing full JSON to avoid adding JSON library dependency

                    // Pattern: "price": "29.99" or "price": 29.99
                    var priceMatch = Regex.Match(json, @"""price""\s*:\s*[""']?(\d+\.?\d{0,2})[""']?");
                    if (priceMatch.Success)
                    {
                        return priceMatch.Groups[1].Value;
                    }

                    // Pattern: "lowPrice": "29.99" (for AggregateOffer)
                    var lowPriceMatch = Regex.Match(json, @"""lowPrice""\s*:\s*[""']?(\d+\.?\d{0,2})[""']?");
                    if (lowPriceMatch.Success)
                    {
                        return lowPriceMatch.Groups[1].Value;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing JSON-LD: {ex.Message}");
            }

            return null;
        }
    }
}
