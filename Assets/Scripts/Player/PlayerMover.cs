using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMover : MonoBehaviour
{
    Rigidbody2D rb;
    private Vector3 moveDir;
    private float moveSpeed = 3f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }


    private void Update()
    {
        Move();
    }

    private void Move()
    {
        rb.velocity = moveSpeed * moveDir;
    }

    private void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        moveDir = new Vector3(input.x, 0, input.y);
    }
    private void OnJump(InputValue value)
    {
        rb.AddForce(Vector2.up * 50f, ForceMode2D.Impulse);
    }
}
