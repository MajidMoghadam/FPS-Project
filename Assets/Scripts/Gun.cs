using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour
{
    // Firing & reload settings
    public float reloadTime = 1f;     // time to reload
    public float fireRate = 0.15f;    // delay between shots
    public int maxSize = 20;          // magazine size

    // Bullet & spawn point
    public GameObject bullet;         // bullet prefab
    public Transform bulletSpawnPoint;// muzzle / spawn position

    // Recoil & flash
    public float recoilDistance = 0.1f; // how far gun moves back
    public float recoilSpeed = 15f;     // how fast recoil animates
    public GameObject weaponFlash;      // muzzle flash prefab (point light)

    // State
    private int currentAmmo;            // current bullets in mag
    private bool isReloading = false;   // true while reloading
    private float nextTimeToFire = 0f;  // time when next shot allowed

    // Reload animation (rotation)
    private Quaternion initialRotation;                 // starting local rotation
    private Vector3 initialPosition;                    // starting local position
    private Vector3 reloadRotationOffset = new Vector3(66f, 50f, 50f); // reload tilt

    void Start()
    {
        currentAmmo = maxSize;                           // fill magazine
        initialRotation = transform.localRotation;       // cache rotation
        initialPosition = transform.localPosition;       // cache position
    }

    public void Shoot()                                  // called while holding click
    {
        if (isReloading) return;                         // block during reload
        if (Time.time < nextTimeToFire) return;          // respect fire rate

        if (currentAmmo <= 0)                            // empty mag → reload
        {
            StartCoroutine(Reload());
            return;
        }

        nextTimeToFire = Time.time + fireRate;           // schedule next shot
        currentAmmo--;                                   // consume ammo

        Instantiate(bullet, bulletSpawnPoint.position, bulletSpawnPoint.rotation);     // spawn bullet at muzzle
        //Instantiate(weaponFlash, bulletSpawnPoint.position, bulletSpawnPoint.rotation); // spawn brief muzzle flash

        Instantiate(weaponFlash,bulletSpawnPoint.position - bulletSpawnPoint.forward * 0.03f, bulletSpawnPoint.rotation); // 3 cm inside the barrel



        StopCoroutine(nameof(Recoil));                   // ensure single recoil
        StartCoroutine(Recoil());                        // run recoil animation
    }

    IEnumerator Reload()                                 // reload animation
    {
        isReloading = true;                              // mark reloading

        Quaternion targetRotation = Quaternion.Euler(    // tilt to reload pose
            initialRotation.eulerAngles + reloadRotationOffset);

        float halfReload = reloadTime / 2f;              // go out then back
        float t = 0f;

        while (t < halfReload)                           // initial → target
        {
            t += Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(initialRotation, targetRotation, t / halfReload);
            yield return null;
        }

        t = 0f;
        while (t < halfReload)                           // target → initial
        {
            t += Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(targetRotation, initialRotation, t / halfReload);
            yield return null;
        }

        currentAmmo = maxSize;                           // refill mag
        isReloading = false;                             // done
    }

    public void TryReload()                              // called on R
    {
        if (isReloading || currentAmmo == maxSize) return; // skip if busy/full
        StartCoroutine(Reload());                        // start reload
    }

    IEnumerator Recoil()                                 // position-based recoil
    {
        Vector3 recoilTarget = initialPosition + new Vector3(0f, 0f, -recoilDistance); // move back along Z
        float t = 0f;

        while (t < 1f)                                   // initial → recoil
        {
            t += Time.deltaTime * recoilSpeed;
            transform.localPosition = Vector3.Lerp(initialPosition, recoilTarget, t);
            yield return null;
        }

        t = 0f;
        while (t < 1f)                                   // recoil → initial
        {
            t += Time.deltaTime * recoilSpeed;
            transform.localPosition = Vector3.Lerp(recoilTarget, initialPosition, t);
            yield return null;
        }

        transform.localPosition = initialPosition;       // snap back to start
    }
}
