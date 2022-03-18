using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleUnit : MonoBehaviour
{

    //bool for determining player control
    [SerializeField] bool bIsPlayerUnit;
    [SerializeField] BattleHUD hud;

    public bool IsPlayerUnit
    {
        get { return bIsPlayerUnit; }
    }

    public BattleHUD HUD
    {
        get { return hud; }
    }
    //pokemon class ref, declared as property
    public Pokemon Pokemon { get; set; }

    Image image;
    Vector3 orgPos;
    Color orgColour;

    private void Awake()
    {
        image = GetComponent<Image>();
        orgPos = image.transform.localPosition;
        orgColour = image.color;
    }

    //set appropriate sprite dependent on player controlled or not
    public void Setup(Pokemon pokemon)
    {
        Pokemon = pokemon;
        if (bIsPlayerUnit)
        {
            image.sprite = Pokemon.Base.B_Sprite;
        }
        else
        {
            image.sprite = Pokemon.Base.F_Sprite;
        }
        
        hud.SetData(pokemon);

        image.color = orgColour;
        PlayEnterAnimation();
    }

    public void PlayEnterAnimation()
    {
        //set sprite positions outside of battle view before animation
        if (bIsPlayerUnit)
        {
            image.transform.localPosition = new Vector3(-125, orgPos.y);
        }
        else
        {
            image.transform.localPosition = new Vector3(125, orgPos.y);
        }

        image.transform.DOLocalMoveX(orgPos.x, 1f);
    }

    public void PlayAttackAnimation()
    {
        var sequence = DOTween.Sequence();
        if (bIsPlayerUnit)
        {
            sequence.Append(image.transform.DOLocalMoveX(orgPos.x + 50f, 0.25f));
        }
        else
        {
            sequence.Append(image.transform.DOLocalMoveX(orgPos.x - 50f, 0.25f));
        }

        sequence.Append(image.transform.DOLocalMoveX(orgPos.x, 0.25f));
    }

    public void PlayHitAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOColor(Color.gray, 0.2f));
        sequence.Append(image.DOColor(orgColour, 0.1f));
    }

    public void PlayFaintAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.transform.DOLocalMoveY(orgPos.y - 200f, 1f));
        sequence.Join(image.DOFade(0f, 0.2f));
    }
}
