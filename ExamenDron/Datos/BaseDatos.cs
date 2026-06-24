using System;
using Npgsql;
using Modelos;
using System.Collections.Generic;

namespace Datos
{
    public class BaseDatos
    {
        private readonly string _cadenaConexion;

        public BaseDatos(string cadenaConexion)
        {
            _cadenaConexion = cadenaConexion;
        }

        public void CrearTablas()
        {
            using var conexion = new NpgsqlConnection(_cadenaConexion);
            conexion.Open();

            string sqlMaster = @"
                CREATE TABLE IF NOT EXISTS tb_master_control (
                    id SERIAL PRIMARY KEY,
                    fecha_hora TIMESTAMP NOT NULL,
                    tamano_n INT NOT NULL,
                    coordenada_x INT NOT NULL,
                    coordenada_y INT NOT NULL
                );";

            string sqlDetalle = @"
                CREATE TABLE IF NOT EXISTS tb_det_log (
                    id SERIAL PRIMARY KEY,
                    id_simulacion INT NOT NULL,
                    paso_ofuscado INT NOT NULL,
                    pos_x INT NOT NULL,
                    pos_y INT NOT NULL,
                    FOREIGN KEY (id_simulacion) REFERENCES tb_master_control(id) ON DELETE CASCADE
                );";

            using var cmd1 = new NpgsqlCommand(sqlMaster, conexion);
            cmd1.ExecuteNonQuery();

            using var cmd2 = new NpgsqlCommand(sqlDetalle, conexion);
            cmd2.ExecuteNonQuery();
        }

        public int GuardarSimulacion(int n, Posicion inicio, List<Posicion> recorrido)
        {
            using var conexion = new NpgsqlConnection(_cadenaConexion);
            conexion.Open();
            using var transaccion = conexion.BeginTransaction();

            try
            {
                string sqlInsertMaster = @"
                    INSERT INTO tb_master_control (fecha_hora, tamano_n, coordenada_x, coordenada_y)
                    VALUES (@fecha, @n, @x, @y)
                    RETURNING id;";

                int idSimulacion;
                using (var cmdMaster = new NpgsqlCommand(sqlInsertMaster, conexion, transaccion))
                {
                    cmdMaster.Parameters.AddWithValue("@fecha", DateTime.Now);
                    cmdMaster.Parameters.AddWithValue("@n", n);
                    cmdMaster.Parameters.AddWithValue("@x", inicio.X);
                    cmdMaster.Parameters.AddWithValue("@y", inicio.Y);
                    idSimulacion = Convert.ToInt32(cmdMaster.ExecuteScalar());
                }

                int i = 0;
                while (i < recorrido.Count)
                {
                    var pos = recorrido[i];
                    int pasoReal = i;
                    int pasoOfuscado = pasoReal % 2 == 0 ? pasoReal * 2 : -pasoReal;

                    string sqlInsertDet = @"
                        INSERT INTO tb_det_log (id_simulacion, paso_ofuscado, pos_x, pos_y)
                        VALUES (@id, @paso, @x, @y);";

                    using var cmdDet = new NpgsqlCommand(sqlInsertDet, conexion, transaccion);
                    cmdDet.Parameters.AddWithValue("@id", idSimulacion);
                    cmdDet.Parameters.AddWithValue("@paso", pasoOfuscado);
                    cmdDet.Parameters.AddWithValue("@x", pos.X);
                    cmdDet.Parameters.AddWithValue("@y", pos.Y);
                    cmdDet.ExecuteNonQuery();

                    i++;
                }

                transaccion.Commit();
                return idSimulacion;
            }
            catch
            {
                transaccion.Rollback();
                throw;
            }
        }

        public void MostrarUltimos5Pasos(int idSimulacion)
        {
            using var conexion = new NpgsqlConnection(_cadenaConexion);
            conexion.Open();

            string sql = @"
                SELECT paso_ofuscado, pos_x, pos_y
                FROM tb_det_log
                WHERE id_simulacion = @id
                ORDER BY id DESC
                LIMIT 5;";

            Console.WriteLine("\n Últimos 5 pasos (reconstruidos):");
            using var cmd = new NpgsqlCommand(sql, conexion);
            cmd.Parameters.AddWithValue("@id", idSimulacion);

            using var lector = cmd.ExecuteReader();
            while (lector.Read())
            {
                int valorGuardado = lector.GetInt32(0);
                int x = lector.GetInt32(1);
                int y = lector.GetInt32(2);

                int pasoReal = valorGuardado >= 0 ? valorGuardado / 2 : -valorGuardado;
                Console.WriteLine($"Paso {pasoReal} → Posición ({x},{y})");
            }
        }
    }
}