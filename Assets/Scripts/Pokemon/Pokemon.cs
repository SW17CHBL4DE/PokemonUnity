using System.Collections.Generic;
using UnityEngine;


public class Pokemon
{
      public PokemonBase Base       { get; set; }
      public int         Level       { get; set; }

      public int        currentHP   { get; set; }
      public List<Move> Moves       { get; set; }

      public Pokemon(PokemonBase pBase, int pLevel)
      {
         Base       = pBase;
         Level       = pLevel;
         currentHP   = MaxHP;
      
         //Generate moves
         Moves = new List<Move>();
         foreach (var move in Base.LearnableMoves)
         {
            if (move.Level <= Level)
               Moves.Add(new Move(move.MoveBase));
         
            if (Moves.Count >= 4)
               break;
         }
      }

      public int MaxHP
      {
         get { return Mathf.FloorToInt((Base.MaxHP * Level) / 100f) + 10; }
      }

      public int Attack
      {
         get { return Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5; }
      }
   
      public int Defence
      {
         get { return Mathf.FloorToInt((Base.Defence * Level) / 100f) + 5; }
      }
   
      public int SpAttack
      {
         get { return Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5; }
      }
   
      public int SpDefence
      {
         get { return Mathf.FloorToInt((Base.SpDefence * Level) / 100f) + 5; }
      }
   
      public int Speed
      {
         get { return Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5; }
      }

   public DamageDetails TakeDamage(Move move, Pokemon attacker)
   {
      float critical = 1f;
      
      if (Random.value * 100f <= 6.25f)
      {
         critical = 2f;
      }
      
      float effectivity =  TypeChart.GetEffectiveness(move.Base.MoveType, Base.Type1 ) *
                           TypeChart.GetEffectiveness(move.Base.MoveType, Base.Type2);

      Debug.Log("Move effectivity = " + effectivity);

      var dmgDetails = new DamageDetails
      {
         Effectiveness = effectivity,
         Critical = critical,
         Fainted = false
      };
      
      float modifiers = Random.Range(0.85f, 1f) * effectivity * critical;
      float a = (2 * attacker.Level + 10) / 250f;
      float d = a * move.Base.movePower * ((float)attacker.Attack / Defence) + 2;
      int dmg = Mathf.FloorToInt(d * modifiers);
      
      currentHP -= dmg;
      if (currentHP <= 0)
      {
         currentHP = 0;
         dmgDetails.Fainted = true;
      }

      return dmgDetails;
   }

   public Move GetRandomMove()
   {
      int r = Random.Range(0, Moves.Count);
      
      
      return Moves[r];
   }
}

public class DamageDetails
{
   public bool Fainted              { get; set; }
   
   public float Critical            { get; set; }
   
   public float Effectiveness       { get; set; }
}
