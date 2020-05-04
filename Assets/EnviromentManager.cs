using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnviromentManager : MonoBehaviour
{

    public Dispenser GlazeDispenser;
    public GameObject DipFluid;

    public Action<bool> DispenserInteractionStateChange;


    public void Init() {
        GlazeDispenser.gameObject.SetActive(false);

        GlazeDispenser.MouseStateChanged += DispenserInteractionStateChange;

    }

    public void UpdateState(GameManager.ActivityType activity) {

        GlazeDispenser.gameObject.SetActive(activity == GameManager.ActivityType.Glaze);
        DipFluid.SetActive(activity == GameManager.ActivityType.Dip);
        

    }
}
