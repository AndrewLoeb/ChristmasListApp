using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class SpiceModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category1 { get; set; }
        public string Category2 { get; set; }
    }
    public class UserModel
    {
        public int Id { get; set; }
        public string Family { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Notes { get; set; }
        public bool VisibleToLisa { get; set; }
        public bool VisibleToGano { get; set; }
    }

    public class ItemModel
    {
        public int ItemId { get; set; }
        public string Name { get; set; }
        public string Item { get; set; }
        public string Link { get; set; }
        public string Notes { get; set; }
        public string DateUpdated { get; set; }
        public string Claimer { get; set; }
        public string DateClaimed { get; set; }
        public int Active { get; set; }

        // Product metadata fields
        public string ImageUrl { get; set; }
        public decimal? Price { get; set; }  // Nullable - may not always have price
        public string MetadataFetchedDate { get; set; }  // When we last scraped this link
        public int IsStarred { get; set; }  // 0 = not starred, 1 = starred (most wanted)
        public string ReceivedBy { get; set; }  // Name of household member who marked item as received
    }
    public class ListModel
    {
        public List<ItemModel> List { get; set; }
        public string Name { get; set; }
        public int itemsListed { get; set; }
        public int itemsClaimed { get; set; }
        public System.DateTime lastUpdated { get; set; }
        public string dropDownStr { get; set; }
    }
    public class TinselTrackerPermissionModel
    {
        public string Viewer { get; set; }
        public string CanSeeClaims { get; set; }
    }
}
