using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
   public float moveSpeed;
   
   private bool bIsMoving;

   private Vector2 input;
   private Animator animator;

   public LayerMask solidObjectsLayer;
   public LayerMask grassLayer;
   public float collisionCull;
   
   
   //delegate handling encounters
   public event Action OnEncountered;

   private void Awake()
   {
      animator = GetComponent<Animator>();
   }

   public void HandleUpdate()
   {
      if (!bIsMoving)
      {
         input.x = Input.GetAxisRaw("Horizontal");
         input.y = Input.GetAxisRaw("Vertical");

         if (input.x != 0) input.y = 0;

         if (input != Vector2.zero)
         {
            animator.SetFloat("moveX", input.x);
            animator.SetFloat("moveY", input.y);
            
            var targetPos = transform.position;
            targetPos.x          += input.x;
            targetPos.y          += input.y;
            
            if(bIsWalkable(targetPos))
            {
               StartCoroutine(Move(targetPos));
               
            }
         }
         animator.SetBool("bIsMoving?", bIsMoving);
      }
   }

   IEnumerator Move(Vector3 targetPos)
   {
      bIsMoving = true;
      while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
      {
         transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
         yield return null;
      }
      transform.position = targetPos;
      bIsMoving = false;

   WildEncounterCheck();
}

   private bool bIsWalkable(Vector3 targetPos)
   {
      return Physics2D.OverlapCircle(targetPos, collisionCull, solidObjectsLayer) == null;
   }

   private void WildEncounterCheck()
   {
      if (Physics2D.OverlapCircle(transform.position, 0.2f, grassLayer) == null) return;
      
      if (Random.Range(1, 101) <= 10)
      {
         animator.SetBool("bIsMoving?", false);
         OnEncountered();
      }
   }
}
