
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

[UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
public class ClimbingScript : UdonSharpBehaviour
{
    private VRCPlayerApi ownerPlayer = null; //owner if object in game

    [SerializeField]
    ChairController chairController;

    [SerializeField]
    CapsuleCollider capsuleCollider;

    public Rocket rocket;

    public ClimbingHand leftHand;
    public ClimbingHand rightHand;

    private VRCPlayerApi localPlayer;
    private ClimbingHand currentClimbingHand;

    private bool isClimbing = false;

    bool isJumping = false;
    bool hasAddedJumpImpulse = false;

    bool onGround;
    bool surface;

    [SerializeField]
    float rayLength = 0.5f;

    [SerializeField]
    LayerMask floorLayer;

    [SerializeField]
    float applyGravityDistance = 0.2f;

    [SerializeField]
    float fallSpeed = 1f;

    [SerializeField]
    float jumpSpeed = 5f;

    bool leftHandHold = false, rightHandHold = false;

    [SerializeField]
    float JumpImpulse = 30f;

    Vector3 originalPosition;
    Quaternion originalRotation;

    const float SLOW_UPDATE_RATE = 0.5f;
    private float last_slow_update = 0;

    [HideInInspector] public float playerHeight;

    public VRCStation climbingStation;
    public Rigidbody climbingRd;
    public CapsuleCollider climbingCollider;

    [SerializeField]
    float timeDelayBeforeRotate = 1f;
    float timer;

    void Start()
    {
        localPlayer = Networking.LocalPlayer;
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        currentClimbingHand = null;
        
        climbingStation.disableStationExit = true;
        RemoveFromWorld();
        SetOwnerPlayer();
    }

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        SetOwnerPlayer();
    }

    public void SetOwnerPlayer()
    {
        ownerPlayer = Networking.GetOwner(gameObject);
        if (ownerPlayer.isMaster)
        {
            if (this != chairController.chairs[0]) //returns to pool
            {
                ownerPlayer = null;
                RemoveFromWorld();
            }
        }
        else
        {
            //code for when someone gets it
        }
    }

    public void AddVelocity(Vector3 value)
    {
        climbingRd.velocity += value;
    }

    public void AddForceAtPoint(Vector3 force, Vector3 position)
    {
        climbingRd.AddForceAtPosition(force, position, ForceMode.Impulse);
    }

    void FixedUpdate()
    {
        if (ownerPlayer == null) return;
        if (ownerPlayer != Networking.LocalPlayer) return; //only for local player

        Vector3 transformDown = transform.up * -1f;

        if (currentClimbingHand != null && isClimbing) //climbing
        {
            climbingRd.angularVelocity = Vector3.zero;
            Climbing();
            RotateClimb();
        }
        else //floating
        {
            Floating();

            //kick off ground
            onGround = Physics.Raycast(transform.position, transformDown, out RaycastHit hit, rayLength, floorLayer);
            if (onGround)
            {
                if (isJumping)
                {
                    if (!hasAddedJumpImpulse)
                    {
                        climbingRd.velocity += hit.normal * JumpImpulse;
                        hasAddedJumpImpulse = true;
                    }
                }
            }
            else
            {
                //push off wall
                Vector3 lookForward = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation * Vector3.forward;
                Vector3 lookBackward = lookForward * -1f;
                onGround = Physics.Raycast(localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position, lookForward, out hit, rayLength, floorLayer);
                if (onGround)
                {
                    if (isJumping)
                    {
                        if (!hasAddedJumpImpulse)
                        {
                            climbingRd.velocity +=  Vector3.Slerp(hit.normal, lookBackward, Vector3.Dot(lookBackward, hit.normal)) * JumpImpulse * 0.5f; //looking sort effect pushoff direction
                            hasAddedJumpImpulse = true;
                        }
                    }
                }
                else //push off roof
                {
                    onGround = Physics.Raycast(localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position, transform.up, out hit, rayLength, floorLayer);
                    if (onGround)
                    {
                        if (isJumping)
                        {
                            if (!hasAddedJumpImpulse)
                            {
                                climbingRd.velocity += hit.normal * JumpImpulse * 0.5f;
                                hasAddedJumpImpulse = true;
                            }
                        }
                    }
                }
            }
            

            if (isJumping)
            {
                isJumping = false;
            }

            

        }

        
    }

    private void Update()
    {
        if (ownerPlayer == null) return;
        if (ownerPlayer != Networking.LocalPlayer) return; //only for local player
        if (Time.time - last_slow_update > SLOW_UPDATE_RATE)
        {
            last_slow_update = Time.time;
            SlowUpdate();
        }
    }

    private void Floating()
    {
        Vector3 currentLeftHandVector = leftHand.currentPos - localPlayer.GetBonePosition(HumanBodyBones.LeftShoulder);
        transform.InverseTransformDirection(currentLeftHandVector);
        Vector3 previousLeftHandVector = leftHand.prevPos - localPlayer.GetBonePosition(HumanBodyBones.LeftShoulder);
        transform.InverseTransformDirection(previousLeftHandVector);

        Vector3 leftAxis = Vector3.Cross(currentLeftHandVector, previousLeftHandVector).normalized;

        float leftAngle = Vector3.SignedAngle(currentLeftHandVector, previousLeftHandVector, leftAxis);

        transform.RotateAround(transform.position, leftAxis, leftAngle);


        Vector3 currentRightHandVector = rightHand.currentPos - localPlayer.GetBonePosition(HumanBodyBones.RightShoulder);
        transform.InverseTransformDirection(currentRightHandVector);
        Vector3 previousRightHandVector = rightHand.prevPos - localPlayer.GetBonePosition(HumanBodyBones.RightShoulder);
        transform.InverseTransformDirection(previousRightHandVector);

        Vector3 rightAxis = Vector3.Cross(currentRightHandVector, previousRightHandVector).normalized;

        float rightAngle = Vector3.SignedAngle(currentRightHandVector, previousRightHandVector, rightAxis);

        transform.RotateAround(transform.position, rightAxis, rightAngle);
    }



    /*private void RotateClimb()
    {
        if (leftHandHold == true && rightHandHold == true)
        {
            if (timer < 0)
            {
                Vector3 startPosition = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
                Vector3 endPosition = leftHand.transform.position;
                Vector3 direction = endPosition - startPosition;
                Vector3 offsetStartPosition = startPosition - direction;
                surface = Physics.Raycast(offsetStartPosition, direction, out RaycastHit hit, direction.magnitude * 3f, laddersLayer);
                if (surface)
                {
                    ladderNormal = hit.normal;
                }
                Debug.DrawRay(offsetStartPosition, direction * 3f, Color.red);

                Vector3 prevDir = leftHand.prevPos - rightHand.prevPos;
                Vector3 currentDir = leftHand.currentPos - rightHand.currentPos;

                float angle = Vector3.SignedAngle(prevDir, currentDir, ladderNormal);

                transform.RotateAround(transform.position, ladderNormal, angle);
            }
            else
            {
                timer -= Time.fixedDeltaTime;
            }
        }
        else
        {
            timer = timeDelayBeforeRotate;
        }
    }*/

    private void RotateClimb()
    {
        if (leftHandHold == true && rightHandHold == true)
        {
            if (timer < 0)
            {
                Vector3 prevDir = leftHand.prevPos - rightHand.prevPos;
                Vector3 currentDir = leftHand.currentPos - rightHand.currentPos;

                climbingRd.rotation *= Quaternion.FromToRotation(prevDir, currentDir); //rotation from where started to here
            }
                else
            {
                timer -= Time.fixedDeltaTime;
            }    
        }
        else
        {
            timer = timeDelayBeforeRotate;
        }
    }

    private void Climbing()
    {
        climbingCollider.center = Vector3.up * playerHeight;

        Vector3 deltaVec = currentClimbingHand.currentPos - currentClimbingHand.prevPos;

        Vector3 handVelocity = deltaVec / Time.fixedDeltaTime;

        climbingRd.velocity = -handVelocity;
    }

    private void EnableClimbing()
    {
        isClimbing = true;
    }

    public void DisableClimbing()
    {
        currentClimbingHand = null;

        isClimbing = false;
    }

    public override void InputUse(bool value, UdonInputEventArgs args)
    {
        bool leftHandClick = (args.handType == HandType.LEFT);

        if (value)
        {
            if (leftHandClick)
            {
                leftHandHold = true;
                if (leftHand.isTouching)
                {
                    currentClimbingHand = leftHand;
                    EnableClimbing();
                }

            }
            else
            {
                rightHandHold = true;
                if (rightHand.isTouching)
                {
                    currentClimbingHand = rightHand;
                    EnableClimbing();
                }
            }
        }
        else
        {
            if (leftHandClick)
            {
                leftHandHold = false;
                if (currentClimbingHand == leftHand)
                {
                    DisableClimbing();
                }
            }
            else
            {
                rightHandHold = false;
                if (currentClimbingHand == rightHand)
                {
                    DisableClimbing();
                }
            }
        }

    }

   

    public override void InputJump(bool value, UdonInputEventArgs args)
    {
        if (ownerPlayer == null) return;
        if (ownerPlayer != Networking.LocalPlayer) return; //only for local player
        if (value)
        {
            isJumping = true;
            hasAddedJumpImpulse = false;
        }
    }

    /*public override void OnPlayerRespawn(VRCPlayerApi player)
    {
        if (ownerPlayer == null) return;
        if (ownerPlayer != Networking.LocalPlayer) return; //only for local player
        climbingRd.velocity = Vector3.zero;
        climbingRd.position = originalPosition;
        climbingRd.rotation = originalRotation;
        climbingStation.UseStation(localPlayer);
    }*/

    public override void OnPlayerRespawn(VRCPlayerApi player)
    {
        if (ownerPlayer == null) return;
        if (ownerPlayer != Networking.LocalPlayer) return; //only for local player
        RemoveFromWorld();
    }

    private void RemoveFromWorld()
    {
        climbingRd.velocity = Vector3.zero;
        climbingRd.position = Vector3.zero;
    }

    public void FloatInSpace()
    {
        if (ownerPlayer == null) return;
        if (ownerPlayer != Networking.LocalPlayer) return; //only for local player
        RescaleCapsuleCollider();

        climbingRd.velocity = Vector3.zero;
        climbingRd.angularVelocity = Vector3.zero;
        climbingRd.position = originalPosition;
        climbingRd.rotation = originalRotation;
        climbingStation.UseStation(localPlayer);
    }

    private void RescaleCapsuleCollider()
    {
        Vector3 feetPos = localPlayer.GetPosition();
        Vector3 EyePos = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;

        Vector3 height = EyePos - feetPos;

        capsuleCollider.height = height.magnitude;
        capsuleCollider.center = new Vector3(0f, height.magnitude * 0.5f, 0f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (ownerPlayer == null) return;
        if (ownerPlayer != Networking.LocalPlayer) return; //only for local player
        climbingRd.velocity = Vector3.zero;
    }

    private void SlowUpdate()
    {
        playerHeight = GetAvatarHeight(localPlayer);
    }

    private float GetAvatarHeight(VRCPlayerApi player) //seems convoluted but what ever
    {
        float height = 0;
        Vector3 postition1 = player.GetBonePosition(HumanBodyBones.Head);
        Vector3 postition2 = player.GetBonePosition(HumanBodyBones.Neck);
        height += (postition1 - postition2).magnitude;
        postition1 = postition2;
        postition2 = player.GetBonePosition(HumanBodyBones.Hips);
        height += (postition1 - postition2).magnitude;
        postition1 = postition2;
        postition2 = player.GetBonePosition(HumanBodyBones.RightLowerLeg);
        height += (postition1 - postition2).magnitude;
        postition1 = postition2;
        postition2 = player.GetBonePosition(HumanBodyBones.RightFoot);
        height += (postition1 - postition2).magnitude;
        return height;
    }
}
