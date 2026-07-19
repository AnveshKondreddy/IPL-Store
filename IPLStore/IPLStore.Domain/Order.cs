using System;
using System.Collections.Generic;
using System.Text;

namespace IPLStore.Domain
{
    public class Order
    {
        public int Id { get; set; }
        public required string UserId { get; set; }

        public DateTimeOffset OrderedAt { get; set; } = DateTime.UtcNow;

        public decimal TotalAmount { get; set; }

        public List<OrderItem> Items { get; set; } = [];

        public static Order FormOrderFromCart(string userId, Cart cart)
        {
            if (cart.Items.Count == 0)
            {
                throw new InvalidOperationException("Cannot create order from empty cart");
            }

            var order = new Order() { UserId = userId };

            foreach (var item in cart.Items)
            {
                order.Items.Add(new OrderItem()
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product?.Name ?? string.Empty,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                });
            }
            order.TotalAmount = cart.Items.Sum(x => x.Quantity * x.UnitPrice);
            return order;
        }
    }
}
