using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum OCEAN { O_pos, O_neg, C_pos, C_neg, E_pos, E_neg, A_pos, A_neg, N_pos, N_neg };

public class AgentController : MonoBehaviour {

    [Range(0f, 1f)] public float IK_MAIN_FACTOR_TARGET = 1f;
    [Range(0f, 1f)] public float IK_MAIN_FACTOR_BASE = 1f;
    [Range(0f, 1f)] public float IK_MAIN_FACTOR_CURRENT = 1f;
    [Range(0f, 1f)] public float TransitionFactor = 0f;
    public bool AnimationNoTransition = false;

    public AudioClip[] clips;
    private AudioSource audioSource;

    AnimatorInspector animatorInspector;

    [Header("Control Switches 1")]
    public bool C_LabanRotation;
    public bool C_LabanIK;
    public bool C_Fluctuation;
    public bool C_SpeedAdjust;
    public bool C_LookIK;
    public bool C_LookShift;
    public bool C_Fingers;

    [Header("Control Switches 2")]
    public bool C_SpeedTest;
    public bool C_SpeedConstant;
    public bool C_IKTest;
    public bool C_IKConstant;

    public bool IKWeightByPass;
    public bool IKALLBYPASS;
    public bool PutMarks;
    public bool TestForVid;
    public bool HipFix;

    public Transform HipsL;
    private Vector3 hipsPos;
    public GameObject MarkToPutR;
    public GameObject MarkToPutG;
    public GameObject MarkToPutB;
    public GameObject MarkToPutBlack;

    [Header("Control Switches 2")]
    public bool Map_OCEAN_to_LabanShape;
    public bool Map_OCEAN_to_LabanEffort;
    public bool Map_OCEAN_to_Additional;

    [Header("OCEAN Parameters")]
    [Range(-1f, 1f)] public float openness = 0f;
    [Range(-1f, 1f)] public float conscientiousness = 0f;
    [Range(-1f, 1f)] public float extraversion = 0f;
    [Range(-1f, 1f)] public float agreeableness = 0f;
    [Range(-1f, 1f)] public float neuroticism = 0f;

    [Header("Laban Effort Parameters")]
    [Range(-1f, 1f)] public float space = 0f;
    [Range(-1f, 1f)] public float weight = 0f;
    [Range(-1f, 1f)] public float time = 0f;
    [Range(-1f, 1f)] public float flow = 0f;

    [Header("Emotion Parameters")]
    [Range(0f, 1f)] public float e_happy = 0f;
    [Range(0f, 1f)] public float e_sad = 0f;
    [Range(0f, 1f)] public float e_angry = 0f;
    [Range(0f, 1f)] public float e_disgust = 0f;
    [Range(0f, 1f)] public float e_fear = 0f;
    [Range(0f, 1f)] public float e_shock = 0f;

    [Header("Base Expression Parameters")]
    [Range(-1f, 1f)] public float base_happy = 0f;
    [Range(-1f, 1f)] public float base_sad = 0f;
    [Range(-1f, 1f)] public float base_angry = 0f;
    [Range(-1f, 1f)] public float base_shock = 0f;
    [Range(-1f, 1f)] public float base_disgust = 0f;
    [Range(-1f, 1f)] public float base_fear = 0f;

    [Header("IK Parameters")]
    [Range(-1f, 1f)] public float IKFAC_forward;
    [Range(-1f, 1f)] public float IKFAC_up;
    [Range(-1f, 1f)] public float IKFAC_side;

    [Header("Look Shift Parameters")]
    [Range(0f, 100f)] public float ls_hor;
    [Range(0f, 100f)] public float ls_ver;
    [Range(0f, 5f)] public float ls_hor_speed;
    [Range(0f, 5f)] public float ls_ver_speed;

    [Header("Additional Body Parameters")]
    [Range(-1f, 1f)] public float spine_bend;
    //private readonly float spine_max = 12;
    private readonly float spine_max = 16;
    //private readonly float spine_min = -10;
    private readonly float spine_min = -14;
    [Range(-1f, 1f)] public float sink_bend;
    //private readonly float sink_max = 13;
    private readonly float sink_max = 18;
    // private readonly float sink_min = -13;
    private readonly float sink_min = -19;
    [Range(-1f, 1f)] public float head_bend;
    //private readonly float head_max = 2f;
    private readonly float head_max = 5f;
    //private readonly float head_min = -2f;
    private readonly float head_min = -5f;
    [Range(-1f, 1f)] public float finger_bend_open;
    private readonly float finger_open_max = 20f;
    private readonly float finger_open_min = -12f;
    [Range(-1f, 1f)] public float finger_bend_close;
    private readonly float finger_close_max = 30f;
    private readonly float finger_close_min = 0f;

    private readonly float multiplyRotationFactor = 1f;

    public GameObject lookObject;

    [HideInInspector] public Animator anim;

    public bool feetOnGround_left;
    public bool feetOnGround_right;

    Vector3 footDiff_left;
    Vector3 footDiff_right;

    [HideInInspector] public String text_O;
    [HideInInspector] public String text_C;
    [HideInInspector] public String text_E;
    [HideInInspector] public String text_A;
    [HideInInspector] public String text_N;

    // distances of body parts
    float d_upperArm, d_lowerArm, d_hand;

    // IK Targets
    private GameObject LeftHandIK;
    private GameObject RightHandIK;
    private GameObject BodyIK;
    private GameObject LeftFootIK;
    private GameObject RightFootIK;
    private GameObject HeadLookIK;

    [Header("Skeleton Transforms")]
    public Transform sk_head_top;
    public Transform sk_head;
    public Transform sk_neck;
    public Transform sk_shoulder_l;
    public Transform sk_shoulder_r;
    public Transform sk_arm_l;
    public Transform sk_arm_r;
    public Transform sk_hand_l;
    public Transform sk_hand_r;
    public Transform sk_spine;
    public Transform sk_hip;
    public Transform sk_leg_l;
    public Transform sk_leg_r;
    public Transform sk_knee_l;
    public Transform sk_knee_r;
    public Transform sk_foot_l;
    public Transform sk_foot_r;
    public Transform sk_foottip_l;
    public Transform sk_foottip_r;
    public Transform sk_handtip_l;
    public Transform sk_handtip_r;

    [Header("BodyParts")]
    public SkinnedMeshRenderer[] skins;
    public GameObject[] thingsToHide;


    #region START
    void Start() {
        audioSource = GetComponent<AudioSource>();

        animatorInspector = GameObject.Find("AnimatorInspector").GetComponent<AnimatorInspector>();

        // get the animator
        anim = GetComponent<Animator>();
        anim.logWarnings = false;

        ikRatioArray = new float[12];
        
        // find body part distances
        d_upperArm = (anim.GetBoneTransform(HumanBodyBones.LeftUpperArm).position - anim.GetBoneTransform(HumanBodyBones.LeftLowerArm).position).magnitude;
        d_lowerArm = (anim.GetBoneTransform(HumanBodyBones.LeftLowerArm).position - anim.GetBoneTransform(HumanBodyBones.LeftHand).position).magnitude;
        d_hand = (anim.GetBoneTransform(HumanBodyBones.LeftHand).position - anim.GetBoneTransform(HumanBodyBones.LeftIndexDistal).position).magnitude;

        footDiff_left = anim.GetBoneTransform(HumanBodyBones.LeftFoot).position - anim.GetBoneTransform(HumanBodyBones.LeftToes).position;

        footDiff_left = new Vector3(0, footDiff_left.y, 0);

        footDiff_right = anim.GetBoneTransform(HumanBodyBones.RightFoot).position - anim.GetBoneTransform(HumanBodyBones.RightToes).position;

        footDiff_right = new Vector3(0, footDiff_right.y, 0);

        // face script part
        GameObject body = GetChildGameObject(gameObject, "Body");
        if(body == null) body = GetChildGameObject(gameObject, "CC_Base_Body"); 
        GameObject body_default = GetChildGameObject(gameObject, "default");
        GameObject body_eyelashes = GetChildGameObject(gameObject, "Eyelashes");
        GameObject body_beards = GetChildGameObject(gameObject, "Beards");
        GameObject body_moustaches = GetChildGameObject(gameObject, "Moustaches");

        /*faceController = body.AddComponent<FaceScript>();
        if(body != null) faceController.meshRenderer = body.GetComponentInChildren<SkinnedMeshRenderer>();
        if (body_default != null) faceController.meshRendererEyes = body_default.GetComponent<SkinnedMeshRenderer>();
        if (body_eyelashes != null) faceController.meshRendererEyelashes = body_eyelashes.GetComponent<SkinnedMeshRenderer>();
        if(body_beards != null)
        {
            faceController.meshRendererBeards = body_beards.GetComponent<SkinnedMeshRenderer>();
        }
        if(body_moustaches != null)
        {
            faceController.meshRendererMoustaches = body_moustaches.GetComponent<SkinnedMeshRenderer>();
        }
        faceController.InitShapeKeys();
        */
        SinkPassInit();
        FluctuatePassInit();
        
        // Create IK Targets
        LeftHandIK = new GameObject ("LeftHandIK");
        RightHandIK = new GameObject("RightHandIK");
        BodyIK = new GameObject("BodyIK");
        LeftFootIK = new GameObject("LeftFootIK");
        RightFootIK = new GameObject("RightFootIK");
        HeadLookIK = new GameObject("HeadLookIK");

        LeftHandIK.transform.SetParent(gameObject.transform);
        RightHandIK.transform.SetParent(gameObject.transform);
        BodyIK.transform.SetParent(gameObject.transform);
        LeftFootIK.transform.SetParent(gameObject.transform);
        RightFootIK.transform.SetParent(gameObject.transform);
        HeadLookIK.transform.SetParent(gameObject.transform);

        // Init IK target positions
        LeftHandIK.transform.position = t_LeftHand.position;
        RightHandIK.transform.position = t_RightHand.position;
        BodyIK.transform.position = t_Hips.position + Vector3.up;
        LeftFootIK.transform.position = t_LeftFoot.position;
        RightFootIK.transform.position = t_RightFoot.position;
        HeadLookIK.transform.position = t_Head.position + t_Head.forward;


        LeftHandIK.AddComponent<MeshFilter>();
        LeftHandIK.AddComponent<MeshRenderer>();
        LeftHandIK.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        LeftHandIK.layer = 10;

        RightHandIK.AddComponent<MeshFilter>();
        RightHandIK.AddComponent<MeshRenderer>();
        RightHandIK.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        o_target_top = new GameObject("Top Anchor"); // Instantiate(sphereBlue);
        o_target_bottom = new GameObject("Bottom Anchor"); // Instantiate(sphereBlue); 
        o_target_forward = new GameObject("Forward Anchor"); // Instantiate(sphereBlue);
        o_target_back = new GameObject("Back Anchor"); // Instantiate(sphereBlue);
        o_target_left = new GameObject("Left Anchor"); // Instantiate(sphereBlue);
        o_target_right = new GameObject("Right Anchor"); // Instantiate(sphereBlue);  
        o_target_center = new GameObject("Center Anchor"); // Instantiate(sphereBlue); 

        o_target_top.transform.SetParent(gameObject.transform);
        o_target_bottom.transform.SetParent(gameObject.transform);
        o_target_forward.transform.SetParent(gameObject.transform);
        o_target_back.transform.SetParent(gameObject.transform);
        o_target_left.transform.SetParent(gameObject.transform);
        o_target_right.transform.SetParent(gameObject.transform);
        o_target_center.transform.SetParent(gameObject.transform);

        o_target_top.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
        o_target_top.transform.position = t_Hips.position + Vector3.up;

        o_target_bottom.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
        o_target_bottom.transform.position = t_Hips.position + Vector3.down;

        o_target_forward.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
        o_target_forward.transform.position = t_Hips.position + t_Hips.forward;

        o_target_back.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
        o_target_back.transform.position = t_Hips.position - t_Hips.forward;

        o_target_left.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
        o_target_left.transform.position = t_Hips.position - t_Hips.right;

        o_target_right.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
        o_target_right.transform.position = t_Hips.position + t_Hips.right;

        o_target_center.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
        o_target_center.transform.position = t_Hips.position;

        ren_target_top = o_target_top.GetComponentInChildren<Renderer>();
        ren_target_bottom = o_target_bottom.GetComponentInChildren<Renderer>();
        ren_target_left = o_target_left.GetComponentInChildren<Renderer>();
        ren_target_right = o_target_right.GetComponentInChildren<Renderer>();
        ren_target_center = o_target_center.GetComponentInChildren<Renderer>();
        ren_target_forward = o_target_forward.GetComponentInChildren<Renderer>();
        ren_target_back = o_target_back.GetComponentInChildren<Renderer>();

       

    }
    #endregion

    static public GameObject GetChildGameObject(GameObject fromGameObject, string withName)
    {
        Transform[] ts = fromGameObject.transform.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in ts) if (t.gameObject.name == withName) return t.gameObject;
        return null;
    }

    private bool talkFlag = false;
    private bool preTalkFlag = false;

    public float[] ikRatioArray;
    public float[] ikRatioArray_target;

    private Renderer ren_target_top;
    private Renderer ren_target_bottom;
    private Renderer ren_target_forward;
    private Renderer ren_target_back;
    private Renderer ren_target_left;
    private Renderer ren_target_right;
    private Renderer ren_target_center;

    private LineRenderer lr_target_top;
    private LineRenderer lr_target_bottom;
    private LineRenderer lr_target_forward;
    private LineRenderer lr_target_back;
    private LineRenderer lr_target_left;
    private LineRenderer lr_target_right;
    private LineRenderer lr_target_center;

    private GameObject o_target_top;
    private GameObject o_target_bottom;
    private GameObject o_target_forward;
    private GameObject o_target_back;
    private GameObject o_target_left;
    private GameObject o_target_right;
    private GameObject o_target_center;

    private Vector3 target_top;
    private Vector3 target_bottom;
    private Vector3 target_forward;
    private Vector3 target_back;
    private Vector3 target_left;
    private Vector3 target_right;
    private Vector3 target_center;



    private Vector3 pppl, pppr;

    private void AdjustIKTargets()
    {
        if (LeftHandIK == null || RightHandIK == null)
        {
            Debug.LogError("Hand IK targets are not set!");
            return;
        }

        // adjust targets
        target_top = t_Hips.position + Vector3.up;
        target_bottom = t_Hips.position + Vector3.down;
        target_forward = t_Hips.position + t_Hips.forward;
        target_back = t_Hips.position - t_Hips.forward;
        target_left = t_Hips.position - t_Hips.right;
        target_right = t_Hips.position + t_Hips.right;
        target_center = t_Hips.position;

        if(IKWeightByPass)
        {
            for(int i = 0; i < 12; i++)
            {
                ikRatioArray[i] = 1;
            }
        }

        float ar0 = Mathf.Clamp(IKFAC_up, 0, 1);
        float ar1 = Mathf.Clamp(-IKFAC_up, 0, 1);
        float ar2 = Mathf.Clamp(IKFAC_side, 0, 1);
        float ar3 = Mathf.Clamp(-IKFAC_side, 0, 1);
        float ar4 = Mathf.Clamp(IKFAC_forward, 0, 1);
        float ar5 = Mathf.Clamp(-IKFAC_forward, 0, 1);

        pppl = t_LeftHand.position
        + Vector3.Lerp(t_LeftHand.position, target_top, ar0 * ikRatioArray[0]) - t_LeftHand.position
        + Vector3.Lerp(t_LeftHand.position, target_bottom, ar1 * ikRatioArray[1]) - t_LeftHand.position
        + Vector3.Lerp(t_LeftHand.position, target_left, ar2 * ikRatioArray[2]) - t_LeftHand.position
        + Vector3.Lerp(t_LeftHand.position, target_center, ar3 * ikRatioArray[3]) - t_LeftHand.position
        + Vector3.Lerp(t_LeftHand.position, target_forward, ar4 * ikRatioArray[4]) - t_LeftHand.position
        + Vector3.Lerp(t_LeftHand.position, target_back, ar5 * ikRatioArray[5]) - t_LeftHand.position;

        pppr = t_RightHand.position
        + Vector3.Lerp(t_RightHand.position, target_top, ar0 * ikRatioArray[6]) - t_RightHand.position
        + Vector3.Lerp(t_RightHand.position, target_bottom, ar1 * ikRatioArray[7]) - t_RightHand.position
        + Vector3.Lerp(t_RightHand.position, target_right, ar2 * ikRatioArray[8]) - t_RightHand.position
        + Vector3.Lerp(t_RightHand.position, target_center, ar3 * ikRatioArray[9]) - t_RightHand.position
        + Vector3.Lerp(t_RightHand.position, target_forward, ar4 * ikRatioArray[10]) - t_RightHand.position
        + Vector3.Lerp(t_RightHand.position, target_back, ar5 * ikRatioArray[11]) - t_RightHand.position;
          
        LeftHandIK.transform.position = pppl;
        RightHandIK.transform.position = pppr;

        LeftFootIK.transform.position = t_LeftFoot.position - t_Hips.right * IKFAC_side * 0.01f;
        RightFootIK.transform.position = t_RightFoot.position + t_Hips.right * IKFAC_side * 0.01f;

        BodyIK.transform.position =
             (
                (
                    (IKFAC_forward > 0) ?
                    Vector3.Lerp(t_Neck.position, target_forward, IKFAC_forward * 0.5f) :
                    Vector3.Lerp(t_Neck.position, target_back, -IKFAC_forward * 0.5f)
                )
            );
    }

    // animator timers & flags
    public bool playAnimationWithoutTalk;

    public Vector3 armUp;
    public Vector3 armDown;

    private int AnimationNo;

    private void Update()
    {

    }

    private void OnAnimatorIK(int layerIndex)
    {
        if(IKALLBYPASS)
        {
            anim.SetIKPosition(AvatarIKGoal.LeftHand, LeftHandIK.transform.position);
            anim.SetIKPosition(AvatarIKGoal.RightHand, RightHandIK.transform.position);
            anim.SetIKPosition(AvatarIKGoal.LeftFoot, LeftFootIK.transform.position);
            anim.SetIKPosition(AvatarIKGoal.RightFoot, RightFootIK.transform.position);

            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
            anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
            return;
        }

        /*
         * UPDATE VARIABLES
         */

        if (Map_OCEAN_to_LabanShape) OCEAN_to_LabanShape();
        if (Map_OCEAN_to_LabanEffort) OCEAN_to_LabanEffort();
        if (Map_OCEAN_to_Additional) OCEAN_to_Additional();

        if (C_SpeedAdjust)
        {
            // anim.speed = animatorInspector.GetCurrentSpeed(anim, ScaleBetween(time, 0.4f, 1f, -1f, 1f), ScaleBetween(time, 1f, 3.2f, -1f, 1f));
            anim.speed = animatorInspector.GetCurrentSpeed(anim, ScaleBetween(time, 0.4f, 1f, -1f, 1f), ScaleBetween(time, 1f, 2.2f, -1f, 1f));
        }
        else if(C_SpeedTest)
        {
            if (C_SpeedConstant)
            {
                anim.speed = ScaleBetween(time, 0.4f, 3.2f, -1f, 1f);
            }
            else
            {
                anim.speed = animatorInspector.GetCurrentSpeed(anim, ScaleBetween(time, 0.4f, 1f, -1f, 1f), ScaleBetween(time, 1f, 3.2f, -1f, 1f));
            }
        }

        ikRatioArray_target = animatorInspector.GetCurrentIKRatioArray(anim);

        for(int i = 0; i < ikRatioArray.Length; i++)
        {
            ikRatioArray[i] = ikRatioArray[i] + (ikRatioArray_target[i] - ikRatioArray[i]) * Time.deltaTime;
        }
  
        /*
         * TALK ANIMATIONS
         */

        if (talkFlag && !preTalkFlag)
        {
            anim.SetInteger("AnimationNo", 1);
        }

        if (!talkFlag && preTalkFlag)
        {
            anim.SetInteger("AnimationNo", 0);
        }

        preTalkFlag = talkFlag;
        
        /*
         * UPDATE ANIMATION
         */

        // LookPass();

        GetBodyTransforms();

        AdjustIKTargets();

        if (C_LabanIK)
        {
            IKFactorUpdate();

            anim.SetIKPosition(AvatarIKGoal.LeftHand, LeftHandIK.transform.position);
            anim.SetIKPosition(AvatarIKGoal.RightHand, RightHandIK.transform.position);
            anim.SetIKPosition(AvatarIKGoal.LeftFoot, LeftFootIK.transform.position);
            anim.SetIKPosition(AvatarIKGoal.RightFoot, RightFootIK.transform.position);

            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, IK_MAIN_FACTOR_CURRENT);
            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, IK_MAIN_FACTOR_CURRENT);
            anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, IK_MAIN_FACTOR_CURRENT);
            anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, IK_MAIN_FACTOR_CURRENT);

            

        }
        else if (C_IKTest)
        {
            anim.SetIKPosition(AvatarIKGoal.LeftHand, LeftHandIK.transform.position);
            anim.SetIKPosition(AvatarIKGoal.RightHand, RightHandIK.transform.position);
            anim.SetIKPosition(AvatarIKGoal.LeftFoot, LeftFootIK.transform.position);
            anim.SetIKPosition(AvatarIKGoal.RightFoot, RightFootIK.transform.position);

            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, IK_MAIN_FACTOR_CURRENT);
            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, IK_MAIN_FACTOR_CURRENT);
            anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, IK_MAIN_FACTOR_CURRENT);
            anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, IK_MAIN_FACTOR_CURRENT);
        }

        /*
        if (lookObject != null)
        {
            HeadLookIK.transform.position = Vector3.Slerp(HeadLookIK.transform.position, lookObject.transform.position, Time.smoothDeltaTime);
        }
        else
        {
            lookObject = Camera.current.gameObject;
        }
        */

        if (C_LookIK)
        {
            anim.SetLookAtPosition(lookObject.transform.position); // HeadLookIK.transform.position);
            anim.SetLookAtWeight(1f, 0f, 0.15f, 0.45f);
        }
    }

    private float[] AnimSpecificIK_Case0 = { 1f, 0.8f, 0.8f };
    private float[] AnimSpecificIK_Case1 = { 0.3f, 0.24f, 0.15f };
    private float[] AnimSpecificIK_Case2 = { 1f, 0.6f, 0.9f };
    private float[] AnimSpecificIK_Case3 = { 0.3f, 0.4f, 0.2f };
    private float[] AnimSpecificIK_Case4 = { 1f, 0.5f, 0.3f };
    private float[] AnimSpecificIK_Case5 = { 0.2f, 0.12f, 0.07f };
    private float[] AnimSpecificIK_Case6 = { 1f, 0.8f, 0.5f };
    private float[] AnimSpecificIK_Case7 = { 0.3f, 0.2f, 0.24f };
    private float[] AnimSpecificIK_Case8 = { 0.4f, 0.24f, 0.08f };
    private float[] AnimSpecificIK_Case9 = { 0.2f, 0.1f, 0.09f };

    private float[] AnimSpecificIK_Current = null;

    private int oldAnimationNo = -1;

    private void IKFactorUpdate()
    {
        if (AnimSpecificIK_Current == null) return;

        AnimationNo = anim.GetInteger("CurrentAnimationStateNo_AnimatorBased");

        if (AnimationNo == 0)
        {
            IK_MAIN_FACTOR_TARGET = AnimSpecificIK_Current[0];
        }
        else if (AnimationNo > 0 && AnimationNo <= 3)
        {
            IK_MAIN_FACTOR_TARGET = AnimSpecificIK_Current[AnimationNo - 1];
        }

        if (AnimationNoTransition)
        {
            TransitionFactor += Time.deltaTime * 3f;
            if(TransitionFactor >= 1f)
            {
                TransitionFactor = 1f;
                AnimationNoTransition = false;
            }
        }
        else
        {
            if (oldAnimationNo != AnimationNo)
            {
                // start transition
                IK_MAIN_FACTOR_BASE = IK_MAIN_FACTOR_CURRENT;

                AnimationNoTransition = true;
                TransitionFactor = 0f;

                oldAnimationNo = AnimationNo;
            }
        }

        IK_MAIN_FACTOR_CURRENT = Mathf.Lerp(IK_MAIN_FACTOR_BASE, IK_MAIN_FACTOR_TARGET, TransitionFactor);
    }

    public void SetPersonality(OCEAN personality)
    {
        switch (personality)
        {
            case OCEAN.O_pos:
                openness = 1f;
                conscientiousness = 0f;
                extraversion = 0f;
                agreeableness = 0f;
                neuroticism = 0f;
                AnimSpecificIK_Current = AnimSpecificIK_Case0;
                break;
            case OCEAN.O_neg:
                openness = -1f;
                conscientiousness = 0f;
                extraversion = 0f;
                agreeableness = 0f;
                neuroticism = 0f;
                AnimSpecificIK_Current = AnimSpecificIK_Case1;
                break;
            case OCEAN.C_pos:
                openness = 0f;
                conscientiousness = 1f;
                extraversion = 0f;
                agreeableness = 0f;
                neuroticism = 0f;
                AnimSpecificIK_Current = AnimSpecificIK_Case2;
                break;
            case OCEAN.C_neg:
                openness = 0f;
                conscientiousness = -1f;
                extraversion = 0f;
                agreeableness = 0f;
                neuroticism = 0f;
                AnimSpecificIK_Current = AnimSpecificIK_Case3;
                break;
            case OCEAN.E_pos:
                openness = 0f;
                conscientiousness = 0f;
                extraversion = 1f;
                agreeableness = 0f;
                neuroticism = 0f;
                AnimSpecificIK_Current = AnimSpecificIK_Case4;
                break;
            case OCEAN.E_neg:
                openness = 0f;
                conscientiousness = 0f;
                extraversion = -1f;
                agreeableness = 0f;
                neuroticism = 0f;
                AnimSpecificIK_Current = AnimSpecificIK_Case5;
                break;
            case OCEAN.A_pos:
                openness = 0f;
                conscientiousness = 0f;
                extraversion = 0f;
                agreeableness = 1f;
                neuroticism = 0f;
                AnimSpecificIK_Current = AnimSpecificIK_Case6;
                break;
            case OCEAN.A_neg:
                openness = 0f;
                conscientiousness = 0f;
                extraversion = 0f;
                agreeableness = -1f;
                neuroticism = 0f;
                AnimSpecificIK_Current = AnimSpecificIK_Case7;
                break;
            case OCEAN.N_pos:
                openness = 0f;
                conscientiousness = 0f;
                extraversion = 0f;
                agreeableness = 0f;
                neuroticism = 1f;
                AnimSpecificIK_Current = AnimSpecificIK_Case8;
                break;
            case OCEAN.N_neg:
                openness = 0f;
                conscientiousness = 0f;
                extraversion = 0f;
                agreeableness = 0f;
                neuroticism = -1f;
                AnimSpecificIK_Current = AnimSpecificIK_Case9;
                break;
        }
    }

    #region TRANSFORMS GET SET
    // arms
    private Transform t_LeftShoulder;
    private Transform t_RightShoulder;
    private Transform t_LeftUpperArm;
    private Transform t_RightUpperArm;
    private Transform t_LeftLowerArm;
    private Transform t_RightLowerArm;
    private Transform t_LeftHand;
    private Transform t_RightHand;

    // legs
    private Transform t_LeftUpperLeg;
    private Transform t_RightUpperLeg;
    private Transform t_LeftLowerLeg;
    private Transform t_RightLowerLeg;
    private Transform t_LeftFoot;
    private Transform t_RightFoot;
    private Transform t_LeftToes;
    private Transform t_RightToes;

    // body
    private Transform t_Spine;
    private Transform t_Chest;
    private Transform t_UpperChest;
    private Transform t_Neck;
    private Transform t_Head;
    private Transform t_Hips;

    // fingers
    private Transform t_LeftIndexDistal;
    private Transform t_LeftIndexIntermediate;
    private Transform t_LeftIndexProximal;
    private Transform t_LeftMiddleDistal;
    private Transform t_LeftMiddleIntermediate;
    private Transform t_LeftMiddleProximal;
    private Transform t_LeftRingDistal;
    private Transform t_LeftRingIntermediate;
    private Transform t_LeftRingProximal;
    private Transform t_LeftThumbDistal;
    private Transform t_LeftThumbIntermediate;
    private Transform t_LeftThumbProximal;
    private Transform t_LeftLittleDistal;
    private Transform t_LeftLittleIntermediate;
    private Transform t_LeftLittleProximal;

    private Transform t_RightIndexDistal;
    private Transform t_RightIndexIntermediate;
    private Transform t_RightIndexProximal;
    private Transform t_RightMiddleDistal;
    private Transform t_RightMiddleIntermediate;
    private Transform t_RightMiddleProximal;
    private Transform t_RightRingDistal;
    private Transform t_RightRingIntermediate;
    private Transform t_RightRingProximal;
    private Transform t_RightThumbDistal;
    private Transform t_RightThumbIntermediate;
    private Transform t_RightThumbProximal;
    private Transform t_RightLittleDistal;
    private Transform t_RightLittleIntermediate;
    private Transform t_RightLittleProximal;

    private void GetBodyTransforms()
    {
        t_LeftShoulder = anim.GetBoneTransform(HumanBodyBones.LeftShoulder);
        t_RightShoulder = anim.GetBoneTransform(HumanBodyBones.RightShoulder);
        t_LeftUpperArm = anim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        t_RightUpperArm = anim.GetBoneTransform(HumanBodyBones.RightUpperArm);
        t_LeftLowerArm = anim.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        t_RightLowerArm = anim.GetBoneTransform(HumanBodyBones.RightLowerArm);
        t_LeftHand = anim.GetBoneTransform(HumanBodyBones.LeftHand);
        t_RightHand = anim.GetBoneTransform(HumanBodyBones.RightHand);

        t_LeftUpperLeg = anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        t_RightUpperLeg = anim.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        t_LeftLowerLeg = anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        t_RightLowerLeg = anim.GetBoneTransform(HumanBodyBones.RightLowerLeg);
        t_LeftFoot = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
        t_RightFoot = anim.GetBoneTransform(HumanBodyBones.RightFoot);
        t_LeftToes = anim.GetBoneTransform(HumanBodyBones.LeftToes);
        t_RightToes = anim.GetBoneTransform(HumanBodyBones.RightToes);

        t_RightFoot = anim.GetBoneTransform(HumanBodyBones.RightFoot);
        t_RightFoot = anim.GetBoneTransform(HumanBodyBones.RightFoot);

        t_Spine = anim.GetBoneTransform(HumanBodyBones.Spine);
        t_Chest = anim.GetBoneTransform(HumanBodyBones.Chest);
        t_UpperChest = anim.GetBoneTransform(HumanBodyBones.UpperChest);
        t_Neck = anim.GetBoneTransform(HumanBodyBones.Neck);
        t_Head = anim.GetBoneTransform(HumanBodyBones.Head);

        t_Hips = anim.GetBoneTransform(HumanBodyBones.Hips);

        t_LeftIndexDistal = anim.GetBoneTransform(HumanBodyBones.LeftIndexDistal);
        t_LeftIndexIntermediate = anim.GetBoneTransform(HumanBodyBones.LeftIndexIntermediate);
        t_LeftIndexProximal = anim.GetBoneTransform(HumanBodyBones.LeftIndexProximal);
        t_LeftMiddleDistal = anim.GetBoneTransform(HumanBodyBones.LeftMiddleDistal);
        t_LeftMiddleIntermediate = anim.GetBoneTransform(HumanBodyBones.LeftMiddleIntermediate);
        t_LeftMiddleProximal = anim.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);
        t_LeftRingDistal = anim.GetBoneTransform(HumanBodyBones.LeftRingDistal);
        t_LeftRingIntermediate = anim.GetBoneTransform(HumanBodyBones.LeftRingIntermediate);
        t_LeftRingProximal = anim.GetBoneTransform(HumanBodyBones.LeftRingProximal);
        t_LeftThumbDistal = anim.GetBoneTransform(HumanBodyBones.LeftThumbDistal);
        t_LeftThumbIntermediate = anim.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate);
        t_LeftThumbProximal = anim.GetBoneTransform(HumanBodyBones.LeftThumbProximal);
        t_LeftLittleDistal = anim.GetBoneTransform(HumanBodyBones.LeftLittleDistal);
        t_LeftLittleIntermediate = anim.GetBoneTransform(HumanBodyBones.LeftLittleIntermediate);
        t_LeftLittleProximal = anim.GetBoneTransform(HumanBodyBones.LeftLittleProximal);

        t_RightIndexDistal = anim.GetBoneTransform(HumanBodyBones.RightIndexDistal);
        t_RightIndexIntermediate = anim.GetBoneTransform(HumanBodyBones.RightIndexIntermediate);
        t_RightIndexProximal = anim.GetBoneTransform(HumanBodyBones.RightIndexProximal);
        t_RightMiddleDistal = anim.GetBoneTransform(HumanBodyBones.RightMiddleDistal);
        t_RightMiddleIntermediate = anim.GetBoneTransform(HumanBodyBones.RightMiddleDistal);
        t_RightMiddleProximal = anim.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
        t_RightRingDistal = anim.GetBoneTransform(HumanBodyBones.RightRingDistal);
        t_RightRingIntermediate = anim.GetBoneTransform(HumanBodyBones.RightRingIntermediate);
        t_RightRingProximal = anim.GetBoneTransform(HumanBodyBones.RightRingProximal);
        t_RightThumbDistal = anim.GetBoneTransform(HumanBodyBones.RightThumbDistal);
        t_RightThumbIntermediate = anim.GetBoneTransform(HumanBodyBones.RightThumbIntermediate);
        t_RightThumbProximal = anim.GetBoneTransform(HumanBodyBones.RightThumbProximal);
        t_RightLittleDistal = anim.GetBoneTransform(HumanBodyBones.RightLittleDistal);
        t_RightLittleIntermediate = anim.GetBoneTransform(HumanBodyBones.RightLittleIntermediate);
        t_RightLittleProximal = anim.GetBoneTransform(HumanBodyBones.RightLittleProximal);
    }

    private void SetBodyTransforms()
    {
        anim.SetBoneLocalRotation(HumanBodyBones.LeftShoulder, t_LeftShoulder.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.RightShoulder, t_RightShoulder.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.LeftUpperArm, t_LeftUpperArm.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.RightUpperArm, t_RightUpperArm.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.LeftLowerArm, t_LeftLowerArm.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.RightLowerArm, t_RightLowerArm.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.LeftHand, t_LeftHand.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.RightHand, t_RightHand.localRotation);

        anim.SetBoneLocalRotation(HumanBodyBones.LeftUpperLeg, t_LeftUpperLeg.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.RightUpperLeg, t_RightUpperLeg.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.LeftLowerLeg, t_LeftLowerLeg.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.RightLowerLeg, t_RightLowerLeg.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.LeftFoot, t_LeftFoot.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.RightFoot, t_RightFoot.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.LeftToes, t_LeftToes.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.RightToes, t_RightToes.localRotation);

        anim.SetBoneLocalRotation(HumanBodyBones.Spine, t_Spine.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.Chest, t_Chest.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.UpperChest, t_UpperChest.localRotation);

        anim.SetBoneLocalRotation(HumanBodyBones.Neck, t_Neck.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.Head, t_Head.localRotation);
        // anim.SetBoneLocalRotation(HumanBodyBones.Hips, t_Hips.localRotation);

        anim.SetBoneLocalRotation(HumanBodyBones.LeftIndexDistal, t_LeftIndexDistal.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.LeftIndexIntermediate, t_LeftIndexIntermediate.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.LeftIndexProximal, t_LeftIndexProximal.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.LeftMiddleDistal, t_LeftMiddleDistal.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.LeftMiddleIntermediate, t_LeftMiddleIntermediate.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.LeftMiddleProximal, t_LeftMiddleProximal.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.LeftRingDistal, t_LeftRingDistal.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.LeftRingIntermediate, t_LeftRingIntermediate.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.LeftRingProximal, t_LeftRingProximal.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.LeftThumbDistal, t_LeftThumbDistal.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.LeftThumbIntermediate, t_LeftThumbIntermediate.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.LeftThumbProximal, t_LeftThumbProximal.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.LeftLittleDistal, t_LeftLittleDistal.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.LeftLittleIntermediate, t_LeftLittleIntermediate.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.LeftLittleProximal, t_LeftLittleProximal.localRotation);

        anim.SetBoneLocalRotation(HumanBodyBones.RightIndexDistal, t_RightIndexDistal.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.RightIndexIntermediate, t_RightIndexIntermediate.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.RightIndexProximal, t_RightIndexProximal.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.RightMiddleDistal, t_RightMiddleDistal.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.RightMiddleIntermediate, t_RightMiddleIntermediate.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.RightMiddleProximal, t_RightMiddleProximal.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.RightRingDistal, t_RightRingDistal.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.RightRingIntermediate, t_RightRingIntermediate.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.RightRingProximal, t_RightRingProximal.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.RightThumbDistal, t_RightThumbDistal.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.RightThumbIntermediate, t_RightThumbIntermediate.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.RightThumbProximal, t_RightThumbProximal.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.RightLittleDistal, t_RightLittleDistal.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.RightLittleIntermediate, t_RightLittleIntermediate.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.RightLittleProximal, t_RightLittleProximal.localRotation);
    }
    #endregion

    // PASSES

    private void AdditionalPass()
    {
        // spine bend
        //nrp_spine.x = ScaleBetween(spine_bend*0.35f, spine_min, spine_max, - 1f, 1f);
        // nrp_chest.x = ScaleBetween(spine_bend, spine_min, spine_max, -1f, 1f);
        nrp_upperChest.x = ScaleBetween(spine_bend, spine_min, spine_max, -1f, 1f);

        // sink bend
        sinkAngle = ScaleBetween(sink_bend, sink_min, sink_max, -1f, 1f);

        // head bend
        nrp_neck.x = ScaleBetween(head_bend, head_min, head_max, -1f, 1f);
        nrp_head.x = ScaleBetween(-head_bend*0.6f, head_min, head_max, -1f, 1f);

        // finger bend
        fingerRotationMax = ScaleBetween(finger_bend_close, finger_close_min, finger_close_max, -1f, 1f);
        fingerRotationMin = ScaleBetween(finger_bend_open, finger_open_min, finger_open_max, -1f, 1f);
    }

    // finger angles
    public float fingerRotationL;
    public float fingerRotationR;

    private float fingerRotationLTarget;
    private float fingerRotationRTarget;

    private float fingerRotationMin;
    private float fingerRotationMax;

    private float fingerChangeTimer;

    private void FingerPass()
    {
        // finger angles
        if (fingerChangeTimer <= 0f)
        {
            fingerChangeTimer = UnityEngine.Random.Range(1f, 5f);
            fingerRotationLTarget = UnityEngine.Random.Range(fingerRotationMin, fingerRotationMax);
            fingerRotationRTarget = UnityEngine.Random.Range(fingerRotationMin, fingerRotationMax);
        }

        fingerChangeTimer -= Time.deltaTime;
        fingerRotationL = (fingerRotationLTarget - fingerRotationL) * 0.01f + fingerRotationL;
        fingerRotationR = (fingerRotationRTarget - fingerRotationR) * 0.01f + fingerRotationR;

        Quaternion fingIndexL = Quaternion.Euler(0, 0, fingerRotationL);
        Quaternion fingIndexR = Quaternion.Euler(0, 0, fingerRotationR);
        Quaternion fingThumbL = Quaternion.Euler(-fingerRotationL * 0.4f, 0, 0);
        Quaternion fingThumbR = Quaternion.Euler(fingerRotationR * 0.4f, 0, 0);
        Quaternion fingRestL = Quaternion.Euler(0, 0, fingerRotationL);
        Quaternion fingRestR = Quaternion.Euler(0, 0, fingerRotationR);

        t_LeftIndexDistal.localRotation *= fingIndexL;
        t_LeftIndexIntermediate.localRotation *= fingIndexL;
        t_LeftIndexProximal.localRotation *= fingIndexL;
        t_LeftMiddleDistal.localRotation *= fingRestL;
        t_LeftMiddleIntermediate.localRotation *= fingRestL;
        t_LeftMiddleProximal.localRotation *= fingRestL;
        t_LeftRingDistal.localRotation *= fingRestL;
        t_LeftRingIntermediate.localRotation *= fingRestL;
        t_LeftRingProximal.localRotation *= fingRestL;
        t_LeftThumbDistal.localRotation *= fingThumbL;
        t_LeftThumbIntermediate.localRotation *= fingThumbL;
        t_LeftThumbProximal.localRotation *= fingThumbL;
        t_LeftLittleDistal.localRotation *= fingRestL;
        t_LeftLittleIntermediate.localRotation *= fingRestL;
        t_LeftLittleProximal.localRotation *= fingRestL;

        t_RightIndexDistal.localRotation *= fingIndexR;
        t_RightIndexIntermediate.localRotation *= fingIndexR;
        t_RightIndexProximal.localRotation *= fingIndexR;
        t_RightMiddleDistal.localRotation *= fingRestR;
        t_RightMiddleIntermediate.localRotation *= fingRestR;
        t_RightMiddleProximal.localRotation *= fingRestR;
        t_RightRingDistal.localRotation *= fingRestR;
        t_RightRingIntermediate.localRotation *= fingRestR;
        t_RightRingProximal.localRotation *= fingRestR;
        t_RightThumbDistal.localRotation *= fingThumbR;
        t_RightThumbIntermediate.localRotation *= fingThumbR;
        t_RightThumbProximal.localRotation *= fingThumbR;
        t_RightLittleDistal.localRotation *= fingRestR;
        t_RightLittleIntermediate.localRotation *= fingRestR;
        t_RightLittleProximal.localRotation *= fingRestR;
    }

    #region FLUCTUATE
    private CircularNoise circularNoise;
    private Quaternion tmpQ;

    private float fluctuateAngle;
    private float fluctuateAngle_pre;
    private readonly int fluctuate_numOfNRandom = 23;

    private float fluctuateSpeed;
    private float fluctuateSpeed_pre;

    private void FluctuatePassInit()
    {
        circularNoise = new CircularNoise(fluctuate_numOfNRandom, 0.02f);
        circularNoise.SetScalingFactorRange(0, 18, -fluctuateAngle, fluctuateAngle);
        tempF = fluctuateAngle * 0.25f;
        circularNoise.SetScalingFactorRange(18, 21, -tempF, tempF);
    }

    float tempF;

    private void FluctuatePass()
    {
        if (fluctuateAngle != fluctuateAngle_pre)
        {
            circularNoise.SetScalingFactorRange(0, 18, -fluctuateAngle, fluctuateAngle);
            tempF = fluctuateAngle * 0.25f;
            circularNoise.SetScalingFactorRange(18, 21, -tempF, tempF);
            fluctuateAngle_pre = fluctuateAngle;
        }

        if(fluctuateSpeed != fluctuateSpeed_pre)
        {
            circularNoise.SetDeltaAngleRange(0, 21, fluctuateSpeed);
            fluctuateSpeed_pre = fluctuateSpeed;
        }

        circularNoise.Tick();

        // quaternion math
        t_LeftUpperArm.localRotation *= Quaternion.Euler(circularNoise.values[0], circularNoise.values[1], circularNoise.values[2]);
        t_RightUpperArm.localRotation *= Quaternion.Euler(circularNoise.values[3], circularNoise.values[4], circularNoise.values[5]);
        t_LeftLowerArm.localRotation *= Quaternion.Euler(circularNoise.values[6], circularNoise.values[7], circularNoise.values[8]);
        t_RightLowerArm.localRotation *= Quaternion.Euler(circularNoise.values[9], circularNoise.values[10], circularNoise.values[11]);
        t_LeftHand.localRotation *= Quaternion.Euler(circularNoise.values[12], circularNoise.values[13], circularNoise.values[14]);
        t_RightHand.localRotation *= Quaternion.Euler(circularNoise.values[15], circularNoise.values[16], circularNoise.values[17]);
        t_Spine.localRotation *= Quaternion.Euler(circularNoise.values[18], circularNoise.values[19], circularNoise.values[20]);
    }
    #endregion
    
    #region SINK
    private float sinkAngle;
    private Vector3 bodyOriginal;
    private Vector3 body_legDisplacement;
    private bool sinkFirstPass;
    private float body_legDistance;
    private float body_legDistance_original_l;
    private float body_legDistance_original_r;
    private float body_legDistance_original_avg;

    private void SinkPassInit()
    {
        GetBodyTransforms();
        sinkFirstPass = true;
        body_legDistance_original_l = (t_LeftUpperLeg.position.y - t_LeftToes.position.y);
        body_legDistance_original_r = (t_RightUpperLeg.position.y - t_RightToes.position.y);
        body_legDistance_original_avg = (body_legDistance_original_l + body_legDistance_original_r) / 2;
    }

    private void SinkPass()
    {
        if (sinkFirstPass)
        {
            bodyOriginal = anim.bodyPosition;
            sinkFirstPass = false;
            return;
        }

        Quaternion q = Quaternion.Euler(new Vector3(sinkAngle / 2, 0, 0));
        Quaternion qt = Quaternion.Euler(new Vector3(-sinkAngle, 0, 0));

        t_LeftUpperLeg.localRotation *= q;
        t_RightUpperLeg.localRotation *= q;
        t_LeftLowerLeg.localRotation *= qt;
        t_RightLowerLeg.localRotation *= qt;
        t_LeftFoot.localRotation *= q;
        t_RightFoot.localRotation *= q;

        body_legDistance = ((t_LeftUpperLeg.position.y - t_LeftToes.position.y) + (t_RightUpperLeg.position.y - t_RightToes.position.y)) / 2;
        body_legDisplacement.y = body_legDistance_original_avg - body_legDistance; // body_legDistance * Mathf.Sin(Mathf.Deg2Rad * sinkAngle);

        anim.bodyPosition = anim.bodyPosition - body_legDisplacement;
    }
    #endregion

    private Vector3 bodyOriginal_old;

    public void ShiftBodyOriginalVec3(Vector3 v)
    {
        bodyOriginal_old = bodyOriginal;
        bodyOriginal += new Vector3(v.x, v.y, v.z);
    }

    public void ResetBodyOriginalVec3()
    {
        bodyOriginal = bodyOriginal_old;
    }

    private void LateUpdate()
    {
        t_Head = anim.GetBoneTransform(HumanBodyBones.Head);
        t_Neck = anim.GetBoneTransform(HumanBodyBones.Neck);

        GetBodyTransforms();

        if (C_LabanRotation)
        {
            t_Head.localRotation *= Quaternion.Euler(nrp_head.x, 0f, 0f);
            t_Neck.localRotation *= Quaternion.Euler(nrp_neck.x, 0f, 0f);
        }

        LabanEffort_to_Rotations();
        AdditionalPass();

        if (C_LabanRotation)
        {
            NewRotatePass();
            SinkPass();
        }

        if (C_Fluctuation) FluctuatePass();
        if (C_Fingers) FingerPass();

        if (C_LookShift)
        {
            circularNoise.SetScalingFactor(21, -ls_ver, ls_ver);
            circularNoise.SetScalingFactor(22, -ls_hor, ls_hor);
            circularNoise.SetDeltaAngle(21, ls_ver_speed);
            circularNoise.SetDeltaAngle(22, ls_hor_speed);
            t_Neck.localRotation *= Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(circularNoise.values[21], circularNoise.values[22], 0), multiplyRotationFactor);
        }

        anim.SetBoneLocalRotation(HumanBodyBones.Head, t_Head.localRotation);
        anim.SetBoneLocalRotation(HumanBodyBones.Neck, t_Neck.localRotation);

        SetBodyTransforms();

        
    }

    #region NEW ROTATE PASS
    private Vector3 nrp_spine;
    private Vector3 nrp_chest;
    private Vector3 nrp_upperChest;
    private Vector3 nrp_neck;
    private Vector3 nrp_head;

    private Vector3 nrp_shoulder;
    private Vector3 nrp_upperArm;
    private Vector3 nrp_lowerArm;
    private Vector3 nrp_hand;

    private Vector3 nrp_upperLeg;
    private Vector3 nrp_lowerLeg;
    private Vector3 nrp_foot;

    private Vector3 nrp_shoulder_x;
    private Vector3 nrp_upperArm_x;
    private Vector3 nrp_lowerArm_x;
    private Vector3 nrp_hand_x;

    private Vector3 nrp_upperLeg_x;
    private Vector3 nrp_lowerLeg_x;
    private Vector3 nrp_foot_x;

    private void NewRotatePass()
    {
        t_Spine.localRotation *= Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(nrp_spine), multiplyRotationFactor);
        t_Chest.localRotation *= Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(nrp_chest), multiplyRotationFactor);
        t_UpperChest.localRotation *= Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(nrp_upperChest), multiplyRotationFactor);
        t_Neck.localRotation *= Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(nrp_neck), multiplyRotationFactor);
        t_Head.localRotation *= Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(nrp_head), multiplyRotationFactor);

        nrp_shoulder_x = nrp_shoulder;
        nrp_shoulder_x.y = -nrp_shoulder_x.y;
        nrp_shoulder_x.z = -nrp_shoulder_x.z;

        nrp_upperArm_x = nrp_upperArm;
        nrp_upperArm_x.y = -nrp_upperArm_x.y;
        nrp_upperArm_x.z = -nrp_upperArm_x.z;

        nrp_lowerArm_x = nrp_lowerArm;
        nrp_lowerArm_x.y = -nrp_lowerArm_x.y;
        nrp_lowerArm_x.z = -nrp_lowerArm_x.z;

        nrp_hand_x = nrp_hand;
        nrp_hand_x.y = -nrp_hand_x.y;
        nrp_hand_x.z = -nrp_hand_x.z;

        t_LeftShoulder.localRotation *= Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(nrp_shoulder), multiplyRotationFactor);
        t_RightShoulder.localRotation *= Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(nrp_shoulder_x), multiplyRotationFactor);
        t_LeftUpperArm.localRotation *= Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(nrp_upperArm), multiplyRotationFactor);
        t_RightUpperArm.localRotation *= Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(nrp_upperArm_x), multiplyRotationFactor);
        t_LeftLowerArm.localRotation *= Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(nrp_lowerArm), multiplyRotationFactor);
        t_RightLowerArm.localRotation *= Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(nrp_lowerArm_x), multiplyRotationFactor);
        t_LeftHand.localRotation *= Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(nrp_hand), multiplyRotationFactor);
        t_RightHand.localRotation *= Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(nrp_hand_x), multiplyRotationFactor);

        nrp_upperLeg_x = nrp_upperLeg;
        nrp_upperLeg_x.y = -nrp_upperLeg_x.y;
        nrp_upperLeg_x.z = -nrp_upperLeg_x.z;

        nrp_lowerLeg_x = nrp_lowerLeg;
        nrp_lowerLeg_x.y = -nrp_lowerLeg_x.y;
        nrp_lowerLeg_x.z = -nrp_lowerLeg_x.z;

        nrp_foot_x = nrp_foot;
        nrp_foot_x.y = -nrp_foot_x.y;
        nrp_foot_x.z = -nrp_foot_x.z;

        t_LeftUpperLeg.localRotation *= Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(nrp_upperLeg), multiplyRotationFactor);
        t_RightUpperLeg.localRotation *= Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(nrp_upperLeg_x), multiplyRotationFactor);
        t_LeftLowerLeg.localRotation *= Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(nrp_lowerLeg), multiplyRotationFactor);
        t_RightLowerLeg.localRotation *= Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(nrp_lowerLeg_x), multiplyRotationFactor);
        t_LeftFoot.localRotation *= Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(nrp_foot), multiplyRotationFactor);
        t_RightFoot.localRotation *= Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(nrp_foot_x), multiplyRotationFactor);
    }
    #endregion

    /* * *
    * 
    * EMOTION PASS
    * 
    * * */

    public void AddBaseToExp()
    {
        e_angry += base_angry;
        e_disgust += base_disgust;
        e_fear += base_fear;
        e_happy += base_happy;
        e_sad += base_sad;
        e_shock += base_shock;
    }

    public void ClampEmotions()
    {
        e_angry = Mathf.Clamp(e_angry, 0, 1);
        e_disgust = Mathf.Clamp(e_disgust, 0, 1);
        e_fear = Mathf.Clamp(e_fear, 0, 1);
        e_happy = Mathf.Clamp(e_happy, 0, 1);
        e_sad = Mathf.Clamp(e_sad, 0, 1);
        e_shock = Mathf.Clamp(e_shock, 0, 1);
    }

    private float emotionDecayFactor = 0.06f; // not scaled to OCEAN for now
    private float tmpDecayValue;

    /* * *
    * 
    * MAP FUNCTIONS
    * 
    * * */

    private readonly bool map_new_weights = false;
    private readonly bool map_emotion_decay = false;
    private readonly bool map_express_factor = false;

    public void OCEAN_to_LabanEffort()
    {
        if(map_new_weights)
        {
            space = ScaleBetween(extraversion + openness, -1, 1, -2, 2);
            weight = ScaleBetween(openness + extraversion + agreeableness, -1, 1, -3, 3);
            time = ScaleBetween(extraversion + neuroticism - (conscientiousness * 1.5f), -1, 1, -3.5f, 3.5f);
            flow = ScaleBetween((neuroticism * 2) - conscientiousness + openness, -1, 1, -4, 4);
        }
        else
        {
            space = ScaleBetween(extraversion + openness, -1, 1, -1, 1);
            weight = ScaleBetween(openness + extraversion + agreeableness, -1, 1, -1, 1);
            time = ScaleBetween(extraversion + neuroticism - (conscientiousness * 1.5f), -1, 1, -1.5f, 1.5f);
            //flow = ScaleBetween((neuroticism) - conscientiousness + openness, -1, 1, -1, 1);
            flow = ScaleBetween((neuroticism * 2) - conscientiousness + openness, -1, 1, -2, 2);
        }
    }

    public void OCEAN_to_LabanShape()
    {
        if(map_new_weights)
        {
            IKFAC_up = ScaleBetween(extraversion + openness + agreeableness + conscientiousness, -1, 1, -4, 4);
            IKFAC_side = ScaleBetween(extraversion * 1.5f + openness, -1, 1, -2.5f, 2.5f);
            IKFAC_forward = ScaleBetween(neuroticism + extraversion, -1, 1, -4f, 4f);

        }
        else
        {
            IKFAC_up = ScaleBetween(extraversion + openness + agreeableness + conscientiousness, -1, 1, -1, 1);
            IKFAC_side = ScaleBetween(extraversion * 1.5f + openness, -1, 1, -1.5f, 1.5f);
            IKFAC_forward = ScaleBetween(neuroticism + extraversion, -1, 1, -1, 1);
        }
    }

    public void OCEAN_to_Additional()
    {
        if(map_new_weights)
        {
            spine_bend = ScaleBetween(-0.5f * agreeableness - extraversion * .8f, -1, 1, -1.5f, 1.5f) * le_lsq_fac;
            head_bend = ScaleBetween(-0.5f * openness - 0.5f * agreeableness - 0.5f * conscientiousness - extraversion * .8f, -1, 1, -2.5f, 2.5f) * le_lsq_fac;
            sink_bend = ScaleBetween(-0.5f * conscientiousness - 0.5f * extraversion * .8f - openness, -1, 1, -2f, 2f) * le_lsq_fac;
            finger_bend_open = ScaleBetween(-0.5f * openness - agreeableness, -1, 1, -1.5f, 1.5f) * le_lsq_fac;
            finger_bend_close = ScaleBetween(-openness - agreeableness + neuroticism, -1, 1, -3f, 3f) * le_lsq_fac;

            ls_hor = ScaleBetween(extraversion - conscientiousness, 0f, 20f, -2, 2) * le_lsq_fac;
            ls_ver = ScaleBetween(extraversion - conscientiousness, 0f, 5f, -2, 2) * le_lsq_fac;
            ls_hor_speed = ScaleBetween(neuroticism, 0.2f, 4f, -1, 1) * le_lsq_fac;
            ls_ver_speed = ScaleBetween(neuroticism, 0.2f, 2f, -1, 1) * le_lsq_fac;

            fluctuateSpeed = ScaleBetween(neuroticism, 0f, 10f, -1, 1) * 4;
        }
        else
        {
            spine_bend = ScaleBetween(-agreeableness * 0.5f - extraversion * .6f, -1, 1, -1f, 1f);
            head_bend = ScaleBetween(openness - agreeableness * 0.5f - conscientiousness - extraversion * .5f, -1, 1, -1, 1);
            sink_bend = ScaleBetween(conscientiousness - extraversion * .7f - openness, -1, 1, -1f, 1f);
            finger_bend_open = ScaleBetween(openness - agreeableness, -1, 1, -1, 1);
            finger_bend_close = ScaleBetween(-openness - agreeableness + neuroticism, -1, 1, -1, 1);

            ls_hor = ScaleBetween(extraversion - conscientiousness, 0f, 20f, -1, 1);
            ls_ver = ScaleBetween(extraversion - conscientiousness, 0f, 5f, -1, 1);
            ls_hor_speed = ScaleBetween(neuroticism, 0.2f, 4f, -1, 1);
            ls_ver_speed = ScaleBetween(neuroticism, 0.2f, 2f, -1, 1);

            fluctuateSpeed = ScaleBetween(neuroticism, 0f, 10f, -1, 1);
        }
    }

    private readonly bool map_effort_instead_direct_OCEAN = true;
    private readonly float le_lsq_fac = 1.2f;

    public void LabanEffort_to_Rotations()
    {
        // fluctuate ocean
        // fluctuateAngle = ScaleBetween(flow, 0, 18, -1, 1);
        fluctuateAngle = ScaleBetween(flow, 0, 8, -1, 1);

        if (map_effort_instead_direct_OCEAN)
        {
            // body legs sink & rotate ocean
            nrp_upperLeg.y = ScaleBetween(space, -8, 6, -1f, 1f) * le_lsq_fac;
            nrp_upperLeg.z = ScaleBetween(space, 4, -2, -1f, 1f) * le_lsq_fac;
            nrp_lowerLeg.y = ScaleBetween(space, -8, 4, -1f, 1f) * le_lsq_fac;
            nrp_lowerLeg.z = ScaleBetween(space, 4, -1, -1f, 1f) * le_lsq_fac;
            nrp_foot.y = ScaleBetween(space, 0, 2, -1f, 1f) * le_lsq_fac;

            // rotate ocean
            nrp_shoulder.x = ScaleBetween(space, 1, -3, -1f, 1f) * le_lsq_fac;
            nrp_shoulder.y = ScaleBetween(-weight, 5, 0, -1f, 1f) * le_lsq_fac;
            nrp_shoulder.z = ScaleBetween(-weight, 0, -3, -1f, 1f) * le_lsq_fac;
            nrp_upperArm.x = ScaleBetween(-weight, 1, -2, -1f, 1f) * le_lsq_fac;
            nrp_lowerArm.x = ScaleBetween(-weight, 1, 0, -1f, 1f) * le_lsq_fac;

            nrp_lowerArm.y = ScaleBetween(space, -10, 10, -1f, 1f) * le_lsq_fac;
            nrp_lowerArm.z = ScaleBetween(space, 0, -4, -1f, 1f) * le_lsq_fac;
            nrp_hand.x = ScaleBetween(space, 14, -10, -1f, 1f) * le_lsq_fac;
            nrp_hand.y = ScaleBetween(space, -10, 28, -1f, 1f) * le_lsq_fac;
            nrp_hand.z = ScaleBetween(space, 0, -6, -1f, 1f) * le_lsq_fac;
        }
        else
        {
            // body legs sink & rotate ocean
            nrp_upperLeg.y = ScaleBetween(openness, -8f, 6f, -1, 1);
            nrp_upperLeg.z = ScaleBetween(openness, 4f, -2f, -1, 1);
            nrp_lowerLeg.y = ScaleBetween(openness, -8f, 4f, -1, 1);
            nrp_lowerLeg.z = ScaleBetween(openness, 4f, -1f, -1, 1);
            nrp_foot.y = ScaleBetween(openness, 0f, 2f, -1, 1);

            // rotate ocean
            nrp_shoulder.x = ScaleBetween(extraversion, 1, -3, -1, 1);
            nrp_shoulder.y = ScaleBetween(extraversion, 5, 0, -1, 1);
            nrp_shoulder.z = ScaleBetween(extraversion, 0, -3, -1, 1);
            nrp_upperArm.x = ScaleBetween(extraversion, 1, -2, -1, 1);
            nrp_lowerArm.x = ScaleBetween(extraversion, 1, 0, -1, 1);

            nrp_lowerArm.y = ScaleBetween(openness, -10, 10, -1, 1);
            nrp_lowerArm.z = ScaleBetween(openness, 0, -4, -1, 1);
            nrp_hand.x = ScaleBetween(openness, 10, -8, -1, 1) + ScaleBetween(agreeableness, 4, -2, -1, 1);
            nrp_hand.y = ScaleBetween(openness, -10, 20, -1, 1) + ScaleBetween(agreeableness, 0, 8, -1, 1);
            nrp_hand.z = ScaleBetween(openness, 0, -6, -1, 1);   
        }
    }

    int markT = 0;
    public void WaweTestTick()
    {
        markT++;

        if(markT == 1)
        {
            IKFAC_side = 1;
            PutMarks = true;
        }
        if (markT == 2)
        {
            PutMarks = false;
        }
        if (markT == 3)
        {
            C_LabanIK = true;
            PutMarks = true;
        }
        if (markT == 4)
        {
            PutMarks = false;
        }
        if (markT == 5)
        {
            PutMarks = true;
            IKWeightByPass = true;
        }
        if (markT == 6)
        {
            PutMarks = false;
            IKWeightByPass = false;

        }
    }

    public void SetAnimation(int i)
    {
        if(anim != null)
        {
            Debug.Log("Agent anim set to " + i);
            anim.SetInteger("AnimationNo", i);
        }
    }

    public void SetAnimationParameter(string parameterName, int i)
    {
        if (anim != null)
        {
            anim.SetInteger(parameterName, i);
        }
    }

    public int GetAnimationNo()
    {
        if (anim != null)
        {
            return anim.GetInteger("AnimationNo");
        }
        else
        {
            return 0;
        }
    }

    public void DeltaHandsToLines()
    {
        animatorInspector.DeltaHandsToLines(anim);
    }

    public void RemoveDeltaHandsToLines()
    {
        animatorInspector.RemoveDeltaHandsToLines();
    }

    public Transform GetHeadPosition()
    {
        return t_Head;
    }

    public Animator GetAnimator()
    {
        return anim;
    }
 
    private static float ScaleBetween(float oldvalue, float newmin, float newmax, float oldmin, float oldmax)
    {
        float d = oldmax - oldmin;
        if (d == 0) return 0;
        else return (newmax - newmin) * (oldvalue - oldmin) / d + newmin;
    }

}
