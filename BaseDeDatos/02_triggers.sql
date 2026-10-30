-- Las fechas de auditoría las pone la base, no la aplicación.
-- En el insert se ignora lo que venga en fecha_creacion / fecha_modificacion,
-- y en el update se protege fecha_creacion para que nadie la cambie.

CREATE OR REPLACE FUNCTION fn_usuarios_fechas()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        NEW.fecha_creacion := now();
        NEW.fecha_modificacion := NULL;
    ELSIF TG_OP = 'UPDATE' THEN
        NEW.fecha_creacion := OLD.fecha_creacion;
        NEW.fecha_modificacion := now();
    END IF;

    RETURN NEW;
END;
$$;

DROP TRIGGER IF EXISTS tr_usuarios_fechas ON usuarios;

CREATE TRIGGER tr_usuarios_fechas
    BEFORE INSERT OR UPDATE ON usuarios
    FOR EACH ROW
    EXECUTE FUNCTION fn_usuarios_fechas();
