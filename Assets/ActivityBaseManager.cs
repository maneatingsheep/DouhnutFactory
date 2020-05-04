using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActivityBaseManager : MonoBehaviour{

    public enum ActivityStateType {Inactive, Intro, Active, Ending };
    public Action<float> UpdateProgress;
    public float ActionProgress;

    protected bool _isInteractive = false;
    protected ActivityStateType _activityState = ActivityStateType.Inactive;

    internal virtual void ResetAll() {
        ActionProgress = 0;
    }

    public virtual void SetInteraction(bool isInteractive) {
        _isInteractive = isInteractive; 
    }

    public virtual void SetActivityState(ActivityStateType activityState) {
        _activityState = activityState;
        if (activityState == ActivityStateType.Intro) {
            ResetAll();
        }

    }

}
