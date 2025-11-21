using UnityEngine;
using TMPro;
using BNG;
using System.Collections;

public class WitchController : MonoBehaviour
{
    [Header("References")]
    public Transform player;               // PlayerController → главная точка игрока
    public TextMeshProUGUI witchMessage;   // UI сообщение (СТОИТ НЕ НА ВЕДЬМЕ!)
    public float rotationSpeed = 2f;       // Скорость поворота
    public float maxDistance = 2.2f;       // Дистанция реакции ведьмы

    private Quaternion defaultRotation;     // исходное положение ведьмы
    private bool isTurning = false;

    private enum QuestStage
    {
        Intro1,
        Intro2,
        ExplainMushrooms,
        MushroomsTaskActive,
        BoneTaskActive,
        KnifeTaskActive,
        Finished
    }

    private QuestStage stage = QuestStage.Intro1;

    private bool playerInside = false;  // игрок в зоне?
    private bool waitingForA = false;   // ждём ли нажатия A
    private Grabbable currentHeldBone;  // какую кость держит игрок

    [Header("Quest Items")]
    public Grabbable basket;
    public Grabbable[] twilightCaps;   // все грибы Twilight Amethystcap в сцене
    public Grabbable[] bones;          // bone2, bone2 (1), bone2 (2), bone2 (3)
    public Grabbable ritualKnife;
    public Grabbable magicPotion;      // объект зелья (в инвентаре, изначально выключен)
                                       // ======== Healing Potion Appearance System ========
    [Header("Healing Potion Appearance")]
    public HealingAppearController healingAppearController;

    [Header("Texts - Intro")]
    [TextArea]
    public string textIntro1 =
        "You're late again. We have a lot to do.\nPress A to continue";
    [TextArea]
    public string textIntro2 =
        "It's your big day. Today you're about to call your first spirit guide.\nPress A to continue";
    [TextArea]
    public string textIntro3 =
        "Now I need to make a potion. Grab a basket and bring me TWILIGHT AMETHYSTCAP ×3. These shiny mushrooms grow only on pumpkin fields.\nPress A to continue";

    [Header("Texts - Mushrooms / Bone / Knife")]
    [TextArea] public string textTryAgain = "Try one more time";
    [TextArea]
    public string textMushroomsOk =
        "Perfect! Now leave the Basket, I need you to bring me BONE ×1. You can find some at the graveyard. I think I left the key somewhere here...\nPress A to continue";
    [TextArea]
    public string textBoneOk =
        "Good job! Hurry up, now its time for the final ingredient- bring me the ritual knife. I buried it in the grave, but I lost the key long time ago on my garden.\nPress A to continue";
    [TextArea]
    public string textKnifeOk =
        "It is time now. You are ready. Take the potion and go to the ritual stone. Good luck...\nPress A to continue";

    [Header("Texts - Generic")]
    [TextArea] public string textDontNeedThis = "I don't need this";

    void Start()
    {
        defaultRotation = transform.rotation;
        if (witchMessage != null)
        {
            witchMessage.text = "";
        }
    }

    void Update()
    {
        if (stage == QuestStage.Finished)
            return;

        float dist = Vector3.Distance(player.position, transform.position);
        bool nowInside = dist < maxDistance;

        // вход в зону
        if (nowInside && !playerInside)
        {
            playerInside = true;
            OnPlayerEnter();
        }

        // выход из зоны
        if (!nowInside && playerInside)
        {
            playerInside = false;
            OnPlayerExit();
        }

        // реакция на кнопку A
        if (playerInside && waitingForA && InputBridge.Instance.AButtonDown)
        {
            OnAButton();
        }
    }

    // ================== ЛОГИКА КВЕСТА ==================

    void OnPlayerEnter()
    {
        switch (stage)
        {
            case QuestStage.Intro1:
                ShowText(textIntro1, true);
                break;

            case QuestStage.Intro2:
                ShowText(textIntro2, true);
                break;

            case QuestStage.ExplainMushrooms:
                ShowText(textIntro3, true);
                break;

            case QuestStage.MushroomsTaskActive:
                HandleMushroomStageEnter();
                break;
            case QuestStage.BoneTaskActive:
                HandleBoneStageEnter();
                break;

            case QuestStage.KnifeTaskActive:
                HandleKnifeStageEnter();
                break;
        }
    }

    void OnPlayerExit()
    {
        // при уходе игрока просто убираем текст и отворачиваемся
        HideText();
        StartCoroutine(TurnAway());
        waitingForA = false;
    }

    void OnAButton()
    {
        switch (stage)
        {
            case QuestStage.Intro1:
                // Переходим ко 2-й реплике
                stage = QuestStage.Intro2;
                ShowText(textIntro2, true);
                break;

            case QuestStage.Intro2:
                // Переходим к заданию с грибами (объяснение)
                stage = QuestStage.ExplainMushrooms;
                ShowText(textIntro3, true);
                break;

            case QuestStage.ExplainMushrooms:
                // Игрок принял задание – прячем текст, начинаем собирать грибы
                stage = QuestStage.MushroomsTaskActive;
                waitingForA = false;
                HideText();
                StartCoroutine(TurnAway());
                break;

            case QuestStage.MushroomsTaskActive:
                // Нажатие A здесь возможно только после текста textMushroomsOk
                AcceptMushroomsAndStartBoneQuest();
                break;

            case QuestStage.BoneTaskActive:
                // Нажатие A здесь возможно только после текста textBoneOk
                AcceptBoneAndStartKnifeQuest();
                break;

            case QuestStage.KnifeTaskActive:
                // Нажатие A здесь возможно только после текста textKnifeOk
                AcceptKnifeAndFinish();
                break;
        }
    }

    // ---------- Этап грибов ----------

    void HandleMushroomStageEnter()
    {
        if (basket == null)
            return;

        // ведьма реагирует только если игрок держит корзину
        if (!basket.BeingHeld)
            return;

        int capsInBasket = CountTwilightCapsInBasket();

        if (capsInBasket >= 3)
        {
            // верный вариант – диалог с переходом по A
            ShowText(textMushroomsOk, true);
        }
        else
        {
            // корзина есть, но грибов мало
            ShowText(textTryAgain, false);
        }
    }

    int CountTwilightCapsInBasket()
    {
        if (basket == null || twilightCaps == null)
            return 0;

        int count = 0;

        foreach (var cap in twilightCaps)
        {
            if (cap == null) continue;

            // считаем, что гриб "в корзине", если он находится в её иерархии
            if (cap.transform.IsChildOf(basket.transform))
            {
                count++;
            }
        }

        return count;
    }

    void AcceptMushroomsAndStartBoneQuest()
    {
        // Удаляем все грибы, которые лежат в корзине
        if (basket != null && twilightCaps != null)
        {
            foreach (var cap in twilightCaps)
            {
                if (cap != null && cap.transform.IsChildOf(basket.transform))
                {
                    Destroy(cap.gameObject);
                }
            }
        }

        stage = QuestStage.BoneTaskActive;
        waitingForA = false;
        HideText();
        StartCoroutine(TurnAway());
    }

    // ---------- Этап кости ----------

    void HandleBoneStageEnter()
    {
        // Проверяем, держит ли игрок одну из костей
        currentHeldBone = GetHeldBone();

        if (currentHeldBone != null)
        {
            ShowText(textBoneOk, true);
        }
        else
        {
            // пришёл с чем-то ещё или с пустыми руками
            ShowText(textDontNeedThis, false);
        }
    }

    Grabbable GetHeldBone()
    {
        if (bones == null)
            return null;

        foreach (var b in bones)
        {
            if (b != null && b.BeingHeld)
                return b;
        }

        return null;
    }
    void AcceptBoneAndStartKnifeQuest()
    {
        if (currentHeldBone != null)
        {
            Destroy(currentHeldBone.gameObject);
            currentHeldBone = null;
        }

        stage = QuestStage.KnifeTaskActive;
        waitingForA = false;
        HideText();
        StartCoroutine(TurnAway());
    }

    // -------------- Этап ножа -----------------

    void HandleKnifeStageEnter()
    {
        if (ritualKnife != null && ritualKnife.BeingHeld)
        {
            ShowText(textKnifeOk, true);
        }
        else
        {
            ShowText(textDontNeedThis, false);
        }
    }

    void AcceptKnifeAndFinish()
    {
        // Уничтожаем нож в руке
        if (ritualKnife != null)
        {
            Destroy(ritualKnife.gameObject);
        }

        // Включаем появление зелия через портал + музыку
        if (healingAppearController != null)
        {
            healingAppearController.StartHealingAppearance();
        }
        else
        {
            Debug.LogWarning("HealingAppearController is not assigned in Inspector!");
        }

        // Активируем модель зелья (само зелье появляется резко — как ты хотела)
        if (magicPotion != null)
        {
            magicPotion.gameObject.SetActive(true);
        }

        // Переход к финальному состоянию
        stage = QuestStage.Finished;
        waitingForA = false;
        HideText();
        StartCoroutine(TurnAway());
    }

    // ================== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ==================

    void ShowText(string text, bool needA)
    {
        if (witchMessage != null)
        {
            witchMessage.text = text;
        }

        waitingForA = needA;
        StartCoroutine(TurnToPlayer());
    }

    void HideText()
    {
        if (witchMessage != null)
        {
            witchMessage.text = "";
        }
    }

    private IEnumerator TurnToPlayer()
    {
        if (isTurning) yield break;
        isTurning = true;

        while (true)
        {
            Vector3 targetPos = new Vector3(player.position.x, transform.position.y, player.position.z);
            Vector3 direction = (targetPos - transform.position).normalized;
            Quaternion targetRot = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);

            if (Quaternion.Angle(transform.rotation, targetRot) < 1f)
                break;

            yield return null;
        }

        isTurning = false;
    }

    private IEnumerator TurnAway()
    {
        if (isTurning) yield break;
        isTurning = true;

        while (true)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, defaultRotation, Time.deltaTime * rotationSpeed);

            if (Quaternion.Angle(transform.rotation, defaultRotation) < 1f)
                break;

            yield return null;
        }

        isTurning = false;
    }
}