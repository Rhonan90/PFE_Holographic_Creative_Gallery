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
    private HoloGameObject camera;
    public HoloGameObject label;
    public HoloGameObject UI;
    public HoloGameObject StyleChoice;



    public HoloGameObject tableau1;
    public HoloGameObject tableau2;
    public HoloGameObject tableau3;
    private GazeComponent tableau1GazeComponent;
    private GazeComponent tableau2GazeComponent;
    private GazeComponent tableau3GazeComponent;


    private bool isRed = false;
    bool inProgress = false;


    public override void Start()
    {
        //gazeComponent.OnGazeEvent += OnGazeEvent;
        UpdateLabel("Ready");
        gestureComp.RegisterPose(HandPose.HandOpenedSky);
        gestureComp.OnPoseStart += OnposeStarted;

        camera = camera.FindInScene("Camera");

        //Interaction choix de style 
        tableau1GazeComponent = Engine.AddHoloComponent<GazeComponent>(nameof(tableau1GazeComponent));
        tableau1GazeComponent.attribute.GameObject = tableau1;
        tableau1GazeComponent.attribute.UseSnap = true;
        tableau1GazeComponent.OnGazeEvent += ScanGazeComponent_OnGazeEvent;

        tableau2GazeComponent = Engine.AddHoloComponent<GazeComponent>(nameof(tableau2GazeComponent));
        tableau2GazeComponent.attribute.GameObject = tableau2;
        tableau2GazeComponent.attribute.UseSnap = true;
        tableau2GazeComponent.OnGazeEvent += ScanGazeComponent_OnGazeEvent;

        tableau3GazeComponent = Engine.AddHoloComponent<GazeComponent>(nameof(tableau3GazeComponent));
        tableau3GazeComponent.attribute.GameObject = tableau3;
        tableau3GazeComponent.attribute.UseSnap = true;
        tableau3GazeComponent.OnGazeEvent += ScanGazeComponent_OnGazeEvent;
    }

    public override void OnDestroy()
    {
        //gazeComponent.OnGazeEvent -= OnGazeEvent;
    }

    private void ScanGazeComponent_OnGazeEvent(GazeComponent _component, GazeEvent _event)
    {
        if (_event == GazeEvent.OnTap)
        {
            if (_component == tableau1GazeComponent || _component == tableau2GazeComponent || _component == tableau3GazeComponent)
            {
                LaunchPhotoCapture();
                StyleChoice.SetActive(false);
            }
        }
    }

    //public void OnGazeEvent(GazeComponent _component, GazeEvent _event)
    //{
    //    if (_event == GazeEvent.OnTap)
    //    {
    //        if (inProgress)
    //        {
    //            Log("Capture already in progress");
    //            return;
    //        }
    //        SetColor(!isRed);
    //        //UpdateLabel("Taking picture");
    //        inProgress = true;
    //        photoCaptureComponent.TakePicture(OnPhotoTaken);
    //    }
    //}

    private void OnposeStarted(HandPose handPose, Handness handness)
    {
        if (inProgress)
        {
            Log("Capture already in progress");
            return;
        }
        StyleChoice.SetActive(true);
        StyleChoice.transform.position = camera.transform.position + camera.transform.forward * 5;
        StyleChoice.transform.rotation = camera.transform.rotation;
        Log("On passe par là");
    }

    public void LaunchPhotoCapture()
    {
        Log("On passe par ici");
        StyleChoice.SetActive(false);
        UpdateLabel("Taking picture, don't move");
        inProgress = true;

        UI.SetActive(true);
        UI.transform.position = camera.transform.position + camera.transform.forward * 5;
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
        //string path = PathHelper.Combine("U:/Users/holof/Pictures/", filename); //pb
        Log("path to save = " + path);

        // Run the simple server in Server/CameraCaptureServer.py  IF server is hosted locally
        //string url = "http://"+NetworkHelper.GetHoloSceneServerIP()+":8000/" + filename;  
        //string url = "http://" + NetworkHelper.GetHoloSceneServerIP() + ":33900/" + filename;

        // IF server is NOT hoster locally
        string serverOfHostIp = "192.168.43.186";
        string url = "http://" + serverOfHostIp + ":33900/" + filename;


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

            Log("Uploading to " + url);
            UpdateLabel("Uploading photo");
            HTTPHelper.SendFile(url, new Dictionary<string, string>(), path, (_sendSuccess, _sendResult) =>
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
                        picture.SetActive(true);
                        picture.transform.position = UI.transform.position;
                        picture.transform.localScale = new HoloVector3( (float)_width / 500f, (float) _height / 500f,1f);
                        UpdateLabel("Done!");
                        UI.SetActive(false);
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
        });

    }


    void UpdateLabel(string _message)
    {
        Log(_message);
        label.GetHoloElement<HoloText>().text = _message;

    }
}