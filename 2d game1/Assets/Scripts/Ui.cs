using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI : MonoBehaviour
{
    public static UI instance;

    [SerializeField] private GameObject gameOverUI;
    [Space]

    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI killCountText;

    private int killCount;
    private float gameStartTime;

    // Procedural Start Menu Master Panel
    private GameObject startMenuPanel;

    // Sequential Sub-Panels
    private GameObject heroPanel;
    private GameObject diffPanel;
    private GameObject mapPanel;
    private GameObject loadingPanel;

    // Hero Preview Elements
    private TextMeshProUGUI previewNameText;
    private TextMeshProUGUI previewDescText;
    private TextMeshProUGUI previewAbilityText;
    private RectTransform hpBarFill;
    private RectTransform speedBarFill;
    private RectTransform jumpBarFill;

    // Loading Screen Elements
    private RectTransform loadingProgressFill;
    private TextMeshProUGUI loadingTipText;

    // Interactive selections visual tracking
    private UnityEngine.UI.Image[] charButtons = new UnityEngine.UI.Image[4];
    private UnityEngine.UI.Image[] diffButtons = new UnityEngine.UI.Image[3];
    private UnityEngine.UI.Image[] mapCards = new UnityEngine.UI.Image[4];

    // In-game Ability HUD
    private GameObject abilityHudContainer;
    private RectTransform hudAbilityFill;
    private TextMeshProUGUI hudAbilityText;

    private void Awake()
    {
        instance = this;
        Time.timeScale = 0f; // Ensure paused on start
    }

    private void Start()
    {
        // Safety: Ensure GameManager and AudioManager are initialized
        if (GameManager.instance == null)
        {
            GameObject gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();
        }

        // Restart looping chiptune BGM if it was stopped on GameOver
        if (AudioManager.instance != null)
            AudioManager.instance.RestartBGM();

        // Build all panels procedurally
        BuildStartMenu();

        // Select Knight, Normal difficulty, Forest map initially
        UpdateCharacterSelectionUI(GameManager.CharacterType.Knight);
        UpdateDifficultySelectionUI(GameManager.Difficulty.Normal);
        UpdateMapSelectionUI(GameManager.MapType.Forest);

        // Show the first panel (Hero Selection)
        ShowMenuStep(1);
    }

    private void Update()
    {
        // 1. Timer Update
        if (GameManager.instance != null && GameManager.instance.isGameActive)
        {
            float elapsed = Time.time - gameStartTime;
            timerText.text = elapsed.ToString("F2") + "s";
        }
        else
        {
            timerText.text = "0.00s";
        }

        // 2. Cooldown HUD Update
        UpdateAbilityHUD();
    }

    public void EnableGameOverUI()
    {
        Time.timeScale = 0.4f;
        gameOverUI.SetActive(true);
        if (abilityHudContainer != null)
            abilityHudContainer.SetActive(false); // Hide ability HUD on game over
    }

    public void RestartLevel()
    {
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(sceneIndex);
    }

    public void AddKillCount()
    {
        killCount++;
        killCountText.text = killCount.ToString();
    }

    // Step-by-Step Transition Switcher
    private void ShowMenuStep(int step)
    {
        if (heroPanel != null) heroPanel.SetActive(step == 1);
        if (diffPanel != null) diffPanel.SetActive(step == 2);
        if (mapPanel != null) mapPanel.SetActive(step == 3);
        if (loadingPanel != null) loadingPanel.SetActive(step == 4);

        if (step == 4)
        {
            StartCoroutine(LoadingProgressRoutine());
        }
    }

    // Interactive elements behavior component
    public class UIInteractiveElement : MonoBehaviour, 
        UnityEngine.EventSystems.IPointerEnterHandler, 
        UnityEngine.EventSystems.IPointerExitHandler, 
        UnityEngine.EventSystems.IPointerClickHandler
    {
        public System.Action onClick;
        public Color normalColor = new Color(0.1f, 0.1f, 0.14f, 0.85f);
        public Color hoverColor = new Color(0.18f, 0.18f, 0.25f, 0.95f);
        public Color selectedColor = new Color(0.3f, 0.25f, 0.55f, 1f);
        public bool isSelected = false;

        public Vector3 normalScale = Vector3.one;
        public Vector3 hoverScale = new Vector3(1.05f, 1.05f, 1.05f);

        private UnityEngine.UI.Image img;

        private void Awake()
        {
            img = GetComponent<UnityEngine.UI.Image>();
        }

        private void Start()
        {
            RefreshVisuals();
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            RefreshVisuals();
        }

        private void RefreshVisuals()
        {
            if (img != null)
                img.color = isSelected ? selectedColor : normalColor;
        }

        public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
        {
            transform.localScale = hoverScale;
            if (img != null && !isSelected)
                img.color = hoverColor;

            if (AudioManager.instance != null)
                AudioManager.instance.PlayHoverSound();
        }

        public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
        {
            transform.localScale = normalScale;
            if (img != null)
                img.color = isSelected ? selectedColor : normalColor;
        }

        public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (onClick != null)
                onClick.Invoke();

            if (AudioManager.instance != null)
                AudioManager.instance.PlayClickSound();
        }
    }

    private void BuildStartMenu()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            canvas = FindFirstObjectByType<Canvas>();

        if (canvas == null) return;

        // Master Overlay Panel
        startMenuPanel = new GameObject("StartMenuPanel", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        RectTransform panelRt = startMenuPanel.GetComponent<RectTransform>();
        panelRt.SetParent(canvas.transform, false);
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.sizeDelta = Vector2.zero;

        UnityEngine.UI.Image panelImg = startMenuPanel.GetComponent<UnityEngine.UI.Image>();
        panelImg.color = new Color(0.04f, 0.04f, 0.07f, 0.97f);

        // ==========================================
        // STEP 1: HERO SELECTION PANEL
        // ==========================================
        heroPanel = new GameObject("HeroSelectionPanel", typeof(RectTransform));
        RectTransform heroRt = heroPanel.GetComponent<RectTransform>();
        heroRt.SetParent(startMenuPanel.transform, false);
        heroRt.anchorMin = Vector2.zero;
        heroRt.anchorMax = Vector2.one;
        heroRt.sizeDelta = Vector2.zero;

        CreatePanelTitle("CHOOSE YOUR HERO", heroPanel.transform);

        // Grid for Hero selection cards
        GameObject heroGrid = new GameObject("HeroGrid", typeof(RectTransform));
        RectTransform gridRt = heroGrid.GetComponent<RectTransform>();
        gridRt.SetParent(heroPanel.transform, false);
        gridRt.anchorMin = new Vector2(0.05f, 0.2f);
        gridRt.anchorMax = new Vector2(0.55f, 0.8f);
        gridRt.sizeDelta = Vector2.zero;

        string[] charNames = { "KNIGHT", "ROGUE", "WIZARD", "WARRIOR" };
        GameManager.CharacterType[] charTypes = { GameManager.CharacterType.Knight, GameManager.CharacterType.Rogue, GameManager.CharacterType.Wizard, GameManager.CharacterType.Warrior };
        Color[] charColors = { new Color(1f, 0.84f, 0.3f), new Color(0.3f, 0.9f, 0.4f), new Color(0.6f, 0.4f, 0.95f), new Color(0.9f, 0.3f, 0.3f) };

        for (int i = 0; i < 4; i++)
        {
            float yMin = 0.72f - (i * 0.23f);
            float yMax = yMin + 0.18f;

            GameObject charBtnObj = new GameObject("CharBtn_" + charNames[i], typeof(RectTransform), typeof(UnityEngine.UI.Image));
            RectTransform btnRt = charBtnObj.GetComponent<RectTransform>();
            btnRt.SetParent(heroGrid.transform, false);
            btnRt.anchorMin = new Vector2(0f, yMin);
            btnRt.anchorMax = new Vector2(1f, yMax);
            btnRt.sizeDelta = Vector2.zero;

            UnityEngine.UI.Image img = charBtnObj.GetComponent<UnityEngine.UI.Image>();
            charButtons[i] = img;

            GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform tRt = textObj.GetComponent<RectTransform>();
            tRt.SetParent(charBtnObj.transform, false);
            tRt.anchorMin = new Vector2(0.08f, 0f);
            tRt.anchorMax = new Vector2(0.92f, 1f);
            tRt.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
            tmp.text = charNames[i];
            tmp.fontSize = 22;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.color = charColors[i];

            var interact = charBtnObj.AddComponent<UIInteractiveElement>();
            interact.normalColor = new Color(0.08f, 0.08f, 0.12f, 0.9f);
            interact.hoverColor = new Color(0.16f, 0.16f, 0.24f, 0.95f);
            interact.selectedColor = new Color(charColors[i].r * 0.4f, charColors[i].g * 0.4f, charColors[i].b * 0.4f, 0.95f);

            int index = i;
            interact.onClick = () => {
                GameManager.instance.currentCharacter = charTypes[index];
                UpdateCharacterSelectionUI(charTypes[index]);
            };
        }

        // Hero Preview panel (Right-hand block in Step 1)
        GameObject prevPanel = new GameObject("PreviewPanel", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        RectTransform prevRt = prevPanel.GetComponent<RectTransform>();
        prevRt.SetParent(heroPanel.transform, false);
        prevRt.anchorMin = new Vector2(0.58f, 0.2f);
        prevRt.anchorMax = new Vector2(0.95f, 0.8f);
        prevRt.sizeDelta = Vector2.zero;

        prevPanel.GetComponent<UnityEngine.UI.Image>().color = new Color(0.06f, 0.06f, 0.1f, 0.9f);

        // Preview Name Text
        GameObject prevName = new GameObject("PrevName", typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform prevNameRt = prevName.GetComponent<RectTransform>();
        prevNameRt.SetParent(prevPanel.transform, false);
        prevNameRt.anchorMin = new Vector2(0.05f, 0.85f);
        prevNameRt.anchorMax = new Vector2(0.95f, 0.97f);
        prevNameRt.sizeDelta = Vector2.zero;

        previewNameText = prevName.GetComponent<TextMeshProUGUI>();
        previewNameText.fontSize = 28;
        previewNameText.fontStyle = FontStyles.Bold;
        previewNameText.alignment = TextAlignmentOptions.Center;

        // Preview Description Text
        GameObject prevDesc = new GameObject("PrevDesc", typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform prevDescRt = prevDesc.GetComponent<RectTransform>();
        prevDescRt.SetParent(prevPanel.transform, false);
        prevDescRt.anchorMin = new Vector2(0.05f, 0.68f);
        prevDescRt.anchorMax = new Vector2(0.95f, 0.82f);
        prevDescRt.sizeDelta = Vector2.zero;

        previewDescText = prevDesc.GetComponent<TextMeshProUGUI>();
        previewDescText.fontSize = 14;
        previewDescText.alignment = TextAlignmentOptions.Center;
        previewDescText.color = new Color(0.85f, 0.85f, 0.9f, 1f);

        // Preview Ability Text (New for abilities)
        GameObject prevAbility = new GameObject("PrevAbility", typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform prevAbilityRt = prevAbility.GetComponent<RectTransform>();
        prevAbilityRt.SetParent(prevPanel.transform, false);
        prevAbilityRt.anchorMin = new Vector2(0.05f, 0.46f);
        prevAbilityRt.anchorMax = new Vector2(0.95f, 0.64f);
        prevAbilityRt.sizeDelta = Vector2.zero;

        previewAbilityText = prevAbility.GetComponent<TextMeshProUGUI>();
        previewAbilityText.fontSize = 13;
        previewAbilityText.alignment = TextAlignmentOptions.Center;

        // Stats bars
        hpBarFill = CreateStatBar("HEALTH (HP)", prevPanel.transform, 0.32f, new Color(0.9f, 0.3f, 0.3f));
        speedBarFill = CreateStatBar("MOVEMENT SPEED", prevPanel.transform, 0.19f, new Color(0.3f, 0.8f, 0.9f));
        jumpBarFill = CreateStatBar("JUMP HEIGHT", prevPanel.transform, 0.06f, new Color(0.3f, 0.9f, 0.4f));

        // "LOCK IN HERO" Button at the bottom of Hero Selection
        GameObject lockHeroBtn = new GameObject("LockHeroBtn", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        RectTransform lockRt = lockHeroBtn.GetComponent<RectTransform>();
        lockRt.SetParent(heroPanel.transform, false);
        lockRt.anchorMin = new Vector2(0.35f, 0.05f);
        lockRt.anchorMax = new Vector2(0.65f, 0.15f);
        lockRt.sizeDelta = Vector2.zero;

        lockHeroBtn.GetComponent<UnityEngine.UI.Image>().color = new Color(0.2f, 0.15f, 0.45f, 1f);

        GameObject lockText = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform ltRt = lockText.GetComponent<RectTransform>();
        ltRt.SetParent(lockHeroBtn.transform, false);
        ltRt.anchorMin = Vector2.zero;
        ltRt.anchorMax = Vector2.one;
        ltRt.sizeDelta = Vector2.zero;

        TextMeshProUGUI lockTmp = lockText.GetComponent<TextMeshProUGUI>();
        lockTmp.text = "SELECT CHARACTER";
        lockTmp.fontSize = 18;
        lockTmp.fontStyle = FontStyles.Bold;
        lockTmp.alignment = TextAlignmentOptions.Center;
        lockTmp.color = Color.white;

        var lockInteract = lockHeroBtn.AddComponent<UIInteractiveElement>();
        lockInteract.normalColor = new Color(0.25f, 0.15f, 0.5f, 1f);
        lockInteract.hoverColor = new Color(0.35f, 0.22f, 0.7f, 1f);
        lockInteract.onClick = () => ShowMenuStep(2); // Go to difficulty page!

        // ==========================================
        // STEP 2: DIFFICULTY SELECTION PANEL
        // ==========================================
        diffPanel = new GameObject("DifficultySelectionPanel", typeof(RectTransform));
        RectTransform diffRt = diffPanel.GetComponent<RectTransform>();
        diffRt.SetParent(startMenuPanel.transform, false);
        diffRt.anchorMin = Vector2.zero;
        diffRt.anchorMax = Vector2.one;
        diffRt.sizeDelta = Vector2.zero;

        CreatePanelTitle("CHOOSE YOUR DIFFICULTY", diffPanel.transform);

        string[] diffNames = { "NORMAL", "MEDIUM", "HIGH" };
        GameManager.Difficulty[] diffTypes = { GameManager.Difficulty.Normal, GameManager.Difficulty.Medium, GameManager.Difficulty.High };
        Color[] diffColors = { new Color(0.4f, 0.9f, 0.4f), new Color(0.9f, 0.8f, 0.3f), new Color(0.9f, 0.3f, 0.3f) };
        string[] diffDescs = { "Standard combat damage. Take normal damage.", "Enhanced hazards. Incoming damage is doubled!", "Ultimate deathmatch. Incoming damage is tripled!" };

        for (int i = 0; i < 3; i++)
        {
            float xMin = 0.12f + (i * 0.27f);
            float xMax = xMin + 0.22f;

            GameObject card = new GameObject("DiffCard_" + diffNames[i], typeof(RectTransform), typeof(UnityEngine.UI.Image));
            RectTransform cRt = card.GetComponent<RectTransform>();
            cRt.SetParent(diffPanel.transform, false);
            cRt.anchorMin = new Vector2(xMin, 0.3f);
            cRt.anchorMax = new Vector2(xMax, 0.7f);
            cRt.sizeDelta = Vector2.zero;

            diffButtons[i] = card.GetComponent<UnityEngine.UI.Image>();

            // Title
            GameObject title = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform tRt = title.GetComponent<RectTransform>();
            tRt.SetParent(card.transform, false);
            tRt.anchorMin = new Vector2(0.05f, 0.75f);
            tRt.anchorMax = new Vector2(0.95f, 0.92f);
            tRt.sizeDelta = Vector2.zero;

            TextMeshProUGUI tTmp = title.GetComponent<TextMeshProUGUI>();
            tTmp.text = diffNames[i];
            tTmp.fontSize = 24;
            tTmp.fontStyle = FontStyles.Bold;
            tTmp.alignment = TextAlignmentOptions.Center;
            tTmp.color = diffColors[i];

            // Desc
            GameObject desc = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform dRt = desc.GetComponent<RectTransform>();
            dRt.SetParent(card.transform, false);
            dRt.anchorMin = new Vector2(0.08f, 0.15f);
            dRt.anchorMax = new Vector2(0.92f, 0.65f);
            dRt.sizeDelta = Vector2.zero;

            TextMeshProUGUI dTmp = desc.GetComponent<TextMeshProUGUI>();
            dTmp.text = diffDescs[i];
            dTmp.fontSize = 14;
            dTmp.alignment = TextAlignmentOptions.Center;
            dTmp.color = new Color(0.8f, 0.8f, 0.85f, 1f);

            var interact = card.AddComponent<UIInteractiveElement>();
            interact.normalColor = new Color(0.06f, 0.06f, 0.1f, 0.9f);
            interact.hoverColor = new Color(0.14f, 0.14f, 0.2f, 0.95f);
            interact.selectedColor = new Color(diffColors[i].r * 0.3f, diffColors[i].g * 0.3f, diffColors[i].b * 0.3f, 0.9f);

            int index = i;
            interact.onClick = () => {
                GameManager.instance.currentDifficulty = diffTypes[index];
                UpdateDifficultySelectionUI(diffTypes[index]);
            };
        }

        // Lock in Difficulty Button
        GameObject lockDiffBtn = new GameObject("LockDiffBtn", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        RectTransform ldRt = lockDiffBtn.GetComponent<RectTransform>();
        ldRt.SetParent(diffPanel.transform, false);
        ldRt.anchorMin = new Vector2(0.35f, 0.12f);
        ldRt.anchorMax = new Vector2(0.65f, 0.22f);
        ldRt.sizeDelta = Vector2.zero;

        lockDiffBtn.GetComponent<UnityEngine.UI.Image>().color = new Color(0.2f, 0.15f, 0.45f, 1f);

        GameObject lockDiffText = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform ldtRt = lockDiffText.GetComponent<RectTransform>();
        ldtRt.SetParent(lockDiffBtn.transform, false);
        ldtRt.anchorMin = Vector2.zero;
        ldtRt.anchorMax = Vector2.one;
        ldtRt.sizeDelta = Vector2.zero;

        TextMeshProUGUI ldTmp = lockDiffText.GetComponent<TextMeshProUGUI>();
        ldTmp.text = "SELECT DIFFICULTY";
        ldTmp.fontSize = 18;
        ldTmp.fontStyle = FontStyles.Bold;
        ldTmp.alignment = TextAlignmentOptions.Center;
        ldTmp.color = Color.white;

        var ldInteract = lockDiffBtn.AddComponent<UIInteractiveElement>();
        ldInteract.normalColor = new Color(0.25f, 0.15f, 0.5f, 1f);
        ldInteract.hoverColor = new Color(0.35f, 0.22f, 0.7f, 1f);
        ldInteract.onClick = () => ShowMenuStep(3); // Go to map selection page!

        // Back Button in Difficulty Panel
        CreateBackButton(diffPanel.transform, () => ShowMenuStep(1));

        // ==========================================
        // STEP 3: BATTLEFIELD SELECTION PANEL
        // ==========================================
        mapPanel = new GameObject("MapSelectionPanel", typeof(RectTransform));
        RectTransform mapRt = mapPanel.GetComponent<RectTransform>();
        mapRt.SetParent(startMenuPanel.transform, false);
        mapRt.anchorMin = Vector2.zero;
        mapRt.anchorMax = Vector2.one;
        mapRt.sizeDelta = Vector2.zero;

        CreatePanelTitle("CHOOSE YOUR BATTLEFIELD", mapPanel.transform);

        string[] mapNames = { "EMERALD FOREST", "SUN DESERT", "CRIMSON REALM", "FROST TUNDRA" };
        GameManager.MapType[] mapTypes = { GameManager.MapType.Forest, GameManager.MapType.Desert, GameManager.MapType.Underworld, GameManager.MapType.Tundra };
        Color[] mapColors = { new Color(0.3f, 0.7f, 0.5f), new Color(0.9f, 0.7f, 0.3f), new Color(0.8f, 0.3f, 0.3f), new Color(0.4f, 0.7f, 0.9f) };
        string[] mapDescs = { "Lush emerald natural landscape. Standard platform scaling.", "Hot arid desert sands. Warm visual layout.", "Molten underworld magma vaults. Hardcore obsidian filters.", "Frozen tundra snow peaks. Ice white platforms." };

        for (int i = 0; i < 4; i++)
        {
            float xMin = 0.05f + (i * 0.23f);
            float xMax = xMin + 0.20f;

            GameObject card = new GameObject("MapCard_" + i, typeof(RectTransform), typeof(UnityEngine.UI.Image));
            RectTransform cRt = card.GetComponent<RectTransform>();
            cRt.SetParent(mapPanel.transform, false);
            cRt.anchorMin = new Vector2(xMin, 0.3f);
            cRt.anchorMax = new Vector2(xMax, 0.7f);
            cRt.sizeDelta = Vector2.zero;

            mapCards[i] = card.GetComponent<UnityEngine.UI.Image>();

            // Title
            GameObject title = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform tRt = title.GetComponent<RectTransform>();
            tRt.SetParent(card.transform, false);
            tRt.anchorMin = new Vector2(0.05f, 0.75f);
            tRt.anchorMax = new Vector2(0.95f, 0.92f);
            tRt.sizeDelta = Vector2.zero;

            TextMeshProUGUI tTmp = title.GetComponent<TextMeshProUGUI>();
            tTmp.text = mapNames[i];
            tTmp.fontSize = 20;
            tTmp.fontStyle = FontStyles.Bold;
            tTmp.alignment = TextAlignmentOptions.Center;
            tTmp.color = mapColors[i];

            // Desc
            GameObject desc = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform dRt = desc.GetComponent<RectTransform>();
            dRt.SetParent(card.transform, false);
            dRt.anchorMin = new Vector2(0.08f, 0.15f);
            dRt.anchorMax = new Vector2(0.92f, 0.65f);
            dRt.sizeDelta = Vector2.zero;

            TextMeshProUGUI dTmp = desc.GetComponent<TextMeshProUGUI>();
            dTmp.text = mapDescs[i];
            dTmp.fontSize = 13;
            dTmp.alignment = TextAlignmentOptions.Center;
            dTmp.color = new Color(0.8f, 0.8f, 0.85f, 1f);

            var interact = card.AddComponent<UIInteractiveElement>();
            interact.normalColor = new Color(0.06f, 0.08f, 0.08f, 0.9f);
            interact.hoverColor = new Color(0.12f, 0.16f, 0.16f, 0.95f);
            interact.selectedColor = new Color(mapColors[i].r * 0.3f, mapColors[i].g * 0.3f, mapColors[i].b * 0.3f, 0.9f);

            int index = i;
            interact.onClick = () => {
                GameManager.instance.currentMap = mapTypes[index];
                UpdateMapSelectionUI(mapTypes[index]);
            };
        }

        // Start Battle Button
        GameObject lockMapBtn = new GameObject("LockMapBtn", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        RectTransform lmRt = lockMapBtn.GetComponent<RectTransform>();
        lmRt.SetParent(mapPanel.transform, false);
        lmRt.anchorMin = new Vector2(0.35f, 0.12f);
        lmRt.anchorMax = new Vector2(0.65f, 0.22f);
        lmRt.sizeDelta = Vector2.zero;

        lockMapBtn.GetComponent<UnityEngine.UI.Image>().color = new Color(0.4f, 0.15f, 0.75f, 1f);

        GameObject lockMapText = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform lmtRt = lockMapText.GetComponent<RectTransform>();
        lmtRt.SetParent(lockMapBtn.transform, false);
        lmtRt.anchorMin = Vector2.zero;
        lmtRt.anchorMax = Vector2.one;
        lmtRt.sizeDelta = Vector2.zero;

        TextMeshProUGUI lmTmp = lockMapText.GetComponent<TextMeshProUGUI>();
        lmTmp.text = "LAUNCH BATTLE";
        lmTmp.fontSize = 18;
        lmTmp.fontStyle = FontStyles.Bold;
        lmTmp.alignment = TextAlignmentOptions.Center;
        lmTmp.color = Color.white;

        var lmInteract = lockMapBtn.AddComponent<UIInteractiveElement>();
        lmInteract.normalColor = new Color(0.45f, 0.16f, 0.8f, 1f);
        lmInteract.hoverColor = new Color(0.55f, 0.22f, 0.95f, 1f);
        lmInteract.onClick = () => ShowMenuStep(4); // Advance to Loading page!

        // Back Button in Map Panel
        CreateBackButton(mapPanel.transform, () => ShowMenuStep(2));

        // ==========================================
        // STEP 4: INTERACTIVE LOADING PANEL
        // ==========================================
        loadingPanel = new GameObject("LoadingPanel", typeof(RectTransform));
        RectTransform loadRt = loadingPanel.GetComponent<RectTransform>();
        loadRt.SetParent(startMenuPanel.transform, false);
        loadRt.anchorMin = Vector2.zero;
        loadRt.anchorMax = Vector2.one;
        loadRt.sizeDelta = Vector2.zero;

        CreatePanelTitle("PREPARING FOR BATTLE...", loadingPanel.transform);

        // Progress Bar Background
        GameObject pBg = new GameObject("ProgressBg", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        RectTransform pBgRt = pBg.GetComponent<RectTransform>();
        pBgRt.SetParent(loadingPanel.transform, false);
        pBgRt.anchorMin = new Vector2(0.15f, 0.42f);
        pBgRt.anchorMax = new Vector2(0.85f, 0.47f);
        pBgRt.sizeDelta = Vector2.zero;

        pBg.GetComponent<UnityEngine.UI.Image>().color = new Color(0.05f, 0.05f, 0.08f, 1f);

        // Progress Bar Fill
        GameObject pFill = new GameObject("ProgressFill", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        loadingProgressFill = pFill.GetComponent<RectTransform>();
        loadingProgressFill.SetParent(pBg.transform, false);
        loadingProgressFill.anchorMin = Vector2.zero;
        loadingProgressFill.anchorMax = new Vector2(0f, 1f); // Starts at 0%
        loadingProgressFill.sizeDelta = Vector2.zero;

        pFill.GetComponent<UnityEngine.UI.Image>().color = new Color(0.5f, 0.2f, 0.9f, 1f); // bright glowing purple bar

        // Loading Tip text
        GameObject tipObj = new GameObject("TipText", typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform tipRt = tipObj.GetComponent<RectTransform>();
        tipRt.SetParent(loadingPanel.transform, false);
        tipRt.anchorMin = new Vector2(0.1f, 0.22f);
        tipRt.anchorMax = new Vector2(0.9f, 0.35f);
        tipRt.sizeDelta = Vector2.zero;

        loadingTipText = tipObj.GetComponent<TextMeshProUGUI>();
        loadingTipText.text = "TIP: Knights are highly resilient but move slower than other classes.";
        loadingTipText.fontSize = 15;
        loadingTipText.fontStyle = FontStyles.Italic;
        loadingTipText.alignment = TextAlignmentOptions.Center;
        loadingTipText.color = new Color(0.7f, 0.7f, 0.75f, 1f);

        // ==========================================
        // IN-GAME HUD ABILITY SLOT
        // ==========================================
        abilityHudContainer = new GameObject("HUD_AbilitySlot", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        RectTransform hudRt = abilityHudContainer.GetComponent<RectTransform>();
        hudRt.SetParent(canvas.transform, false);
        hudRt.anchorMin = new Vector2(0.7f, 0.03f);
        hudRt.anchorMax = new Vector2(0.97f, 0.09f);
        hudRt.sizeDelta = Vector2.zero;

        abilityHudContainer.GetComponent<UnityEngine.UI.Image>().color = new Color(0.06f, 0.06f, 0.08f, 0.85f);

        // Cooldown Fill bar overlay
        GameObject hudFill = new GameObject("CooldownFill", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        hudAbilityFill = hudFill.GetComponent<RectTransform>();
        hudAbilityFill.SetParent(abilityHudContainer.transform, false);
        hudAbilityFill.anchorMin = Vector2.zero;
        hudAbilityFill.anchorMax = Vector2.one; // Full
        hudAbilityFill.sizeDelta = Vector2.zero;

        hudFill.GetComponent<UnityEngine.UI.Image>().color = new Color(0.5f, 0.3f, 0.8f, 0.9f);

        // HUD text
        GameObject hudTextObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform htRt = hudTextObj.GetComponent<RectTransform>();
        htRt.SetParent(abilityHudContainer.transform, false);
        htRt.anchorMin = Vector2.zero;
        htRt.anchorMax = Vector2.one;
        htRt.sizeDelta = Vector2.zero;

        hudAbilityText = hudTextObj.GetComponent<TextMeshProUGUI>();
        hudAbilityText.fontSize = 14;
        hudAbilityText.fontStyle = FontStyles.Bold;
        hudAbilityText.alignment = TextAlignmentOptions.Center;
        hudAbilityText.color = Color.white;

        abilityHudContainer.SetActive(false); // Hide until game actually starts
    }

    private void CreatePanelTitle(string textStr, Transform parent)
    {
        GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform rt = titleObj.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = new Vector2(0.1f, 0.86f);
        rt.anchorMax = new Vector2(0.9f, 0.96f);
        rt.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = titleObj.GetComponent<TextMeshProUGUI>();
        tmp.text = textStr;
        tmp.fontSize = 32;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.92f, 0.86f, 1f, 1f);
    }

    private void CreateBackButton(Transform parent, System.Action onBackClick)
    {
        GameObject backBtn = new GameObject("BackButton", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        RectTransform rt = backBtn.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = new Vector2(0.04f, 0.87f);
        rt.anchorMax = new Vector2(0.12f, 0.95f);
        rt.sizeDelta = Vector2.zero;

        backBtn.GetComponent<UnityEngine.UI.Image>().color = new Color(0.12f, 0.12f, 0.16f, 0.85f);

        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform tRt = textObj.GetComponent<RectTransform>();
        tRt.SetParent(backBtn.transform, false);
        tRt.anchorMin = Vector2.zero;
        tRt.anchorMax = Vector2.one;
        tRt.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        tmp.text = "< BACK";
        tmp.fontSize = 13;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.7f, 0.7f, 0.75f, 1f);

        var interact = backBtn.AddComponent<UIInteractiveElement>();
        interact.normalColor = new Color(0.12f, 0.12f, 0.16f, 0.85f);
        interact.hoverColor = new Color(0.2f, 0.2f, 0.28f, 0.95f);
        interact.onClick = onBackClick;
    }

    private RectTransform CreateStatBar(string statName, Transform parent, float yMin, Color fillColor)
    {
        // Label
        GameObject labelObj = new GameObject(statName + "_Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform lRt = labelObj.GetComponent<RectTransform>();
        lRt.SetParent(parent, false);
        lRt.anchorMin = new Vector2(0.08f, yMin + 0.05f);
        lRt.anchorMax = new Vector2(0.92f, yMin + 0.11f);
        lRt.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = labelObj.GetComponent<TextMeshProUGUI>();
        tmp.text = statName;
        tmp.fontSize = 11;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = new Color(0.7f, 0.7f, 0.75f, 1f);

        // Bar Background
        GameObject barBg = new GameObject(statName + "_BarBg", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        RectTransform bgRt = barBg.GetComponent<RectTransform>();
        bgRt.SetParent(parent, false);
        bgRt.anchorMin = new Vector2(0.08f, yMin);
        bgRt.anchorMax = new Vector2(0.92f, yMin + 0.04f);
        bgRt.sizeDelta = Vector2.zero;

        barBg.GetComponent<UnityEngine.UI.Image>().color = new Color(0.04f, 0.04f, 0.06f, 1f);

        // Bar Fill
        GameObject barFill = new GameObject(statName + "_BarFill", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        RectTransform fillRt = barFill.GetComponent<RectTransform>();
        fillRt.SetParent(barBg.transform, false);
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = new Vector2(0.5f, 1f);
        fillRt.sizeDelta = Vector2.zero;

        barFill.GetComponent<UnityEngine.UI.Image>().color = fillColor;

        return fillRt;
    }

    private void UpdateCharacterSelectionUI(GameManager.CharacterType selectedType)
    {
        int selectedIndex = (int)selectedType;
        for (int i = 0; i < 4; i++)
        {
            var btn = charButtons[i].GetComponent<UIInteractiveElement>();
            if (btn != null)
                btn.SetSelected(i == selectedIndex);
        }

        GameManager.CharacterData data = GameManager.instance.GetCharacterData(selectedType);
        previewNameText.text = data.name.ToUpper();
        previewNameText.color = data.color;
        previewDescText.text = data.description;

        // Ability Text integration
        previewAbilityText.text = "<color=#bf8cff>UNIQUE ABILITY:</color>\n<b>" + data.abilityName.ToUpper() + "</b>\n" + data.abilityDesc;

        hpBarFill.anchorMax = new Vector2(data.maxHealth / 10f, 1f);
        speedBarFill.anchorMax = new Vector2(data.speed / 15f, 1f);
        jumpBarFill.anchorMax = new Vector2(data.jumpForce / 20f, 1f);
    }

    private void UpdateDifficultySelectionUI(GameManager.Difficulty selectedDiff)
    {
        int selectedIdx = (int)selectedDiff;
        for (int i = 0; i < 3; i++)
        {
            var btn = diffButtons[i].GetComponent<UIInteractiveElement>();
            if (btn != null)
                btn.SetSelected(i == selectedIdx);
        }
    }

    private void UpdateMapSelectionUI(GameManager.MapType selectedMap)
    {
        int selectedIdx = (int)selectedMap;
        for (int i = 0; i < 4; i++)
        {
            var card = mapCards[i].GetComponent<UIInteractiveElement>();
            if (card != null)
                card.SetSelected(i == selectedIdx);
        }
    }

    // Interactive Loading Coroutine (fills over 2 seconds)
    private System.Collections.IEnumerator LoadingProgressRoutine()
    {
        string[] tips = {
            "TIP: Knight's Shield Wall blocks ALL incoming damage for 3 seconds!",
            "TIP: Rogue's Shadow Dash lets you phase through enemy bullets safely!",
            "TIP: Wizard's Mana Flare triggers a massive circular magic explosion!",
            "TIP: Warrior's Berserker Rage doubles slash range and grants double damage!"
        };

        float progress = 0f;
        float soundTimer = 0f;

        while (progress < 1.0f)
        {
            progress += Time.unscaledDeltaTime / 2.0f; // fill over 2 seconds
            if (progress > 1.0f) progress = 1.0f;

            loadingProgressFill.anchorMax = new Vector2(progress, 1f);

            // Shifting tip message based on progress bracket
            int tipIdx = Mathf.Clamp((int)(progress * tips.Length), 0, tips.Length - 1);
            loadingTipText.text = tips[tipIdx];

            // Trigger loading retro synth tick sound every 0.12 seconds
            soundTimer += Time.unscaledDeltaTime;
            if (soundTimer >= 0.12f && progress < 1.0f)
            {
                soundTimer = 0f;
                if (AudioManager.instance != null)
                    AudioManager.instance.PlayLoadingSound();
            }

            yield return null;
        }

        // Failsafe tick on end
        if (AudioManager.instance != null)
            AudioManager.instance.PlayClickSound();

        // Start the game!
        startMenuPanel.SetActive(false);
        gameStartTime = Time.time;
        
        if (abilityHudContainer != null)
            abilityHudContainer.SetActive(true); // Enable gameplay ability HUD slot!

        GameManager.instance.StartGame();
    }

    private void UpdateAbilityHUD()
    {
        if (GameManager.instance == null || !GameManager.instance.isGameActive || abilityHudContainer == null || !abilityHudContainer.activeSelf)
            return;

        Player player = FindFirstObjectByType<Player>();
        if (player != null)
        {
            float cooldownRatio = player.GetAbilityCooldownNormalized();
            
            // Shrinks to represent remaining cooldown
            hudAbilityFill.anchorMax = new Vector2(1f - cooldownRatio, 1f);

            GameManager.CharacterData data = GameManager.instance.GetCharacterData(GameManager.instance.currentCharacter);

            if (player.abilityCooldown > 0f)
            {
                hudAbilityText.text = data.abilityName.ToUpper() + " (COOLDOWN: " + player.abilityCooldown.ToString("F1") + "s)";
                hudAbilityFill.GetComponent<UnityEngine.UI.Image>().color = new Color(0.24f, 0.24f, 0.28f, 0.85f); // greyed out
            }
            else
            {
                hudAbilityText.text = "PRESS [E] TO UNLEASH: " + data.abilityName.ToUpper();
                hudAbilityFill.GetComponent<UnityEngine.UI.Image>().color = new Color(0.48f, 0.18f, 0.85f, 0.95f); // neon purple glowing
            }
        }
    }
}