using UnityEngine;
using System;
using DungeonDelver.Battle;

namespace DungeonDelver.Battle
{
    public static class BattleRules
    {
        // Tunables (copied from rules.ts)
        const int BASE_HIT   = 88;
        const int DEX_DIFF   = 2;
        const float LUCK_HIT = 0.5f;
        const float LEVEL_HIT = 1.5f;
        const int MIN_HIT    = 60;
        const int MAX_HIT    = 98;

        const int BASE_CRIT  = 5;
        const int MAX_CRIT   = 50;
        const float LUCK_CRIT = 1f;
        const float CRIT_MULT = 1.5f;

        const int GRAZE_WINDOW = 8;
        const float GRAZE_MULT = 0.5f;

        static System.Random rng = new System.Random();

        static int ClampInt(int n, int min, int max) =>
            Math.Min(max, Math.Max(min, n));

        static bool RollPct(int pct)
        {
            int r = rng.Next(1, 101); // 1..100
            return r <= pct;
        }

        static int RandomInt(int minInclusive, int maxInclusive)
        {
            return rng.Next(minInclusive, maxInclusive + 1);
        }

        // TODO: when you port armor.ts, swap this for true armor calc.
        static int GetTotalArmor(Actor defender)
        {
            return defender.baseStats.armor;
        }

        static float ComputeLinearMitigation(int armor)
        {
            // Placeholder: 0..0.60 like your TS armor helper
            float mit = armor / 100f;
            return Mathf.Clamp(mit, 0f, 0.60f);
        }

        static int ComputeRawPhysicalDamage(Actor attacker)
        {
            int weaponBase = attacker.equipment.weapon != null
                ? attacker.equipment.weapon.mods.damage
                : 2;

            float strBonus = attacker.baseStats.str * 0.5f;
            return Mathf.FloorToInt(weaponBase + strBonus);
        }

        static int ApplyMitigationToRaw(int raw, Actor defender)
        {
            int totalArmor = GetTotalArmor(defender);
            float mit = ComputeLinearMitigation(totalArmor);
            float after = raw * (1f - mit);
            return Math.Max(1, Mathf.FloorToInt(after));
        }

        public static int DealPhysicalDamage(Actor attacker, Actor defender)
        {
            int raw = ComputeRawPhysicalDamage(attacker);
            int totalArmor = GetTotalArmor(defender);
            float mit = ComputeLinearMitigation(totalArmor);
            float mitigated = raw * (1f - mit);

            Debug.Log($"[DMG/Armor] atk={attacker.actorName} def={defender.actorName} raw={raw} armor={totalArmor} mit%={Mathf.RoundToInt(mit * 100)} after={mitigated}");

            return Math.Max(1, Mathf.FloorToInt(mitigated));
        }

        public static int ComputeHitChance(Actor attacker, Actor defender)
        {
            int dexDelta   = attacker.baseStats.dex - defender.baseStats.dex;
            int luckDelta  = attacker.baseStats.luck - defender.baseStats.luck;
            int levelDelta = attacker.level - defender.level;

            int weaponAcc = attacker.equipment.weapon != null
                ? attacker.equipment.weapon.mods.accuracy
                : 0;

            float chance = BASE_HIT
                           + dexDelta * DEX_DIFF
                           + luckDelta * LUCK_HIT
                           + levelDelta * LEVEL_HIT
                           + weaponAcc;

            return ClampInt(Mathf.FloorToInt(chance), MIN_HIT, MAX_HIT);
        }

        public static int ComputeCritChance(Actor attacker)
        {
            int weaponCrit = attacker.equipment.weapon != null
                ? attacker.equipment.weapon.mods.critPct
                : 0;

            float baseCrit = BASE_CRIT
                             + attacker.baseStats.luck * LUCK_CRIT
                             + weaponCrit;

            return ClampInt(Mathf.FloorToInt(baseCrit), 0, MAX_CRIT);
        }

        // Very simple weapon naming for now
        static string GetWeaponName(Actor attacker)
        {
            if (attacker.equipment.weapon != null &&
                !string.IsNullOrEmpty(attacker.equipment.weapon.name))
                return attacker.equipment.weapon.name;

            return "Fists";
        }

        static string VerbForAttack(Actor attacker, string weaponName)
        {
            string lower = weaponName.ToLowerInvariant();
            if (weaponName == "Fists") return "punches";
            if (lower.Contains("dagger") || lower.Contains("rapier")) return "stabs";
            if (lower.Contains("spear") || lower.Contains("lance") || lower.Contains("arrow")) return "pierces";
            if (lower.Contains("mace") || lower.Contains("hammer") || lower.Contains("staff") || lower.Contains("club")) return "smashes";
            return "slashes";
        }

        public static AttackResult PerformAttack(Actor attacker, Actor defender)
        {
            int toHit = ComputeHitChance(attacker, defender);
            int roll = RandomInt(1, 100);

            string weaponName = GetWeaponName(attacker);

            // Full miss
            if (roll > toHit + GRAZE_WINDOW)
            {
                string logMiss = $"{attacker.actorName} misses {defender.actorName} with {weaponName}.";
                return new AttackResult
                {
                    hit = false,
                    crit = false,
                    dmg = 0,
                    verb = "misses",
                    weaponName = weaponName,
                    logLine = logMiss
                };
            }

            int raw = ComputeRawPhysicalDamage(attacker);
            int baseAfter = ApplyMitigationToRaw(raw, defender);
            int nonCrit = Math.Max(1, baseAfter + RandomInt(-2, 2));

            // Graze
            if (roll > toHit)
            {
                int dmgGraze = Math.Max(1, Mathf.FloorToInt(nonCrit * GRAZE_MULT));
                string logGraze = $"{attacker.actorName} grazes {defender.actorName} with {weaponName} for {dmgGraze} damage.";
                return new AttackResult
                {
                    hit = true,
                    crit = false,
                    dmg = dmgGraze,
                    verb = "grazes",
                    weaponName = weaponName,
                    logLine = logGraze
                };
            }

            // Hit (maybe crit)
            bool isCrit = RollPct(ComputeCritChance(attacker));
            int dmg = nonCrit;

            if (isCrit)
            {
                float critRaw = raw * CRIT_MULT;
                int critBase = ApplyMitigationToRaw(Mathf.FloorToInt(critRaw), defender);
                int critFinal = Math.Max(2, critBase + RandomInt(0, 2));
                dmg = Math.Max(critFinal, nonCrit + 1);
            }

            string verb = VerbForAttack(attacker, weaponName);

            string logLine = isCrit
                ? $"Critical Hit! {attacker.actorName} lands a powerful blow on {defender.actorName} with {weaponName}, dealing {dmg} damage!"
                : $"{attacker.actorName} {verb} {defender.actorName} with {weaponName} for {dmg} damage.";

            return new AttackResult
            {
                hit = true,
                crit = isCrit,
                dmg = dmg,
                verb = verb,
                weaponName = weaponName,
                logLine = logLine
            };
        }

        // Shared resource helpers
        public static int ClampResource(int value, int max)
        {
            return Mathf.Clamp(value, 0, max);
        }

        public static void ApplyDamage(Actor target, int dmg)
        {
            target.hp.current = ClampResource(target.hp.current - dmg, target.hp.max);
        }

        public static void ApplyHeal(Actor target, int amount)
        {
            target.hp.current = ClampResource(target.hp.current + amount, target.hp.max);
        }
    }
}
