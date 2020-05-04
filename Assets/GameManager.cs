using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum ActivityType {None, Glaze, Dip, Sparkle };

    public TorusController[] Doughnut;
    internal UIManager UIMngr;
    internal EnviromentManager EnviromentMngr;


    private int _doughnutCount;


    public ActivityType CurrentActivity;
    public int CurrentActivityVariarion;

    private Vector3 EnterFromPos;
    private Vector3 ExitToPos;

    void Start() {

        _doughnutCount = 0;
        EnterFromPos = new Vector3(-3, 0, 0);
        ExitToPos = new Vector3(3, 0, 0);


        UIMngr = GetComponent<UIManager>();
        EnviromentMngr = GetComponent<EnviromentManager>();

        EnviromentMngr.DispenserInteractionStateChange += DispenserInteracted;
        
        
        CurrentActivity = ActivityType.None;

        UIMngr.Init();
        EnviromentMngr.Init();

        for (int i = 0; i < Doughnut.Length; i++) {
            Doughnut[i].Init();
            Doughnut[i].transform.position = EnterFromPos;
            Doughnut[i].ResetDoughnut(CurrentActivity, CurrentActivityVariarion);
        }

       
        UIMngr.ShowMenu();
        EnviromentMngr.UpdateState(ActivityType.None);


    }

    private void DispenserInteracted(bool state) {
        Doughnut[_doughnutCount % Doughnut.Length].DispenserInteracted(state);
    }

    public void StartGame() {
        StartCoroutine(RunAllActivities());
    }
    

    public IEnumerator RunAllActivities() {


        UIMngr.UpdateState(ActivityType.None);
        CurrentActivity = ActivityType.Dip;
        CurrentActivityVariarion = 0;

        yield return new WaitForSeconds(0.5f);

        //LeanTween.move(Camera.main.gameObject, new Vector3(0, Camera.main.transform.position.y, Camera.main.transform.position.z), 0.4f);
        //yield return new WaitForSeconds(0.4f);
        bool isSamePrevActivity = false;
        bool isSameNextActivity = true;
        bool isfirst = true;

        while (CurrentActivity != ActivityType.None) {
            
            yield return StartCoroutine(PlayFullActivity(!isfirst, isSamePrevActivity, true, isSameNextActivity));

            isfirst = false;

            isSamePrevActivity = isSameNextActivity;
            
            if (CurrentActivity == ActivityType.Dip) {
                if (CurrentActivityVariarion < 2) {
                    isSameNextActivity = CurrentActivityVariarion < 1;
                    CurrentActivityVariarion++;

                } else {
                    CurrentActivity = ActivityType.Glaze;
                    CurrentActivityVariarion = 0;
                    isSameNextActivity = false;
                }
            } else if (CurrentActivity == ActivityType.Glaze) {
                CurrentActivity = ActivityType.Sparkle;
                isSameNextActivity = false;
            } else if (CurrentActivity == ActivityType.Sparkle) {
                CurrentActivity = ActivityType.None;
                isSameNextActivity = false;
            }
            //_doughnutCount++;
        }


    }

    private IEnumerator PlayFullActivity(bool isSameTorusOnStart, bool isSameActOnStart, bool isSameTorusOnExit, bool isSameActOnExit) {

        Doughnut[_doughnutCount % Doughnut.Length].ResetDoughnut(CurrentActivity, CurrentActivityVariarion);

        if (!isSameTorusOnStart) {
            //Doughnut[_doughnutCount % Doughnut.Length].Predecorate();
            Doughnut[_doughnutCount % Doughnut.Length].transform.position = EnterFromPos;
            
            LeanTween.move(Doughnut[_doughnutCount % Doughnut.Length].gameObject, Vector3.zero, 0.4f);
            yield return new WaitForSeconds(0.4f);
        }


        yield return StartCoroutine(PlayActivityIntro(isSameTorusOnStart, isSameActOnStart));
        yield return StartCoroutine(PlayActivityGameplay());

        yield return StartCoroutine(PlayActivityOut(isSameTorusOnExit, isSameActOnExit));

        if (!isSameTorusOnExit) {
            LeanTween.move(Doughnut[_doughnutCount % Doughnut.Length].gameObject, ExitToPos, 0.4f);
        }


    }


    private IEnumerator PlayActivityIntro(bool isSameDoughnut, bool isSameActivity) {
        
        UIMngr.UpdateState(ActivityType.None);
        EnviromentMngr.UpdateState(ActivityType.None);

        yield return StartCoroutine( Doughnut[_doughnutCount % Doughnut.Length].PlayActivityIntro(isSameDoughnut, isSameActivity));


        UIMngr.UpdateState(CurrentActivity);
        EnviromentMngr.UpdateState(CurrentActivity);

    }

    private IEnumerator PlayActivityGameplay() {

        Doughnut[_doughnutCount % Doughnut.Length].PlayActivityGameplay();

        while (Doughnut[_doughnutCount % Doughnut.Length].ActionProgress < 1) {

            UIMngr.UpdateProgress(Doughnut[_doughnutCount % Doughnut.Length].ActionProgress);
            yield return null;
        }

        UIMngr.UpdateProgress(1);

    }

    private IEnumerator PlayActivityOut(bool isSameDoughnut, bool isSameActivity) {

        UIMngr.UpdateState(ActivityType.None);
        EnviromentMngr.UpdateState(ActivityType.None);

        yield return StartCoroutine(Doughnut[_doughnutCount % Doughnut.Length].PlayActivityOut(isSameDoughnut, isSameActivity));

    }

}
