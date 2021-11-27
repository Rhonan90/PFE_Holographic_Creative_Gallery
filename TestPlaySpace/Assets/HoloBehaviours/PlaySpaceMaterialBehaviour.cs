using System.Collections;
using System.Collections.Generic;

using Holo;

public class PlaySpaceMaterialBehaviour : HoloBehaviour
{
    public HoloGameObject scanButton;
    public HoloGameObject displayButton;
    public HoloGameObject materialButton;
    public HoloGameObject collisionButton;
    public HoloGameObject redMaterialGO;
    public HoloGameObject blueMaterialGO;
    public HoloGameObject otherMaterialGO1;
    public HoloGameObject otherMaterialGO2;
    public HoloGameObject otherMaterialGO3;
    public HoloGameObject playspaceGO;

    public HoloGameObject torch;

    public HoloGameObject wallHit;
    
    [GestureComponent]
    GestureComponent gestureComponent;

    private GazeComponent scanGazeComponent;
    private GazeComponent displayGazeComponent;
    private GazeComponent materialGazeComponent;
    private GazeComponent collisionGazeComponent;


    private HoloMaterial redMaterial;
    private HoloMaterial blueMaterial;
    private HoloMaterial otherMaterial1;
    private HoloMaterial otherMaterial2;
    private HoloMaterial otherMaterial3;

    bool trackHand = false;
    int currentMateialIndex = 0;
    List<HoloMaterial> materials;

    override public void Start()
    {

        scanGazeComponent = Engine.AddHoloComponent<GazeComponent>(nameof(scanGazeComponent));
        scanGazeComponent.attribute.GameObject = scanButton;
        scanGazeComponent.attribute.UseSnap = true;
        scanGazeComponent.OnGazeEvent += ScanGazeComponent_OnGazeEvent;

        displayGazeComponent = Engine.AddHoloComponent<GazeComponent>(nameof(displayGazeComponent));
        displayGazeComponent.attribute.GameObject = displayButton;
        displayGazeComponent.attribute.UseSnap = true;
        displayGazeComponent.OnGazeEvent += ScanGazeComponent_OnGazeEvent;

        materialGazeComponent = Engine.AddHoloComponent<GazeComponent>(nameof(materialGazeComponent));
        materialGazeComponent.attribute.GameObject = materialButton;
        materialGazeComponent.attribute.UseSnap = true;
        materialGazeComponent.OnGazeEvent += ScanGazeComponent_OnGazeEvent;

        collisionGazeComponent = Engine.AddHoloComponent<GazeComponent>(nameof(collisionGazeComponent));
        collisionGazeComponent.attribute.GameObject = collisionButton;
        collisionGazeComponent.attribute.UseSnap = true;
        collisionGazeComponent.OnGazeEvent += ScanGazeComponent_OnGazeEvent;

        redMaterial = redMaterialGO.GetHoloElement<HoloRenderer>().material;
        blueMaterial = blueMaterialGO.GetHoloElement<HoloRenderer>().material;
        otherMaterial1 = otherMaterialGO1.GetHoloElement<HoloRenderer>().material;
        otherMaterial2 = otherMaterialGO2.GetHoloElement<HoloRenderer>().material;
        otherMaterial3 = otherMaterialGO3.GetHoloElement<HoloRenderer>().material;

        materials = new List<HoloMaterial>();
        materials.Add(redMaterial);
        materials.Add(blueMaterial);
        materials.Add(otherMaterial1);
        materials.Add(otherMaterial2);
        materials.Add(otherMaterial3);

        if (PlatformHelper.IsDesktop() || PlatformHelper.IsEditor())
        {
            PlayspaceHelper.SetSource(playspaceGO);
            playspaceGO.SetActive(true);
        }
 
        PlayspaceHelper.SetMaterial(blueMaterial);
        PlayspaceHelper.SetActive(true);
        PlayspaceHelper.SetCollision(false);
        PlayspaceHelper.SetDisplay(true);
        UpdateLabels();


        torch.SetActive(false);
        gestureComponent.RegisterPose(HandPose.HandOpenedSky);
        gestureComponent.RegisterPose(HandPose.HandOpenedGround);
        gestureComponent.OnPoseStart += GestureComponent_OnPoseStart;
        gestureComponent.OnPoseStop += GestureComponent_OnPoseStop;

        Async.OnUpdate += Async_OnUpdate;

    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        PlayspaceHelper.RestoreLocalSource();
    }

    private void Async_OnUpdate()
    {
        if (trackHand)
        {
            gestureComponent.FollowHandPosition(Handness.Left, 1);
            gestureComponent.FollowHandRotation(Handness.Left, 1);
        }

        HoloRay ray = new HoloRay(torch.transform.position, torch.transform.forward);
        var raycastHit = PhysicsHelper.Raycast(ray, 10, PlayspaceHelper.GetPlayspaceLayerMask());
        if (raycastHit == null)
        {
            wallHit.SetActive(false);
        }
        else
        {
            wallHit.SetActive(true);
            wallHit.transform.position = raycastHit.point;
            wallHit.transform.rotation = HoloQuaternion.LookRotation(raycastHit.normal);
        }
    }

    private void GestureComponent_OnPoseStop(HandPose _pose, Handness _handness, bool arg3)
    {
        if (_handness != Handness.Left)
        {
            return;
        }
        if (_pose == HandPose.HandOpenedSky)
        {
            trackHand = false;
        }
    }

    private void GestureComponent_OnPoseStart(HandPose _pose, Handness _handness)
    {
        if (_handness != Handness.Left)
        {
            return;
        }
        if (_pose == HandPose.HandOpenedSky)
        {
            torch.SetActive(true);
            trackHand = true;
        }
        else if (_pose == HandPose.HandOpenedGround)
        {
            torch.SetActive(false);
        }
    }

    private void ScanGazeComponent_OnGazeEvent(GazeComponent _component, GazeEvent _event)
    {
        if (_event == GazeEvent.OnTap)
        {
            if (_component == scanGazeComponent)
            {
                PlayspaceHelper.SetScan(!PlayspaceHelper.GetScan());  
            }
            else if (_component == displayGazeComponent)
            {
                PlayspaceHelper.SetDisplay(!PlayspaceHelper.GetDisplay());
            }
            else if (_component == collisionGazeComponent)
            {
                PlayspaceHelper.SetCollision(!PlayspaceHelper.GetCollision());
            }
            else if (_component == materialGazeComponent)
            {
                currentMateialIndex = (currentMateialIndex + 1) % materials.Count;
                PlayspaceHelper.SetMaterial(materials[currentMateialIndex]);
            }
            UpdateLabels();
        }
    }

    void UpdateLabels()
    {
        scanButton.GetHoloElement<HoloText>().text = "Scan\n" + PlayspaceHelper.GetScan();
        displayButton.GetHoloElement<HoloText>().text = "Display\n" + PlayspaceHelper.GetDisplay();
        string materialName = PlayspaceHelper.GetMaterial().GetName();
        materialButton.GetHoloElement<HoloText>().text = "Material\n" + materialName;
        collisionButton.GetHoloElement<HoloText>().text = "Collision\n" + PlayspaceHelper.GetCollision();
    }
        
}
