using UnityEngine;

/// <summary>
/// When a GameObject exits the bounds of the OnScreenBounds, screen wrap it.
/// </summary>
[RequireComponent(typeof(Collider))]
public class OffScreenWrapper : MonoBehaviour
{

    void Start()
    {
        //Debug.Log("OffScreenWrapper Start");   
    }

    void Update()
    {
        // Adding an Update() method shows the "enabled" checkbox in the Inspector
    }

    void OnTriggerEnter(Collider other)
    {
        //Debug.Log("OnTriggerEnter - " + gameObject.name);
    }

    // This is called whenever this GameObject exits the bounds of OnScreenBounds
    void OnTriggerExit(Collider other)
    {
        //Debug.Log("OnTriggerExit");
        // NOTE: OnTriggerExit is still called when this.enabled==false, so I've
        //  added this to stop wrapping when enabled is unchecked in the Inspector
        if (!enabled)
        {
            return;
        }

        // Ensure that the other is OnScreenBounds
        ScreenBounds bounds = other.GetComponent<ScreenBounds>();
        if (bounds == null)
        {
            return;
        }

        ScreenWrap(bounds);

#if DEBUG_AnnounceOnTriggerExit
        // GetComponent is pretty slow, but because this is in a debug test case 
        //  and only happens once every few seconds, it's okay here.
		if (GetComponent<Asteroid>() != null)
        {
    		Debug.LogWarning(gameObject.name+" OnTriggerExit "+Time.time);
		}
#endif
    }


    /// <summary>
    /// Wraps this object to the other side of the screen when it has exited the 
    ///  OnScreenBounds BoxCollider.<para/><para/>
    /// </summary>
    /// <param name="bounds">A reference to the ScreenBounds.</param>
    private void ScreenWrap(ScreenBounds bounds) {

        // Wrap whichever direction is necessary
        Vector3 relativeLoc = bounds.transform.InverseTransformPoint(transform.position);
        // relativeLoc is in the local coords of OnScreenBounds, 0.5f is the screen edge.
        if (Mathf.Abs(relativeLoc.x) > 0.5f)
        {
            relativeLoc.x *= -1;
        }
        if (Mathf.Abs(relativeLoc.y) > 0.5f)
        {
            relativeLoc.y *= -1;
        }

        transform.position = bounds.transform.TransformPoint(relativeLoc);

        CalculateOverrideCoordinates(relativeLoc.x);
    }                

    void CalculateOverrideCoordinates(float x)
    {
        if (Mathf.Abs(x) > 0.5f)
        {
            // left the screen on the left or right
            CompensateForGyroscopeDrift(x);
        }
    }

    void CompensateForGyroscopeDrift(float x)
    {
        if (Mathf.Abs(x) > 0.5f)
        {
            PlayerShip.GyroscopeDelta();
        }
    }    

}