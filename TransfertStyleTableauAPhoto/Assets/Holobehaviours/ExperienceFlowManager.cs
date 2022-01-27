using System.Collections;
using System.Collections.Generic;
using Holo;

public class ExperienceFlowManager : HoloBehaviour
{
    [GestureComponent]
    private GestureComponent gestureComp;
    [GazeComponent]
    public GazeComponent gazeComponent;

    public HoloGameObject firstXpGestureManager;
    private PhotoCaptureBehaviour firstXpBehaviour;

    public HoloGameObject secondXpGestureManager;
    private HoloRoomChange secondXpBehaviour;

    private bool firstXP = false;
    private bool xpStarted = false;
    private bool flowStarted = false;

    public HoloAudioSource sonDebut;


    public override void Start()
    {
        gestureComp.RegisterPose(HandPose.HandFaceToEachOther);
        gestureComp.RegisterPose(HandPose.HandOpenedSky);
        gestureComp.OnPoseStart += OnPoseStarted;
        firstXpBehaviour = (PhotoCaptureBehaviour) firstXpGestureManager.GetBehaviour("PhotoCaptureBehaviour");
        secondXpBehaviour = (HoloRoomChange) secondXpGestureManager.GetBehaviour("HoloRoomChange");
    }


    public void StartFlow(HoloGameObject tuto)
    {
        xpStarted = true;
        firstXP = true;
        firstXpBehaviour.StartXp();
        tuto.SetActive(false);
    }

    private void OnPoseStarted(HandPose handPose, Handness handness)
    {
        if (handPose == HandPose.HandOpenedSky && !flowStarted)
        {
            flowStarted = true;
            sonDebut.enabled = true;
            HoloCoroutine.StartCoroutine(firstXpBehaviour.StartAfterIntroCoroutine);
        }
        if (handPose == HandPose.HandFaceToEachOther && xpStarted)
        {
            if (firstXP)
            {
                firstXpBehaviour.EndXp();
                Log("Debut de la seconde experience");
                secondXpBehaviour.StartXp();
            }
            else
            {
                secondXpBehaviour.EndXp();
                firstXpBehaviour.StartXp();
                Log("Debut de la première experience");
            }
            firstXP = !firstXP;
        }
    }

}
