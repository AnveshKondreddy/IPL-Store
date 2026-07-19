using System;
using System.Collections.Generic;
using System.Text;

namespace IPLStore.Domain
{
    public class Product
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public int StockQty { get; set; }
        public required string Type { get; set; }

        public required string Description { get; set; }
        public int FranchiseId { get; set; }

        public Franchise? Franchise { get; set; }
        public decimal Price { get; set; }

        public bool IsActive { get; set; }

        public byte[] RowVersion { get; set; } = [];
    }
}
