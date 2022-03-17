using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum GameState
{
    FreeRoam,
    Battle
}
public class GameController : MonoBehaviour
{

    [SerializeField] PlayerController controller;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera mainCamera;
    
    
    GameState state;

    private void Start()
    {
        controller.OnEncountered += StartBattle;
        battleSystem.OnEndBattle += EndBattle;
    }

    void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        mainCamera.gameObject.SetActive(false);
        battleSystem.StartBattle();

    }

    void EndBattle(bool victory)
    {
        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        mainCamera.gameObject.SetActive(true);
    }
    
    private void Update()
    {
        if (state == GameState.FreeRoam)
        {
            controller.HandleUpdate();
        }
        else if (state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
    }
}
