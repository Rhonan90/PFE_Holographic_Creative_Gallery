using Holo;
using System.Collections;
using System.Collections.Generic;


public class PhotoCaptureBehaviour : HoloBehaviour
{
    [GestureComponent]
    private GestureComponent gestureComp;
    [GazeComponent]
    public GazeComponent gazeComponent;

    [PhotoCaptureComponent]
    public PhotoCaptureComponent photoCaptureComponent;

    public HoloGameObject picture;
    public HoloGameObject toile;
    public HoloGameObject toileHolder;
    public List<HoloGameObject> toileHolders;
    private int currIndexToile = 0;
    public HoloGameObject LabelUI;
    public HoloGameObject Chevalet;
    private HoloVector3 chevaletInitialScale;
    public HoloGameObject photoViewer;
    public HoloGameObject imagePrefab;

    private bool xpActivated = false;

    // Tableaux dans la pièce comportant un box collider
    public HoloGameObject tableau1; private GazeComponent tableau1GazeComponent;
    public HoloGameObject tableau2; private GazeComponent tableau2GazeComponent;
    public HoloGameObject tableau3; private GazeComponent tableau3GazeComponent;
    public HoloGameObject tableau4; private GazeComponent tableau4GazeComponent;
    public HoloGameObject tableau5; private GazeComponent tableau5GazeComponent;

    public HoloGameObject firstXpTuto;
    public HoloGameObject firstXpTuto2;

    private List<GazeComponent> gazeComponents;

    private Timer timer;
    private bool canGesture = false;     //si les gestes sont reconnus dans le script
    private bool firstTutoDone = false;
    private bool canTakePhoto = false;
    private bool canLaunchEndTuto = false;
    private bool secondTutoDone = false;

    private List<string> photosPath;     //path des photos prises pour y acceder/les supprimer
    private List<HoloGameObject> photos;    //photos prises et affichees disponibles à la selection
    private List<HoloGameObject> transformedPhotos;     //photos transformees recuperees depuis le serveur


    private int? chosenPhotoIndex = null;
    public int maxPhotos = 4;
    private int _currentPhotoPath = 0;
    
    private int? styleIndex; //?? not sure yet if int is the best indicator, mb style name better ?
    private string[] styles = { "mosaic", "rain_princess", "candy", "scream", "udnie"};

    bool inProgress = false;

    public HoloAudioSource sonIntro;
    public HoloAudioSource sonPhoto;
    public HoloAudioSource sonTableau;
    public HoloAudioSource sonResultat;
    public HoloAudioSource sonPrisePhoto;
    public HoloAudioSource sonTransformationImage;
    public HoloAudioSource sonChoixStyle;
    public HoloAudioSource sonAutreChoix;

    HoloAudioSource sonCoroutine; 
    float timeRepeatCoroutine;

    public override void Start()
    {
        UpdateLabel("Ready");

        //gestes reconnus par ce script
        gestureComp.RegisterPose(HandPose.HandFaceUser);
        gestureComp.OnPoseStart += OnposeStarted;

        //initialisation des listes 
        gazeComponents = new List<GazeComponent>();
        photosPath = new List<string>();
        photos = new List<HoloGameObject>();
        transformedPhotos = new List<HoloGameObject>();

        //initialisation des tableaux
        InitializeGazeComponents();

        //initialisation pour le temps minimum entre deux reconnaissances de gestes
        timer = new Timer(2,() => { canGesture = true; });
        timer.Start();
        canGesture = false;

        chevaletInitialScale = Chevalet.transform.localScale;

        //initialise les tableaux transformés en tant que Editable pour qu'ils soient déplaçables
        for (int i = 0; i < toileHolders.Count; i++)
        {
            HandlerHelper.SetHandlerEditable(toileHolders[i],true);
        }
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
        if (_event == GazeEvent.OnTap && firstTutoDone)
        {
            for (int i = 0; i < gazeComponents.Count; i++)
            {
                if (_component == gazeComponents[i])
                {
                    styleIndex = i;

                    if (inProgress)
                    {
                        Log("Already in progress");
                        return;
                    }
                    if (chosenPhotoIndex == null)
                    {
                        Log("No photo taken yet");
                        return;
                    }
                    sonChoixStyle.enabled = false;
                    sonChoixStyle.enabled = true;
                    SendPhotoToServer(chosenPhotoIndex.Value, styleIndex.Value);

                    firstXpTuto2.SetActive(false);                   
                }
            }
        }
    }


    private void OnposeStarted(HandPose handPose, Handness handness)
    {
        if (xpActivated && canGesture && canTakePhoto)
        {
            if (handPose == HandPose.HandFaceUser)
            {
                if (inProgress)
                {
                    Log("Already in progress");
                    return;
                }
                LaunchPhotoCapture();

                timer.Start();
                canGesture = false;
            }
        }
    }

    public void LaunchPhotoCapture()
    {
        //StyleChoice.SetActive(false);
        UpdateLabel("Taking picture, don't move");
        inProgress = true;
        ShowLabelUi();
        sonPrisePhoto.enabled = true;
        photoCaptureComponent.TakePicture(OnPhotoTaken);
    }

    void OnPhotoTaken(bool _success, int _id)
    {
        if (!_success)
        {
            Error("Failed to take photo");
            inProgress = false;
            return;
        }


        string filename = "Photo_" + _currentPhotoPath + ".png";
        string path = PathHelper.Combine(PathHelper.GetPersistentDataPath(), filename);
        //Log("path to save = " + path);
        photosPath.Add(path);
        chosenPhotoIndex = photosPath.FindIndex(0,(str) => (str==path));
        int tempCurrentPhotoPath = _currentPhotoPath;
        _currentPhotoPath = (_currentPhotoPath + 1) % maxPhotos;

        UpdateLabel("Saving photo");

        Log("Saving photo to " + path);
        photoCaptureComponent.SavePhotoIDToFile(_id, path, (saveSuccess, saveId) =>
        {
            if (!saveSuccess)
            {
                inProgress = false;
                UpdateLabel("Failed to save photo");
                return;

            }
            photoCaptureComponent.ReleasePhotoID(_id);
            LabelUI.SetActive(false);
            picture.GetHoloElementInChildren<HoloRenderer>().material.SetTextureFromUrl(path);
            picture.SetActive(true);


            if (photos.Count < maxPhotos)
            {
                HoloGameObject temp = imagePrefab.Duplicate(imagePrefab.transform.position, photoViewer, chosenPhotoIndex.Value.ToString());
                photos.Add(temp);
                temp.GetHoloElement<HoloImage>().SetFile(path);
                temp.SetActive(true);
            }
            else
            {
                photos[tempCurrentPhotoPath].GetHoloElement<HoloImage>().SetFile(path);
            }
            SelectPicture(chosenPhotoIndex.Value);

            
        });
        sonPrisePhoto.enabled = false;
        inProgress = false;
        firstTutoDone = true;
    }

    void SendPhotoToServer(int photoIndex, int styleIndex)
    {
        ShowLabelUi();
        inProgress = true;
        int lastSlashIndex = photosPath[photoIndex].LastIndexOf("\\");
        string filename = photosPath[photoIndex].Substring(lastSlashIndex + 1);
        string tempPath = photosPath[photoIndex].Substring(0, lastSlashIndex);
        tempPath = tempPath + styles[styleIndex] + "$" + filename;

        FileHelper.CopyFile(photosPath[photoIndex], tempPath);

        // Run command  ServerTF/CameraCaptureServer.py runServer  to start the server
        string serverOfHostIp = NetworkHelper.GetHoloSceneServerIP();  //192.168.43.186 maxime    192.168.43.112 Quentin     192.168.43.132 Paul  pour les tests en local sur la 4G de Paul
        string url = "http://" + serverOfHostIp + ":33900/" + styles[styleIndex] + "$" + filename;

        Log("Uploading to " + url);
        UpdateLabel("Uploading photo");
        HTTPHelper.SendFile(url, new Dictionary<string, string>(), photosPath[photoIndex], (_sendSuccess, _sendResult) =>
        {
            if (_sendSuccess)
            {
                Log("Set texture from file " + url);
                UpdateLabel("Donwloading process photo");

                //HoloGameObject newTransformedPhoto = toileHolder.Duplicate(toile.transform.position,_name: filename + "_" + styles[styleIndex] );
                //newTransformedPhoto.SetActive(true);
                //transformedPhotos.Add(newTransformedPhoto);

                //HandlerHelper.SetHandlerEditable(newTransformedPhoto.transform.parent.gameObject, true);
                //HoloGameObject picture = newTransformedPhoto.FindInHierarchy("ToileSubstitute");


                toileHolders[currIndexToile].transform.position = Chevalet.transform.position;
                toileHolders[currIndexToile].transform.rotation = toileHolder.transform.rotation;
                toileHolders[currIndexToile].transform.localScale *= (Chevalet.transform.localScale.x / chevaletInitialScale.x);
                HoloGameObject picture = toileHolders[currIndexToile].FindInHierarchy("ToileSubstitute");
                picture.transform.rotation = toileHolder.FindInHierarchy("ToileSubstitute").transform.rotation;
                transformedPhotos.Add(picture);
                currIndexToile = (currIndexToile + 1) % toileHolders.Count;

                picture.GetHoloElementInChildren<HoloRenderer>().material.SetTextureFromUrl(url, (_width, _height) =>
                {
                    if (_width == 0)
                    {
                        Error("Could not set texture");
                        UpdateLabel("Failed to download");
                        HoloCoroutine.StartCoroutine(HideLabelAfterTimeCoroutine);
                    }
                    else
                    {
                        Log("Texture loaded " + _width + "x" + _height);
                        UpdateLabel("Done!");
                        LabelUI.SetActive(false);
                        sonTransformationImage.enabled = false;
                        sonTransformationImage.enabled = true;
                        //HoloCoroutine.StartCoroutine(PlayInstructionEndCoroutine);
                    }
                    inProgress = false;
                });
            }
            else
            {
                Error("Could not send file " + url);
                UpdateLabel("Failed to upload");
                inProgress = false;
                HoloCoroutine.StartCoroutine(HideLabelAfterTimeCoroutine);
            }
        });
        sonChoixStyle.enabled = false;
        secondTutoDone = true;
        canLaunchEndTuto = true;
    }

    void UpdateLabel(string _message)
    {
        Log(_message);
        LabelUI.FindInHierarchy("Label").GetHoloElement<HoloText>().text = _message;
    }

    void ShowLabelUi()
    {
        LabelUI.SetActive(true);
        LabelUI.transform.position = HoloCamera.mainCamera.transform.position + HoloCamera.mainCamera.transform.forward * 2.5f;
    }

    public void StartXp()
    {
        xpActivated = true;
        Chevalet.SetActive(true);
        if (chosenPhotoIndex != null)
        {
            picture.SetActive(true);
        }
        SetActiveTransformedPhotos(true);
        SetActiveTableaux(true);
        canTakePhoto = true;
    }

    public void EndXp()
    {
        xpActivated = false;
        LabelUI.SetActive(false);
        SetActiveTableaux(false);
        picture.SetActive(false);
        Chevalet.SetActive(false);
        SetActiveTransformedPhotos(false);
        inProgress = false;
    }

    public void SelectPhoto(HoloGameObject image)
    {
        int num = 0;
        int.TryParse(image.name,out num);
        chosenPhotoIndex = num;
        picture.GetHoloElementInChildren<HoloRenderer>().material.SetTextureFromUrl(photosPath[chosenPhotoIndex.Value]);
        SelectPicture(chosenPhotoIndex.Value);
    }

    private void SelectPicture(int index)
    {
        for (int i = 0; i < photos.Count; i++)
        {
            photos[i].GetChildren().ForEach((outine) => { outine.SetActive(false); });
        }
        photos[index].GetChildren().ForEach((outine) => { outine.SetActive(true); });
    }

    private void SetActiveTransformedPhotos(bool state)
    {
        for (int i = 0; i < transformedPhotos.Count; i++)
        {
            transformedPhotos[i].SetActive(state);
        }
    }

    IEnumerator HideLabelAfterTimeCoroutine()
    {
        yield return HoloCoroutine.WaitForSeconds(3);
        LabelUI.SetActive(false);
    }

    IEnumerator PlayInstructionEndCoroutine()
    {
        yield return HoloCoroutine.WaitForSeconds(1);
        sonResultat.enabled = false;
        sonResultat.enabled = true;
    }

    public IEnumerator StartAfterIntroCoroutine()
    {
        //sonIntro.enabled = true;
        yield return HoloCoroutine.WaitForSeconds(9);
        
        firstXpTuto.transform.position = HoloCamera.mainCamera.transform.position + HoloCamera.mainCamera.transform.forward * 1f;
        firstXpTuto.transform.position.y = HoloCamera.mainCamera.transform.position.y;
        firstXpTuto.transform.LookAt(HoloCamera.mainCamera.transform.position);
        firstXpTuto.SetActive(true);
        yield return HoloCoroutine.WaitForSeconds(1);
        sonPhoto.enabled = true;
        xpActivated = true;
        sonCoroutine = sonPhoto;
        timeRepeatCoroutine = 25;
        HoloCoroutine.StartCoroutine(RepeatSonPhotoCoroutine);
        

        yield return HoloCoroutine.WaitUntil(() => (firstTutoDone));
        yield return HoloCoroutine.WaitForSeconds(4);
        sonPhoto.enabled = false;
        firstXpTuto2.transform.position = HoloCamera.mainCamera.transform.position + HoloCamera.mainCamera.transform.forward * 1f;
        firstXpTuto2.transform.position.y = HoloCamera.mainCamera.transform.position.y;
        firstXpTuto2.transform.LookAt(HoloCamera.mainCamera.transform.position);
        firstXpTuto2.SetActive(true);
        yield return HoloCoroutine.WaitForSeconds(1);
        sonTableau.enabled = true;
        sonCoroutine = sonTableau;
        timeRepeatCoroutine = 25;
        HoloCoroutine.StartCoroutine(RepeatSonTableauCoroutine);

        yield return HoloCoroutine.WaitUntil(() => (secondTutoDone));
        sonTableau.enabled = false;

        yield return HoloCoroutine.WaitUntil(() => (canLaunchEndTuto));
        yield return HoloCoroutine.WaitForSeconds(10);
        sonAutreChoix.enabled = true;
    }

    IEnumerator RepeatSonPhotoCoroutine()
    {
        yield return HoloCoroutine.WaitForSeconds(timeRepeatCoroutine);
        if (!firstTutoDone)
        {
            sonCoroutine.enabled = false;
            sonCoroutine.enabled = true;
            HoloCoroutine.StartCoroutine(RepeatSonPhotoCoroutine);
        }
    }

    IEnumerator RepeatSonTableauCoroutine()
    {
        yield return HoloCoroutine.WaitForSeconds(timeRepeatCoroutine);
        if (!secondTutoDone)
        {
            sonCoroutine.enabled = false;
            sonCoroutine.enabled = true;
            HoloCoroutine.StartCoroutine(RepeatSonTableauCoroutine);
        }
    }

    public void HideSecondTuto()
    {
        firstXpTuto2.SetActive(false);
    }

    private void SetActiveTableaux(bool _bool)
    {
        for (int i = 0; i < gazeComponents.Count; i++)
        {
            gazeComponents[i].GameObject.SetActive(_bool);
        }
    }
}