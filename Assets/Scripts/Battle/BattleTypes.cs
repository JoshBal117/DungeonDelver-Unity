using System.Collections.Generic;
using UnityEngine;

namespace DungeonDelver.Battle
{
    // Rough C# equivalent of Resource in types.ts
    [System.Serializable]
    public struct Resource
    {
        public int current;
        public int max;

        public Resource(int current, int max)
        {
            this.current = current;
            this.max = max;
        }
    }

    // Status codes from your TS union
    public enum StatusCode
    {
        Stun,
        Parry,
        Defend,
        Paralyzed,
        ArmorDown
    }

    [System.Serializable]
    public class StatusEffect
    {
        public StatusCode code;
        public int turns;
        public float potency;   // use 0 if unused

        public StatusEffect(StatusCode code, int turns, float potency = 0f)
        {
            this.code = code;
            this.turns = turns;
            this.potency = potency;
        }
    }

    // BaseAttributes from types.ts (renamed int -> intellect)
    [System.Serializable]
    public class BaseAttributes
    {
        public int str;
        public int dex;
        public int intellect;
        public int wis;
        public int vit;
        public int con;
        public int speed;
        public int armor;
        public int resist;
        public int luck;
    }

    // Tags (only the ones you actually use in rules/combat for now)
    [System.Serializable]
    public class Tags
    {
        public bool spellcaster;
        public bool undead;
        public bool beast;
        public bool humanoid;
        public bool flying;
        public bool demon;
        public bool goblinoid;
        public bool slime;

        // boss/miniboss can be “levels” so we use int (0 = false)
        public int boss;
        public int miniboss;
    }

    // Minimal weapon/equipment just to support damage rules
    [System.Serializable]
    public class WeaponMods
    {
        public int damage;
        public int accuracy;
        public int critPct;
    }

    [System.Serializable]
    public class Weapon
    {
        public string name;
        public WeaponMods mods = new WeaponMods();
    }

    [System.Serializable]
    public class Equipment
    {
        public Weapon weapon;
        // we can add shield/armor later when we port your armor.ts
    }

    [System.Serializable]
    public class Actor
    {
        public string id;
        public string actorName;
        public bool isPlayer;

        public int level;
        public int xp;
        public int xpToNext;

        public Tags tags = new Tags();
        public BaseAttributes baseStats = new BaseAttributes();
        public Equipment equipment = new Equipment();

        public Resource hp;
        public Resource mp;
        public Resource sp;   // you can leave max = 0 for enemies without stamina

        public List<StatusEffect> effects = new List<StatusEffect>();

        public Actor(string id, string name, bool isPlayer)
        {
            this.id = id;
            this.actorName = name;
            this.isPlayer = isPlayer;
        }
    }

    [System.Serializable]
    public class LogEvent
    {
        public string text;
        public LogEvent(string text) { this.text = text; }
    }

    [System.Serializable]
    public class CombatState
    {
        public int turn;
        public List<string> order = new List<string>();
        public Dictionary<string, Actor> actors = new Dictionary<string, Actor>();
        public List<LogEvent> log = new List<LogEvent>();
        public bool over;
        public Dictionary<string, List<StatusEffect>> statuses = new Dictionary<string, List<StatusEffect>>();
    }

    // C# version of AttackResult from rules.ts
    public class AttackResult
    {
        public bool hit;
        public bool crit;
        public int dmg;
        public string verb;
        public string weaponName;
        public string logLine;
    }
}
