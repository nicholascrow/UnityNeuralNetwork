using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LineDrawManager : MonoBehaviour {

    //the controller to draw with
    public SteamVR_TrackedObject trackedObj;

    //3d line drawing
 //   private CustomLineRenderer current3DLine;
    private LineRenderer current2DLine;

    public float width = 3;
    public Material mat;

    int index = 0;

    public GameObject attach;
    public List<GameObject> lines;

    void Start() {
        lines = new List<GameObject>();
    }

    // Update is called once per frame
    void Update() {

        var device = SteamVR_Controller.Input((int)trackedObj.index);
        if(device.GetTouchDown(SteamVR_Controller.ButtonMask.ApplicationMenu)) {
            foreach(var item in lines) {
                Destroy(item);
            }
        }
        //start drawing in 2d
        if(device.GetTouchDown(SteamVR_Controller.ButtonMask.Trigger) && Physics.OverlapSphere(attach.transform.position, .1f).Length > 0) {
           // print("here");
            GameObject g = new GameObject();
            g.name = "Line2D";
            current2DLine = g.AddComponent<LineRenderer>();
            current2DLine.material = mat;
            current2DLine.SetWidth(width,width);
          
            current2DLine.startWidth = .05f;
            current2DLine.endWidth = .05f;
            index = 0;
            lines.Add(g);
            print(current2DLine.startWidth);
        }


        //continue drawing in 2d
        else if(device.GetTouch(SteamVR_Controller.ButtonMask.Trigger) && Physics.OverlapSphere(attach.transform.position + new Vector3(0, 0, .06f), .03f).Length > 0 && current2DLine != null) {
            device.TriggerHapticPulse();
            Vector3 v = trackedObj.transform.position;
           // current2DLine.numPositions = index + 1;
            
            //might be a problem
            current2DLine.SetVertexCount(index + 1);
            v.z = .807f;
            current2DLine.SetPosition(index, v);
            index++;
           
        }
    }
}


