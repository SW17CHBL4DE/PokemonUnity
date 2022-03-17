using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnit : MonoBehaviour
{
    //ref to pokemon base class
    [SerializeField] PokemonBase _base;
    
    //pokemon level int variable
    [SerializeField] int level;
    
    //bool for determining player control
    [SerializeField] bool bIsPlayerUnit;

    //pokemon class ref, declared as property
    public Pokemon Pokemon { get; set; }
    
    //set appropriate sprite dependent on player controlled or not
    public void Setup()
    {
        Pokemon = new Pokemon(_base, level);
        if (bIsPlayerUnit)
        {
            GetComponent<Image>().sprite = Pokemon.Base.B_Sprite;
        }
        else
        {
            GetComponent<Image>().sprite = Pokemon.Base.F_Sprite;
        }
    }
}
