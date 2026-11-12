using UnityEngine;
namespace RetyKotlety {
[CreateAssetMenu(menuName="Rety Kotlety/Movement tuning",fileName="ArenaMovement")]
public class MovementTuning : ScriptableObject {
 [Header("Bieg i pęd")]
 public float runSpeed=16.5f,groundAcceleration=180,groundBraking=100,airAcceleration=42,airFriction=0,maxMomentum=72;
 [Header("Skok")]
 public float jumpSpeed=12.5f,gravity=40,terminalSpeed=80,jumpBuffer=.12f,coyoteTime=.1f;
 [Header("Dash")]
 public float dashSpeed=42,dashDuration=.15f,maxStamina=3,staminaRegen=.85f,staminaDelay=.25f;
 public KeyCode dashKey=KeyCode.LeftShift;
 [Header("Ślizg i slam")]
 public KeyCode slideKey=KeyCode.LeftControl,alternateSlideKey=KeyCode.C;
 public float slideSpeed=24,slideTurnRate=.75f,slideDrag=8,slamSpeed=80,slamBounceMin=16,slamBounceMax=22;
 [Header("Ściany")]
 public float wallOutwardSpeed=8,wallTangentSpeed=12,wallCooldown=.22f,wallSteerLock=.1f,wallFallSpeed=4;
 public int wallJumps=3;
 [Header("Kamera")]
 public bool interpolateCamera=true;
 public float speedFov=6,cameraResponse=18;
 void OnValidate(){runSpeed=Mathf.Max(1,runSpeed);maxMomentum=Mathf.Max(runSpeed,maxMomentum);dashDuration=Mathf.Max(.05f,dashDuration);maxStamina=Mathf.Max(1,maxStamina);gravity=Mathf.Max(1,gravity);wallJumps=Mathf.Max(0,wallJumps);}
}
}
