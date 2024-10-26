using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private bool serverAuth_; 

    private NetworkVariable<PlayerNetworkState> playerState_;

    [SerializeField] private float cheapInterpolationTime_ = 0.1f;

    private Rigidbody rb_;

    private Vector3 vel_;
    private float rotVelX_, rotVelY_, rotVelZ_;

    private void Awake()
    {
        rb_ = GetComponent<Rigidbody>();

        var permission = serverAuth_ ? NetworkVariableWritePermission.Server : NetworkVariableWritePermission.Owner;
        playerState_ = new NetworkVariable<PlayerNetworkState>(writePerm: permission);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            Destroy(transform.GetComponent<DemoPlayerMovement>());
        }
    }

    private void Update()
    {
        if (IsOwner)
        {
            TransmitState();
        }
        else
        {
            ConsumeState();
        }
    }

    private void TransmitState()
    {
        var state = new PlayerNetworkState
        {
            Position = rb_.position,
            Rotation = transform.rotation.eulerAngles
        };

        if (IsServer || !serverAuth_)
        {
            playerState_.Value = state;
        }
        else
        {
            TransmitStateServerRpc(state);
        }
    }

    [ServerRpc]
    private void TransmitStateServerRpc(PlayerNetworkState state)
    {
        playerState_.Value = state;
    }

    private void ConsumeState()
    {
        rb_.MovePosition(Vector3.SmoothDamp(rb_.position, playerState_.Value.Position, ref vel_, cheapInterpolationTime_));

        transform.rotation = Quaternion.Euler(
            Mathf.SmoothDampAngle(transform.rotation.eulerAngles.x, playerState_.Value.Rotation.x, ref rotVelX_, cheapInterpolationTime_),
            Mathf.SmoothDampAngle(transform.rotation.eulerAngles.y, playerState_.Value.Rotation.y, ref rotVelY_, cheapInterpolationTime_),
            Mathf.SmoothDampAngle(transform.rotation.eulerAngles.z, playerState_.Value.Rotation.z, ref rotVelZ_, cheapInterpolationTime_)
            );
    }

    struct PlayerNetworkState : INetworkSerializable
    {
        private float x_, y_, z_;
        private float xRot_, yRot_, zRot_;
        internal Vector3 Position {
            get => new (x_, y_, z_);

            set {
                x_ = value.x;
                y_ = value.y;
                z_ = value.z;
            }
        }

        internal Vector3 Rotation {
            get => new Vector3(xRot_, yRot_, zRot_);
            set
            {
                xRot_ = (short)value.x;
                yRot_ = (short)value.y;
                zRot_ = (short)value.z;
            }
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref x_);
            serializer.SerializeValue(ref y_);
            serializer.SerializeValue(ref z_);

            serializer.SerializeValue(ref xRot_);
            serializer.SerializeValue(ref yRot_);
            serializer.SerializeValue(ref zRot_);
        }

    }
    
}
