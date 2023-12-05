

using System.Collections;
using TMPro;
using UnityEngine;

public class PopupToast : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI contentText;

    public void Show(string content)
    {
        contentText.text = content;
        this.gameObject.SetActive(true);
        StartCoroutine(IEHide(2f));
    }

    public IEnumerator IEHide(float time)
    {
        yield return new WaitForSeconds(time);
        Hide();
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}