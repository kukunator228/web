using System;
using System.Collections.Generic;

namespace web.Models;

public partial class BookSupply
{
    public int BookSupplyId { get; set; }

    public int BookKey { get; set; }

    public int SupplierKey { get; set; }

    public string SupplyQuantity { get; set; } = null!;

    public DateOnly SupplyDate { get; set; }

    public decimal BookSupplyPiecePrice { get; set; }

    public virtual Book BookKeyNavigation { get; set; } = null!;

    public virtual Supplier SupplierKeyNavigation { get; set; } = null!;

    public decimal RetailPrice => BookSupplyPiecePrice * 1.6m;

}