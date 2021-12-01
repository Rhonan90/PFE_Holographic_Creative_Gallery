using Holo;
using System.Collections.Generic;


public class PhotoCaptureBehaviour : HoloBehaviour
{
    [SharedAnimatorComponent]
    public SharedAnimatorComponent animatorComponent;
    [GazeComponent]
    public GazeComponent gazeComponent;

    [PhotoCaptureComponent]
    public PhotoCaptureComponent photoCaptureComponent;

    public HoloGameObject target;
    //public HoloGameObject label;

    private bool isRed = false;
    bool inProgress = false;


    public override void Start()
    {
        gazeComponent.OnGazeEvent += OnGazeEvent;
        //UpdateLabel("Ready");
        SetColor(isRed);
    }

    public override void OnDestroy()
    {
        gazeComponent.OnGazeEvent -= OnGazeEvent;
    }

    public void OnGazeEvent(GazeComponent _component, GazeEvent _event)
    {
        if (_event == GazeEvent.OnTap)
        {
            if (inProgress)
            {
                Log("Capture already in progress");
                return;
            }
            SetColor(!isRed);
            //UpdateLabel("Taking picture");
            inProgress = true;
            photoCaptureComponent.TakePicture(OnPhotoTaken);
        }
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


        //UpdateLabel("Saving photo");

        Log("Saving photo to " + path);
        photoCaptureComponent.SavePhotoIDToFile(_id, path, (saveSuccess, saveId) =>
        {
            if (!saveSuccess)
            {
                inProgress = false;
                //UpdateLabel("Failed to save photo");
                return;

            }
            photoCaptureComponent.ReleasePhotoID(_id);

            Log("Uploading to " + url);
            //UpdateLabel("Uploading photo");
            HTTPHelper.SendFile(url, new Dictionary<string, string>(), path, (_sendSuccess, _sendResult) =>
            {
                if (_sendSuccess)
                {
                    Log("Set texture from file " + url);
                    //UpdateLabel("Donwloading process photo");
                    target.GetHoloElementInChildren<HoloRenderer>().material.SetTextureFromUrl(url, (_width, _height) =>
                    {
                    if (_width == 0)
                    {
                        Error("Could not set texture");
                        //UpdateLabel("Failed to download");
                    }
                    else
                    {
                        Log("Texture loaded " + _width + "x" + _height);
                        target.transform.localScale = new HoloVector3( (float)_width / 500f, (float) _height / 500f,1f);
                            //UpdateLabel("Done!");

                        }
                        inProgress = false;
                    });
                }
                else
                {
                    Error("Could not send file " + url);
                    //UpdateLabel("Failed to upload");
                    inProgress = false;
                }

            });
        });

    }

    private void SetColor(bool _value)
    {
        isRed = _value;
        //animatorComponent.SetBoolParameter("TurnRed", isRed);
    }


    void UpdateLabel(string _message)
    {
        Log(_message);
        //label.GetHoloElement<HoloText>().text = _message;

    }
}