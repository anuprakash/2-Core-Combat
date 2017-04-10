﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Player : MonoBehaviour, IDamageable {

    [SerializeField] int enemyLayer = 9;
    [SerializeField] float maxHealthPoints = 100f;
    [SerializeField] float damagePerHit = 10f;
    [SerializeField] float minTimeBetweenHits = 1f;
    [SerializeField] float maxAttackRange = 2f;
    [SerializeField] Weapon weaponInUse;
    [SerializeField] AnimatorOverrideController animOverride;

    const string ATTACK = "Attack";
    const string DEFAULT_ANIM_CLIP_NAME = "PlayerDefault";
    float currentHealthPoints;
    CameraRaycaster cameraRaycaster;
    float lastHitTime = 0f;
    GameObject enemy;
    Animator animator;
    bool isAttacking = false;

    public float healthAsPercentage { get { return currentHealthPoints / maxHealthPoints; }}

    void Awake()
    {
        animator = GetComponent<Animator>();
        animator.runtimeAnimatorController = animOverride;
        animOverride[DEFAULT_ANIM_CLIP_NAME] = weaponInUse.attackAnimation;
    }

    void Start()
    {
        RegisterForMouseClick();
        currentHealthPoints = maxHealthPoints;
        PutWeaponInHand();
    }

    public bool IsAttacking()
    {
        return isAttacking;
    }

    private void PutWeaponInHand()
    {
        var weaponPrefab = weaponInUse.GetWeaponPrefab();
        GameObject dominantHand = RequestDominantHand();
        var weapon = Instantiate(weaponPrefab, dominantHand.transform);
        weapon.transform.localPosition = weaponInUse.gripTransform.localPosition;
        weapon.transform.localRotation = weaponInUse.gripTransform.localRotation;
    }

    private GameObject RequestDominantHand()
    {
        var dominantHands = GetComponentsInChildren<DominantHand>();
        int numberOfDominantHands = dominantHands.Length;
        Assert.IsFalse(numberOfDominantHands <= 0, "No DominantHand found on Player, please add one");
        Assert.IsFalse(numberOfDominantHands >  1, "Multiple DominantHand scripts on Player, please remove one");
        return dominantHands[0].gameObject;
    }

    private void RegisterForMouseClick()
    {
        cameraRaycaster = FindObjectOfType<CameraRaycaster>();
        cameraRaycaster.notifyMouseClickObservers += OnMouseClick;
    }

    // TODO refactor to reduce number of lines
    void OnMouseClick(RaycastHit raycastHit, int layerHit)
    {
        if (layerHit == enemyLayer)
        {
            enemy = raycastHit.collider.gameObject;

            // Check enemy is in range 
            if ((enemy.transform.position - transform.position).magnitude > maxAttackRange)
            {
                return;
            }

            RequestAttack();   
        }
    }

    void Update()
    {
        if (Time.time - lastHitTime > minTimeBetweenHits)
        {
            isAttacking = false;
        }
   }

    private void RequestAttack()
    {
        var enemyComponent = enemy.GetComponent<Enemy>();
        if (!isAttacking)
        {
            isAttacking = true;
            enemyComponent.TakeDamage(damagePerHit);
            transform.LookAt(enemy.transform);
            animator.SetTrigger(ATTACK);
            lastHitTime = Time.time;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealthPoints = Mathf.Clamp(currentHealthPoints - damage, 0f, maxHealthPoints);
    }
}
