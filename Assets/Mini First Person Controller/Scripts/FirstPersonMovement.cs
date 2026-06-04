using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(Rigidbody))]
public class FirstPersonMovement : MonoBehaviourPun
{
    public float speed = 5;
    public bool canRun = true;
    public bool IsRunning { get; private set; }
    public float runSpeed = 9;
    public KeyCode runningKey = KeyCode.LeftShift;

    private Rigidbody rb;
    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (!photonView.IsMine)
        {
            rb.isKinematic = true; // 他のプレイヤーは物理演算しない
        }
    }

    void FixedUpdate()
    {
        Debug.Log($"IsMine={photonView.IsMine} ViewID={photonView.ViewID}");

        if (!photonView.IsMine) return; // 自分だけ操作

        IsRunning = canRun && Input.GetKey(runningKey);

        float targetMovingSpeed = IsRunning ? runSpeed : speed;
        if (speedOverrides.Count > 0)
        {
            targetMovingSpeed = speedOverrides[speedOverrides.Count - 1]();
        }

        Vector2 targetVelocity = new Vector2(
            Input.GetAxis("Horizontal") * targetMovingSpeed,
            Input.GetAxis("Vertical") * targetMovingSpeed
        );

        // Rigidbodyの正しいプロパティを使う
        Vector3 newVelocity = transform.rotation * new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.y);
        rb.linearVelocity = newVelocity;
    }
}
