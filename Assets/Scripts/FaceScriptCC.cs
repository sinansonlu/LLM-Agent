using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class FaceScriptCC : MonoBehaviour
{
    public float[] visemes;
    public float[] visemes_current;
    
    private float speed = 5;

    private string[] shapeKeys;
    private int[] shapeValues;
    
    private Transform jawBone;
    private Quaternion jawBoneRotation;
    private bool hasJawBone;
    private float jawAngleInc = 0f;


    private SkinnedMeshRenderer meshRenderer_body;
    private SkinnedMeshRenderer meshRenderer_eye;
    private SkinnedMeshRenderer meshRenderer_eye_occlusion;
    private SkinnedMeshRenderer meshRenderer_tear_line;
    private SkinnedMeshRenderer meshRenderer_teeth;
    private SkinnedMeshRenderer meshRenderer_tongue;
    private SkinnedMeshRenderer meshRenderer_brow;

    private Dictionary<string, int> shapeKeyDict_body;
    private Dictionary<string, int> shapeKeyDict_eye;
    private Dictionary<string, int> shapeKeyDict_eye_occlusion;
    private Dictionary<string, int> shapeKeyDict_tear_line;
    private Dictionary<string, int> shapeKeyDict_teeth;
    private Dictionary<string, int> shapeKeyDict_tongue;
    private Dictionary<string, int> shapeKeyDict_brow;

    private string[] gameObjectNames_body = { "CC_Base_Body" };
    private string[] gameObjectNames_eye = { "CC_Base_Eye" };
    private string[] gameObjectNames_eye_occlusion = { "CC_Base_EyeOcclusion" };
    private string[] gameObjectNames_tear_line = { "CC_Base_TearLine" };
    //private string[] gameObjectNames_teeth = { "CC_Base_Teeth" };
    private string[] gameObjectNames_tongue = { "CC_Base_Tongue" };
    //private string[] gameObjectNames_brow = { "Female_Brow_Extracted0" };

    private void TrySetSkinnedMeshRenderer(ref SkinnedMeshRenderer skinnedMeshRenderer, ref string[] names)
    {
        foreach (string name in names)
        {
            skinnedMeshRenderer = GetChildGameObject(gameObject, name).GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
            {
                return;
            }
        }
    }

    private void GetSkinnedMeshRenderers()
    {
        TrySetSkinnedMeshRenderer(ref meshRenderer_body, ref gameObjectNames_body);
        TrySetSkinnedMeshRenderer(ref meshRenderer_eye, ref gameObjectNames_eye);
        TrySetSkinnedMeshRenderer(ref meshRenderer_eye_occlusion, ref gameObjectNames_eye_occlusion);
        TrySetSkinnedMeshRenderer(ref meshRenderer_tear_line, ref gameObjectNames_tear_line);
        //TrySetSkinnedMeshRenderer(ref meshRenderer_teeth, ref gameObjectNames_teeth);
        TrySetSkinnedMeshRenderer(ref meshRenderer_tongue, ref gameObjectNames_tongue);
        //TrySetSkinnedMeshRenderer(ref meshRenderer_brow, ref gameObjectNames_brow);
    }

    private void MakeShapeKeyDictionaries()
    {
        if(meshRenderer_body != null) shapeKeyDict_body = MakeShapeKeyDictionary(meshRenderer_body);
        if(meshRenderer_eye != null) shapeKeyDict_eye = MakeShapeKeyDictionary(meshRenderer_eye);
        if(meshRenderer_eye_occlusion != null) shapeKeyDict_eye_occlusion = MakeShapeKeyDictionary(meshRenderer_eye_occlusion);
        if(meshRenderer_tear_line != null) shapeKeyDict_tear_line = MakeShapeKeyDictionary(meshRenderer_tear_line);
        if(meshRenderer_teeth != null) shapeKeyDict_teeth = MakeShapeKeyDictionary(meshRenderer_teeth);
        if(meshRenderer_tongue != null) shapeKeyDict_tongue = MakeShapeKeyDictionary(meshRenderer_tongue);
        //if(meshRenderer_brow != null) shapeKeyDict_brow = MakeShapeKeyDictionary(meshRenderer_brow);
    }
    private void MakeAdjustableShapeKeys()
    {
        shapeKeys = shapeKeyDict_body.Keys.ToArray();
        shapeValues = new int[shapeKeys.Length];
    }

    // Start is called before the first frame update
    void Start()
    {
        visemes = new float[15];
        visemes_current = new float[15];

        GetSkinnedMeshRenderers();
        MakeShapeKeyDictionaries();
        MakeAdjustableShapeKeys();

        Animator animator = GetComponent<Animator>();
        if(animator != null)
        {
            if (animator.isHuman)
            {
                jawBone = animator.GetBoneTransform(HumanBodyBones.Jaw);
                hasJawBone = true;
                jawBoneRotation = jawBone.localRotation;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        for (int i = 0; i < visemes.Length; i++)
        {
            if (Mathf.Abs(visemes_current[i] - visemes[i]) <= speed)
            {
                visemes_current[i] = visemes[i];
            }
            else
            {
                visemes_current[i] -= (speed * Mathf.Sign(visemes_current[i] - visemes[i]));
            }
        }

        float javOpen = Mathf.Max(visemes_current[11] * 0.5f, visemes_current[12] * 0.3f, visemes_current[10] * 0.9f, visemes_current[13] * 0.9f, visemes_current[14] * 0.7f);
        jawAngleInc = Mathf.Lerp(0, 15, javOpen / 100);

        SetShapeKey("EE", visemes_current[11]);
        //SetShapeKey("Er", visemes_current[0]);
        SetShapeKey("IH", visemes_current[12]);
        SetShapeKey("Ah", visemes_current[10]);
        SetShapeKey("Oh", visemes_current[13]);
        SetShapeKey("W_OO", visemes_current[14]);
        SetShapeKey("S_Z", visemes_current[7]);
        SetShapeKey("Ch_J", visemes_current[6]);
        SetShapeKey("F_V", visemes_current[2]);
        SetShapeKey("TH", visemes_current[3]);
        SetShapeKey("T_L_D_N", visemes_current[4]);
        SetShapeKey("B_M_P", visemes_current[1]);
        SetShapeKey("K_G_H_NG", visemes_current[5]);
        // SetShapeKey("AE", visemes_current[0]);
        SetShapeKey("R", visemes_current[9]);

        if (hasJawBone)
        {
            jawBone.localRotation = jawBoneRotation * Quaternion.Euler(new Vector3(0, 0, -jawAngleInc));
        }
    }

    private void TrySetShapeKeyValue(ref SkinnedMeshRenderer skinnedMeshRenderer, ref Dictionary<string,int> dictionary, string name, float value)
    {
        if(skinnedMeshRenderer != null)
        {
            int key = -1;
            dictionary.TryGetValue(name, out key);
            if(key != -1) { 
                skinnedMeshRenderer.SetBlendShapeWeight(key, value);
            }
        }
    }

    private void SetShapeKey(string name, float value)
    {
        TrySetShapeKeyValue(ref meshRenderer_body, ref shapeKeyDict_body, name, value);
        TrySetShapeKeyValue(ref meshRenderer_eye, ref shapeKeyDict_eye, name, value);
        TrySetShapeKeyValue(ref meshRenderer_eye_occlusion, ref shapeKeyDict_eye_occlusion, name, value);
        TrySetShapeKeyValue(ref meshRenderer_tear_line, ref shapeKeyDict_tear_line, name, value);
        //TrySetShapeKeyValue(ref meshRenderer_teeth, ref shapeKeyDict_teeth, name, value);
        TrySetShapeKeyValue(ref meshRenderer_tongue, ref shapeKeyDict_tongue, name, value);
        //TrySetShapeKeyValue(ref meshRenderer_brow, ref shapeKeyDict_brow, name, value);
    }

    private Dictionary<string,int> MakeShapeKeyDictionary(SkinnedMeshRenderer meshRenderer)
    {
        Dictionary<string, int> shapeKeyDict = new Dictionary<string, int>();
        for (int i = 0; i < meshRenderer.sharedMesh.blendShapeCount; i++)
        {
            shapeKeyDict.Add(meshRenderer.sharedMesh.GetBlendShapeName(i), i);
        }
        return shapeKeyDict;
    }

    static private GameObject GetChildGameObject(GameObject fromGameObject, string withName)
    {
        Transform[] ts = fromGameObject.transform.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in ts) if (t.gameObject.name == withName) return t.gameObject;
        return null;
    }
}
