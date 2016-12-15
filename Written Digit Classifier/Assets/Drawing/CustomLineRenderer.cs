using UnityEngine;
using System.Collections;
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class CustomLineRenderer : MonoBehaviour {

    public Material lmat;
    private Mesh ml;
    private Vector3 s;
    private float lineSize = .1f;
    private bool firstQ = true;



    // Use this for initialization
    void Start() {
        ml = GetComponent<MeshFilter>().mesh;
        GetComponent<MeshRenderer>().material = lmat;
    }

    public void addPt(Vector3 pt) {
        if(s != Vector3.zero) {
            addLine(ml, makeQuad(s, pt, lineSize, firstQ));
            firstQ = false;
        }
        s = pt;
    }


    Vector3[] makeQuad(Vector3 s, Vector3 e, float w, bool all) {
        w = w / 2;
        Vector3[] q;

        if(all)
            q = new Vector3[4];
        else
            q = new Vector3[2];

        Vector3 n = Vector3.Cross(s, e);
        Vector3 l = Vector3.Cross(n, e - s);
        l.Normalize();

        if(all) {
            q[0] = transform.InverseTransformPoint(s + l * w);
            q[1] = transform.InverseTransformPoint(s + l * -w);
            q[2] = transform.InverseTransformPoint(e + l * w);
            q[3] = transform.InverseTransformPoint(e + l * -w);
        }
        else {
            q[0] = transform.InverseTransformPoint(e + l * w);
            q[1] = transform.InverseTransformPoint(e + l * -w);
        }
        return q;
    }

    void addLine(Mesh m, Vector3[] quad) {
        int v1 = m.vertices.Length;

        Vector3[] vs = m.vertices;

        vs = resizeVerts(vs, 2 * quad.Length);

        for(int i = 0; i < 2 * quad.Length; i += 2) {
            vs[v1 + i] = quad[i / 2];
            vs[v1 + i + 1] = quad[i / 2];
        }

        Vector2[] uv = m.uv;

        uv = resizeUVs(uv, 2 * quad.Length);

        if(quad.Length == 4) {
            uv[v1] = Vector2.zero;
            uv[v1 + 1] = Vector2.zero;
            uv[v1 + 2] = Vector2.right;
            uv[v1 + 3] = Vector2.right;
            uv[v1 + 4] = Vector2.up;
            uv[v1 + 5] = Vector2.up;
            uv[v1 + 6] = Vector2.one;
            uv[v1 + 7] = Vector2.one;
        }
        else {
            if(v1 % 8 == 0) {
                uv[v1] = Vector2.zero;
                uv[v1 + 1] = Vector2.zero;
                uv[v1 + 2] = Vector2.right;
                uv[v1 + 3] = Vector2.right;
            }
            else {
                uv[v1] = Vector2.up;
                uv[v1 + 1] = Vector2.up;
                uv[v1 + 2] = Vector2.one;
                uv[v1 + 3] = Vector2.one;
            }
        }

        int t1 = m.triangles.Length;

        int[] ts = m.triangles;
        ts = resizeTris(ts, 12);

        if(quad.Length == 2)
            v1 -= 4;

        ts[t1] = v1;
        ts[t1 + 1] = v1 + 2;
        ts[t1 + 2] = v1 + 4;

        ts[t1 + 3] = v1 + 2;
        ts[t1 + 4] = v1 + 6;
        ts[t1 + 5] = v1 + 4;

        ts[t1 + 6] = v1 + 5;
        ts[t1 + 7] = v1 + 3;
        ts[t1 + 8] = v1 + 1;

        ts[t1 + 9] = v1 + 5;
        ts[t1 + 10] = v1 + 7;
        ts[t1 + 11] = v1 + 3;

        m.vertices = vs;
        m.uv = uv;
        m.triangles = ts;
        m.RecalculateBounds();
        m.RecalculateNormals();
    }

    Vector2[] resizeUVs(Vector2[] uvs, int ns) {
        Vector2[] nvs = new Vector2[uvs.Length + ns];
        for(int i = 0; i < uvs.Length; i++) {
            nvs[i] = uvs[i];
        }
        return nvs;
    }
    int[] resizeTris(int[] ovs, int ns) {
        int [] nvs = new int[ovs.Length + ns];
        for(int i = 0; i < ovs.Length; i++) {
            nvs[i] = ovs[i];
        }
        return nvs;
    }
    Vector3[] resizeVerts(Vector3[] ovs, int ns) {
        Vector3[] nvs = new Vector3[ovs.Length + ns];
        for(int i = 0; i < ovs.Length; i++) {
            nvs[i] = ovs[i];
        }
        return nvs;
    }

    public void setWidth(float width) {
        lineSize = width;
    }
}
