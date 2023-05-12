using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using BitBenderGames;
using UnityEngine.Events;

public class OnTouchUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler //required interface when using the OnPointerExit method.
{
    public UnityEvent onPointerDown, onPointerUp;
    public void OnPointerDown(PointerEventData eventData)
    {
        onPointerDown?.Invoke();
        Debug.Log("OnPointerDown");
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        onPointerUp?.Invoke();
        Debug.Log("OnPointerUp");
    }
}

