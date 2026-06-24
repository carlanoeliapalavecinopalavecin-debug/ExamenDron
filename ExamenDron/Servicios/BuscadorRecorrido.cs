using System;
using System.Collections.Generic;
using System.Linq;
using Modelos;

namespace Servicios
{
    public class BuscadorRecorrido
    {
        private static readonly (int dx, int dy)[] Movimientos = new (int, int)[]
        {
            (-2, -1), (-2, 1),
            (2, -1),  (2, 1),
            (-1, -2), (-1, 2),
            (1, -2),  (1, 2)
        };

        private readonly int _n;
        private int[,] _matriz;
        private int _pasoActual;
        private int _totalAlcanzables;
        private readonly List<Posicion> _recorrido = new List<Posicion>();

        public BuscadorRecorrido(int tamanoTerreno)
        {
            _n = tamanoTerreno;
            _matriz = new int[tamanoTerreno, tamanoTerreno];
            for (int i = 0; i < tamanoTerreno; i++)
                for (int j = 0; j < tamanoTerreno; j++)
                    _matriz[i, j] = -1;
        }

        public int ContarAlcanzables(Posicion inicio)
        {
            bool[,] visitadas = new bool[_n, _n];
            RecorrerAlcanzables(inicio.X, inicio.Y, visitadas);
            int cuenta = 0;
            for (int i = 0; i < _n; i++)
                for (int j = 0; j < _n; j++)
                    if (visitadas[i, j]) cuenta++;
            return cuenta;
        }

        private void RecorrerAlcanzables(int x, int y, bool[,] visitadas)
        {
            if (x < 0 || x >= _n || y < 0 || y >= _n || visitadas[x, y])
                return;
            visitadas[x, y] = true;
            foreach (var mov in Movimientos)
                RecorrerAlcanzables(x + mov.dx, y + mov.dy, visitadas);
        }

        public bool BuscarRecorrido(Posicion inicio, int totalAlcanzables)
        {
            _totalAlcanzables = totalAlcanzables;
            _pasoActual = 0;
            _recorrido.Clear();
            _matriz[inicio.X, inicio.Y] = 0;
            _recorrido.Add(inicio);
            return Explorar(inicio.X, inicio.Y);
        }

        private bool Explorar(int x, int y)
        {
            if (_pasoActual == _totalAlcanzables - 1)
                return true;

            var candidatos = ObtenerCandidatosOrdenados(x, y);

            foreach (var (nx, ny) in candidatos)
            {
                _pasoActual++;
                _matriz[nx, ny] = _pasoActual;
                _recorrido.Add(new Posicion(nx, ny));

                if (Explorar(nx, ny))
                    return true;

                _pasoActual--;
                _matriz[nx, ny] = -1;
                _recorrido.RemoveAt(_recorrido.Count - 1);
            }

            return false;
        }

        private List<(int x, int y)> ObtenerCandidatosOrdenados(int x, int y)
        {
            var lista = new List<(int x, int y, int grado)>();
            foreach (var mov in Movimientos)
            {
                int nx = x + mov.dx;
                int ny = y + mov.dy;
                if (nx >= 0 && nx < _n && ny >= 0 && ny < _n && _matriz[nx, ny] == -1)
                {
                    int grado = CalcularGrado(nx, ny);
                    lista.Add((nx, ny, grado));
                }
            }
            return lista.OrderBy(c => c.grado).Select(c => (c.x, c.y)).ToList();
        }

        private int CalcularGrado(int x, int y)
        {
            int salidas = 0;
            foreach (var mov in Movimientos)
            {
                int nx = x + mov.dx;
                int ny = y + mov.dy;
                if (nx >= 0 && nx < _n && ny >= 0 && ny < _n && _matriz[nx, ny] == -1)
                    salidas++;
            }
            return salidas;
        }

        public int[,] ObtenerMatriz() => _matriz;
        public List<Posicion> ObtenerRecorrido() => _recorrido;
    }
}
