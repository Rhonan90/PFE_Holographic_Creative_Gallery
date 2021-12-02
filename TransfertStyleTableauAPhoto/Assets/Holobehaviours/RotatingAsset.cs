using Holo;

public class RotatingAsset : HoloBehaviour
{
    public HoloTransform _Icon;
    public float _timeStep;
    public float _oneStepAngle;

    float _startTime;

    private HoloVector3 originalScale;

    public override void Awake()
    {
        _startTime = TimeHelper.currentTime;
        Async.OnUpdate += Async_OnUpdate;
        originalScale = _Icon.localScale;
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
        _Icon.localScale = new HoloVector3(originalScale.x * (1 + MathHelper.Sin(TimeHelper.currentTime) * 0.1f), originalScale.y * (1 + MathHelper.Sin(TimeHelper.currentTime) * 0.1f), originalScale.z * (1 + MathHelper.Sin(TimeHelper.currentTime) * 0.1f));
    }
}
