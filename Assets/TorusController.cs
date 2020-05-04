using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class TorusController : MonoBehaviour
{

    public bool isSpinning = false;

    public DoughManager DoughMgr;
    public DipManager[] DipsMgrs;
    public GlazingManager GlazeMgr;
    public SparkleManager[] SparkleMgrs;
    private ActivityType _currentActivity;
    private int _activityVariation;

    

    public float ActionProgress {
        get {
            switch (_currentActivity) {
                case ActivityType.Glaze:
                    return GlazeMgr.ActionProgress;
                case ActivityType.Sparkle:
                    return SparkleMgrs[_activityVariation].ActionProgress;
                case ActivityType.Dip:
                    return DipsMgrs[_activityVariation].ActionProgress;
                default:
                    return 0;
            }
        }
        internal set {
            switch (_currentActivity) {
                case ActivityType.Glaze:
                    GlazeMgr.ActionProgress = value;
                    break;
                case ActivityType.Sparkle:
                    SparkleMgrs[_activityVariation].ActionProgress = value; ;
                    break;
                case ActivityType.Dip:
                    DipsMgrs[_activityVariation].ActionProgress = value; ;
                    break;
            }
        }
    }


    public void DispenserInteracted(bool state) {
        GlazeMgr.SetInteraction(state);
    }

    private void SparlesInteracted(bool state) {
    }

    public void Init()
    {


        DoughMgr.Init();

        for (int i = 0; i < DipsMgrs.Length; i++) {
            //UIMngr.DipInteractionStateChange += DipsMgrs[i].SetInteraction;
            DipsMgrs[i].Init();
        }

        GlazeMgr.Init();

        for (int i = 0; i < SparkleMgrs.Length; i++) {
            //UIMngr.SparkleInteractionStateChange += SparkleMgrs[i].SetInteraction;
            SparkleMgrs[i].Init();
        }
    }

    public void ResetDoughnut(ActivityType activity, int actVariation) {
        _currentActivity = activity;
        _activityVariation = actVariation;
        

        for (int i = 0; i < DipsMgrs.Length; i++) {

            if (activity == ActivityType.None || (activity == ActivityType.Dip && i == actVariation)) {
                DipsMgrs[i].ResetAll();
            }

        }

        if (activity == ActivityType.None || (activity == ActivityType.Glaze)) {
            GlazeMgr.ResetAll();
        }


        for (int i = 0; i < SparkleMgrs.Length; i++) {
            if (activity == ActivityType.None || (activity == ActivityType.Sparkle && i == actVariation)) {
                SparkleMgrs[i].ResetAll();
            }
        }



    }

    internal void Predecorate() {
        
        DipsMgrs[UnityEngine.Random.Range(0, DipsMgrs.Length)].Predecorate();
    }

    internal IEnumerator PlayActivityIntro(bool isSamePrevDoughnut, bool isSamePrevActivity) {

        float totalPlayTime = 0f;

        GlazeMgr.SetActivityState(ActivityBaseManager.ActivityStateType.Inactive);

        for (int i = 0; i < SparkleMgrs.Length; i++) {
            SparkleMgrs[i].SetActivityState(ActivityBaseManager.ActivityStateType.Inactive);
        }
        for (int i = 0; i < DipsMgrs.Length; i++) {
            DipsMgrs[i].SetActivityState(ActivityBaseManager.ActivityStateType.Inactive);
        }

        switch (_currentActivity) {
            case ActivityType.Glaze:
                GlazeMgr.SetActivityState(ActivityBaseManager.ActivityStateType.Intro);

                break;
            case ActivityType.Sparkle:
                SparkleMgrs[_activityVariation].SetActivityState(ActivityBaseManager.ActivityStateType.Intro);
                break;
            case ActivityType.Dip:
                DipsMgrs[_activityVariation].SetActivityState(ActivityBaseManager.ActivityStateType.Intro);
                
                LeanTween.rotate(gameObject, new Vector3(90 -90 * _activityVariation, 0, 0), 0.4f);

                break;
        }

        yield return new WaitForSeconds(totalPlayTime + 0.5f);
    }

    internal void PlayActivityGameplay() {

        switch (_currentActivity) {
            case ActivityType.Glaze:
                GlazeMgr.SetActivityState(ActivityBaseManager.ActivityStateType.Active);
                GlazeMgr.SetInteraction(false);

                break;
            case ActivityType.Sparkle:
                SparkleMgrs[_activityVariation].SetActivityState(ActivityBaseManager.ActivityStateType.Active);
                SparkleMgrs[_activityVariation].SetInteraction(false);
                break;
            case ActivityType.Dip:

                DipsMgrs[_activityVariation].SetActivityState(ActivityBaseManager.ActivityStateType.Active);
                DipsMgrs[_activityVariation].SetInteraction(false);
                break;
        }
    }

    internal IEnumerator PlayActivityOut(bool isSameNextDoughnut, bool isSameNextActivity) {

        float totalPlayTime = 0f;


        GlazeMgr.SetInteraction(false);
        GlazeMgr.SetActivityState(ActivityBaseManager.ActivityStateType.Inactive);

        for (int i = 0; i < SparkleMgrs.Length; i++) {
            SparkleMgrs[i].SetInteraction(false);
            SparkleMgrs[i].SetActivityState(ActivityBaseManager.ActivityStateType.Inactive);
        }
        for (int i = 0; i < DipsMgrs.Length; i++) {
            DipsMgrs[i].SetInteraction(false);
            DipsMgrs[i].SetActivityState(ActivityBaseManager.ActivityStateType.Inactive);
        }


        switch (_currentActivity) {
            case ActivityType.Glaze:
                GlazeMgr.SetActivityState(ActivityBaseManager.ActivityStateType.Ending);
                break;
            case ActivityType.Sparkle:
                SparkleMgrs[_activityVariation].SetActivityState(ActivityBaseManager.ActivityStateType.Ending);
                break;
            case ActivityType.Dip:
                DipsMgrs[_activityVariation].SetActivityState(ActivityBaseManager.ActivityStateType.Ending);
                
                if (!isSameNextActivity) {
                    totalPlayTime = Math.Max(totalPlayTime, 0.8f);
                    LeanTween.rotate(gameObject, new Vector3(0, 0, 0), 0.4f);
                }
                

                break;
        }

        yield return new WaitForSeconds(totalPlayTime);
    }

    void Update()
    {
        if (isSpinning) {
            transform.Rotate(0, 130 * Time.deltaTime, 0);
        }

        if (ActionProgress < 1) {
            switch (_currentActivity) {
                case ActivityType.Glaze:
                    break;
                case ActivityType.Sparkle:
                    break;
                case ActivityType.Dip:
                    for (int i = 0; i < _activityVariation; i++) {
                        DipsMgrs[i].Reduce(DipsMgrs[_activityVariation], i + 1);
                    }
                    break;
            }
        }
        
    }
}
