using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Attach this script to an Image GameObject and set its Source Image to the Sprite you would like.

public class MoveTypeScript : MonoBehaviour
{
    [SerializeField]    Image moveTypeImage;

    public void Start()
    {
        //Fetch the Image form the GameObject
        moveTypeImage = GetComponent<Image>();
    }

    public void UpdateImage(Sprite movSprite)
    {
        moveTypeImage.sprite = movSprite;
    }
}
