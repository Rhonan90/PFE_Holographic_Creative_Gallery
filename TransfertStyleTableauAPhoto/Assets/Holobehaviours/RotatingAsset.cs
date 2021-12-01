using Holo;

public class RotatingAsset : HoloBehaviour
{
    public HoloTransform _Icon;
    public float _timeStep;
    public float _oneStepAngle;

    float _startTime;


    public override void Awake()
    {
        _startTime = TimeHelper.currentTime;
        Async.OnUpdate += Async_OnUpdate;
    }

    private void Async_OnUpdate()
    {
        if(TimeHelper.currentTime - _startTime >= _timeStep)
        {
            HoloVector3 iconAngle = _Icon.localEulerAngles;
            iconAngle.z += _oneStepAngle;

            _Icon.localEulerAngles = iconAngle;

            _startTime = TimeHelper.currentTime;
        }
        //HoloVector3 iconAngle = _Icon.localEulerAngles;
        //iconAngle.z += _oneStepAngle;

        //_Icon.localEulerAngles = iconAngle;

        //_startTime = TimeHelper.currentTime;
    }
}
