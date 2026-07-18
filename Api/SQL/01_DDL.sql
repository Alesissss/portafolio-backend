CREATE EXTENSION IF NOT EXISTS pgcrypto;

DROP TABLE IF EXISTS detalle_venta;
DROP TABLE IF EXISTS venta;
DROP TABLE IF EXISTS estado_venta;
DROP TABLE IF EXISTS producto;
DROP TABLE IF EXISTS categoria;
DROP TABLE IF EXISTS permiso_rol;
DROP TABLE IF EXISTS usuario;
DROP TABLE IF EXISTS rol;
DROP TABLE IF EXISTS permiso;
DROP TABLE IF EXISTS auditoria_log;

CREATE TABLE permiso (
	id_permiso CHAR(15) PRIMARY KEY, --MOD_VENTAS, MOD_MAESTROS
	nombre VARCHAR(30) NOT NULL,
	descripcion VARCHAR(255) NULL,
	orden SMALLINT NOT NULL,
	-- campos de auditoría
    estado_registro  BOOLEAN         NOT NULL DEFAULT TRUE,
    usuario_registro UUID            NULL,
    fecha_registro   TIMESTAMPTZ     NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE rol (
	id_rol UUID PRIMARY KEY DEFAULT gen_random_uuid(),
	nombre VARCHAR(30) NOT NULL,
	estado BOOLEAN NOT NULL DEFAULT TRUE, -- dar de baja
	-- campos de auditoría
    estado_registro  BOOLEAN         NOT NULL DEFAULT TRUE,
    usuario_registro UUID            NULL,
    fecha_registro   TIMESTAMPTZ     NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Índice parcial para el nombre del rol (La cláusula UNIQUE no es suficiente para garantizar la unicidad de los nombres de rol activos)
CREATE UNIQUE INDEX idx_rol_nombre_unico_activo 
ON rol (nombre) 
WHERE (estado_registro = TRUE);

CREATE TABLE permiso_rol(
	id_permiso CHAR(15) NOT NULL,
	id_rol UUID NOT NULL,
	
	-- campos de auditoría
    estado_registro  BOOLEAN         NOT NULL DEFAULT TRUE,
    usuario_registro UUID            NULL,
    fecha_registro   TIMESTAMPTZ     NOT NULL DEFAULT CURRENT_TIMESTAMP,
	-- constraints
	CONSTRAINT pk_permiso_rol PRIMARY KEY (id_permiso, id_rol),
	CONSTRAINT fk_permiso FOREIGN KEY (id_permiso) REFERENCES permiso (id_permiso),
	CONSTRAINT fk_rol FOREIGN KEY (id_rol) REFERENCES rol (id_rol)
);

CREATE TABLE usuario (
	id_usuario UUID PRIMARY KEY DEFAULT gen_random_uuid(),
	id_rol UUID NOT NULL, 
	apellido_paterno VARCHAR(30) NOT NULL,
	apellido_materno VARCHAR(30) NOT NULL,
	nombres VARCHAR (30) NOT NULL,
	correo VARCHAR (255) NOT NULL,
	username VARCHAR (255) NOT NULL,
	password_hash VARCHAR(255) NOT NULL,
	estado BOOLEAN NOT NULL DEFAULT TRUE, -- para dar de baja
	-- campos de auditoría
    estado_registro  BOOLEAN         NOT NULL DEFAULT TRUE,
    usuario_registro UUID            NULL,
    fecha_registro   TIMESTAMPTZ     NOT NULL DEFAULT CURRENT_TIMESTAMP,

	-- constraints
	CONSTRAINT fk_usuario_rol FOREIGN KEY (id_rol) REFERENCES rol(id_rol)
);

-- Índice parcial para el username (La cláusula UNIQUE no es suficiente para garantizar la unicidad de los usernames activos)
CREATE UNIQUE INDEX idx_usuario_username_unico_activo 
ON usuario (username) 
WHERE (estado_registro = TRUE);

CREATE TABLE categoria (
	id_categoria CHAR(3) PRIMARY KEY,
	nombre VARCHAR(30) NOT NULL,
	descripcion VARCHAR(255) NULL,
	estado BOOLEAN DEFAULT TRUE, -- para dar de baja
	-- campos de auditoría
    estado_registro  BOOLEAN         NOT NULL DEFAULT TRUE,
    usuario_registro UUID            NULL,
    fecha_registro   TIMESTAMPTZ     NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Índice parcial para el nombre (La cláusula UNIQUE no es suficiente para garantizar la unicidad de los nombres activos)
CREATE UNIQUE INDEX idx_categoria_nombre_unico_activo 
ON categoria (nombre) 
WHERE (estado_registro = TRUE);

CREATE TABLE producto (
	id_producto SERIAL PRIMARY KEY,
	nombre VARCHAR(50) NOT NULL,
	descripcion VARCHAR(50) NOT NULL,
	stock NUMERIC(19,2) NOT NULL,
	precio NUMERIC(19,2) NOT NULL,
	estado BOOLEAN DEFAULT TRUE, -- para dar de baja
	id_categoria CHAR(3) NOT NULL,
	-- campos de auditoría
    estado_registro  BOOLEAN         NOT NULL DEFAULT TRUE,
    usuario_registro UUID            NULL,
    fecha_registro   TIMESTAMPTZ     NOT NULL DEFAULT CURRENT_TIMESTAMP,
	-- constraints
	CONSTRAINT fk_producto_categoria FOREIGN KEY (id_categoria) REFERENCES categoria(id_categoria)
);

-- Índice parcial para el nombre (La cláusula UNIQUE no es suficiente para garantizar la unicidad de los nombres activos)
CREATE UNIQUE INDEX idx_producto_nombre_unico_activo 
ON producto (nombre) 
WHERE (estado_registro = TRUE);

CREATE TABLE estado_venta (
	id_estado_venta CHAR(3) PRIMARY KEY,
	nombre VARCHAR(30) NOT NULL,
	descripcion VARCHAR(30) NOT NULL
);

CREATE TABLE venta (
	id_venta UUID PRIMARY KEY DEFAULT gen_random_uuid(),
	fecha_emision TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
	subtotal NUMERIC(19,2) NOT NULL,
	igv NUMERIC(19,2) NOT NULL,
	total NUMERIC(19,2) NOT NULL,
	id_vendedor UUID NOT NULL, 
	id_estado_venta CHAR(3) NOT NULL, --BO (BORRADOR) --GEN (GENERADA) --PAG(PAGADA) --AN(ANULADA)
	-- campos de auditoría
    estado_registro  BOOLEAN         NOT NULL DEFAULT TRUE,
    usuario_registro UUID            NULL,
    fecha_registro   TIMESTAMPTZ     NOT NULL DEFAULT CURRENT_TIMESTAMP,
	-- constraints
	CONSTRAINT fk_usuario_vendedor FOREIGN KEY (id_vendedor) REFERENCES usuario(id_usuario),
	CONSTRAINT fk_estado_venta FOREIGN KEY (id_estado_venta) REFERENCES estado_venta(id_estado_venta)
);

CREATE TABLE detalle_venta (
	id_venta UUID NOT NULL,
	id_producto INT NOT NULL,
	precio_venta NUMERIC(9,2) NOT NULL,
	cantidad NUMERIC(9,2) NOT NULL,
	observacion VARCHAR(255) NULL,
	-- campos de auditoría
    estado_registro  BOOLEAN         NOT NULL DEFAULT TRUE,
    usuario_registro UUID            NULL,
    fecha_registro   TIMESTAMPTZ     NOT NULL DEFAULT CURRENT_TIMESTAMP,
	-- constraints
	CONSTRAINT pk_detalle_venta PRIMARY KEY (id_venta, id_producto),
	CONSTRAINT fk_venta FOREIGN KEY (id_venta) REFERENCES venta(id_venta),
	CONSTRAINT fk_producto FOREIGN KEY (id_producto) REFERENCES producto(id_producto)
);

CREATE TABLE auditoria_log (
    id_log              BIGSERIAL       PRIMARY KEY,
    nombre_tabla        VARCHAR(100)    NOT NULL,
    operacion           VARCHAR(10)     NOT NULL CHECK (operacion IN ('INSERT', 'UPDATE', 'DELETE')),
    id_registro         TEXT,
    registro_anterior   JSONB,
    registro_nuevo      JSONB,
    usuario_accion      UUID,                       -- usuario de la app (del JWT)
    usuario_bd          VARCHAR(100)    NOT NULL DEFAULT CURRENT_USER,  -- usuario de la BD
    fecha_accion        TIMESTAMPTZ     NOT NULL DEFAULT CURRENT_TIMESTAMP
);