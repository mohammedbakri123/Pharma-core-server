using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Auth;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;

namespace PharmaCore.Infrastructure;

public class DataSeeder
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(IServiceScopeFactory scopeFactory, ILogger<DataSeeder> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var services = scope.ServiceProvider;

        var userRepo = services.GetRequiredService<IUserRepository>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher>();

        var existing = await userRepo.ListAsync();
        if (existing.Any())
        {
            _logger.LogInformation("Database already seeded. Skipping.");
            return;
        }

        _logger.LogInformation("Seeding database...");

        var categoryRepo = services.GetRequiredService<ICategoryRepository>();
        var medicineRepo = services.GetRequiredService<IMedicineRepository>();
        var supplierRepo = services.GetRequiredService<ISupplierRepository>();
        var customerRepo = services.GetRequiredService<ICustomerRepository>();
        var purchaseRepo = services.GetRequiredService<IPurchaseRepository>();
        var batchRepo = services.GetRequiredService<IBatchRepository>();
        var stockMovementRepo = services.GetRequiredService<IStockMovementRepository>();
        var paymentRepo = services.GetRequiredService<IPaymentRepository>();
        var saleRepo = services.GetRequiredService<ISaleRepository>();
        var expenseRepo = services.GetRequiredService<IExpenseRepository>();

        // ── Users ──────────────────────────────────────────────────────────
        var admin = await userRepo.AddAsync(
            User.Create("admin", passwordHasher.Hash("admin123"), "01000000001", "123 Admin St",
                UserRole.Admin));

        var cashier = await userRepo.AddAsync(
            User.Create("cashier1", passwordHasher.Hash("cashier123"), "01000000002", "456 Cashier St",
                UserRole.Cashier));

        var users = new[] { admin, cashier };
        _logger.LogInformation("Seeded {Count} users", users.Length);

        // ── Categories ─────────────────────────────────────────────────────
        var categoryData = new[]
        {
            ("Antibiotics", "مضادات حيوية"),
            ("Analgesics", "مسكنات ألم"),
            ("Cardiovascular", "قلب وأوعية دموية"),
            ("Respiratory", "جهاز تنفسي"),
            ("Gastrointestinal", "جهاز هضمي"),
            ("Dermatological", "جلدية"),
            ("Vitamins & Supplements", "فيتامينات ومكملات"),
            ("Antidiabetics", "مضادات السكري"),
            ("Antihistamines", "مضادات الهيستامين"),
            ("Antipyretics", "خافضات حرارة"),
        };

        var categories = new List<Category>();
        foreach (var (name, arabicName) in categoryData)
        {
            categories.Add(await categoryRepo.AddAsync(Category.Create(name, arabicName)));
        }
        _logger.LogInformation("Seeded {Count} categories", categories.Count);

        // ── Medicines ──────────────────────────────────────────────────────
        var medicineData = new[]
        {
            ("Amoxicillin 500mg", "أموكسيسيلين 500 مجم", "6281000000010", categories[0].CategoryId, MedicineUnit.strip),
            ("Azithromycin 250mg", "أزيثروميسين 250 مجم", "6281000000027", categories[0].CategoryId, MedicineUnit.strip),
            ("Paracetamol 500mg", "باراسيتامول 500 مجم", "6281000000034", categories[1].CategoryId, MedicineUnit.pill),
            ("Ibuprofen 400mg", "ايبوبروفين 400 مجم", "6281000000041", categories[1].CategoryId, MedicineUnit.pill),
            ("Amlodipine 5mg", "أملوديبين 5 مجم", "6281000000058", categories[2].CategoryId, MedicineUnit.pill),
            ("Lisinopril 10mg", "ليسينوبريل 10 مجم", "6281000000065", categories[2].CategoryId, MedicineUnit.pill),
            ("Salbutamol Inhaler", "سالبوتامول بخاخ", "6281000000072", categories[3].CategoryId, MedicineUnit.inhaler),
            ("Montelukast 10mg", "مونتيلوكاست 10 مجم", "6281000000089", categories[3].CategoryId, MedicineUnit.pill),
            ("Omeprazole 20mg", "أوميبرازول 20 مجم", "6281000000096", categories[4].CategoryId, MedicineUnit.strip),
            ("Metoclopramide 10mg", "ميتوكلوبراميد 10 مجم", "6281000000102", categories[4].CategoryId, MedicineUnit.pill),
            ("Clotrimazole Cream", "كلوتريمازول كريم", "6281000000119", categories[5].CategoryId, MedicineUnit.tube),
            ("Hydrocortisone Cream", "هيدروكورتيزون كريم", "6281000000126", categories[5].CategoryId, MedicineUnit.tube),
            ("Vitamin C 1000mg", "فيتامين سي 1000 مجم", "6281000000133", categories[6].CategoryId, MedicineUnit.pill),
            ("Vitamin D3 5000IU", "فيتامين د3 5000 وحدة", "6281000000140", categories[6].CategoryId, MedicineUnit.strip),
            ("Metformin 500mg", "ميتفورمين 500 مجم", "6281000000157", categories[7].CategoryId, MedicineUnit.pill),
            ("Insulin Glargine", "أنسولين جلارجين", "6281000000164", categories[7].CategoryId, MedicineUnit.vial),
            ("Loratadine 10mg", "لوراتادين 10 مجم", "6281000000171", categories[8].CategoryId, MedicineUnit.pill),
            ("Cetirizine 10mg", "سيتريزين 10 مجم", "6281000000188", categories[8].CategoryId, MedicineUnit.pill),
            ("Paracetamol Syrup", "باراسيتامول شراب", "6281000000195", categories[9].CategoryId, MedicineUnit.bottle),
            ("Mefenamic Acid 500mg", "حمض ميفيناميك 500 مجم", "6281000000201", categories[9].CategoryId, MedicineUnit.strip),
        };

        var medicines = new List<Medicine>();
        foreach (var (name, arabicName, barcode, catId, unit) in medicineData)
        {
            medicines.Add(await medicineRepo.AddAsync(Medicine.Create(name, arabicName, barcode, catId, unit)));
        }
        _logger.LogInformation("Seeded {Count} medicines", medicines.Count);

        // ── Suppliers ──────────────────────────────────────────────────────
        var supplierData = new[]
        {
            ("PharmaDistrib Co.", "01010000001", "Cairo, Egypt"),
            ("MedSupply Ltd.", "01010000002", "Alexandria, Egypt"),
            ("HealthPlus Pharmaceuticals", "01010000003", "Giza, Egypt"),
            ("GlobalMed Trading", "01010000004", "Sharjah, UAE"),
            ("United Pharma Group", "01010000005", "Riyadh, KSA"),
        };

        var suppliers = new List<Supplier>();
        foreach (var (name, phone, address) in supplierData)
        {
            suppliers.Add(await supplierRepo.AddAsync(Supplier.Create(name, phone, address)));
        }
        _logger.LogInformation("Seeded {Count} suppliers", suppliers.Count);

        // ── Customers ──────────────────────────────────────────────────────
        var customerData = new[]
        {
            ("Ahmed Hassan", "01110000001", "15 El-Tahrir St, Cairo", null),
            ("Mohamed Ali", "01110000002", "22 El-Haram St, Giza", "Regular customer"),
            ("Sara Ibrahim", "01110000003", "8 El-Nile St, Alexandria", null),
            ("Khaled Omar", "01110000004", "5 El-Salam St, Mansoura", "VIP"),
            ("Nourhan Adel", "01110000005", "12 El-Maadi St, Cairo", null),
            ("Youssef Samir", "01110000006", "3 El-Nozha St, Cairo", null),
            ("Mona Tarek", "01110000007", "18 El-Montazah St, Alexandria", null),
            ("Omar Farouk", "01110000008", "7 El-Mohandeseen St, Giza", null),
            ("Dina Hany", "01110000009", "9 El-Sharq St, Port Said", null),
            ("Hassan Mahmoud", "01110000010", "14 El-Abbaseya St, Cairo", null),
        };

        var customers = new List<Customer>();
        foreach (var (name, phone, address, note) in customerData)
        {
            customers.Add(await customerRepo.AddAsync(Customer.Create(name, phone, address, note)));
        }
        _logger.LogInformation("Seeded {Count} customers", customers.Count);

        // ── Helper to create a completed purchase ──────────────────────────
        async Task<int> SeedPurchase(int supplierIdx, string invoice, string note,
            (int medicineIdx, int qty, decimal purchasePrice, decimal sellPrice, DateOnly expire)[] items)
        {
            var purchase = Purchase.Create(suppliers[supplierIdx].SupplierId, invoice, note);
            purchase = await purchaseRepo.AddAsync(purchase);

            foreach (var (medIdx, qty, pp, sp, exp) in items)
            {
                var batch = Batch.Create(medicines[medIdx].MedicineId, $"BATCH-{invoice}-{medIdx}", qty, pp, sp, exp);
                batch = await batchRepo.AddAsync(batch);

                var item = PurchaseItem.Create(purchase.PurchaseId, medicines[medIdx].MedicineId,
                    batch.BatchId, qty, pp, sp, exp);
                await purchaseRepo.AddItemAsync(item);
            }

            await purchaseRepo.UpdateTotalAmountAsync(purchase.PurchaseId);

            var saved = await purchaseRepo.GetByIdWithItemsAsync(purchase.PurchaseId);
            if (saved is null) return purchase.PurchaseId;

            saved.Complete();
            await purchaseRepo.UpdateAsync(saved);

            var stockMovements = saved.Items.Select(i =>
                StockMovement.Create(i.MedicineId, i.BatchId, i.Quantity,
                    StockMovementType.IN, StockMovementReferenceType.PURCHASE, saved.PurchaseId)).ToList();
            await stockMovementRepo.AddRangeAsync(stockMovements);

            var payment = Payment.Create(PaymentType.OUTGOING, PaymentReferenceType.PURCHASE,
                saved.PurchaseId, PaymentMethod.CASH, admin.UserId,
                saved.TotalAmount, $"دفعة فاتورة شراء رقم {invoice}");
            await paymentRepo.AddAsync(payment);

            return saved.PurchaseId;
        }

        // ── Helper to create a completed sale ──────────────────────────────
        async Task<int> SeedSale(int customerIdx, int userIdx, string note,
            (int medicineIdx, int qty, decimal unitPrice)[] items)
        {
            var sale = Sale.Create(users[userIdx].UserId, customers[customerIdx].CustomerId, note);
            sale = await saleRepo.AddAsync(sale);

            foreach (var (medIdx, qty, up) in items)
            {
                var medicine = medicines[medIdx];
                var availableBatches = await batchRepo.ListAvailableByMedicineAsync(medicine.MedicineId);
                var batch = availableBatches.FirstOrDefault();
                if (batch is null) continue;

                batch.DecreaseStock(qty);
                await batchRepo.UpdateAsync(batch);

                var purchasePrice = batch.PurchasePrice;
                var item = SaleItem.Create(sale.SaleId, medicine.MedicineId, batch.BatchId, qty, up, purchasePrice);
                await saleRepo.AddItemAsync(item);
            }

            await saleRepo.UpdateTotalAmountAsync(sale.SaleId);

            var saved = await saleRepo.GetByIdWithItemsAsync(sale.SaleId);
            if (saved is null) return sale.SaleId;

            saved.Complete();
            await saleRepo.UpdateAsync(saved);

            var stockMovements = saved.Items.Select(i =>
                StockMovement.Create(i.MedicineId, i.BatchId, i.Quantity,
                    StockMovementType.OUT, StockMovementReferenceType.SALE, saved.SaleId)).ToList();
            await stockMovementRepo.AddRangeAsync(stockMovements);

            var payment = Payment.Create(PaymentType.INCOMING, PaymentReferenceType.SALE,
                saved.SaleId, PaymentMethod.CASH, users[userIdx].UserId,
                saved.TotalAmount, $"دفعة فاتورة بيع رقم {saved.SaleId}");
            await paymentRepo.AddAsync(payment);

            return saved.SaleId;
        }

        // ── Purchases ──────────────────────────────────────────────────────
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await SeedPurchase(0, "INV-001", "Purchase from PharmaDistrib", new[]
        {
            (0, 200, 8.50m, 15.00m, today.AddMonths(18)),
            (2, 500, 2.00m, 5.00m, today.AddMonths(24)),
            (4, 150, 12.00m, 25.00m, today.AddMonths(20)),
            (6, 100, 18.00m, 35.00m, today.AddMonths(15)),
            (12, 300, 4.50m, 10.00m, today.AddMonths(30)),
        });

        await SeedPurchase(1, "INV-002", "Purchase from MedSupply", new[]
        {
            (1, 150, 18.00m, 35.00m, today.AddMonths(16)),
            (3, 200, 6.00m, 12.00m, today.AddMonths(22)),
            (8, 250, 3.50m, 8.00m, today.AddMonths(24)),
            (14, 300, 4.00m, 8.00m, today.AddMonths(28)),
            (16, 200, 2.50m, 6.00m, today.AddMonths(20)),
        });

        await SeedPurchase(2, "INV-003", "Purchase from HealthPlus", new[]
        {
            (5, 100, 22.00m, 40.00m, today.AddMonths(18)),
            (7, 120, 15.00m, 30.00m, today.AddMonths(14)),
            (9, 200, 3.00m, 7.00m, today.AddMonths(26)),
            (10, 80, 14.00m, 28.00m, today.AddMonths(12)),
            (11, 60, 16.00m, 32.00m, today.AddMonths(10)),
            (13, 400, 8.00m, 18.00m, today.AddMonths(30)),
        });

        _logger.LogInformation("Seeded purchases");

        // ── Sales ──────────────────────────────────────────────────────────
        await SeedSale(0, 0, "Walk-in customer", new[]
        {
            (0, 10, 15.00m),
            (2, 20, 5.00m),
            (8, 5, 8.00m),
        });

        await SeedSale(1, 1, "Regular customer - monthly supply", new[]
        {
            (14, 30, 8.00m),
            (4, 10, 25.00m),
            (12, 15, 10.00m),
        });

        await SeedSale(2, 0, "Customer with prescription", new[]
        {
            (1, 6, 35.00m),
            (6, 2, 35.00m),
            (7, 10, 30.00m),
        });

        await SeedSale(3, 1, "VIP customer order", new[]
        {
            (5, 15, 40.00m),
            (13, 20, 18.00m),
            (16, 25, 6.00m),
            (18, 10, 12.00m),
        });

        await SeedSale(4, 0, "Pharmacy stock-up", new[]
        {
            (3, 30, 12.00m),
            (9, 20, 7.00m),
            (17, 20, 5.00m),
            (19, 15, 14.00m),
        });

        _logger.LogInformation("Seeded sales");

        // ── Expenses ───────────────────────────────────────────────────────
        await expenseRepo.AddAsync(Expense.Create(admin.UserId, 15000m, "إيجار الشهر"));
        await expenseRepo.AddAsync(Expense.Create(admin.UserId, 2500m, "فاتورة كهرباء"));
        await expenseRepo.AddAsync(Expense.Create(cashier.UserId, 800m, "صيانة عامة"));

        _logger.LogInformation("Seeded expenses");

        _logger.LogInformation("Database seeding completed successfully.");
    }
}
