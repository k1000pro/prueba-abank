-- Datos de prueba
-- Passwords (guardados con bcrypt):
--   70000000 -> Admin12345
--   el resto -> Prueba123
-- Las fechas de creación las pone el trigger.

INSERT INTO usuarios (nombres, apellidos, fecha_nacimiento, direccion, password, telefono, email, estado)
SELECT v.* FROM (VALUES
    ('Administrador', 'Pruebas', DATE '1990-01-15', 'Col. Escalón, San Salvador',
        '$2a$12$4WiaiY3T5Ba2qYvFh9s.hOPYMA6GaQvLhNrNiQ7miagIGc6QVpx9m', '70000000', 'admin@abank-pruebas.com', 'A'),
    ('María José', 'López Ramírez', DATE '1995-06-21', 'Res. Santa Elena, Antiguo Cuscatlán',
        '$2a$12$MCqQM2GXqzHO7yuH..tWUOBzYWIJfQhRoNwM6SZFckFC22nLhJIay', '71234567', 'maria.lopez@abank-pruebas.com', 'A'),
    ('Carlos Alberto', 'Hernández Flores', DATE '1988-03-17', 'Calle 50 B Sur, Santa Tecla',
        '$2a$12$MCqQM2GXqzHO7yuH..tWUOBzYWIJfQhRoNwM6SZFckFC22nLhJIay', '72345678', 'carlos.hernandez@abank-pruebas.com', 'A'),
    ('Ana Sofía', 'Rivas Molina', DATE '2000-11-02', 'Barrio El Centro, Santa Ana',
        '$2a$12$MCqQM2GXqzHO7yuH..tWUOBzYWIJfQhRoNwM6SZFckFC22nLhJIay', '22501234', 'ana.rivas@abank-pruebas.com', 'A'),
    ('Pedro Antonio', 'Martínez Cruz', DATE '1979-08-09', 'Col. Miramonte, San Salvador',
        '$2a$12$MCqQM2GXqzHO7yuH..tWUOBzYWIJfQhRoNwM6SZFckFC22nLhJIay', '76543210', 'pedro.martinez@abank-pruebas.com', 'I')
) AS v (nombres, apellidos, fecha_nacimiento, direccion, password, telefono, email, estado)
-- para poder volver a correr el script sin duplicar
WHERE NOT EXISTS (SELECT 1 FROM usuarios u WHERE u.telefono = v.telefono);
