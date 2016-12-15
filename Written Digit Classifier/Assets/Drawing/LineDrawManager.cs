using UnityEngine;
using System.Collections;

public class LineDrawManager : MonoBehaviour {

    //the controller to draw with
    public SteamVR_TrackedObject trackedObj;

    //3d line drawing
 //   private CustomLineRenderer current3DLine;
    private LineRenderer current2DLine;

    public float width = .01f;
    public Material mat;

    int index = 0;

    public GameObject attach;

    // Update is called once per frame
    void Update() {

        var device = SteamVR_Controller.Input((int)trackedObj.index);

        //start drawing in 2d
        if(device.GetTouchDown(SteamVR_Controller.ButtonMask.Trigger) && Physics.OverlapSphere(attach.transform.position, .07f).Length > 0) {

            GameObject g = new GameObject();
            g.name = "Line2D";
            current2DLine = g.AddComponent<LineRenderer>();
            current2DLine.material = mat;
            current2DLine.startWidth = width;
            current2DLine.endWidth = width;
            index = 0;
        }


        //continue drawing in 2d
        else if(device.GetTouch(SteamVR_Controller.ButtonMask.Trigger) && Physics.OverlapSphere(attach.transform.position + new Vector3(0, 0, .06f), .03f).Length > 0) {
            device.TriggerHapticPulse();
            Vector3 v = trackedObj.transform.position;
            current2DLine.numPositions = index + 1;
            
            //might be a problem
            //current2DLine.SetVertexCount(index + 1);
            v.z = .76f;
            current2DLine.SetPosition(index, v);
            index++;

        }
    }
}


