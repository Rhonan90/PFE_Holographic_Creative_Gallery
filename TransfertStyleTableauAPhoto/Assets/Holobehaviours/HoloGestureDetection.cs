using System.Collections;
using System.Collections.Generic;
using Holo;

public class HoloGestureDetection : HoloBehaviour
{
    // Start is called before the first frame update
    [GestureComponent]
    private GestureComponent gestureComp;

    public HoloGameObject picture;
    public HoloGameObject camera;


    public override void Start()
    {
        base.Start();

        gestureComp.RegisterPose(HandPose.HandOpenedSky);
        gestureComp.OnPoseStart += OnposeStarted;

        Log("Start");

    }

    private void OnposeStarted(HandPose handPose, Handness handness)
    {
        picture.SetActive(true);
        picture.transform.position = camera.transform.position + camera.transform.forward * 10;
    }
}
