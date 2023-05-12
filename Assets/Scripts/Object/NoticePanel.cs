using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Pixelplacement;
using UnityEngine.Events;

public class NoticePanel : Singleton<NoticePanel>
{
    [SerializeField] GameObject content;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] Button closeButton, yesButton, noButton;
    public void Init(string text, bool hasCloseButton = true, bool yesConfirmButton = false, bool hasNoButton = false)
    {
        content.SetActive(true);
        this.text.text = text;
        closeButton.gameObject.SetActive(hasCloseButton);
        yesButton.gameObject.SetActive(yesConfirmButton);
        noButton.gameObject.SetActive(hasNoButton);
    }
    public void OnClickCloseButton()
    {
        content.SetActive(false);
    }
    public void Init(string text, UnityAction yesButtonEvent, UnityAction noButtonEvent, UnityAction clsoeButtonEvent, bool yesConfirmButton = true, bool hasNoButton = true, bool hasCloseButton = true)
    {
        content.SetActive(true);
        this.text.text = text;
        closeButton.gameObject.SetActive(hasCloseButton);
        yesButton.gameObject.SetActive(yesConfirmButton);
        noButton.gameObject.SetActive(hasNoButton);

        yesButton.onClick.RemoveAllListeners();
        yesButton.onClick.AddListener(yesButtonEvent);
        noButton.onClick.RemoveAllListeners();
        noButton.onClick.AddListener(noButtonEvent);
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(clsoeButtonEvent);
    }
}
