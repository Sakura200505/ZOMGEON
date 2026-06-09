using UnityEngine;
using Photon.Pun;

public class FirstPersonLook : MonoBehaviour
{
    [SerializeField]
    Transform character;

    public float sensitivity = 2;
    public float smoothing = 1.5f;

    Vector2 velocity;
    Vector2 frameVelocity;

    PhotonView pv;

    void Start()
    {
        pv = GetComponentInParent<PhotonView>();

        if (pv != null && !pv.IsMine)
        {
            Camera cam = GetComponent<Camera>();
            if (cam != null)
                cam.enabled = false;

            AudioListener listener =
                GetComponent<AudioListener>();

            if (listener != null)
                listener.enabled = false;

            enabled = false;
            return;
        }
    }

    void Update()
    {
        Vector2 mouseDelta =
            new Vector2(
                Input.GetAxisRaw("Mouse X"),
                Input.GetAxisRaw("Mouse Y"));

        Vector2 rawFrameVelocity =
            Vector2.Scale(
                mouseDelta,
                Vector2.one * sensitivity);

        frameVelocity =
            Vector2.Lerp(
                frameVelocity,
                rawFrameVelocity,
                1 / smoothing);

        velocity += frameVelocity;
        velocity.y = Mathf.Clamp(velocity.y, -90, 90);

        transform.localRotation =
            Quaternion.AngleAxis(
                -velocity.y,
                Vector3.right);

        character.localRotation =
            Quaternion.AngleAxis(
                velocity.x,
                Vector3.up);
    }
}