using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Grpc.Core;
using TransferPoints;
using static FastIKFabric;

public class csClientGetter : MonoBehaviour
{
    //establish connection to server (run from pyServer.py)
    Channel channel = new Channel("localhost:50051", ChannelCredentials.Insecure);

    //create list of GameObjects, angles, and distances matching rightHand
    public GameObject[] rightHand;
    public GameObject cameraObject;
    List<Vector3> pointsVector3List = new List<Vector3>();
    List<Vector3> angles = new List<Vector3>();
    List<Vector3> distances = new List<Vector3>();

    //distance of camera from avatar
    public int dist = 1;

    //create array of x y and z mod directionals
    int[] xyzMods = { 1, 1, 1 };

    //create array to store all cubes for right hand
    GameObject[] rightHandCubes = new GameObject[21];

    //create storage variable for IK
    FastIKFabric ik;


    // Start is called before the first frame update
    //sets camera and creates cubes for finger tracking
    void Start()
    {

        //set camera position and rotation to look towards positive z axis
        //this allows xyz values from PointSender to be added into Unity easily and correctly
        //when camera is used as an offset so they appear in front of the camera
        cameraObject.transform.position = new Vector3(-1, 1, -1);
        cameraObject.transform.LookAt(new Vector3(-1, 1, 0));

        //create 21 cubes for hand positionals whose color matches cvHands coloration
        for (int i = 0; i < 21; i++)
        {
            rightHandCubes[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightHandCubes[i].transform.localScale = new Vector3(.01f, .01f, .01f);
            rightHandCubes[i].name = "Cube ";

            if (i == 0)
            {
                rightHandCubes[i].GetComponent<MeshRenderer>().material.color = new Color(128 / 255f, 128 / 255f, 128 / 255f, 255 / 255f);
                rightHandCubes[i].name += "Wrist" + " (" + i + ")";
            }
            else if (i < 5)
            {
                rightHandCubes[i].GetComponent<MeshRenderer>().material.color = new Color(225 / 255f, 229 / 255f, 180 / 255f, 255 / 255f);
                rightHandCubes[i].name += "Thumb " + (((i - 1) % 4) + 1) + " (" + i + ")";
            }
            else if (i < 9)
            {
                rightHandCubes[i].GetComponent<MeshRenderer>().material.color = new Color(128 / 255f, 64 / 255f, 128 / 255f, 255 / 255f);
                rightHandCubes[i].name += "Index " + (((i - 1) % 4) + 1) + " (" + i + ")";
            }
            else if (i < 13)
            {
                rightHandCubes[i].GetComponent<MeshRenderer>().material.color = new Color(225 / 255f, 204 / 255f, 0 / 255f, 255 / 255f);
                rightHandCubes[i].name += "Middle " + (((i - 1) % 4) + 1) + " (" + i + ")";
            }
            else if (i < 17)
            {
                rightHandCubes[i].GetComponent<MeshRenderer>().material.color = new Color(48 / 255f, 255 / 255f, 48 / 255f, 255 / 255f);
                rightHandCubes[i].name += "Ring " + (((i - 1) % 4) + 1) + " (" + i + ")";
            }
            else
            {
                rightHandCubes[i].GetComponent<MeshRenderer>().material.color = new Color(21 / 255f, 101 / 255f, 192 / 255f, 255 / 255f);
                rightHandCubes[i].name += "Pinky " + (((i - 1) % 4) + 1) + " (" + i + ")";
            }
        }

        //attach IK's to every point (except wrist: i=0) in rightHand
        //TESTING: SET AT 5 TO 9
        for(int i = 1; i < 21; i++)
        {
            ik = rightHand[i].AddComponent<FastIKFabric>();
            ik.ChainLength = 1;
            ik.Target = rightHandCubes[i].transform;
        }
        
    }

    // Update is called once per frame and calls via client/server grpc to get points
    //also sets up cube positions for LateUpdate() to resolve Inverse Kinematics
    void Update()
    {
        //call server as client to get new points as AllPoints in reply
        var client = new PointSender.PointSenderClient(channel);
        var reply = client.GetPoints(new dummyPoint { });

        //get positional data from camera input
        for (int i = 0; i < 21; i++)
        {
            pointsVector3List.Add(new Vector3(-reply.Vec[i].X, -reply.Vec[i].Y, reply.Vec[i].Z));
        }

        //perform updates per function
        UpdateCubesPosition();
        //UpdateRightHand();

        //clear lists
        //necessary so all lists can iterate through from the beginning
        pointsVector3List.Clear();
        angles.Clear();

    }

    //Update rightHand to match rightHandCubes position
    //DELETE/CHANGE AFTER INVERSE KINEMATICS IS FINISHED
    void UpdateRightHand()
    {
        for (int i = 0; i < 21; i++)
            rightHand[i].transform.position = rightHandCubes[i].transform.position;
    }


    //function to be called from Update()
    //updates placement cubes with points
    void UpdateCubesPosition()
    {
        //FOR TESTING ONLING
        //set wrist to origin
        rightHandCubes[0].transform.position = rightHand[0].transform.position;

        //get xyzModifiers for directionals and then set targetCube destinations for each cube

        //set knuckles
        
        xyzMods = getxyzMods(xyzMods, pointsVector3List[0], pointsVector3List[5]);
        rightHandCubes[5].transform.position = newPosition(xyzMods[0], xyzMods[1], xyzMods[2], rightHandCubes[0].transform.position, rightHandCubes[5].transform.position, pointsVector3List[0], pointsVector3List[5], rightHand[0].transform.position, rightHand[5].transform.position);
        xyzMods = getxyzMods(xyzMods, pointsVector3List[0], pointsVector3List[9]);
        rightHandCubes[9].transform.position = newPosition(xyzMods[0], xyzMods[1], xyzMods[2], rightHandCubes[0].transform.position, rightHandCubes[9].transform.position, pointsVector3List[0], pointsVector3List[9], rightHand[0].transform.position, rightHand[9].transform.position);
        xyzMods = getxyzMods(xyzMods, pointsVector3List[0], pointsVector3List[13]);
        rightHandCubes[13].transform.position = newPosition(xyzMods[0], xyzMods[1], xyzMods[2], rightHandCubes[0].transform.position, rightHandCubes[13].transform.position, pointsVector3List[0], pointsVector3List[13], rightHand[0].transform.position, rightHand[13].transform.position);
        xyzMods = getxyzMods(xyzMods, pointsVector3List[0], pointsVector3List[17]);
        rightHandCubes[17].transform.position = newPosition(xyzMods[0], xyzMods[1], xyzMods[2], rightHandCubes[0].transform.position, rightHandCubes[17].transform.position, pointsVector3List[0], pointsVector3List[17], rightHand[0].transform.position, rightHand[17].transform.position);
        xyzMods = getxyzMods(xyzMods, pointsVector3List[0], pointsVector3List[1]);
        rightHandCubes[1].transform.position = newPosition(xyzMods[0], xyzMods[1], xyzMods[2], rightHandCubes[0].transform.position, rightHandCubes[1].transform.position, pointsVector3List[0], pointsVector3List[1], rightHand[0].transform.position, rightHand[1].transform.position);
        
        //set each individual finger

        //set index finger
        rightHandCubes[5].transform.position = rightHand[5].transform.position;
        for (int i = 6; i <= 8; i++)
        {
            xyzMods = getxyzMods(xyzMods, pointsVector3List[i - 1], pointsVector3List[i]);
            rightHandCubes[i].transform.position = newPosition(xyzMods[0], xyzMods[1], xyzMods[2], rightHandCubes[i - 1].transform.position, rightHandCubes[i].transform.position, pointsVector3List[i - 1], pointsVector3List[i], rightHand[i - 1].transform.position, rightHand[i].transform.position);
        }

        
        //set middle finger
        for (int i = 10; i <= 12; i++)
        {
            xyzMods = getxyzMods(xyzMods, pointsVector3List[i - 1], pointsVector3List[i]);
            rightHandCubes[i].transform.position = newPosition(xyzMods[0], xyzMods[1], xyzMods[2], rightHandCubes[i - 1].transform.position, rightHandCubes[i].transform.position, pointsVector3List[i - 1], pointsVector3List[i], rightHand[i - 1].transform.position, rightHand[i].transform.position);
        }

        //set ring finger
        for (int i = 14; i <= 16; i++)
        {
            xyzMods = getxyzMods(xyzMods, pointsVector3List[i - 1], pointsVector3List[i]);
            rightHandCubes[i].transform.position = newPosition(xyzMods[0], xyzMods[1], xyzMods[2], rightHandCubes[i - 1].transform.position, rightHandCubes[i].transform.position, pointsVector3List[i - 1], pointsVector3List[i], rightHand[i - 1].transform.position, rightHand[i].transform.position);
        }

        //set pinky finger
        for (int i = 18; i <= 20; i++)
        {
            xyzMods = getxyzMods(xyzMods, pointsVector3List[i - 1], pointsVector3List[i]);
            rightHandCubes[i].transform.position = newPosition(xyzMods[0], xyzMods[1], xyzMods[2], rightHandCubes[i - 1].transform.position, rightHandCubes[i].transform.position, pointsVector3List[i - 1], pointsVector3List[i], rightHand[i - 1].transform.position, rightHand[i].transform.position);
        }
        
        //set thumb
        for (int i = 2; i <= 4; i++)
        {
            xyzMods = getxyzMods(xyzMods, pointsVector3List[i - 1], pointsVector3List[i]);
            rightHandCubes[i].transform.position = newPosition(xyzMods[0], xyzMods[1], xyzMods[2], rightHandCubes[i - 1].transform.position, rightHandCubes[i].transform.position, pointsVector3List[i - 1], pointsVector3List[i], rightHand[i - 1].transform.position, rightHand[i].transform.position);
        }
        
    }

    //calculates new position of placement cube based off of distance from previous placement cube, location in AR display, and True Distance of armature bone (length between points of articulation).
    Vector3 newPosition(int xMod, int yMod, int zMod, Vector3 originCube, Vector3 targetCube, Vector3 originPoint, Vector3 targetPoint, Vector3 trueHandOrigin, Vector3 trueHandTarget)
    {
        //create variables that comprise the return Vector that is the new position requested
        var returnX = 0f;
        var returnY = 0f;
        var returnZ = 0f;

        //create trueLengthVar variable and calculate it
        var trueLengthVar = getTrueLengthVar(originPoint, targetPoint, trueHandOrigin, trueHandTarget);

        //perform calculation to get xyz coords
        returnX = originCube.x + (xMod * trueLengthVar * Mathf.Abs(targetPoint.x - originPoint.x));
        returnY = originCube.y + (yMod * trueLengthVar * Mathf.Abs(targetPoint.y - originPoint.y));
        returnZ = originCube.z + (zMod * trueLengthVar * Mathf.Abs(targetPoint.z - originPoint.z));

        //return new position
        return (new Vector3(returnX, returnY, returnZ));
    }

    //calculate trueLengthVariable based off of two points, with the idea of:
    //ab * trueLengthVar = truea*trueb
    float getTrueLengthVar(Vector3 a, Vector3 b, Vector3 truea, Vector3 trueb)
    {
        var length = Vector3.Distance(a, b);
        var trueLength = Vector3.Distance(truea, trueb);
        return (trueLength / length);
    }

    //function to determine modifiers of xyz coord directionals
    int[] getxyzMods(int[] xyzMod, Vector3 origin, Vector3 target)
    {
        if (origin.x < target.x)
            xyzMod[0] = 1;
        else
            xyzMod[0] = -1;
        if (origin.y < target.y)
            xyzMod[1] = 1;
        else
            xyzMod[1] = -1;
        if (origin.z < target.z)
            xyzMod[2] = 1;
        else
            xyzMod[2] = -1;

        return xyzMod;
    }
}
