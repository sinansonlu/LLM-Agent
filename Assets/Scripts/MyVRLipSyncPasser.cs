using UnityEngine;

public class MyVRLipSyncPasser : MonoBehaviour
{
    public FaceScriptCC currentFace;
    public FaceScriptCC currentFace2;

    public int smoothAmount = 70;

    private OVRLipSyncContextBase lipsyncContext = null;

    void Start()
    {
        lipsyncContext = GetComponent<OVRLipSyncContextBase>();
        if (lipsyncContext == null)
        {
            Debug.LogError("LipSyncContextMorphTarget.Start Error: " +
                "No OVRLipSyncContext component on this object!");
        }
        else
        {
            lipsyncContext.Smoothing = smoothAmount;
        }
    }

    void Update()
    {
        if ((lipsyncContext != null) && currentFace != null)
        {
            OVRLipSync.Frame frame = lipsyncContext.GetCurrentPhonemeFrame();
            
            if (frame != null)
            {
                for (int i = 0; i < currentFace.visemes.Length; i++)
                {
                    currentFace.visemes[i] = frame.Visemes[i] * 100;
                }
            }

            if (smoothAmount != lipsyncContext.Smoothing)
            {
                lipsyncContext.Smoothing = smoothAmount;
            }
        }

        if ((lipsyncContext != null) && currentFace2 != null)
        {
            OVRLipSync.Frame frame = lipsyncContext.GetCurrentPhonemeFrame();

            if (frame != null)
            {
                for (int i = 0; i < currentFace2.visemes.Length; i++)
                {
                    currentFace2.visemes[i] = frame.Visemes[i] * 100;
                }
            }

            if (smoothAmount != lipsyncContext.Smoothing)
            {
                lipsyncContext.Smoothing = smoothAmount;
            }
        }
    }
}