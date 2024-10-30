using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerController controller;
    public PlayerCondition condition;
    public PlayerCollider collider;

    public ItemData itemData;
    public Action addItem;
    
    public Rigidbody rigidbody;
    public Transform dropPosition;
    private void Awake()
    {
        CharacterManager.Instance.Player = this;
        controller = GetComponent<PlayerController>();
        condition = GetComponent<PlayerCondition>();
        collider = GetComponent<PlayerCollider>();
        rigidbody = GetComponent<Rigidbody>();
    }
}
