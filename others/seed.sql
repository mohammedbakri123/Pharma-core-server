-- =============================================================================
-- PharmaCore Database Seed Script
-- =============================================================================
-- Run: psql -U postgres -d pharma_core -f others/seed.sql
-- =============================================================================

BEGIN;

-- Abort if already seeded (check if any users exist)
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM users) THEN
        RAISE EXCEPTION 'Database already contains data. Truncate all tables first or use a fresh database.';
    END IF;
END $$;

-- =============================================================================
-- USERS
-- =============================================================================
-- Passwords are PBKDF2-SHA256 hashed with 100,000 iterations
-- admin / admin123
-- cashier1 / cashier123
INSERT INTO users (user_name, password_hash, phone_number, address, role, created_at)
VALUES
    ('admin',    'pbkdf2$100000$YI1a2wEIM82pW+p/7ds8pA==$eaAYMFAyFrKN0DY9/en/vpN2+evQUh2lvOy3aiSsca0=', '01000000001', '123 Admin St',    1, NOW()),
    ('cashier1', 'pbkdf2$100000$jA2W4Kpm2Sl1NRZ4Fv3U5g==$Q98cwa8ZgCvih0uamPNFLNpGViOXG6J24cu39mQ6QVU=', '01000000002', '456 Cashier St',  2, NOW());

-- =============================================================================
-- CATEGORIES
-- =============================================================================
INSERT INTO categories (category_name, category_arabic_name)
VALUES
    ('Antibiotics',              'مضادات حيوية'),
    ('Analgesics',               'مسكنات ألم'),
    ('Cardiovascular',           'قلب وأوعية دموية'),
    ('Respiratory',              'جهاز تنفسي'),
    ('Gastrointestinal',         'جهاز هضمي'),
    ('Dermatological',           'جلدية'),
    ('Vitamins & Supplements',   'فيتامينات ومكملات'),
    ('Antidiabetics',            'مضادات السكري'),
    ('Antihistamines',           'مضادات الهيستامين'),
    ('Antipyretics',             'خافضات حرارة');

-- =============================================================================
-- MEDICINES
-- =============================================================================
INSERT INTO medicines (name, arabic_name, barcode, category_id, unit, created_at)
VALUES
    ('Amoxicillin 500mg',      'أموكسيسيلين 500 مجم', '6281000000010', 1, 2, NOW()),
    ('Azithromycin 250mg',     'أزيثروميسين 250 مجم', '6281000000027', 1, 2, NOW()),
    ('Paracetamol 500mg',      'باراسيتامول 500 مجم', '6281000000034', 2, 3, NOW()),
    ('Ibuprofen 400mg',        'ايبوبروفين 400 مجم', '6281000000041', 2, 3, NOW()),
    ('Amlodipine 5mg',         'أملوديبين 5 مجم',    '6281000000058', 3, 3, NOW()),
    ('Lisinopril 10mg',        'ليسينوبريل 10 مجم',  '6281000000065', 3, 3, NOW()),
    ('Salbutamol Inhaler',     'سالبوتامول بخاخ',    '6281000000072', 4, 9, NOW()),
    ('Montelukast 10mg',       'مونتيلوكاست 10 مجم', '6281000000089', 4, 3, NOW()),
    ('Omeprazole 20mg',        'أوميبرازول 20 مجم',  '6281000000096', 5, 2, NOW()),
    ('Metoclopramide 10mg',    'ميتوكلوبراميد 10 مجم', '6281000000102', 5, 3, NOW()),
    ('Clotrimazole Cream',     'كلوتريمازول كريم',   '6281000000119', 6, 7, NOW()),
    ('Hydrocortisone Cream',   'هيدروكورتيزون كريم', '6281000000126', 6, 7, NOW()),
    ('Vitamin C 1000mg',       'فيتامين سي 1000 مجم', '6281000000133', 7, 3, NOW()),
    ('Vitamin D3 5000IU',      'فيتامين د3 5000 وحدة', '6281000000140', 7, 2, NOW()),
    ('Metformin 500mg',        'ميتفورمين 500 مجم',  '6281000000157', 8, 3, NOW()),
    ('Insulin Glargine',       'أنسولين جلارجين',    '6281000000164', 8, 6, NOW()),
    ('Loratadine 10mg',        'لوراتادين 10 مجم',   '6281000000171', 9, 3, NOW()),
    ('Cetirizine 10mg',        'سيتريزين 10 مجم',    '6281000000188', 9, 3, NOW()),
    ('Paracetamol Syrup',      'باراسيتامول شراب',   '6281000000195', 10, 4, NOW()),
    ('Mefenamic Acid 500mg',   'حمض ميفيناميك 500 مجم', '6281000000201', 10, 2, NOW());

-- =============================================================================
-- SUPPLIERS
-- =============================================================================
INSERT INTO suppliers (name, phone_number, address, created_at)
VALUES
    ('PharmaDistrib Co.',          '01010000001', 'Cairo, Egypt',        NOW()),
    ('MedSupply Ltd.',             '01010000002', 'Alexandria, Egypt',   NOW()),
    ('HealthPlus Pharmaceuticals', '01010000003', 'Giza, Egypt',         NOW()),
    ('GlobalMed Trading',          '01010000004', 'Sharjah, UAE',        NOW()),
    ('United Pharma Group',        '01010000005', 'Riyadh, KSA',         NOW());

-- =============================================================================
-- CUSTOMERS
-- =============================================================================
INSERT INTO customers (name, phone_number, address, note, created_at)
VALUES
    ('Ahmed Hassan',    '01110000001', '15 El-Tahrir St, Cairo',      NULL,            NOW()),
    ('Mohamed Ali',     '01110000002', '22 El-Haram St, Giza',        'Regular customer', NOW()),
    ('Sara Ibrahim',    '01110000003', '8 El-Nile St, Alexandria',    NULL,            NOW()),
    ('Khaled Omar',     '01110000004', '5 El-Salam St, Mansoura',     'VIP',           NOW()),
    ('Nourhan Adel',    '01110000005', '12 El-Maadi St, Cairo',       NULL,            NOW()),
    ('Youssef Samir',   '01110000006', '3 El-Nozha St, Cairo',        NULL,            NOW()),
    ('Mona Tarek',      '01110000007', '18 El-Montazah St, Alexandria', NULL,           NOW()),
    ('Omar Farouk',     '01110000008', '7 El-Mohandeseen St, Giza',   NULL,            NOW()),
    ('Dina Hany',       '01110000009', '9 El-Sharq St, Port Said',    NULL,            NOW()),
    ('Hassan Mahmoud',  '01110000010', '14 El-Abbaseya St, Cairo',    NULL,            NOW());

-- =============================================================================
-- PURCHASE 1 — PharmaDistrib (supplier 1)
-- =============================================================================
INSERT INTO purchases (supplier_id, invoice_number, total_amount, status, note, created_at)
VALUES (1, 'INV-001', 7650.00, 2, 'Purchase from PharmaDistrib', NOW());

INSERT INTO batches (medicine_id, batch_number, quantity_entered, quantity_remaining, purchase_price, sell_price, expire_date, created_at)
VALUES
    (1,  'BATCH-INV-001-0',  200, 200,  8.50,  15.00, CURRENT_DATE + INTERVAL '18 months', NOW()),
    (3,  'BATCH-INV-001-2',  500, 500,  2.00,   5.00, CURRENT_DATE + INTERVAL '24 months', NOW()),
    (5,  'BATCH-INV-001-4',  150, 150, 12.00,  25.00, CURRENT_DATE + INTERVAL '20 months', NOW()),
    (7,  'BATCH-INV-001-6',  100, 100, 18.00,  35.00, CURRENT_DATE + INTERVAL '15 months', NOW()),
    (13, 'BATCH-INV-001-12', 300, 300,  4.50,  10.00, CURRENT_DATE + INTERVAL '30 months', NOW());

INSERT INTO purchase_items (purchase_id, medicine_id, batch_id, quantity, purchase_price, sell_price, expire_date)
VALUES
    (1, 1,  1,  200,  8.50,  15.00, CURRENT_DATE + INTERVAL '18 months'),
    (1, 3,  2,  500,  2.00,   5.00, CURRENT_DATE + INTERVAL '24 months'),
    (1, 5,  3,  150, 12.00,  25.00, CURRENT_DATE + INTERVAL '20 months'),
    (1, 7,  4,  100, 18.00,  35.00, CURRENT_DATE + INTERVAL '15 months'),
    (1, 13, 5,  300,  4.50,  10.00, CURRENT_DATE + INTERVAL '30 months');

INSERT INTO stock_movements (medicine_id, batch_id, quantity, type, reference_type, reference_id, created_at)
VALUES
    (1,  1, 200, 1, 1, 1, NOW()),
    (3,  2, 500, 1, 1, 1, NOW()),
    (5,  3, 150, 1, 1, 1, NOW()),
    (7,  4, 100, 1, 1, 1, NOW()),
    (13, 5, 300, 1, 1, 1, NOW());

INSERT INTO payments (type, reference_type, reference_id, method, user_id, amount, description, created_at)
VALUES (2, 2, 1, 1, 1, 7650.00, 'دفعة فاتورة شراء رقم INV-001', NOW());

-- =============================================================================
-- PURCHASE 2 — MedSupply (supplier 2)
-- =============================================================================
INSERT INTO purchases (supplier_id, invoice_number, total_amount, status, note, created_at)
VALUES (2, 'INV-002', 6475.00, 2, 'Purchase from MedSupply', NOW());

INSERT INTO batches (medicine_id, batch_number, quantity_entered, quantity_remaining, purchase_price, sell_price, expire_date, created_at)
VALUES
    (2,  'BATCH-INV-002-1',  150, 150, 18.00, 35.00, CURRENT_DATE + INTERVAL '16 months', NOW()),
    (4,  'BATCH-INV-002-3',  200, 200,  6.00, 12.00, CURRENT_DATE + INTERVAL '22 months', NOW()),
    (9,  'BATCH-INV-002-8',  250, 250,  3.50,  8.00, CURRENT_DATE + INTERVAL '24 months', NOW()),
    (15, 'BATCH-INV-002-14', 300, 300,  4.00,  8.00, CURRENT_DATE + INTERVAL '28 months', NOW()),
    (17, 'BATCH-INV-002-16', 200, 200,  2.50,  6.00, CURRENT_DATE + INTERVAL '20 months', NOW());

INSERT INTO purchase_items (purchase_id, medicine_id, batch_id, quantity, purchase_price, sell_price, expire_date)
VALUES
    (2, 2,  6,  150, 18.00, 35.00, CURRENT_DATE + INTERVAL '16 months'),
    (2, 4,  7,  200,  6.00, 12.00, CURRENT_DATE + INTERVAL '22 months'),
    (2, 9,  8,  250,  3.50,  8.00, CURRENT_DATE + INTERVAL '24 months'),
    (2, 15, 9,  300,  4.00,  8.00, CURRENT_DATE + INTERVAL '28 months'),
    (2, 17, 10, 200,  2.50,  6.00, CURRENT_DATE + INTERVAL '20 months');

INSERT INTO stock_movements (medicine_id, batch_id, quantity, type, reference_type, reference_id, created_at)
VALUES
    (2,  6,  150, 1, 1, 2, NOW()),
    (4,  7,  200, 1, 1, 2, NOW()),
    (9,  8,  250, 1, 1, 2, NOW()),
    (15, 9,  300, 1, 1, 2, NOW()),
    (17, 10, 200, 1, 1, 2, NOW());

INSERT INTO payments (type, reference_type, reference_id, method, user_id, amount, description, created_at)
VALUES (2, 2, 2, 1, 1, 6475.00, 'دفعة فاتورة شراء رقم INV-002', NOW());

-- =============================================================================
-- PURCHASE 3 — HealthPlus (supplier 3)
-- =============================================================================
INSERT INTO purchases (supplier_id, invoice_number, total_amount, status, note, created_at)
VALUES (3, 'INV-003', 9880.00, 2, 'Purchase from HealthPlus', NOW());

INSERT INTO batches (medicine_id, batch_number, quantity_entered, quantity_remaining, purchase_price, sell_price, expire_date, created_at)
VALUES
    (6,  'BATCH-INV-003-5',  100, 100, 22.00, 40.00, CURRENT_DATE + INTERVAL '18 months', NOW()),
    (8,  'BATCH-INV-003-7',  120, 120, 15.00, 30.00, CURRENT_DATE + INTERVAL '14 months', NOW()),
    (10, 'BATCH-INV-003-9',  200, 200,  3.00,  7.00, CURRENT_DATE + INTERVAL '26 months', NOW()),
    (11, 'BATCH-INV-003-10',  80,  80, 14.00, 28.00, CURRENT_DATE + INTERVAL '12 months', NOW()),
    (12, 'BATCH-INV-003-11',  60,  60, 16.00, 32.00, CURRENT_DATE + INTERVAL '10 months', NOW()),
    (14, 'BATCH-INV-003-13', 400, 400,  8.00, 18.00, CURRENT_DATE + INTERVAL '30 months', NOW());

INSERT INTO purchase_items (purchase_id, medicine_id, batch_id, quantity, purchase_price, sell_price, expire_date)
VALUES
    (3, 6,  11, 100, 22.00, 40.00, CURRENT_DATE + INTERVAL '18 months'),
    (3, 8,  12, 120, 15.00, 30.00, CURRENT_DATE + INTERVAL '14 months'),
    (3, 10, 13, 200,  3.00,  7.00, CURRENT_DATE + INTERVAL '26 months'),
    (3, 11, 14,  80, 14.00, 28.00, CURRENT_DATE + INTERVAL '12 months'),
    (3, 12, 15,  60, 16.00, 32.00, CURRENT_DATE + INTERVAL '10 months'),
    (3, 14, 16, 400,  8.00, 18.00, CURRENT_DATE + INTERVAL '30 months');

INSERT INTO stock_movements (medicine_id, batch_id, quantity, type, reference_type, reference_id, created_at)
VALUES
    (6,  11, 100, 1, 1, 3, NOW()),
    (8,  12, 120, 1, 1, 3, NOW()),
    (10, 13, 200, 1, 1, 3, NOW()),
    (11, 14,  80, 1, 1, 3, NOW()),
    (12, 15,  60, 1, 1, 3, NOW()),
    (14, 16, 400, 1, 1, 3, NOW());

INSERT INTO payments (type, reference_type, reference_id, method, user_id, amount, description, created_at)
VALUES (2, 2, 3, 1, 1, 9880.00, 'دفعة فاتورة شراء رقم INV-003', NOW());

-- =============================================================================
-- SALE 1 — Walk-in customer (customer 1, user 1)
-- =============================================================================
INSERT INTO sales (user_id, customer_id, status, total_amount, discount, note, created_at)
VALUES (1, 1, 2, 290.00, 0, 'Walk-in customer', NOW());

INSERT INTO sale_items (sale_id, medicine_id, batch_id, quantity, unit_price, total_price, purchase_price)
VALUES
    (1, 1,  1, 10, 15.00, 150.00,  8.50),
    (1, 3,  2, 20,  5.00, 100.00,  2.00),
    (1, 9,  8,  5,  8.00,  40.00,  3.50);

INSERT INTO stock_movements (medicine_id, batch_id, quantity, type, reference_type, reference_id, created_at)
VALUES
    (1, 1, 10, 2, 2, 1, NOW()),
    (3, 2, 20, 2, 2, 1, NOW()),
    (9, 8,  5, 2, 2, 1, NOW());

INSERT INTO payments (type, reference_type, reference_id, method, user_id, amount, description, created_at)
VALUES (1, 1, 1, 1, 1, 290.00, 'دفعة فاتورة بيع رقم 1', NOW());

-- Update batch stock after sale 1
UPDATE batches SET quantity_remaining = 190 WHERE batch_id = 1;
UPDATE batches SET quantity_remaining = 480 WHERE batch_id = 2;
UPDATE batches SET quantity_remaining = 245 WHERE batch_id = 8;

-- =============================================================================
-- SALE 2 — Regular customer monthly supply (customer 2, user 2)
-- =============================================================================
INSERT INTO sales (user_id, customer_id, status, total_amount, discount, note, created_at)
VALUES (2, 2, 2, 640.00, 0, 'Regular customer - monthly supply', NOW());

INSERT INTO sale_items (sale_id, medicine_id, batch_id, quantity, unit_price, total_price, purchase_price)
VALUES
    (2, 15, 9,  30,  8.00, 240.00, 4.00),
    (2, 5,  3,  10, 25.00, 250.00, 12.00),
    (2, 13, 5,  15, 10.00, 150.00, 4.50);

INSERT INTO stock_movements (medicine_id, batch_id, quantity, type, reference_type, reference_id, created_at)
VALUES
    (15, 9, 30, 2, 2, 2, NOW()),
    (5,  3, 10, 2, 2, 2, NOW()),
    (13, 5, 15, 2, 2, 2, NOW());

INSERT INTO payments (type, reference_type, reference_id, method, user_id, amount, description, created_at)
VALUES (1, 1, 2, 1, 2, 640.00, 'دفعة فاتورة بيع رقم 2', NOW());

UPDATE batches SET quantity_remaining = 270 WHERE batch_id = 9;
UPDATE batches SET quantity_remaining = 140 WHERE batch_id = 3;
UPDATE batches SET quantity_remaining = 285 WHERE batch_id = 5;

-- =============================================================================
-- SALE 3 — Customer with prescription (customer 3, user 1)
-- =============================================================================
INSERT INTO sales (user_id, customer_id, status, total_amount, discount, note, created_at)
VALUES (1, 3, 2, 580.00, 0, 'Customer with prescription', NOW());

INSERT INTO sale_items (sale_id, medicine_id, batch_id, quantity, unit_price, total_price, purchase_price)
VALUES
    (3, 2, 6,  6, 35.00, 210.00, 18.00),
    (3, 7, 4,  2, 35.00,  70.00, 18.00),
    (3, 8, 12, 10, 30.00, 300.00, 15.00);

INSERT INTO stock_movements (medicine_id, batch_id, quantity, type, reference_type, reference_id, created_at)
VALUES
    (2, 6,  6, 2, 2, 3, NOW()),
    (7, 4,  2, 2, 2, 3, NOW()),
    (8, 12, 10, 2, 2, 3, NOW());

INSERT INTO payments (type, reference_type, reference_id, method, user_id, amount, description, created_at)
VALUES (1, 1, 3, 1, 1, 580.00, 'دفعة فاتورة بيع رقم 3', NOW());

UPDATE batches SET quantity_remaining = 144 WHERE batch_id = 6;
UPDATE batches SET quantity_remaining = 98  WHERE batch_id = 4;
UPDATE batches SET quantity_remaining = 110 WHERE batch_id = 12;

-- =============================================================================
-- SALE 4 — VIP customer order (customer 4, user 2)
-- =============================================================================
INSERT INTO sales (user_id, customer_id, status, total_amount, discount, note, created_at)
VALUES (2, 4, 2, 1250.00, 0, 'VIP customer order', NOW());

INSERT INTO sale_items (sale_id, medicine_id, batch_id, quantity, unit_price, total_price, purchase_price)
VALUES
    (4, 6,  11, 15, 40.00, 600.00, 22.00),
    (4, 14, 16, 20, 18.00, 360.00,  8.00),
    (4, 17, 10, 25,  6.00, 150.00,  2.50),
    (4, 10, 13, 20,  7.00, 140.00,  3.00);

INSERT INTO stock_movements (medicine_id, batch_id, quantity, type, reference_type, reference_id, created_at)
VALUES
    (6,  11, 15, 2, 2, 4, NOW()),
    (14, 16, 20, 2, 2, 4, NOW()),
    (17, 10, 25, 2, 2, 4, NOW()),
    (10, 13, 20, 2, 2, 4, NOW());

INSERT INTO payments (type, reference_type, reference_id, method, user_id, amount, description, created_at)
VALUES (1, 1, 4, 1, 2, 1250.00, 'دفعة فاتورة بيع رقم 4', NOW());

UPDATE batches SET quantity_remaining = 85  WHERE batch_id = 11;
UPDATE batches SET quantity_remaining = 380 WHERE batch_id = 16;
UPDATE batches SET quantity_remaining = 175 WHERE batch_id = 10;
UPDATE batches SET quantity_remaining = 180 WHERE batch_id = 13;

-- =============================================================================
-- SALE 5 — Pharmacy stock-up (customer 5, user 1)
-- =============================================================================
INSERT INTO sales (user_id, customer_id, status, total_amount, discount, note, created_at)
VALUES (1, 5, 2, 1240.00, 0, 'Pharmacy stock-up', NOW());

INSERT INTO sale_items (sale_id, medicine_id, batch_id, quantity, unit_price, total_price, purchase_price)
VALUES
    (5, 4,  7,  30, 12.00, 360.00,  6.00),
    (5, 10, 13, 20,  7.00, 140.00,  3.00),
    (5, 11, 14, 15, 28.00, 420.00, 14.00),
    (5, 12, 15, 10, 32.00, 320.00, 16.00);

INSERT INTO stock_movements (medicine_id, batch_id, quantity, type, reference_type, reference_id, created_at)
VALUES
    (4,  7,  30, 2, 2, 5, NOW()),
    (10, 13, 20, 2, 2, 5, NOW()),
    (11, 14, 15, 2, 2, 5, NOW()),
    (12, 15, 10, 2, 2, 5, NOW());

INSERT INTO payments (type, reference_type, reference_id, method, user_id, amount, description, created_at)
VALUES (1, 1, 5, 1, 1, 1240.00, 'دفعة فاتورة بيع رقم 5', NOW());

UPDATE batches SET quantity_remaining = 170 WHERE batch_id = 7;
UPDATE batches SET quantity_remaining = 160 WHERE batch_id = 13;
UPDATE batches SET quantity_remaining = 65  WHERE batch_id = 14;
UPDATE batches SET quantity_remaining = 50  WHERE batch_id = 15;

-- =============================================================================
-- EXPENSES
-- =============================================================================
INSERT INTO expenses (user_id, amount, description, created_at)
VALUES
    (1, 15000.00, 'إيجار الشهر',     NOW()),
    (1,  2500.00, 'فاتورة كهرباء',   NOW()),
    (2,   800.00, 'صيانة عامة',       NOW());

COMMIT;
