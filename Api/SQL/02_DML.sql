-- 1. Insertar el rol de soporte/infraestructura (Superadmin)
INSERT INTO rol (nombre, estado) 
VALUES ('Superadmin', TRUE);

-- 2. Insertar el rol para el cliente/dueño del negocio
INSERT INTO rol (nombre, estado) 
VALUES ('Administrador', TRUE);

-- 3. Insertar el rol operativo
INSERT INTO rol (nombre, estado) 
VALUES ('Vendedor', TRUE);


