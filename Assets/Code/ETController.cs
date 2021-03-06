using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ETController : MonoBehaviour
{

  [Range(1f, 5f)]
  public float MovementSpeed = 3f;

  [Range(1, 200f)]
  public float LookSensitivity = 10f;

  [Range(1, 100f)]
  public float JumpStrength = 2f;

  //获取到场景中的Joystick
  public ETCJoystick moveJoystick;

  //获取场景中的Button
  public ETCButton jumpButton;
  public ETCButton jetButton;

  public Vector3 debugGro = new Vector3(0.0f, 270.0f, 90.0f);

  private const float lowPassFilterFactor = 0.2f;



  private CharacterController characterController;
  private Transform cameraTransform;

  private float cameraTilt = 0f;
  private float verticalSpeed = 0f;
  private float timeInAir = 0f;
  private bool jumpLocked = false;



  void Start()
  {
    this.characterController = this.GetComponent<CharacterController>();
    this.cameraTransform = this.GetComponentInChildren<Camera>().transform;
    this.moveJoystick = ETCInput.GetControlJoystick("MoveJoystick");
    this.jumpButton.onDown.AddListener(() =>
    {
      if (!this.jumpLocked && this.timeInAir < 0.5f)
      {
        this.timeInAir = 0.5f;
        this.verticalSpeed = this.JumpStrength;
        this.jumpLocked = true;
      }
    });
    this.jumpButton.onUp.AddListener(() => this.jumpLocked = false);
    this.jetButton.onPressed.AddListener(() => this.verticalSpeed = 2f);
    Cursor.visible = true;
    Cursor.lockState = CursorLockMode.Confined;
    //陀螺仪的设置
    Input.gyro.enabled = true;
    Input.compensateSensors = true;
    Input.gyro.updateInterval = 0.01f;
  }

  void Awake()
  {
    this.transform.eulerAngles = new Vector3(90, 0, 0);
  }

  void Update()
  {
    bool touchesGround = this.onGround();
    float runMultiplier = 1f + 2f * Input.GetAxis("Run");
    float y = this.transform.position.y;
    Vector3 movementVector = this.cameraTransform.forward * this.moveJoystick.axisY.axisValue + this.cameraTransform.right * this.moveJoystick.axisX.axisValue;
    movementVector.y = 0;
    if (movementVector.sqrMagnitude > 1)
    { // this check prevents partial joystick input from becoming 100% speed
      movementVector.Normalize();  // this prevents diagonal movement form being too fast
    }
    this.characterController.Move(movementVector * Time.deltaTime * this.MovementSpeed * runMultiplier);
    float verticalMovement = this.transform.position.y - y;
    if (verticalMovement < 0)
    {
      this.transform.position += Vector3.down * verticalMovement;
    }
    // this.transform.rotation = Quaternion.AngleAxis(this.lookJoystick.axisX.axisValue * Time.deltaTime * this.LookSensitivity, Vector3.up) * this.transform.rotation;
    // this.cameraTilt = Mathf.Clamp(this.cameraTilt - this.lookJoystick.axisY.axisValue * this.LookSensitivity * Time.deltaTime, -90f, 90f);
    // this.cameraTransform.localRotation = Quaternion.AngleAxis(this.cameraTilt, Vector3.right);

    // this.transform.localRotation = (isDebug ? Quaternion.Euler(debugGro) : Input.gyro.attitude) * new Quaternion(0, 0, 1, 0);
#if UNITY_EDITOR
    this.cameraTransform.localRotation = Quaternion.Slerp(this.cameraTransform.localRotation, Quaternion.Euler(debugGro), lowPassFilterFactor);
#else
    this.cameraTransform.localRotation = Quaternion.Slerp(this.cameraTransform.localRotation, Input.gyro.attitude * new Quaternion(0, 0, 1, 0), lowPassFilterFactor);
#endif
    // this.transform.localRotation = Quaternion.Euler(new Vector3(0, -this.v.y, 0));
    // this.cameraTransform.localRotation = Quaternion.Euler(new Vector3(90 - this.v.x, 0, 0));

    if (touchesGround)
    {
      this.timeInAir = 0;
    }
    else
    {
      this.timeInAir += Time.deltaTime;
    }

    if (touchesGround && this.verticalSpeed < 0)
    {
      this.verticalSpeed = 0;
    }
    else
    {
      this.verticalSpeed -= 9.18f * Time.deltaTime;
    }

    this.characterController.Move(Vector3.up * Time.deltaTime * this.verticalSpeed);

    if (Input.GetKeyDown(FlightController.OnOffKey))
    {
      var flyBehaviour = this.GetComponent<FlightController>();
      if (flyBehaviour != null)
      {
        this.GetComponent<FlightController>().enabled = true;
      }
      this.cameraTilt = 24;
    }
  }

  // void OnGUI()
  // {
  //   GUI.Label(new Rect(50, 100, 500, 20), "Label : " + this.v.x + "       " + this.v.y + "         " + this.v.z);
  // }

  public void Enable()
  {
    this.verticalSpeed = 0;
  }

  private bool onGround()
  {
    var ray = new Ray(this.transform.position, Vector3.down);
    return Physics.SphereCast(ray, this.characterController.radius, this.characterController.height / 2 - this.characterController.radius + 0.1f);
  }
}
