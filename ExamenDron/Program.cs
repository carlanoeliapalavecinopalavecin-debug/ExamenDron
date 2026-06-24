using System;
using Microsoft.Extensions.Configuration;
using Modelos;
using Servicios;
using Datos;

namespace ExamenDron
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== EXAMEN PARCIAL: SIMULADOR DE RECORRIDO DEL DRON ===");

            
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            string cadenaConexion = config.GetConnectionString("CadenaPostgres")!;

            try
            {
                // Leer y validar entradas del usuario
                int n = LeerEntero("Ingrese el tamaño del terreno N (mayor o igual a 1): ", 1);
                int x = LeerEntero($"Ingrese la coordenada X inicial (entre 0 y {n - 1}): ", 0, n - 1);
                int y = LeerEntero($"Ingrese la coordenada Y inicial (entre 0 y {n - 1}): ", 0, n - 1);

                Posicion inicio = new Posicion(x, y);

                Console.WriteLine("\n Calculando recorrido...");

                var buscador = new BuscadorRecorrido(n);
                int totalAlcanzables = buscador.ContarAlcanzables(inicio);
                Console.WriteLine($" Cantidad de parcelas alcanzables: {totalAlcanzables}");

                bool exito = buscador.BuscarRecorrido(inicio, totalAlcanzables);

                if (!exito)
                {
                    Console.WriteLine("\n No existe un recorrido que cubra todas las parcelas alcanzables.");
                    return;
                }

                
                Console.WriteLine("\n TERRENO RECORRIDO:");
                int[,] matriz = buscador.ObtenerMatriz();
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (matriz[i, j] == -1)
                            Console.Write(" . ".PadRight(5));
                        else
                            Console.Write($"{matriz[i, j],3} ");
                    }
                    Console.WriteLine();
                }

                
                var baseDatos = new BaseDatos(cadenaConexion);
                baseDatos.CrearTablas();
                int idSimulacion = baseDatos.GuardarSimulacion(n, inicio, buscador.ObtenerRecorrido());

                Console.WriteLine($"\n Simulación guardada correctamente con ID: {idSimulacion}");

                baseDatos.MostrarUltimos5Pasos(idSimulacion);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n Error en la ejecución: {ex.Message}");
            }

            Console.WriteLine("\nPresione cualquier tecla para finalizar...");
            Console.ReadKey();
        }

        
        private static int LeerEntero(string mensaje, int minimo, int maximo = int.MaxValue)
        {
            int valor;
            while (true)
            {
                Console.Write(mensaje);
                if (int.TryParse(Console.ReadLine(), out valor) && valor >= minimo && valor <= maximo)
                    return valor;

                Console.WriteLine($" Entrada inválida. Debe ser un número entre {minimo} y {maximo}.");
            }
        }
    }
}
