﻿using System;
using System.Collections;
using UnityEngine;
using RPG.CameraUI;
using RPG.Gameplay;

namespace RPG.Characters
{
    public class PlayerControl : MonoBehaviour //no Idamageable because we are going from interface to component
    {
        SpecialAbilities abilities;
        Character character;
        WeaponSystem weaponSystem;
        

        private void Start()
        {
            weaponSystem = GetComponent<WeaponSystem>();
            character = GetComponent<Character>();
            abilities = GetComponent<SpecialAbilities>();
            RegisterForMouseEvents();
        }

        private void RegisterForMouseEvents()
        {
            var cameraRaycaster = FindObjectOfType<CameraRaycaster>();
            cameraRaycaster.onMouseOverEnemy += OnMouseOverEnemy; //Delegate
            cameraRaycaster.onMouseOverpotentiallyWalkable += onMouseOverpotentiallyWalkable;
            cameraRaycaster.onMouseOverNPC += onMouseOverNPC;
            cameraRaycaster.onMouseOverLootChest += onMouseOverLootChest;
        }

        private void Update()
        {         
            ScanForAbilityKeyDown();         
        }

        void onMouseOverpotentiallyWalkable(Vector3 destination)
        {
            if(Input.GetMouseButton(0))
            {
                weaponSystem.StopAttacking();
                character.SetDestination(destination);
            }
        }

        void onMouseOverLootChest(LootChest lootChest)
        {
            if (Input.GetMouseButton(0))
            {
                StartCoroutine(MoveToTarget(lootChest.gameObject));
                if (Vector3.Distance(lootChest.transform.position, this.gameObject.transform.position) < 5)
                {
                    StartCoroutine(lootChest.OpenLootChest());
                }
            }
        }

        void onMouseOverNPC(EnemyAI NPC)
        {
            if(Input.GetMouseButton(0))
            {
                // TODO@: Go To NPC
                StartCoroutine(MoveToTarget(NPC.gameObject));
                // Start Dialogue Seuquence.
                if(Vector3.Distance(NPC.transform.position, this.gameObject.transform.position) < 5)
                {
                    var dialogueTrigger = NPC.GetComponent<DialogueTrigger>();
                    if(dialogueTrigger)
                    {
                        dialogueTrigger.TriggerDialogue();
                        NPC.transform.LookAt(gameObject.transform);
                    }
                }
            }
        }

        private void ScanForAbilityKeyDown()
        {

            for (int keyIndex = 1; keyIndex < abilities.GetNumberOfAbilities(); keyIndex++)
            {
                if (Input.GetKeyDown(keyIndex.ToString()))
                {
                    abilities.AttemptSpecialAbility(keyIndex);
                }
            }
        }

        private bool IsTargetInRange(GameObject target)
        {
            float distanceToTarget = (target.transform.position - transform.position).magnitude;
            return distanceToTarget <= weaponSystem.GetCurrentWeapon().GetMaxAttackRange();
        }

        void OnMouseOverEnemy(EnemyAI enemy)
        {
            if (Input.GetMouseButton(0) && IsTargetInRange(enemy.gameObject))
            {
                weaponSystem.AttackTarget(enemy.gameObject);
            }
            else if (Input.GetMouseButton(0) && !IsTargetInRange(enemy.gameObject))
            {
                StartCoroutine(MoveAndAttack(enemy));
            }
            else if (Input.GetMouseButtonDown(1) && IsTargetInRange(enemy.gameObject))
            {
                abilities.AttemptSpecialAbility(0, enemy.gameObject); 
            }
            else if (Input.GetMouseButtonDown(1) && !IsTargetInRange(enemy.gameObject))
            {
                StartCoroutine(MoveAndPowerAttack(enemy));
            }
        }

        IEnumerator MoveToTarget(GameObject target)
        {
            this.character.SetDestination(target.transform.position);
            while(!IsTargetInRange(target))
            {
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForEndOfFrame();
        }

        IEnumerator MoveAndAttack(EnemyAI enemy)
        {
            yield return StartCoroutine(MoveToTarget(enemy.gameObject));
            weaponSystem.AttackTarget(enemy.gameObject);
        }

        IEnumerator MoveAndPowerAttack (EnemyAI enemy)
        {
            yield return StartCoroutine(MoveToTarget(enemy.gameObject));
            abilities.AttemptSpecialAbility(0, enemy.gameObject);
        }
    }
}
