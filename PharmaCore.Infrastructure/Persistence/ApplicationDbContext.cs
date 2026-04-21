using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PharmaCore.Infrastructure.Models;

namespace PharmaCore.Infrastructure.Persistence;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Adjustment> Adjustments { get; set; }

    public virtual DbSet<Batch> Batches { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Expense> Expenses { get; set; }

    public virtual DbSet<Medicine> Medicines { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Purchase> Purchases { get; set; }

    public virtual DbSet<PurchaseItem> PurchaseItems { get; set; }

    public virtual DbSet<PurchaseReturn> PurchaseReturns { get; set; }

    public virtual DbSet<PurchaseReturnItem> PurchaseReturnItems { get; set; }

    public virtual DbSet<Sale> Sales { get; set; }

    public virtual DbSet<SaleItem> SaleItems { get; set; }

    public virtual DbSet<SalesReturn> SalesReturns { get; set; }

    public virtual DbSet<SalesReturnItem> SalesReturnItems { get; set; }

    public virtual DbSet<StockMovement> StockMovements { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Adjustment>(entity =>
        {
            entity.HasKey(e => e.AdjustmentId).HasName("adjustments_pkey");

            entity.ToTable("adjustments");

            entity.Property(e => e.AdjustmentId).HasColumnName("adjustment_id");
            entity.Property(e => e.BatchId).HasColumnName("batch_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deleted_at");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.MedicineId).HasColumnName("medicine_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Batch).WithMany(p => p.Adjustments)
                .HasForeignKey(d => d.BatchId)
                .HasConstraintName("adjustments_batch_id_fkey");

            entity.HasOne(d => d.Medicine).WithMany(p => p.Adjustments)
                .HasForeignKey(d => d.MedicineId)
                .HasConstraintName("adjustments_medicine_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Adjustments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("adjustments_user_id_fkey");
        });

        modelBuilder.Entity<Batch>(entity =>
        {
            entity.HasKey(e => e.BatchId).HasName("batches_pkey");

            entity.ToTable("batches");

            entity.HasIndex(e => e.MedicineId, "idx_batches_medicine");

            entity.Property(e => e.BatchId).HasColumnName("batch_id");
            entity.Property(e => e.BatchNumber).HasColumnName("batch_number");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deleted_at");
            entity.Property(e => e.ExpireDate).HasColumnName("expire_date");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.MedicineId).HasColumnName("medicine_id");
            entity.Property(e => e.PurchasePrice)
                .HasPrecision(12, 2)
                .HasColumnName("purchase_price");
            entity.Property(e => e.QuantityRemaining).HasColumnName("quantity_remaining");
            entity.Property(e => e.QuantityEntered)
                .HasColumnName("quantity_entered")
                .IsRequired(false);
            entity.Property(e => e.SellPrice)                .HasPrecision(12, 2)
                .HasColumnName("sell_price");

            entity.HasOne(d => d.Medicine).WithMany(p => p.Batches)
                .HasForeignKey(d => d.MedicineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("batches_medicine_id_fkey");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("categories_pkey");

            entity.ToTable("categories");

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CategoryName).HasColumnName("category_name");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deleted_at");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.CategoryArabicName).HasColumnName("category_arabic_name");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("customers_pkey");

            entity.ToTable("customers");

            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deleted_at");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.PhoneNumber).HasColumnName("phone_number");
        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(e => e.ExpenseId).HasName("expenses_pkey");

            entity.ToTable("expenses");

            entity.Property(e => e.ExpenseId).HasColumnName("expense_id");
            entity.Property(e => e.Amount)
                .HasPrecision(12, 2)
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("expenses_user_id_fkey");
        });

        modelBuilder.Entity<Medicine>(entity =>
        {
            entity.HasKey(e => e.MedicineId).HasName("medicines_pkey");

            entity.ToTable("medicines");

            entity.HasIndex(e => e.Barcode, "idx_medicine_barcode");

            entity.HasIndex(e => e.Barcode, "medicines_barcode_key").IsUnique();

            entity.Property(e => e.MedicineId).HasColumnName("medicine_id");
            entity.Property(e => e.ArabicName).HasColumnName("arabic_name");
            entity.Property(e => e.Barcode).HasColumnName("barcode");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deleted_at");
           
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Unit).HasColumnName("unit");

            entity.HasOne(d => d.Category).WithMany(p => p.Medicines)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("medicines_category_id_fkey");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("payments_pkey");

            entity.ToTable("payments");

            entity.HasIndex(e => new { e.ReferenceType, e.ReferenceId }, "idx_payments_reference");

            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.Amount)
                .HasPrecision(12, 2)
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Method).HasColumnName("method");
            entity.Property(e => e.ReferenceId).HasColumnName("reference_id");
            entity.Property(e => e.ReferenceType).HasColumnName("reference_type");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Payments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("payments_user_id_fkey");
        });

        modelBuilder.Entity<Purchase>(entity =>
        {
            entity.HasKey(e => e.PurchaseId).HasName("purchases_pkey");

            entity.ToTable("purchases");

            entity.Property(e => e.PurchaseId).HasColumnName("purchase_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deleted_at");
            entity.Property(e => e.InvoiceNumber).HasColumnName("invoice_number");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(12, 2)
                .HasColumnName("total_amount");

            entity.HasOne(d => d.Supplier).WithMany(p => p.Purchases)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("purchases_supplier_id_fkey");
        });

        modelBuilder.Entity<PurchaseItem>(entity =>
        {
            entity.HasKey(e => e.PurchaseItemId).HasName("purchase_items_pkey");

            entity.ToTable("purchase_items");

            entity.Property(e => e.PurchaseItemId).HasColumnName("purchase_item_id");
            entity.Property(e => e.BatchId).HasColumnName("batch_id");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deleted_at");
            entity.Property(e => e.ExpireDate).HasColumnName("expire_date");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.MedicineId).HasColumnName("medicine_id");
            entity.Property(e => e.PurchaseId).HasColumnName("purchase_id");
            entity.Property(e => e.PurchasePrice)
                .HasPrecision(12, 2)
                .HasColumnName("purchase_price");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.SellPrice)
                .HasPrecision(12, 2)
                .HasColumnName("sell_price");

            entity.HasOne(d => d.Batch).WithMany(p => p.PurchaseItems)
                .HasForeignKey(d => d.BatchId)
                .HasConstraintName("purchase_items_batch_id_fkey");

            entity.HasOne(d => d.Medicine).WithMany(p => p.PurchaseItems)
                .HasForeignKey(d => d.MedicineId)
                .HasConstraintName("purchase_items_medicine_id_fkey");

            entity.HasOne(d => d.Purchase).WithMany(p => p.PurchaseItems)
                .HasForeignKey(d => d.PurchaseId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("purchase_items_purchase_id_fkey");
        });

        modelBuilder.Entity<PurchaseReturn>(entity =>
        {
            entity.HasKey(e => e.PurchaseReturnId).HasName("purchase_returns_pkey");

            entity.ToTable("purchase_returns");

            entity.Property(e => e.PurchaseReturnId).HasColumnName("purchase_return_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deleted_at");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.PurchaseId).HasColumnName("purchase_id");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(12, 2)
                .HasColumnName("total_amount");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Purchase).WithMany(p => p.PurchaseReturns)
                .HasForeignKey(d => d.PurchaseId)
                .HasConstraintName("purchase_returns_purchase_id_fkey");

            entity.HasOne(d => d.Supplier).WithMany(p => p.PurchaseReturns)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("purchase_returns_supplier_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.PurchaseReturns)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("purchase_returns_user_id_fkey");
        });

        modelBuilder.Entity<PurchaseReturnItem>(entity =>
        {
            entity.HasKey(e => e.PurchaseReturnItemId).HasName("purchase_return_items_pkey");

            entity.ToTable("purchase_return_items");

            entity.Property(e => e.PurchaseReturnItemId).HasColumnName("purchase_return_item_id");
            entity.Property(e => e.BatchId).HasColumnName("batch_id");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deleted_at");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.PurchaseItemId).HasColumnName("purchase_item_id");
            entity.Property(e => e.PurchaseReturnId).HasColumnName("purchase_return_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.TotalPrice)
                .HasPrecision(12, 2)
                .HasColumnName("total_price");
            entity.Property(e => e.UnitPrice)
                .HasPrecision(12, 2)
                .HasColumnName("unit_price");

            entity.HasOne(d => d.Batch).WithMany(p => p.PurchaseReturnItems)
                .HasForeignKey(d => d.BatchId)
                .HasConstraintName("purchase_return_items_batch_id_fkey");

            entity.HasOne(d => d.PurchaseItem).WithMany(p => p.PurchaseReturnItems)
                .HasForeignKey(d => d.PurchaseItemId)
                .HasConstraintName("purchase_return_items_purchase_item_id_fkey");

            entity.HasOne(d => d.PurchaseReturn).WithMany(p => p.PurchaseReturnItems)
                .HasForeignKey(d => d.PurchaseReturnId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("purchase_return_items_purchase_return_id_fkey");
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasKey(e => e.SaleId).HasName("sales_pkey");

            entity.ToTable("sales");

            entity.HasIndex(e => e.CustomerId, "idx_sales_customer");

            entity.Property(e => e.SaleId).HasColumnName("sale_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Discount)
                .HasPrecision(12, 2)
                .HasColumnName("discount");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(12, 2)
                .HasColumnName("total_amount");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Customer).WithMany(p => p.Sales)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("sales_customer_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Sales)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("sales_user_id_fkey");
        });

        modelBuilder.Entity<SaleItem>(entity =>
        {
            entity.HasKey(e => e.SaleItemId).HasName("sale_items_pkey");

            entity.ToTable("sale_items");

            entity.Property(e => e.SaleItemId).HasColumnName("sale_item_id");
            entity.Property(e => e.BatchId).HasColumnName("batch_id");
            entity.Property(e => e.MedicineId).HasColumnName("medicine_id");
            entity.Property(e => e.PurchasePrice)
                .HasPrecision(12, 2)
                .HasColumnName("purchase_price");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.SaleId).HasColumnName("sale_id");
            entity.Property(e => e.TotalPrice)
                .HasPrecision(12, 2)
                .HasColumnName("total_price");
            entity.Property(e => e.UnitPrice)
                .HasPrecision(12, 2)
                .HasColumnName("unit_price");

            entity.HasOne(d => d.Batch).WithMany(p => p.SaleItems)
                .HasForeignKey(d => d.BatchId)
                .HasConstraintName("sale_items_batch_id_fkey");

            entity.HasOne(d => d.Medicine).WithMany(p => p.SaleItems)
                .HasForeignKey(d => d.MedicineId)
                .HasConstraintName("sale_items_medicine_id_fkey");

            entity.HasOne(d => d.Sale).WithMany(p => p.SaleItems)
                .HasForeignKey(d => d.SaleId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("sale_items_sale_id_fkey");
        });

        modelBuilder.Entity<SalesReturn>(entity =>
        {
            entity.HasKey(e => e.SalesReturnId).HasName("sales_returns_pkey");

            entity.ToTable("sales_returns");

            entity.Property(e => e.SalesReturnId).HasColumnName("sales_return_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deleted_at");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.SaleId).HasColumnName("sale_id");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(12, 2)
                .HasColumnName("total_amount");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Customer).WithMany(p => p.SalesReturns)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("sales_returns_customer_id_fkey");

            entity.HasOne(d => d.Sale).WithMany(p => p.SalesReturns)
                .HasForeignKey(d => d.SaleId)
                .HasConstraintName("sales_returns_sale_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.SalesReturns)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("sales_returns_user_id_fkey");
        });

        modelBuilder.Entity<SalesReturnItem>(entity =>
        {
            entity.HasKey(e => e.SalesReturnItemId).HasName("sales_return_items_pkey");

            entity.ToTable("sales_return_items");

            entity.Property(e => e.SalesReturnItemId).HasColumnName("sales_return_item_id");
            entity.Property(e => e.BatchId).HasColumnName("batch_id");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deleted_at");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.SaleItemId).HasColumnName("sale_item_id");
            entity.Property(e => e.SalesReturnId).HasColumnName("sales_return_id");
            entity.Property(e => e.TotalPrice)
                .HasPrecision(12, 2)
                .HasColumnName("total_price");
            entity.Property(e => e.UnitPrice)
                .HasPrecision(12, 2)
                .HasColumnName("unit_price");

            entity.HasOne(d => d.Batch).WithMany(p => p.SalesReturnItems)
                .HasForeignKey(d => d.BatchId)
                .HasConstraintName("sales_return_items_batch_id_fkey");

            entity.HasOne(d => d.SaleItem).WithMany(p => p.SalesReturnItems)
                .HasForeignKey(d => d.SaleItemId)
                .HasConstraintName("sales_return_items_sale_item_id_fkey");

            entity.HasOne(d => d.SalesReturn).WithMany(p => p.SalesReturnItems)
                .HasForeignKey(d => d.SalesReturnId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("sales_return_items_sales_return_id_fkey");
        });

        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.HasKey(e => e.StockMovementId).HasName("stock_movements_pkey");

            entity.ToTable("stock_movements");

            entity.HasIndex(e => e.MedicineId, "idx_stock_movements_medicine");

            entity.Property(e => e.StockMovementId).HasColumnName("stock_movement_id");
            entity.Property(e => e.BatchId).HasColumnName("batch_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deleted_at");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.MedicineId).HasColumnName("medicine_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.ReferenceId).HasColumnName("reference_id");
            entity.Property(e => e.ReferenceType).HasColumnName("reference_type");
            entity.Property(e => e.Type).HasColumnName("type");

            entity.HasOne(d => d.Batch).WithMany(p => p.StockMovements)
                .HasForeignKey(d => d.BatchId)
                .HasConstraintName("stock_movements_batch_id_fkey");

            entity.HasOne(d => d.Medicine).WithMany(p => p.StockMovements)
                .HasForeignKey(d => d.MedicineId)
                .HasConstraintName("stock_movements_medicine_id_fkey");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.SupplierId).HasName("suppliers_pkey");

            entity.ToTable("suppliers");

            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deleted_at");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.PhoneNumber).HasColumnName("phone_number");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("users_pkey");

            entity.ToTable("users");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deleted_at");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.PhoneNumber).HasColumnName("phone_number");
            entity.Property(e => e.Role).HasColumnName("role");
            entity.Property(e => e.UserName).HasColumnName("user_name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
