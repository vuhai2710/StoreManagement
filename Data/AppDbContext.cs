using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using StoreManagement.Models;

namespace StoreManagement.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CartItems> CartItems { get; set; }

    public virtual DbSet<Carts> Carts { get; set; }

    public virtual DbSet<Categories> Categories { get; set; }

    public virtual DbSet<ChatConversations> ChatConversations { get; set; }

    public virtual DbSet<ChatMessages> ChatMessages { get; set; }

    public virtual DbSet<Customers> Customers { get; set; }

    public virtual DbSet<Employees> Employees { get; set; }

    public virtual DbSet<FlywaySchemaHistory> FlywaySchemaHistory { get; set; }

    public virtual DbSet<InventoryTransactions> InventoryTransactions { get; set; }

    public virtual DbSet<Notifications> Notifications { get; set; }

    public virtual DbSet<OrderDetails> OrderDetails { get; set; }

    public virtual DbSet<OrderReturnItems> OrderReturnItems { get; set; }

    public virtual DbSet<OrderReturns> OrderReturns { get; set; }

    public virtual DbSet<Orders> Orders { get; set; }

    public virtual DbSet<PasswordResetTokens> PasswordResetTokens { get; set; }

    public virtual DbSet<ProductImages> ProductImages { get; set; }

    public virtual DbSet<ProductPromotions> ProductPromotions { get; set; }

    public virtual DbSet<ProductReviews> ProductReviews { get; set; }

    public virtual DbSet<ProductView> ProductView { get; set; }

    public virtual DbSet<Products> Products { get; set; }

    public virtual DbSet<PromotionRules> PromotionRules { get; set; }

    public virtual DbSet<PromotionUsage> PromotionUsage { get; set; }

    public virtual DbSet<Promotions> Promotions { get; set; }

    public virtual DbSet<PurchaseOrderDetails> PurchaseOrderDetails { get; set; }

    public virtual DbSet<PurchaseOrders> PurchaseOrders { get; set; }

    public virtual DbSet<Shipments> Shipments { get; set; }

    public virtual DbSet<ShippingAddresses> ShippingAddresses { get; set; }

    public virtual DbSet<Suppliers> Suppliers { get; set; }

    public virtual DbSet<SystemSettings> SystemSettings { get; set; }

    public virtual DbSet<Users> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_unicode_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<CartItems>(entity =>
        {
            entity.HasKey(e => e.IdCartItem).HasName("PRIMARY");

            entity
                .ToTable("cart_items")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.IdCart, "idx_cart_items_cart");

            entity.HasIndex(e => e.IdProduct, "idx_cart_items_product");

            entity.HasIndex(e => new { e.IdCart, e.IdProduct }, "uq_cart_items_cart_product").IsUnique();

            entity.Property(e => e.IdCartItem).HasColumnName("id_cart_item");
            entity.Property(e => e.AddedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("added_at");
            entity.Property(e => e.IdCart).HasColumnName("id_cart");
            entity.Property(e => e.IdProduct).HasColumnName("id_product");
            entity.Property(e => e.Quantity)
                .HasDefaultValueSql("'1'")
                .HasColumnName("quantity");

            entity.HasOne(d => d.IdCartNavigation).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.IdCart)
                .HasConstraintName("fk_cart_items_cart");

            entity.HasOne(d => d.IdProductNavigation).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.IdProduct)
                .HasConstraintName("fk_cart_items_product");
        });

        modelBuilder.Entity<Carts>(entity =>
        {
            entity.HasKey(e => e.IdCart).HasName("PRIMARY");

            entity
                .ToTable("carts")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.IdCustomer, "uq_carts_customer").IsUnique();

            entity.Property(e => e.IdCart).HasColumnName("id_cart");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.IdCustomer).HasColumnName("id_customer");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.IdCustomerNavigation).WithOne(p => p.Carts)
                .HasForeignKey<Carts>(d => d.IdCustomer)
                .HasConstraintName("fk_carts_customer");
        });

        modelBuilder.Entity<Categories>(entity =>
        {
            entity.HasKey(e => e.IdCategory).HasName("PRIMARY");

            entity
                .ToTable("categories")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.CategoryName, "uq_categories_name").IsUnique();

            entity.Property(e => e.IdCategory).HasColumnName("id_category");
            entity.Property(e => e.CategoryName).HasColumnName("category_name");
            entity.Property(e => e.CodePrefix)
                .HasMaxLength(10)
                .HasDefaultValueSql("''")
                .HasComment("Prefix cho SKU: SP, LT, AO...")
                .HasColumnName("code_prefix");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<ChatConversations>(entity =>
        {
            entity.HasKey(e => e.IdConversation).HasName("PRIMARY");

            entity
                .ToTable("chat_conversations")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.IdCustomer, "idx_chat_conversations_customer");

            entity.HasIndex(e => e.LastViewedByAdminAt, "idx_chat_conversations_last_viewed_admin");

            entity.HasIndex(e => e.LastViewedByCustomerAt, "idx_chat_conversations_last_viewed_customer");

            entity.HasIndex(e => e.Status, "idx_chat_conversations_status");

            entity.Property(e => e.IdConversation).HasColumnName("id_conversation");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.IdCustomer).HasColumnName("id_customer");
            entity.Property(e => e.LastViewedByAdminAt)
                .HasComment("Thời gian admin/employee xem conversation lần cuối")
                .HasColumnType("datetime")
                .HasColumnName("last_viewed_by_admin_at");
            entity.Property(e => e.LastViewedByCustomerAt)
                .HasComment("Thời gian customer xem conversation lần cuối")
                .HasColumnType("datetime")
                .HasColumnName("last_viewed_by_customer_at");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'OPEN'")
                .HasColumnType("enum('OPEN','CLOSED')")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.IdCustomerNavigation).WithMany(p => p.ChatConversations)
                .HasForeignKey(d => d.IdCustomer)
                .HasConstraintName("fk_chat_conversations_customer");
        });

        modelBuilder.Entity<ChatMessages>(entity =>
        {
            entity.HasKey(e => e.IdMessage).HasName("PRIMARY");

            entity
                .ToTable("chat_messages")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.IdConversation, "idx_chat_messages_conversation");

            entity.HasIndex(e => new { e.IdConversation, e.CreatedAt }, "idx_chat_messages_created");

            entity.HasIndex(e => new { e.SenderId, e.SenderType }, "idx_chat_messages_sender");

            entity.Property(e => e.IdMessage).HasColumnName("id_message");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.IdConversation).HasColumnName("id_conversation");
            entity.Property(e => e.Message)
                .HasColumnType("text")
                .HasColumnName("message");
            entity.Property(e => e.SenderId)
                .HasComment("ID of user (customer/employee/admin)")
                .HasColumnName("sender_id");
            entity.Property(e => e.SenderType)
                .HasColumnType("enum('CUSTOMER','ADMIN','EMPLOYEE')")
                .HasColumnName("sender_type");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.IdConversationNavigation).WithMany(p => p.ChatMessages)
                .HasForeignKey(d => d.IdConversation)
                .HasConstraintName("fk_chat_messages_conversation");
        });

        modelBuilder.Entity<Customers>(entity =>
        {
            entity.HasKey(e => e.IdCustomer).HasName("PRIMARY");

            entity
                .ToTable("customers")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.CustomerName, "idx_customer_name");

            entity.HasIndex(e => e.PhoneNumber, "idx_customers_phone").IsUnique();

            entity.HasIndex(e => e.IdUser, "uq_customers_user").IsUnique();

            entity.Property(e => e.IdCustomer).HasColumnName("id_customer");
            entity.Property(e => e.Address)
                .HasColumnType("text")
                .HasColumnName("address");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerName).HasColumnName("customer_name");
            entity.Property(e => e.CustomerType)
                .HasDefaultValueSql("'REGULAR'")
                .HasColumnType("enum('VIP','REGULAR')")
                .HasColumnName("customer_type");
            entity.Property(e => e.IdUser)
                .HasComment("Liên kết tới bảng users")
                .HasColumnName("id_user");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .HasColumnName("phone_number");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.IdUserNavigation).WithOne(p => p.Customers)
                .HasForeignKey<Customers>(d => d.IdUser)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_customers_user");
        });

        modelBuilder.Entity<Employees>(entity =>
        {
            entity.HasKey(e => e.IdEmployee).HasName("PRIMARY");

            entity
                .ToTable("employees")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.EmployeeName, "idx_employees_name");

            entity.HasIndex(e => e.PhoneNumber, "idx_employees_phone");

            entity.HasIndex(e => e.IdUser, "uq_employees_user").IsUnique();

            entity.Property(e => e.IdEmployee).HasColumnName("id_employee");
            entity.Property(e => e.Address)
                .HasColumnType("text")
                .HasColumnName("address");
            entity.Property(e => e.BaseSalary)
                .HasPrecision(12, 2)
                .HasColumnName("base_salary");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.EmployeeName).HasColumnName("employee_name");
            entity.Property(e => e.HireDate).HasColumnName("hire_date");
            entity.Property(e => e.IdUser)
                .HasComment("Liên kết tới bảng users")
                .HasColumnName("id_user");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .HasColumnName("phone_number");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.IdUserNavigation).WithOne(p => p.Employees)
                .HasForeignKey<Employees>(d => d.IdUser)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_employees_user");
        });

        modelBuilder.Entity<FlywaySchemaHistory>(entity =>
        {
            entity.HasKey(e => e.InstalledRank).HasName("PRIMARY");

            entity.ToTable("flyway_schema_history");

            entity.HasIndex(e => e.Success, "flyway_schema_history_s_idx");

            entity.Property(e => e.InstalledRank)
                .ValueGeneratedNever()
                .HasColumnName("installed_rank");
            entity.Property(e => e.Checksum).HasColumnName("checksum");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.ExecutionTime).HasColumnName("execution_time");
            entity.Property(e => e.InstalledBy)
                .HasMaxLength(100)
                .HasColumnName("installed_by");
            entity.Property(e => e.InstalledOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("installed_on");
            entity.Property(e => e.Script)
                .HasMaxLength(1000)
                .HasColumnName("script");
            entity.Property(e => e.Success).HasColumnName("success");
            entity.Property(e => e.Type)
                .HasMaxLength(20)
                .HasColumnName("type");
            entity.Property(e => e.Version)
                .HasMaxLength(50)
                .HasColumnName("version");
        });

        modelBuilder.Entity<InventoryTransactions>(entity =>
        {
            entity.HasKey(e => e.IdTransaction).HasName("PRIMARY");

            entity
                .ToTable("inventory_transactions")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.IdEmployee, "idx_inv_employee");

            entity.HasIndex(e => e.IdProduct, "idx_inv_product");

            entity.HasIndex(e => new { e.ReferenceType, e.ReferenceId }, "idx_inv_ref");

            entity.Property(e => e.IdTransaction).HasColumnName("id_transaction");
            entity.Property(e => e.IdEmployee).HasColumnName("id_employee");
            entity.Property(e => e.IdProduct).HasColumnName("id_product");
            entity.Property(e => e.Notes)
                .HasColumnType("text")
                .HasColumnName("notes");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.ReferenceId)
                .HasComment("ID của purchase_orders / orders (tuỳ loại)")
                .HasColumnName("reference_id");
            entity.Property(e => e.ReferenceType)
                .HasColumnType("enum('PURCHASE_ORDER','SALE_ORDER','ADJUSTMENT','SALE_RETURN','SALE_EXCHANGE')")
                .HasColumnName("reference_type");
            entity.Property(e => e.TransactionDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("transaction_date");
            entity.Property(e => e.TransactionType)
                .HasColumnType("enum('IN','OUT')")
                .HasColumnName("transaction_type");

            entity.HasOne(d => d.IdEmployeeNavigation).WithMany(p => p.InventoryTransactions)
                .HasForeignKey(d => d.IdEmployee)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_inv_employee");

            entity.HasOne(d => d.IdProductNavigation).WithMany(p => p.InventoryTransactions)
                .HasForeignKey(d => d.IdProduct)
                .HasConstraintName("fk_inv_product");
        });

        modelBuilder.Entity<Notifications>(entity =>
        {
            entity.HasKey(e => e.IdNotification).HasName("PRIMARY");

            entity
                .ToTable("notifications", tb => tb.HasComment("[DEPRECATED] Notification module removed. Table preserved for historical data."))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.CreatedAt, "idx_created_at").IsDescending();

            entity.HasIndex(e => e.NotificationType, "idx_notification_type");

            entity.HasIndex(e => new { e.IdUser, e.IsRead }, "idx_user_read");

            entity.Property(e => e.IdNotification).HasColumnName("id_notification");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.IdUser)
                .HasComment("User nhận thông báo")
                .HasColumnName("id_user");
            entity.Property(e => e.IsRead)
                .HasDefaultValueSql("'0'")
                .HasComment("Đã đọc chưa")
                .HasColumnName("is_read");
            entity.Property(e => e.Message)
                .HasComment("Nội dung thông báo")
                .HasColumnType("text")
                .HasColumnName("message");
            entity.Property(e => e.NotificationType)
                .HasComment("Loại thông báo")
                .HasColumnType("enum('ORDER_STATUS','LOW_STOCK','NEW_ORDER','NEW_CUSTOMER','INVENTORY_UPDATE','PROMOTION')")
                .HasColumnName("notification_type");
            entity.Property(e => e.ReferenceId)
                .HasComment("ID đối tượng liên quan")
                .HasColumnName("reference_id");
            entity.Property(e => e.ReferenceType)
                .HasComment("Loại đối tượng liên quan")
                .HasColumnType("enum('ORDER','PRODUCT','CUSTOMER','IMPORT_ORDER','OTHER')")
                .HasColumnName("reference_type");
            entity.Property(e => e.SentEmail)
                .HasDefaultValueSql("'0'")
                .HasComment("Đã gửi email chưa")
                .HasColumnName("sent_email");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasComment("Tiêu đề thông báo")
                .HasColumnName("title");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("notifications_ibfk_1");
        });

        modelBuilder.Entity<OrderDetails>(entity =>
        {
            entity.HasKey(e => e.IdOrderDetail).HasName("PRIMARY");

            entity
                .ToTable("order_details")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.IdOrder, "idx_order_details_order");

            entity.HasIndex(e => e.IdProduct, "idx_order_details_product");

            entity.HasIndex(e => new { e.IdOrder, e.IdProduct }, "uq_order_details_order_product").IsUnique();

            entity.Property(e => e.IdOrderDetail).HasColumnName("id_order_detail");
            entity.Property(e => e.IdOrder).HasColumnName("id_order");
            entity.Property(e => e.IdProduct).HasColumnName("id_product");
            entity.Property(e => e.Price)
                .HasPrecision(12, 2)
                .HasComment("Giá tại thời điểm mua")
                .HasColumnName("price");
            entity.Property(e => e.ProductCodeSnapshot)
                .HasMaxLength(100)
                .HasComment("Mã sản phẩm tại thời điểm mua")
                .HasColumnName("product_code_snapshot");
            entity.Property(e => e.ProductImageSnapshot)
                .HasMaxLength(500)
                .HasComment("URL ảnh tại thời điểm mua")
                .HasColumnName("product_image_snapshot");
            entity.Property(e => e.ProductNameSnapshot)
                .HasMaxLength(255)
                .HasComment("Tên sản phẩm tại thời điểm mua")
                .HasColumnName("product_name_snapshot");
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(d => d.IdOrderNavigation).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.IdOrder)
                .HasConstraintName("fk_order_details_order");

            entity.HasOne(d => d.IdProductNavigation).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.IdProduct)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_order_details_product");
        });

        modelBuilder.Entity<OrderReturnItems>(entity =>
        {
            entity.HasKey(e => e.IdReturnItem).HasName("PRIMARY");

            entity.ToTable("order_return_items");

            entity.HasIndex(e => e.ExchangeProductId, "fk_return_exchange");

            entity.HasIndex(e => e.IdReturn, "fk_return_item");

            entity.HasIndex(e => e.IdOrderDetail, "fk_return_item_order_detail");

            entity.Property(e => e.IdReturnItem).HasColumnName("id_return_item");
            entity.Property(e => e.ExchangeProductId).HasColumnName("exchange_product_id");
            entity.Property(e => e.ExchangeQuantity).HasColumnName("exchange_quantity");
            entity.Property(e => e.IdOrderDetail).HasColumnName("id_order_detail");
            entity.Property(e => e.IdReturn).HasColumnName("id_return");
            entity.Property(e => e.LineRefundAmount)
                .HasPrecision(15, 2)
                .HasColumnName("line_refund_amount");
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(d => d.ExchangeProduct).WithMany(p => p.OrderReturnItems)
                .HasForeignKey(d => d.ExchangeProductId)
                .HasConstraintName("fk_return_exchange");

            entity.HasOne(d => d.IdOrderDetailNavigation).WithMany(p => p.OrderReturnItems)
                .HasForeignKey(d => d.IdOrderDetail)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_return_item_order_detail");

            entity.HasOne(d => d.IdReturnNavigation).WithMany(p => p.OrderReturnItems)
                .HasForeignKey(d => d.IdReturn)
                .HasConstraintName("fk_return_item");
        });

        modelBuilder.Entity<OrderReturns>(entity =>
        {
            entity.HasKey(e => e.IdReturn).HasName("PRIMARY");

            entity.ToTable("order_returns");

            entity.HasIndex(e => e.ProcessedByEmployeeId, "fk_return_employee");

            entity.HasIndex(e => e.CreatedByCustomerId, "idx_return_customer");

            entity.HasIndex(e => e.IdOrder, "idx_return_order");

            entity.HasIndex(e => e.Status, "idx_return_status");

            entity.Property(e => e.IdReturn).HasColumnName("id_return");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedByCustomerId).HasColumnName("created_by_customer_id");
            entity.Property(e => e.IdOrder).HasColumnName("id_order");
            entity.Property(e => e.NoteAdmin)
                .HasColumnType("text")
                .HasColumnName("note_admin");
            entity.Property(e => e.ProcessedByEmployeeId).HasColumnName("processed_by_employee_id");
            entity.Property(e => e.Reason)
                .HasColumnType("text")
                .HasColumnName("reason");
            entity.Property(e => e.RefundAmount)
                .HasPrecision(15, 2)
                .HasColumnName("refund_amount");
            entity.Property(e => e.ReturnType)
                .HasColumnType("enum('RETURN','EXCHANGE')")
                .HasColumnName("return_type");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'REQUESTED'")
                .HasColumnType("enum('REQUESTED','APPROVED','REJECTED','COMPLETED','CANCELED')")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.CreatedByCustomer).WithMany(p => p.OrderReturns)
                .HasForeignKey(d => d.CreatedByCustomerId)
                .HasConstraintName("fk_return_customer");

            entity.HasOne(d => d.IdOrderNavigation).WithMany(p => p.OrderReturns)
                .HasForeignKey(d => d.IdOrder)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_return_order");

            entity.HasOne(d => d.ProcessedByEmployee).WithMany(p => p.OrderReturns)
                .HasForeignKey(d => d.ProcessedByEmployeeId)
                .HasConstraintName("fk_return_employee");
        });

        modelBuilder.Entity<Orders>(entity =>
        {
            entity.HasKey(e => e.IdOrder).HasName("PRIMARY");

            entity
                .ToTable("orders")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.IdCustomer, "idx_orders_customer");

            entity.HasIndex(e => e.IdEmployee, "idx_orders_employee");

            entity.HasIndex(e => e.PaymentLinkId, "idx_orders_payment_link_id");

            entity.HasIndex(e => e.IdPromotion, "idx_orders_promotion");

            entity.HasIndex(e => e.IdPromotionRule, "idx_orders_promotion_rule");

            entity.HasIndex(e => e.IdShippingAddress, "idx_orders_shipping_address");

            entity.HasIndex(e => e.IdShippingPromotion, "idx_orders_shipping_promotion");

            entity.Property(e => e.IdOrder).HasColumnName("id_order");
            entity.Property(e => e.CompletedAt)
                .HasColumnType("timestamp")
                .HasColumnName("completed_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.DeliveredAt)
                .HasComment("Thời điểm customer xác nhận đã nhận hàng")
                .HasColumnType("timestamp")
                .HasColumnName("delivered_at");
            entity.Property(e => e.Discount)
                .HasPrecision(15, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("discount");
            entity.Property(e => e.FinalAmount)
                .HasPrecision(15, 2)
                .HasComputedColumnSql("greatest(((ifnull(`total_amount`,0) + ifnull(`shipping_fee`,0)) - ifnull(`discount`,0)),0)", true)
                .HasComment("Tổng tiền cuối = total_amount + shipping_fee - discount (>=0)")
                .HasColumnName("final_amount");
            entity.Property(e => e.IdCustomer).HasColumnName("id_customer");
            entity.Property(e => e.IdEmployee).HasColumnName("id_employee");
            entity.Property(e => e.IdPromotion)
                .HasComment("Reference đến promotions (nếu sử dụng mã giảm giá)")
                .HasColumnName("id_promotion");
            entity.Property(e => e.IdPromotionRule)
                .HasComment("Reference đến promotion_rules (nếu áp dụng discount tự động)")
                .HasColumnName("id_promotion_rule");
            entity.Property(e => e.IdShippingAddress)
                .HasComment("Reference đến shipping_addresses")
                .HasColumnName("id_shipping_address");
            entity.Property(e => e.IdShippingPromotion).HasColumnName("id_shipping_promotion");
            entity.Property(e => e.InvoicePrinted)
                .HasDefaultValueSql("'0'")
                .HasColumnName("invoice_printed");
            entity.Property(e => e.InvoicePrintedAt)
                .HasColumnType("datetime")
                .HasColumnName("invoice_printed_at");
            entity.Property(e => e.InvoicePrintedBy).HasColumnName("invoice_printed_by");
            entity.Property(e => e.Notes)
                .HasColumnType("text")
                .HasColumnName("notes");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("order_date");
            entity.Property(e => e.PaymentLinkId)
                .HasComment("PayOS payment link ID - được set khi tạo payment link thành công")
                .HasColumnName("payment_link_id");
            entity.Property(e => e.PaymentMethod)
                .HasColumnType("enum('CASH','TRANSFER','ZALOPAY','PAYOS')")
                .HasColumnName("payment_method");
            entity.Property(e => e.PromotionCode)
                .HasMaxLength(50)
                .HasComment("Mã giảm giá được sử dụng")
                .HasColumnName("promotion_code");
            entity.Property(e => e.ReturnWindowDays)
                .HasComment("Snapshot số ngày cho phép đổi/trả tại thời điểm hoàn thành đơn")
                .HasColumnName("return_window_days");
            entity.Property(e => e.ShippingAddressSnapshot)
                .HasComment("Snapshot của địa chỉ tại thời điểm đặt hàng")
                .HasColumnType("text")
                .HasColumnName("shipping_address_snapshot");
            entity.Property(e => e.ShippingDiscount)
                .HasPrecision(15, 2)
                .HasDefaultValueSql("'0.00'")
                .HasComment("Giảm giá phí vận chuyển riêng biệt với giảm giá đơn hàng")
                .HasColumnName("shipping_discount");
            entity.Property(e => e.ShippingFee)
                .HasPrecision(12, 2)
                .HasComment("Phí giao hàng")
                .HasColumnName("shipping_fee");
            entity.Property(e => e.ShippingPromotionCode)
                .HasMaxLength(50)
                .HasComment("Mã giảm giá phí vận chuyển đã sử dụng")
                .HasColumnName("shipping_promotion_code");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'PENDING'")
                .HasColumnType("enum('PENDING','CONFIRMED','COMPLETED','CANCELED')")
                .HasColumnName("status");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(15, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("total_amount");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.IdCustomerNavigation).WithMany(p => p.Orders)
                .HasForeignKey(d => d.IdCustomer)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_orders_customer");

            entity.HasOne(d => d.IdEmployeeNavigation).WithMany(p => p.Orders)
                .HasForeignKey(d => d.IdEmployee)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_orders_employee");

            entity.HasOne(d => d.IdPromotionNavigation).WithMany(p => p.OrdersIdPromotionNavigation)
                .HasForeignKey(d => d.IdPromotion)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_orders_promotion");

            entity.HasOne(d => d.IdPromotionRuleNavigation).WithMany(p => p.Orders)
                .HasForeignKey(d => d.IdPromotionRule)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_orders_promotion_rule");

            entity.HasOne(d => d.IdShippingAddressNavigation).WithMany(p => p.Orders)
                .HasForeignKey(d => d.IdShippingAddress)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_orders_shipping_address");

            entity.HasOne(d => d.IdShippingPromotionNavigation).WithMany(p => p.OrdersIdShippingPromotionNavigation)
                .HasForeignKey(d => d.IdShippingPromotion)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_orders_shipping_promotion");
        });

        modelBuilder.Entity<PasswordResetTokens>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("password_reset_tokens");

            entity.HasIndex(e => e.ExpiresAt, "idx_expires_at");

            entity.HasIndex(e => e.Token, "idx_token").IsUnique();

            entity.HasIndex(e => e.UserId, "idx_user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt)
                .HasColumnType("datetime")
                .HasColumnName("expires_at");
            entity.Property(e => e.Token).HasColumnName("token");
            entity.Property(e => e.Used)
                .HasDefaultValueSql("'0'")
                .HasColumnName("used");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.PasswordResetTokens)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("password_reset_tokens_ibfk_1");
        });

        modelBuilder.Entity<ProductImages>(entity =>
        {
            entity.HasKey(e => e.IdProductImage).HasName("PRIMARY");

            entity
                .ToTable("product_images")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => new { e.IdProduct, e.DisplayOrder }, "idx_product_images_order");

            entity.HasIndex(e => new { e.IdProduct, e.IsPrimary }, "idx_product_images_primary");

            entity.HasIndex(e => e.IdProduct, "idx_product_images_product");

            entity.Property(e => e.IdProductImage).HasColumnName("id_product_image");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.DisplayOrder)
                .HasDefaultValueSql("'0'")
                .HasColumnName("display_order");
            entity.Property(e => e.IdProduct).HasColumnName("id_product");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");
            entity.Property(e => e.IsPrimary)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_primary");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.IdProductNavigation).WithMany(p => p.ProductImages)
                .HasForeignKey(d => d.IdProduct)
                .HasConstraintName("fk_product_images_product");
        });

        modelBuilder.Entity<ProductPromotions>(entity =>
        {
            entity.HasKey(e => e.IdProductPromotion).HasName("PRIMARY");

            entity.ToTable("product_promotions");

            entity.HasIndex(e => new { e.IsActive, e.ValidFrom, e.ValidTo }, "idx_product_promotions_active_valid");

            entity.HasIndex(e => e.IdProduct, "idx_product_promotions_product");

            entity.Property(e => e.IdProductPromotion).HasColumnName("id_product_promotion");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.DiscountType)
                .HasMaxLength(20)
                .HasComment("PERCENTAGE or FIXED_AMOUNT")
                .HasColumnName("discount_type");
            entity.Property(e => e.DiscountValue)
                .HasPrecision(12, 2)
                .HasColumnName("discount_value");
            entity.Property(e => e.IdProduct).HasColumnName("id_product");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.PromotionName)
                .HasMaxLength(255)
                .HasColumnName("promotion_name");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.ValidFrom)
                .HasColumnType("datetime")
                .HasColumnName("valid_from");
            entity.Property(e => e.ValidTo)
                .HasColumnType("datetime")
                .HasColumnName("valid_to");

            entity.HasOne(d => d.IdProductNavigation).WithMany(p => p.ProductPromotions)
                .HasForeignKey(d => d.IdProduct)
                .HasConstraintName("fk_product_promotion_product");
        });

        modelBuilder.Entity<ProductReviews>(entity =>
        {
            entity.HasKey(e => e.IdReview).HasName("PRIMARY");

            entity
                .ToTable("product_reviews", tb => tb.HasComment("Bảng lưu đánh giá sản phẩm của khách hàng"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.IdCustomer, "idx_reviews_customer");

            entity.HasIndex(e => e.IdOrder, "idx_reviews_order");

            entity.HasIndex(e => e.IdOrderDetail, "idx_reviews_order_detail").IsUnique();

            entity.HasIndex(e => e.IdProduct, "idx_reviews_product");

            entity.Property(e => e.IdReview).HasColumnName("id_review");
            entity.Property(e => e.AdminReply)
                .HasComment("Câu trả lời từ admin/employee")
                .HasColumnType("text")
                .HasColumnName("admin_reply");
            entity.Property(e => e.Comment)
                .HasColumnType("text")
                .HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.EditCount)
                .HasComment("Số lần đã chỉnh sửa review (tối đa 1 lần)")
                .HasColumnName("edit_count");
            entity.Property(e => e.IdCustomer).HasColumnName("id_customer");
            entity.Property(e => e.IdOrder)
                .HasComment("Reference đến order đã mua sản phẩm")
                .HasColumnName("id_order");
            entity.Property(e => e.IdOrderDetail)
                .HasComment("Reference đến order detail cụ thể")
                .HasColumnName("id_order_detail");
            entity.Property(e => e.IdProduct).HasColumnName("id_product");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.IdCustomerNavigation).WithMany(p => p.ProductReviews)
                .HasForeignKey(d => d.IdCustomer)
                .HasConstraintName("fk_reviews_customer");

            entity.HasOne(d => d.IdOrderNavigation).WithMany(p => p.ProductReviews)
                .HasForeignKey(d => d.IdOrder)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_reviews_order");

            entity.HasOne(d => d.IdOrderDetailNavigation).WithOne(p => p.ProductReviews)
                .HasForeignKey<ProductReviews>(d => d.IdOrderDetail)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_reviews_order_detail");

            entity.HasOne(d => d.IdProductNavigation).WithMany(p => p.ProductReviews)
                .HasForeignKey(d => d.IdProduct)
                .HasConstraintName("fk_reviews_product");
        });

        modelBuilder.Entity<ProductView>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("product_view")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => new { e.ProductId, e.CreatedAt }, "idx_product_view_product_created");

            entity.HasIndex(e => new { e.UserId, e.CreatedAt }, "idx_product_view_user_created");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActionType)
                .HasMaxLength(20)
                .HasColumnName("action_type");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.SessionId)
                .HasMaxLength(100)
                .HasColumnName("session_id");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<Products>(entity =>
        {
            entity.HasKey(e => e.IdProduct).HasName("PRIMARY");

            entity
                .ToTable("products")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.Brand, "idx_products_brand");

            entity.HasIndex(e => e.IdCategory, "idx_products_category");

            entity.HasIndex(e => e.ProductCode, "idx_products_code").IsUnique();

            entity.HasIndex(e => e.CodeType, "idx_products_code_type");

            entity.HasIndex(e => e.IsDelete, "idx_products_is_delete");

            entity.HasIndex(e => e.ProductName, "idx_products_name");

            entity.HasIndex(e => e.Sku, "idx_products_sku").IsUnique();

            entity.HasIndex(e => e.IdSupplier, "idx_products_supplier");

            entity.Property(e => e.IdProduct).HasColumnName("id_product");
            entity.Property(e => e.Brand)
                .HasMaxLength(100)
                .HasComment("Thương hiệu")
                .HasColumnName("brand");
            entity.Property(e => e.CodeType)
                .HasDefaultValueSql("'SKU'")
                .HasColumnType("enum('IMEI','SERIAL','SKU','BARCODE')")
                .HasColumnName("code_type");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.IdCategory).HasColumnName("id_category");
            entity.Property(e => e.IdSupplier)
                .HasComment("Nhà cung cấp = Brand (Apple, Samsung...)")
                .HasColumnName("id_supplier");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");
            entity.Property(e => e.IsDelete)
                .HasComment("Soft delete flag")
                .HasColumnName("is_delete");
            entity.Property(e => e.Price)
                .HasPrecision(12, 2)
                .HasColumnName("price");
            entity.Property(e => e.ProductCode)
                .HasMaxLength(100)
                .HasDefaultValueSql("''")
                .HasComment("Mã: IMEI/Serial/SKU/Barcode")
                .HasColumnName("product_code");
            entity.Property(e => e.ProductName).HasColumnName("product_name");
            entity.Property(e => e.Sku)
                .HasMaxLength(50)
                .HasComment("SKU: PREFIX-XXXX")
                .HasColumnName("sku");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'IN_STOCK'")
                .HasColumnType("enum('IN_STOCK','OUT_OF_STOCK','DISCONTINUED')")
                .HasColumnName("status");
            entity.Property(e => e.StockQuantity)
                .HasDefaultValueSql("'0'")
                .HasColumnName("stock_quantity");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.IdCategoryNavigation).WithMany(p => p.Products)
                .HasForeignKey(d => d.IdCategory)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_products_category");

            entity.HasOne(d => d.IdSupplierNavigation).WithMany(p => p.Products)
                .HasForeignKey(d => d.IdSupplier)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_products_supplier");
        });

        modelBuilder.Entity<PromotionRules>(entity =>
        {
            entity.HasKey(e => e.IdRule).HasName("PRIMARY");

            entity
                .ToTable("promotion_rules", tb => tb.HasComment("Bảng lưu quy tắc giảm giá tự động"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.Scope, "idx_promotion_rules_scope");

            entity.HasIndex(e => e.IsActive, "idx_rules_active");

            entity.HasIndex(e => new { e.StartDate, e.EndDate }, "idx_rules_dates");

            entity.HasIndex(e => e.Priority, "idx_rules_priority");

            entity.Property(e => e.IdRule).HasColumnName("id_rule");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerType)
                .HasDefaultValueSql("'ALL'")
                .HasColumnType("enum('VIP','REGULAR','ALL')")
                .HasColumnName("customer_type");
            entity.Property(e => e.DiscountType)
                .HasColumnType("enum('PERCENTAGE','FIXED_AMOUNT')")
                .HasColumnName("discount_type");
            entity.Property(e => e.DiscountValue)
                .HasPrecision(12, 2)
                .HasColumnName("discount_value");
            entity.Property(e => e.EndDate)
                .HasColumnType("datetime")
                .HasColumnName("end_date");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.MinOrderAmount)
                .HasPrecision(15, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("min_order_amount");
            entity.Property(e => e.Priority)
                .HasDefaultValueSql("'0'")
                .HasComment("Ưu tiên (số càng cao càng ưu tiên)")
                .HasColumnName("priority");
            entity.Property(e => e.RuleName)
                .HasMaxLength(255)
                .HasColumnName("rule_name");
            entity.Property(e => e.Scope)
                .HasDefaultValueSql("'ORDER'")
                .HasComment("Phạm vi áp dụng: ORDER = giảm giá đơn hàng, SHIPPING = giảm giá phí vận chuyển, PRODUCT = giảm giá sản phẩm")
                .HasColumnType("enum('ORDER','SHIPPING','PRODUCT')")
                .HasColumnName("scope");
            entity.Property(e => e.StartDate)
                .HasColumnType("datetime")
                .HasColumnName("start_date");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<PromotionUsage>(entity =>
        {
            entity.HasKey(e => e.IdUsage).HasName("PRIMARY");

            entity
                .ToTable("promotion_usage", tb => tb.HasComment("Bảng lưu lịch sử sử dụng mã giảm giá"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.IdCustomer, "idx_usage_customer");

            entity.HasIndex(e => e.IdOrder, "idx_usage_order");

            entity.HasIndex(e => e.IdPromotion, "idx_usage_promotion");

            entity.Property(e => e.IdUsage).HasColumnName("id_usage");
            entity.Property(e => e.IdCustomer).HasColumnName("id_customer");
            entity.Property(e => e.IdOrder).HasColumnName("id_order");
            entity.Property(e => e.IdPromotion).HasColumnName("id_promotion");
            entity.Property(e => e.UsedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("used_at");

            entity.HasOne(d => d.IdCustomerNavigation).WithMany(p => p.PromotionUsage)
                .HasForeignKey(d => d.IdCustomer)
                .HasConstraintName("fk_usage_customer");

            entity.HasOne(d => d.IdOrderNavigation).WithMany(p => p.PromotionUsage)
                .HasForeignKey(d => d.IdOrder)
                .HasConstraintName("fk_usage_order");

            entity.HasOne(d => d.IdPromotionNavigation).WithMany(p => p.PromotionUsage)
                .HasForeignKey(d => d.IdPromotion)
                .HasConstraintName("fk_usage_promotion");
        });

        modelBuilder.Entity<Promotions>(entity =>
        {
            entity.HasKey(e => e.IdPromotion).HasName("PRIMARY");

            entity
                .ToTable("promotions", tb => tb.HasComment("Bảng lưu mã giảm giá"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.IsActive, "idx_promotions_active");

            entity.HasIndex(e => e.Code, "idx_promotions_code").IsUnique();

            entity.HasIndex(e => new { e.StartDate, e.EndDate }, "idx_promotions_dates");

            entity.HasIndex(e => e.Scope, "idx_promotions_scope");

            entity.Property(e => e.IdPromotion).HasColumnName("id_promotion");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.DiscountType)
                .HasColumnType("enum('PERCENTAGE','FIXED_AMOUNT')")
                .HasColumnName("discount_type");
            entity.Property(e => e.DiscountValue)
                .HasPrecision(12, 2)
                .HasColumnName("discount_value");
            entity.Property(e => e.EndDate)
                .HasColumnType("datetime")
                .HasColumnName("end_date");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.MinOrderAmount)
                .HasPrecision(15, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("min_order_amount");
            entity.Property(e => e.Scope)
                .HasDefaultValueSql("'ORDER'")
                .HasComment("Phạm vi áp dụng: ORDER = giảm giá đơn hàng, SHIPPING = giảm giá phí vận chuyển, PRODUCT = giảm giá sản phẩm")
                .HasColumnType("enum('ORDER','SHIPPING','PRODUCT')")
                .HasColumnName("scope");
            entity.Property(e => e.StartDate)
                .HasColumnType("datetime")
                .HasColumnName("start_date");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UsageCount)
                .HasDefaultValueSql("'0'")
                .HasColumnName("usage_count");
            entity.Property(e => e.UsageLimit)
                .HasComment("Số lần sử dụng tối đa (NULL = không giới hạn)")
                .HasColumnName("usage_limit");

            entity.HasMany(d => d.IdProduct).WithMany(p => p.IdPromotion)
                .UsingEntity<Dictionary<string, object>>(
                    "PromotionProducts",
                    r => r.HasOne<Products>().WithMany()
                        .HasForeignKey("IdProduct")
                        .HasConstraintName("fk_promotion_products_product"),
                    l => l.HasOne<Promotions>().WithMany()
                        .HasForeignKey("IdPromotion")
                        .HasConstraintName("fk_promotion_products_promotion"),
                    j =>
                    {
                        j.HasKey("IdPromotion", "IdProduct")
                            .HasName("PRIMARY")
                            .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                        j
                            .ToTable("promotion_products", tb => tb.HasComment("Bảng liên kết promotion với sản phẩm cụ thể"))
                            .UseCollation("utf8mb4_0900_ai_ci");
                        j.HasIndex(new[] { "IdProduct" }, "idx_promotion_products_product");
                        j.IndexerProperty<int>("IdPromotion").HasColumnName("id_promotion");
                        j.IndexerProperty<int>("IdProduct").HasColumnName("id_product");
                    });
        });

        modelBuilder.Entity<PurchaseOrderDetails>(entity =>
        {
            entity.HasKey(e => e.IdPurchaseOrderDetail).HasName("PRIMARY");

            entity
                .ToTable("purchase_order_details")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.IdPurchaseOrder, "idx_po_details_order");

            entity.HasIndex(e => e.IdProduct, "idx_po_details_product");

            entity.HasIndex(e => new { e.IdPurchaseOrder, e.IdProduct }, "uq_po_details_order_product").IsUnique();

            entity.Property(e => e.IdPurchaseOrderDetail).HasColumnName("id_purchase_order_detail");
            entity.Property(e => e.IdProduct).HasColumnName("id_product");
            entity.Property(e => e.IdPurchaseOrder).HasColumnName("id_purchase_order");
            entity.Property(e => e.ImportPrice)
                .HasPrecision(12, 2)
                .HasColumnName("import_price");
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(d => d.IdProductNavigation).WithMany(p => p.PurchaseOrderDetails)
                .HasForeignKey(d => d.IdProduct)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_po_details_product");

            entity.HasOne(d => d.IdPurchaseOrderNavigation).WithMany(p => p.PurchaseOrderDetails)
                .HasForeignKey(d => d.IdPurchaseOrder)
                .HasConstraintName("fk_po_details_order");
        });

        modelBuilder.Entity<PurchaseOrders>(entity =>
        {
            entity.HasKey(e => e.IdPurchaseOrder).HasName("PRIMARY");

            entity
                .ToTable("purchase_orders")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.IdEmployee, "idx_po_employee");

            entity.HasIndex(e => e.IdSupplier, "idx_po_supplier");

            entity.Property(e => e.IdPurchaseOrder).HasColumnName("id_purchase_order");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.IdEmployee).HasColumnName("id_employee");
            entity.Property(e => e.IdSupplier).HasColumnName("id_supplier");
            entity.Property(e => e.InvoicePrinted)
                .HasDefaultValueSql("'0'")
                .HasColumnName("invoice_printed");
            entity.Property(e => e.InvoicePrintedAt)
                .HasColumnType("datetime")
                .HasColumnName("invoice_printed_at");
            entity.Property(e => e.InvoicePrintedBy).HasColumnName("invoice_printed_by");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("order_date");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(15, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("total_amount");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.IdEmployeeNavigation).WithMany(p => p.PurchaseOrders)
                .HasForeignKey(d => d.IdEmployee)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_po_employee");

            entity.HasOne(d => d.IdSupplierNavigation).WithMany(p => p.PurchaseOrders)
                .HasForeignKey(d => d.IdSupplier)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_po_supplier");
        });

        modelBuilder.Entity<Shipments>(entity =>
        {
            entity.HasKey(e => e.IdShipment).HasName("PRIMARY");

            entity
                .ToTable("shipments")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.GhnOrderCode, "idx_shipments_ghn_order_code");

            entity.HasIndex(e => e.ShippingMethod, "idx_shipments_shipping_method");

            entity.HasIndex(e => e.IdOrder, "uq_shipments_order").IsUnique();

            entity.Property(e => e.IdShipment).HasColumnName("id_shipment");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.GhnExpectedDeliveryTime)
                .HasComment("Thời gian giao hàng dự kiến từ GHN")
                .HasColumnType("datetime")
                .HasColumnName("ghn_expected_delivery_time");
            entity.Property(e => e.GhnNote)
                .HasComment("Ghi chú hoặc lý do từ GHN (ví dụ: lý do giao thất bại)")
                .HasColumnType("text")
                .HasColumnName("ghn_note");
            entity.Property(e => e.GhnOrderCode)
                .HasMaxLength(50)
                .HasComment("Mã đơn hàng từ GHN API")
                .HasColumnName("ghn_order_code");
            entity.Property(e => e.GhnShippingFee)
                .HasPrecision(12, 2)
                .HasComment("Phí vận chuyển từ GHN")
                .HasColumnName("ghn_shipping_fee");
            entity.Property(e => e.GhnStatus)
                .HasMaxLength(50)
                .HasComment("Trạng thái đơn hàng từ GHN (ready_to_pick, delivering, delivered, etc.)")
                .HasColumnName("ghn_status");
            entity.Property(e => e.GhnUpdatedAt)
                .HasComment("Thời gian cập nhật trạng thái từ GHN webhook")
                .HasColumnType("timestamp")
                .HasColumnName("ghn_updated_at");
            entity.Property(e => e.IdOrder).HasColumnName("id_order");
            entity.Property(e => e.LocationLat)
                .HasPrecision(9, 6)
                .HasComment("Vĩ độ")
                .HasColumnName("location_lat");
            entity.Property(e => e.LocationLong)
                .HasPrecision(9, 6)
                .HasComment("Kinh độ")
                .HasColumnName("location_long");
            entity.Property(e => e.ShippingMethod)
                .HasDefaultValueSql("'GHN'")
                .HasComment("Phương thức vận chuyển: GHN hoặc khách tự đến lấy")
                .HasColumnType("enum('GHN','SELF_PICKUP')")
                .HasColumnName("shipping_method");
            entity.Property(e => e.ShippingStatus)
                .HasDefaultValueSql("'PREPARING'")
                .HasColumnType("enum('PREPARING','SHIPPED','DELIVERED')")
                .HasColumnName("shipping_status");
            entity.Property(e => e.TrackingNumber)
                .HasMaxLength(50)
                .HasColumnName("tracking_number");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.IdOrderNavigation).WithOne(p => p.Shipments)
                .HasForeignKey<Shipments>(d => d.IdOrder)
                .HasConstraintName("fk_shipments_order");
        });

        modelBuilder.Entity<ShippingAddresses>(entity =>
        {
            entity.HasKey(e => e.IdShippingAddress).HasName("PRIMARY");

            entity
                .ToTable("shipping_addresses")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.IdCustomer, "idx_shipping_addresses_customer");

            entity.HasIndex(e => e.DistrictId, "idx_shipping_addresses_district");

            entity.HasIndex(e => e.ProvinceId, "idx_shipping_addresses_province");

            entity.Property(e => e.IdShippingAddress).HasColumnName("id_shipping_address");
            entity.Property(e => e.Address)
                .HasColumnType("text")
                .HasColumnName("address");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.DistrictId)
                .HasComment("ID quận/huyện từ GHN API")
                .HasColumnName("district_id");
            entity.Property(e => e.IdCustomer).HasColumnName("id_customer");
            entity.Property(e => e.IsDefault)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_default");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .HasColumnName("phone_number");
            entity.Property(e => e.ProvinceId)
                .HasComment("ID tỉnh/thành phố từ GHN API")
                .HasColumnName("province_id");
            entity.Property(e => e.RecipientName)
                .HasMaxLength(255)
                .HasColumnName("recipient_name");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.WardCode)
                .HasMaxLength(50)
                .HasComment("Code phường/xã từ GHN API")
                .HasColumnName("ward_code");

            entity.HasOne(d => d.IdCustomerNavigation).WithMany(p => p.ShippingAddresses)
                .HasForeignKey(d => d.IdCustomer)
                .HasConstraintName("fk_shipping_addresses_customer");
        });

        modelBuilder.Entity<Suppliers>(entity =>
        {
            entity.HasKey(e => e.IdSupplier).HasName("PRIMARY");

            entity
                .ToTable("suppliers")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.SupplierName, "idx_suppliers_name");

            entity.Property(e => e.IdSupplier).HasColumnName("id_supplier");
            entity.Property(e => e.Address)
                .HasColumnType("text")
                .HasColumnName("address");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .HasColumnName("phone_number");
            entity.Property(e => e.SupplierName).HasColumnName("supplier_name");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<SystemSettings>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("system_settings");

            entity.HasIndex(e => e.SettingKey, "setting_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.SettingKey)
                .HasMaxLength(100)
                .HasColumnName("setting_key");
            entity.Property(e => e.SettingValue)
                .HasMaxLength(255)
                .HasColumnName("setting_value");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Users>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("PRIMARY");

            entity
                .ToTable("users")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.Email, "uq_users_email").IsUnique();

            entity.HasIndex(e => e.Username, "uq_users_username").IsUnique();

            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(500)
                .HasComment("URL ảnh đại diện của user")
                .HasColumnName("avatar_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasComment("Lưu mật khẩu đã mã hoá (BCrypt/Argon2)")
                .HasColumnName("password");
            entity.Property(e => e.Role)
                .HasColumnType("enum('ADMIN','EMPLOYEE','CUSTOMER')")
                .HasColumnName("role");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
