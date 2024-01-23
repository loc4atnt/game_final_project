using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleStateIKController : MonoBehaviour
{
    protected Animator animator;

    public Transform upHandPoint = null;
    public Transform rightRestHandPoint = null;
    public Transform leftRestHandPoint = null;
    public Transform rightHandPoint = null;
    public Transform leftHandPoint = null;
    public Transform rightLegPoint = null;
    public Transform leftLegPoint = null;

    private Transform currentLeftHandPoint = null;
    private Transform currentRightHandPoint = null;
    private Transform currentLeftLegPoint = null;
    private Transform currentRightLegPoint = null;

    public static int STATE_STRETCH_HAND = 0;
    public static int STATE_RAISE_HAND = 1;
    public static int STATE_REST_HAND = 2;
    //
    public static int STATE_STRETCH_LEG = 3;
    public static int STATE_REST_LEG = 4;

    public static int MATERIAL_NORMAL = 0;
    public static int MATERIAL_CORRECT = 1;

    public Material normalMaterial = null;
    public Material correctMaterial = null;

    private Renderer stateRenderer = null;

    public void moveLeftHand(int state)
    {
        if (state == STATE_STRETCH_HAND) currentLeftHandPoint = leftHandPoint;
        else if (state == STATE_RAISE_HAND) currentLeftHandPoint = upHandPoint;
        else if (state == STATE_REST_HAND) currentLeftHandPoint = leftRestHandPoint;
    }

    public void moveRightHand(int state)
    {
        if (state == STATE_STRETCH_HAND) currentRightHandPoint = rightHandPoint;
        else if (state == STATE_RAISE_HAND) currentRightHandPoint = upHandPoint;
        else if (state == STATE_REST_HAND) currentRightHandPoint = rightRestHandPoint;
    }

    public void moveLeftLeg(int state)
    {
        if (state == STATE_STRETCH_LEG) currentLeftLegPoint = leftLegPoint;
        else if (state == STATE_REST_LEG) currentLeftLegPoint = null;
    }

    public void moveRightLeg(int state)
    {
        if (state == STATE_STRETCH_LEG) currentRightLegPoint = rightLegPoint;
        else if (state == STATE_REST_LEG) currentRightLegPoint = null;
    }

    public void makeCorrectAnimation()
    {
        if (!stateRenderer) return;
        this.stateRenderer.material = correctMaterial;
    }

    public void makeNormalAnimation()
    {
        if (!stateRenderer) return;
        this.stateRenderer.material = correctMaterial;
    }

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        stateRenderer = GetComponent<Renderer>();
        makeNormalAnimation();
    }

    void OnAnimatorIK()
    {
        if (!animator) return;

        if (currentRightLegPoint)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);
            animator.SetIKPosition(AvatarIKGoal.RightFoot, currentRightLegPoint.position);
            animator.SetIKRotation(AvatarIKGoal.RightFoot, currentRightLegPoint.rotation);
        } else
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0);
        }

        if (currentLeftLegPoint)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
            animator.SetIKPosition(AvatarIKGoal.LeftFoot, currentLeftLegPoint.position);
            animator.SetIKRotation(AvatarIKGoal.LeftFoot, currentLeftLegPoint.rotation);
        }
        else
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0);
        }

        if (currentRightHandPoint)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKPosition(AvatarIKGoal.RightHand, currentRightHandPoint.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, currentRightHandPoint.rotation);
        }
        else
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
        }

        if (currentLeftHandPoint)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, currentLeftHandPoint.position);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, currentLeftHandPoint.rotation);
        }
        else
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
