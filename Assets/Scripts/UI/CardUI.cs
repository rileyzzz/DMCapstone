using UnityEngine;

public class CardUI : MonoBehaviour
{
    public RectTransform m_Overlay;
    public Animator m_CardAnim;

    private void OnEnable()
    {
    }

    public void CardClicked( int cardIndex )
    {
        Debug.Log($"card {cardIndex} clicked!");
        m_CardAnim.SetBool("visible", false);
    }
}
