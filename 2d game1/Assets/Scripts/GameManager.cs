using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public enum Difficulty { Normal, Medium, High }
    public enum CharacterType { Knight, Rogue, Wizard, Warrior }
    public enum MapType { Forest, Desert, Underworld, Tundra }

    [Header("Current Selection")]
    public Difficulty currentDifficulty = Difficulty.Normal;
    public CharacterType currentCharacter = CharacterType.Knight;
    public MapType currentMap = MapType.Forest;

    [Header("Game State")]
    public bool isGameActive = false;

    // Character Struct Definition
    public struct CharacterData
    {
        public string name;
        public int maxHealth;
        public float speed;
        public float jumpForce;
        public Color color;
        public string description;
        public string abilityName;
        public string abilityDesc;

        public CharacterData(string name, int maxHealth, float speed, float jumpForce, Color color, string description, string abilityName, string abilityDesc)
        {
            this.name = name;
            this.maxHealth = maxHealth;
            this.speed = speed;
            this.jumpForce = jumpForce;
            this.color = color;
            this.description = description;
            this.abilityName = abilityName;
            this.abilityDesc = abilityDesc;
        }
    }

    public CharacterData GetCharacterData(CharacterType type)
    {
        switch (type)
        {
            case CharacterType.Knight:
                return new CharacterData("Knight", 10, 8.5f, 12.5f, new Color(1f, 0.84f, 0.3f, 1f), "High health & heavy armor. Slow but resilient.", "Shield Wall", "Active: Take 0 damage for 3 seconds. [8s Cooldown]");
            case CharacterType.Rogue:
                return new CharacterData("Rogue", 5, 13.5f, 17f, new Color(0.3f, 0.9f, 0.4f, 1f), "Extremely fast and agile. Can jump high, but low health.", "Shadow Dash", "Active: Rapid forward dash, moving through hazards safely. [4s Cooldown]");
            case CharacterType.Wizard:
                return new CharacterData("Wizard", 6, 9.5f, 13f, new Color(0.6f, 0.4f, 0.95f, 1f), "A master of elements. Balanced stats with mystical purple aura.", "Mana Flare", "Active: Release a magical shockwave damaging nearby foes. [6s Cooldown]");
            case CharacterType.Warrior:
                return new CharacterData("Warrior", 8, 10.5f, 14.5f, new Color(0.9f, 0.3f, 0.3f, 1f), "Mighty berserker. Powerful, balanced, and battle-hardened.", "Berserker Rage", "Active: Boost speed + attack range, deals double damage. [10s Cooldown]");
            default:
                return new CharacterData("Knight", 10, 8f, 12f, Color.white, "", "", "");
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Spawn an AudioManager if none exists in the scene!
            if (AudioManager.instance == null)
            {
                GameObject audioManagerObj = new GameObject("AudioManager");
                audioManagerObj.AddComponent<AudioManager>();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Pause the game initially until user presses Start
        isGameActive = false;
        Time.timeScale = 0f;
    }

    public void StartGame()
    {
        isGameActive = true;
        Time.timeScale = 1f;

        // Apply visual character choices to the player
        CustomizePlayer();

        // Apply environmental visual layout changes to the map
        ApplyMapTheme(currentMap);

        // Play click sound
        if (AudioManager.instance != null)
            AudioManager.instance.PlayClickSound();

        Debug.Log("Game started with Difficulty: " + currentDifficulty + ", Character: " + currentCharacter + ", Map: " + currentMap);
    }

    public void CustomizePlayer()
    {
        Player playerObj = FindFirstObjectByType<Player>();
        if (playerObj != null)
        {
            CharacterData data = GetCharacterData(currentCharacter);

            // Refactor stats
            playerObj.SetEntityStats(data.maxHealth, data.speed, data.jumpForce, data.color);

            // 1. Swap Character Visual scale/model
            if (currentCharacter == CharacterType.Knight)
            {
                playerObj.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f); // bulky heavy knight
            }
            else if (currentCharacter == CharacterType.Rogue)
            {
                playerObj.transform.localScale = new Vector3(0.85f, 0.85f, 0.85f); // tiny agile rogue
            }
            else if (currentCharacter == CharacterType.Wizard)
            {
                playerObj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f); // medium wizard
            }
            else if (currentCharacter == CharacterType.Warrior)
            {
                playerObj.transform.localScale = new Vector3(1.15f, 1.15f, 1.15f); // balanced warrior
            }

            // 2. Assign Cooldown times
            if (currentCharacter == CharacterType.Knight) playerObj.abilityMaxCooldown = 8f;
            else if (currentCharacter == CharacterType.Rogue) playerObj.abilityMaxCooldown = 4f;
            else if (currentCharacter == CharacterType.Wizard) playerObj.abilityMaxCooldown = 6f;
            else if (currentCharacter == CharacterType.Warrior) playerObj.abilityMaxCooldown = 10f;
        }
    }

    public void ApplyMapTheme(MapType map)
    {
        // Colors definition for maps
        Color groundColor = Color.white;
        Color bgLayerColor = Color.white;

        switch (map)
        {
            case MapType.Forest:
                groundColor = new Color(0.7f, 0.9f, 0.7f, 1f); // forest green/brown tint
                bgLayerColor = new Color(0.6f, 0.8f, 0.75f, 1f); // natural green teal
                break;
            case MapType.Desert:
                groundColor = new Color(0.95f, 0.75f, 0.4f, 1f); // hot desert gold
                bgLayerColor = new Color(1f, 0.82f, 0.55f, 1f); // warm desert sun tint
                break;
            case MapType.Underworld:
                groundColor = new Color(0.8f, 0.25f, 0.2f, 1f); // deep lava red
                bgLayerColor = new Color(0.45f, 0.15f, 0.22f, 1f); // dark crimson obsidian
                break;
            case MapType.Tundra:
                groundColor = new Color(0.65f, 0.85f, 0.95f, 1f); // frosty snow blue
                bgLayerColor = new Color(0.75f, 0.9f, 1f, 1f); // icy white-blue mist
                break;
        }

        // Apply colors to SpriteRenderers in the scene
        SpriteRenderer[] srs = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        foreach (SpriteRenderer sr in srs)
        {
            if (sr.gameObject.CompareTag("Player") || sr.gameObject.GetComponentInParent<Player>() != null)
            {
                // Leave the player to customized colors, don't overwrite with map tint
                continue;
            }

            if (sr.gameObject.GetComponentInParent<Enemy>() != null)
            {
                // Leave enemy sprites standard or give them a dark purple touch for underworld!
                if (map == MapType.Underworld)
                    sr.color = new Color(0.9f, 0.5f, 0.5f, 1f);
                else if (map == MapType.Tundra)
                    sr.color = new Color(0.7f, 0.85f, 1f, 1f);
                else
                    sr.color = Color.white;
                continue;
            }

            if (sr.gameObject.GetComponentInParent<ObjectToProtect>() != null)
            {
                // Leave girl clean
                continue;
            }

            string name = sr.gameObject.name.ToLower();
            if (name.Contains("ground"))
            {
                sr.color = groundColor;
            }
            else if (name.Contains("layer") || name.Contains("background") || name.Contains("bg"))
            {
                sr.color = bgLayerColor;
            }
        }
    }
}
