using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public CharacterController controller;
    public Transform cam;

    public float speed = 6f;
    public float originSpeed;
    public static int hp = 3;

    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;
    Animator animator;
    private void Start()
    {
        originSpeed = speed;
        animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.isPause == false)
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 dir = new Vector3(horizontal, 0f, vertical).normalized; // 대각선으로 움직일 때, 방향만을 얻기 위해 벡터 정규화

            animator.SetFloat("MoveMotion", dir.magnitude + (speed / originSpeed) - 1);
            controller.Move(new Vector3(0, -5f, 0f) * Time.deltaTime);    // 중력

            if (dir.magnitude >= 0.1f)
            {
                float targetAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

                Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

                if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") == false)
                {
                    transform.rotation = Quaternion.Euler(0f, angle, 0f);
                    controller.Move(moveDir.normalized * speed * Time.deltaTime);
                }

            }

            if (speed > originSpeed)
                speed = Mathf.Lerp(speed, originSpeed, Time.deltaTime * 0.5f);

            if (Input.GetMouseButtonDown(0) && animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") == false)
            {
                animator.Play("Attack");
                SoundManager.instance.audioSourceEFX.PlayOneShot(SoundManager.instance.audioAttack);                
            }
             
        }
    }

    public void Damaged()
    {
        speed = originSpeed;
        animator.SetTrigger("PlayerDamaged");
        UIManager.playerDamaged = true;
        hp--;     
        speed *= 4;

        if (hp == 0)
            GameManager.instance.LoseGame();
    }
}
