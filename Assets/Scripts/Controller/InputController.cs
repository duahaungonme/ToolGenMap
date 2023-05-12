using System;
using UnityEngine;

public class InputController : MonoBehaviour
{
    private bool isLeftCtrlPressing;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Debug.Log($"a");
            isLeftCtrlPressing = true;
        }

        if (Input.GetKeyDown(KeyCode.V) && isLeftCtrlPressing)
        {
            Debug.Log($"undo");
            isLeftCtrlPressing = false;
        }
    }
}