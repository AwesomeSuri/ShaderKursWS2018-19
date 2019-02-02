using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowController : MonoBehaviour
{
    [SerializeField]
    Transform linePointUp;
    [SerializeField]
    Transform linePointMid;
    [SerializeField]
    Transform linePointDown;

    [Space]
    [SerializeField]
    LineRenderer line;
    [SerializeField]
    Transform[] bones;

    float startAngle;
    float bendedAngle = 50;
    float currentAngle;
    float toAngle;
    bool bended;

    // Start is called before the first frame update
    void Awake()
    {
        startAngle = bones[0].localEulerAngles.y;
        if (startAngle > 180)
        {
            startAngle = (startAngle - 360) * -1;
        }
        currentAngle = startAngle;
        toAngle = startAngle;
    }

    private void Update()
    {
        // bend
        if (Mathf.Abs(currentAngle - toAngle) > .1f)
        {
            currentAngle = Mathf.Lerp(currentAngle, toAngle, Time.deltaTime * 50);

            for (int i = 0; i < bones.Length; i++)
            {
                Vector3 rot = bones[i].localEulerAngles;
                float angle = rot.y;
                if (angle > 180)
                {
                    angle = 360 - currentAngle;
                }
                else
                {
                    angle = currentAngle;
                }

                rot = new Vector3(rot.x, angle, rot.z);

                bones[i].localEulerAngles = rot;
            }
        }

        // line
        Vector3[] positions = new Vector3[3];
        positions[0] = linePointUp.position;
        positions[2] = linePointDown.position;
        if (bended)
        {
            positions[1] = linePointMid.position;
        }
        else
        {
            positions[1] = (positions[0] + positions[2]) / 2;
        }

        line.SetPositions(positions);
    }

    public void BendBow(bool bend)
    {
        print("bendbow");
        bended = bend;
        toAngle = bend ? bendedAngle : startAngle;
    }
}
