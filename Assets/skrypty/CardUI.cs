using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    public Image cardImage;   // g³ówny obraz karty (Twoja pe³na grafika)

    [Header("Card Sprites")]
    public Sprite pistolSprite;
    public Sprite grenadeSprite;
    public Sprite bandageSprite;
    public Sprite helmetSprite;

    [HideInInspector] public Card cardData;
    [HideInInspector] public TurnManager turnManager;

    public void Setup(Card card, TurnManager tm)
    {
        cardData = card;
        turnManager = tm;

        // Ustaw odpowiedni¹ grafikê w zale¿noœci od typu karty
        if (cardImage != null)
        {
            switch (card.CardType)
            {
                case CardType.Pistol:
                    cardImage.sprite = pistolSprite;
                    break;
                case CardType.Grenade:
                    cardImage.sprite = grenadeSprite;
                    break;
                case CardType.Bandage:
                    cardImage.sprite = bandageSprite;
                    break;
                case CardType.Helmet:
                    cardImage.sprite = helmetSprite;
                    break;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (turnManager != null)
        {
            turnManager.OnCardClicked(this);
        }
    }
}