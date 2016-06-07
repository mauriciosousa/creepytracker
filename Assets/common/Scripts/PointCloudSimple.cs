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
	Mesh[] cloud;
	int nclouds;
	int id;
	List <Point3DRGB> allpoints;

	public void setPoints(List<Point3DRGB> input, int newid){

		if (newid != id) {
			allpoints.Clear();
			id = newid;
		}
		allpoints.AddRange (input);
		List<Vector3> points = new List<Vector3> ();
		List<int> ind = new List<int> ();
		List<Color> colors = new List<Color> ();
		cloud = new Mesh[4];
			
		nclouds = 0;
		Mesh m = new Mesh ();
		int i = 0;
		foreach(Point3DRGB p in allpoints){
			points.Add(p.point);
			colors.Add(p.color);
			ind.Add (i);
			i++;
			if(i == 65000){
				m.vertices = points.ToArray ();
				m.colors = colors.ToArray ();
				m.SetIndices (ind.ToArray(), MeshTopology.Points, 0);
				cloud[nclouds] = m;
				m = new Mesh();
				i = 0;
				points.Clear();
				colors.Clear();
				ind.Clear();
				nclouds++;
			}
		}
		
		m.vertices = points.ToArray ();
		m.colors = colors.ToArray ();
		m.SetIndices (ind.ToArray(), MeshTopology.Points, 0);
		cloud[nclouds] = m;
	}

	public void setToView(){
		MeshFilter[] filters = GetComponentsInChildren<MeshFilter> ();
		int i = 0;
		foreach (MeshFilter mf in filters) {
			if(i <= nclouds){
				mf.mesh = cloud[i++];
			}else{
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
		Material mat = Resources.Load ("Materials/cloudmat") as Material;
		for (int i = 0; i < 4; i++) {
			GameObject a = new GameObject("cloud" + i);
			MeshFilter mf = a.AddComponent<MeshFilter>();
			MeshRenderer mr = a.AddComponent<MeshRenderer>();
			mr.material = mat;
			a.transform.parent = this.gameObject.transform;
			a.transform.localPosition = Vector3.zero;
			a.transform.localRotation = Quaternion.identity;
			a.transform.localScale = new Vector3 (1, 1, 1);
		}
		allpoints = new List<Point3DRGB> ();
	}

}