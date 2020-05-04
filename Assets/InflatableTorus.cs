using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class InflatableTorus {

    public readonly int SEGMENTS_ALONG_CIRCLE = 80;

    public readonly int SEGMENTS_ACROSS_CIRCLE = 26;

    public readonly float CENTRAL_RADIUS = 0.5f;
    public float OUTER_RADIUS = 0.28f;
    public float INNER_RADIUS = 0.25f;

    public bool ApplyNoize = true;

    public float PerlinFreqScale = 20f;

    

    MeshRenderer _mr;
    MeshFilter _mf;
    Mesh _me;

    internal Vector3[] _vert;
    
    Vector2[] _uv;


    Vector2 _noiseShift;

    public void Init(MeshRenderer mr, MeshFilter mf) {

        _noiseShift = new Vector2(UnityEngine.Random.value * 100, UnityEngine.Random.value * 100);

        _mr = mr;
        _mf = mf;

        _me = new Mesh();

        /*Color[] texels = new Color[500 * 500];
        for (int i = 0; i < texels.Length; i++) {
            texels[i] = (i< 500 * 250)? Color.blue:Color.black;
        }

        _mr.material.mainTexture = new Texture2D(500, 500);
        (_mr.material.mainTexture as Texture2D).SetPixels(texels);
        (_mr.material.mainTexture as Texture2D).Apply();*/


        //mesh construction
        _vert = new Vector3[(SEGMENTS_ALONG_CIRCLE) * (SEGMENTS_ACROSS_CIRCLE)];
        _uv = new Vector2[(SEGMENTS_ALONG_CIRCLE) * (SEGMENTS_ACROSS_CIRCLE)];
        for (int i = 0; i < _uv.Length; i++) {
            _uv[i] = new Vector2();
        }


        int[] tri = new int[(SEGMENTS_ALONG_CIRCLE) * (SEGMENTS_ACROSS_CIRCLE) * 2 * 3];

        int quadIndex = 0;

        for (int i = 0; i < SEGMENTS_ALONG_CIRCLE; i++) {
            for (int j = 0; j < SEGMENTS_ACROSS_CIRCLE; j++) {

                int baseVertIndex = (i * (SEGMENTS_ACROSS_CIRCLE) + j);
                int baseTriIndex = quadIndex * 6;

                tri[baseTriIndex + 0] = baseVertIndex;
                tri[baseTriIndex + 1] = baseVertIndex - 1;
                tri[baseTriIndex + 2] = baseVertIndex - 1 - (SEGMENTS_ACROSS_CIRCLE);
                tri[baseTriIndex + 3] = baseVertIndex - 1 - (SEGMENTS_ACROSS_CIRCLE);
                tri[baseTriIndex + 4] = baseVertIndex - (SEGMENTS_ACROSS_CIRCLE);
                tri[baseTriIndex + 5] = baseVertIndex;

                if (i == 0) {

                    tri[baseTriIndex + 2] += _vert.Length;
                    tri[baseTriIndex + 3] += _vert.Length;
                    tri[baseTriIndex + 4] += _vert.Length;
                }

                if (j == 0) {

                    tri[baseTriIndex + 1] += SEGMENTS_ACROSS_CIRCLE;
                    tri[baseTriIndex + 2] += SEGMENTS_ACROSS_CIRCLE;
                    tri[baseTriIndex + 3] += SEGMENTS_ACROSS_CIRCLE;
                }



                quadIndex++;

            }
        }

        _me.vertices = _vert;
        _me.triangles = tri;

        _mf.mesh = _me;
    }


    public void UpdateVertexInterpolation(float[] interpolationData) {
        for (int i = 0; i < SEGMENTS_ALONG_CIRCLE; i++) {
            for (int j = 0; j < SEGMENTS_ACROSS_CIRCLE; j++) {

                SetVertex(i, j, interpolationData[(i * (SEGMENTS_ACROSS_CIRCLE) + j)]);

            }
        }

        FinlizeMesh();
    }

    private void SetVertex(int i, int j, float progress) {
        float alongAng = Mathf.PI * 2 * (float)i / (float)SEGMENTS_ALONG_CIRCLE;

        float acrossAng = -Mathf.PI * 2 * (float)j / (float)SEGMENTS_ACROSS_CIRCLE; ;

        Vector3 sliceCenter = new Vector3(Mathf.Cos(alongAng), 0, Mathf.Sin(alongAng)) * CENTRAL_RADIUS;
        Vector3 hugingPoint = new Vector3(Mathf.Sin(acrossAng), Mathf.Cos(acrossAng), 0);

        hugingPoint *= INNER_RADIUS + (OUTER_RADIUS - INNER_RADIUS) * progress;


        //rotate the hugging circle according to slice
        hugingPoint = Quaternion.Euler(0, -360 * alongAng / (Mathf.PI * 2), 0) * hugingPoint;


        if (ApplyNoize) {
            float perlin = Mathf.PerlinNoise(PerlinFreqScale * ((sliceCenter + hugingPoint).x + _noiseShift.x), PerlinFreqScale * ((sliceCenter + hugingPoint).z + +_noiseShift.y));

            /*if (perlin < 0.49f || perlin > 0.51f) {
                perlin = 0;
            } else {
                perlin = -0.05f;
            }*/


            hugingPoint *= 1 + perlin * 0.05f;
        }
        


        _vert[(i * (SEGMENTS_ACROSS_CIRCLE) + j)] = sliceCenter + hugingPoint;

        _uv[(i * (SEGMENTS_ACROSS_CIRCLE) + j)] = new Vector2(_vert[(i * (SEGMENTS_ACROSS_CIRCLE) + j)].x, _vert[(i * (SEGMENTS_ACROSS_CIRCLE) + j)].z) * 0.6f + new Vector2(0.5f, 0.5f);
    }

    private void FinlizeMesh() {
        _me.vertices = _vert;
        _me.uv = _uv;
        _me.RecalculateNormals();
        _me.RecalculateBounds();
        _me.RecalculateTangents();
    }
}
