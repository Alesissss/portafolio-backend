-- 1. Insertar el rol de soporte/infraestructura (Superadmin)
INSERT INTO rol (nombre, estado) 
VALUES ('Superadmin', TRUE);

-- 2. Insertar el rol para el cliente/dueño del negocio
INSERT INTO rol (nombre, estado) 
VALUES ('Administrador', TRUE);

-- 3. Insertar el rol operativo
INSERT INTO rol (nombre, estado) 
VALUES ('Vendedor', TRUE);


INSERT INTO usuario (
    id_rol, apellido_paterno, apellido_materno, nombres, correo, username, password_hash, estado, estado_registro
) 
SELECT 
    (SELECT id_rol FROM rol WHERE nombre = 'Superadmin' LIMIT 1),
    'Torres', 'Cabrejos', 'Jorge Alexis', 'alexistorrescabrejos@outlook.com', 'atorres', 
    crypt('AlexisD3v$', gen_salt('bf', 11)), 
    TRUE, TRUE
WHERE NOT EXISTS (
    SELECT 1 FROM usuario WHERE username = 'atorres' AND estado_registro = TRUE
);
