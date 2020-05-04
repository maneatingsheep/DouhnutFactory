using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DipManager : ActivityBaseManager {
    

    public float[] VertInterpolationProg;

    public MeshRenderer Floor;

    

    private bool isDipping = false;
    private float _startDipContY;
    private float _startDipMouseY;
    public TorusController MainCont;

    internal const float DipLevel = -0.7f;
    internal float DipVariation = 0.04f;

    public Material DipMaterial;

    private Vector2 _contPositionMinMax = new Vector2(DipLevel, 0f);

    InflatableTorus _torus;

    public float DipAngle;

    public Texture FloorTexture;

    public void Init() {

        MeshRenderer mr = GetComponent<MeshRenderer>();

        _torus = new InflatableTorus();
        _torus.OUTER_RADIUS = 0.27f;
        _torus.INNER_RADIUS = 0.24f;

        _torus.Init(mr, GetComponent<MeshFilter>());

        VertInterpolationProg = new float[(_torus.SEGMENTS_ALONG_CIRCLE) * (_torus.SEGMENTS_ACROSS_CIRCLE)];

        _torus.UpdateVertexInterpolation(VertInterpolationProg);

        mr.material = DipMaterial;

    }


    internal override void ResetAll() {
        base.ResetAll();

        for (int i = 0; i < VertInterpolationProg.Length; i++) {
            VertInterpolationProg[i] = 0;
        }
        isDipping = false;
        _torus.UpdateVertexInterpolation(VertInterpolationProg);

    }

    public override void SetActivityState(ActivityStateType activityState) {
        if (activityState != _activityState) {
            if (activityState == ActivityStateType.Intro) {
                Floor.material = DipMaterial;
                Floor.material.mainTexture = FloorTexture;
            }

            //MainCont.isSpinning = activityState == ActivityStateType.Active;
        }
        
        base.SetActivityState(activityState);
    }

    public override void SetInteraction(bool isInteractive) {
        base.SetInteraction(isInteractive);
        
    }

    void Update() {

        float dipSpeed = 0.7f;

        if ((!isDipping && _activityState == ActivityStateType.Active) || _activityState == ActivityStateType.Ending){
            MainCont.transform.position = new Vector3(MainCont.transform.position.x, Math.Min(MainCont.transform.position.y + dipSpeed * Time.deltaTime, _contPositionMinMax.y), MainCont.transform.position.z);
        }

        if (_activityState != ActivityStateType.Active) return;


        /*if (Input.GetMouseButtonDown(0)) {
            isDipping = true;
            _startDipContY = transform.position.y;
            _startDipMouseY = Input.mousePosition.y;
        } else if (Input.GetMouseButtonUp(0)) {
            isDipping = false;
        }*/

        isDipping = ActionProgress < 0.5f;

        if (isDipping) {
            //float yDelta = 2 * (_startDipMouseY - Input.mousePosition.y) / Screen.height;
            //MainCont.transform.position = new Vector3(MainCont.transform.position.x, _startDipContY - yDelta, MainCont.transform.position.z);
            MainCont.transform.position = new Vector3(MainCont.transform.position.x, Math.Max(MainCont.transform.position.y - dipSpeed * Time.deltaTime, _contPositionMinMax.x), MainCont.transform.position.z);
        }



        for (int i = 0; i < _torus.SEGMENTS_ALONG_CIRCLE; i++) {
            for (int j = 0; j < _torus.SEGMENTS_ACROSS_CIRCLE; j++) {

                float y = transform.TransformPoint(_torus._vert[(i * (_torus.SEGMENTS_ACROSS_CIRCLE) + j)]).y;

                float startAt = DipLevel + DipVariation / 2f;
                float maxAt = DipLevel - DipVariation / 2f;

                float target = 0;

                if (y <= maxAt) {
                    target = 1;
                } else if (y < startAt) {

                    target = (y - startAt) / (DipVariation);
                }

                VertInterpolationProg[(i * (_torus.SEGMENTS_ACROSS_CIRCLE) + j)] = Math.Max(target, VertInterpolationProg[(i * (_torus.SEGMENTS_ACROSS_CIRCLE) + j)]);


            }
        }

        _torus.UpdateVertexInterpolation(VertInterpolationProg);

        ActionProgress += Time.deltaTime / 3f;
    }

    internal void Predecorate() {
        int randomDivision = UnityEngine.Random.Range(2, 4);

        int sectionVertexCount =  (_torus.SEGMENTS_ALONG_CIRCLE / (randomDivision * 2)) * _torus.SEGMENTS_ACROSS_CIRCLE;


        for (int i = 0; i < VertInterpolationProg.Length; i++) {
            
            VertInterpolationProg[i] = (Mathf.FloorToInt(i / sectionVertexCount) % 2 == 0)? 1: 0;
        }

        _torus.UpdateVertexInterpolation(VertInterpolationProg);
    }

    internal void Reduce(DipManager dipManager, int preexistingColliders) {
        for (int i = 0; i < _torus.SEGMENTS_ALONG_CIRCLE; i++) {
            for (int j = 0; j < _torus.SEGMENTS_ACROSS_CIRCLE; j++) {


                float target = 1 - (dipManager.VertInterpolationProg[(i * (_torus.SEGMENTS_ACROSS_CIRCLE) + j)]) / preexistingColliders;
                //float target = 0;

                VertInterpolationProg[(i * (_torus.SEGMENTS_ACROSS_CIRCLE) + j)] = Math.Min(VertInterpolationProg[(i * (_torus.SEGMENTS_ACROSS_CIRCLE) + j)], target);



            }
        }

        _torus.UpdateVertexInterpolation(VertInterpolationProg);
    }

    

    
}
