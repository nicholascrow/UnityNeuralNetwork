using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTexturePainter : MonoBehaviour {

    public SteamVR_TrackedObject trackedObj;

    public TestBMP tester;

    public Texture2D whitboardBak;
    public Texture2D whiteboardTex;

    void Awake() {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    // Update is called once per frame
    void Update () {
        Debug.DrawLine(transform.position, transform.forward,Color.red);
        var device = SteamVR_Controller.Input((int)trackedObj.index);
        if(device.GetTouchDown(SteamVR_Controller.ButtonMask.Trigger) || device.GetTouch(SteamVR_Controller.ButtonMask.Trigger)) {
            RaycastHit hit;
            Ray raydirection = new Ray(transform.position, transform.forward);

            if(!Physics.Raycast(raydirection, out hit, 10f))
                return;

            Renderer rend = hit.transform.GetComponent<Renderer>();
            MeshCollider meshCollider = hit.collider as MeshCollider;
            if(rend == null || rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null || meshCollider == null)
                return;

            Texture2D tex = rend.material.mainTexture as Texture2D;
            Vector2 pixelUV = hit.textureCoord;
            pixelUV.x *= tex.width;
            pixelUV.y *= tex.height;
            int size = 30;
            for(int i = 0; i < size; i++) {
                for(int j = 0; j < size; j++) {
                    tex.SetPixel((int)pixelUV.x + i, (int)pixelUV.y + j, Color.black);
                    /*  tex.SetPixel((int)pixelUV.x - i/2, (int)pixelUV.y-j/2, Color.black);
                        tex.SetPixel((int)pixelUV.x + i/2, (int)pixelUV.y + j/2, Color.black);
                        tex.SetPixel((int)pixelUV.x, (int)pixelUV.y, Color.black);*/

                }

            }

            tex.Apply();
        }
        else if(device.GetTouchDown(SteamVR_Controller.ButtonMask.ApplicationMenu) || tester.ClearCanvas) {
            whiteboardTex.SetPixels(whitboardBak.GetPixels());
            whiteboardTex.Apply();
           tester.ClearCanvas = false;
        }
    }
}
