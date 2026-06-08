using System;
using System.Collections.Generic;

namespace web.Models;

public partial class OrderItem
{
    public int OrderItemId { get; set; }

    public int OrderKey { get; set; }

    public int BookKey { get; set; }

    public string OrderBookQuantity { get; set; } = null!;

    public virtual Book BookKeyNavigation { get; set; } = null!;

    public virtual Order OrderKeyNavigation { get; set; } = null!;
}