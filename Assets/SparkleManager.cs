using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SparkleManager : ActivityBaseManager {

    public Color[] SparkleColors;

    public Sparkle ParticlePrefab;

    public Collider DonutCollider;
    public Collider FloorCollider;

    bool _isrunning = false;
    float _generateTimer = 0;

    List<Sparkle> _sparkles;

    public float GeneratPeriod;

    public TorusController MainCont;

    public float EmiterVariation;

    public void Init()
    {
        _sparkles = new List<Sparkle>();
    }

    void Update()
    {
        if (_activityState != ActivityStateType.Active) return;

        if (_isrunning) {
            if (_generateTimer > GeneratPeriod) {
                while (_generateTimer > GeneratPeriod) {
                    _generateTimer -= GeneratPeriod;
                    Sparkle sp = Instantiate(ParticlePrefab);
                    _sparkles.Add(sp);
                    sp.transform.position = new Vector3(transform.position.x + Random.Range(-EmiterVariation, EmiterVariation), transform.position.y + Random.Range(-EmiterVariation, EmiterVariation), transform.position.z + Random.Range(-EmiterVariation, EmiterVariation));
                    sp.FloorCollider = FloorCollider;
                    sp.DonutCollider = DonutCollider;

                    sp.UpdateColor(SparkleColors[Random.Range(0, SparkleColors.Length)]);
                }

               
            }
            
            _generateTimer += Time.deltaTime;
        }

        ActionProgress += Time.deltaTime / 7f;

    }

    override public void SetInteraction(bool isInteractive) {
        base.SetInteraction(isInteractive);
        //_isrunning = isInteractive;
    }

    public override void SetActivityState(ActivityStateType activityState) {
        if (activityState != _activityState) {
            
            MainCont.isSpinning = activityState == ActivityStateType.Active;
            _isrunning = activityState == ActivityStateType.Active;
        }

        base.SetActivityState(activityState);
    }
}
