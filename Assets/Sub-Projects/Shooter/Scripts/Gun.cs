using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public Transform muzzleTransform;
    public GameObject bulletObject;
    [Tooltip("If negative, then infinite magazine size")]
    [SerializeField]
    private int magazineSize = -1;
    [Tooltip("Number of bullets that can be fired in a second")]
    [SerializeField]
    private float fireInterval = 4;
    [Tooltip("Speed at which bullet is shot at")]
    [SerializeField]
    private float shootSpeed = 5f;
    public bool IsShootReady { get { return shootReady; } }
    private int currentAmmo;
    private bool shootReady;


    private void Awake()
    {
        if (muzzleTransform == null)
        {
            Debug.LogError("Muzzle not defined");
        }
    }

    private void Start()
    {
        currentAmmo = magazineSize;
        shootReady = true;
    }

    public void Fire()
    {
        if (shootReady)
        {
            StartCoroutine(Shoot());
        }
    }

    // TODO: add object pooling later
    // for now just fire a ray cast
    IEnumerator Shoot()
    {
        // TODO: implement reload/magazine size later on
        shootReady = false;
        GameObject bullet = Instantiate(
            bulletObject,
            muzzleTransform.position,
            Quaternion.identity
        );
        bullet.GetComponent<Rigidbody>().AddForce(muzzleTransform.forward * shootSpeed, ForceMode.VelocityChange);
        Destroy(bullet, 2f);
        yield return new WaitForSeconds(1f / fireInterval);
        // yield return null;
        shootReady = true;
    }

    public void Reload()
    {

    }
}
