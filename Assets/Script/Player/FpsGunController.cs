using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class FirstPersonGunController : MonoBehaviourPun
{
    public enum ShootMode
    {
        AUTO,
        SEMIAUTO
    }


    public bool shootEnabled = true;


    [Header("射撃設定")]
    [SerializeField] private ShootMode shootMode = ShootMode.AUTO;
    [SerializeField] private int maxAmmo = 50;
    [SerializeField] private float shootRange = 50f;
    [SerializeField] private float shootInterval = 0.1f;
    [SerializeField] private int damage = 1;


    [Header("エフェクト")]
    [SerializeField] private GameObject muzzleFlashPrefab;
    [SerializeField] private Vector3 muzzleFlashScale;

    [SerializeField] private GameObject hitEffectPrefab;

    [SerializeField] private GameObject explosionEffectPrefab;


    [Header("UI")]
    [SerializeField] private Image ammoGauge;
    [SerializeField] private Text ammoText;


    [Header("音")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip fireSe;
    [SerializeField] private AudioClip explosionSound;



    private bool shooting = false;

    private int ammo;

    private GameObject muzzleFlash;
    private GameObject hitEffect;



    public int Ammo
    {
        get => ammo;

        set
        {
            ammo = Mathf.Clamp(value, 0, maxAmmo);


            if (ammoText != null)
            {
                ammoText.text = ammo.ToString("D3");
            }


            if (ammoGauge != null)
            {
                ammoGauge.rectTransform.localScale =
                    new Vector3(
                        (float)ammo / maxAmmo,
                        1,
                        1
                    );
            }
        }
    }




    void Start()
    {
        if (!photonView.IsMine)
            return;


        Ammo = maxAmmo;
    }




    void Update()
    {
        if (!photonView.IsMine)
            return;


        if (
            shootEnabled &&
            ammo > 0 &&
            GetInput()
        )
        {
            StartCoroutine(ShootTimer());
        }
    }





    bool GetInput()
    {
        if (shootMode == ShootMode.AUTO)
        {
            return Input.GetMouseButton(0);
        }
        else
        {
            return Input.GetMouseButtonDown(0);
        }
    }





    IEnumerator ShootTimer()
    {
        if (shooting)
            yield break;


        shooting = true;


        HandleMuzzleFlash(true);


        Shoot();


        yield return new WaitForSeconds(shootInterval);


        HandleMuzzleFlash(false);


        shooting = false;
    }





    void Shoot()
    {
        Ray ray = new Ray(
            transform.position,
            transform.forward
        );


        if (
            Physics.Raycast(
                ray,
                out RaycastHit hit,
                shootRange
            )
        )
        {
            HandleHitEffect(true, hit);



            if (hit.collider.CompareTag("Enemy"))
            {
                EnemyController enemy =
                    hit.collider.GetComponent<EnemyController>();


                if (enemy != null)
                {
                    // MasterClientへダメージ送信
                    enemy.photonView.RPC(
                        "TakeDamage",
                        RpcTarget.MasterClient,
                        damage
                    );
                }
            }
        }


        Ammo--;


        if (
            audioSource != null &&
            fireSe != null
        )
        {
            audioSource.PlayOneShot(fireSe);
        }
    }





    void HandleMuzzleFlash(bool active)
    {
        if (muzzleFlashPrefab == null)
            return;


        if (
            muzzleFlash == null &&
            active
        )
        {
            muzzleFlash =
                Instantiate(
                    muzzleFlashPrefab,
                    transform.position,
                    transform.rotation,
                    transform
                );


            muzzleFlash.transform.localScale =
                muzzleFlashScale;
        }


        if (muzzleFlash != null)
        {
            muzzleFlash.SetActive(active);
        }
    }





    void HandleHitEffect(
        bool active,
        RaycastHit hit = default
    )
    {
        if (
            hitEffectPrefab == null ||
            !active
        )
        {
            if (hitEffect != null)
                hitEffect.SetActive(false);

            return;
        }



        if (hitEffect == null)
        {
            hitEffect =
                Instantiate(
                    hitEffectPrefab,
                    hit.point,
                    Quaternion.identity
                );
        }


        hitEffect.transform.position =
            hit.point;


        hitEffect.transform.rotation =
            Quaternion.FromToRotation(
                Vector3.forward,
                hit.normal
            );


        hitEffect.SetActive(true);
    }
}