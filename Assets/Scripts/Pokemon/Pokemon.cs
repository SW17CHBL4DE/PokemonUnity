using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class Pokemon
{
   [SerializeField] PokemonBase _base;
   [SerializeField] int level;

   public PokemonBase Base
   {
      get
      { return _base; }
   }

   public int Level
   {
      get { return level; }
   }

      public int        currentHP   { get; set; }
      public List<Move> Moves       { get; set; }
      
      //declaring dictionary(similar to UE Map variable) of the type enum stats
      //publically accessible, set only on this
      public Dictionary<Stat, int> Stats { get; private set; }
      
      public Dictionary<Stat, int> StatsBoosted { get; private set; }
      
      //similar to list, however we can remove elements at runtime and it follows FIFO priority
      public Queue<string> StatusChanges { get; private set; } = new Queue<string>();
      
      //variables set as properties
      public int MaxHP
      {
         get;
         private set;
      }

      public int Attack
      {
         get { return GetStat(Stat.Attack); }
      }
   
      public int Defence
      {
         get { return GetStat(Stat.Defence); }
      }
   
      public int SpAttack
      {
         get { return GetStat(Stat.SpAttack); }
      }
   
      public int SpDefence
      {
         get { return GetStat(Stat.SpDefence); }
      }
   
      public int Speed
      {
         get { return GetStat(Stat.Speed); }
      }

      public void Init()
      {
         //Generate moves
         Moves = new List<Move>();
         foreach (var move in Base.LearnableMoves)
         {
            if (move.Level <= Level)
               Moves.Add(new Move(move.MoveBase));
         
            if (Moves.Count >= 4)
               break;
         }
         
         CalcStats();
         currentHP   = MaxHP;
         ResetStatBoost();
      }

      void CalcStats()
      {
         /*add a key and value to dictionary variable. Each key is an enum value. The int value is being set from
          calculations */
         
         Stats = new Dictionary<Stat, int>();
         Stats.Add(Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
         Stats.Add(Stat.Defence, Mathf.FloorToInt((Base.Defence * Level) / 100f) + 5);
         Stats.Add(Stat.SpAttack, Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5);
         Stats.Add(Stat.SpDefence, Mathf.FloorToInt((Base.SpDefence * Level) / 100f) + 5);
         Stats.Add(Stat.Speed, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5);

         //also setting the max health value here, as we are running the calc stat function in init. MaxHealth should only ever be set once generally.
         MaxHP = Mathf.FloorToInt((Base.MaxHP * Level) / 100f) + 10;
      }

      //this is where we calculate any changes to the stat values, and then return the calculated output. Get stat should be called with each attack/move
      int GetStat(Stat stat)
      {
         int statVal = Stats[stat];
         //TODO: stat boost
         int boost = StatsBoosted[stat];
         
         //if value of boost = 1, stat value is increased by 1 'level', multiplying it by index at 1 (default is index 0)
         //if value of boost = 2, stat value is multiplied by value at index 2
         var boostValues = new float[] { 1.0f, 1.5f, 2.0f, 3.0f, 3.5f, 4.0f };

         if (boost >= 0)
         {
            statVal = Mathf.FloorToInt(statVal * boostValues[boost]);
         }
         else
         {
            statVal = Mathf.FloorToInt(statVal / boostValues[-boost]);
         }

         return statVal;
      }

      void ResetStatBoost()
      {
         StatsBoosted = new Dictionary<Stat, int>()
         {
            { Stat.Attack, 0 },
            { Stat.Defence, 0 },
            { Stat.SpAttack, 0 },
            { Stat.SpDefence, 0 },
            { Stat.Speed, 0 }
         };
      }

      public void ApplyBoosts(List<StatBoost> statBoosts)
      {
         foreach (var statBoost in statBoosts)
         {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            StatsBoosted[stat] = Mathf.Clamp(StatsBoosted[stat] + boost, -6, 6);
            if (boost > 0)
            {
               StatusChanges.Enqueue($"{Base.Name}'s {stat} rose!");
            }
            else
            {
               StatusChanges.Enqueue($"{Base.Name}'s {stat} fell!");
            }
            
            Debug.Log($"{_base.Name}'s {stat} has changed by {boost}");
         }
      }

      //calculating damage details. Stab output is based on whether attacker move type = damage taker pokemon type
      //critical output is determined from a random number gen. If random number is = or < a set value, then the critical (damage multiplier) = 2
   public DamageDetails TakeDamage(Move move, Pokemon attacker)
   {
      float critical = 1f;

      float stab = 1f;
      
      if (move.Base.MoveType == attacker.Base.Type1 || move.Base.MoveType == attacker.Base.Type2)
      {
         stab = 1.5f;
      }

      if (Random.value * 100f <= 6.25f)
      {
         critical = 2f;
      }
      
      //uses a 2D array to calculate type effectiveness. The incoming damage on attack receiver is multiplied by the effectiveness value
      //the 2D array output simply sets a float value by multiplying column by row
      //typechart (2D array) exists on PokemonBase.cs and is therefore usable by Pokemon.cs
      float effectivity =  TypeChart.GetEffectiveness(move.Base.MoveType, Base.Type1 ) *
                           TypeChart.GetEffectiveness(move.Base.MoveType, Base.Type2);

      Debug.Log("Move effectivity = " + effectivity);

      var dmgDetails = new DamageDetails
      {
         Effectiveness = effectivity,
         Critical = critical,
         Fainted = false,
         Stab = stab
         
      };

      float attack = (move.Base.MoveCategory == MoveCategory.Special) ? attacker.SpAttack : attacker.Attack;
      float defence = (move.Base.MoveCategory == MoveCategory.Special) ? SpDefence : Defence;
      
      float modifiers = Random.Range(0.85f, 1f) * effectivity * critical * stab;
      float a = (2 * attacker.Level + 10) / 250f;
      float d = a * move.Base.movePower * (attack / defence) + 2;
      int dmg = Mathf.FloorToInt(d * modifiers);
      
      currentHP -= dmg;
      if (currentHP <= 0)
      {
         currentHP = 0;
         dmgDetails.Fainted = true;
      }

      return dmgDetails;
   }
   
   //determines move used from existing move list for enemy pokemon
   public Move GetRandomMove()
   {
      int r = Random.Range(0, Moves.Count);
      
      
      return Moves[r];
   }

   public void OnBattleOver()
   {
      ResetStatBoost();
   }
}
//these bools and float are returned from the attack function in BattleSystem
public class DamageDetails
{
   public bool Fainted              { get; set; }
   
   public float Critical            { get; set; }
   
   public float Effectiveness       { get; set; }
   
   public float Stab                { get; set; }
   
}
