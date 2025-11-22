using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UnitView : MonoBehaviour, IPointerClickHandler
{
    [Header("References")]
    public Image spriteImage;    // UI Image z grafiką jednostki
    public Slider hpSlider;      // pasek HP
    public Slider shieldSlider;  // pasek tarczy

    [Header("Ikony celu / leczenia")]
    public GameObject targetIcon;    // celownik (dla ataku)
    public GameObject healIcon;      // plus (dla leczenia / buffów)

    [Header("Typ postaci")]
    public bool isPolish;        // zaznacz dla Polaka
    public bool isGerman;        // zaznacz dla Niemca
    public bool isMedic;         // zaznacz tylko dla medyka

    [Header("Logic")]
    public TurnManager turnManager;

    [HideInInspector] public Unit unitData;

    // Wywoływane z TurnManager.InitUnits
    public void InitUnit(Unit data)
    {
        unitData = data;
        UpdateUI();
        SetTargetHighlight(false, false); // wyłącz ikony na start
    }

    void Start()
    {
        if (unitData == null)
        {
            Team team = Team.Poles;
            if (isGerman) team = Team.Germans;
            else if (isPolish) team = Team.Poles;

            unitData = new Unit
            {
                Name = gameObject.name,
                Team = team,
                UnitType = isMedic ? UnitType.Medic : UnitType.Soldier,
                MaxHp = isGerman ? 5 : 6,
                CurrentHp = isGerman ? 5 : 6,
                Shield = isGerman ? 2 : 0
            };
        }

        UpdateUI();
        SetTargetHighlight(false, false); // wyłącz ikony na start
    }

    public void UpdateUI()
    {
        if (unitData == null) return;

        // HP
        if (hpSlider != null)
        {
            hpSlider.maxValue = unitData.MaxHp;
            hpSlider.value = unitData.CurrentHp;
        }

        // Tarcza
        if (shieldSlider != null)
        {
            bool hasShield = unitData.Shield > 0;
            shieldSlider.gameObject.SetActive(hasShield);

            if (hasShield)
            {
                // np. max 3 – wtedy tarcza 3/3 wygląda na pełną
                shieldSlider.maxValue = 3;
                shieldSlider.value = Mathf.Min(unitData.Shield, (int)shieldSlider.maxValue);
            }
        }

        // Kolor sprite'a przy śmierci
        if (spriteImage != null)
        {
            spriteImage.color = unitData.IsAlive
                ? Color.white
                : new Color(0.3f, 0.3f, 0.3f, 1f);
        }
    }

    public void SetTargetHighlight(bool attack, bool heal)
    {
        if (targetIcon != null)
            targetIcon.SetActive(attack);

        if (healIcon != null)
            healIcon.SetActive(heal);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (turnManager != null)
        {
            turnManager.OnUnitClicked(this);
        }
    }
}