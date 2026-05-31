-- =============================================================================
-- PharmaCore Database Seed Script — v2
-- =============================================================================
-- Run: PGPASSWORD=123 psql -h localhost -U postgres -d pharma_core -f others/seed.sql
-- =============================================================================

BEGIN;

DO $$ BEGIN IF EXISTS (SELECT 1 FROM users) THEN
    RAISE EXCEPTION 'Database already contains data. Truncate all tables first.';
END IF; END $$;

-- =============================================================================
-- USERS
-- =============================================================================
INSERT INTO users (user_name, password_hash, phone_number, address, role, created_at)
VALUES
    ('admin',    'pbkdf2$100000$YI1a2wEIM82pW+p/7ds8pA==$eaAYMFAyFrKN0DY9/en/vpN2+evQUh2lvOy3aiSsca0=', '01000000001', '123 Admin St',    1, NOW() - INTERVAL '90 days'),
    ('cashier1', 'pbkdf2$100000$jA2W4Kpm2Sl1NRZ4Fv3U5g==$Q98cwa8ZgCvih0uamPNFLNpGViOXG6J24cu39mQ6QVU=', '01000000002', '456 Cashier St',  2, NOW() - INTERVAL '85 days');

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
    ('PharmaDistrib Co.',          '01010000001', 'Cairo, Egypt',        NOW() - INTERVAL '90 days'),
    ('MedSupply Ltd.',             '01010000002', 'Alexandria, Egypt',   NOW() - INTERVAL '90 days'),
    ('HealthPlus Pharmaceuticals', '01010000003', 'Giza, Egypt',         NOW() - INTERVAL '90 days'),
    ('GlobalMed Trading',          '01010000004', 'Sharjah, UAE',        NOW() - INTERVAL '90 days'),
    ('United Pharma Group',        '01010000005', 'Riyadh, KSA',         NOW() - INTERVAL '90 days');

-- =============================================================================
-- CUSTOMERS
-- =============================================================================
INSERT INTO customers (name, phone_number, address, note, created_at)
VALUES
    ('Ahmed Hassan',    '01110000001', '15 El-Tahrir St, Cairo',        'Regular patient - monthly visit',  NOW() - INTERVAL '90 days'),
    ('Mohamed Ali',     '01110000002', '22 El-Haram St, Giza',          'Diabetic - monthly supply',        NOW() - INTERVAL '90 days'),
    ('Sara Ibrahim',    '01110000003', '8 El-Nile St, Alexandria',      'Asthma patient',                    NOW() - INTERVAL '90 days'),
    ('Khaled Omar',     '01110000004', '5 El-Salam St, Mansoura',       'VIP - hypertension',                NOW() - INTERVAL '90 days'),
    ('Nourhan Adel',    '01110000005', '12 El-Maadi St, Cairo',         'Clinic account',                    NOW() - INTERVAL '90 days'),
    ('Youssef Samir',   '01110000006', '3 El-Nozha St, Cairo',          'Diabetic',                          NOW() - INTERVAL '90 days'),
    ('Mona Tarek',      '01110000007', '18 El-Montazah St, Alexandria', 'Chronic patient',                   NOW() - INTERVAL '90 days'),
    ('Omar Farouk',     '01110000008', '7 El-Mohandeseen St, Giza',     'Walk-in customer',                  NOW() - INTERVAL '90 days'),
    ('Dina Hany',       '01110000009', '9 El-Sharq St, Port Said',      NULL,                                NOW() - INTERVAL '90 days'),
    ('Hassan Mahmoud',  '01110000010', '14 El-Abbaseya St, Cairo',      'Regular - skin conditions',         NOW() - INTERVAL '90 days');

-- =============================================================================
-- PURCHASE 1 — PharmaDistrib (90 days ago) — Initial stock
-- =============================================================================
INSERT INTO purchases (supplier_id, invoice_number, total_amount, status, note, created_at)
VALUES (1, 'INV-001', 7650.00, 2, 'First bulk order - opening stock', NOW() - INTERVAL '90 days');

INSERT INTO batches (medicine_id, batch_number, quantity_entered, quantity_remaining, purchase_price, sell_price, expire_date, created_at)
VALUES
    (1,  'BATCH-INV-001-0',  200, 200,  8.50,  15.00, CURRENT_DATE + INTERVAL '18 months', NOW() - INTERVAL '90 days'),
    (3,  'BATCH-INV-001-2',  500, 500,  2.00,   5.00, CURRENT_DATE + INTERVAL '24 months', NOW() - INTERVAL '90 days'),
    (5,  'BATCH-INV-001-4',  150, 150, 12.00,  25.00, CURRENT_DATE + INTERVAL '20 months', NOW() - INTERVAL '90 days'),
    (7,  'BATCH-INV-001-6',  100, 100, 18.00,  35.00, CURRENT_DATE + INTERVAL '15 months', NOW() - INTERVAL '90 days'),
    (13, 'BATCH-INV-001-12', 300, 300,  4.50,  10.00, CURRENT_DATE + INTERVAL '30 months', NOW() - INTERVAL '90 days');

INSERT INTO purchase_items (purchase_id, medicine_id, batch_id, quantity, purchase_price, sell_price, expire_date)
VALUES
    (1, 1,  1,  200,  8.50,  15.00, CURRENT_DATE + INTERVAL '18 months'),
    (1, 3,  2,  500,  2.00,   5.00, CURRENT_DATE + INTERVAL '24 months'),
    (1, 5,  3,  150, 12.00,  25.00, CURRENT_DATE + INTERVAL '20 months'),
    (1, 7,  4,  100, 18.00,  35.00, CURRENT_DATE + INTERVAL '15 months'),
    (1, 13, 5,  300,  4.50,  10.00, CURRENT_DATE + INTERVAL '30 months');

INSERT INTO stock_movements (medicine_id, batch_id, quantity, type, reference_type, reference_id, created_at)
VALUES
    (1,  1, 200, 1, 1, 1, NOW() - INTERVAL '90 days'),
    (3,  2, 500, 1, 1, 1, NOW() - INTERVAL '90 days'),
    (5,  3, 150, 1, 1, 1, NOW() - INTERVAL '90 days'),
    (7,  4, 100, 1, 1, 1, NOW() - INTERVAL '90 days'),
    (13, 5, 300, 1, 1, 1, NOW() - INTERVAL '90 days');

INSERT INTO payments (type, reference_type, reference_id, method, user_id, amount, description, created_at)
VALUES (2, 2, 1, 1, 1, 7650.00, 'دفعة فاتورة شراء رقم INV-001', NOW() - INTERVAL '90 days');

-- =============================================================================
-- PURCHASE 2 — MedSupply (60 days ago)
-- =============================================================================
INSERT INTO purchases (supplier_id, invoice_number, total_amount, status, note, created_at)
VALUES (2, 'INV-002', 6475.00, 2, 'Second order - antibiotics and chronic meds', NOW() - INTERVAL '60 days');

INSERT INTO batches (medicine_id, batch_number, quantity_entered, quantity_remaining, purchase_price, sell_price, expire_date, created_at)
VALUES
    (2,  'BATCH-INV-002-1',  150, 150, 18.00, 35.00, CURRENT_DATE + INTERVAL '16 months', NOW() - INTERVAL '60 days'),
    (4,  'BATCH-INV-002-3',  200, 200,  6.00, 12.00, CURRENT_DATE + INTERVAL '22 months', NOW() - INTERVAL '60 days'),
    (9,  'BATCH-INV-002-8',  250, 250,  3.50,  8.00, CURRENT_DATE + INTERVAL '24 months', NOW() - INTERVAL '60 days'),
    (15, 'BATCH-INV-002-14', 300, 300,  4.00,  8.00, CURRENT_DATE + INTERVAL '28 months', NOW() - INTERVAL '60 days'),
    (17, 'BATCH-INV-002-16', 200, 200,  2.50,  6.00, CURRENT_DATE + INTERVAL '20 months', NOW() - INTERVAL '60 days');

INSERT INTO purchase_items (purchase_id, medicine_id, batch_id, quantity, purchase_price, sell_price, expire_date)
VALUES
    (2, 2,  6,  150, 18.00, 35.00, CURRENT_DATE + INTERVAL '16 months'),
    (2, 4,  7,  200,  6.00, 12.00, CURRENT_DATE + INTERVAL '22 months'),
    (2, 9,  8,  250,  3.50,  8.00, CURRENT_DATE + INTERVAL '24 months'),
    (2, 15, 9,  300,  4.00,  8.00, CURRENT_DATE + INTERVAL '28 months'),
    (2, 17, 10, 200,  2.50,  6.00, CURRENT_DATE + INTERVAL '20 months');

INSERT INTO stock_movements (medicine_id, batch_id, quantity, type, reference_type, reference_id, created_at)
VALUES
    (2,  6,  150, 1, 1, 2, NOW() - INTERVAL '60 days'),
    (4,  7,  200, 1, 1, 2, NOW() - INTERVAL '60 days'),
    (9,  8,  250, 1, 1, 2, NOW() - INTERVAL '60 days'),
    (15, 9,  300, 1, 1, 2, NOW() - INTERVAL '60 days'),
    (17, 10, 200, 1, 1, 2, NOW() - INTERVAL '60 days');

INSERT INTO payments (type, reference_type, reference_id, method, user_id, amount, description, created_at)
VALUES (2, 2, 2, 1, 1, 6475.00, 'دفعة فاتورة شراء رقم INV-002', NOW() - INTERVAL '60 days');

-- =============================================================================
-- PURCHASE 3 — HealthPlus (30 days ago)
-- =============================================================================
INSERT INTO purchases (supplier_id, invoice_number, total_amount, status, note, created_at)
VALUES (3, 'INV-003', 9880.00, 2, 'Respiratory and dermatology stock', NOW() - INTERVAL '30 days');

INSERT INTO batches (medicine_id, batch_number, quantity_entered, quantity_remaining, purchase_price, sell_price, expire_date, created_at)
VALUES
    (6,  'BATCH-INV-003-5',  100, 100, 22.00, 40.00, CURRENT_DATE + INTERVAL '18 months', NOW() - INTERVAL '30 days'),
    (8,  'BATCH-INV-003-7',  120, 120, 15.00, 30.00, CURRENT_DATE + INTERVAL '14 months', NOW() - INTERVAL '30 days'),
    (10, 'BATCH-INV-003-9',  200, 200,  3.00,  7.00, CURRENT_DATE + INTERVAL '26 months', NOW() - INTERVAL '30 days'),
    (11, 'BATCH-INV-003-10',  80,  80, 14.00, 28.00, CURRENT_DATE + INTERVAL '12 months', NOW() - INTERVAL '30 days'),
    (12, 'BATCH-INV-003-11',  60,  60, 16.00, 32.00, CURRENT_DATE + INTERVAL '10 months', NOW() - INTERVAL '30 days'),
    (14, 'BATCH-INV-003-13', 400, 400,  8.00, 18.00, CURRENT_DATE + INTERVAL '30 months', NOW() - INTERVAL '30 days');

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
    (6,  11, 100, 1, 1, 3, NOW() - INTERVAL '30 days'),
    (8,  12, 120, 1, 1, 3, NOW() - INTERVAL '30 days'),
    (10, 13, 200, 1, 1, 3, NOW() - INTERVAL '30 days'),
    (11, 14,  80, 1, 1, 3, NOW() - INTERVAL '30 days'),
    (12, 15,  60, 1, 1, 3, NOW() - INTERVAL '30 days'),
    (14, 16, 400, 1, 1, 3, NOW() - INTERVAL '30 days');

INSERT INTO payments (type, reference_type, reference_id, method, user_id, amount, description, created_at)
VALUES (2, 2, 3, 1, 1, 9880.00, 'دفعة فاتورة شراء رقم INV-003', NOW() - INTERVAL '30 days');

-- =============================================================================
-- PURCHASE 4 — GlobalMed (14 days ago) — Cover missing meds + restock fast-movers
-- =============================================================================
INSERT INTO purchases (supplier_id, invoice_number, total_amount, status, note, created_at)
VALUES (4, 'INV-004', 9250.00, 2, 'Covering diabetic and allergy meds + restock', NOW() - INTERVAL '14 days');

INSERT INTO batches (medicine_id, batch_number, quantity_entered, quantity_remaining, purchase_price, sell_price, expire_date, created_at)
VALUES
    (16, 'BATCH-INV-004-15',  50,  50, 45.00, 80.00, CURRENT_DATE + INTERVAL '24 months', NOW() - INTERVAL '14 days'),
    (18, 'BATCH-INV-004-17', 300, 300,  2.00,  5.00, CURRENT_DATE + INTERVAL '30 months', NOW() - INTERVAL '14 days'),
    (19, 'BATCH-INV-004-18', 200, 200,  5.00, 12.00, CURRENT_DATE + INTERVAL '18 months', NOW() - INTERVAL '14 days'),
    (20, 'BATCH-INV-004-19', 150, 150,  8.00, 18.00, CURRENT_DATE + INTERVAL '20 months', NOW() - INTERVAL '14 days'),
    (1,  'BATCH-INV-004-0',  300, 300,  8.00, 15.00, CURRENT_DATE + INTERVAL '22 months', NOW() - INTERVAL '14 days'),
    (3,  'BATCH-INV-004-2', 1000,1000,  1.80,  5.00, CURRENT_DATE + INTERVAL '26 months', NOW() - INTERVAL '14 days');

INSERT INTO purchase_items (purchase_id, medicine_id, batch_id, quantity, purchase_price, sell_price, expire_date)
VALUES
    (4, 16, 17,  50, 45.00, 80.00, CURRENT_DATE + INTERVAL '24 months'),
    (4, 18, 18, 300,  2.00,  5.00, CURRENT_DATE + INTERVAL '30 months'),
    (4, 19, 19, 200,  5.00, 12.00, CURRENT_DATE + INTERVAL '18 months'),
    (4, 20, 20, 150,  8.00, 18.00, CURRENT_DATE + INTERVAL '20 months'),
    (4, 1,  21, 300,  8.00, 15.00, CURRENT_DATE + INTERVAL '22 months'),
    (4, 3,  22,1000,  1.80,  5.00, CURRENT_DATE + INTERVAL '26 months');

INSERT INTO stock_movements (medicine_id, batch_id, quantity, type, reference_type, reference_id, created_at)
VALUES
    (16, 17,  50, 1, 1, 4, NOW() - INTERVAL '14 days'),
    (18, 18, 300, 1, 1, 4, NOW() - INTERVAL '14 days'),
    (19, 19, 200, 1, 1, 4, NOW() - INTERVAL '14 days'),
    (20, 20, 150, 1, 1, 4, NOW() - INTERVAL '14 days'),
    (1,  21, 300, 1, 1, 4, NOW() - INTERVAL '14 days'),
    (3,  22,1000, 1, 1, 4, NOW() - INTERVAL '14 days');

INSERT INTO payments (type, reference_type, reference_id, method, user_id, amount, description, created_at)
VALUES (2, 2, 4, 2, 1, 9250.00, 'دفعة فاتورة شراء رقم INV-004 (كارد)', NOW() - INTERVAL '14 days');

-- =============================================================================
-- PURCHASE 5 — United Pharma (7 days ago) — Restock fast-movers
-- =============================================================================
INSERT INTO purchases (supplier_id, invoice_number, total_amount, status, note, created_at)
VALUES (5, 'INV-005', 6250.00, 2, 'Restock top-selling items', NOW() - INTERVAL '7 days');

INSERT INTO batches (medicine_id, batch_number, quantity_entered, quantity_remaining, purchase_price, sell_price, expire_date, created_at)
VALUES
    (4,  'BATCH-INV-005-3',  300, 300,  5.50, 12.00, CURRENT_DATE + INTERVAL '20 months', NOW() - INTERVAL '7 days'),
    (9,  'BATCH-INV-005-8',  400, 400,  3.00,  8.00, CURRENT_DATE + INTERVAL '22 months', NOW() - INTERVAL '7 days'),
    (13, 'BATCH-INV-005-12', 500, 500,  4.00, 10.00, CURRENT_DATE + INTERVAL '28 months', NOW() - INTERVAL '7 days'),
    (15, 'BATCH-INV-005-14', 400, 400,  3.50,  8.00, CURRENT_DATE + INTERVAL '26 months', NOW() - INTERVAL '7 days');

INSERT INTO purchase_items (purchase_id, medicine_id, batch_id, quantity, purchase_price, sell_price, expire_date)
VALUES
    (5, 4,  23, 300,  5.50, 12.00, CURRENT_DATE + INTERVAL '20 months'),
    (5, 9,  24, 400,  3.00,  8.00, CURRENT_DATE + INTERVAL '22 months'),
    (5, 13, 25, 500,  4.00, 10.00, CURRENT_DATE + INTERVAL '28 months'),
    (5, 15, 26, 400,  3.50,  8.00, CURRENT_DATE + INTERVAL '26 months');

INSERT INTO stock_movements (medicine_id, batch_id, quantity, type, reference_type, reference_id, created_at)
VALUES
    (4,  23, 300, 1, 1, 5, NOW() - INTERVAL '7 days'),
    (9,  24, 400, 1, 1, 5, NOW() - INTERVAL '7 days'),
    (13, 25, 500, 1, 1, 5, NOW() - INTERVAL '7 days'),
    (15, 26, 400, 1, 1, 5, NOW() - INTERVAL '7 days');

INSERT INTO payments (type, reference_type, reference_id, method, user_id, amount, description, created_at)
VALUES (2, 2, 5, 1, 1, 6250.00, 'دفعة فاتورة شراء رقم INV-005', NOW() - INTERVAL '7 days');

-- =============================================================================
-- SALE 1 — Walk-in (80 days ago) — customer 1, user 1
-- =============================================================================
INSERT INTO sales (user_id, customer_id, status, total_amount, discount, note, created_at)
VALUES (1, 1, 2, 340.00, 0, 'Walk-in - fever and infection', NOW() - INTERVAL '80 days');

INSERT INTO sale_items (sale_id, medicine_id, batch_id, quantity, unit_price, total_price, purchase_price)
VALUES
    (1, 1,  1, 10, 15.00, 150.00,  8.50),
    (1, 3,  2, 30,  5.00, 150.00,  2.00),
    (1, 9,  8,  5,  8.00,  40.00,  3.50);

INSERT INTO stock_movements (medicine_id, batch_id, quantity, type, reference_type, reference_id, created_at)
VALUES
    (1, 1, 10, 2, 2, 1, NOW() - INTERVAL '80 days'),
    (3, 2, 30, 2, 2, 1, NOW() - INTERVAL '80 days'),
    (9, 8,  5, 2, 2, 1, NOW() - INTERVAL '80 days');

INSERT INTO payments (type, reference_type, reference_id, method, user_id, amount, description, created_at)
VALUES (1, 1, 1, 1, 1, 340.00, 'دفعة فاتورة بيع رقم 1', NOW() - INTERVAL '80 days');

UPDATE batches SET quantity_remaining = 190 WHERE batch_id = 1;
UPDATE batches SET quantity_remaining = 470 WHERE batch_id = 2;
UPDATE batches SET quantity_remaining = 245 WHERE batch_id = 8;

-- =============================================================================
-- SALE 2 — Diabetic patient (55 days ago) — customer 2, user 2
-- =============================================================================
INSERT INTO sales (user_id, customer_id, status, total_amount, discount, note, created_at)
VALUES (2, 2, 2, 610.00, 0, 'Monthly diabetic supply', NOW() - INTERVAL '55 days');

INSERT INTO sale_items (sale_id, medicine_id, batch_id, quantity, unit_price, total_price, purchase_price)
VALUES
    (2, 15, 9,  20,  8.00, 160.00, 4.00),
    (2, 5,  3,  10, 25.00, 250.00, 12.00),
    (2, 13, 5,  20, 10.00, 200.00, 4.50);

INSERT INTO stock_movements (medicine_id, batch_id, quantity, type, reference_type, reference_id, created_at)
VALUES
    (15, 9, 20, 2, 2, 2, NOW() - INTERVAL '55 days'),
    (5,  3, 10, 2, 2, 2, NOW() - INTERVAL '55 days'),
    (13, 5, 20, 2, 2, 2, NOW() - INTERVAL '55 days');

INSERT INTO payments (type, reference_type, reference_id, method, user_id, amount, description, created_at)
VALUES (1, 1, 2, 1, 2, 610.00, 'دفعة فاتورة بيع رقم 2', NOW() - INTERVAL '55 days');

UPDATE batches SET quantity_remaining = 280 WHERE batch_id = 9;
UPDATE batches SET quantity_remaining = 140 WHERE batch_id = 3;
UPDATE batches SET quantity_remaining = 280 WHERE batch_id = 5;

-- =============================================================================
-- SALE 3 — Asthma patient (25 days ago) — customer 3, user 1 — CARD payment
-- =============================================================================
INSERT INTO sales (user_id, customer_id, status, total_amount, discount, note, created_at)
VALUES (1, 3, 2, 905.00, 0, 'Respiratory infection - prescription', NOW() - INTERVAL '25 days');

INSERT INTO sale_items (sale_id, medicine_id, batch_id, quantity, unit_price, total_price, purchase_price)
VALUES
    (3, 2, 6,  10, 35.00, 350.00, 18.00),
    (3, 7, 4,   3, 35.00, 105.00, 18.00),
    (3, 8, 12, 15, 30.00, 450.00, 15.00);

INSERT INTO stock_movements (medicine_id, batch_id, quantity, type, reference_type, reference_id, created_at)
VALUES
    (2, 6,  10, 2, 2, 3, NOW() - INTERVAL '25 days'),
    (7, 4,   3, 2, 2, 3, NOW() - INTERVAL '25 days'),
    (8, 12, 15, 2, 2, 3, NOW() - INTERVAL '25 days');

INSERT INTO payments (type, reference_type, reference_id, method, user_id, amount, description, created_at)
VALUES (1, 1, 3, 2, 1, 905.00, 'دفعة فاتورة بيع رقم 3 (كارد)', NOW() - INTERVAL '25 days');

UPDATE batches SET quantity_remaining = 140 WHERE batch_id = 6;
UPDATE batches SET quantity_remaining = 97  WHERE batch_id = 4;
UPDATE batches SET quantity_remaining = 105 WHERE batch_id = 12;

-- =============================================================================
-- SALE 4 — VIP hypertension patient (20 days ago) — customer 4, user 2 — partial
-- =============================================================================
INSERT INTO sales (user_id, customer_id, status, total_amount, discount, note, created_at)
VALUES (2, 4, 2, 1625.00, 25, 'VIP quarterly checkup - with discount', NOW() - INTERVAL '20 days');

INSERT INTO sale_items (sale_id, medicine_id, batch_id, quantity, unit_price, total_price, purchase_price)
VALUES
    (4, 6,  11, 20, 40.00,  800.00, 22.00),
    (4, 14, 16, 30, 18.00,  540.00,  8.00),
    (4, 17, 10, 30,  6.00,  180.00,  2.50),
    (4, 10, 13, 15,  7.00,  105.00,  3.00);

INSERT INTO stock_movements (medicine_id, batch_id, quantity, type, reference_type, reference_id, created_at)
VALUES
    (6,  11, 20, 2, 2, 4, NOW() - INTERVAL '20 days'),
    (14, 16, 30, 2, 2, 4, NOW() - INTERVAL '20 days'),
    (17, 10, 30, 2, 2, 4, NOW() - INTERVAL '20 days'),
    (10, 13, 15, 2, 2, 4, NOW() - INTERVAL '20 days');

INSERT INTO payments (type, reference_type, reference_id, method, user_id, amount, description, created_at)
VALUES (1, 1, 4, 1, 2, 1500.00, 'دفعة فاتورة بيع رقم 4 (دفعة جزئية)', NOW() - INTERVAL '20 days');

UPDATE batches SET quantity_remaining = 80  WHERE batch_id = 11;
UPDATE batches SET quantity_remaining = 370 WHERE batch_id = 16;
UPDATE batches SET quantity_remaining = 170 WHERE batch_id = 10;
UPDATE batches SET quantity_remaining = 185 WHERE batch_id = 13;

-- =============================================================================
-- SALE 5 — Clinic order (14 days ago) — customer 5, user 1 — CARD
-- =============================================================================
INSERT INTO sales (user_id, customer_id, status, total_amount, discount, note, created_at)
VALUES (1, 5, 2, 1520.00, 20, 'Clinic bulk order', NOW() - INTERVAL '14 days');

INSERT INTO sale_items (sale_id, medicine_id, batch_id, quantity, unit_price, total_price, purchase_price)
VALUES
    (5, 4,  7,  40, 12.00, 480.00,  6.00),
    (5, 11, 14, 20, 28.00, 560.00, 14.00),
    (5, 12, 15, 15, 32.00, 480.00, 16.00);

INSERT INTO stock_movements (medicine_id, batch_id, quantity, type, reference_type, reference_id, created_at)
VALUES
    (4,  7,  40, 2, 2, 5, NOW() - INTERVAL '14 days'),
    (11, 14, 20, 2, 2, 5, NOW() - INTERVAL '14 days'),
    (12, 15, 15, 2, 2, 5, NOW() - INTERVAL '14 days');

INSERT INTO payments (type, reference_type, reference_id, method, user_id, amount, description, created_at)
VALUES (1, 1, 5, 2, 1, 1500.00, 'دفعة فاتورة بيع رقم 5 (كارد)', NOW() - INTERVAL '14 days');

UPDATE batches SET quantity_remaining = 160 WHERE batch_id = 7;
UPDATE batches SET quantity_remaining = 60  WHERE batch_id = 14;
UPDATE batches SET quantity_remaining = 45  WHERE batch_id = 15;

-- =============================================================================
-- SALE 6 — Diabetic (7 days ago) — customer 6, user 1
-- =============================================================================
INSERT INTO sales (user_id, customer_id, status, total_amount, discount, note, created_at)
VALUES (1, 6, 2, 620.00, 0, 'Diabetic checkup - insulin and supplements', NOW() - INTERVAL '7 days');

INSERT INTO sale_items (sale_id, medicine_id, batch_id, quantity, unit_price, total_price, purchase_price)
VALUES
    (6, 16, 17,  5, 80.00, 400.00, 45.00),
    (6, 19, 19, 10, 12.00, 120.00,  5.00),
    (6, 18, 18, 20,  5.00, 100.00,  2.00);

INSERT INTO stock_movements (medicine_id, batch_id, quantity, type, reference_type, reference_id, created_at)
VALUES
    (16, 17,  5, 2, 2, 6, NOW() - INTERVAL '7 days'),
    (19, 19, 10, 2, 2, 6, NOW() - INTERVAL '7 days'),
    (18, 18, 20, 2, 2, 6, NOW() - INTERVAL '7 days');

INSERT INTO payments (type, reference_type, reference_id, method, user_id, amount, description, created_at)
VALUES (1, 1, 6, 1, 1, 620.00, 'دفعة فاتورة بيع رقم 6', NOW() - INTERVAL '7 days');

UPDATE batches SET quantity_remaining = 45  WHERE batch_id = 17;
UPDATE batches SET quantity_remaining = 190 WHERE batch_id = 19;
UPDATE batches SET quantity_remaining = 280 WHERE batch_id = 18;

-- =============================================================================
-- SALE 7 — Chronic patient (3 days ago) — customer 7, user 2 — CARD
-- =============================================================================
INSERT INTO sales (user_id, customer_id, status, total_amount, discount, note, created_at)
VALUES (2, 7, 2, 510.00, 0, 'Monthly chronic disease medication', NOW() - INTERVAL '3 days');

INSERT INTO sale_items (sale_id, medicine_id, batch_id, quantity, unit_price, total_price, purchase_price)
VALUES
    (7, 14, 16, 10, 18.00, 180.00, 8.00),
    (7, 15, 9,  30,  8.00, 240.00, 4.00),
    (7, 17, 10, 15,  6.00,  90.00, 2.50);

INSERT INTO stock_movements (medicine_id, batch_id, quantity, type, reference_type, reference_id, created_at)
VALUES
    (14, 16, 10, 2, 2, 7, NOW() - INTERVAL '3 days'),
    (15, 9,  30, 2, 2, 7, NOW() - INTERVAL '3 days'),
    (17, 10, 15, 2, 2, 7, NOW() - INTERVAL '3 days');

INSERT INTO payments (type, reference_type, reference_id, method, user_id, amount, description, created_at)
VALUES (1, 1, 7, 2, 2, 510.00, 'دفعة فاتورة بيع رقم 7 (كارد)', NOW() - INTERVAL '3 days');

UPDATE batches SET quantity_remaining = 360 WHERE batch_id = 16;
UPDATE batches SET quantity_remaining = 250 WHERE batch_id = 9;
UPDATE batches SET quantity_remaining = 155 WHERE batch_id = 10;

-- =============================================================================
-- SALE 8 — Walk-in (today) — customer 8, user 1
-- =============================================================================
INSERT INTO sales (user_id, customer_id, status, total_amount, discount, note, created_at)
VALUES (1, 8, 2, 505.00, 5, 'Walk-in - pain and fever', NOW());

INSERT INTO sale_items (sale_id, medicine_id, batch_id, quantity, unit_price, total_price, purchase_price)
VALUES
    (8, 20, 20, 10, 18.00, 180.00,  8.00),
    (8, 3,  22, 20,  5.00, 100.00,  1.80),
    (8, 1,  21, 15, 15.00, 225.00,  8.00);

INSERT INTO stock_movements (medicine_id, batch_id, quantity, type, reference_type, reference_id, created_at)
VALUES
    (20, 20, 10, 2, 2, 8, NOW()),
    (3,  22, 20, 2, 2, 8, NOW()),
    (1,  21, 15, 2, 2, 8, NOW());

INSERT INTO payments (type, reference_type, reference_id, method, user_id, amount, description, created_at)
VALUES (1, 1, 8, 1, 1, 500.00, 'دفعة فاتورة بيع رقم 8', NOW());

UPDATE batches SET quantity_remaining = 140 WHERE batch_id = 20;
UPDATE batches SET quantity_remaining = 980 WHERE batch_id = 22;
UPDATE batches SET quantity_remaining = 285 WHERE batch_id = 21;

-- =============================================================================
-- SALES RETURN 1 — customer 1 returns partial (70 days ago)
-- =============================================================================
INSERT INTO sales_returns (sale_id, customer_id, user_id, total_amount, note, created_at)
VALUES (1, 1, 1, 25.00, 'Returned 5 Paracetamol tablets - nearing expiry', NOW() - INTERVAL '70 days');

INSERT INTO sales_return_items (sales_return_id, sale_item_id, batch_id, quantity, unit_price, total_price)
VALUES (1, 2, 2, 5, 5.00, 25.00);

-- Stock comes back in
INSERT INTO stock_movements (medicine_id, batch_id, quantity, type, reference_type, reference_id, created_at)
VALUES (3, 2, 5, 1, 3, 1, NOW() - INTERVAL '70 days');

UPDATE batches SET quantity_remaining = 475 WHERE batch_id = 2;

-- Refund payment
INSERT INTO payments (type, reference_type, reference_id, method, user_id, amount, description, created_at)
VALUES (2, 4, 1, 1, 1, 25.00, 'استرجاع مشتريات - فاتورة بيع رقم 1', NOW() - INTERVAL '70 days');

-- =============================================================================
-- PURCHASE RETURN 1 — Return defective to MedSupply (50 days ago)
-- =============================================================================
INSERT INTO purchase_returns (purchase_id, supplier_id, user_id, total_amount, note, created_at)
VALUES (2, 2, 1, 60.00, 'Returned 10 Ibuprofen strips - damaged packaging', NOW() - INTERVAL '50 days');

INSERT INTO purchase_return_items (purchase_return_id, purchase_item_id, batch_id, quantity, unit_price, total_price)
VALUES (1, 7, 7, 10, 6.00, 60.00);

-- Stock goes out
INSERT INTO stock_movements (medicine_id, batch_id, quantity, type, reference_type, reference_id, created_at)
VALUES (4, 7, 10, 2, 3, 1, NOW() - INTERVAL '50 days');

UPDATE batches SET quantity_remaining = 150 WHERE batch_id = 7;

-- Refund received
INSERT INTO payments (type, reference_type, reference_id, method, user_id, amount, description, created_at)
VALUES (1, 5, 1, 1, 1, 60.00, 'استرجاع مشتريات - فاتورة شراء رقم INV-002', NOW() - INTERVAL '50 days');

-- =============================================================================
-- ADJUSTMENTS
-- =============================================================================
-- Damaged goods found during inventory (40 days ago)
INSERT INTO adjustments (medicine_id, batch_id, quantity, type, reason, user_id, created_at)
VALUES (1, 1, 2, 2, 'تلف في المخزون - أمبيسيلين', 1, NOW() - INTERVAL '40 days');

INSERT INTO stock_movements (medicine_id, batch_id, quantity, type, reference_type, reference_id, created_at)
VALUES (1, 1, 2, 3, 4, 1, NOW() - INTERVAL '40 days');

UPDATE batches SET quantity_remaining = 188 WHERE batch_id = 1;

-- Surplus found during inventory count (10 days ago)
INSERT INTO adjustments (medicine_id, batch_id, quantity, type, reason, user_id, created_at)
VALUES (5, 3, 1, 1, 'جرد مخازن - زيادة', 1, NOW() - INTERVAL '10 days');

INSERT INTO stock_movements (medicine_id, batch_id, quantity, type, reference_type, reference_id, created_at)
VALUES (5, 3, 1, 3, 4, 2, NOW() - INTERVAL '10 days');

UPDATE batches SET quantity_remaining = 141 WHERE batch_id = 3;

-- =============================================================================
-- EXPENSES — spread across the timeline
-- =============================================================================
INSERT INTO expenses (user_id, amount, description, created_at)
VALUES
    (1, 15000.00, 'إيجار الشهر - مارس',   NOW() - INTERVAL '85 days'),
    (1,  2500.00, 'فاتورة كهرباء',         NOW() - INTERVAL '55 days'),
    (1,  1200.00, 'صيانة معدات التبريد',   NOW() - INTERVAL '25 days'),
    (2,   400.00, 'فاتورة مياه',           NOW() - INTERVAL '10 days'),
    (1, 15000.00, 'إيجار الشهر - أبريل',   NOW() - INTERVAL '5 days');

COMMIT;
