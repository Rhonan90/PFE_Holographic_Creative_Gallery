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
    public HoloGameObject StyleChoice;
    public HoloGameObject Chevalet;
    private HoloQuaternion chevaletInitialRotation;
    public HoloGameObject photoViewer;
    public HoloGameObject imagePrefab;

    private bool xpActivated = false;

    public HoloGameObject tableau1; private GazeComponent tableau1GazeComponent;
    public HoloGameObject tableau2; private GazeComponent tableau2GazeComponent;
    public HoloGameObject tableau3; private GazeComponent tableau3GazeComponent;
    public HoloGameObject tableau4; private GazeComponent tableau4GazeComponent;

    private List<GazeComponent> gazeComponents;

    private Timer timer;
    private bool canGesture = false;

    private List<string> photosPath;     //path des photos prises pour y accéder/les supprimer
    private List<HoloGameObject> photos;    //photos prises et affichées disponibles à la sélection
    private List<HoloGameObject> transformedPhotos;     //photos transformées récupérées depuis le serveur
    private int? chosenPhotoIndex = null;
    
    private int? styleIndex; //?? not sure yet if int is the best indicator, mb style name better ?
    private string[] styles = { "mosaic", "rain-princess", "candy", "scream" };

    bool inProgress = false;

    public HoloAudioSource sonIntro;
    public HoloAudioSource sonPhoto;
    public HoloAudioSource sonTableau;
    public HoloAudioSource sonRésultat;
    public HoloAudioSource sonPrisePhoto;
    public HoloAudioSource sonTransformationImage;


    public override void Start()
    {
        //gazeComponent.OnGazeEvent += OnGazeEvent;
        UpdateLabel("Ready");
        gestureComp.RegisterPose(HandPose.HandOpenedSky);
        gestureComp.RegisterPose(HandPose.TwoFingerUp);
        gestureComp.RegisterPose(HandPose.HandOpenedGround);
        gestureComp.OnPoseStart += OnposeStarted;



        //camera = camera.FindInScene("Camera");

        gazeComponents = new List<GazeComponent>();
        photosPath = new List<string>();
        photos = new List<HoloGameObject>();
        transformedPhotos = new List<HoloGameObject>();

        //Interaction choix de style 
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

        timer = new Timer(2,() => { canGesture = true; });
        timer.Start();
        canGesture = false;

        chevaletInitialRotation = Chevalet.transform.rotation;

        for (int i = 0; i < toileHolders.Count; i++)
        {
            HandlerHelper.SetHandlerEditable(toileHolders[i],true);
        }
    }

    public override void OnDestroy()
    {
        //gazeComponent.OnGazeEvent -= OnGazeEvent;
    }

    private void ScanGazeComponent_OnGazeEvent(GazeComponent _component, GazeEvent _event)
    {
        if (_event == GazeEvent.OnTap)
        {
            for (int i = 0; i < gazeComponents.Count; i++)
            {
                if (_component == gazeComponents[i])
                {
                    styleIndex = i;
                    //StyleChoice.SetActive(false);
                    //inProgress = false;
                }
            }
        }
    }


    private void OnposeStarted(HandPose handPose, Handness handness)
    {
        if (xpActivated && canGesture)
        {
            if (handPose == HandPose.HandOpenedSky)
            {
                if (inProgress)
                {
                    Log("Already in progress");
                    return;
                }
                //StyleChoice.SetActive(true);
                //StyleChoice.transform.position = HoloCamera.mainCamera.transform.position + HoloCamera.mainCamera.transform.forward * 3;
                //StyleChoice.transform.rotation = HoloCamera.mainCamera.transform.rotation;
                //inProgress = true;

                //timer.Start();
                //canGesture = false;
            }

            if (handPose == HandPose.HandOpenedGround)
            {
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
                if (styleIndex == null)
                {
                    Log("No style chosen yet");
                    return;
                }
                SendPhotoToServer(chosenPhotoIndex.Value, styleIndex.Value);


                timer.Start();
                canGesture = false;
            }

            if (handPose == HandPose.TwoFingerUp)
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


        string filename = "Photo_" + _id + ".png";
        string path = PathHelper.Combine(PathHelper.GetPersistentDataPath(), filename);
        //Log("path to save = " + path);
        photosPath.Add(path);
        chosenPhotoIndex = photosPath.Count - 1;

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
            


            HoloGameObject temp = imagePrefab.Duplicate(imagePrefab.transform.position,photoViewer,chosenPhotoIndex.Value.ToString());
            photos.Add(temp);
            SelectPicture(chosenPhotoIndex.Value);
            temp.SetActive(true);
            temp.GetHoloElement<HoloImage>().SetFile(path);
            
        });
        sonPrisePhoto.enabled = false;
        inProgress = false;

    }

    void SendPhotoToServer(int photoIndex, int styleIndex)
    {
        ShowLabelUi();
        inProgress = true;
        int lastSlashIndex = photosPath[photoIndex].LastIndexOf("\\");
        string filename = photosPath[photoIndex].Substring(lastSlashIndex + 1);
        string tempPath = photosPath[photoIndex].Substring(0, lastSlashIndex);
        tempPath = tempPath + styles[styleIndex] + "$" + filename;
        Log(filename);

        FileHelper.CopyFile(photosPath[photoIndex],tempPath);

        // Run command  ServerTF/CameraCaptureServer.py runServer  to start the server
        string serverOfHostIp = NetworkHelper.GetHoloSceneServerIP();  //192.168.43.186 maxime    192.168.43.112 Quentin     192.168.56.1 Paul  pour les tests en local sur la 4G de Paul
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
                        HoloCoroutine.StartCoroutine(PlayTransformSoundCoroutine);
                        HoloCoroutine.StartCoroutine(PlayInstructionEndCoroutine);
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
    }


    void UpdateLabel(string _message)
    {
        Log(_message);
        LabelUI.FindInHierarchy("Label").GetHoloElement<HoloText>().text = _message;
    }

    void ShowLabelUi()
    {
        LabelUI.SetActive(true);
        LabelUI.transform.position = HoloCamera.mainCamera.transform.position + HoloCamera.mainCamera.transform.forward * 3;
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
        StyleChoice.SetActive(true);
    }

    public void EndXp()
    {
        xpActivated = false;
        LabelUI.SetActive(false);
        StyleChoice.SetActive(false);
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

    public void TestFunc()
    {
        Log("test");
    }

    IEnumerator HideLabelAfterTimeCoroutine()
    {
        yield return HoloCoroutine.WaitForSeconds(3);
        LabelUI.SetActive(false);
    }

    public IEnumerator VocalInstructionsCoroutine()
    {
        sonIntro.enabled = true;
        yield return HoloCoroutine.WaitForSeconds(5);
        sonPhoto.enabled = true;
        yield return HoloCoroutine.WaitUntil(() => (chosenPhotoIndex!=null));
        sonTableau.enabled = true;
        yield return HoloCoroutine.WaitUntil(() => (styleIndex != null));
    }

    IEnumerator PlayTransformSoundCoroutine()
    {
        sonTransformationImage.enabled = true;
        yield return HoloCoroutine.WaitForSeconds(2);
        sonTransformationImage.enabled = false;
    }

    IEnumerator PlayInstructionEndCoroutine()
    {
        yield return HoloCoroutine.WaitForSeconds(2);
        sonRésultat.enabled = true;
    }
}