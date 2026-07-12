# Store Management Backend API

> A backend-only **store management API** for customer shopping flows, staff operations, and administrator management.

> The system provides:
> - REST APIs for product browsing, cart, ordering, payment, shipping, reviews, returns, and customer accounts.
> - Admin and employee APIs for products, inventory, suppliers, customers, orders, invoices, promotions, reports, and system settings.
> - Real-time customer support chat through SignalR.
> - External integrations for **PayOS** payment, **GHN** shipping, email, file uploads, and product recommendations.
>
> This repository contains the **backend only**. It does not include a client website or admin frontend.

---

## Backend Overview

This project is an ASP.NET Core Web API backend using a layered structure:

```text
Controllers -> Services -> Repositories -> AppDbContext -> PostgreSQL
```

Main backend modules:

- `Controllers/` exposes versioned REST endpoints under `api/v1/...`.
- `Services/` contains business logic.
- `Repositories/` handles data access through Entity Framework Core.
- `Models/` contains database entities.
- `Dtos/` contains request and response contracts.
- `Hubs/ChatHub.cs` provides SignalR real-time chat at `/hubs/chat`.
- `Middleware/GlobalExceptionMiddleware.cs` centralizes API error handling.
- `uploads/` stores uploaded files and is served publicly from `/uploads`.

---

## Main Features

- **Authentication & Role-Based Access Control**  
  Supports JWT authentication and role-based authorization for `CUSTOMER`, `EMPLOYEE`, and `ADMIN`.

- **Product Catalog APIs**  
  Manage products, categories, suppliers, images, brands, new products, best sellers, on-sale products, and related products.

- **Cart & Ordering Flow**  
  Supports cart management, checkout, buy-now orders, customer order history, cancellation, delivery confirmation, and order status updates.

- **Inventory & Import Management**  
  Tracks inventory transactions, import orders, suppliers, purchase history, and import invoice export/print APIs.

- **Payment & Shipping Integration**  
  Integrates **PayOS** for payment creation, payment status, return/cancel URLs, and webhooks. Integrates **GHN** for address data, shipping fee calculation, shipment creation, tracking, and webhooks.

- **Return & Exchange Handling**  
  Customers can create return or exchange requests. Admin or staff can approve, reject, and complete requests.

- **Promotions & Discounts**  
  Supports admin promotion management, promotion rules, discount validation, discount calculation, and automatic shipping promotions.

- **Reviews & Recommendations**  
  Supports product reviews, admin replies, review moderation, product recommendations, and similar-product APIs.

- **Real-Time Chat**  
  Provides customer support conversations and SignalR messaging through `/hubs/chat`.

- **Admin Reporting**  
  Provides dashboard overview, revenue summary, revenue by time, and revenue by product APIs.

---

## Technology Used

| Area | Technology |
|---|---|
| Language | C# |
| Framework | ASP.NET Core Web API |
| Target Framework | .NET 10 |
| Database | PostgreSQL |
| ORM | Entity Framework Core + PostgreSQL provider |
| Authentication | JWT Bearer |
| Authorization | ASP.NET Core role-based authorization |
| Realtime | SignalR |
| Password Hashing | BCrypt.Net |
| File Uploads | ASP.NET Core static files from `/uploads` |
| Payment | PayOS |
| Shipping | GHN |
| Email | SMTP |