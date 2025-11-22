using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UnitView : MonoBehaviour, IPointerClickHandler
{
    [Header("References")]
    public Image spriteImage;
    public Slider hpSlider;
    public Slider shieldSlider;

    [Header("Ikony celu / leczenia")]
    public GameObject targetIcon;
    public GameObject healIcon;

    [Header("Typ postaci")]
    public bool isPolish;
    public bool isGerman;
    public bool isTank;

    [Header("Logic")]
    public TurnManager turnManager;

    [HideInInspector] public Unit unitData;

    [Header("Obiekty efektów (dzieci)")]
    public GameObject healEffectObject;
    public GameObject shieldEffectObject;
    public GameObject grenadeEffectObject;
    public GameObject tankEffectObject;

    [Header("Animacje Polaka (klipy)")]
    public Animator animator;
    public AnimationClip shootClip;
    public AnimationClip grenadeClip;

    [Header("Animacje Niemca (klipy)")]
    public AnimationClip germanShootClip;
    public AnimationClip germanGrenadeClip;
    public AnimationClip tankAttackClip;

    public void InitUnit(Unit data)
    {
        unitData = data;
        UpdateUI();
        SetTargetHighlight(false, false);

        if (healEffectObject != null)
            healEffectObject.SetActive(false);
        if (shieldEffectObject != null)
            shieldEffectObject.SetActive(false);
        if (grenadeEffectObject != null)
            grenadeEffectObject.SetActive(false);
        if (tankEffectObject != null)
            tankEffectObject.SetActive(false);
    }

    void Start()
    {
        if (unitData == null)
        {
            Team team = Team.Poles;
            if (isGerman) team = Team.Germans;
            else if (isPolish) team = Team.Poles;

            UnitType unitType = UnitType.Soldier;
            if (isTank) unitType = UnitType.Tank;

            int maxHp = 6;
            int currentHp = 6;
            int shield = 0;

            if (isGerman)
            {
                maxHp = isTank ? 12 : 5;
                currentHp = isTank ? 12 : 5;
                shield = isTank ? 5 : 2;
            }

            unitData = new Unit
            {
                Name = gameObject.name,
                Team = team,
                UnitType = unitType,
                MaxHp = maxHp,
                CurrentHp = currentHp,
                Shield = shield
            };
        }

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

        if (healEffectObject != null)
            healEffectObject.SetActive(false);
        if (shieldEffectObject != null)
            shieldEffectObject.SetActive(false);
        if (grenadeEffectObject != null)
            grenadeEffectObject.SetActive(false);
        if (tankEffectObject != null)
            tankEffectObject.SetActive(false);
    }

    public void UpdateUI()
    {
        if (unitData == null) return;

        if (hpSlider != null)
        {
            hpSlider.maxValue = unitData.MaxHp;
            hpSlider.value = unitData.CurrentHp;
        }

        if (shieldSlider != null)
        {
            bool hasShield = unitData.Shield > 0;
            shieldSlider.gameObject.SetActive(hasShield);

            if (hasShield)
            {
                shieldSlider.maxValue = isTank ? 5 : 3;
                shieldSlider.value = Mathf.Min(unitData.Shield, (int)shieldSlider.maxValue);
            }
        }

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

    public void ShowTankEffect()
    {
        if (tankEffectObject != null)
            tankEffectObject.SetActive(true);
        else
            Debug.LogWarning("ShowTankEffect: tankEffectObject == null na obiekcie: " + gameObject.name);
    }

    public void HideTankEffect()
    {
        if (tankEffectObject != null)
            tankEffectObject.SetActive(false);
    }

    public void PlayShootAnimation()
    {
        if (animator == null || shootClip == null)
        {
            Debug.LogWarning("PlayShootAnimation: brak animatora lub klipu na obiekcie: " + gameObject.name);
            return;
        }

        animator.Play(shootClip.name, 0, 0f);
    }

    public void PlayGrenadeThrowAnimation()
    {
        if (animator == null || grenadeClip == null)
        {
            Debug.LogWarning("PlayGrenadeThrowAnimation: brak animatora lub klipu na obiekcie: " + gameObject.name);
            return;
        }

        animator.Play(grenadeClip.name, 0, 0f);
    }

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

    public void PlayTankAttackAnimation()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }

        if (animator == null || tankAttackClip == null)
        {
            Debug.LogWarning("PlayTankAttackAnimation: brak animatora lub klipu na obiekcie: " + gameObject.name);
            return;
        }

        animator.Play(tankAttackClip.name, 0, 0f);

        ShowTankEffect();
        Invoke("HideTankEffect", 1f);
    }
}