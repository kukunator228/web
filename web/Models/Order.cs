using System;
using System.Collections.Generic;

namespace web.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public string AdressIndexKey { get; set; } = null!;

    public DateOnly OrderDate { get; set; }

    public int ClientKey { get; set; }

    public int StatusKey { get; set; }

    public virtual OrderIndex AdressIndexKeyNavigation { get; set; } = null!;

    public virtual User ClientKeyNavigation { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual OrderStatus StatusKeyNavigation { get; set; } = null!;

}
