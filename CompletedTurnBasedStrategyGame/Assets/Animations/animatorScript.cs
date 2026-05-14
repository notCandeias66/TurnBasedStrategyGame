using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class animatorScript : MonoBehaviour
{
    public Animator swordsman1Animator;
    public Animator swordsman2Animator;

    // 1 animator
    int isAttackingHash1;
    int isKickingHash1;
    int isBlockingHash1;
    int isAttackedHash1;
    int isCelebratingHash;
    int isDodgingHash1;

    bool isAttacking1;
    bool isKicking1;
    bool isBlocking1;
    bool isAttacked1;
    bool isCelebrating;
    bool isDodging1;

    // 2 animator
    int isAttackingHash2;
    int isKickingHash2;
    int isBlockingHash2;
    int isAttackedHash2;
    int isDyingHash;
    int isDodgingHash2;

    bool isAttacking2;
    bool isKicking2;
    bool isBlocking2;
    bool isAttacked2;
    bool isDying;
    bool isDodging2;

    bool attackInProgress = false;
    bool isDead = false;
    bool isFading = false;

    public AudioSource audioBackground;
    public AudioSource audioCelebrate;
    public AudioSource audioAttack;
    public AudioSource audioDeath;
    public AudioSource audioKick;

    public TMPro.TMP_Text text;

    public bool isUnloading = false;

    void Start()
    {
        // 1 animator
        isAttackingHash1 = Animator.StringToHash("isAttacking1");
        isKickingHash1 = Animator.StringToHash("isKicking1");
        isBlockingHash1 = Animator.StringToHash("isBlocking1");
        isAttackedHash1 = Animator.StringToHash("isAttacked1");
        isCelebratingHash = Animator.StringToHash("isCelebrating");
        isDodgingHash1 = Animator.StringToHash("isDodging1");

        // 2 animator
        isAttackingHash2 = Animator.StringToHash("isAttacking2");
        isKickingHash2 = Animator.StringToHash("isKicking2");
        isBlockingHash2 = Animator.StringToHash("isBlocking2");
        isAttackedHash2 = Animator.StringToHash("isAttacked2");
        isDyingHash = Animator.StringToHash("isDying");
        isDodgingHash2 = Animator.StringToHash("isDodging2");

        if(audioBackground != null) audioBackground.Play();
    }

    void Update()
    {
        if (isDead && Input.GetKeyDown(KeyCode.Escape))
        {
            Loaderv4.invisible.SetActive(true);
            SceneManager.UnloadSceneAsync("Cutscene");
            Loaderv4.inBattle = false;
        }

        UpdateFlags();

        if (isIdle1() && isIdle2() && !attackInProgress)
        {
            attackInProgress = true;
            StartCoroutine(HandleAttack());
        }
    }

    private void UpdateFlags()
    {
        // Update flags for animator 1
        isAttacking1 = swordsman1Animator.GetBool(isAttackingHash1);
        isKicking1 = swordsman1Animator.GetBool(isKickingHash1);
        isBlocking1 = swordsman1Animator.GetBool(isBlockingHash1);
        isAttacked1 = swordsman1Animator.GetBool(isAttackedHash1);
        isCelebrating = swordsman1Animator.GetBool(isCelebratingHash);
        isDodging1 = swordsman1Animator.GetBool(isDodgingHash1);

        // Update flags for animator 2
        isAttacking2 = swordsman2Animator.GetBool(isAttackingHash2);
        isKicking2 = swordsman2Animator.GetBool(isKickingHash2);
        isBlocking2 = swordsman2Animator.GetBool(isBlockingHash2);
        isAttacked2 = swordsman2Animator.GetBool(isAttackedHash2);
        isDying = swordsman2Animator.GetBool(isDyingHash);
        isDodging2 = swordsman2Animator.GetBool(isDodgingHash2);
    }

    private IEnumerator HandleAttack()
    {
        float randomAttacker = Random.value;
        if (randomAttacker < 0.5f)
        {
            if (randomAttacker < 0.25f)
            {
                swordsman1Animator.SetBool(isAttackingHash1, true);
                if(audioAttack != null) audioAttack.Play();
                yield return ExecuteRandomAction(swordsman1Animator, swordsman2Animator, isAttackingHash1);
            }
            else
            {
                swordsman1Animator.SetBool(isKickingHash1, true);
                if(audioKick != null) audioKick.Play();
                yield return ExecuteRandomAction(swordsman1Animator, swordsman2Animator, isKickingHash1);
            }
        }
        else
        {
            if (randomAttacker < 0.75f)
            {
                swordsman2Animator.SetBool(isAttackingHash2, true);
                if(audioAttack != null) audioAttack.Play();
                yield return ExecuteRandomAction(swordsman2Animator, swordsman1Animator, isAttackingHash2);
            }
            else
            {
                swordsman2Animator.SetBool(isKickingHash2, true);
                if(audioKick != null) audioKick.Play();
                yield return ExecuteRandomAction(swordsman2Animator, swordsman1Animator, isKickingHash2);
            }
        }
    }

    private IEnumerator ExecuteRandomAction(Animator attackerAnimator, Animator defenderAnimator, int attackHash)
    {
        float randomAction = Random.value;
        int actionHash = 0;

        if (attackerAnimator == swordsman1Animator)
        {
            if (randomAction < 0.33f)
            {
                actionHash = isAttackedHash2;
                defenderAnimator.SetBool(actionHash, true);

                if (randomAction < 0.1f)
                {
                    isFading = true;
                }
            }
            else if (randomAction < 0.66f)
            {
                actionHash = isBlockingHash2;
                defenderAnimator.SetBool(actionHash, true);
            }
            else
            {
                actionHash = isDodgingHash2;
                defenderAnimator.SetBool(actionHash, true);
            }

            yield return new WaitForSeconds(Mathf.Max(attackerAnimator.GetCurrentAnimatorStateInfo(0).length, defenderAnimator.GetCurrentAnimatorStateInfo(0).length));

            attackerAnimator.SetBool(attackHash, false);

            if (actionHash != isDyingHash && !isFading)
            {
                defenderAnimator.SetBool(actionHash, false);
            }

            if (isFading)
            {
                actionHash = isDyingHash;
                if(audioDeath != null) audioDeath.Play();
                defenderAnimator.SetBool(actionHash, true);

                yield return new WaitForSeconds(defenderAnimator.GetCurrentAnimatorStateInfo(0).length);

                attackerAnimator.SetBool(isCelebratingHash, true);
                if(audioBackground != null) audioBackground.Stop();
                if(audioCelebrate != null) audioCelebrate.Play();
                isDead = true;
                
                text.gameObject.SetActive(true);
            }
                
        } else {
            if (randomAction < 0.33f)
            {
                actionHash = isAttackedHash1;
                defenderAnimator.SetBool(actionHash, true);
            }
            else if (randomAction < 0.66f)
            {
                actionHash = isDodgingHash1;
                defenderAnimator.SetBool(actionHash, true);
            }
            else
            {
                actionHash = isBlockingHash1;
                defenderAnimator.SetBool(actionHash, true);
            }

            yield return new WaitForSeconds(Mathf.Max(attackerAnimator.GetCurrentAnimatorStateInfo(0).length, defenderAnimator.GetCurrentAnimatorStateInfo(0).length));

            attackerAnimator.SetBool(attackHash, false);
            defenderAnimator.SetBool(actionHash, false);

        }
        attackInProgress = false;
    }

    private bool isIdle1()
    {
        return !isAttacking1 && !isKicking1 && !isBlocking1 && !isAttacked1 && !isCelebrating && !isDodging1;
    }

    private bool isIdle2()
    {
        return !isAttacking2 && !isKicking2 && !isBlocking2 && !isAttacked2 && !isDying && !isDodging2;
    }
}
