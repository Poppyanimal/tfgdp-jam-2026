using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;

public class CharacteraMouthCameraSnap : MonoBehaviour
{
    //IGNORE, IDEA FAILED, CAUSE ?
    //
    //
    public GameObject mouthBone, eyeBoneL, eyeBoneR;
    public float mouthrotend = 110f, mouthdeadzone = 15f;
    Quaternion startingLocalRotation; bool collectedStartingLocation = false;

    CinemachineBrain brain;

    void Start()
    {
        brain = FindFirstObjectByType<CinemachineBrain>();
    }

    void Update()
    {
        if(!collectedStartingLocation)
        {
            Debug.Log("collecting starting location");
            startingLocalRotation = mouthBone.transform.localRotation;
            collectedStartingLocation = true;
        }
        
        Debug.Log("before angle: "+ mouthBone.transform.rotation.y);

        mouthBone.transform.localRotation = startingLocalRotation;
        float startY = mouthBone.transform.eulerAngles.y;
        float camY = ((CinemachineCamera)brain.ActiveVirtualCamera).transform.eulerAngles.y;

        float yChange = camY - startY;


        Debug.Log("bweh angle: "+ mouthBone.transform.rotation.y);


        /*if(yChange <= mouthdeadzone && yChange >= -mouthdeadzone)
        {
            yChange = 0;
        }
        else
        {
            if(yChange < 0)
                yChange += mouthdeadzone;
            else
                yChange -= mouthdeadzone;
        }

        if(yChange > mouthrotend)
            yChange = mouthrotend;
        if(yChange < -mouthrotend)
            yChange = -mouthrotend;*/

        Vector3 rot = mouthBone.transform.rotation.eulerAngles;
        rot.y += yChange;
        mouthBone.transform.rotation = quaternion.Euler(rot);
        Debug.Log("new angle: "+ mouthBone.transform.rotation.y);
        Debug.Log("starting loc:"+startingLocalRotation);
    }
}
