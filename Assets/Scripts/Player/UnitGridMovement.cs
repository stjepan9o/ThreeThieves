using UnityEngine;
using System;
using System.Collections.Generic;
 
/// <summary>
/// Generalna komponenta koja pomice bilo koju jedinicu (player ili guard) duz zadanog
/// puta (liste world-pozicija). Namjerno ne zna nista o inputu, AP-u ili AI odlukama -
/// samo izvrsava path koji joj netko drugi da preko SetPath().
///
/// Player controller ce zvati: SetPath(Pathfinder.Instance.FindPath(...), maxTilesPerTurn)
/// Guard AI ce zvati istu metodu sa svojim patrol/chase odredistem.
/// Tako oba sustava koriste identicnu A* logiku i identican nacin kretanja - lakse za
/// debug i balansiranje (npr. ako promijenis moveSpeed formulu, mijenjas na jednom mjestu).
/// </summary>
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
 
    /// <summary>True dok jedinica aktivno prolazi kroz path.</summary>
    public bool IsMoving { get; private set; }
 
    /// <summary>Trenutni cilj (zadnji waypoint trenutnog puta), korisno za guard FOV/debug.</summary>
    public Vector3? CurrentDestination => (currentPath != null && currentPath.Count > 0) ? currentPath[currentPath.Count - 1] : (Vector3?)null;
 
    private List<Vector3> currentPath;
    private int pathIndex;
 
    /// <summary>Okida se kad jedinica zavrsi (ili joj se prekine) put.</summary>
    public event Action OnPathComplete;
 
    /// <summary>
    /// Postavlja novi put i pokrece kretanje.
    /// maxSteps ogranicava broj tile-ova koji ce se proci (npr. player: maxTilesPerTurn,
    /// guard: koliko daleko moze otici u svom AI potezu). -1 = bez ogranicenja.
    /// </summary>
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
 
    /// <summary>Odmah prekida kretanje (npr. ako guard primijeti playera usred patrole).</summary>
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
        target.y = transform.position.y; // drzi jedinicu na svojoj Y razini (top-down 2.5D)
 
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
 
        Vector3 direction = target - transform.position;
        if (direction.sqrMagnitude > 0.0001f)
        {
            // modelForwardOffset ispravlja slucaj kad vizualni model nije orijentiran prema lokalnom +Z
            // (npr. model je modeliran da "gleda" prema +X) - bez ovoga ce lik hodati ravno, ali djelovati kao da ide bocno
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