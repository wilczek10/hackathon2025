using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UnitView : MonoBehaviour, IPointerClickHandler
{
    [Header("References")]
    public Image spriteImage;   // jeœli u¿ywasz UI Image (w Canvasie)
    public Slider hpSlider;
    public Text shieldText;

    [Header("Inspector Setup")]
    public bool isPolish;       // zaznacz dla Polaków
    public bool isMedic;        // zaznacz dla medyka

    [HideInInspector] public Unit unitData;
    [HideInInspector] public TurnManager turnManager;

    // TurnManager wywo³uje InitUnit; jeœli nie, mo¿esz fallbackowaæ w Start
    public void InitUnit(Unit data)
    {
        unitData = data;
        UpdateUI();
    }

    void Start()
    {
        // Jeœli nie dosta³ danych z TurnManager, zainicjuj sam z flag
        if (unitData == null)
        {
            unitData = new Unit
            {
                Name = gameObject.name,
                Team = isPolish ? Team.Poles : Team.Germans,
                UnitType = isMedic ? UnitType.Medic : UnitType.Soldier,
                MaxHp = isPolish ? 6 : 5,
                CurrentHp = isPolish ? 6 : 5,
                Shield = isPolish ? 0 : 2
            };
            UpdateUI();
        }
    }

    public void UpdateUI()
    {
        if (unitData == null) return;

        if (hpSlider != null)
        {
            hpSlider.maxValue = unitData.MaxHp;
            hpSlider.value = unitData.CurrentHp;
        }

        if (shieldText != null)
        {
            shieldText.text = unitData.Shield > 0 ? unitData.Shield.ToString() : "";
        }

        if (spriteImage != null)
        {
            spriteImage.color = unitData.IsAlive ? Color.white : new Color(0.3f, 0.3f, 0.3f, 1f);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (turnManager != null)
        {
            turnManager.OnUnitClicked(this);
        }
    }
}