using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LinHowe.WaterRender;

public class TestForceToWater : MonoBehaviour {

    
    public WaterSurface water;//水面
    public float swipeSize;
    public Mesh swipeMesh;
    public float height,force;
    private bool IsBeginDrag;
    private Camera MainCamera;
    private void Start()
    {
        MainCamera = GameObject.FindGameObjectWithTag("MainCamera")
            .GetComponent<Camera>();
    }
    private void Update()
    {
        Shader.SetGlobalFloat("internal_Force", force);

        if (Input.GetMouseButton(0))
        {
            Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (!IsBeginDrag)
            {
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.gameObject == water.gameObject)
                    {
                        float t = (height -
                            Vector3.Dot(ray.origin,Vector3.up)) /
                            Vector3.Dot(ray.direction, Vector3.up);
                        Vector3 hitpos = ray.origin + ray.direction * t;
                        Matrix4x4 matrix = Matrix4x4.TRS(hitpos, Quaternion.identity, Vector3.one * swipeSize);
                        if(water&&water.enabled) water.DrawMesh(swipeMesh, matrix);
                    }
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            IsBeginDrag = false;
        }
    }
}
