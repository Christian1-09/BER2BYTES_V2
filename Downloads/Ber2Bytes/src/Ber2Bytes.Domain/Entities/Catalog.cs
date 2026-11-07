namespace Ber2Bytes.Domain.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string Icon { get; set; } = "box";
    public string Color { get; set; } = "#E11D2A";
    public List<Product> Products { get; set; } = [];
}

public class Supplier
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string PaymentTerms { get; set; } = "COD";
    public DateTime? LastDeliveryAt { get; set; }
    public List<Product> Products { get; set; } = [];
}

public class Product
{
    public int Id { get; set; }
    /// <summary>The store's own unique product code, e.g. LP-ASU-X1504.</summary>
    public string Sku { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Brand { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int Stock { get; set; }
    public int ReorderLevel { get; set; } = 5;
    public string? Description { get; set; }
    /// <summary>File name of the product photo (null = no photo).</summary>
    public string? ImageFile { get; set; }
    /// <summary>Archived products are hidden from the POS but keep their sales history.</summary>
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }

    // Business rules that belong to the product itself.
    public bool IsLowStock => Stock <= ReorderLevel;
    public bool IsOutOfStock => Stock <= 0;
    public decimal ProfitPerUnit => SellingPrice - CostPrice;
    public string StockStatus => IsOutOfStock ? "Out of Stock" : IsLowStock ? "Low Stock" : "In Stock";
}
