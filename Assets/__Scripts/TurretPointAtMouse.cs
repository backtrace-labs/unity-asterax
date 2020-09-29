//#define DEBUG_TurretPointAtMouse_DrawMousePoint

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretPointAtMouse : MonoBehaviour
{

#if DEBUG_TurretPointAtMouse_DrawMousePoint
    public bool         DrawMousePoint = false;
#endif

    private Vector3     mousePoint3D;


    // Update is called even when the game is paused, which I think will look 
    //  nice in the pause window.
    void Update()
    {
        PointAtMouse();
    }


    /// <summary>
    /// <para>Point the turretTransform at the current Input.mousePositon.</para>
    /// <para>Note: This will snap to position on mobile because the mouse is not always there</para>
    /// <para>Note: This only works for Cameras with a rotation of Quaternion.identity 
    ///  (i.e., Euler rotation of [0,0,0]) because it's attempting to project the mouse point
    ///  onto the z=0 plane.</para>
    /// </summary>
    void PointAtMouse()
    {
        mousePoint3D = Camera.main.ScreenToWorldPoint(Input.mousePosition
                          + Vector3.back * Camera.main.transform.position.z);
        // NOTE: To prevent snapping on mobile, could possibly add a Slerp over 
        //  a short time, but should still fire the shot immediately when the 
        //  player taps (i.e., not wait for the turret to be pointing in the 
        //  right direction.
        transform.LookAt(mousePoint3D, Vector3.back);
    }

#if DEBUG_TurretPointAtMouse_DrawMousePoint
    private void OnDrawGizmos()
    {
        if (DrawMousePoint && Application.isPlaying)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(mousePoint3D, 0.2f);
            Gizmos.DrawLine(transform.position, mousePoint3D);
        }
    }
#endif

   
}
