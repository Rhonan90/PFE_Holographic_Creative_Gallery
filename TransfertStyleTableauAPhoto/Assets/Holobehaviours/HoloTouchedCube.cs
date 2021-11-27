using System.Collections;
using System.Collections.Generic;
using Holo;

public class HoloTouchedCube : HoloBehaviour
{
    [GestureComponent]
    private GestureComponent gestureComp;

    public override void Start()
    {
        base.Start();

        gestureComp.RegisterAllPose();
        gestureComp.OnPoseStart += OnposeStarted;

        Log("Start");

    }

    private void OnposeStarted(HandPose handPose, Handness handness)
    {
        HoloMeshRenderer holo = gameObject.GetHoloElement<HoloMeshRenderer>();
        holo.material.color = new HoloColor(0, 0, 0);
        Log("YO");
    }

}
