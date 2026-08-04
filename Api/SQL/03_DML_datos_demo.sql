-- ============================================================================
-- 03_DML_datos_demo.sql — Datos de prueba para Ventas y Reportes
-- ----------------------------------------------------------------------------
-- Ejecutar DESPUÉS de:  01_DDL.sql  y  02_DML.sql
--   (02 crea los roles Superadmin/Administrador/Vendedor y los estado_venta BO/GEN/PAG/AN).
--
-- Contraseña de TODOS los vendedores: Vendedor123!
--   Se hashea con pgcrypto (bcrypt, mismo patrón que el superadmin de 02_DML) para que
--   BCrypt.Net.Verify del backend lo acepte al hacer login.
--
-- Convención de montos: subtotal = Σ(precio_venta × cantidad); IGV = 18% del subtotal;
--   total = subtotal + IGV.
--
-- Este script tiene DOS partes:
--   A) 5 ventas escritas a mano con UUID fijo (11111..., 22222...) para probar el flujo
--      BO → GEN → PAG / AN siempre contra los mismos ids.
--   B) ~3 AÑOS de ventas GENERADAS con generate_series, para que los gráficos de Reportes
--      tengan tendencia, estacionalidad y volumen. Se generan en vez de escribirse a mano
--      porque son miles de filas.
--
-- La parte B es idempotente: si ya hay 100+ ventas, no vuelve a generar.
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


-- ═════════════════════════════════════════════════════════════════════════════
-- PARTE A — 5 VENTAS FIJAS (para el smoke test del flujo de estados)
--   id_venta con UUID explícito para poder enlazar sus detalles y volver siempre a
--   las mismas. Estados: 2 BORRADOR, 2 GENERADA, 1 ANULADA.
--   Los montos NO se escriben aquí: los recalcula la Parte C desde el detalle.
-- ═════════════════════════════════════════════════════════════════════════════

-- ── Venta 1: BORRADOR — jperez
INSERT INTO venta (id_venta, fecha_emision, subtotal, igv, total, id_vendedor, id_estado_venta) VALUES
('11111111-1111-4111-8111-111111111111', CURRENT_TIMESTAMP - INTERVAL '5 days',
 0, 0, 0, (SELECT id_usuario FROM usuario WHERE username = 'jperez' LIMIT 1), 'BO')
ON CONFLICT (id_venta) DO NOTHING;

INSERT INTO detalle_venta (id_venta, id_producto, precio_venta, cantidad, observacion) VALUES
('11111111-1111-4111-8111-111111111111', (SELECT id_producto FROM producto WHERE nombre = 'Agua San Luis 625ml'), 1.50, 3, NULL),
('11111111-1111-4111-8111-111111111111', (SELECT id_producto FROM producto WHERE nombre = 'Papas Lays Clasicas'), 3.00, 2, 'Cliente frecuente')
ON CONFLICT (id_venta, id_producto) DO NOTHING;

-- ── Venta 2: BORRADOR — mrodriguez
INSERT INTO venta (id_venta, fecha_emision, subtotal, igv, total, id_vendedor, id_estado_venta) VALUES
('22222222-2222-4222-8222-222222222222', CURRENT_TIMESTAMP - INTERVAL '3 days',
 0, 0, 0, (SELECT id_usuario FROM usuario WHERE username = 'mrodriguez' LIMIT 1), 'BO')
ON CONFLICT (id_venta) DO NOTHING;

INSERT INTO detalle_venta (id_venta, id_producto, precio_venta, cantidad, observacion) VALUES
('22222222-2222-4222-8222-222222222222', (SELECT id_producto FROM producto WHERE nombre = 'Leche Gloria Tarro'), 4.20, 4, NULL),
('22222222-2222-4222-8222-222222222222', (SELECT id_producto FROM producto WHERE nombre = 'Arroz Costeno 1kg'),   4.50, 2, NULL)
ON CONFLICT (id_venta, id_producto) DO NOTHING;

-- ── Venta 3: GENERADA — jperez
INSERT INTO venta (id_venta, fecha_emision, subtotal, igv, total, id_vendedor, id_estado_venta) VALUES
('33333333-3333-4333-8333-333333333333', CURRENT_TIMESTAMP - INTERVAL '2 days',
 0, 0, 0, (SELECT id_usuario FROM usuario WHERE username = 'jperez' LIMIT 1), 'GEN')
ON CONFLICT (id_venta) DO NOTHING;

INSERT INTO detalle_venta (id_venta, id_producto, precio_venta, cantidad, observacion) VALUES
('33333333-3333-4333-8333-333333333333', (SELECT id_producto FROM producto WHERE nombre = 'Coca Cola 500ml'),   2.50, 6, NULL),
('33333333-3333-4333-8333-333333333333', (SELECT id_producto FROM producto WHERE nombre = 'Chocolate Sublime'), 1.20, 10, NULL),
('33333333-3333-4333-8333-333333333333', (SELECT id_producto FROM producto WHERE nombre = 'Galleta Oreo'),      1.50, 5, NULL)
ON CONFLICT (id_venta, id_producto) DO NOTHING;

-- ── Venta 4: GENERADA — cvargas
INSERT INTO venta (id_venta, fecha_emision, subtotal, igv, total, id_vendedor, id_estado_venta) VALUES
('44444444-4444-4444-8444-444444444444', CURRENT_TIMESTAMP - INTERVAL '1 day',
 0, 0, 0, (SELECT id_usuario FROM usuario WHERE username = 'cvargas' LIMIT 1), 'GEN')
ON CONFLICT (id_venta) DO NOTHING;

INSERT INTO detalle_venta (id_venta, id_producto, precio_venta, cantidad, observacion) VALUES
('44444444-4444-4444-8444-444444444444', (SELECT id_producto FROM producto WHERE nombre = 'Aceite Primor 1L'),       9.80, 2, NULL),
('44444444-4444-4444-8444-444444444444', (SELECT id_producto FROM producto WHERE nombre = 'Azucar Rubia 1kg'),       4.00, 3, NULL),
('44444444-4444-4444-8444-444444444444', (SELECT id_producto FROM producto WHERE nombre = 'Detergente Bolivar 1kg'), 8.90, 1, 'Pago pendiente')
ON CONFLICT (id_venta, id_producto) DO NOTHING;

-- ── Venta 5: ANULADA — mrodriguez
INSERT INTO venta (id_venta, fecha_emision, subtotal, igv, total, id_vendedor, id_estado_venta) VALUES
('55555555-5555-4555-8555-555555555555', CURRENT_TIMESTAMP - INTERVAL '6 days',
 0, 0, 0, (SELECT id_usuario FROM usuario WHERE username = 'mrodriguez' LIMIT 1), 'AN')
ON CONFLICT (id_venta) DO NOTHING;

INSERT INTO detalle_venta (id_venta, id_producto, precio_venta, cantidad, observacion) VALUES
('55555555-5555-4555-8555-555555555555', (SELECT id_producto FROM producto WHERE nombre = 'Yogurt Gloria 1L'), 6.50, 2, NULL),
('55555555-5555-4555-8555-555555555555', (SELECT id_producto FROM producto WHERE nombre = 'Inca Kola 500ml'),  2.50, 4, 'Anulada: cliente desistio')
ON CONFLICT (id_venta, id_producto) DO NOTHING;


-- ═════════════════════════════════════════════════════════════════════════════
-- PARTE B — ~3 AÑOS DE VENTAS GENERADAS
-- ─────────────────────────────────────────────────────────────────────────────
-- Por qué generadas y no escritas: son miles de filas, y lo que interesa del dato es
-- su FORMA (tendencia, estacionalidad, mezcla de estados), no cada venta concreta.
--
-- Cómo se le da forma:
--   · Día de semana: sábado vende más, domingo casi nada.
--   · Mes: diciembre es el pico (campaña), febrero el valle; enero y julio suben algo.
--   · Antigüedad: lo viejo ya está PAGADO (o anulado); lo reciente todavía tiene
--     borradores y generadas sin cobrar. Así "Por cobrar" del dashboard da un número
--     con sentido en vez de cero.
--
-- setseed hace que la aleatoriedad sea REPRODUCIBLE: dos personas que corran este
-- script obtienen exactamente los mismos datos, que es lo que uno quiere de un seed.
-- ═════════════════════════════════════════════════════════════════════════════
DO $$
DECLARE
    v_ventas   bigint;
    v_detalles bigint;
BEGIN
    IF (SELECT COUNT(*) FROM venta) >= 100 THEN
        RAISE NOTICE 'Ya existen % ventas: se omite la generacion masiva.', (SELECT COUNT(*) FROM venta);
        RETURN;
    END IF;

    PERFORM setseed(0.4242);

    -- 1) Cabeceras. Los montos van en 0 a proposito: los calcula la Parte C desde el
    --    detalle, para que cabecera y detalle NO puedan contradecirse.
    WITH dias AS (
        SELECT d::date                    AS dia,
               EXTRACT(DOW   FROM d)::int AS dow,
               EXTRACT(MONTH FROM d)::int AS mes
        FROM generate_series(
                 CURRENT_DATE - INTERVAL '3 years',
                 CURRENT_DATE,
                 INTERVAL '1 day') AS d
    ),
    plan AS (
        SELECT dia,
               GREATEST(0, ROUND(
                     (CASE WHEN dow = 0 THEN 0.6      -- domingo
                           WHEN dow = 6 THEN 2.9      -- sabado
                           ELSE 2.0 END)
                   * (CASE WHEN mes = 12       THEN 1.9   -- campaña navideña
                           WHEN mes IN (1, 7)  THEN 1.3   -- fiestas patrias / verano
                           WHEN mes = 2        THEN 0.7   -- valle
                           ELSE 1.0 END)
                   * (0.4 + random() * 1.2)
               ))::int AS cuantas
        FROM dias
    )
    INSERT INTO venta (id_venta, fecha_emision, subtotal, igv, total, id_vendedor, id_estado_venta, estado_registro, fecha_registro)
    SELECT gen_random_uuid(),
           m.momento,
           0, 0, 0,
           v.id_usuario,
           CASE
               WHEN p.dia < CURRENT_DATE - 60 THEN
                    CASE WHEN random() < 0.05 THEN 'AN' ELSE 'PAG' END
               WHEN p.dia < CURRENT_DATE - 15 THEN
                    CASE WHEN random() < 0.07 THEN 'AN'
                         WHEN random() < 0.30 THEN 'GEN'
                         ELSE 'PAG' END
               ELSE
                    CASE WHEN random() < 0.12 THEN 'BO'
                         WHEN random() < 0.45 THEN 'GEN'
                         ELSE 'PAG' END
           END,
           TRUE,
           m.momento
    FROM plan p
    -- generate_series(1, 0) no devuelve filas: los dias sin ventas simplemente no generan.
    CROSS JOIN LATERAL generate_series(1, p.cuantas) AS g(i)
    -- OJO: el ORDER BY tiene que depender de la fila externa (aqui p.dia y g.i). Un LATERAL
    -- que no referencia nada de afuera NO esta correlacionado y Postgres puede evaluarlo una
    -- sola vez y reusar el resultado: todas las ventas saldrian con el mismo vendedor.
    CROSS JOIN LATERAL (
        SELECT id_usuario FROM usuario
        WHERE username IN ('jperez', 'mrodriguez', 'cvargas')
        ORDER BY md5(p.dia::text || g.i::text || id_usuario::text)
        LIMIT 1
    ) v
    -- Horario comercial: entre las 08:00 y las 19:00.
    CROSS JOIN LATERAL (
        SELECT (p.dia + INTERVAL '8 hour' + random() * INTERVAL '11 hour')::timestamptz AS momento
    ) m;

    -- 2) Detalles. Solo para las ventas que aun no tienen ninguno, es decir las recien
    --    generadas (las 5 fijas de la Parte A ya traen los suyos).
    --    El LATERAL toma 5 productos DISTINTOS al azar y luego descarta algunos; rn = 1
    --    se conserva siempre, asi ninguna venta queda sin items.
    INSERT INTO detalle_venta (id_venta, id_producto, precio_venta, cantidad, observacion)
    SELECT ve.id_venta,
           pr.id_producto,
           -- Precio con +-10% sobre el de lista: en una venta real el precio se congela
           -- al momento de vender, no se lee del producto.
           -- El ::numeric NO es adorno: random() devuelve double precision, contagia a toda
           -- la expresion, y round(double precision, int) NO existe en PostgreSQL (solo
           -- round(numeric, int) y round(double) de un solo argumento).
           ROUND((pr.precio * (0.90 + random() * 0.20))::numeric, 2),
           (1 + FLOOR(random() * 8))::numeric,
           NULL
    FROM venta ve
    -- Mismo cuidado que arriba: el orden depende de ve.id_venta, asi cada venta recibe una
    -- combinacion distinta de productos. Con ORDER BY random() a secas, el LATERAL no queda
    -- correlacionado y las 3000 ventas compartirian los mismos 5 productos.
    CROSS JOIN LATERAL (
        SELECT id_producto, precio, ROW_NUMBER() OVER () AS rn
        FROM (
            SELECT id_producto, precio
            FROM producto
            ORDER BY md5(ve.id_venta::text || id_producto::text)
            LIMIT 5
        ) s
    ) pr
    WHERE NOT EXISTS (SELECT 1 FROM detalle_venta d WHERE d.id_venta = ve.id_venta)
      AND (pr.rn = 1 OR random() < 0.45)
    ON CONFLICT (id_venta, id_producto) DO NOTHING;

    SELECT COUNT(*) INTO v_ventas   FROM venta;
    SELECT COUNT(*) INTO v_detalles FROM detalle_venta;
    RAISE NOTICE 'Generadas % ventas con % lineas de detalle.', v_ventas, v_detalles;
END $$;


-- ═════════════════════════════════════════════════════════════════════════════
-- PARTE C — CONSISTENCIA
-- ═════════════════════════════════════════════════════════════════════════════

-- Los montos de la cabecera SIEMPRE se derivan del detalle (incluidas las 5 fijas):
-- una cabecera que no cuadra con sus lineas es el bug clasico de un seed escrito a mano.
UPDATE venta v
SET subtotal = t.sub,
    igv      = ROUND(t.sub * 0.18, 2),
    total    = ROUND(t.sub * 1.18, 2)
FROM (
    SELECT id_venta, SUM(precio_venta * cantidad) AS sub
    FROM detalle_venta
    GROUP BY id_venta
) t
WHERE v.id_venta = t.id_venta;

-- Red de seguridad: una venta sin lineas no deberia existir.
DELETE FROM venta v
WHERE NOT EXISTS (SELECT 1 FROM detalle_venta d WHERE d.id_venta = v.id_venta);

-- El stock es LO QUE QUEDA HOY, no el histórico: no tendría sentido dejar el valor
-- inicial despues de 3 años de ventas. Algunos quedan bajos a proposito, para que el
-- reporte de stock critico tenga algo que mostrar.
UPDATE producto
SET stock = (5 + FLOOR(random() * 400))::numeric;
