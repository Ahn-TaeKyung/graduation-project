using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class RoomIDCopyToClipboard : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TMP_Text m_roomIDText;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (m_roomIDText != null)
        {
            GUIUtility.systemCopyBuffer = m_roomIDText.text;
            Debug.Log($"Room ID 복사됨: {m_roomIDText.text}");
        }
    }
}
