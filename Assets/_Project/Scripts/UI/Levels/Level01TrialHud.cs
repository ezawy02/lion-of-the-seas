using System;
using SeaLion.Gameplay.Input;
using SeaLion.Gameplay.Flagship;
using SeaLion.Gameplay.Levels;
using SeaLion.UI.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace SeaLion.UI.Levels
{
    /// <summary>Self-contained, safe-area portrait HUD for the direct Level 1 trial.</summary>
    [RequireComponent(typeof(Level01TrialRuntime))]
    public sealed partial class Level01TrialHud : MonoBehaviour
    {
        private static readonly Color Ink = new Color(0.025f, 0.09f, 0.12f, 0.94f);
        private static readonly Color Teal = new Color(0.04f, 0.46f, 0.48f, 0.96f);
        private static readonly Color Gold = new Color(0.95f, 0.68f, 0.18f, 1f);
        private static readonly Color Pale = new Color(0.78f, 0.94f, 0.94f, 1f);
        [Header("Control HUD review assets")]
        [SerializeField] private Sprite steeringIcon;
        [SerializeField] private Sprite captainAbilityIcon;
        [SerializeField] private Sprite circularUiSprite;
        private Level01TrialRuntime runtime;
        private FlagshipInputAdapter input;
        private FlagshipController flagship;
        private GameLanguage language;
        private RectTransform safeArea;
        private Font englishFont;
        private Text stage, phase, force, gate, boss, ability, abilityState, abilityPercent;
        private Text steeringHint, steeringSubHint, leftArrow, rightArrow, result, reward, retry;
        private Text fireLabel, reloadLabel;
        private Text englishToggle, arabicToggle;
        private Slider bossHealth;
        private Image abilityCharge, reloadCharge;
        private Button abilityButton, fireButton, retryButton;
        private GameObject bossCard, resultOverlay, steeringRoot, abilityRoot, fireRoot;
        private RectTransform steeringWheel, abilityTransform;
        private Level01SteeringHoldButton leftSteering, rightSteering;
        private Vector2 steeringWheelHome;
        private Rect lastSafeArea;
        private bool hasSteered;

        private void Awake()
        {
            runtime = GetComponent<Level01TrialRuntime>();
            language = Level01TrialLocalization.LoadLanguage();
            Build();
        }

        private void OnEnable()
        {
            runtime.PhaseChanged += OnPhase;
            runtime.StateChanged += Refresh;
        }

        private void OnDisable()
        {
            runtime.PhaseChanged -= OnPhase;
            runtime.StateChanged -= Refresh;
        }

        private void LateUpdate()
        {
            ApplySafeArea();
            Refresh();
        }

        private void Build()
        {
            var canvas = Create("Level01TrialHUD", transform, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvasComponent = canvas.GetComponent<Canvas>();
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasComponent.sortingOrder = 50;
            var scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(720f, 1280f);
            scaler.matchWidthOrHeight = 0.5f;

            safeArea = Rect(canvas.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var top = Panel(safeArea, "Command Deck", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-360f, -180f), new Vector2(360f, 0f), Ink);
            stage = Label(top.transform, "Stage", new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.78f), new Vector2(-310f, -17f), new Vector2(310f, 17f), 18, TextAnchor.MiddleCenter, Gold);
            phase = Label(top.transform, "Phase", new Vector2(0.5f, 0.49f), new Vector2(0.5f, 0.49f), new Vector2(-320f, -25f), new Vector2(320f, 25f), 28, TextAnchor.MiddleCenter, Color.white);
            force = Label(top.transform, "Force", new Vector2(0.08f, 0.18f), new Vector2(0.48f, 0.18f), new Vector2(0f, -16f), new Vector2(0f, 16f), 17, TextAnchor.MiddleLeft, new Color(0.65f, 0.88f, 0.9f));
            gate = Label(top.transform, "Gate", new Vector2(0.52f, 0.18f), new Vector2(0.92f, 0.18f), new Vector2(0f, -16f), new Vector2(0f, 16f), 15, TextAnchor.MiddleRight, new Color(0.65f, 0.88f, 0.9f));
            var languages = Panel(top.transform, "Languages", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-190f, -66f), new Vector2(-40f, -16f), new Color(0f, 0f, 0f, 0.22f));
            englishToggle = Button(languages.transform, "English", new Vector2(0f, 0f), new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero, "EN", () => SetLanguage(GameLanguage.English));
            arabicToggle = Button(languages.transform, "Arabic", new Vector2(0.5f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, "ع", () => SetLanguage(GameLanguage.Arabic));

            bossCard = Panel(safeArea, "Guardian Card", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-290f, -242f), new Vector2(290f, -170f), Ink);
            boss = Label(bossCard.transform, "Guardian", new Vector2(0.5f, 0.68f), new Vector2(0.5f, 0.68f), new Vector2(-260f, -16f), new Vector2(260f, 16f), 16, TextAnchor.MiddleCenter, Gold);
            bossHealth = Bar(bossCard.transform, "Boss Health", new Vector2(0.5f, 0.31f), new Vector2(0.5f, 0.31f), new Vector2(520f, 22f), new Color(0.12f, 0.21f, 0.24f), new Color(0.84f, 0.19f, 0.16f));

            BuildControlDeck();

            resultOverlay = Panel(safeArea, "Result Overlay", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0f, 0.025f, 0.04f, 0.78f));
            var resultCard = Panel(resultOverlay.transform, "Result Card", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-286f, -195f), new Vector2(286f, 195f), new Color(0.025f, 0.1f, 0.13f, 0.99f));
            result = Label(resultCard.transform, "Result", new Vector2(0.5f, 0.74f), new Vector2(0.5f, 0.74f), new Vector2(-250f, -27f), new Vector2(250f, 27f), 33, TextAnchor.MiddleCenter, Gold);
            reward = Label(resultCard.transform, "Reward", new Vector2(0.5f, 0.47f), new Vector2(0.5f, 0.47f), new Vector2(-240f, -32f), new Vector2(240f, 32f), 18, TextAnchor.MiddleCenter, Color.white);
            retry = Button(resultCard.transform, "Retry", new Vector2(0.5f, 0.14f), new Vector2(0.5f, 0.14f), new Vector2(-145f, -26f), new Vector2(145f, 26f), "SAIL AGAIN", Retry);
            retryButton = retry.transform.parent.GetComponent<Button>();
            loadoutLabel = Button(resultCard.transform, "Change Loadout", new Vector2(.5f, .30f),
                new Vector2(.5f, .30f), new Vector2(-145f, -22f), new Vector2(145f, 22f),
                "CHANGE LOADOUT", () => GetComponent<Level01LoadoutFlow>()?.Open());
            BuildCampaignControls(resultCard.transform);
            ApplyLanguage();
            ApplySafeArea();
        }

        private void Refresh()
        {
            if (runtime == null || stage == null) return;
            stage.text = Local("stage");
            phase.text = runtime.CanRetry ? string.Empty : Local(PhaseKey(runtime.Phase));
            force.text = Level01TrialLocalization.FormatCurrentForce(runtime.ForceCount, language);
            gate.text = BuildGateText();
            bossCard.SetActive(runtime.Phase == Level01TrialPhase.Assault);
            boss.text = Local("guardian");
            bossHealth.value = runtime.BossHealth01;
            ability.text = runtime.ActiveAbility != null && runtime.ActiveAbility.GameplayEffect.Outcome == SeaLion.Core.Definitions.GateOutcome.Damage
                ? (language == GameLanguage.Arabic ? SeaLion.UI.Localization.ArabicTextShaper.Shape("القصف") : "BARRAGE") : Local("abilityShort");
            abilityCharge.fillAmount = runtime.AbilityCharge01;
            abilityState.text = Local(runtime.AbilityReady ? "ready" : "charging");
            abilityPercent.text = runtime.AbilityReady ? string.Empty :
                Level01TrialLocalization.FormatPercent(runtime.AbilityCharge01, language);
            abilityButton.interactable = runtime.AbilityReady;
            var inAssault = (runtime.Phase == Level01TrialPhase.Assault || runtime.BlockadeActive) && !runtime.CanRetry;
            var inLanding = runtime.Phase == Level01TrialPhase.Landing && !runtime.CanRetry;
            fireRoot.SetActive((inAssault || inLanding));
            fireButton.interactable = (inAssault && runtime.CanPrimaryAttack) ||
                (inLanding && runtime.CanAssistLanding);
            reloadCharge.fillAmount = runtime.PrimaryAttackReady01;
            fireLabel.text = language == GameLanguage.Arabic ? "إطلاق" : "FIRE";
            if (inLanding)
                reloadLabel.text = Local("landingAssist");
            else reloadLabel.text = runtime.CanPrimaryAttack ?
                (language == GameLanguage.Arabic ? "جاهز" : "READY") :
                (language == GameLanguage.Arabic ? "إعادة التلقيم" : "RELOADING");
            RefreshControlDeck();
            RefreshCampaign();
            resultOverlay.SetActive(runtime.CanRetry);
            if (!runtime.CanRetry) return;
            result.text = Local(runtime.Phase == Level01TrialPhase.Victory ? "victory" : "failure");
            if (runtime.Phase == Level01TrialPhase.Failure)
                reward.text = Local(Level01TrialLocalization.FailureKey(runtime.FailureReason));
            else reward.text = !runtime.RewardResult.HasValue ? string.Empty :
                runtime.RewardResult.Value.Succeeded ? RewardDescription() :
                Local("rewardFailure");
            retry.text = Local("retry");
            loadoutLabel.text = language == GameLanguage.Arabic ? SeaLion.UI.Localization.ArabicTextShaper.Shape("تغيير التجهيزات") : "CHANGE LOADOUT";
            retryButton.interactable = true;
        }

        private Text loadoutLabel;

        private string RewardDescription()
        {
            var item = runtime.RewardResult.Value;
            if (runtime.LevelNumber > 1) return CampaignReward(item);
            var heading = item.AlreadyGranted ? (language == GameLanguage.Arabic ? "مخطط مملوك بالفعل" : "BLUEPRINT ALREADY OWNED") : Local("reward");
            var text = heading + "\n" + Local("rewardBody");
            return item.AlreadyGranted && language == GameLanguage.Arabic
                ? SeaLion.UI.Localization.ArabicTextShaper.Shape("مخطط مملوك بالفعل") + "\n" + Local("rewardBody") : text;
        }

        private void SetLanguage(GameLanguage value)
        {
            language = value;
            Level01TrialLocalization.SaveLanguage(value);
            ApplyLanguage();
            Refresh();
        }

        private void ApplyLanguage()
        {
            var arabic = language == GameLanguage.Arabic;
            var font = arabic ? RuntimeArabicFont.Resolve(englishFont) : englishFont;
            foreach (var label in GetComponentsInChildren<Text>(true))
                label.font = font;
            stage.alignment = phase.alignment = boss.alignment = ability.alignment =
                abilityState.alignment = abilityPercent.alignment = steeringHint.alignment =
                steeringSubHint.alignment = result.alignment = reward.alignment = retry.alignment = TextAnchor.MiddleCenter;
            force.alignment = arabic ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
            gate.alignment = arabic ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
            bossHealth.direction = arabic ? Slider.Direction.RightToLeft : Slider.Direction.LeftToRight;
            englishToggle.color = language == GameLanguage.English ? Ink : Color.white;
            arabicToggle.color = language == GameLanguage.Arabic ? Ink : Color.white;
            if (fireLabel != null) fireLabel.text = arabic ? "إطلاق" : "FIRE";
            if (reloadLabel != null) reloadLabel.text = arabic ? "إعادة التلقيم" : "RELOADING";
        }

        private void ApplySafeArea()
        {
            if (safeArea == null || lastSafeArea == Screen.safeArea) return;
            lastSafeArea = Screen.safeArea;
            safeArea.anchorMin = new Vector2(lastSafeArea.xMin / Screen.width, lastSafeArea.yMin / Screen.height);
            safeArea.anchorMax = new Vector2(lastSafeArea.xMax / Screen.width, lastSafeArea.yMax / Screen.height);
            safeArea.offsetMin = safeArea.offsetMax = Vector2.zero;
        }

        private string Local(string key) { return Level01TrialLocalization.Display(key, language); }
        private void Activate() { runtime.TryActivateAbility(); }
        private void Retry() { runtime.Retry(); }
        private void OnPhase(Level01TrialPhase _) { Refresh(); }

        private void BuildControlDeck()
        {
            var circle = circularUiSprite != null ? circularUiSprite :
                Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
            steeringRoot = Panel(safeArea, "Steering Deck", Vector2.zero, Vector2.zero,
                new Vector2(28f, 30f), new Vector2(300f, 220f), Color.clear);
            steeringRoot.GetComponent<Image>().raycastTarget = false;
            steeringHint = Label(steeringRoot.transform, "Steering Hint", new Vector2(0f, 0.78f),
                new Vector2(1f, 1f), Vector2.zero, Vector2.zero, 16, TextAnchor.MiddleCenter, Gold);
            steeringSubHint = Label(steeringRoot.transform, "Steering Anywhere", new Vector2(0f, 0.62f),
                new Vector2(1f, 0.8f), Vector2.zero, Vector2.zero, 11, TextAnchor.MiddleCenter, Pale);
            var steeringBack = Panel(steeringRoot.transform, "Steering Ring", new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(-76f, 4f), new Vector2(76f, 156f), Ink);
            StyleCircle(steeringBack.GetComponent<Image>(), circle, new Color(0.02f, 0.12f, 0.15f, 0.86f));
            steeringWheel = Rect(Create("Steering Helm", steeringBack.transform, typeof(Image)).transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-58f, -58f), new Vector2(58f, 58f));
            var wheelImage = steeringWheel.GetComponent<Image>();
            wheelImage.sprite = steeringIcon;
            wheelImage.preserveAspect = true;
            wheelImage.raycastTarget = false;
            steeringWheelHome = steeringWheel.anchoredPosition;
            var leftControl = Panel(steeringRoot.transform, "Steer Left Button", Vector2.zero,
                Vector2.zero, new Vector2(0f, 48f), new Vector2(64f, 116f), Ink);
            StyleCircle(leftControl.GetComponent<Image>(), circle, new Color(0.02f, 0.12f, 0.15f, 0.94f));
            leftSteering = leftControl.AddComponent<Level01SteeringHoldButton>();
            leftArrow = Label(leftControl.transform, "Left Direction", Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero, 36, TextAnchor.MiddleCenter, Pale);
            var rightControl = Panel(steeringRoot.transform, "Steer Right Button", new Vector2(1f, 0f),
                new Vector2(1f, 0f), new Vector2(-64f, 48f), new Vector2(0f, 116f), Ink);
            StyleCircle(rightControl.GetComponent<Image>(), circle, new Color(0.02f, 0.12f, 0.15f, 0.94f));
            rightSteering = rightControl.AddComponent<Level01SteeringHoldButton>();
            rightArrow = Label(rightControl.transform, "Right Direction", Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero, 36, TextAnchor.MiddleCenter, Pale);
            leftArrow.text = "‹";
            rightArrow.text = "›";

            abilityRoot = Panel(safeArea, "Captain Ability Control", new Vector2(1f, 0f),
                new Vector2(1f, 0f), new Vector2(-198f, 222f), new Vector2(-48f, 370f), Color.clear);
            abilityRoot.GetComponent<Image>().raycastTarget = false;
            var abilityBack = Panel(abilityRoot.transform, "Ability Button", Vector2.zero, Vector2.one,
                new Vector2(12f, 12f), new Vector2(-12f, -12f), Ink);
            var abilityGraphic = abilityBack.GetComponent<Image>();
            StyleCircle(abilityGraphic, circle, new Color(0.02f, 0.12f, 0.15f, 0.96f));
            abilityGraphic.raycastTarget = true;
            abilityTransform = abilityBack.GetComponent<RectTransform>();
            abilityButton = abilityBack.AddComponent<Button>();
            abilityButton.targetGraphic = abilityGraphic;
            abilityButton.onClick.AddListener(Activate);
            abilityButton.colors = AbilityColors();
            var chargeObject = Create("Ability Charge", abilityBack.transform, typeof(Image));
            Rect(chargeObject.transform, Vector2.zero, Vector2.one, new Vector2(5f, 5f), new Vector2(-5f, -5f));
            abilityCharge = chargeObject.GetComponent<Image>();
            StyleCircle(abilityCharge, circle, new Color(0.04f, 0.62f, 0.62f, 0.82f));
            abilityCharge.type = Image.Type.Filled;
            abilityCharge.fillMethod = Image.FillMethod.Radial360;
            abilityCharge.fillOrigin = (int)Image.Origin360.Bottom;
            abilityCharge.fillClockwise = true;
            var icon = Create("Rally Icon", abilityBack.transform, typeof(Image)).GetComponent<Image>();
            Rect(icon.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-48f, -42f), new Vector2(48f, 54f));
            icon.sprite = captainAbilityIcon;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            ability = Label(abilityRoot.transform, "Ability", new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(0f, -4f), new Vector2(0f, 30f), 17, TextAnchor.MiddleCenter, Color.white);
            abilityState = Label(abilityRoot.transform, "Ability State", new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(-22f, -26f), new Vector2(22f, 8f), 13, TextAnchor.MiddleCenter, Gold);
            abilityPercent = Label(abilityBack.transform, "Ability Percent", new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(-44f, -60f), new Vector2(44f, -28f), 14,
                TextAnchor.MiddleCenter, Color.white);

            fireRoot = Panel(safeArea, "Primary Fire Control", new Vector2(1f, 0f),
                new Vector2(1f, 0f), new Vector2(-218f, 28f), new Vector2(-28f, 206f), Color.clear);
            fireRoot.GetComponent<Image>().raycastTarget = false;
            var fireBack = Panel(fireRoot.transform, "Fire Button", Vector2.zero, Vector2.one,
                new Vector2(12f, 18f), new Vector2(-12f, -18f), Ink);
            var fireGraphic = fireBack.GetComponent<Image>();
            StyleCircle(fireGraphic, circle, new Color(0.60f, 0.16f, 0.10f, 0.98f));
            fireGraphic.raycastTarget = true;
            fireButton = fireBack.AddComponent<Button>();
            fireButton.targetGraphic = fireGraphic;
            fireButton.onClick.AddListener(Fire);
            fireButton.colors = FireColors();
            var reloadObject = Create("Reload Progress", fireBack.transform, typeof(Image));
            Rect(reloadObject.transform, Vector2.zero, Vector2.one, new Vector2(5f, 5f), new Vector2(-5f, -5f));
            reloadCharge = reloadObject.GetComponent<Image>();
            StyleCircle(reloadCharge, circle, new Color(1f, 0.70f, 0.22f, 0.78f));
            reloadCharge.type = Image.Type.Filled;
            reloadCharge.fillMethod = Image.FillMethod.Radial360;
            reloadCharge.fillOrigin = (int)Image.Origin360.Bottom;
            reloadCharge.fillClockwise = true;
            BuildFireGlyph(fireBack.transform, circle);
            fireLabel = Label(fireRoot.transform, "Fire Label", new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(0f, -2f), new Vector2(0f, 24f), 16, TextAnchor.MiddleCenter, Color.white);
            reloadLabel = Label(fireRoot.transform, "Reload Label", new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(-22f, -24f), new Vector2(22f, 2f), 12, TextAnchor.MiddleCenter, Gold);
        }

        private void RefreshControlDeck()
        {
            input = input != null ? input : GetComponent<FlagshipInputAdapter>();
            flagship = flagship != null ? flagship : FindFirstObjectByType<FlagshipController>();
            if (input != null)
            {
                leftSteering.Bind(input, flagship, -1f);
                rightSteering.Bind(input, flagship, 1f);
            }
            var traversal = runtime.Phase == Level01TrialPhase.Traversal && !runtime.CanRetry;
            var assaultSteer = (runtime.Phase == Level01TrialPhase.Assault || runtime.BlockadeActive) && !runtime.CanRetry;
            steeringRoot.SetActive(traversal || assaultSteer);
            abilityRoot.SetActive(!runtime.CanRetry);
            if (input != null && input.HasSteered) hasSteered = true;
            var intent = input != null ? input.HorizontalIntent : 0f;
            steeringWheel.anchoredPosition = steeringWheelHome + new Vector2(intent * 20f, 0f);
            steeringWheel.localEulerAngles = new Vector3(0f, 0f, -intent * 18f);
            leftArrow.color = intent < -0.08f ? Gold : Pale;
            rightArrow.color = intent > 0.08f ? Gold : Pale;
            if (runtime.NeedsSteeringChoice)
            {
                steeringHint.text = Local("steerToChoose");
                steeringSubHint.text = Local("steerAnywhere");
            }
            else if ((runtime.Phase == Level01TrialPhase.Assault || runtime.BlockadeActive) && !runtime.CanRetry)
            {
                steeringHint.text = Local("dodgeHint");
                steeringSubHint.text = string.Empty;
            }
            else
            {
                steeringHint.text = hasSteered ? string.Empty : Local("steer");
                steeringSubHint.text = hasSteered ? string.Empty : Local("steerAnywhere");
            }
            var pulse = runtime.AbilityReady ? 1f + Mathf.Sin(Time.unscaledTime * 4.6f) * 0.025f : 1f;
            abilityTransform.localScale = Vector3.one * pulse;
        }

        private void Fire()
        {
            if (runtime.Phase == Level01TrialPhase.Assault && !runtime.TryPrimaryAttack().Fired)
                return;
            if (runtime.Phase == Level01TrialPhase.Landing)
                runtime.TryAssistLanding();
            else if (runtime.Phase != Level01TrialPhase.Assault)
                runtime.TryPrimaryAttack();
        }

        private string BuildGateText()
        {
            if (!runtime.GateCommitted)
                return GateValue(runtime.EasyGate) + "  |  " + GateValue(runtime.RiskyGate);
            return runtime.LastGateBefore + " → " + runtime.LastGateAfter;
        }

        private static string GateValue(SeaLion.Core.Definitions.GateDefinition definition)
        {
            if (definition == null) return string.Empty;
            var prefix = definition.Outcome == SeaLion.Core.Definitions.GateOutcome.Multiply ? "×" :
                definition.Outcome == SeaLion.Core.Definitions.GateOutcome.Damage ? "−" : "+";
            return prefix + definition.Value.ToString("0.#");
        }

        private static void BuildFireGlyph(Transform parent, Sprite circle)
        {
            var gold = new Color(1f, 0.86f, 0.62f, 1f);
            var ball = Create("Cannonball", parent, typeof(Image)).GetComponent<Image>();
            Rect(ball.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-22f, -12f), new Vector2(22f, 32f));
            StyleCircle(ball, circle, gold);
            for (var index = 0; index < 3; index++)
            {
                var streak = Create("Recoil Streak " + index, parent, typeof(Image)).GetComponent<Image>();
                Rect(streak.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(-58f, -21f + index * 13f), new Vector2(-27f - index * 4f, -15f + index * 13f));
                streak.color = gold;
                streak.raycastTarget = false;
                streak.rectTransform.localEulerAngles = new Vector3(0f, 0f, 12f - index * 12f);
            }
        }

        private static void StyleCircle(Image image, Sprite sprite, Color color)
        {
            image.sprite = sprite;
            image.color = color;
            image.preserveAspect = true;
            image.raycastTarget = false;
        }

        private static ColorBlock AbilityColors()
        {
            var colors = ColorBlock.defaultColorBlock;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.88f, 0.58f, 1f);
            colors.pressedColor = new Color(0.72f, 0.94f, 0.92f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(0.44f, 0.54f, 0.56f, 0.72f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            return colors;
        }

        private static ColorBlock FireColors()
        {
            var colors = AbilityColors();
            colors.highlightedColor = new Color(1f, 0.62f, 0.34f, 1f);
            colors.pressedColor = new Color(1f, 0.82f, 0.46f, 1f);
            return colors;
        }

        private Text Label(Transform parent, string name, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax, int size, TextAnchor align, Color color)
        {
            var text = Create(name, parent, typeof(Text)).GetComponent<Text>();
            Rect(text.transform, min, max, offsetMin, offsetMax);
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (englishFont == null) englishFont = text.font;
            text.fontSize = size;
            text.alignment = align;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.color = color;
            return text;
        }

        private Text Button(Transform parent, string name, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax, string caption, UnityEngine.Events.UnityAction action)
        {
            var panel = Panel(parent, name, min, max, offsetMin, offsetMax, Gold);
            var button = panel.AddComponent<Button>();
            button.targetGraphic = panel.GetComponent<Image>();
            button.onClick.AddListener(action);
            var label = Label(panel.transform, name + " Label", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, 16, TextAnchor.MiddleCenter, Ink);
            label.text = caption;
            return label;
        }

        private Slider Bar(Transform parent, string name, Vector2 min, Vector2 max, Vector2 size, Color background, Color fill)
        {
            var root = Panel(parent, name, min, max, -size * 0.5f, size * 0.5f, background);
            var fillObject = Panel(root.transform, "Fill", Vector2.zero, Vector2.one, new Vector2(3f, 3f), new Vector2(-3f, -3f), fill);
            var slider = root.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.fillRect = fillObject.GetComponent<RectTransform>();
            slider.targetGraphic = root.GetComponent<Image>();
            slider.handleRect = null;
            slider.interactable = false;
            slider.direction = Slider.Direction.LeftToRight;
            return slider;
        }

        private static GameObject Panel(Transform parent, string name, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var panel = Create(name, parent, typeof(Image));
            panel.GetComponent<Image>().color = color;
            Rect(panel.transform, min, max, offsetMin, offsetMax);
            return panel;
        }

        private static RectTransform Rect(Transform transform, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
        {
            var rect = (RectTransform)transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            return rect;
        }

        private static GameObject Create(string name, Transform parent, params Type[] types)
        {
            var value = new GameObject(name, types);
            value.transform.SetParent(parent, false);
            return value;
        }

        private static string PhaseKey(Level01TrialPhase phase)
        {
            switch (phase)
            {
                case Level01TrialPhase.Assault: return "assault";
                case Level01TrialPhase.Victory: return "victory";
                case Level01TrialPhase.Failure: return "failure";
                case Level01TrialPhase.Landing: return "landing";
                case Level01TrialPhase.Traversal: return "traversal";
                default: return "opening";
            }
        }

    }
}
