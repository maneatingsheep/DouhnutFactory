using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    public GameObject MenuButt;
    public GameObject SparkleUI;

    public Slider ProgBar;

    public Action<bool> DipInteractionStateChange;
    public Action<bool> SparkleInteractionStateChange;

    public void Init() {

    }

    public void UpdateState(GameManager.ActivityType activity) {

        MenuButt.SetActive(false);
        SparkleUI.SetActive(false);

        ProgBar.gameObject.SetActive(activity != GameManager.ActivityType.None);
    }

    internal void ShowMenu() {
        MenuButt.SetActive(true);
        SparkleUI.SetActive(false);
        ProgBar.gameObject.SetActive(false);
    }

    public void UpdateProgress(float val) {
        ProgBar.value = val;
    }

    public void DipInteractionStateChanged(bool val) {
        DipInteractionStateChange(val);
    }

    public void SparkleInteractionStateChanged(bool val) {
        SparkleInteractionStateChange(val);
    }
}
