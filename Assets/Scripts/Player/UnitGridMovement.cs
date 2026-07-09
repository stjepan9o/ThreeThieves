using UnityEngine;
using System;
using System.Collections.Generic;
 
public class UnitGridMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float waypointTolerance = 0.05f;
 
    [Header("Model Orientation")]
    [Tooltip("Ako model 'hoda bocno' - znaci da njegov vizualni 'front' nije poravnat s transform.forward " +
             "(lokalni +Z). Podesi ovaj kut (rotacija oko Y osi, u stupnjevima) dok se model ne pocne " +
             "ispravno okretati u smjeru kretanja. Najcesce tocne vrijednosti su -90, 90 ili 180, " +
             "ovisno o tome kako je model izvorno modeliran/uvezen u Unity.")]
    public float modelForwardOffset = 0f;
 
    public bool IsMoving { get; private set; }
 

    public Vector3? CurrentDestination => (currentPath != null && currentPath.Count > 0) ? currentPath[currentPath.Count - 1] : (Vector3?)null;
 
    private List<Vector3> currentPath;
    private int pathIndex;
 
    public event Action OnPathComplete;
 
    public void SetPath(List<Vector3> path, int maxSteps = -1)
    {
        if (path == null || path.Count == 0)
            return;
 
        if (maxSteps >= 0 && path.Count > maxSteps)
            path = path.GetRange(0, maxSteps);
 
        currentPath = path;
        pathIndex = 0;
        IsMoving = true;
    }
 
    public void StopMoving()
    {
        currentPath = null;
        pathIndex = 0;
        IsMoving = false;
    }
 
    void Update()
    {
        if (!IsMoving || currentPath == null)
            return;
 
        if (pathIndex >= currentPath.Count)
        {
            FinishPath();
            return;
        }
 
        Vector3 target = currentPath[pathIndex];
        target.y = transform.position.y; 
 
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
 
        Vector3 direction = target - transform.position;
        if (direction.sqrMagnitude > 0.0001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction.normalized, Vector3.up)
                                      * Quaternion.Euler(0f, modelForwardOffset, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }
 
        if (Vector3.Distance(transform.position, target) <= waypointTolerance)
        {
            transform.position = target;
            pathIndex++;
        }
    }
 
    private void FinishPath()
    {
        IsMoving = false;
        currentPath = null;
        pathIndex = 0;
        OnPathComplete?.Invoke();
    }
}