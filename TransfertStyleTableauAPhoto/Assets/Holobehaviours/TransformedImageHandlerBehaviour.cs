using System.Collections;
using System.Collections.Generic;
using Holo;


// Sert à remettre les images en mode Editable après être sorti du mode édition, sinon elles ne le sont plus
public class TransformedImageHandlerBehaviour : HoloBehaviour
{
    private GazeComponent imageGazeComponent;


    public override void Start()
    {
        imageGazeComponent = Engine.AddHoloComponent<GazeComponent>(nameof(imageGazeComponent));
        imageGazeComponent.attribute.GameObject = this.gameObject;
        imageGazeComponent.attribute.UseSnap = true;
        imageGazeComponent.OnGazeEvent += ScanGazeComponent_OnGazeEvent;

    }

    private void ScanGazeComponent_OnGazeEvent(GazeComponent _component, GazeEvent _event)
    {
        HandlerHelper.SetHandlerEditable(this.gameObject, true);
    }
}
