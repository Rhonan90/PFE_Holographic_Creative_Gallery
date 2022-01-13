using Holo;
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
    //private HoloGameObject camera;
    public HoloGameObject LabelUI;
    public HoloGameObject StyleChoice;
    public HoloGameObject Chevalet;
    public HoloGameObject photoViewer;
    public HoloGameObject imagePrefab;

    private bool xpActivated = false;

    public HoloGameObject tableau1;
    public HoloGameObject tableau2;
    public HoloGameObject tableau3;
    private GazeComponent tableau1GazeComponent;
    private GazeComponent tableau2GazeComponent;
    private GazeComponent tableau3GazeComponent;

    private List<GazeComponent> gazeComponents;

    private List<string> photos;
    private int? chosenPhotoIndex = null;
    
    private int? styleIndex; //?? not sure yet if int is the best indicator, mb style name better ?

    bool inProgress = false;

    public override void Start()
    {
        //gazeComponent.OnGazeEvent += OnGazeEvent;
        UpdateLabel("Ready");
        gestureComp.RegisterPose(HandPose.HandOpenedSky);
        gestureComp.RegisterPose(HandPose.IndexPinch);
        gestureComp.RegisterPose(HandPose.HandOpenedGround);
        gestureComp.OnPoseStart += OnposeStarted;



        //camera = camera.FindInScene("Camera");

        gazeComponents = new List<GazeComponent>();
        photos = new List<string>();

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
                    StyleChoice.SetActive(false);
                    inProgress = false;
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
                if (inProgress)
                {
                    Log("Already in progress");
                    return;
                }
                StyleChoice.SetActive(true);
                StyleChoice.transform.position = HoloCamera.mainCamera.transform.position + HoloCamera.mainCamera.transform.forward * 5;
                StyleChoice.transform.rotation = HoloCamera.mainCamera.transform.rotation;
                inProgress = true;
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
                sendPhotoToServer(chosenPhotoIndex.Value, styleIndex.Value);
            }

            if (handPose == HandPose.IndexPinch)
            {
                if (inProgress)
                {
                    Log("Already in progress");
                    return;
                }
                LaunchPhotoCapture();
            }
        }
    }

    public void LaunchPhotoCapture()
    {
        //StyleChoice.SetActive(false);
        UpdateLabel("Taking picture, don't move");

        showLabelUi();
        photoCaptureComponent.TakePicture(OnPhotoTaken);
    }

    void OnPhotoTaken(bool _success, int _id)
    {
        inProgress = true;
        if (!_success)
        {
            Error("Failed to take photo");
            inProgress = false;
            return;
        }


        string filename = "Photo_" + _id + ".png";
        string path = PathHelper.Combine(PathHelper.GetPersistentDataPath(), filename);
        //Log("path to save = " + path);
        photos.Add(path);
        chosenPhotoIndex = photos.Count - 1;

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


            HoloGameObject temp = imagePrefab.Duplicate(imagePrefab.transform.position,photoViewer);
            temp.SetActive(true);
            temp.GetHoloElement<HoloImage>().SetFile(path);
        });
        inProgress = false;

    }

    void sendPhotoToServer(int photoIndex, int styleIndex)
    {
        showLabelUi();
        inProgress = true;
        int lastSlashIndex = photos[photoIndex].LastIndexOf("/");
        string filename = photos[photoIndex].Substring(lastSlashIndex + 1);
        Log(filename);

        // Run the simple server in Server/CameraCaptureServer.py  IF server is hosted locally
        //string url = "http://"+NetworkHelper.GetHoloSceneServerIP()+":8000/" + filename;  
        //string url = "http://" + NetworkHelper.GetHoloSceneServerIP() + ":33900/" + filename;

        // IF server is NOT hoster locally
        string serverOfHostIp = "192.168.43.186";
        string url = "http://" + serverOfHostIp + ":33900/" + filename;

        Log("Uploading to " + url);
        UpdateLabel("Uploading photo");
        HTTPHelper.SendFile(url, new Dictionary<string, string>(), photos[photoIndex], (_sendSuccess, _sendResult) =>
        {
            if (_sendSuccess)
            {
                Log("Set texture from file " + url);
                UpdateLabel("Donwloading process photo");
                picture.GetHoloElementInChildren<HoloRenderer>().material.SetTextureFromUrl(url, (_width, _height) =>
                {
                    if (_width == 0)
                    {
                        Error("Could not set texture");
                        UpdateLabel("Failed to download");
                    }
                    else
                    {
                        Log("Texture loaded " + _width + "x" + _height);
                        if (xpActivated)
                            picture.SetActive(true);
                        //picture.transform.position = LabelUI.transform.position;
                        //picture.transform.localScale = new HoloVector3((float)_width / 500f, (float)_height / 500f, 1f);
                        UpdateLabel("Done!");
                        LabelUI.SetActive(false);
                        toile.GetHoloElementInChildren<HoloRenderer>().material = picture.GetHoloElementInChildren<HoloRenderer>().material;
                    }
                    inProgress = false;
                });
            }
            else
            {
                Error("Could not send file " + url);
                UpdateLabel("Failed to upload");
                inProgress = false;
            }
        });
    }


    void UpdateLabel(string _message)
    {
        Log(_message);
        LabelUI.FindInHierarchy("Label").GetHoloElement<HoloText>().text = _message;
    }

    void showLabelUi()
    {
        LabelUI.SetActive(true);
        LabelUI.transform.position = HoloCamera.mainCamera.transform.position + HoloCamera.mainCamera.transform.forward * 5;
    }

    public void StartXp()
    {
        xpActivated = true;
        Chevalet.SetActive(true);
    }

    public void EndXp()
    {
        xpActivated = false;
        LabelUI.SetActive(false);
        StyleChoice.SetActive(false);
        picture.SetActive(false);
        Chevalet.SetActive(false);
        inProgress = false;
    }

}