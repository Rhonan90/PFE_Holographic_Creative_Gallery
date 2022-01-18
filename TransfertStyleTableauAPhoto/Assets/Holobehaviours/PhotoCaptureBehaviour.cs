﻿using Holo;
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
    public HoloGameObject tableau4;
    private GazeComponent tableau1GazeComponent;
    private GazeComponent tableau2GazeComponent;
    private GazeComponent tableau3GazeComponent;
    private GazeComponent tableau4GazeComponent;

    private List<GazeComponent> gazeComponents;

    private Timer timer;
    private bool canGesture = false;

    private List<string> photosPath;
    private List<HoloGameObject> photos;
    private int? chosenPhotoIndex = null;
    
    private int? styleIndex; //?? not sure yet if int is the best indicator, mb style name better ?
    private string[] styles = { "mosaic", "rain-princess", "candy", "scream" };

    bool inProgress = false;

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
        if (xpActivated && canGesture)
        {
            if (handPose == HandPose.HandOpenedSky)
            {
                if (inProgress)
                {
                    Log("Already in progress");
                    return;
                }
                StyleChoice.SetActive(true);
                StyleChoice.transform.position = HoloCamera.mainCamera.transform.position + HoloCamera.mainCamera.transform.forward * 3;
                StyleChoice.transform.rotation = HoloCamera.mainCamera.transform.rotation;
                inProgress = true;

                timer.Start();
                canGesture = false;
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
                toile.GetHoloElementInChildren<HoloRenderer>().material.SetTextureFromUrl(url, (_width, _height) =>
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
                        //if (xpActivated)
                        //    picture.SetActive(true);
                        //picture.transform.position = LabelUI.transform.position;
                        //picture.transform.localScale = new HoloVector3((float)_width / 500f, (float)_height / 500f, 1f);
                        UpdateLabel("Done!");
                        LabelUI.SetActive(false);
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
            //((UnityEngine.GameObject) photos[i]).GetComponent<UnityEngine.UI.Outline>().enabled = false;
            photos[i].GetChildren().ForEach((outine) => { outine.SetActive(false); });
        }
        //((UnityEngine.GameObject)photos[index]).GetComponent<UnityEngine.UI.Outline>().enabled = true;
        photos[index].GetChildren().ForEach((outine) => { outine.SetActive(true); });
    }

    public void testFunc()
    {
        Log("test");
    }

    IEnumerator HideLabelAfterTimeCoroutine()
    {
        yield return HoloCoroutine.WaitForSeconds(3);
        LabelUI.SetActive(false);
    }
}