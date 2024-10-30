using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollider : MonoBehaviour
{
    public float jumpFower;

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("JumpPad"))
        {
            CharacterManager.Instance.Player.rigidbody.AddForce(Vector3.up * jumpFower, ForceMode.Impulse );
        }
    }
}
