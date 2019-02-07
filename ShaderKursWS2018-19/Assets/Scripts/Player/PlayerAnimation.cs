using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField]
    Collider sword;
    [SerializeField]
    BowController bow;
    [SerializeField]
    GameObject arrow;

    [Space]
    [SerializeField]
    SoundEffectPlayer sfx;

    Animator anim;
    bool aimTo;

    public bool IsSwinging { get; private set; }
    public float IsAiming { get; private set; }

    Vector3 swordIdlePos;                               // bug workaround
    Vector3 swordIdleRot;

    // Start is called before the first frame update
    void Awake()
    {
        anim = GetComponent<Animator>();

        swordIdlePos = sword.transform.localPosition;
        swordIdleRot = sword.transform.localEulerAngles;

        ResetAnim();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateMovement(float x, float y)
    {
        anim.SetFloat("SpeedX", x);
        anim.SetFloat("SpeedY", y);
    }

    public void SetFlying(bool flying)
    {
        anim.SetBool("Flying", flying);
    }

    public IEnumerator Swinging()
    {
        IsSwinging = true;

        anim.Play("Swing", 1);
        sfx.PlayAudio("WhooshMetal");

        float weight = 0;
        while (anim.GetLayerWeight(1) < 1)
        {
            weight += Time.deltaTime * 20;

            sword.transform.localScale = Vector3.one * (1 + weight * .5f);
            anim.SetLayerWeight(1, weight);

            yield return null;
        }

        sword.enabled = true;

        yield return new WaitForSeconds(.2f);

        sword.enabled = false;

        while (anim.GetLayerWeight(1) > 0)
        {
            weight -= Time.deltaTime * 10;

            sword.transform.localScale = Vector3.one * (1 + weight * .5f);
            anim.SetLayerWeight(1, weight);

            yield return null;
        }

        IsSwinging = false;

        sword.transform.localPosition = swordIdlePos;
        sword.transform.localEulerAngles = swordIdleRot;

        yield return null;
    }

    public void StartAiming(bool hasArrow)
    {
        aimTo = true;
        IsAiming = 0;

        StartCoroutine(Aiming());

        bow.BendBow(true);
        sfx.PlayAudio("BowReady");

        if (hasArrow)
        {
            arrow.SetActive(true);
        }
    }

    public void StopAiming()
    {
        aimTo = false;

        bow.BendBow(false);
        sfx.PlayAudio("BowFire");
        sfx.StopAudio("BowReady");

        arrow.SetActive(false);
    }

    IEnumerator Aiming()
    {
        while (IsAiming >= 0)
        {
            while (aimTo && IsAiming < 1
                || !aimTo && IsAiming > 0)
            {
                IsAiming += Time.deltaTime * 5 * (aimTo ? 1 : -1f);

                anim.SetLayerWeight(2, IsAiming);

                yield return null;
            }

            yield return null;
        }

        IsAiming = -1;
    }

    public void Die()
    {
        StopAllCoroutines();

        anim.Play("Dying");
    }

    public void ResetAnim()
    {
        anim.Play("Idle");

        IsAiming = -1;
        IsSwinging = false;
    }
}
