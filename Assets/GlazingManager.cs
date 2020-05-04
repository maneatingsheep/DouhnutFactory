using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlazingManager : ActivityBaseManager
{

    MeshRenderer _mr;
    MeshFilter _mf;
    Mesh _me;

    Vector3[] _vert;

    GlazingSpread[] _vertexSpread;
    GlazingSpread[] _glazePointSpread;


    const int GLAZE_POINTS_ALONG = 30;
    const int SEGMENTS_PER_GLAZEPOINT = 6;
    int SEGMENTS_ALONG_CIRCLE = 120;

    const int SEGMENTS_ACROSS_CIRCLE = 20;

    const float CENTRAL_RADIUS = 0.5f;
    const float LARGE_RADIUS = 0.28f;
    const float SMALL_RADIUS = 0.24f;


    private float _perlinFreqScale = 15f;
    private float _perlinAmpScale = 0.08f;

    public TorusController MainCont;

    public void Init()
    {

        SEGMENTS_ALONG_CIRCLE = GLAZE_POINTS_ALONG * SEGMENTS_PER_GLAZEPOINT;

        _mr = GetComponent<MeshRenderer>();
        _mf = GetComponent<MeshFilter>();

        _me = new Mesh();

        //glazing data
        _glazePointSpread = new GlazingSpread[GLAZE_POINTS_ALONG];
        for (int i = 0; i < GLAZE_POINTS_ALONG; i++) {
            _glazePointSpread[i] = new GlazingSpread();
        }

        _vertexSpread = new GlazingSpread[SEGMENTS_ALONG_CIRCLE];
        for (int i = 0; i < SEGMENTS_ALONG_CIRCLE; i++) {
            _vertexSpread[i] = new GlazingSpread();
        }


        //mesh construction
        _vert = new Vector3[(SEGMENTS_ALONG_CIRCLE) * (SEGMENTS_ACROSS_CIRCLE + 3)];

        int[] tri = new int[(SEGMENTS_ALONG_CIRCLE) * (SEGMENTS_ACROSS_CIRCLE + 2) * 2 * 3];

        int quadIndex = 0;

        for (int i = 0; i < SEGMENTS_ALONG_CIRCLE; i++) {
            for (int j = 1; j < SEGMENTS_ACROSS_CIRCLE + 3; j++) {
                
                int baseVertIndex = (i * (SEGMENTS_ACROSS_CIRCLE + 3) + j);
                int baseTriIndex = quadIndex * 6;

                tri[baseTriIndex + 0] = baseVertIndex;
                tri[baseTriIndex + 1] = baseVertIndex - 1;
                tri[baseTriIndex + 2] = baseVertIndex - 1 - (SEGMENTS_ACROSS_CIRCLE + 3);
                tri[baseTriIndex + 3] = baseVertIndex - 1 - (SEGMENTS_ACROSS_CIRCLE + 3);
                tri[baseTriIndex + 4] = baseVertIndex - (SEGMENTS_ACROSS_CIRCLE + 3);
                tri[baseTriIndex + 5] = baseVertIndex;

                if (i == 0) {

                    tri[baseTriIndex + 2] += _vert.Length;
                    tri[baseTriIndex + 3] += _vert.Length;
                    tri[baseTriIndex + 4] += _vert.Length;
                }

                quadIndex++;

            }
        }

        _me.vertices = _vert;
        _me.triangles = tri;

        _mf.mesh = _me;
    }

    internal override void ResetAll() {

        base.ResetAll();

        for (int i = 0; i < GLAZE_POINTS_ALONG; i++) {
            _glazePointSpread[i].Reset();
        }
        for (int i = 0; i < SEGMENTS_ACROSS_CIRCLE; i++) {
            _vertexSpread[i].Reset();
        }

    }

    public override void SetActivityState(ActivityStateType activityState) {
        if (activityState != _activityState) {
            MainCont.isSpinning = activityState == ActivityStateType.Active;
        }
        
        base.SetActivityState(activityState);

    }



    void Update() {

        for (int i = 0; i < GLAZE_POINTS_ALONG; i++) {
            if (/*_isInteractive*/ _activityState == ActivityStateType.Active) {

                float alongAng = Mathf.PI * 2 * ((float)i / (float)GLAZE_POINTS_ALONG);

                Vector3 sliceCenter = new Vector3(Mathf.Cos(alongAng), 0, Mathf.Sin(alongAng)) * (CENTRAL_RADIUS);


                if (transform.TransformPoint(sliceCenter).z < -0.48f) {
                    _glazePointSpread[i].AddGlaze();
                }

            }

            _glazePointSpread[i].Update();
        }

        if (_activityState == ActivityStateType.Active) {
            ActionProgress += Time.deltaTime / 3f;
        }

        UpdateMesh();

        

    }

    private void UpdateMesh() {
        //interpolate glaze points to segmentPoints
        for (int i = 0; i < GLAZE_POINTS_ALONG; i++) {
            for (int j = 0; j < SEGMENTS_PER_GLAZEPOINT; j++) {
                _vertexSpread[i * SEGMENTS_PER_GLAZEPOINT + j].Interpolate(_glazePointSpread[((i == 0) ? GLAZE_POINTS_ALONG : i) - 1], _glazePointSpread[i], j / (float)SEGMENTS_PER_GLAZEPOINT);
            }
        }

        for (int i = 0; i < SEGMENTS_ALONG_CIRCLE; i++) {
            for (int j = 0; j < SEGMENTS_ACROSS_CIRCLE + 3; j++) {
                float alongAng = Mathf.PI * 2 * (float)i / (float)SEGMENTS_ALONG_CIRCLE;

                int relativeAcrossIndex = Math.Min(j - 1, SEGMENTS_ACROSS_CIRCLE + 1); //takes care of edges

                float acrossAng = 0;

                float circle = Mathf.PI * 2;


                //progress is from -0.5 to 0.5
                float progress = ((float)relativeAcrossIndex - ((SEGMENTS_ACROSS_CIRCLE + 1) / 2f)) / (float)(SEGMENTS_ACROSS_CIRCLE + 1);

                bool hide = false;

                if (progress > 0) {
                    acrossAng = -circle * progress * _vertexSpread[i].InnerSpread;
                    hide |= _vertexSpread[i].InnerSpread == 0;
                } else {
                    acrossAng = -circle * progress * _vertexSpread[i].OuterSpread;
                    hide |= _vertexSpread[i].OuterSpread == 0;

                }



                Vector3 sliceCenter = new Vector3(Mathf.Cos(alongAng), 0, Mathf.Sin(alongAng)) * CENTRAL_RADIUS;
                Vector3 hugingPoint = new Vector3(Mathf.Sin(acrossAng), Mathf.Cos(acrossAng), 0);

                if (hide) {
                    hugingPoint *= SMALL_RADIUS - 0.1f;
                } else {
                    if (j == 0 || j == SEGMENTS_ACROSS_CIRCLE + 3 - 1) {
                        hugingPoint *= SMALL_RADIUS;
                    } else {
                        hugingPoint *= LARGE_RADIUS;
                    }
                }


                //rotate the hugging circle according to slice
                hugingPoint = Quaternion.Euler(0, -360 * alongAng / (Mathf.PI * 2), 0) * hugingPoint;

                hugingPoint *= 1 + Mathf.PerlinNoise(_perlinFreqScale * ((sliceCenter + hugingPoint).x), _perlinFreqScale * ((sliceCenter + hugingPoint).z)) * _perlinAmpScale;

                _vert[(i * (SEGMENTS_ACROSS_CIRCLE + 3) + j)] = sliceCenter + hugingPoint;

            }
        }

        _me.vertices = _vert;
        _me.RecalculateNormals();
        _me.RecalculateBounds();
        _me.RecalculateTangents();
    }



    /*public void StartGlaze() {

        StopAllCoroutines();

        for (int i = 0; i < GLAZE_POINTS_ALONG; i++) {
            _glazePointSpread[i].Reset();
        }

        for (int i = 0; i < SEGMENTS_ACROSS_CIRCLE; i++) {
            _vertexSpread[i].Reset();
        }

        _noiseShift = new Vector2(UnityEngine.Random.value * 100, UnityEngine.Random.value * 100);

        StartCoroutine(StartGlazeCR());
        
        
    }

    private IEnumerator StartGlazeCR() {
        for (int i = 0; i < GLAZE_POINTS_ALONG; i++) {
            yield return new WaitForSeconds(0.1f);
            _glazePointSpread[i].StartRunning();
        }
    }*/




    class GlazingSpread {
        public float InnerSpread = 0;
        public float OuterSpread = 0;

        private float _innerSpeed = 0;
        private float _outerSpeed = 0;

        private float _innerDecay = 0;
        private float _outerDecay = 0;

       
        public void Reset() {
            InnerSpread = 0;
            OuterSpread = 0;
            _innerSpeed = 0;
            _outerSpeed = 0;
            _innerDecay = UnityEngine.Random.Range(0.6f, 0.7f);
            _outerDecay = UnityEngine.Random.Range(0.6f, 0.7f);

        }

        public void AddGlaze() {
            _innerSpeed = UnityEngine.Random.Range(0.4f, 0.65f);
            _outerSpeed = UnityEngine.Random.Range(0.4f, 0.65f);
        }

        public void Update() {

            InnerSpread += _innerSpeed * Time.deltaTime;
            OuterSpread += _outerSpeed * Time.deltaTime;
            _innerSpeed -= _innerDecay * Time.deltaTime;
            _outerSpeed -= _outerDecay * Time.deltaTime;


            if (_innerSpeed < 0) {
                _innerSpeed = 0;
            }
            if (_outerSpeed < 0) {
                _outerSpeed = 0;
            }
        }

        public void Interpolate(GlazingSpread from, GlazingSpread to, float progress) {
            InnerSpread = InterpolateValue(from.InnerSpread, to.InnerSpread, progress);
            OuterSpread = InterpolateValue(from.OuterSpread, to.OuterSpread, progress);
        }

        private float InterpolateValue(float from, float to, float progress) {

            float interpolationVal = Mathf.Cos(progress * Mathf.PI);

            if (from == 0) {
                //interpolationVal = Mathf.Cos(progress * Mathf.PI / 2 + Mathf.PI / 2);
            } else if (to == 0) {
                //interpolationVal = Mathf.Cos(progress * Mathf.PI / 2f);
            }

            float res = (interpolationVal / 2 + 0.5f) * (from - to) + to;

            return res;
            //linear
            //return from + (to - from) * progress;
        }
    }
}


