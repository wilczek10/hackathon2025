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
    public GameObject targetIcon;  // celownik (dla ataku)
    public GameObject healIcon;    // plus (dla trybu leczenia / buffów)

    [Header("Typ postaci")]
    public bool isPolish;    // zaznacz dla Polaka
    public bool isGerman;    // zaznacz dla Niemca
    public bool isMedic;     // zaznacz tylko dla medyka

    [Header("Logic")]
    public TurnManager turnManager;

    [HideInInspector] public Unit unitData;

    [Header("Obiekty efektów (dzieci)")]
    public GameObject healEffectObject;    // obiekt "Leczenie" u Polaków
    public GameObject shieldEffectObject;  // obiekt "Tarcza" u Polaków
    public GameObject grenadeEffectObject; // obiekt "Granat" u Niemców

    [Header("Animacje Polaka (klipy)")]
    public Animator animator;          // Animator przypięty do Polaka
    public AnimationClip shootClip;    // klip animacji strzału
    public AnimationClip grenadeClip;  // klip animacji rzutu granatem

    [Header("Animacje Niemca (klipy)")]
    public AnimationClip germanShootClip;      // animacja strzału Niemca  
    public AnimationClip germanGrenadeClip;    // animacja granatu Niemca

    // Wywoływane z TurnManager.InitUnits
    public void InitUnit(Unit data)
    {
        unitData = data;
        UpdateUI();
        SetTargetHighlight(false, false); // wyłącz ikony na start

        // Upewnij się, że wszystkie efekty są wyłączone na starcie
        if (healEffectObject != null)
            healEffectObject.SetActive(false);
        if (shieldEffectObject != null)
            shieldEffectObject.SetActive(false);
        if (grenadeEffectObject != null)
            grenadeEffectObject.SetActive(false);
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

            // >>> NOWE: spróbuj automatycznie znaleźć Animatora, jeśli nie jest ustawiony
            if (animator == null)
            {
                animator = GetComponent<Animator>();
                if (animator == null)
                {
                    animator = GetComponentInChildren<Animator>();
                }
            }

            UpdateUI();
            SetTargetHighlight(false, false);

            // Na wszelki wypadek wyłącz efekty na starcie
            if (healEffectObject != null)
                healEffectObject.SetActive(false);
            if (shieldEffectObject != null)
                shieldEffectObject.SetActive(false);
            if (grenadeEffectObject != null)
                grenadeEffectObject.SetActive(false);
        }
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

    // === EFEKTY – proste włącz/wyłącz ===

    public void ShowHealEffect()
    {
        if (healEffectObject != null)
            healEffectObject.SetActive(true);
        else
            Debug.LogWarning("ShowHealEffect: healEffectObject == null na obiekcie: " + gameObject.name);
    }

    public void HideHealEffect()
    {
        if (healEffectObject != null)
            healEffectObject.SetActive(false);
    }

    public void ShowShieldEffect()
    {
        if (shieldEffectObject != null)
            shieldEffectObject.SetActive(true);
        else
            Debug.LogWarning("ShowShieldEffect: shieldEffectObject == null na obiekcie: " + gameObject.name);
    }

    public void HideShieldEffect()
    {
        if (shieldEffectObject != null)
            shieldEffectObject.SetActive(false);
    }

    public void ShowGrenadeEffect()
    {
        if (grenadeEffectObject != null)
            grenadeEffectObject.SetActive(true);
        else
            Debug.LogWarning("ShowGrenadeEffect: grenadeEffectObject == null na obiekcie: " + gameObject.name);
    }

    public void HideGrenadeEffect()
    {
        if (grenadeEffectObject != null)
            grenadeEffectObject.SetActive(false);
    }

    // === ANIMACJE POLAKA (na podstawie klipów) ===

    /// <summary>
    /// Odpala animację strzału Polaka przy użyciu klipu przypiętego w Inspectorze.
    /// </summary>
    public void PlayShootAnimation()
    {
        if (animator == null || shootClip == null)
        {
            Debug.LogWarning("PlayShootAnimation: brak animatora lub klipu na obiekcie: " + gameObject.name);
            return;
        }

        animator.Play(shootClip.name, 0, 0f);
    }

    /// <summary>
    /// Odpala animację rzutu granatem Polaka.
    /// </summary>
    public void PlayGrenadeThrowAnimation()
    {
        if (animator == null || grenadeClip == null)
        {
            Debug.LogWarning("PlayGrenadeThrowAnimation: brak animatora lub klipu na obiekcie: " + gameObject.name);
            return;
        }

        animator.Play(grenadeClip.name, 0, 0f);
    }

    // === ANIMACJE NIEMCA ===

    public void PlayGermanShootAnimation()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }

        if (animator == null || germanShootClip == null)
        {
            Debug.LogWarning("PlayGermanShootAnimation: brak animatora lub klipu na obiekcie: " + gameObject.name);
            return;
        }

        animator.Play(germanShootClip.name, 0, 0f);
    }

    public void PlayGermanGrenadeAnimation()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }

        if (animator == null || germanGrenadeClip == null)
        {
            Debug.LogWarning("PlayGermanGrenadeAnimation: brak animatora lub klipu na obiekcie: " + gameObject.name);
            return;
        }

        animator.Play(germanGrenadeClip.name, 0, 0f);
    }
}