using System.Collections;
using System.Collections.Generic;
using Holo;

public class HoloRoomChange : HoloBehaviour
{
    [GestureComponent]
    private GestureComponent gestureComp;
    [GazeComponent]
    private GazeComponent gazeComponent;

    public HoloGameObject playSpace;
    private List<GazeComponent> gazeComponents;

    // Tableaux dans la pièce comportant un box collider
    public HoloGameObject tableau1; private GazeComponent tableau1GazeComponent;
    public HoloGameObject tableau2; private GazeComponent tableau2GazeComponent;
    public HoloGameObject tableau3; private GazeComponent tableau3GazeComponent;
    public HoloGameObject tableau4; private GazeComponent tableau4GazeComponent;
    public HoloGameObject tableau5; private GazeComponent tableau5GazeComponent;

    public HoloGameObject[] styles;

    private float time = 0;
    public float apparitionTime = 1;

    private bool xpActivated = false;

    // Start is called before the first frame update
    public override void Start()
    {
        gestureComp.RegisterPose(HandPose.HandOpenedSky);
        gestureComp.RegisterPose(HandPose.HandOpenedGround);
        gestureComp.OnPoseStart += OnposeStarted;

        gazeComponents = new List<GazeComponent>();
        InitializeGazeComponents();
    }

    private void InitializeGazeComponents()
    {
        tableau1GazeComponent = Engine.AddHoloComponent<GazeComponent>(nameof(tableau1GazeComponent));
        tableau1GazeComponent.attribute.GameObject = tableau1;
        tableau1GazeComponent.attribute.UseSnap = true;
        tableau1GazeComponent.OnGazeEvent += ScanGazeComponent_OnGazeEvent;
        gazeComponents.Add(tableau1GazeComponent);

        tableau2GazeComponent = Engine.AddHoloComponent<GazeComponent>(nameof(tableau2GazeComponent));
        tableau2GazeComponent.attribute.GameObject = tableau2;
        tableau2GazeComponent.attribute.UseSnap = true;
        tableau2GazeComponent.OnGazeEvent += ScanGazeComponent_OnGazeEvent;
        gazeComponents.Add(tableau2GazeComponent);

        tableau3GazeComponent = Engine.AddHoloComponent<GazeComponent>(nameof(tableau3GazeComponent));
        tableau3GazeComponent.attribute.GameObject = tableau3;
        tableau3GazeComponent.attribute.UseSnap = true;
        tableau3GazeComponent.OnGazeEvent += ScanGazeComponent_OnGazeEvent;
        gazeComponents.Add(tableau3GazeComponent);

        tableau4GazeComponent = Engine.AddHoloComponent<GazeComponent>(nameof(tableau4GazeComponent));
        tableau4GazeComponent.attribute.GameObject = tableau4;
        tableau4GazeComponent.attribute.UseSnap = true;
        tableau4GazeComponent.OnGazeEvent += ScanGazeComponent_OnGazeEvent;
        gazeComponents.Add(tableau4GazeComponent);

        tableau5GazeComponent = Engine.AddHoloComponent<GazeComponent>(nameof(tableau5GazeComponent));
        tableau5GazeComponent.attribute.GameObject = tableau5;
        tableau5GazeComponent.attribute.UseSnap = true;
        tableau5GazeComponent.OnGazeEvent += ScanGazeComponent_OnGazeEvent;
        gazeComponents.Add(tableau5GazeComponent);
    }

    private void ScanGazeComponent_OnGazeEvent(GazeComponent _component, GazeEvent _event)
    {
        if (_event == GazeEvent.OnTap && xpActivated)
        {
            for (int i = 0; i < gazeComponents.Count; i++)
            {
                if (_component == gazeComponents[i])   //gestion des événements sur les différents tableaux
                {
                    playSpace.GetHoloElement<HoloMeshRenderer>().material = styles[i].GetHoloElement<HoloMeshRenderer>().material;
                }
            }
        }
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
        SetActiveTableaux(true);
    }

    public void EndXp()
    {
        xpActivated = false;
        playSpace.SetActive(false);
        HoloCoroutine.StartCoroutine(hidePlayspaceCoroutine);
    }

    private void SetActiveTableaux(bool _bool)
    {
        for (int i = 0; i < gazeComponents.Count; i++)
        {
            gazeComponents[i].GameObject.SetActive(_bool);
        }
    }
}
