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
        public string Notes { get; set; }
    }

    public class ItemModel
    {
        public int ItemId { get; set; }
        public string Name { get; set; }
        public string Item { get; set; }
        public string Link { get; set; }
        public string DateUpdated { get; set; }
        public string Claimer { get; set; }
        public string DateClaimed { get; set; }
    }
}
