using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//variable for the current state of battle
public enum BattleState
{
   Start,
   PlayerAction,
   PlayerMove,
   EnemyMove,
   Busy,
}
public class BattleSystem : MonoBehaviour
{
   //player pokemon on base
   [SerializeField] BattleUnit playerUnit;
   
   //player frame
   [SerializeField] BattleHUD playerHUD;
   
   //enemy pokemon on base
   [SerializeField] BattleUnit foeUnit;
   
   //enemy frame
   [SerializeField] BattleHUD foeHUD;
   
   //text box at bottom
   [SerializeField] BattleDialogueBox dialogueBox;

   //declaring battle state variable
   private BattleState state;
   private int currentAction;

   public event Action<bool> OnEndBattle;
   
   //move selection
   private int currentMove;

   PokemonParty playerParty;

   Pokemon wildPokemon;
   
   
   public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
   {

      this.playerParty = playerParty;
      this.wildPokemon = wildPokemon;
      
      //run setup battle coroutine
      StartCoroutine(SetupBattle());
   }

   //setup battle coroutine
   public IEnumerator SetupBattle()
   {
      playerUnit.Setup(playerParty.GetHealthyPokemon());
      playerHUD.SetData(playerUnit.Pokemon);
      foeUnit.Setup(wildPokemon);
      foeHUD.SetData(foeUnit.Pokemon);
      
      dialogueBox.SetMoveNames(playerUnit.Pokemon.Moves);
      
      //will run text write coroutine in text box class when this coroutine finishes
      yield return dialogueBox.TypewriteDialogue($"A wild {foeUnit.Pokemon.Base.Name} appeared!");

      StartCoroutine(PlayerAction());
      
   }

   //runs function in player action state
   IEnumerator PlayerAction()
   {
      StartCoroutine(dialogueBox.TypewriteDialogue($"What will {playerUnit.Pokemon.Base.Name} do?"));
      state = BattleState.PlayerAction;
      dialogueBox.EnableActionSelector(true);

      return null;
   }

   void OpenPartyMenu()
   {
      print("party screen");
   }

   //runs function at player move state
   void PlayerMove()
   {
      state = BattleState.PlayerMove;
      dialogueBox.EnableActionSelector(false);
      dialogueBox.EnableDialogueText(false);
      dialogueBox.EnableMoveSelector(true);
   }

   //change actions based on current selection
   private void ActionSelectHandler()
   {
      if (Input.GetKeyDown(KeyCode.RightArrow))
      {
         ++currentAction;
      }
      else if (Input.GetKeyDown(KeyCode.LeftArrow))
      {
         --currentAction;
      }
      else if (Input.GetKeyDown(KeyCode.DownArrow))
      {
         currentAction += 2;
      }
      else if (Input.GetKeyDown(KeyCode.UpArrow))
      {
         currentAction -= 2;
      }

      currentAction = Mathf.Clamp(currentAction, 0, 3);
      
      dialogueBox.UpdateActionSelection(currentAction);

      if (Input.GetKeyDown(KeyCode.Return))
      switch (currentAction)
      {
         case 0:
            //fight
            PlayerMove();
            break;
         case 1:
            //bag
            break;
         case 2:
            //pokemon
            OpenPartyMenu();
            break;
         case 3:
            //run
            break;
      }
   }

   private void MoveSelectionHandle()
   {
      if (Input.GetKeyDown(KeyCode.RightArrow))
      {
         ++currentMove;
      }
      else if (Input.GetKeyDown(KeyCode.LeftArrow))
      {
         --currentMove;
      }
      else if (Input.GetKeyDown(KeyCode.DownArrow))
      {
         currentMove += 2;
      }
      else if (Input.GetKeyDown(KeyCode.UpArrow))
      {
         currentMove -= 2;
      }

      currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Pokemon.Moves.Count - 1);

      dialogueBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);
      
      if (Input.GetKeyDown(KeyCode.Return))
      {
         dialogueBox.EnableMoveSelector(false);
         dialogueBox.EnableDialogueText(true);
         StartCoroutine(PerformPlayerMove());
      }
      else if (Input.GetKeyDown(KeyCode.Escape))
      {
         dialogueBox.EnableMoveSelector(false);
         dialogueBox.EnableDialogueText(true);
         PlayerAction();
      }
   }

   
   //PLAYER POKEMON MOVE
   IEnumerator PerformPlayerMove()
   {
      //set non interactable state so player cannot fuck around with order of operation
      state = BattleState.Busy;
      
      //set move variable to selected move
      var move = playerUnit.Pokemon.Moves[currentMove];
      move.movePP--;
      
      //show move use text
      yield return dialogueBox.TypewriteDialogue($"{playerUnit.Pokemon.Base.Name} used {move.Base.Name}");
      
      Debug.Log("Player used = " + move.Base.Name);
      
      playerUnit.PlayAttackAnimation();
      foeUnit.PlayHitAnimation();
      yield return new WaitForSeconds(1f);

      //use returned class as variable to determine consequent text and update hp
      var dmgDetails = foeUnit.Pokemon.TakeDamage(move, playerUnit.Pokemon);
      yield return foeHUD.UpdateHP();
      yield return ShowDamageDetails(dmgDetails);

      if (dmgDetails.Fainted)
      {
         yield return dialogueBox.TypewriteDialogue($"{foeUnit.Pokemon.Base.Name} has fainted");
         foeUnit.PlayFaintAnimation();
         yield return new WaitForSeconds(2);
         OnEndBattle(true);
      }
      
      //if enemy pokemon didnt faint, start their move
      else
      {
         StartCoroutine(PerformEnemyMove());
      }
   }

   //ENEMY POKEMON MOVE
   IEnumerator PerformEnemyMove()
   {
      //set state to enemy move
      state = BattleState.EnemyMove;

      //enemy pokemon move is randomly generated from their assigned move list
      var move = foeUnit.Pokemon.GetRandomMove();
      move.movePP--;
      
      //show chosen move as text
      yield return dialogueBox.TypewriteDialogue($"{foeUnit.Pokemon.Base.Name} used {move.Base.Name}");
      
      Debug.Log("Enemy used = " + move.Base.Name);
      
      foeUnit.PlayAttackAnimation();
      playerUnit.PlayHitAnimation();
      yield return new WaitForSeconds(1f);

      //use returned details class to determine consequent text and update hp
      var dmgDetails = playerUnit.Pokemon.TakeDamage(move, foeUnit.Pokemon);
      yield return playerHUD.UpdateHP();
      yield return ShowDamageDetails(dmgDetails);

      if (dmgDetails.Fainted)
      {
         yield return dialogueBox.TypewriteDialogue($"{playerUnit.Pokemon.Base.Name} has fainted!");
         playerUnit.PlayFaintAnimation();
         yield return new WaitForSeconds(2f);

         var nextPokemon = playerParty.GetHealthyPokemon();
         if (nextPokemon != null)
         {
            playerUnit.Setup(nextPokemon);
            playerHUD.SetData(nextPokemon);

            dialogueBox.SetMoveNames(nextPokemon.Moves);
            
            yield return dialogueBox.TypewriteDialogue($"Go {nextPokemon.Base.Name}!");

            StartCoroutine(PlayerAction());
         }
         else
         {
            OnEndBattle(false);
         }
      }
      
      //if player pokemon didnt faint, set state to player action through function
      else
      {
         PlayerAction();
      }
   }

   IEnumerator ShowDamageDetails(DamageDetails dmgDetails)
   {
      if (dmgDetails.Critical > 1f)
      {
         yield return dialogueBox.TypewriteDialogue("A critical hit!");
      }

      if (dmgDetails.Effectiveness > 1f)
      {
         yield return dialogueBox.TypewriteDialogue("It's super effective!");
      }
      
      else if (dmgDetails.Effectiveness < 1f)
      {
         yield return dialogueBox.TypewriteDialogue("It's not very effective!");
      }
   }
   
   public void HandleUpdate()
   {
      if (state == BattleState.PlayerAction)
      {
         ActionSelectHandler();
      }
      else if (state == BattleState.PlayerMove)
      {
         MoveSelectionHandle();
      }
   }
}
