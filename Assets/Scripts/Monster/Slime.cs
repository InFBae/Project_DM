using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : MonoBehaviour
{
    // ����, �ǰ�, �̵�, ������ ���
    // IAttackable, IHittable, IMovable
    private float attackCoolTime = 2f;
    private bool isAttack = true;
    private Vector3 moveDir;

    // ���۽� �������ϰ� �̵� ���� �ǰݷ�ƾ ������
    private void Awake()
    {
        moveDir = new Vector3(1, 0, 0);
        StartCoroutine(MoveRoutine());
        StartCoroutine(AttackRoutine());
    }

    private void Update()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position + moveDir - new Vector3(0,-3.5f,0), Vector3.down, 5f);

        if (hit.collider != null)
        {
            transform.Translate(moveDir * Time.deltaTime);
        }
        else
        {
            moveDir = -(moveDir);

        }
    }

    IEnumerator MoveRoutine()
    {
        

        //yield return new WaitForSeconds(0.1f);
        yield return null;
    }

    IEnumerator AttackRoutine()
    {
        if(attackCoolTime > 0 || isAttack)
        {

        }

        yield return null;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        isAttack = true;
    }

}
