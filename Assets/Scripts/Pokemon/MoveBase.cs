using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Move", menuName = "Pokemon/Create new move")]
public class MoveBase : ScriptableObject
{
  [SerializeField]  string moveName;
  
  [TextArea]
  [SerializeField]  string moveDescription;

  [SerializeField]  PokemonType moveType;
  [SerializeField]  Sprite typeSprite;
  [SerializeField]  MoveCategory moveCategory;
  [SerializeField]  public int movePP;
  [SerializeField]  public int movePower;
  [SerializeField]  public int moveAccuracy;

  public string Name
  {
    get { return moveName; }
  }

  public string Description
  {
    get { return moveDescription; }
  }

  public PokemonType MoveType
  {
    get { return moveType; }
  }

  public Sprite TypeSprite
  {
    get { return typeSprite; }
  }

  public MoveCategory MoveCategory
  {
    get { return moveCategory; }
  }

  public int MovePP
  {
    get { return movePP; }
  }

  public int MovePower
  {
    get { return movePower; }
  }

  public int MoveAccuracy
  {
    get { return moveAccuracy; }
  }
}

public enum MoveCategory
{
  Physical,
  Status,
  Special
}
