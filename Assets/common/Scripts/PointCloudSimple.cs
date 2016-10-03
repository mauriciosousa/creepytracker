using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Point3DRGB {
	public Vector3 point;
	public Color color;
	
	public Point3DRGB(Vector3 point, Color color){
		this.point = point;
		this.color = color;
	}
}

public class PointCloudSimple : MonoBehaviour {
	Mesh[] highres_cloud;
    Mesh[] lowres_cloud;
    int highres_nclouds;
    int lowres_nclouds;
    int id;
    List <Point3DRGB> highres_points;
    List<Point3DRGB> lowres_points;

    public void setPoints(List<Point3DRGB> highres, List<Point3DRGB> lowres, int newid){
		if (newid != id) {
            highres_points.Clear();
            lowres_points.Clear();
            id = newid;
		}
        highres_points.AddRange (highres); // TMA: Store the high resolution points.
        lowres_points.AddRange(lowres); // Store the low resolution points.
        List<Vector3> points = new List<Vector3> ();
		List<int> ind = new List<int> ();
		List<Color> colors = new List<Color> ();
        // Create 4 Mesh for each quality
        highres_cloud = new Mesh[4]; 
        lowres_cloud = new Mesh[4];

        // Add points to each Mesh from the points' arrays

        // High Resolution
        highres_nclouds = 0;
		Mesh mh = new Mesh ();
		int i = 0;
		foreach(Point3DRGB p in highres_points)
        {
            points.Add(p.point);
			colors.Add(p.color);
			ind.Add (i);
			i++;
			if(i == 65000){
				mh.vertices = points.ToArray ();
				mh.colors = colors.ToArray ();
				mh.SetIndices (ind.ToArray(), MeshTopology.Points, 0);
				highres_cloud[highres_nclouds] = mh;
				mh = new Mesh();
				i = 0;
				points.Clear();
				colors.Clear();
				ind.Clear();
                highres_nclouds++;
            }
		}
        mh.vertices = points.ToArray ();
		mh.colors = colors.ToArray ();
		mh.SetIndices (ind.ToArray(), MeshTopology.Points, 0);
		highres_cloud[highres_nclouds] = mh;

        points.Clear();
        colors.Clear();
        ind.Clear();

        //Low Resolution
        lowres_nclouds = 0;
        Mesh ml = new Mesh();
        i = 0;
        foreach (Point3DRGB p in lowres_points)
        {
            points.Add(p.point);
            colors.Add(p.color);
            ind.Add(i);
            i++;
            if (i == 65000)
            {
                ml.vertices = points.ToArray();
                ml.colors = colors.ToArray();
                ml.SetIndices(ind.ToArray(), MeshTopology.Points, 0);
                lowres_cloud[lowres_nclouds] = ml;
                ml = new Mesh();
                i = 0;
                points.Clear();
                colors.Clear();
                ind.Clear();
                lowres_nclouds++;
            }
        }
        ml.vertices = points.ToArray();
        ml.colors = colors.ToArray();
        ml.SetIndices(ind.ToArray(), MeshTopology.Points, 0);
        lowres_cloud[lowres_nclouds] = ml;
    }

	public void setToView(){
		MeshFilter[] filters = GetComponentsInChildren<MeshFilter> ();
        // Note that there are 8 MeshFilter -> [HR HR HR HR LR LR LR LR]
        lowres_nclouds += 4;  // Therefore, the low resolution clouds start at index 4
        for (int i = 0; i < filters.Length; i++) {
            MeshFilter mf = filters[i];
            if (i <= highres_nclouds)
            {
				mf.mesh = highres_cloud[i];
            }
            else if (i <= lowres_nclouds && i >= 4)
            {
                mf.mesh = lowres_cloud[i - 4];
            }
            else
            {
				mf.mesh.Clear();
            }
		}
    }

	public void hideFromView(){
		MeshFilter[] filters = GetComponentsInChildren<MeshFilter> ();
		foreach (MeshFilter mf in filters) {
			mf.mesh.Clear();
		}	
	}

	// Use this for initialization
	void Start () {
        // Material for the high resolution points
        Material mat = Resources.Load("Materials/cloudmat") as Material;
        // Material for the low resolution points
        Material other = Instantiate(mat) as Material;

        // Update size for each material.
        mat.SetFloat("_Size", 0.02f);  // HR
        other.SetFloat("_Size", 0.055f); // LR

        for (int i = 0; i < 4; i++) {
			GameObject a = new GameObject("highres_cloud" + i);
			MeshFilter mf = a.AddComponent<MeshFilter>();
			MeshRenderer mr = a.AddComponent<MeshRenderer>();
			mr.material = mat;
			a.transform.parent = this.gameObject.transform;
			a.transform.localPosition = Vector3.zero;
			a.transform.localRotation = Quaternion.identity;
			a.transform.localScale = new Vector3 (1, 1, 1);
		}
        for (int i = 0; i < 4; i++)
        {
            GameObject a = new GameObject("lowres_cloud" + i);
            MeshFilter mf = a.AddComponent<MeshFilter>();
            MeshRenderer mr = a.AddComponent<MeshRenderer>();
            mr.material = other;
            a.transform.parent = this.gameObject.transform;
            a.transform.localPosition = Vector3.zero;
            a.transform.localRotation = Quaternion.identity;
            a.transform.localScale = new Vector3(1, 1, 1);
        }
        highres_points = new List<Point3DRGB> ();
        lowres_points = new List<Point3DRGB>();
    }

}