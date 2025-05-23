using UnityEngine;
using Fusion;

public class PlayerUIPositioner : NetworkBehaviour
{
    [Networked] public int m_player_index { get; set; }

    public override void Spawned()
    {
        base.Spawned();

        SetUIPosition();
    }

    public void SetUIPosition()
    {
        Transform canvas_transform = transform.Find("Canvas");
        if (canvas_transform == null)
            return;

        Transform left_button_transform = canvas_transform.Find("Button Left");
        Transform right_button_transform = canvas_transform.Find("Button Right");
        Transform role_text_transform = canvas_transform.Find("Role");
        
        switch (m_player_index)
        {
            case 1:
                if (left_button_transform != null)
                    left_button_transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(110, 0);
                if (right_button_transform != null)
                    right_button_transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(-1290, 0);
                if (role_text_transform != null)
                    role_text_transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(-640, 0);
                break;
            case 2:
                if (left_button_transform != null)
                    left_button_transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(750, 0);
                if (right_button_transform != null)
                    right_button_transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(-650, 0);
                if (role_text_transform != null)
                    role_text_transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                break;
            case 3:
                if (left_button_transform != null)
                    left_button_transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(1390, 0);
                if (right_button_transform != null)
                    right_button_transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(-10, 0);
                if (role_text_transform != null)
                    role_text_transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(640, 0);
                break;
        }
    }
}
