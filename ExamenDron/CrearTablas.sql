-- Script DDL de creación de tablas
-- Base de datos: examendron


CREATE TABLE IF NOT EXISTS tb_master_control (
    id SERIAL PRIMARY KEY,
    fecha_hora TIMESTAMP NOT NULL,
    tamano_n INT NOT NULL,
    coordenada_x INT NOT NULL,
    coordenada_y INT NOT NULL
);

-- Tabla de detalle: secuencia de pasos
CREATE TABLE IF NOT EXISTS tb_det_log (
    id SERIAL PRIMARY KEY,
    id_simulacion INT NOT NULL,
    paso_ofuscado INT NOT NULL,
    pos_x INT NOT NULL,
    pos_y INT NOT NULL,
    CONSTRAINT fk_simulacion 
        FOREIGN KEY (id_simulacion) 
        REFERENCES tb_master_control(id) 
        ON DELETE CASCADE
);