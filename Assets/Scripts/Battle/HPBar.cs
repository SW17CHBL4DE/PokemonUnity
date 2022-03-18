using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
   [SerializeField] GameObject healthBar;
   public float healthBarYScale;
   

   public void SetHP(float hpNormalized)
   {
      healthBar.transform.localScale = new Vector3(hpNormalized, healthBarYScale);
      
   }
   
   public IEnumerator SetHPLerp(float newHP)
   {
      float curHP     = healthBar.transform.localScale.x;
      float deltaHP = curHP - newHP;

      while (curHP - newHP > Mathf.Epsilon)
      {
         curHP -= deltaHP * Time.deltaTime;
         healthBar.transform.localScale = new Vector3(curHP, healthBarYScale);
         yield return null;
      }

      healthBar.transform.localScale = new Vector3(newHP, healthBarYScale);
      
   }
}
