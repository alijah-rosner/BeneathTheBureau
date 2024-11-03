using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hud : MonoBehaviour
{
  [Header("Components")]
  [SerializeField] private MouseFlightController mouseFlight = null;

  [Header("HUD Elements")]
  [SerializeField] private RectTransform boresight = null;
  [SerializeField] private RectTransform mousePos = null;

  private Camera playerCam = null;

  private void Awake()
  {
    if (mouseFlight == null)
      Debug.LogError(name + ": Hud - Mouse Flight Controller not assigned!");

    playerCam = mouseFlight.GetComponentInChildren<Camera>();

    if (playerCam == null && mouseFlight._isEnabled)
      Debug.LogError(name + ": Hud - No camera found on assigned Mouse Flight Controller!");
  }

  private void Update()
  {
    if (mouseFlight == null || playerCam == null)
      return;

    UpdateGraphics(mouseFlight);
  }

  private void UpdateGraphics(MouseFlightController controller)
  {
    if (boresight != null)
    {
      if (GameObject.FindObjectOfType<FlightController>())
      {
        Vector3 pos = GameObject.FindObjectOfType<FlightController>().transform.forward + GameObject.FindObjectOfType<FlightController>().transform.position;
        boresight.position = playerCam.WorldToScreenPoint(pos);

      }
      else
      {

        boresight.position = playerCam.WorldToScreenPoint(controller.BoresightPos);
        boresight.gameObject.SetActive(boresight.position.z > 1f);
      }
    }

    if (mousePos != null)
    {
      mousePos.position = playerCam.WorldToScreenPoint(controller.MouseAimPos);
      mousePos.gameObject.SetActive(mousePos.position.z > 1f);
    }
  }

  public void SetReferenceMouseFlight(MouseFlightController controller)
  {
    mouseFlight = controller;
  }
}
