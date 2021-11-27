using Holo;
using System.Collections.Generic;

public class CubeBehaviour : HoloBehaviour
{
    [SharedAnimatorComponent]
    public SharedAnimatorComponent animatorComponent;
    [GazeComponent]
    public GazeComponent gazeComponent;

    [PhotoCaptureComponent]
    public PhotoCaptureComponent photoCaptureComponent;

    public HoloGameObject target;

    private bool isRed = false;

    public override void Start()
    {
        gazeComponent.OnGazeEvent += OnGazeEvent;

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
            SetColor(!isRed);
            photoCaptureComponent.TakePicture(OnPhotoTaken);
        }
    }

    void OnPhotoTaken(bool _success, int _id)
    {
        if (!_success)
        {
            Error("Failed to take photo");
            return;
        }

        string filename = "Photo_" + _id + ".png";
        string path = PathHelper.Combine("U:/Users/holof/Pictures/", filename); //pb
        Log("path to save = " + path);

        // Run the simple server in Server/CameraCaptureServer.py  IF server is hosted locally
        //string url = "http://"+NetworkHelper.GetHoloSceneServerIP()+":8000/" + filename;  
        
        // IF server is NOT hoster locally
        string serverOfHostIp = "192.168.43.186";
        string url = "http://" + serverOfHostIp + ":8000/" + filename;  


        Log("url to upload image = " + url);

        photoCaptureComponent.SavePhotoIDToFile(_id, path, (saveSuccess, saveId) =>
        {
            photoCaptureComponent.ReleasePhotoID(_id);
            
            HTTPHelper.SendFile(url, new Dictionary<string, string>(), path, (_sendSuccess, _sendResult) =>
            {
                if (_sendSuccess)
                {
                    Log("Set texture from file " + url);
                    target.GetHoloElementInChildren<HoloRenderer>().material.SetTextureFromUrl(url, (_width, _height) =>
                    {
                        if (_width == 0)

                        {
                            Error("Could not set texture");
                        }
                        else
                        {
                            Log("Texture loaded " + _width + "x" + _height);
                        }
                    });
                }
                else
                {
                    Error("Could not send file " + url);
                }
            });


        });
    }

    private void SetColor(bool _value)
    {
        isRed = _value;
        animatorComponent.SetBoolParameter("TurnRed", isRed);
    }
}