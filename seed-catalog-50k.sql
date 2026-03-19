USE `ecommerce-plataform-catalog-write`;

SET @target_products = 50000;
SET @replace_existing_data = 1;

DROP PROCEDURE IF EXISTS seed_catalog_massive_load;

DELIMITER $$

CREATE PROCEDURE seed_catalog_massive_load()
BEGIN
    IF @replace_existing_data = 1 THEN
        TRUNCATE TABLE `ecommerce-plataform-catalog-read`.products;
        TRUNCATE TABLE `ecommerce-plataform-catalog-read`.categories;
        TRUNCATE TABLE `ecommerce-plataform-catalog-write`.products;
        TRUNCATE TABLE `ecommerce-plataform-catalog-write`.categories;
    END IF;

    DROP TEMPORARY TABLE IF EXISTS tmp_numbers;
    DROP TEMPORARY TABLE IF EXISTS tmp_categories;

    CREATE TEMPORARY TABLE tmp_categories (
        seq INT NOT NULL PRIMARY KEY,
        id CHAR(36) NOT NULL,
        name VARCHAR(150) NOT NULL
    );

    INSERT INTO tmp_categories (seq, id, name)
    VALUES
        (1, '7d1ce379-748d-4d77-b9c9-99a610000001', 'Processadores'),
        (2, '7d1ce379-748d-4d77-b9c9-99a610000002', 'Placas de Video'),
        (3, '7d1ce379-748d-4d77-b9c9-99a610000003', 'Placas Mae'),
        (4, '7d1ce379-748d-4d77-b9c9-99a610000004', 'Memorias RAM'),
        (5, '7d1ce379-748d-4d77-b9c9-99a610000005', 'SSDs'),
        (6, '7d1ce379-748d-4d77-b9c9-99a610000006', 'Fontes'),
        (7, '7d1ce379-748d-4d77-b9c9-99a610000007', 'Gabinetes'),
        (8, '7d1ce379-748d-4d77-b9c9-99a610000008', 'Water Coolers'),
        (9, '7d1ce379-748d-4d77-b9c9-99a610000009', 'Monitores'),
        (10, '7d1ce379-748d-4d77-b9c9-99a610000010', 'Notebooks');

    INSERT INTO `ecommerce-plataform-catalog-write`.categories (Id, name)
    SELECT id, name
    FROM tmp_categories
    ON DUPLICATE KEY UPDATE
        name = VALUES(name);

    INSERT INTO `ecommerce-plataform-catalog-read`.categories (Id, name)
    SELECT id, name
    FROM tmp_categories
    ON DUPLICATE KEY UPDATE
        name = VALUES(name);

    CREATE TEMPORARY TABLE tmp_numbers (
        product_number INT NOT NULL PRIMARY KEY
    );

    INSERT INTO tmp_numbers (product_number)
    SELECT generated_number
    FROM (
        SELECT
            ones.n
            + tens.n * 10
            + hundreds.n * 100
            + thousands.n * 1000
            + ten_thousands.n * 10000
            + 1 AS generated_number
        FROM
            (SELECT 0 AS n UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4 UNION ALL SELECT 5 UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9) ones
        CROSS JOIN
            (SELECT 0 AS n UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4 UNION ALL SELECT 5 UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9) tens
        CROSS JOIN
            (SELECT 0 AS n UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4 UNION ALL SELECT 5 UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9) hundreds
        CROSS JOIN
            (SELECT 0 AS n UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4 UNION ALL SELECT 5 UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9) thousands
        CROSS JOIN
            (SELECT 0 AS n UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4 UNION ALL SELECT 5 UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9) ten_thousands
    ) numbers
    WHERE generated_number <= @target_products;

    INSERT INTO `ecommerce-plataform-catalog-write`.products
    (
        Id,
        name,
        description,
        price,
        stock_quantity,
        active,
        category_id,
        height_cm,
        width_cm,
        cubage_m3,
        weight_kg,
        origin_zip_code
    )
    SELECT
        UUID() AS Id,
        CASE category.seq
            WHEN 1 THEN CONCAT(
                CASE MOD(numbers.product_number, 2)
                    WHEN 0 THEN 'Processador Intel Core i'
                    ELSE 'Processador AMD Ryzen '
                END,
                3 + MOD(numbers.product_number, 7),
                ' Serie ',
                1000 + MOD(numbers.product_number * 17, 9000)
            )
            WHEN 2 THEN CONCAT(
                CASE MOD(numbers.product_number, 3)
                    WHEN 0 THEN 'Placa de Video NVIDIA RTX '
                    WHEN 1 THEN 'Placa de Video AMD Radeon RX '
                    ELSE 'Placa de Video Intel Arc '
                END,
                3050 + MOD(numbers.product_number * 13, 5000)
            )
            WHEN 3 THEN CONCAT(
                CASE MOD(numbers.product_number, 3)
                    WHEN 0 THEN 'Placa Mae ASUS B'
                    WHEN 1 THEN 'Placa Mae Gigabyte X'
                    ELSE 'Placa Mae ASRock Z'
                END,
                450 + MOD(numbers.product_number * 7, 350)
            )
            WHEN 4 THEN CONCAT(
                'Memoria RAM DDR',
                4 + MOD(numbers.product_number, 2),
                ' ',
                8 * (1 + MOD(numbers.product_number, 8)),
                'GB '
                ,CASE MOD(numbers.product_number, 3)
                    WHEN 0 THEN 'Kingston Fury'
                    WHEN 1 THEN 'Corsair Vengeance'
                    ELSE 'G.Skill Ripjaws'
                END
            )
            WHEN 5 THEN CONCAT(
                CASE MOD(numbers.product_number, 2)
                    WHEN 0 THEN 'SSD NVMe '
                    ELSE 'SSD SATA '
                END,
                240 * (1 + MOD(numbers.product_number, 8)),
                'GB ',
                CASE MOD(numbers.product_number, 3)
                    WHEN 0 THEN 'Samsung'
                    WHEN 1 THEN 'Kingston'
                    ELSE 'WD Blue'
                END
            )
            WHEN 6 THEN CONCAT(
                'Fonte ',
                400 + MOD(numbers.product_number * 25, 850),
                'W ',
                CASE MOD(numbers.product_number, 3)
                    WHEN 0 THEN '80 Plus Bronze'
                    WHEN 1 THEN '80 Plus Gold'
                    ELSE '80 Plus White'
                END
            )
            WHEN 7 THEN CONCAT(
                'Gabinete Mid Tower ',
                CASE MOD(numbers.product_number, 4)
                    WHEN 0 THEN 'Aero'
                    WHEN 1 THEN 'Phantom'
                    WHEN 2 THEN 'Nova'
                    ELSE 'Stealth'
                END,
                ' ',
                100 + MOD(numbers.product_number * 9, 900)
            )
            WHEN 8 THEN CONCAT(
                'Water Cooler ',
                120 + (MOD(numbers.product_number, 3) * 120),
                'mm ',
                CASE MOD(numbers.product_number, 3)
                    WHEN 0 THEN 'Cooler Master'
                    WHEN 1 THEN 'Corsair'
                    ELSE 'DeepCool'
                END
            )
            WHEN 9 THEN CONCAT(
                'Monitor ',
                21 + MOD(numbers.product_number, 14),
                '" ',
                CASE MOD(numbers.product_number, 3)
                    WHEN 0 THEN 'Full HD'
                    WHEN 1 THEN 'QHD'
                    ELSE 'UltraWide'
                END
            )
            ELSE CONCAT(
                'Notebook ',
                CASE MOD(numbers.product_number, 4)
                    WHEN 0 THEN 'Lenovo IdeaPad'
                    WHEN 1 THEN 'Dell Inspiron'
                    WHEN 2 THEN 'Acer Nitro'
                    ELSE 'ASUS TUF'
                END,
                ' ',
                100 + MOD(numbers.product_number * 11, 900)
            )
        END AS name,
        CASE category.seq
            WHEN 1 THEN CONCAT('CPU para desktop com foco em desempenho, multitarefa e workloads de produtividade. Lote ', numbers.product_number, '.')
            WHEN 2 THEN CONCAT('GPU dedicada para jogos, renderizacao e aceleracao grafica. Lote ', numbers.product_number, '.')
            WHEN 3 THEN CONCAT('Placa mae com chipset moderno, suporte a DDR e armazenamento NVMe. Lote ', numbers.product_number, '.')
            WHEN 4 THEN CONCAT('Kit de memoria RAM para setups gamer e profissionais. Lote ', numbers.product_number, '.')
            WHEN 5 THEN CONCAT('Unidade de armazenamento para inicializacao rapida e carregamento de arquivos. Lote ', numbers.product_number, '.')
            WHEN 6 THEN CONCAT('Fonte de alimentacao para PCs gamer e workstations. Lote ', numbers.product_number, '.')
            WHEN 7 THEN CONCAT('Gabinete com fluxo de ar otimizado e espaco para placas longas. Lote ', numbers.product_number, '.')
            WHEN 8 THEN CONCAT('Solucao de refrigeracao liquida para processadores de alto desempenho. Lote ', numbers.product_number, '.')
            WHEN 9 THEN CONCAT('Monitor para produtividade, consumo multimidia e jogos. Lote ', numbers.product_number, '.')
            ELSE CONCAT('Notebook para trabalho, estudo e entretenimento. Lote ', numbers.product_number, '.')
        END AS description,
        CASE category.seq
            WHEN 1 THEN ROUND(650 + MOD(numbers.product_number * 19, 4200) + (MOD(numbers.product_number, 100) / 100), 2)
            WHEN 2 THEN ROUND(1400 + MOD(numbers.product_number * 29, 9800) + (MOD(numbers.product_number, 100) / 100), 2)
            WHEN 3 THEN ROUND(520 + MOD(numbers.product_number * 13, 2600) + (MOD(numbers.product_number, 100) / 100), 2)
            WHEN 4 THEN ROUND(180 + MOD(numbers.product_number * 7, 1200) + (MOD(numbers.product_number, 100) / 100), 2)
            WHEN 5 THEN ROUND(160 + MOD(numbers.product_number * 11, 2100) + (MOD(numbers.product_number, 100) / 100), 2)
            WHEN 6 THEN ROUND(230 + MOD(numbers.product_number * 17, 1600) + (MOD(numbers.product_number, 100) / 100), 2)
            WHEN 7 THEN ROUND(260 + MOD(numbers.product_number * 9, 1900) + (MOD(numbers.product_number, 100) / 100), 2)
            WHEN 8 THEN ROUND(280 + MOD(numbers.product_number * 15, 1500) + (MOD(numbers.product_number, 100) / 100), 2)
            WHEN 9 THEN ROUND(780 + MOD(numbers.product_number * 21, 4200) + (MOD(numbers.product_number, 100) / 100), 2)
            ELSE ROUND(2400 + MOD(numbers.product_number * 31, 14000) + (MOD(numbers.product_number, 100) / 100), 2)
        END AS price,
        5 + MOD(numbers.product_number * 3, 146) AS stock_quantity,
        1 AS active,
        category.id AS category_id,
        CASE category.seq
            WHEN 1 THEN ROUND(4.50 + (MOD(numbers.product_number, 8) * 0.40), 2)
            WHEN 2 THEN ROUND(11.00 + (MOD(numbers.product_number, 12) * 0.80), 2)
            WHEN 3 THEN ROUND(22.00 + (MOD(numbers.product_number, 8) * 0.70), 2)
            WHEN 4 THEN ROUND(1.50 + (MOD(numbers.product_number, 6) * 0.30), 2)
            WHEN 5 THEN ROUND(0.80 + (MOD(numbers.product_number, 4) * 0.20), 2)
            WHEN 6 THEN ROUND(8.00 + (MOD(numbers.product_number, 6) * 0.60), 2)
            WHEN 7 THEN ROUND(39.00 + (MOD(numbers.product_number, 10) * 1.40), 2)
            WHEN 8 THEN ROUND(5.00 + (MOD(numbers.product_number, 7) * 0.50), 2)
            WHEN 9 THEN ROUND(31.00 + (MOD(numbers.product_number, 6) * 1.10), 2)
            ELSE ROUND(2.20 + (MOD(numbers.product_number, 6) * 0.30), 2)
        END AS height_cm,
        CASE category.seq
            WHEN 1 THEN ROUND(4.50 + (MOD(numbers.product_number, 8) * 0.40), 2)
            WHEN 2 THEN ROUND(22.00 + (MOD(numbers.product_number, 10) * 1.10), 2)
            WHEN 3 THEN ROUND(22.00 + (MOD(numbers.product_number, 10) * 0.90), 2)
            WHEN 4 THEN ROUND(13.00 + (MOD(numbers.product_number, 4) * 0.60), 2)
            WHEN 5 THEN ROUND(8.00 + (MOD(numbers.product_number, 3) * 0.50), 2)
            WHEN 6 THEN ROUND(16.00 + (MOD(numbers.product_number, 6) * 0.80), 2)
            WHEN 7 THEN ROUND(21.00 + (MOD(numbers.product_number, 12) * 1.60), 2)
            WHEN 8 THEN ROUND(12.00 + (MOD(numbers.product_number, 5) * 0.70), 2)
            WHEN 9 THEN ROUND(52.00 + (MOD(numbers.product_number, 8) * 1.70), 2)
            ELSE ROUND(31.00 + (MOD(numbers.product_number, 8) * 0.90), 2)
        END AS width_cm,
        CASE category.seq
            WHEN 1 THEN ROUND(0.0009 + (MOD(numbers.product_number, 8) * 0.0001), 4)
            WHEN 2 THEN ROUND(0.0045 + (MOD(numbers.product_number, 12) * 0.0003), 4)
            WHEN 3 THEN ROUND(0.0034 + (MOD(numbers.product_number, 8) * 0.0002), 4)
            WHEN 4 THEN ROUND(0.0004 + (MOD(numbers.product_number, 6) * 0.0001), 4)
            WHEN 5 THEN ROUND(0.0002 + (MOD(numbers.product_number, 4) * 0.0001), 4)
            WHEN 6 THEN ROUND(0.0028 + (MOD(numbers.product_number, 5) * 0.0002), 4)
            WHEN 7 THEN ROUND(0.0280 + (MOD(numbers.product_number, 8) * 0.0015), 4)
            WHEN 8 THEN ROUND(0.0016 + (MOD(numbers.product_number, 5) * 0.0002), 4)
            WHEN 9 THEN ROUND(0.0340 + (MOD(numbers.product_number, 8) * 0.0020), 4)
            ELSE ROUND(0.0060 + (MOD(numbers.product_number, 5) * 0.0004), 4)
        END AS cubage_m3,
        CASE category.seq
            WHEN 1 THEN ROUND(0.250 + (MOD(numbers.product_number, 5) * 0.035), 3)
            WHEN 2 THEN ROUND(0.900 + (MOD(numbers.product_number, 8) * 0.120), 3)
            WHEN 3 THEN ROUND(0.700 + (MOD(numbers.product_number, 8) * 0.080), 3)
            WHEN 4 THEN ROUND(0.080 + (MOD(numbers.product_number, 6) * 0.020), 3)
            WHEN 5 THEN ROUND(0.050 + (MOD(numbers.product_number, 4) * 0.010), 3)
            WHEN 6 THEN ROUND(1.100 + (MOD(numbers.product_number, 8) * 0.150), 3)
            WHEN 7 THEN ROUND(4.500 + (MOD(numbers.product_number, 8) * 0.350), 3)
            WHEN 8 THEN ROUND(0.600 + (MOD(numbers.product_number, 6) * 0.070), 3)
            WHEN 9 THEN ROUND(3.100 + (MOD(numbers.product_number, 6) * 0.400), 3)
            ELSE ROUND(1.400 + (MOD(numbers.product_number, 8) * 0.180), 3)
        END AS weight_kg,
        CASE MOD(numbers.product_number, 6)
            WHEN 0 THEN '01095-000'
            WHEN 1 THEN '04538-132'
            WHEN 2 THEN '30130-010'
            WHEN 3 THEN '80010-000'
            WHEN 4 THEN '40015-010'
            ELSE '88010-400'
        END AS origin_zip_code
    FROM tmp_numbers numbers
    JOIN tmp_categories category
        ON category.seq = MOD(numbers.product_number - 1, 10) + 1;

    INSERT INTO `ecommerce-plataform-catalog-read`.products
    (
        Id,
        name,
        description,
        price,
        stock_quantity,
        active,
        category_id,
        height_cm,
        width_cm,
        cubage_m3,
        weight_kg,
        origin_zip_code
    )
    SELECT
        Id,
        name,
        description,
        price,
        stock_quantity,
        active,
        category_id,
        height_cm,
        width_cm,
        cubage_m3,
        weight_kg,
        origin_zip_code
    FROM `ecommerce-plataform-catalog-write`.products
    ON DUPLICATE KEY UPDATE
        name = VALUES(name),
        description = VALUES(description),
        price = VALUES(price),
        stock_quantity = VALUES(stock_quantity),
        active = VALUES(active),
        category_id = VALUES(category_id),
        height_cm = VALUES(height_cm),
        width_cm = VALUES(width_cm),
        cubage_m3 = VALUES(cubage_m3),
        weight_kg = VALUES(weight_kg),
        origin_zip_code = VALUES(origin_zip_code);

    SELECT 'catalog_write.categories' AS table_name, COUNT(*) AS total_rows
    FROM `ecommerce-plataform-catalog-write`.categories
    UNION ALL
    SELECT 'catalog_write.products', COUNT(*)
    FROM `ecommerce-plataform-catalog-write`.products
    UNION ALL
    SELECT 'catalog_read.categories', COUNT(*)
    FROM `ecommerce-plataform-catalog-read`.categories
    UNION ALL
    SELECT 'catalog_read.products', COUNT(*)
    FROM `ecommerce-plataform-catalog-read`.products;

    DROP TEMPORARY TABLE IF EXISTS tmp_numbers;
    DROP TEMPORARY TABLE IF EXISTS tmp_categories;
END$$

DELIMITER ;

CALL seed_catalog_massive_load();
DROP PROCEDURE IF EXISTS seed_catalog_massive_load;
