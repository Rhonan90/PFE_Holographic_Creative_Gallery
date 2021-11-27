using System.Collections;
using System.Collections.Generic;
using Holo;

enum state
{
    INIT,
    CHOIXIMAGE,
    RESULTAT
};

public class HoloTableau : HoloBehaviour
{
    public HoloGameObject materialTableau;
    public HoloGameObject materialLeaf;
    //public HoloGameObject materialRoom;
    //public HoloGameObject materialSelfie;
    //public HoloGameObject materialStreet;
    public HoloGameObject materialLeafTf;
    //public HoloGameObject materialRoomTf;
    //public HoloGameObject materialStreetTf;
    //public HoloGameObject materialSelfieTf;
    private List<HoloMaterial> materials;
    private List<HoloMaterial> materialsTF;
    private int index = 0;
    public HoloAudioSource[] clips;  //choix_image -> wow -> airtap_me
    private int indexChoisi;
    public HoloGameObject imageChoiceCanvas;
    public HoloGameObject restartChoiceCanvas;
    public HoloGameObject petitTableauFusion;

    public HoloGameObject image1;
    //public HoloGameObject image2;
    //public HoloGameObject image3;
    public HoloGameObject startAgainButton;
    public HoloGameObject quitButton;

    [GestureComponent]
    GestureComponent gestureComponent;

    private GazeComponent tableauGazeComponent;
    private GazeComponent image1GazeComponent;
    //private GazeComponent image2GazeComponent;
    //private GazeComponent image3GazeComponent;
    private GazeComponent startAgainGazeComponent;
    private GazeComponent quitGazeComponent;

    private int iImage;


    state etat = state.INIT;

    override public void Awake()
    {
        base.Awake();
        materials = new List<HoloMaterial>();
        materialsTF = new List<HoloMaterial>();
        ////materials.SetValue(materialSelfie.GetHoloElement<HoloRenderer>().material, 0);
        ////materials.SetValue(materialRoom.GetHoloElement<HoloRenderer>().material, 1);
        ////materials.SetValue(materialStreet.GetHoloElement<HoloRenderer>().material, 2);
        materials.Add(materialLeaf.GetHoloElement<HoloRenderer>().material);
        //materials.Add(materialSelfie.GetHoloElement<HoloRenderer>().material);
        //materials.Add(materialRoom.GetHoloElement<HoloRenderer>().material);
        //materials.Add(materialStreet.GetHoloElement<HoloRenderer>().material);
        materialsTF.Add(materialTableau.GetHoloElement<HoloRenderer>().material);
        materialsTF.Add(materialLeafTf.GetHoloElement<HoloRenderer>().material);
        //materialsTF.Add(materialSelfieTf.GetHoloElement<HoloRenderer>().material);
        //materialsTF.Add(materialRoomTf.GetHoloElement<HoloRenderer>().material);
        //materialsTF.Add(materialStreetTf.GetHoloElement<HoloRenderer>().material);

    }

    public override void Start()
    {
        base.Start();
        tableauGazeComponent = Engine.AddHoloComponent<GazeComponent>(nameof(tableauGazeComponent));
        tableauGazeComponent.attribute.GameObject = this.gameObject;
        tableauGazeComponent.attribute.UseSnap = true;
        tableauGazeComponent.OnGazeEvent += ScanGazeComponent_OnGazeEvent;

        image1GazeComponent = Engine.AddHoloComponent<GazeComponent>(nameof(image1GazeComponent));
        image1GazeComponent.attribute.GameObject = image1;
        image1GazeComponent.attribute.UseSnap = true;
        image1GazeComponent.OnGazeEvent += ScanGazeComponent_OnGazeEvent;

        //image2GazeComponent = Engine.AddHoloComponent<GazeComponent>(nameof(image2GazeComponent));
        //image2GazeComponent.attribute.GameObject = image2;
        //image2GazeComponent.attribute.UseSnap = true;
        //image2GazeComponent.OnGazeEvent += ScanGazeComponent_OnGazeEvent;

        //image3GazeComponent = Engine.AddHoloComponent<GazeComponent>(nameof(image3GazeComponent));
        //image3GazeComponent.attribute.GameObject = image3;
        //image3GazeComponent.attribute.UseSnap = true;
        //image3GazeComponent.OnGazeEvent += ScanGazeComponent_OnGazeEvent;

        startAgainGazeComponent = Engine.AddHoloComponent<GazeComponent>(nameof(startAgainGazeComponent));
        startAgainGazeComponent.attribute.GameObject = startAgainButton;
        startAgainGazeComponent.attribute.UseSnap = true;
        startAgainGazeComponent.OnGazeEvent += ScanGazeComponent_OnGazeEvent;

        quitGazeComponent = Engine.AddHoloComponent<GazeComponent>(nameof(quitGazeComponent));
        quitGazeComponent.attribute.GameObject = quitButton;
        quitGazeComponent.attribute.UseSnap = true;
        quitGazeComponent.OnGazeEvent += ScanGazeComponent_OnGazeEvent;
    }

    private void ScanGazeComponent_OnGazeEvent(GazeComponent _component, GazeEvent _event)
    {
        if (_event == GazeEvent.OnTap)
        {
            if (_component == tableauGazeComponent)
            {
                TableauFixé();
            }
            if (_component == image1GazeComponent )   //|| _component == image2GazeComponent || _component == image3GazeComponent)
            {
                ChoixImage(iImage);
            }
            if (_component == startAgainGazeComponent)
            {
                RestartExp();
            }
            if (_component == quitGazeComponent)
            {
                Quit();
            }

        }
    }

    public void ChoixIndexImage(int indexImage)
    {
        iImage = indexImage;
    }

        public void ChoixImage(int indexImage)
    {
        indexChoisi = indexImage + 1;
        imageChoiceCanvas.SetActive(false);
        etat = state.RESULTAT;
        SwitchTextureInt(indexChoisi);
        clips[1].gameObject.SetActive(true);
        petitTableauFusion.transform.parent.gameObject.SetActive(true);
        clips[2].gameObject.SetActive(true);
    }

    public void RestartExp()
    {
        etat = state.INIT;
        petitTableauFusion.transform.parent.gameObject.SetActive(false);
        restartChoiceCanvas.SetActive(false);
        clips[1].gameObject.SetActive(false);
        SwitchTextureInt(0);
    }

    public void Quit()
    {
        RestartExp();
        //quit ?
    }

    public void TableauFixé()
    {
        switch (etat)
        {
            case state.INIT:
                etat = state.CHOIXIMAGE;
                imageChoiceCanvas.SetActive(true);
                clips[0].gameObject.SetActive(true);
                break;
            case state.CHOIXIMAGE:
                break;
            case state.RESULTAT:
                restartChoiceCanvas.SetActive(true);
                break;
            default:
                break;
        }
    }

    public void OnAirTap()
    {
        if (etat == state.RESULTAT)
        {
            restartChoiceCanvas.SetActive(true);
        }
    }

    public void SwitchTexture()
    {
        index = (index + 1) % materialsTF.Count;
        gameObject.GetHoloElement<HoloRenderer>().material = materialsTF[index];      
    }

    public void SwitchTextureInt(int index)
    {
        if (index >= materialsTF.Count)
            return;
        gameObject.GetHoloElement<HoloRenderer>().material = materialsTF[index];
        petitTableauFusion.GetHoloElement<HoloRenderer>().material = materials[index-1];
    }
}
