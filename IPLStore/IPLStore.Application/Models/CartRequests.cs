using System;
using System.Collections.Generic;
using System.Text;

namespace IPLStore.Application.Models
{
    public record UpsertCartItemRequest(int ProductId, int Quantity);

    public record UpdateCartItemRequest(int Quantity);
}
