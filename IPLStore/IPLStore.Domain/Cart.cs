namespace IPLStore.Domain
{
    public class Cart
    {
        public int Id { get; set; }
        public required string UserId { get; set; }

        public List<CartItem> Items { get; set; } = [];

        public decimal TotalAmount => Items.Sum(x => x.Quantity * x.UnitPrice);

        public void AddOrUpdateItem(int productId, int quantity, decimal price)
        {
            var existing = Items.SingleOrDefault(x => x.ProductId == productId);

            if (existing == null)
            {
                Items.Add(new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = price
                });
                return;
            }
            existing.Quantity = quantity;
            existing.UnitPrice = price;
        }

        public bool RemoveItem(int productId)
        {
            var existing = Items.SingleOrDefault(x => x.ProductId == productId);

            if (existing == null)
                return false;

            Items.Remove(existing);
            return true;
        }
    }
}
