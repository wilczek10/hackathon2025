using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UnitView : MonoBehaviour, IPointerClickHandler
{
    [Header("References")]
    public Image spriteImage;    // UI Image z grafik¹ jednostki
    public Slider hpSlider;      // pasek HP
    public Slider shieldSlider;  // pasek tarczy

    [Header("Typ postaci")]
    public bool isPolish;        // zaznacz dla Polaka
    public bool isGerman;        // zaznacz dla Niemca
    public bool isMedic;         // zaznacz tylko dla medyka

    [Header("Logic")]
    public TurnManager turnManager;

    [HideInInspector] public Unit unitData;

    // Wywo³ywane z TurnManager.InitUnits (w Twoim TurnManager ju¿ to robisz)
    public void InitUnit(Unit data)
    {
        unitData = data;
        UpdateUI();
    }

    void Start()
    {
        // Fallback, gdyby InitUnit nie zosta³ wywo³any
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
                MaxHp = isGerman ? 5 : 6,      // przyk³adowo: Niemiec 5 HP, Polak 6 HP
                CurrentHp = isGerman ? 5 : 6,
                Shield = isGerman ? 2 : 0      // Niemiec 2 tarczy, Polak 0
            };
        }

        UpdateUI();
    }

    public void UpdateUI()
    {
        if (unitData == null) return;

        // Pasek HP
        if (hpSlider != null)
        {
            hpSlider.maxValue = unitData.MaxHp;
            hpSlider.value = unitData.CurrentHp;
        }

        // Pasek tarczy
        if (shieldSlider != null)
        {
            // ustaw maksymaln¹ wartoœæ tarczy wed³ug balansu (np. 5)
            shieldSlider.maxValue = 5;
            shieldSlider.value = unitData.Shield;
        }

        // Efekt wizualny przy œmierci (wyszarzenie)
        if (spriteImage != null)
        {
            spriteImage.color = unitData.IsAlive
                ? Color.white
                : new Color(0.3f, 0.3f, 0.3f, 1f);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("OnPointerClick na obiekcie: " + gameObject.name);  // DEBUG – musi siê pojawiæ przy klikniêciu

        if (turnManager != null)
        {
            turnManager.OnUnitClicked(this);
        }
        else
        {
            Debug.LogWarning("turnManager == null na obiekcie: " + gameObject.name);
        }
    }
}