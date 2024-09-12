using System;
using UnityEngine;

public class LineOfSight : MonoBehaviour
{
    [Header("||--Line Of View--||")]
    public float _radiusView;    // Radio en el cual la araña puede detectar al objetivo.
    public float _angleView;     // Ángulo de visión de la araña.
    public LayerMask _ignoreMask; // Capas que deben ser ignoradas en los rayos de visión.
    public Transform _target;  // Referencia al objetivo (por ejemplo, el jugador). [Tambien podria ser tratada como un Vector 3]

    /// <summary>
    /// Verifica si el objetivo está dentro del rango de visión definido.
    /// </summary>
    /// <param name="target">Transform del objetivo (jugador u otro).</param>
    /// <returns>Devuelve true si el objetivo está dentro del rango, false en caso contrario.</returns>
    public bool CheckRange(Transform target)
    {
        // Calcula la distancia entre la araña y el objetivo
        float distance = Vector3.Distance(transform.position, target.position);

        // Comprueba si la distancia es menor al radio de visión de la araña
        return distance < _radiusView;
    }

    /// <summary>
    /// Verifica si el objetivo está dentro del ángulo de visión de la araña.
    /// </summary>
    /// <param name="target">Transform del objetivo (jugador u otro).</param>
    /// <returns>Devuelve true si el objetivo está dentro del ángulo de visión, false en caso contrario.</returns>
    public bool CheckAngle(Transform target)
    {
        // Dirección hacia el objetivo desde la posición de la araña
        Vector3 forward = transform.forward;
        Vector3 dirToTarget = (target.position - transform.position).normalized;

        // Calcula el ángulo entre la dirección hacia el objetivo y la orientación actual de la araña
        float angleToTarget = Vector3.Angle(forward, dirToTarget);

        // Comprueba si el ángulo es menor a la mitad del ángulo de visión
        return _angleView / 2 > angleToTarget;
    }

    /// <summary>
    /// Verifica si el objetivo es visible directamente desde la posición de la araña.
    /// </summary>
    /// <param name="target">Transform del objetivo (jugador u otro).</param>
    /// <returns>Devuelve true si el objetivo es visible, false en caso contrario.</returns>
    public bool CheckView(Transform target)
    {
        // Calcula la dirección y la distancia al objetivo
        Vector3 diff = target.position - transform.position;
        float distanceToTarget = diff.magnitude;
        Vector3 dirToTarget = diff.normalized;

        // Ajusta la posición de origen del rayo para que sea a nivel del suelo
        Vector3 fixedOriginY = transform.position;

        RaycastHit hit;

        // Lanza un rayo desde la araña hacia el objetivo, ignorando ciertas capas
        return !Physics.Raycast(fixedOriginY, dirToTarget, out hit, distanceToTarget, _ignoreMask);
    }
}
