using System.Collections;
using System.Collections.Generic;
using Holo;

public class HoloRoomChange : HoloBehaviour
{
    [GestureComponent]
    private GestureComponent gestureComp;

    public HoloGameObject playSpace;

    private float time = 0;
    public float apparitionTime = 1;

    private bool xpActivated = false;

    // Start is called before the first frame update
    public override void Start()
    {
        gestureComp.RegisterPose(HandPose.HandOpenedSky);
        gestureComp.RegisterPose(HandPose.HandOpenedGround);
        gestureComp.OnPoseStart += OnposeStarted;
    }

    private void OnposeStarted(HandPose handPose, Handness handness)
    {
        if (xpActivated)
        {
            if (handPose == HandPose.HandOpenedSky)
            {
                playSpace.SetActive(true);
                //HoloCoroutine.StartCoroutine(showPlayspaceCoroutine);
            }
            if (handPose == HandPose.HandOpenedGround)
            {
                playSpace.SetActive(false);
                //HoloCoroutine.StartCoroutine(hidePlayspaceCoroutine);
            }
        }
    }

    IEnumerator showPlayspaceCoroutine()
    {
        playSpace.SetActive(true);
        time += TimeHelper.deltaTime;
        if (time < apparitionTime)
        {
            Log( (time / apparitionTime).ToString());
            yield return HoloCoroutine.WaitForFrame(1);
            playSpace.GetHoloElement<HoloMeshRenderer>().material.color.a = time / apparitionTime * 255;
            HoloCoroutine.StartCoroutine(showPlayspaceCoroutine);
        }
        else
        {
            time = 0;
            yield return HoloCoroutine.WaitForSeconds(0);
        }
    }

    IEnumerator hidePlayspaceCoroutine()
    {
        time += TimeHelper.deltaTime;
        if (time < apparitionTime)
        {
            yield return HoloCoroutine.WaitForFrame(1);
            playSpace.GetHoloElement<HoloMeshRenderer>().material.color.a = 255 - time / apparitionTime * 255;
            HoloCoroutine.StartCoroutine(hidePlayspaceCoroutine);
        }
        else
        {
            time = 0;
            playSpace.SetActive(false);
            yield return HoloCoroutine.WaitForSeconds(0);
        }
    }

    public void StartXp()
    {
        xpActivated = true;
    }

    public void EndXp()
    {
        xpActivated = false;
        HoloCoroutine.StartCoroutine(hidePlayspaceCoroutine);
    }
}
