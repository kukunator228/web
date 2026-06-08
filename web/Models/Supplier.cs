using System;
using System.Collections.Generic;

namespace web.Models;

public partial class Supplier
{
    public int SupplierId { get; set; }

    public string SupplierName { get; set; } = null!;

    public string SupplierInn { get; set; } = null!;

    public virtual ICollection<BookSupply> BookSupplies { get; set; } = new List<BookSupply>();
}
