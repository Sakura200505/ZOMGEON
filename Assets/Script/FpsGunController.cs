using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class FirstPersonGunController : MonoBehaviourPun
{
    public enum ShootMode { AUTO, SEMIAUTO }

    public bool shootEnabled = true;

    [SerializeField] private ShootMode shootMode = ShootMode.AUTO;
    [SerializeField] private int maxAmmo = 50;
    [SerializeField] private GameObject muzzleFlashPrefab;
    [SerializeField] private Vector3 muzzleFlashScale;
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private float shootRange = 50;
    [SerializeField] private float shootInterval = 0.1f;
    [SerializeField] private int damage = 1;
    [SerializeField] private Image ammoGauge;
    [SerializeField] private Text ammoText;
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
            if (ammoText != null) ammoText.text = ammo.ToString("D3");
            if (ammoGauge != null)
                ammoGauge.rectTransform.localScale = new Vector3((float)ammo / maxAmmo, 1, 1);
        }
    }

    private void Start()
    {
        if (!photonView.IsMine) return;
        Ammo = maxAmmo;
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        if (shootEnabled && ammo > 0 && GetInput())
            StartCoroutine(ShootTimer());
    }

    private bool GetInput()
    {
        return shootMode == ShootMode.AUTO ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0);
    }

    private IEnumerator ShootTimer()
    {
        if (shooting) yield break;

        shooting = true;
        HandleMuzzleFlash(true);
        Shoot();
        yield return new WaitForSeconds(shootInterval);
        HandleMuzzleFlash(false);
        shooting = false;
    }

    private void Shoot()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, shootRange))
        {
            HandleHitEffect(true, hit);

            if (hit.collider.CompareTag("Enemy"))
            {
                EnemyController enemy = hit.collider.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    // マスタークライアントがダメージ処理
                    if (PhotonNetwork.IsMasterClient)
                    {
                        enemy.Hp -= damage;
                    }
                    else
                    {
                        enemy.photonView.RPC("TakeDamage", RpcTarget.MasterClient, damage);
                    }

                    // HP <=0なら爆発エフェクトをローカルで再生
                    if (enemy.Hp <= 0)
                    {
                        if (explosionEffectPrefab != null)
                            Instantiate(explosionEffectPrefab, hit.point, Quaternion.identity);

                        if (audioSource != null && explosionSound != null)
                            audioSource.PlayOneShot(explosionSound);
                    }
                }
            }
        }

        Ammo--;
        if (audioSource != null && fireSe != null)
            audioSource.PlayOneShot(fireSe);
    }

    private void HandleMuzzleFlash(bool isActive)
    {
        if (muzzleFlashPrefab == null) return;

        if (muzzleFlash == null && isActive)
        {
            muzzleFlash = Instantiate(muzzleFlashPrefab, transform.position, transform.rotation, transform);
            muzzleFlash.transform.localScale = muzzleFlashScale;
        }

        if (muzzleFlash != null)
            muzzleFlash.SetActive(isActive);
    }

    private void HandleHitEffect(bool isActive, RaycastHit hit = default)
    {
        if (hitEffectPrefab == null || !isActive)
        {
            if (hitEffect != null) hitEffect.SetActive(false);
            return;
        }

        if (hitEffect == null)
        {
            hitEffect = Instantiate(hitEffectPrefab, hit.point, Quaternion.identity);
        }

        hitEffect.transform.position = hit.point;
        hitEffect.transform.rotation = Quaternion.FromToRotation(Vector3.forward, hit.normal);
        hitEffect.SetActive(true);
    }
}
