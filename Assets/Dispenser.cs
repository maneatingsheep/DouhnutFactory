using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dispenser : MonoBehaviour
{
    public Action<bool> MouseStateChanged;

    private void OnMouseDown() {
        MouseStateChanged(true);
    }

    private void OnMouseUp() {
        MouseStateChanged(false);
    }

    private void OnMouseExit() {
        MouseStateChanged(false);

    }
}
