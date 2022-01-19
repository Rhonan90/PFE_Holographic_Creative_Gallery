using System.Collections;
using System.Collections.Generic;
using Holo;

public class ExperienceFlowManager : HoloBehaviour
{
    [GestureComponent]
    private GestureComponent gestureComp;
    [GazeComponent]
    public GazeComponent gazeComponent;

    public HoloGameObject firstXpTuto;
    public HoloGameObject firstXpTutoQuitButton;
    private GazeComponent firstXpTutoQuitButtonGazeComponent;
    public HoloGameObject firstXpGestureManager;
    private PhotoCaptureBehaviour firstXpBehaviour;

    public HoloGameObject secondXpGestureManager;
    private HoloRoomChange secondXpBehaviour;

    private bool firstXP = false;
    private bool xpStarted = false;

    public override void Start()
    {
        gestureComp.RegisterPose(HandPose.HandFaceToEachOther);
        gestureComp.OnPoseStart += OnPoseStarted;
        firstXpBehaviour = (PhotoCaptureBehaviour) firstXpGestureManager.GetBehaviour("PhotoCaptureBehaviour");
        secondXpBehaviour = (HoloRoomChange) secondXpGestureManager.GetBehaviour("HoloRoomChange");


        firstXpTuto.transform.position = HoloCamera.mainCamera.transform.position + HoloCamera.mainCamera.transform.forward * 3;
        firstXpTuto.transform.position.y = HoloCamera.mainCamera.transform.position.y;
        firstXpTuto.SetActive(true);
    }


    public void StartFlow()
    {
        xpStarted = true;
        firstXP = true;
        firstXpBehaviour.StartXp();
        firstXpTuto.SetActive(false);
    }

    private void OnPoseStarted(HandPose handPose, Handness handness)
    {
        if (handPose == HandPose.HandFaceToEachOther && xpStarted)
        {
            if (firstXP)
            {
                firstXpBehaviour.EndXp();
                Log("Début de la seconde expérience");
                secondXpBehaviour.StartXp();
            }
            else
            {
                secondXpBehaviour.EndXp();
                firstXpBehaviour.StartXp();
                Log("Début de la première expérience");
            }
            firstXP = !firstXP;
        }
    }

}
