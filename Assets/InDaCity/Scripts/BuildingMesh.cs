using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace InDaCity
{
    public class BuildingMesh : MonoBehaviour
    {
        private List<Vector3> vertices = new List<Vector3>();
        private List<int> triangles = new List<int>();

		private void Clear()
		{
			vertices.Clear ();
            triangles.Clear ();
		}

		private Vector3 MakeV3(Vector2 v, float height)
		{
			return new Vector3 (v.x, height, v.y);
		}
    
        private void PutVertexPair(Vector2 v, float height)
        {
            vertices.Add(MakeV3(v, 0));
            vertices.Add(MakeV3(v, height));
        }

		public Mesh MakeBuilding (Vector2[] polygon, float height)
		{
			Debug.Assert (polygon.Length >= 3);
			Debug.Assert (height > 0.0f);

			Clear ();

			Mesh mesh = new Mesh ();

			mesh.Clear ();

            // sides
            foreach (var v in polygon)
                PutVertexPair(v, height);
            
            for (int i = 0; i < polygon.Length; ++i)
            {
                var nextI = (i + 1) % polygon.Length;
                var j0 = i * 2;
                var j1 = nextI * 2;

                triangles.Add(j1);
                triangles.Add(j0 + 1);
                triangles.Add(j0);

                triangles.Add(j1 + 1);
                triangles.Add(j0 + 1);
                triangles.Add(j1);
            }

            // top
            foreach (var v in polygon)
                vertices.Add(MakeV3(v, height));

            var topStart = polygon.Length * 2;
            for (int i = topStart + 1; i < polygon.Length + topStart - 1; i ++)
            {
                triangles.Add(topStart);
                triangles.Add(i);
                triangles.Add(i + 1);
            }

            mesh.vertices = vertices.ToArray ();
			mesh.triangles = triangles.ToArray ();

			mesh.RecalculateNormals ();
			mesh.RecalculateBounds ();
			mesh.Optimize ();

			return mesh;
		}

		private Vector2[] ShiftBasement(Vector2[] basement, Vector2 shift)
		{
			for (int i = 0; i < basement.Length; ++i)
				basement [i] += shift;
			return basement;
		}

		public Vector2[] SampleBasement(Vector2 shift)
		{
			Vector2[] polygon = {
				new Vector2(0, -1),
				new Vector2(-1, -1),
				new Vector2(-2, 0),
				new Vector2(0, 2),
				new Vector2(2, 0)
			};

			return ShiftBasement(polygon, shift);
		}


		public void Build(Polygon polygon, float height)
		{
			// make a mesh
			var meshFilter = GetComponent<MeshFilter> ();

            var basement = polygon.Vertices.ToArray();
            basement = ShiftBasement (basement, -polygon.center);
			meshFilter.mesh = MakeBuilding (basement, height);
		}
	}
}