using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace mattatz.Utils {

    public class MeshDisperser {

        public static Mesh Disperse (Mesh mesh) {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uv = new List<Vector2>();
            var uv2 = new List<Vector2>();
            var triangles = new List<int>();

            int count = 0;
            int all = mesh.triangles.Length * 3;

            for(int i = 0, n = mesh.triangles.Length; i < n; i += 3) {
                int a = mesh.triangles[i + 0];
                int b = mesh.triangles[i + 1];
                int c = mesh.triangles[i + 2];

                vertices.Add(mesh.vertices[a]);
                vertices.Add(mesh.vertices[b]);
                vertices.Add(mesh.vertices[c]);

                var ba = mesh.vertices[b] - mesh.vertices[a];
                var cb = mesh.vertices[c] - mesh.vertices[b];

                var normal = Vector3.Cross(ba, cb);
                normals.Add(normal);
                normals.Add(normal);
                normals.Add(normal);

                float cur = (float)count / all;

                triangles.Add(count++);
                triangles.Add(count++);
                triangles.Add(count++);

                uv.Add(mesh.uv[a]);
                uv.Add(mesh.uv[b]);
                uv.Add(mesh.uv[c]);

                float v = Random.value;
                Vector2 seed = new Vector2(cur, 0f);
                uv2.Add(seed);
                uv2.Add(seed);
                uv2.Add(seed);
            }

            var newMesh = new Mesh();
            newMesh.vertices = vertices.ToArray();
            newMesh.normals = normals.ToArray();
            newMesh.uv = uv.ToArray();
            newMesh.uv2 = uv2.ToArray();
            newMesh.triangles = triangles.ToArray();

            return newMesh;
        }

    }

}


