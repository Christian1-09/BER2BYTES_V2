using Ber2Bytes.Domain.Enums;

namespace Ber2Bytes.Domain.Entities;

public class Sale
{
    public int Id { get; set; }
    public string InvoiceNo { get; set; } = "";
    public DateTime Date { get; set; } = DateTime.Now;
    public string CustomerName { get; set; } = "Walk-in";
    public string? CustomerPhone { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public string? ReferenceNo { get; set; }
    public decimal AmountTendered { get; set; }
    public decimal Change { get; set; }
    public int CashierId { get; set; }
    public User? Cashier { get; set; }
    public SaleStatus Status { get; set; } = SaleStatus.Completed;
    public string? VoidReason { get; set; }
    public int? VoidedById { get; set; }
    public DateTime? VoidedAt { get; set; }
    public List<SaleItem> Items { get; set; } = [];

    /// <summary>Gross profit of this sale: (price − cost) × qty for every line, minus the discount.</summary>
    public decimal GrossProfit => Items.Sum(i => (i.UnitPrice - i.UnitCost) * i.Quantity) - Discount;
    public int ItemCount => Items.Sum(i => i.Quantity);
}

public class SaleItem
{
    public int Id { get; set; }
    public int SaleId { get; set; }
    public Sale? Sale { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    /// <summary>Copied at the time of sale so receipts never change.</summary>
    public string ProductName { get; set; } = "";
    public string Sku { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal UnitCost { get; set; }
    /// <summary>How many units of this line were refunded.</summary>
    public int ReturnedQty { get; set; }
    public decimal LineTotal => UnitPrice * Quantity;
}

public class StockMovement
{
    public int Id { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public MovementType Type { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    /// <summary>Positive = added to stock, negative = removed.</summary>
    public int Quantity { get; set; }
    public int StockAfter { get; set; }
    public string? Reference { get; set; }
    public int? SupplierId { get; set; }
    public decimal? UnitCost { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public string? Notes { get; set; }
}

public class ReturnRequest
{
    public int Id { get; set; }
    public string ReturnNo { get; set; } = "";
    public DateTime Date { get; set; } = DateTime.Now;
    public int SaleId { get; set; }
    public Sale? Sale { get; set; }
    public int SaleItemId { get; set; }
    public SaleItem? SaleItem { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; } = 1;
    public string Reason { get; set; } = "";
    public ItemCondition Condition { get; set; } = ItemCondition.Defective;
    public ReturnResolution Resolution { get; set; } = ReturnResolution.Replacement;
    public ReturnStatus Status { get; set; } = ReturnStatus.Pending;
    public decimal RefundAmount { get; set; }
    public int CreatedById { get; set; }
    public User? CreatedBy { get; set; }
    public int? ProcessedById { get; set; }
    public User? ProcessedBy { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? AdminNote { get; set; }
}

public class AuditLog
{
    public int Id { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public int? UserId { get; set; }
    public string UserName { get; set; } = "";
    public string Action { get; set; } = "";
    public string Details { get; set; } = "";
}

/// <summary>Single row (Id = 1) holding the store settings.</summary>
public class StoreSettings
{
    public int Id { get; set; } = 1;
    public string StoreName { get; set; } = "Ber2Bytes Computer Parts & Accessories";
    public string BranchName { get; set; } = "Dipolog City Branch";
    public string? Address { get; set; } = "Dipolog City, Zamboanga del Norte";
    public string? Phone { get; set; }
    public string ReceiptFooter { get; set; } = "Thank you for shopping at Ber2Bytes! Please keep this receipt for returns.";
    public int DefaultReorderLevel { get; set; } = 5;
    public decimal StaffMaxDiscountPercent { get; set; } = 10;
    /// <summary>A product with stock but no sales in this many days is "dead stock".</summary>
    public int DeadStockDays { get; set; } = 60;
    /// <summary>Selling this many units or fewer in the period counts as "slow-moving".</summary>
    public int SlowMovingMaxQty { get; set; } = 2;
    /// <summary>Returns are accepted within this many days of purchase.</summary>
    public int ReturnWindowDays { get; set; } = 7;
    public bool AutoBackupOnExit { get; set; } = true;
}
