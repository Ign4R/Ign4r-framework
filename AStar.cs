using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary> 
/// Implementación genérica del algoritmo A* para búsqueda del camino óptimo entre nodos.
/// Optimizado para minimizar llamadas y mejorar eficiencia.
/// Generico para utilizar con distintos tipos de objetos
/// </summary>
/// <typeparam name="T">Tipo de nodo utilizado en la búsqueda.</typeparam>
public class AStar<T>
{
    /// <summary>
    /// Ejecuta el algoritmo A* para encontrar el camino más corto desde el nodo inicial.
    /// </summary>
    /// <param name="start">Nodo de inicio.
    /// <param name="satiesfies">Función que verifica si se ha alcanzado el nodo objetivo.
    /// <param name="conections">Función que devuelve los vecinos de un nodo.
    /// <param name="getCost">Función que calcula el costo entre dos nodos.
    /// <param name="heuristic">Función heurística para estimar la distancia al objetivo.
    /// <param name="watchdog">Número máximo de iteraciones para evitar bucles infinitos.
    /// <returns> Lista de nodos que representan el camino más corto al objetivo.

    public List<T> Run(
        T start,
        Func<T, bool> satiesfies,
        Func<T, List<T>> conections,
        Func<T, T, float> getCost,
        Func<T, float> heuristic,
        int watchdog = 100)
    {
        // Cola de prioridad para los nodos pendientes por explorar
        PriorityQueue<T> pending = new PriorityQueue<T>();
        // Conjunto de nodos visitados
        HashSet<T> visited = new HashSet<T>();
        // Diccionario que guarda el nodo padre para reconstruir el camino
        Dictionary<T, T> parent = new Dictionary<T, T>();
        // Diccionario que guarda el costo acumulado para cada nodo
        Dictionary<T, float> cost = new Dictionary<T, float>();

        pending.Enqueue(start, 0); // Iniciar con el nodo de inicio
        cost[start] = 0;

        // Búsqueda mientras hay nodos por explorar y no se excede el límite de iteraciones
        while (watchdog > 0 && !pending.IsEmpty)
        {
            watchdog--;
            var curr = pending.Dequeue(); // Obtener el nodo con el menor costo
            Debug.Log("ASTAR");

            // Si se alcanza el objetivo, reconstruir el camino y devolverlo
            if (satiesfies(curr))
            {
                var path = new List<T> { curr };
                while (parent.ContainsKey(path[path.Count - 1]))
                {
                    var father = parent[path[path.Count - 1]];
                    path.Add(father);
                }
                path.Reverse();
                return path;
            }

            visited.Add(curr); // Marcar el nodo como visitado

            // Obtener nodos vecinos y explorar cada uno
            var neighbours = conections(curr);
            for (int i = 0; i < neighbours.Count; i++)
            {
                var neigh = neighbours[i];

                // Si el vecino ya fue visitado, continuar con el siguiente
                if (visited.Contains(neigh)) continue;

                // Calcular el costo tentativo para llegar al vecino
                float tentativeCost = cost[curr] + getCost(curr, neigh);

                // Si el vecino tiene un costo menor registrado, no actualizar
                if (cost.ContainsKey(neigh) && cost[neigh] < tentativeCost) continue;

                // Agregar el vecino a la cola con su costo acumulado + heurística
                pending.Enqueue(neigh, tentativeCost + heuristic(neigh));
                parent[neigh] = curr; // Establecer el nodo actual como padre
                cost[neigh] = tentativeCost; // Actualizar el costo acumulado
            }
        }
        return new List<T>(); // Si no se encuentra un camino, devolver una lista vacía
    }

    /// <summary>
    /// Limpia el camino eliminando nodos innecesarios si dos nodos pueden verse directamente.
    /// </summary>
    /// <param name="path">Lista original de nodos que representan el camino.
    /// <param name="inView">Función que determina si dos nodos pueden verse entre sí.
    /// <returns>Camino optimizado sin nodos innecesarios.

    public List<T> CleanPath(List<T> path, Func<T, T, bool> inView)
    {
        // Si el camino es nulo o tiene 2 nodos o menos, no se requiere limpieza
        if (path == null || path.Count <= 2) return path;

        var list = new List<T> { path[0] }; // Agregar el primer nodo al camino limpio

        // Revisar el camino para eliminar nodos innecesarios
        for (int i = 2; i < path.Count - 1; i++)
        {
            var grandParent = list[list.Count - 1];
            if (!inView(grandParent, path[i]))
            {
                list.Add(path[i - 1]); // Agregar nodo si no hay línea de visión directa
            }
        }

        list.Add(path[path.Count - 1]); // Agregar el nodo final al camino limpio
        return list;
    }
}
