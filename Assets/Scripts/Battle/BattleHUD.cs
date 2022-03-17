using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHUD : MonoBehaviour
{
    //game objects for name text, lvl text, and hpbar image
    [SerializeField] Text nameText;
    [SerializeField] Text lvlText;
    [SerializeField] HPBar hpBar;

    Pokemon _pokemon;

    //set relevant info from pokemon class
    public void SetData(Pokemon pokemon)
    {
        _pokemon = pokemon;
        
        //reference pokemon base name property
        nameText.text = pokemon.Base.Name;
        
        //get info from pokemon class, level
        lvlText.text = pokemon.Level.ToString();
        hpBar.SetHP((float)pokemon.currentHP / pokemon.MaxHP);
    }

    public IEnumerator UpdateHP()
    {
        yield return hpBar.SetHPLerp((float)_pokemon.currentHP / _pokemon.MaxHP);
    }
    
}
