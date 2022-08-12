using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Grpc.Core;
using TransferPoints;

public class csClientGetterCOPY : MonoBehaviour
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

    public Transform originCube, targetCube1, targetCube2, targetCube3;
    Quaternion q1, q2, q3;

    // Start is called before the first frame update
    void Start()
    {
        //get original distances between points on avatar
        /*
        distances.Add(rightHand[1].transform.position - rightHand[0].transform.position); //0
        distances.Add(rightHand[2].transform.position - rightHand[1].transform.position); //1
        distances.Add(rightHand[3].transform.position - rightHand[2].transform.position); //2
        distances.Add(rightHand[4].transform.position - rightHand[3].transform.position); //3
        distances.Add(rightHand[5].transform.position - rightHand[0].transform.position); //4
        distances.Add(rightHand[6].transform.position - rightHand[5].transform.position); //5
        distances.Add(rightHand[7].transform.position - rightHand[6].transform.position); //6
        distances.Add(rightHand[8].transform.position - rightHand[7].transform.position); //7
        distances.Add(rightHand[9].transform.position - rightHand[0].transform.position); //8
        distances.Add(rightHand[10].transform.position - rightHand[9].transform.position); //9
        distances.Add(rightHand[11].transform.position - rightHand[10].transform.position); //10
        distances.Add(rightHand[12].transform.position - rightHand[11].transform.position); //11
        distances.Add(rightHand[13].transform.position - rightHand[0].transform.position); //12
        distances.Add(rightHand[14].transform.position - rightHand[13].transform.position); //13
        distances.Add(rightHand[15].transform.position - rightHand[14].transform.position); //14
        distances.Add(rightHand[16].transform.position - rightHand[15].transform.position); //15
        distances.Add(rightHand[17].transform.position - rightHand[0].transform.position); //16
        distances.Add(rightHand[18].transform.position - rightHand[17].transform.position); //17
        distances.Add(rightHand[19].transform.position - rightHand[18].transform.position); //18
        distances.Add(rightHand[20].transform.position - rightHand[19].transform.position); //19
        */

        //set camera position and rotation to look towards positive z axis
        //this allows xyz values from PointSender to be added into Unity easily and correctly
        //when camera is used as an offset so they appear in front of the camera
        cameraObject.transform.position = new Vector3(-1,1,-1);
        cameraObject.transform.LookAt(new Vector3(-1,1,0));

        //rightHand[5].transform.rotation = rightHand[5].transform.rotation * Quaternion.Euler(90, 0, 0);

        var lookDirection1 = targetCube1.position - originCube.position;
        var lookDirection2 = targetCube2.position - targetCube1.position;
        var lookDirection3 = targetCube3.position - targetCube2.position;

        var lookRotation1 = Quaternion.LookRotation(lookDirection1);
        var lookRotation2 = Quaternion.LookRotation(lookDirection2);
        var lookRotation3 = Quaternion.LookRotation(lookDirection3);

        q1 = lookRotation1;
        q2 = lookRotation2;
        q3 = lookRotation3;

    }

    // Update is called once per frame and calls via client/server grpc
    void Update()
    {
        //call server as client to get new points as AllPoints in reply
        var client = new PointSender.PointSenderClient(channel);
        var reply = client.GetPoints(new dummyPoint { });

        //get positional data from camera input
        for(int i = 0; i < 21; i++)
        {
            pointsVector3List.Add(new Vector3(reply.Vec[i].X, -reply.Vec[i].Y, reply.Vec[i].Z));
        }

        //perform updates per function
        UpdateRightHand();
        UpdateCubesPosition();

        //calculations for cube rotation changes
        var lookDirection1 = targetCube1.position - originCube.position;
        var lookDirection2 = targetCube2.position - targetCube1.position;
        var lookDirection3 = targetCube3.position - targetCube2.position;
        var lookRotation1 = Quaternion.LookRotation(lookDirection1);
        var lookRotation2 = Quaternion.LookRotation(lookDirection2);
        var lookRotation3 = Quaternion.LookRotation(lookDirection3);
        var diff1 = lookRotation1 * Quaternion.Inverse(q1);
        var diff2 = lookRotation2 * Quaternion.Inverse(q2);
        var diff3 = lookRotation3 * Quaternion.Inverse(q3);

        

        //TODO
        //rightHand.rotation can NOT be just set to cube.rotation
        //rightHand points with y axis, cube points with z axis
        //editing needed

        //add to original cube rotations based off of cube rotation change
        //mimic change in rightHand points

        originCube.rotation = Quaternion.Slerp(originCube.rotation, lookRotation1, 1);
        rightHand[5].transform.eulerAngles = new Vector3(originCube.eulerAngles.x, originCube.eulerAngles.y, rightHand[5].transform.eulerAngles.z);

        targetCube1.rotation = Quaternion.Slerp(targetCube1.rotation, lookRotation2, 1);
        //rightHand[6].transform.eulerAngles = new Vector3(rightHand[6].transform.eulerAngles.x, targetCube1.eulerAngles.y, rightHand[6].transform.eulerAngles.z);
        rightHand[6].transform.LookAt(targetCube2, rightHand[5].transform.up);

        targetCube2.rotation = Quaternion.Slerp(targetCube2.rotation, lookRotation3, 1);
        //rightHand[7].transform.eulerAngles = new Vector3(targetCube2.eulerAngles.x, rightHand[7].transform.eulerAngles.y, rightHand[7].transform.eulerAngles.z);
        rightHand[7].transform.LookAt(targetCube3, rightHand[6].transform.up);

        //targetCube3.rotation = targetCube2.rotation;
        //rightHand[8].transform.rotation = rightHand[7].transform.rotation;

        q1 = lookRotation1;
        q2 = lookRotation2;
        q3 = lookRotation3;

        //clear lists
        pointsVector3List.Clear();
        angles.Clear();

    }

    //function to be called from Update()
    //updates right hand with new points
    void UpdateRightHand()
    {
        //localize positions based off of camera offset
        //create camera offset so that all points appear 'dist' amount away from camera
        //.5 values are for normalized 0.0-1.0 values of x and y from PointSender
        Vector3 offset = cameraObject.transform.position + new Vector3(-0.5f, 0.5f, dist);


        /*
        //update rightHand positions according to camera points with offset
        rightHand[0].transform.position = pointsVector3List[0] + offset;
        rightHand[1].transform.position = pointsVector3List[1] + offset;
        rightHand[2].transform.position = pointsVector3List[2] + offset;
        rightHand[3].transform.position = pointsVector3List[3] + offset;
        rightHand[4].transform.position = pointsVector3List[4] + offset;
        rightHand[5].transform.position = pointsVector3List[5] + offset;
        rightHand[6].transform.position = pointsVector3List[6] + offset;
        rightHand[7].transform.position = pointsVector3List[7] + offset;
        rightHand[8].transform.position = pointsVector3List[8] + offset;
        rightHand[9].transform.position = pointsVector3List[9] + offset;
        rightHand[10].transform.position = pointsVector3List[10] + offset;
        rightHand[11].transform.position = pointsVector3List[11] + offset;
        rightHand[12].transform.position = pointsVector3List[12] + offset;
        rightHand[13].transform.position = pointsVector3List[13] + offset;
        rightHand[14].transform.position = pointsVector3List[14] + offset;
        rightHand[15].transform.position = pointsVector3List[15] + offset;
        rightHand[16].transform.position = pointsVector3List[16] + offset;
        rightHand[17].transform.position = pointsVector3List[17] + offset;
        rightHand[18].transform.position = pointsVector3List[18] + offset;
        rightHand[19].transform.position = pointsVector3List[19] + offset;
        rightHand[20].transform.position = pointsVector3List[20] + offset;
        */

        



    }

    //function to be called from Update()
    //updates placement cubes with points
    void UpdateCubesPosition()
    {
        //localize positions based off of camera offset
        //create camera offset so that all points appear 'dist' amount away from camera
        //.5 values are for normalized 0.0-1.0 values of x and y from PointSender
        Vector3 offset = cameraObject.transform.position + new Vector3(-0.5f, 0.5f, dist);

        //update positions of cubes based off of camera points with offset
        originCube.position = rightHand[5].transform.position;
        var xMod = 1;
        var yMod = 1;
        var zMod = 1;
        if (pointsVector3List[5].x < pointsVector3List[6].x)
            xMod = 1;
        else
            xMod = -1;
        if (pointsVector3List[5].y < pointsVector3List[6].y)
            yMod = 1;
        else
            yMod = -1;
        if (pointsVector3List[5].z < pointsVector3List[6].z)
            zMod = 1;
        else
            zMod = -1;
        targetCube1.position = newPosition(xMod, yMod, zMod, originCube.position, targetCube1.position, pointsVector3List[5], pointsVector3List[6], rightHand[5].transform.position, rightHand[6].transform.position);
        if (pointsVector3List[6].x < pointsVector3List[7].x)
            xMod = 1;
        else
            xMod = -1;
        if (pointsVector3List[6].y < pointsVector3List[7].y)
            yMod = 1;
        else
            yMod = -1;
        if (pointsVector3List[6].z < pointsVector3List[7].z)
            zMod = 1;
        else
            zMod = -1;
        targetCube2.position = newPosition(xMod, yMod, zMod, targetCube1.position, targetCube2.position, pointsVector3List[6], pointsVector3List[7], rightHand[6].transform.position, rightHand[7].transform.position);
        if (pointsVector3List[7].x < pointsVector3List[8].x)
            xMod = 1;
        else
            xMod = -1;
        if (pointsVector3List[7].y < pointsVector3List[8].y)
            yMod = 1;
        else
            yMod = -1;
        if (pointsVector3List[7].z < pointsVector3List[8].z)
            zMod = 1;
        else
            zMod = -1;
        targetCube3.position = newPosition(xMod, yMod, zMod, targetCube2.position, targetCube3.position, pointsVector3List[7], pointsVector3List[8], rightHand[7].transform.position, rightHand[8].transform.position);

    }

    Vector3 newPosition(int xMod, int yMod, int zMod, Vector3 originCube, Vector3 targetCube, Vector3 originPoint, Vector3 targetPoint, Vector3 trueHandOrigin, Vector3 trueHandTarget)
    {
        var returnX = 0f;
        var returnY = 0f; 
        var returnZ = 0f;
        var trueLengthVar = 0f;
        trueLengthVar = getTrueLengthVar(originPoint, targetPoint, trueHandOrigin, trueHandTarget);
        returnX = originCube.x + (xMod * trueLengthVar * Mathf.Abs(targetPoint.x - originPoint.x));
        returnY = originCube.y + (yMod * trueLengthVar * Mathf.Abs(targetPoint.y - originPoint.y));
        returnZ = originCube.z + (zMod * trueLengthVar * Mathf.Abs(targetPoint.z - originPoint.z));

        return (new Vector3(returnX, returnY, returnZ));
    }


    float getTrueLengthVar(Vector3 a, Vector3 b, Vector3 truea, Vector3 trueb)
    {
        var length = Vector3.Distance(a, b);
        var trueLength = Vector3.Distance(truea, trueb);
        return (trueLength / length);
    }
}
