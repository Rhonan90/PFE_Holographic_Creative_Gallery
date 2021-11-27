using System.Collections;
using System.Collections.Generic;
using Holo;

public class HoloChosePicture : HoloBehaviour
{
    public HoloGameObject choice1;
    public HoloGameObject choice2;

    public void ChosePicture()
    {
        if(this.gameObject.tag == "choice1")
        {
            choice1.SetActive(true);
        }
        else if(this.gameObject.tag == "choice2")
        {
            choice2.SetActive(true);
        }
        this.gameObject.transform.parent.gameObject.SetActive(false);
    }
}
