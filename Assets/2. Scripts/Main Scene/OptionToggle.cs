using UnityEngine;
using UnityEngine.InputSystem; // 반드시 포함

public class OptionToggle : MonoBehaviour
{
    [SerializeField] private GameObject m_optionCanvas;
    private Keyboard _keyboard;

    private void Start()
    {
        _keyboard = Keyboard.current;

        if (m_optionCanvas != null)
            m_optionCanvas.SetActive(false);
    }

    private void Update()
    {
        if (_keyboard != null && _keyboard.escapeKey.wasPressedThisFrame)
        {
            if (m_optionCanvas != null)
            {
                bool isActive = m_optionCanvas.activeSelf;
                m_optionCanvas.SetActive(!isActive);
            }
        }
    }
}
