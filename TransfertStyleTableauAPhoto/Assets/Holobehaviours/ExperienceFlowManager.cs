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
    public HoloGameObject firstXpTuto2;
    public HoloGameObject firstXpGestureManager;
    private PhotoCaptureBehaviour firstXpBehaviour;

    public HoloGameObject secondXpGestureManager;
    private HoloRoomChange secondXpBehaviour;

    private bool firstXP = false;
    private bool xpStarted = false;

    public HoloAudioSource sonDébut;


    public override void Start()
    {
        gestureComp.RegisterPose(HandPose.HandFaceToEachOther);
        gestureComp.OnPoseStart += OnPoseStarted;
        firstXpBehaviour = (PhotoCaptureBehaviour) firstXpGestureManager.GetBehaviour("PhotoCaptureBehaviour");
        secondXpBehaviour = (HoloRoomChange) secondXpGestureManager.GetBehaviour("HoloRoomChange");

        sonDébut.enabled = true;
        HoloCoroutine.StartCoroutine(StartAfterIntroCoroutine);
    }


    public void StartFlow()
    {
        xpStarted = true;
        firstXP = true;
        firstXpBehaviour.StartXp();
        firstXpTuto2.SetActive(false);
        HoloCoroutine.StartCoroutine(firstXpBehaviour.VocalInstructionsCoroutine);
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

    IEnumerator StartAfterIntroCoroutine()
    {
        yield return HoloCoroutine.WaitForSeconds(2.5f);
        firstXpTuto.transform.position = HoloCamera.mainCamera.transform.position + HoloCamera.mainCamera.transform.forward * 3;
        firstXpTuto.transform.position.y = HoloCamera.mainCamera.transform.position.y;
        firstXpTuto.SetActive(true);

        firstXpTuto2.transform.position = HoloCamera.mainCamera.transform.position + HoloCamera.mainCamera.transform.forward * 3;
        firstXpTuto2.transform.position.y = HoloCamera.mainCamera.transform.position.y;
    }

    public void HideFirstTuto()
    {
        firstXpTuto.SetActive(false);
    }
}
