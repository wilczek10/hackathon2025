using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum Team
{
    Poles,
    Germans
}

public enum UnitType
{
    Soldier,
    Tank
}

public enum CardType
{
    Pistol,
    Grenade,
    Bandage,
    Helmet
}

public enum TurnState
{
    PlayerAction,
    EnemyAction,
    DrawPhase,
    CheckEnd
}

[System.Serializable]
public class Unit
{
    public string Name;
    public Team Team;
    public UnitType UnitType;

    public int MaxHp;
    public int CurrentHp;
    public int Shield;

    public bool IsAlive => CurrentHp > 0;
}

[System.Serializable]
public class Card
{
    public CardType CardType;
    public string CardName;
    public string Description;
}

public class Deck
{
    public List<Card> drawPile = new List<Card>();
    public List<Card> discardPile = new List<Card>();

    public void InitializeDefaultDeck()
    {
        drawPile.Clear();
        discardPile.Clear();

        for (int i = 0; i < 6; i++)
        {
            drawPile.Add(new Card { CardType = CardType.Pistol, CardName = "Pistolet" });
            drawPile.Add(new Card { CardType = CardType.Grenade, CardName = "Granat" });
            drawPile.Add(new Card { CardType = CardType.Bandage, CardName = "Bandaż" });
            drawPile.Add(new Card { CardType = CardType.Helmet, CardName = "Hełm" });
        }

        Shuffle(drawPile);
    }

    public void Shuffle(List<Card> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;

        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }

    public Card DrawCard()
    {
        if (drawPile.Count == 0)
            return null;

        Card c = drawPile[0];
        drawPile.RemoveAt(0);
        return c;
    }

    public void Discard(Card card)
    {
        if (card != null)
            discardPile.Add(card);
    }
}

public class Hand
{
    public List<Card> cardsInHand = new List<Card>();
    public int MaxHandSize = 4;

    public void AddCard(Card card)
    {
        if (card == null) return;
        if (cardsInHand.Count >= MaxHandSize) return;
        cardsInHand.Add(card);
    }

    public void RemoveCard(Card card)
    {
        cardsInHand.Remove(card);
    }
}

public class TurnManager : MonoBehaviour
{
    [Header("Units")]
    public List<UnitView> polishUnitViews;
    public List<UnitView> germanUnitViews;

    [Header("Cards UI")]
    public Transform handPanel;
    public GameObject cardUIPrefab;

    [Header("Game State UI")]
    public Text infoText;

    [Header("Animacje barykady / tła")]
    public AnimationClip barykadaClip;
    public GameObject barykadaObject;
    public Animator barykadaAnimator;

    public BackgroundScroll backgroundScroll;
    public float waitTimeAfterScroll = 0.5f;

    [Header("Deck Visual")]
    public GameObject[] deckBackCards;

    private Deck deck;
    private Hand hand;

    private TurnState state;

    private CardUI selectedCardUI;
    [HideInInspector] public Card selectedCard;
    private bool selectingTarget;

    private bool gameEnded = false;

    void Start()
    {
        deck = new Deck();
        deck.InitializeDefaultDeck();

        hand = new Hand();

        InitUnits();

        for (int i = 0; i < hand.MaxHandSize; i++)
        {
            DrawCardToHand();
        }

        UpdateDeckVisual();

        state = TurnState.PlayerAction;
        UpdateInfoText("Tura gracza - wybierz kartę.");
    }

    void InitUnits()
    {
        // Polacy - wszyscy żołnierze (bez medyka)
        foreach (var pv in polishUnitViews)
        {
            var unit = new Unit
            {
                Name = pv.gameObject.name,
                Team = Team.Poles,
                UnitType = UnitType.Soldier,
                MaxHp = 6,
                CurrentHp = 6,
                Shield = 0
            };

            pv.InitUnit(unit);
            pv.turnManager = this;
        }

        // Niemcy - żołnierze i czołg
        foreach (var gv in germanUnitViews)
        {
            UnitType unitType = gv.isTank ? UnitType.Tank : UnitType.Soldier;
            int maxHp = gv.isTank ? 12 : 5;
            int currentHp = gv.isTank ? 12 : 5;
            int shield = gv.isTank ? 5 : 2;

            var unit = new Unit
            {
                Name = gv.gameObject.name,
                Team = Team.Germans,
                UnitType = unitType,
                MaxHp = maxHp,
                CurrentHp = currentHp,
                Shield = shield
            };

            gv.InitUnit(unit);
            gv.turnManager = this;
        }
    }

    void UpdateDeckVisual()
    {
        if (deckBackCards == null || deckBackCards.Length == 0 || deck == null) return;

        int cardsLeft = deck.drawPile.Count;

        for (int i = 0; i < deckBackCards.Length; i++)
        {
            deckBackCards[i].SetActive(cardsLeft > i * 3);
        }
    }

    public void DrawCardToHand()
    {
        if (hand.cardsInHand.Count >= hand.MaxHandSize) return;

        Card drawn = deck.DrawCard();
        if (drawn == null)
        {
            UpdateDeckVisual();
            return;
        }

        // USUNIĘTO WARUNEK BLOKUJĄCY BANDAŻE - teraz bandaże są zawsze dostępne
        hand.AddCard(drawn);
        CreateCardUI(drawn);

        UpdateDeckVisual();
    }

    void CreateCardUI(Card card)
    {
        GameObject go = Instantiate(cardUIPrefab, handPanel);
        var ui = go.GetComponent<CardUI>();
        ui.Setup(card, this);
    }

    public void OnCardClicked(CardUI cardUI)
    {
        if (state != TurnState.PlayerAction) return;
        if (gameEnded) return;

        ClearAllTargetHighlights();

        selectedCardUI = cardUI;
        selectedCard = cardUI.cardData;
        selectingTarget = false;

        if (selectedCard.CardType == CardType.Grenade)
        {
            PlayCardGrenade();
            AfterCardPlayed();
            return;
        }

        selectingTarget = true;

        switch (selectedCard.CardType)
        {
            case CardType.Pistol:
                foreach (var gv in germanUnitViews)
                    if (gv.unitData.IsAlive)
                        gv.SetTargetHighlight(true, false);
                UpdateInfoText("Wybierz NIEMCA dla karty: " + selectedCard.CardName);
                break;

            case CardType.Bandage:
            case CardType.Helmet:
                foreach (var pv in polishUnitViews)
                    if (pv.unitData.IsAlive)
                        pv.SetTargetHighlight(false, true);
                UpdateInfoText("Wybierz POLAKA dla karty: " + selectedCard.CardName);
                break;
        }
    }

    public void OnUnitClicked(UnitView unitView)
    {
        Debug.Log("Kliknięto jednostkę: " + unitView.unitData.Name + " | Team: " + unitView.unitData.Team);

        if (!selectingTarget)
        {
            Debug.Log("Klik zignorowany: nie jestem w trybie wybierania celu.");
            return;
        }

        if (selectedCard == null)
        {
            Debug.Log("Brak wybranej karty (selectedCard == null).");
            return;
        }

        if (state != TurnState.PlayerAction)
        {
            Debug.Log("Nie jest teraz tura gracza.");
            return;
        }

        if (gameEnded)
        {
            Debug.Log("Gra już się skończyła.");
            return;
        }

        Unit target = unitView.unitData;

        if (!IsValidTarget(selectedCard, target))
        {
            Debug.Log("Nieprawidłowy cel dla karty: " + selectedCard.CardName);
            return;
        }

        Debug.Log("Używam karty " + selectedCard.CardName + " na " + target.Name);

        PlayCardOnTarget(selectedCard, target, unitView);

        unitView.UpdateUI();
        AfterCardPlayed();
    }

    bool IsValidTarget(Card card, Unit target)
    {
        switch (card.CardType)
        {
            case CardType.Pistol:
                return target.Team == Team.Germans && target.IsAlive;

            case CardType.Bandage:
            case CardType.Helmet:
                return target.Team == Team.Poles && target.IsAlive;

            default:
                return false;
        }
    }

    void PlayCardOnTarget(Card card, Unit target, UnitView targetView)
    {
        Debug.Log("PlayCardOnTarget: " + card.CardName + " na " + target.Name);

        UnitView actingPole = polishUnitViews != null && polishUnitViews.Count > 0
            ? polishUnitViews[0]
            : null;

        switch (card.CardType)
        {
            case CardType.Pistol:
                if (actingPole != null)
                    actingPole.PlayShootAnimation();

                DealDamage(target, 3);
                break;

            case CardType.Bandage:
                Heal(target, 2);
                if (targetView != null)
                {
                    StartCoroutine(HealEffectRoutine(targetView, 1f));
                }
                break;

            case CardType.Helmet:
                target.Shield += 2;
                if (targetView != null)
                {
                    StartCoroutine(ShieldEffectRoutine(targetView, 0.4f));
                }
                break;
        }

        RefreshAllUnitsUI();
    }

    void PlayCardGrenade()
    {
        UnitView actingPole = polishUnitViews != null && polishUnitViews.Count > 0
            ? polishUnitViews[0]
            : null;

        if (actingPole != null)
            actingPole.PlayGrenadeThrowAnimation();

        var aliveGermans = germanUnitViews.Where(v => v.unitData.IsAlive).ToList();

        foreach (var gv in aliveGermans)
        {
            StartCoroutine(GrenadeEffectRoutine(gv, 1f));
            int dmg = Random.Range(1, 3);
            DealDamage(gv.unitData, dmg);
        }

        RefreshAllUnitsUI();
        UpdateInfoText("Rzucasz granat w Niemców!");
    }

    void AfterCardPlayed()
    {
        if (selectedCard != null)
        {
            hand.RemoveCard(selectedCard);
            deck.Discard(selectedCard);
        }

        if (selectedCardUI != null)
        {
            Destroy(selectedCardUI.gameObject);
        }

        selectedCard = null;
        selectedCardUI = null;
        selectingTarget = false;

        ClearAllTargetHighlights();

        if (!gameEnded)
        {
            while (hand.cardsInHand.Count < hand.MaxHandSize && deck.drawPile.Count > 0)
            {
                DrawCardToHand();
            }
        }

        if (!gameEnded)
            StartCoroutine(EnemyTurnWithDelay());
    }

    IEnumerator EnemyTurnWithDelay()
    {
        yield return new WaitForSeconds(0.6f);
        EndPlayerAction();
    }

    void RefreshAllUnitsUI()
    {
        foreach (var v in polishUnitViews) v.UpdateUI();
        foreach (var v in germanUnitViews) v.UpdateUI();
    }

    public void DealDamage(Unit target, int amount)
    {
        if (!target.IsAlive) return;

        int remaining = amount;

        if (target.Shield > 0)
        {
            int absorbed = Mathf.Min(target.Shield, remaining);
            target.Shield -= absorbed;
            remaining -= absorbed;
        }

        if (remaining > 0)
        {
            target.CurrentHp -= remaining;
            if (target.CurrentHp <= 0)
            {
                target.CurrentHp = 0;
                OnUnitDied(target);
            }
        }
    }

    public void Heal(Unit target, int amount)
    {
        if (!target.IsAlive) return;
        target.CurrentHp = Mathf.Min(target.CurrentHp + amount, target.MaxHp);
    }

    void OnUnitDied(Unit unit)
    {
        RefreshAllUnitsUI();
    }

    public void EndPlayerAction()
    {
        if (gameEnded) return;

        state = TurnState.EnemyAction;
        EnemyAction();
    }

    void EnemyAction()
    {
        if (gameEnded) return;

        var aliveGermans = germanUnitViews.Where(v => v.unitData.IsAlive).ToList();
        var alivePoles = polishUnitViews.Where(v => v.unitData.IsAlive).ToList();

        if (aliveGermans.Count == 0 || alivePoles.Count == 0)
        {
            state = TurnState.CheckEnd;
            CheckEndConditions();
            return;
        }

        // Sprawdzamy czy jest żywy czołg
        bool tankAlive = aliveGermans.Any(g => g.unitData.UnitType == UnitType.Tank);

        int action;
        if (tankAlive && Random.Range(0, 10) < 3) // 40% szans na atak czołgu
        {
            action = 3;
        }
        else
        {
            action = Random.Range(0, 3);
        }

        switch (action)
        {
            case 0:
                {
                    UnitView targetPole = alivePoles[Random.Range(0, alivePoles.Count)];
                    var aliveSoldiers = aliveGermans.Where(g => g.unitData.UnitType == UnitType.Soldier).ToList();
                    if (aliveSoldiers.Count > 0)
                    {
                        UnitView shootingGerman = aliveSoldiers[Random.Range(0, aliveSoldiers.Count)];
                        shootingGerman.PlayGermanShootAnimation();
                        DealDamage(targetPole.unitData, 3);
                        UpdateInfoText("Niemcy: strzał w " + targetPole.unitData.Name);
                    }
                }
                break;

            case 1:
                {
                    var aliveSoldiers = aliveGermans.Where(g => g.unitData.UnitType == UnitType.Soldier).ToList();
                    if (aliveSoldiers.Count > 0)
                    {
                        UnitView throwingGerman = aliveSoldiers[Random.Range(0, aliveSoldiers.Count)];
                        throwingGerman.PlayGermanGrenadeAnimation();

                        foreach (var pv in alivePoles)
                        {
                            int dmg = Random.Range(1, 3);
                            DealDamage(pv.unitData, dmg);
                        }
                        UpdateInfoText("Niemcy: granat w wszystkich Polaków.");
                    }
                }
                break;

            case 2:
                {
                    var aliveSoldiers = aliveGermans.Where(g => g.unitData.UnitType == UnitType.Soldier).ToList();
                    if (aliveSoldiers.Count > 0)
                    {
                        UnitView targetGerman = aliveSoldiers[Random.Range(0, aliveSoldiers.Count)];
                        Heal(targetGerman.unitData, 2);
                        UpdateInfoText("Niemcy: leczenie " + targetGerman.unitData.Name);
                    }
                }
                break;

            case 3: // Akcja czołgu
                {
                    UnitView tank = aliveGermans.FirstOrDefault(g => g.unitData.UnitType == UnitType.Tank);
                    if (tank != null)
                    {
                        tank.PlayTankAttackAnimation();

                        foreach (var pv in alivePoles)
                        {
                            int dmg = Random.Range(3, 5); // 1-4 obrażeń
                            DealDamage(pv.unitData, dmg);
                        }
                        UpdateInfoText("CZOLG atakuje wszystkich Polaków!");
                    }
                }
                break;
        }

        RefreshAllUnitsUI();
        state = TurnState.DrawPhase;
        DrawPhase();
    }

    void DrawPhase()
    {
        if (gameEnded) return;

        DrawCardToHand();
        state = TurnState.CheckEnd;
        CheckEndConditions();
    }

    public void StartLevelTransition()
    {
        if (backgroundScroll != null)
        {
            backgroundScroll.StartScrolling();
            float loadTime = (backgroundScroll.distance / backgroundScroll.scrollSpeed) + waitTimeAfterScroll;
            Invoke(nameof(LoadNextScene), loadTime);
        }
        else
        {
            Invoke(nameof(LoadNextScene), waitTimeAfterScroll);
        }
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    void CheckEndConditions()
    {
        if (gameEnded) return;

        bool anyPoleAlive = polishUnitViews.Any(v => v.unitData.IsAlive);
        bool anyGermanAlive = germanUnitViews.Any(v => v.unitData.IsAlive);

        if (!anyGermanAlive)
        {
            gameEnded = true;
            UpdateInfoText("WYGRANA! Wszyscy Niemcy pokonani.");

            if (barykadaAnimator != null && barykadaClip != null)
                barykadaAnimator.Play(barykadaClip.name);

            Invoke(nameof(StartLevelTransition), 1.5f);
            return;
        }

        if (!anyPoleAlive)
        {
            gameEnded = true;
            UpdateInfoText("PRZEGRANA! Wszyscy Polacy zginęli.");
            return;
        }

        bool deckEmpty = deck.drawPile.Count == 0 && hand.cardsInHand.Count == 0;
        if (deckEmpty && anyGermanAlive)
        {
            gameEnded = true;
            UpdateInfoText("PRZEGRANA! Skończyły się karty, a Niemcy żyją.");
            return;
        }

        state = TurnState.PlayerAction;
        UpdateInfoText("Tura gracza - wybierz kartę.");
    }

    void UpdateInfoText(string msg)
    {
        if (infoText != null)
            infoText.text = msg;
        else
            Debug.Log(msg);
    }

    void ClearAllTargetHighlights()
    {
        foreach (var pv in polishUnitViews)
            pv.SetTargetHighlight(false, false);

        foreach (var gv in germanUnitViews)
            gv.SetTargetHighlight(false, false);
    }

    IEnumerator HealEffectRoutine(UnitView targetView, float duration)
    {
        if (targetView == null) yield break;
        targetView.ShowHealEffect();
        yield return new WaitForSeconds(duration);
        targetView.HideHealEffect();
    }

    IEnumerator ShieldEffectRoutine(UnitView targetView, float duration)
    {
        if (targetView == null) yield break;
        targetView.ShowShieldEffect();
        yield return new WaitForSeconds(duration);
        targetView.HideShieldEffect();
    }

    IEnumerator GrenadeEffectRoutine(UnitView targetView, float duration)
    {
        if (targetView == null) yield break;
        targetView.ShowGrenadeEffect();
        yield return new WaitForSeconds(duration);
        targetView.HideGrenadeEffect();
    }
}