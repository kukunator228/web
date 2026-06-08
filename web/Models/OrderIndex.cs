using System;
using System.Collections.Generic;

namespace web.Models;

public partial class OrderIndex
{
    public string Index { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
