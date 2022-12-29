using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class IKMovment : MonoBehaviour
{
    [System.Serializable]
    struct Leg
    {
        //Local space
        public Vector3 destination;
        public Vector3 root;
        public Vector3 desiredPos;


        [HideInInspector]
        public Vector3 desirePosInWorld;
        

        public float maxAcceptableLegDistanceFromDesire;

        public float[] bonesLength;

        [HideInInspector]
        public Transform[] segments;
        [HideInInspector]
        public Vector3[] bones;
        [HideInInspector]
        public float allLength;
    }

    [SerializeField]
    Transform spider;

    [SerializeField]
    GameObject segmentPrefab;

    [SerializeField]
    Leg[] legs;

    [SerializeField]
    float delta;
    [SerializeField]
    int iterations;


    [SerializeField]
    float speed;
    [SerializeField]
    float desireHeigth;

    Vector3 movmentAxis;

    private void Awake()
    {
        Innit();
    }

    private void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        movmentAxis = spider.forward * vertical + spider.right * horizontal;

        Vector3 input = spider.forward * vertical + spider.right * horizontal;
        input = input.magnitude > 1 ? input.normalized : input;
        spider.position += input * speed * Time.deltaTime;

        MoveLegsTargetPosition();
        MoveBodyTargetPosition();
        CalculateIK();
        AnimateLegs();
    }


    private void Innit()
    {
        for(int legNumber = 0; legNumber < legs.Length; legNumber++)
        {
            legs[legNumber].bones = new Vector3[legs[legNumber].bonesLength.Length + 1];
            legs[legNumber].segments = new Transform[legs[legNumber].bonesLength.Length];

            Vector3 dir = (legs[legNumber].destination - legs[legNumber].root).normalized;
            //Debug.Log(legs[legNumber].bones[legNumber]);
            legs[legNumber].bones[0] = legs[legNumber].root;
            Vector3 dist = Vector3.zero;

            for (int i = 1; i < legs[legNumber].bones.Length; i++)
            {
                legs[legNumber].bones[i] = legs[legNumber].root + dir * legs[legNumber].bonesLength[i - 1] + dist;
                dist += dir * legs[legNumber].bonesLength[i - 1];
            }
            legs[legNumber].allLength = dist.magnitude;

            for(int i = 0; i < legs[legNumber].bonesLength.Length; i++)
            {
                GameObject temp = Instantiate(segmentPrefab, transform.position, quaternion.identity);
                legs[legNumber].segments[i] = temp.transform;
            }
        }
        
    }
    private void MoveLegsTargetPosition()
    {
        for (int legNumber = 0; legNumber < legs.Length; legNumber++) // Obliczenia wykonywane s� dla ka�dej nogi osobno
        {
            Vector3 nextStep = (legs[legNumber].desiredPos + spider.position) - legs[legNumber].desirePosInWorld;
            nextStep = nextStep.normalized * legs[legNumber].maxAcceptableLegDistanceFromDesire;
            Vector3 normailzedMovment = new Vector3(Mathf.Abs(movmentAxis.x), Mathf.Abs(movmentAxis.y), Mathf.Abs(movmentAxis.z)).normalized;
            nextStep = legs[legNumber].desiredPos + Vector3.Scale(nextStep, normailzedMovment); ; //new Vector3(nextStep.x * normailzedMovment.x, nextStep.y * normailzedMovment.y, nextStep.z * normailzedMovment.z);
            nextStep = nextStep + spider.position;

            Debug.DrawLine(nextStep , nextStep + spider.up, Color.red);


            RaycastHit hit;
            Vector3 posFromCenterOfSphere = Vector3.positiveInfinity;
            if (Physics.Raycast(legs[legNumber].desiredPos + spider.position, spider.TransformDirection(Vector3.down), out hit, Mathf.Infinity))
            {
                posFromCenterOfSphere = hit.point;
            }

            if (posFromCenterOfSphere != Vector3.positiveInfinity)
            if (Physics.Raycast(nextStep, spider.TransformDirection(Vector3.down), out hit, Mathf.Infinity)) //legs[legNumber].desiredPos + spider.position
            {
                if (Vector3.Distance(new Vector3(legs[legNumber].desirePosInWorld.x, legs[legNumber].desirePosInWorld.z), new Vector3(posFromCenterOfSphere.x, posFromCenterOfSphere.z)) > legs[legNumber].maxAcceptableLegDistanceFromDesire || legs[legNumber].allLength < Vector3.Distance(legs[legNumber].root, legs[legNumber].desirePosInWorld - spider.position))
                {
                    legs[legNumber].desirePosInWorld = hit.point;
                }

            }
        }
    }

    private void MoveBodyTargetPosition()
    {
        Vector3 heigth = Vector3.zero;
        Vector3 rotation = Vector3.zero;

        for (int legNumber = 0; legNumber < legs.Length; legNumber++) // Obliczenia wykonywane s� dla ka�dej nogi osobno
        {
            Vector3 down = -spider.up;
            Vector3 myHeigth = Vector3.Project(spider.position - legs[legNumber].desirePosInWorld, spider.up);
            heigth += myHeigth;
            Debug.DrawLine(legs[legNumber].desirePosInWorld, myHeigth + legs[legNumber].desirePosInWorld, Color.magenta);

            #region rotatnion
            Vector3 firstLeg = legs[legNumber + 2 > legs.Length - 1 ? legNumber + 2 - legs.Length : legNumber + 2].desirePosInWorld;
            Vector3 secondLeg = legs[legNumber + 1 > legs.Length - 1 ? 0 : legNumber + 1].desirePosInWorld;
            Vector3 thirdLeg = legs[legNumber].desirePosInWorld;
            Vector3 vectorUp = Vector3.Cross(secondLeg - firstLeg, thirdLeg - firstLeg);
            rotation += vectorUp.normalized;

            //Debug.DrawLine(firstLeg, firstLeg + vectorUp.normalized * 1, Color.green);
            #endregion
        }

        heigth /= legs.Length;
        rotation = (rotation / legs.Length).normalized;
        float heightScalar = desireHeigth - heigth.magnitude;

        //Debug.DrawLine(spider.position, spider.position + rotation, Color.green);
        Debug.DrawLine(spider.position, spider.up * heightScalar + spider.position, Color.magenta);
        spider.position += spider.up * heightScalar;
        spider.up = rotation;
    }

    private void CalculateIK()
    {
        for (int legNumber = 0; legNumber < legs.Length; legNumber++) // Obliczenaia wykonywane s� dla ka�dej nogi osobno
        {
            //  Je�li odleg�o�� od celu jest wi�ksza ni� d�ugo�� nogi obliczamy kierunek w kt�rym powinna by� wyci�gni�ta a nast�pnie ustawiamy ko�ci prosto w kierunku celu z wyj�tkiem pierwszej, kt�ra jest rootem
            if (Vector3.Magnitude(legs[legNumber].destination - legs[legNumber].root) > legs[legNumber].allLength) 
            {
                Vector3 dir = (legs[legNumber].destination - legs[legNumber].root).normalized;
                Vector3 dist = Vector3.zero;

                for(int bone = 1; bone < legs[legNumber].bones.Length; bone++)
                {
                    legs[legNumber].bones[bone] = legs[legNumber].root + dir * legs[legNumber].bonesLength[bone - 1] + dist;
                    dist += dir * legs[legNumber].bonesLength[bone - 1];
                }

            }
            else
            {
                for (int iterationsCount = 0; iterationsCount < iterations; iterationsCount++)
                {
                    legs[legNumber].bones[legs[legNumber].bones.Length - 1] = legs[legNumber].desirePosInWorld - spider.position;

                    for(int boneIndex = legs[legNumber].bones.Length - 2; boneIndex >= 0; boneIndex--) //back
                    {
                        legs[legNumber].bones[boneIndex] = legs[legNumber].bones[boneIndex + 1] + (legs[legNumber].bones[boneIndex] - legs[legNumber].bones[boneIndex + 1]).normalized * legs[legNumber].bonesLength[boneIndex];
                    }

                    
                    legs[legNumber].bones[0] = legs[legNumber].root;

                    for (int boneIndex = 1; boneIndex < legs[legNumber].bones.Length - 1; boneIndex++) // forward
                    {
                        legs[legNumber].bones[boneIndex] = legs[legNumber].bones[boneIndex - 1] + (legs[legNumber].bones[boneIndex] - legs[legNumber].bones[boneIndex - 1]).normalized * legs[legNumber].bonesLength[boneIndex - 1];
                    }

                    if (Vector3.Distance(legs[legNumber].bones[legs[legNumber].bones.Length - 1], legs[legNumber].destination) < delta) break;

                }
            }
        }
    }

    private void AnimateLegs()
    {
        for (int legNumber = 0; legNumber < legs.Length; legNumber++)
        {
            for (int i = 0; i < legs[legNumber].segments.Length; i++)
            {
                legs[legNumber].segments[i].position = legs[legNumber].bones[i] + spider.position;
                legs[legNumber].segments[i].rotation = Quaternion.LookRotation((legs[legNumber].bones[i] - legs[legNumber].bones[i + 1]), Vector3.up);
            }
        }
    }

    private void OnDrawGizmos()
    {

        for (int legNumber = 0; legNumber < legs.Length; legNumber++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(legs[legNumber].root + spider.position, 0.2f);

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(legs[legNumber].destination + spider.position, 0.1f);
            
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(legs[legNumber].desirePosInWorld, 0.2f);

            Gizmos.color = Color.yellow;
            foreach (Vector3 joint in legs[legNumber].bones)
            {
                Gizmos.DrawSphere(joint + spider.position, 0.1f);
            }

            for (int i = 1; i < legs[legNumber].bones.Length; i++)
            {
                Gizmos.DrawLine(legs[legNumber].bones[i - 1] + spider.position, legs[legNumber].bones[i] + spider.position);
            }

            Gizmos.color = Color.cyan;

            RaycastHit hit;
            if (Physics.Raycast(legs[legNumber].desiredPos + spider.position, spider.TransformDirection(Vector3.down), out hit, Mathf.Infinity))
            {
                Gizmos.DrawLine(legs[legNumber].desiredPos + spider.position, hit.point);
                Gizmos.DrawWireSphere(hit.point, legs[legNumber].maxAcceptableLegDistanceFromDesire);

            }
        }

      

    }
}