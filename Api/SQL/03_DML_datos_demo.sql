-- ============================================================================
-- 03_DML_datos_demo.sql — Datos de prueba para el módulo de Ventas
-- ----------------------------------------------------------------------------
-- Ejecutar DESPUÉS de:  01_DDL.sql  y  02_DML.sql
--   (02 crea los roles Superadmin/Administrador/Vendedor y los estado_venta BO/GEN/PAG/AN).
--
-- Contraseña de TODOS los vendedores: Vendedor123!
--   Se hashea con pgcrypto (bcrypt, mismo patrón que el superadmin de 02_DML) para que
--   BCrypt.Net.Verify del backend lo acepte al hacer login.
--
-- Convención de montos: subtotal = Σ(precio_venta × cantidad); IGV = 18% del subtotal;
--   total = subtotal + IGV. (Los montos van pre-calculados a mano en cada venta.)
-- ============================================================================


-- ─────────────────────────────────────────────────────────────────────────────
-- 1) CATEGORÍAS
-- ─────────────────────────────────────────────────────────────────────────────
INSERT INTO categoria (id_categoria, nombre, descripcion, estado) VALUES
('BEB', 'Bebidas',   'Gaseosas, aguas y refrescos',      TRUE),
('SNK', 'Snacks',    'Piqueos, galletas y golosinas',    TRUE),
('LAC', 'Lacteos',   'Leche, yogurt y derivados',        TRUE),
('LIM', 'Limpieza',  'Productos de limpieza del hogar',  TRUE),
('ABA', 'Abarrotes', 'Productos de primera necesidad',   TRUE)
ON CONFLICT (id_categoria) DO NOTHING;


-- ─────────────────────────────────────────────────────────────────────────────
-- 2) PRODUCTOS  (id_producto es SERIAL: se autogenera; los enlazamos por nombre)
-- ─────────────────────────────────────────────────────────────────────────────
INSERT INTO producto (nombre, descripcion, stock, precio, estado, id_categoria) VALUES
('Agua San Luis 625ml',    'Agua sin gas 625ml',        200, 1.50, TRUE, 'BEB'),
('Coca Cola 500ml',        'Gaseosa 500ml',             150, 2.50, TRUE, 'BEB'),
('Inca Kola 500ml',        'Gaseosa 500ml',             150, 2.50, TRUE, 'BEB'),
('Papas Lays Clasicas',    'Papas fritas 40g',          100, 3.00, TRUE, 'SNK'),
('Chocolate Sublime',      'Chocolate con mani',        120, 1.20, TRUE, 'SNK'),
('Galleta Oreo',           'Galleta rellena 36g',        90, 1.50, TRUE, 'SNK'),
('Leche Gloria Tarro',     'Leche evaporada 400g',       80, 4.20, TRUE, 'LAC'),
('Yogurt Gloria 1L',       'Yogurt bebible fresa 1L',    60, 6.50, TRUE, 'LAC'),
('Detergente Bolivar 1kg', 'Detergente en polvo 1kg',    40, 8.90, TRUE, 'LIM'),
('Arroz Costeno 1kg',      'Arroz superior 1kg',        100, 4.50, TRUE, 'ABA'),
('Aceite Primor 1L',       'Aceite vegetal 1L',          50, 9.80, TRUE, 'ABA'),
('Azucar Rubia 1kg',       'Azucar rubia 1kg',           70, 4.00, TRUE, 'ABA');


-- ─────────────────────────────────────────────────────────────────────────────
-- 3) VENDEDORES  (rol 'Vendedor', password 'Vendedor123!')
--    Mismo patrón que el superadmin de 02_DML: crypt + gen_salt('bf', 11) y guard
--    NOT EXISTS para no duplicar si se re-ejecuta.
-- ─────────────────────────────────────────────────────────────────────────────
INSERT INTO usuario (id_rol, apellido_paterno, apellido_materno, nombres, correo, username, password_hash, estado, estado_registro)
SELECT (SELECT id_rol FROM rol WHERE nombre = 'Vendedor' LIMIT 1),
       'Perez', 'Garcia', 'Juan Carlos', 'jperez@demo.com', 'jperez',
       crypt('Vendedor123!', gen_salt('bf', 11)), TRUE, TRUE
WHERE NOT EXISTS (SELECT 1 FROM usuario WHERE username = 'jperez' AND estado_registro = TRUE);

INSERT INTO usuario (id_rol, apellido_paterno, apellido_materno, nombres, correo, username, password_hash, estado, estado_registro)
SELECT (SELECT id_rol FROM rol WHERE nombre = 'Vendedor' LIMIT 1),
       'Rodriguez', 'Ramos', 'Maria Elena', 'mrodriguez@demo.com', 'mrodriguez',
       crypt('Vendedor123!', gen_salt('bf', 11)), TRUE, TRUE
WHERE NOT EXISTS (SELECT 1 FROM usuario WHERE username = 'mrodriguez' AND estado_registro = TRUE);

INSERT INTO usuario (id_rol, apellido_paterno, apellido_materno, nombres, correo, username, password_hash, estado, estado_registro)
SELECT (SELECT id_rol FROM rol WHERE nombre = 'Vendedor' LIMIT 1),
       'Vargas', 'Diaz', 'Carlos Antonio', 'cvargas@demo.com', 'cvargas',
       crypt('Vendedor123!', gen_salt('bf', 11)), TRUE, TRUE
WHERE NOT EXISTS (SELECT 1 FROM usuario WHERE username = 'cvargas' AND estado_registro = TRUE);


-- ─────────────────────────────────────────────────────────────────────────────
-- 4) VENTAS + DETALLES
--    id_venta con UUID explícito (para enlazar sus detalles). id_producto se
--    resuelve por subconsulta sobre el nombre (robusto ante el SERIAL).
--    Estados: 2 en BORRADOR (BO), 2 en GENERADA (GEN), 1 en ANULADA (AN).
-- ─────────────────────────────────────────────────────────────────────────────

-- ── Venta 1: BORRADOR — jperez ── subtotal 10.50 · IGV 1.89 · total 12.39
INSERT INTO venta (id_venta, fecha_emision, subtotal, igv, total, id_vendedor, id_estado_venta) VALUES
('11111111-1111-4111-8111-111111111111', CURRENT_TIMESTAMP - INTERVAL '5 days',
 10.50, 1.89, 12.39, (SELECT id_usuario FROM usuario WHERE username = 'jperez' LIMIT 1), 'BO')
ON CONFLICT (id_venta) DO NOTHING;

INSERT INTO detalle_venta (id_venta, id_producto, precio_venta, cantidad, observacion) VALUES
('11111111-1111-4111-8111-111111111111', (SELECT id_producto FROM producto WHERE nombre = 'Agua San Luis 625ml'), 1.50, 3, NULL),
('11111111-1111-4111-8111-111111111111', (SELECT id_producto FROM producto WHERE nombre = 'Papas Lays Clasicas'), 3.00, 2, 'Cliente frecuente')
ON CONFLICT (id_venta, id_producto) DO NOTHING;

-- ── Venta 2: BORRADOR — mrodriguez ── subtotal 25.80 · IGV 4.64 · total 30.44
INSERT INTO venta (id_venta, fecha_emision, subtotal, igv, total, id_vendedor, id_estado_venta) VALUES
('22222222-2222-4222-8222-222222222222', CURRENT_TIMESTAMP - INTERVAL '3 days',
 25.80, 4.64, 30.44, (SELECT id_usuario FROM usuario WHERE username = 'mrodriguez' LIMIT 1), 'BO')
ON CONFLICT (id_venta) DO NOTHING;

INSERT INTO detalle_venta (id_venta, id_producto, precio_venta, cantidad, observacion) VALUES
('22222222-2222-4222-8222-222222222222', (SELECT id_producto FROM producto WHERE nombre = 'Leche Gloria Tarro'), 4.20, 4, NULL),
('22222222-2222-4222-8222-222222222222', (SELECT id_producto FROM producto WHERE nombre = 'Arroz Costeno 1kg'),   4.50, 2, NULL)
ON CONFLICT (id_venta, id_producto) DO NOTHING;

-- ── Venta 3: GENERADA — jperez ── subtotal 34.50 · IGV 6.21 · total 40.71
INSERT INTO venta (id_venta, fecha_emision, subtotal, igv, total, id_vendedor, id_estado_venta) VALUES
('33333333-3333-4333-8333-333333333333', CURRENT_TIMESTAMP - INTERVAL '2 days',
 34.50, 6.21, 40.71, (SELECT id_usuario FROM usuario WHERE username = 'jperez' LIMIT 1), 'GEN')
ON CONFLICT (id_venta) DO NOTHING;

INSERT INTO detalle_venta (id_venta, id_producto, precio_venta, cantidad, observacion) VALUES
('33333333-3333-4333-8333-333333333333', (SELECT id_producto FROM producto WHERE nombre = 'Coca Cola 500ml'),   2.50, 6, NULL),
('33333333-3333-4333-8333-333333333333', (SELECT id_producto FROM producto WHERE nombre = 'Chocolate Sublime'), 1.20, 10, NULL),
('33333333-3333-4333-8333-333333333333', (SELECT id_producto FROM producto WHERE nombre = 'Galleta Oreo'),      1.50, 5, NULL)
ON CONFLICT (id_venta, id_producto) DO NOTHING;

-- ── Venta 4: GENERADA — cvargas ── subtotal 40.50 · IGV 7.29 · total 47.79
INSERT INTO venta (id_venta, fecha_emision, subtotal, igv, total, id_vendedor, id_estado_venta) VALUES
('44444444-4444-4444-8444-444444444444', CURRENT_TIMESTAMP - INTERVAL '1 day',
 40.50, 7.29, 47.79, (SELECT id_usuario FROM usuario WHERE username = 'cvargas' LIMIT 1), 'GEN')
ON CONFLICT (id_venta) DO NOTHING;

INSERT INTO detalle_venta (id_venta, id_producto, precio_venta, cantidad, observacion) VALUES
('44444444-4444-4444-8444-444444444444', (SELECT id_producto FROM producto WHERE nombre = 'Aceite Primor 1L'),       9.80, 2, NULL),
('44444444-4444-4444-8444-444444444444', (SELECT id_producto FROM producto WHERE nombre = 'Azucar Rubia 1kg'),       4.00, 3, NULL),
('44444444-4444-4444-8444-444444444444', (SELECT id_producto FROM producto WHERE nombre = 'Detergente Bolivar 1kg'), 8.90, 1, 'Pago pendiente')
ON CONFLICT (id_venta, id_producto) DO NOTHING;

-- ── Venta 5: ANULADA — mrodriguez ── subtotal 23.00 · IGV 4.14 · total 27.14
INSERT INTO venta (id_venta, fecha_emision, subtotal, igv, total, id_vendedor, id_estado_venta) VALUES
('55555555-5555-4555-8555-555555555555', CURRENT_TIMESTAMP - INTERVAL '6 days',
 23.00, 4.14, 27.14, (SELECT id_usuario FROM usuario WHERE username = 'mrodriguez' LIMIT 1), 'AN')
ON CONFLICT (id_venta) DO NOTHING;

INSERT INTO detalle_venta (id_venta, id_producto, precio_venta, cantidad, observacion) VALUES
('55555555-5555-4555-8555-555555555555', (SELECT id_producto FROM producto WHERE nombre = 'Yogurt Gloria 1L'), 6.50, 2, NULL),
('55555555-5555-4555-8555-555555555555', (SELECT id_producto FROM producto WHERE nombre = 'Inca Kola 500ml'),  2.50, 4, 'Anulada: cliente desistio')
ON CONFLICT (id_venta, id_producto) DO NOTHING;
