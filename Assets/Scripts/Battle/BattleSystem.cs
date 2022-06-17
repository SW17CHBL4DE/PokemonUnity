using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//variable for the current state of battle
public enum BattleState
{
   Start,
   ActionSelection,
   MoveSelection,
   PerformMove,
   Busy,
   PartyScreen,
   BattleOver
}
public class BattleSystem : MonoBehaviour
{
   //player pokemon on base
   [SerializeField] BattleUnit playerUnit;

   //enemy pokemon on base
   [SerializeField] BattleUnit foeUnit;

   //text box at bottom
   [SerializeField] BattleDialogueBox dialogueBox;

   [SerializeField] PartyScreen partyScreen;

   //declaring battle state variable
   private BattleState state;
   private int currentAction;

   public event Action<bool> OnEndBattle;
   
   //move selection
   private int currentMove;

   PokemonParty playerParty;
   
   //pokemon selection
   private int currentMember;

   Pokemon wildPokemon;
   
   
   public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
   {

      this.playerParty = playerParty;
      this.wildPokemon = wildPokemon;
      
      //run setup battle coroutine
      StartCoroutine(SetupPokemonUnitsAndData());
   }

   //setup battle coroutine
   public IEnumerator SetupPokemonUnitsAndData()
   {
      playerUnit.Setup(playerParty.GetHealthyPokemon());
      foeUnit.Setup(wildPokemon);
      
      
      partyScreen.Init();
      
      dialogueBox.SetMoveNames(playerUnit.Pokemon.Moves);
      
      //will run text write coroutine in text box class when  ^ this v coroutine finishes
      yield return dialogueBox.TypewriteDialogue($"A wild {foeUnit.Pokemon.Base.Name} appeared!");

      ChooseFirstTurn();
      
   }

   //RUN IN ACTION SELECTION STATE
   IEnumerator ActionSelection()
   {
      StartCoroutine(dialogueBox.TypewriteDialogue($"What will {playerUnit.Pokemon.Base.Name} do?"));
      state = BattleState.ActionSelection;
      dialogueBox.EnableActionSelector(true);

      return null;
   }
   
   //CHANGE HIGHLIGHTED ACTIONS AND SELECTS BASED ON CURRENT SELECTION, CALLED IN HANDLE UPDATE (BOTTOM)
   private void ActivatePlayerAction()
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

      //SWITCH WHICH FORKS TO OTHER FUNCTIONS
      if (Input.GetKeyDown(KeyCode.Return))
         switch (currentAction)
         {
            case 0:
               //fight
               EnterMoveSelection();
               break;
            case 1:
               //bag
               break;
            case 2:
               //pokemon
               EnterPartyMenu();
               break;
            case 3:
               //run
               break;
         }
   }
   
   //FIGHT SELECTED: runs function at player move state
   void EnterMoveSelection()
   {
      state = BattleState.MoveSelection;
      dialogueBox.EnableActionSelector(false);
      dialogueBox.EnableDialogueText(false);
      dialogueBox.EnableMoveSelector(true);
   }

   //'POKEMON' SELECTED: runs function at party screen state
   void EnterPartyMenu()
   {
      partyScreen.SetPartyData(playerParty.Pokemons);
      partyScreen.gameObject.SetActive(true);
      state = BattleState.PartyScreen;
   }

   //IN PLAYER MOVE SELECTION STATE, CALLED IN HANDLE UPDATE (BOTTOM)
   private void ActivatePlayerMove()
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
         ActionSelection();
      }
   }
   
   //PLAYER POKEMON MOVE, CALLS RUN MOVE FUNCTION (BOTTOM)
   IEnumerator PerformPlayerMove()
   {
      //set non interactable state so player cannot fuck around with order of operation
      state = BattleState.PerformMove;
      
      //set move variable to selected move
      var move = playerUnit.Pokemon.Moves[currentMove];
      yield return RunMove(playerUnit, foeUnit, move);
      
      //if enemy pokemon didnt faint, start their move
      if (state == BattleState.PerformMove)
      {
         StartCoroutine(PerformEnemyMove());
      }
   }
   
   //ENEMY POKEMON MOVE, CALLS RUN MOVE FUNCTION (BOTTOM)
   IEnumerator PerformEnemyMove()
   {
      //set state to enemy move
      state = BattleState.PerformMove;

      //enemy pokemon move is randomly generated from their assigned move list
      var move = foeUnit.Pokemon.GetRandomMove();
      yield return RunMove(foeUnit, playerUnit, move);
      
      //if player pokemon didnt faint, set state to player action through function
      if (state == BattleState.PerformMove)
      {
         ActionSelection();
         
      }
   }

   //HIGHLIGHTS SELECTED POKEMON IN PARTY SCREEN AND RETRIEVES/DEPLOYS NEW POKEMON< CALLED IN HANDLE UPDATE (BOTTOM)
   private void ActivateNewPokemon()
   {
      if (Input.GetKeyDown(KeyCode.RightArrow))
      {
         ++currentMember;
         
      }
      else if (Input.GetKeyDown(KeyCode.LeftArrow))
      {
         --currentMember;
         
      }
      else if (Input.GetKeyDown(KeyCode.DownArrow))
      {
         currentMember += 2;
         
      }
      else if (Input.GetKeyDown(KeyCode.UpArrow))
      {
         currentMember -= 2;
         
      }
      
      currentMember = Mathf.Clamp(currentMember, 0, playerParty.Pokemons.Count - 1);
      partyScreen.UpdateMemberSelection(currentMember);
      
      if (Input.GetKeyDown(KeyCode.Return))
      {
         var selectedPokemon = playerParty.Pokemons[currentMember];
         if (selectedPokemon.currentHP <= 0)
         {
            partyScreen.SetMessageText($"{playerParty.Pokemons[currentMember]} is too tired to fight");
            return;
         }

         if (selectedPokemon == playerUnit.Pokemon)
         {
            partyScreen.SetMessageText($"{playerParty.Pokemons[currentMember]} is already fighting!");
            return;
         }

         partyScreen.gameObject.SetActive(false);
         state = BattleState.Busy;
         StartCoroutine(PerformActivatedPokemon(selectedPokemon));
      }
      else if (Input.GetKeyDown(KeyCode.Escape))
      {
         partyScreen.gameObject.SetActive(false);
         ActionSelection();
      }
   }

   IEnumerator PerformActivatedPokemon(Pokemon newPokemon)
   {

      bool currentPokemonFainted = true;

      if (playerUnit.Pokemon.currentHP > 0)
      {
         currentPokemonFainted = false;
         yield return dialogueBox.TypewriteDialogue($"Comeback {playerUnit.Pokemon.Base.Name}!");
         playerUnit.PlayFaintAnimation();
         yield return new WaitForSeconds(2f);
      }

      playerUnit.Setup(newPokemon);

      dialogueBox.SetMoveNames(newPokemon.Moves);
            
      yield return dialogueBox.TypewriteDialogue($"Go {newPokemon.Base.Name}!");

      if (currentPokemonFainted)
      {
         ChooseFirstTurn();
      }
      else
      {
         StartCoroutine(PerformEnemyMove());  
      }
   }

   void ChooseFirstTurn()
   {
      if (playerUnit.Pokemon.Speed >= foeUnit.Pokemon.Speed)
      {
         ActionSelection();
      }
      else
      {
         StartCoroutine(PerformEnemyMove());
      }
   }

   //HANDLES ALL MOVES FOR FOE AND PLAYER POKEMON
   IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit destinationUnit, Move move)
   {
      move.movePP--;
      
      //show move use text
      yield return dialogueBox.TypewriteDialogue($"{sourceUnit.Pokemon.Base.Name} used {move.Base.Name}");
      
      Debug.Log("Player used = " + move.Base.Name);
      
      sourceUnit.PlayAttackAnimation();
      destinationUnit.PlayHitAnimation();
      yield return new WaitForSeconds(1f);

      if (move.Base.MoveCategory == MoveCategory.Status)
      {
         yield return RunMoveEffects(move, sourceUnit.Pokemon, destinationUnit.Pokemon);
      }
      else
      {
         //use returned class as variable to determine consequent text and update hp
         var dmgDetails = destinationUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
         yield return destinationUnit.HUD.UpdateHP();
         yield return ShowDamageDetails(dmgDetails);
      }
      
      if (destinationUnit.Pokemon.currentHP <= 0)
      {
         yield return dialogueBox.TypewriteDialogue($"{destinationUnit.Pokemon.Base.Name} has fainted");
         destinationUnit.PlayFaintAnimation();
         yield return new WaitForSeconds(2);
         
         CheckForBattleOver(destinationUnit);
      }
   }

   IEnumerator RunMoveEffects(Move move, Pokemon source, Pokemon target)
   {
      var effects = move.Base.Effects.Boosts;
      if (effects != null)
      {
         if (move.Base.Target == MoveTarget.Self)
         {
            source.ApplyBoosts(effects);  
         }
         else
         {
            target.ApplyBoosts(effects);
         }

         yield return ShowStatusChanges(source);
         yield return ShowStatusChanges(target);
      }
   }

   IEnumerator ShowStatusChanges(Pokemon pokemon)
   {
      while (pokemon.StatusChanges.Count > 0)
      {
         var message = pokemon.StatusChanges.Dequeue();
         yield return dialogueBox.TypewriteDialogue(message);
      }
   }
   //POST MOVE DIALOGUE INFORMATION
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

   //CALLED DURING RUN MOVE AFTER EVERY POKEMON MOVE
   void CheckForBattleOver(BattleUnit faintedUnit)
   {
      if (faintedUnit.IsPlayerUnit)
      {
         var nextPokemon = playerParty.GetHealthyPokemon();
         if (nextPokemon != null)
         {
            
            EnterPartyMenu();
            
         }
         else
         {
            BattleOver(false);
         }
      }
      else
      {
         BattleOver(true);
      }
   }

   void BattleOver(bool victory)
   {
      state = BattleState.BattleOver;
      playerParty.Pokemons.ForEach(p => p.OnBattleOver());
      OnEndBattle(victory);
   }

   //UPDATE SELECTIONS IN VARIOUS STATES, CALLED FROM GAME CONTROLLER (CUSTOM TICK)
   public void HandleUpdate()
   {
      if (state == BattleState.ActionSelection)
      {
         ActivatePlayerAction();
      }
      else if (state == BattleState.MoveSelection)
      {
         ActivatePlayerMove();
      }
      else if (state == BattleState.PartyScreen)
      {
         ActivateNewPokemon();
      }
   }
}
