using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverSpriteChangerUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite hoverSprite;

    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
        if (normalSprite == null)
            normalSprite = image.sprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        image.sprite = hoverSprite;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        image.sprite = normalSprite;
    }
}
