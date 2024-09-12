using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

/// <summary>
/// Clase que genera una malla de nodos en un área definida. 
/// Permite crear nodos en una grilla, asignarles vecinos y buscar nodos en función de su proximidad a un objetivo o NPC.
/// </summary>

public class NodeGridTool : MonoBehaviour
{
    [Header("Layer Masks")]
    public LayerMask ignoreLayer;  // Mascara de capas para ignorar ciertos objetos al generar nodos.
    public LayerMask nodeLayer;    // Mascara de capas para la deteccion de nodos (si es necesario).

    [Header("Node Settings")]
    public float radius = 1f;  // Radio de deteccion para evitar colisiones con otros objetos.
    public int nodeCount = 10; // Numero de nodos en cada dimension.
    public Vector3Int size = new Vector3Int(10, 0, 10); // Tamano del area de generacion de nodos.
    public Vector3Int startPosition = new Vector3Int(0, 0, 0); // Posicion inicial para la generacion de nodos.
    public GameObject nodePrefab;  // Prefabricado de nodo a instanciar.
    public Vector3 nodeSize = new Vector3(2, 1, 2);  // Tamano del nodo, se puede ajustar si es necesario.

    private int nodeSpacing;      // Espacio entre cada nodo, calculado a partir del tamano y numero de nodos.
    private List<Node> allNodes = new List<Node>();  // Lista para almacenar todos los nodos generados.

    // Propiedad publica para acceder a todos los nodos generados.
    public List<Node> AllNodes { get => allNodes; }

    /// <summary>
    /// Genera la malla de nodos en base a la configuracion.
    /// </summary>
    public void Generate()
    {
        // Ajusta la dimension Y del tamano del area de nodos a 0.
        size.y = 0;

        if (size.sqrMagnitude >= 2)
        {
            // Limpia la lista de nodos y elimina el objeto padre existente si es necesario.
            allNodes.Clear();
            GameObject parentNode = GameObject.Find("NodeParent");
            if (parentNode != null)
            {
                DestroyImmediate(parentNode);
            }
            parentNode = new GameObject("NodeParent");
            parentNode.transform.parent = transform;

            Vector3Int extents = size / 2;  // Calcula el extenso del area de nodos.
            Vector3Int center = startPosition;  // Centro del area de nodos.
            Vector3Int min = center - extents;   // Coordenadas minimas del area.
            Vector3Int max = center + extents;   // Coordenadas maximas del area.
            Vector3Int nodeLength = size / (nodeCount - 1);  // Espacio entre nodos.
            nodeSpacing = nodeLength.x;  // Espaciado entre nodos en la direccion X.

            int nodeIndex = 0;

            // Llama a la funcion para crear una matriz de nodos.
            CreateNodeMatrix(parentNode, nodeSize, min, max, nodeLength, ref nodeIndex);
        }
    }

    /// <summary>
    /// Crea una matriz de nodos en el area especificada.
    /// </summary>
    /// <param name="parentNode">El objeto que actua como el padre de todos los nodos.</param>
    /// <param name="nodeSize">Tamano de cada nodo.</param>
    /// <param name="min">Coordenadas minimas del area de nodos.</param>
    /// <param name="max">Coordenadas maximas del area de nodos.</param>
    /// <param name="nodeLength">Espacio entre nodos.</param>
    /// <param name="nodeIndex">Indice para nombrar los nodos.</param>
    public void CreateNodeMatrix(GameObject parentNode, Vector3 nodeSize, Vector3Int min, Vector3Int max, Vector3Int nodeLength, ref int nodeIndex)
    {
        // Recorre el area en una malla de puntos para crear nodos.
        for (int x = min.x; x <= max.x; x += nodeLength.x)
        {
            for (int z = min.z; z <= max.z; z += nodeLength.z)
            {
                Vector3Int position = new Vector3Int(x, 0, z);  // Posicion del nodo a crear.
                Collider[] countColliders = new Collider[10];  // Array para almacenar colisiones detectadas.
                int colliderCount = Physics.OverlapSphereNonAlloc(position, radius, countColliders, ignoreLayer);  // Detecta colisiones alrededor de la posicion.

                if (colliderCount == 0)
                {
                    // Crea el objeto del nodo si no hay colisiones.
                    GameObject prefab = Instantiate(nodePrefab);
                    prefab.name = "Node" + nodeIndex++;  // Asigna un nombre unico al nodo.
                    prefab.transform.localScale = nodeSize;  // Configura el tamano del nodo.
                    prefab.transform.position = position;  // Configura la posicion del nodo.
                    prefab.transform.parent = parentNode.transform;  // Asigna el nodo al objeto padre.

                    Node node = prefab.GetComponent<Node>();  // Obtiene el componente Node del prefab.
                    node.radius = radius;  // Configura el radio de deteccion del nodo.
                    allNodes.Add(node);  // Anade el nodo a la lista de nodos.
                }
                else
                {
                    Debug.Log("No se genero el nodo: " + nodeIndex++);  // Log de error si no se genero el nodo.
                }
            }
        }
    }

    /// <summary>
    /// Asigna nodos vecinos a cada nodo en la lista de nodos usando un raycast para obtenerlos.
    /// </summary>
    public void GetNeigh()
    {
        foreach (Node node in _allNodes)
        {
            if (node != null && node._neightbourds.Count < 1)
            {
                node.GetNeightbourd(Vector3.forward, _size.z);
                node.GetNeightbourd(Vector3.back, _size.z);
                node.GetNeightbourd(Vector3.left, _size.z);
                node.GetNeightbourd(Vector3.right, _size.z);
            }
        }
    }

    /// <summary>
    /// Obtiene el nodo más cercano al objetivo, ignorando un nodo específico si se proporciona.
    /// </summary>
    /// <param name="target">Transform del objetivo.</param>
    /// <param name="ignoredNode">Nodo que se debe ignorar.</param>
    /// <returns>El nodo más cercano al objetivo.</returns>
    public Node GetNodeNearTarget(Transform target, Node ignoredNode = null)
    {
        float bestDistance = Mathf.Infinity;
        Node bestNode = null;

        foreach (Node currNode in _allNodes)
        {
            if (currNode == ignoredNode) continue;

            Vector3 nodePosition = currNode.transform.position;
            nodePosition.y = 0;
            float currDistance = Vector3.Distance(target.position, nodePosition);
            Vector3 directionToTarget = (target.position - nodePosition).normalized;
            float angleToTarget = Vector3.Angle(target.forward, directionToTarget);

            if (currDistance < bestDistance || (currDistance == bestDistance && angleToTarget < 180))
            {
                bestDistance = currDistance;
                bestNode = currNode;
            }
        }

        return bestNode;
    }

    /// <summary>
    /// Dibuja gizmos para representar el área de generación de nodos.
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(_startPosition, _size);
    }



}
