using System.Collections;
using UnityEngine;
using UnityEngine.UI;   // For HP bars (optional)

namespace DungeonDelver.Battle
{
    /// <summary>
    /// Super-lean “demo battle” controller: one player vs one enemy.
    /// Turn-based:
    ///   - Player turn: press 1 to Attack, 2 to Defend, 3 to Pass
    ///   - Enemy takes a simple auto-attack turn
    /// Logs go to the Console (and optionally a UI Text).
    /// </summary>
    public class BattleController : MonoBehaviour
    {
        // ------------------------
        // Simple runtime actor data
        // (We can later replace this with your real BattleTypes/Actors)
        // ------------------------
        [System.Serializable]
        public class RuntimeActor
        {
            public string id;
            public string displayName;

            [Header("Stats")]
            public int maxHP = 20;
            public int attack = 5;
            public int armor = 0;
            public int speed = 10;   // not used yet, but we’ll want it later
            public bool isPlayer;

            [HideInInspector] public int currentHP;
            [HideInInspector] public bool defending;
        }

        public enum Phase
        {
            Starting,
            PlayerTurn,
            EnemyTurn,
            Busy,       // during animations / delays
            Victory,
            Defeat
        }

        [Header("Actors")]
        public RuntimeActor player;
        public RuntimeActor enemy;

        [Header("Optional UI")]
        public Text logText;          // Any UI Text in your battle scene
        public Slider playerHpBar;    // Optional HP bars
        public Slider enemyHpBar;

        [Header("Timing")]
        public float enemyTurnDelay = 0.7f;

        private Phase _phase = Phase.Starting;

        // =====================================================
        // Unity lifecycle
        // =====================================================

        void Start()
        {
            // Initialize HP
            if (player != null) player.currentHP = player.maxHP;
            if (enemy  != null) enemy.currentHP  = enemy .maxHP;

            UpdateHpUI();
            Log("A wild slime appears!");
            _phase = Phase.PlayerTurn;
            Log("Player turn – press 1: Attack, 2: Defend, 3: Pass");
        }

        void Update()
        {
            switch (_phase)
            {
                case Phase.PlayerTurn:
                    HandlePlayerInput();
                    break;

                case Phase.EnemyTurn:
                    // We run the enemy logic in a coroutine so we don’t do anything here.
                    break;

                case Phase.Victory:
                case Phase.Defeat:
                    // Later: hook “continue” or “return to dungeon” here
                    break;
            }
        }

        // =====================================================
        // Player turn
        // =====================================================

        void HandlePlayerInput()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                // Basic attack
                StartCoroutine(PlayerAttackRoutine());
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                StartCoroutine(PlayerDefendRoutine());
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                StartCoroutine(PlayerPassRoutine());
            }
        }

        IEnumerator PlayerAttackRoutine()
        {
            _phase = Phase.Busy;

            Log($"{player.displayName} attacks!");

            int dmg = ComputeDamage(player, enemy);
            ApplyDamage(enemy, dmg);

            Log($"{player.displayName} hits {enemy.displayName} for {dmg} damage. " +
                $"({enemy.currentHP}/{enemy.maxHP} HP)");

            UpdateHpUI();

            // Victory check
            if (enemy.currentHP <= 0)
            {
                yield return EndBattle(victory: true);
                yield break;
            }

            yield return new WaitForSeconds(0.3f);

            // Enemy turn
            _phase = Phase.EnemyTurn;
            StartCoroutine(EnemyTurnRoutine());
        }

        IEnumerator PlayerDefendRoutine()
        {
            _phase = Phase.Busy;
            player.defending = true;
            Log($"{player.displayName} raises their shield and takes a defensive stance.");
            yield return new WaitForSeconds(0.3f);

            _phase = Phase.EnemyTurn;
            StartCoroutine(EnemyTurnRoutine());
        }

        IEnumerator PlayerPassRoutine()
        {
            _phase = Phase.Busy;
            Log($"{player.displayName} waits, watching the enemy closely…");
            yield return new WaitForSeconds(0.3f);

            _phase = Phase.EnemyTurn;
            StartCoroutine(EnemyTurnRoutine());
        }

        // =====================================================
        // Enemy turn
        // =====================================================

        IEnumerator EnemyTurnRoutine()
        {
            Log($"{enemy.displayName}'s turn…");
            yield return new WaitForSeconds(enemyTurnDelay);

            // Simple AI: always attack
            Log($"{enemy.displayName} lashes out!");

            int dmg = ComputeDamage(enemy, player);

            // If the player was defending, reduce damage and clear the flag
            if (player.defending)
            {
                int reduced = Mathf.CeilToInt(dmg * 0.5f);
                Log($"Defense halves the blow from {dmg} → {reduced}.");
                dmg = reduced;
                player.defending = false;
            }

            ApplyDamage(player, dmg);
            Log($"{enemy.displayName} hits {player.displayName} for {dmg} damage. " +
                $"({player.currentHP}/{player.maxHP} HP)");

            UpdateHpUI();

            if (player.currentHP <= 0)
            {
                yield return EndBattle(victory: false);
                yield break;
            }

            yield return new WaitForSeconds(0.3f);

            _phase = Phase.PlayerTurn;
            Log("Player turn – press 1: Attack, 2: Defend, 3: Pass");
        }

        // =====================================================
        // Core math – placeholder, later we can plug in BattleRules
        // =====================================================

        int ComputeDamage(RuntimeActor attacker, RuntimeActor defender)
        {
            // Super simple for now: attack + small random, minus armor, min 1
            int raw = attacker.attack + Random.Range(-1, 2); // -1, 0, or +1
            int mitigated = raw - defender.armor;
            return Mathf.Max(1, mitigated);
        }

        void ApplyDamage(RuntimeActor target, int dmg)
        {
            target.currentHP = Mathf.Max(0, target.currentHP - dmg);
        }

        // =====================================================
        // End battle & helpers
        // =====================================================

        IEnumerator EndBattle(bool victory)
        {
            _phase = victory ? Phase.Victory : Phase.Defeat;

            if (victory)
            {
                Log("Victory! The slime is defeated.");
            }
            else
            {
                Log("Defeat… the Knight falls.");
            }

            // Later: XP gain, loot, return to dungeon, etc.
            yield return null;
        }

        void Log(string msg)
        {
            Debug.Log(msg);

            if (logText != null)
            {
                logText.text += msg + "\n";
            }
        }

        void UpdateHpUI()
        {
            if (playerHpBar != null)
            {
                playerHpBar.maxValue = player.maxHP;
                playerHpBar.value    = player.currentHP;
            }

            if (enemyHpBar != null)
            {
                enemyHpBar.maxValue = enemy.maxHP;
                enemyHpBar.value    = enemy.currentHP;
            }
        }
    }
}
