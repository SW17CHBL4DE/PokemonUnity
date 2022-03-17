using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BattleDialogueBox : MonoBehaviour
{
   [SerializeField] int          textSpeed;

   [SerializeField] Color        highlightColor;
   [SerializeField] Text         dialgoueText;
   [SerializeField] GameObject   actionSelector;
   [SerializeField] GameObject   moveSelector;
   [SerializeField] GameObject   moveDetails;
   public MoveTypeScript moveType;

   [SerializeField] List<Text> actionTexts;
   [SerializeField] List<Text> moveTexts;
   
   [SerializeField] Text ppText;

   public void SetDialogue(string dialogue)
   {
      dialgoueText.text = dialogue;
   }

   public IEnumerator TypewriteDialogue(string dialogue)
   {
      dialgoueText.text = "";
      foreach (var letter in dialogue.ToCharArray())
      {
         dialgoueText.text += letter;
         yield return new WaitForSeconds(1f/textSpeed);
      }

      yield return new WaitForSeconds(1.5f);
   }

   public void EnableDialogueText(bool enabled)
   {
      dialgoueText.enabled = enabled;
   }
   
   public void EnableActionSelector(bool enabled)
   {
      actionSelector.SetActive(enabled);
   }
   
   public void EnableMoveSelector(bool enabled)
   {
      moveSelector.SetActive(enabled);
      moveDetails.SetActive(enabled);
   }
   
   public void UpdateActionSelection(int selectedAction)
   {
      for (int i = 0; i < actionTexts.Count; ++i)
      {
         actionTexts[i].color = i == selectedAction ? highlightColor : Color.black;
      }
   }

   public void UpdateMoveSelection(int selectedMove, Move move)
   {
      for (int i = 0; i < moveTexts.Count; ++i)
      {
         moveTexts[i].color = i == selectedMove ? highlightColor : Color.black;
      }

      ppText.text = $"PP {move.movePP}/{move.Base.MovePP}";
      moveType.UpdateImage(move.Base.TypeSprite);



   }

   public void SetMoveNames(List<Move> moves)
   {
      for (int i = 0; i <moveTexts.Count; ++i)
      {
         moveTexts[i].text = i < moves.Count ? moves[i].Base.Name : "-";
      }
   }
}
