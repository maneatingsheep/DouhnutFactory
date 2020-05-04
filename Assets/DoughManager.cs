using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoughManager : MonoBehaviour
{
    InflatableTorus _torus;


    public void Init() {

        MeshRenderer mr = GetComponent<MeshRenderer>();

        _torus = new InflatableTorus();

        _torus.INNER_RADIUS = 0.26f;
        _torus.ApplyNoize = false;

        _torus.Init(mr, GetComponent<MeshFilter>());


        _torus.UpdateVertexInterpolation(new float[(_torus.SEGMENTS_ALONG_CIRCLE) * (_torus.SEGMENTS_ACROSS_CIRCLE)]);


    }



}
