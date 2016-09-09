using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;


//public enum HumanBodyBones
//{
//    Hips = 0,  //身体
//    RightUpperLeg = 1,  //右大腿
//    RightLowerLeg = 2,   //右膝盖
//    RightFoot = 3,   //右脚
//    LeftUpperLeg = 4,
//    LeftLowerLeg = 5,
//    LeftFoot = 6,
//    Spine = 7,  //脊椎
//    Spine1 = 8,  //胸腔1
//    Spine2 = 9,  //胸腔2
//    Spine3 = 10,  //胸腔3  //*
//    Neck = 11,   //颈
//    Head = 12,   //头
//    RightShoulder = 13,  //右肩
//    RightUpperArm = 14,  //右臂
//    RightLowerArm = 15,  //右前臂
//    RightHand = 16,  //右手
//    RightThumbProximal = 17,
//    RightThumbIntermediate = 18,
//    RightThumbDistal = 19,
//    RightInHandIndex = 20,  //*
//    RightIndexProximal = 21,
//    RightIndexIntermediate = 22,
//    RightIndexDistal = 23,
//    RightInHandMiddle = 24,  //*
//    RightMiddleProximal = 25,
//    RightMiddleIntermediate = 26,
//    RightMiddleDistal = 27,
//    RightInHandRing = 28,   //*
//    RightRingProximal = 29,
//    RightRingIntermediate = 30,
//    RightRingDistal = 31,
//    RightInHandPinky = 32,  //*
//    RightLittleProximal = 33,
//    RightLittleIntermediate = 34,
//    RightLittleDistal = 35,
//    LeftShoulder = 36,  //左肩
//    LeftUpperArm = 37,  //左臂
//    LeftLowerArm = 38,  //左前臂
//    LeftHand = 39,   //左手
//    LeftThumbProximal = 40,
//    LeftThumbIntermediate = 41,
//    LeftThumbDistal = 42,
//    LeftInHandIndex = 43,    //*
//    LeftIndexProximal = 44,
//    LeftIndexIntermediate = 45,
//    LeftIndexDistal = 46,
//    LeftInHandMiddle = 47,   //*
//    LeftMiddleProximal = 48,
//    LeftMiddleIntermediate = 49,
//    LeftMiddleDistal = 50,
//    LeftInHandRing = 51,    //*
//    LeftRingProximal = 52,
//    LeftRingIntermediate = 53,
//    LeftRingDistal = 54,
//    LeftInHandPinky = 55,   //*
//    LeftLittleProximal = 56,
//    LeftLittleIntermediate = 57,
//    LeftLittleDistal = 58,

//    NumOfBones

//}
////骨骼枚举值

public class UsbPort : MonoBehaviour
{
    #region
    //vector3_innfos m_vector;//一个模块包的三维向量
    //vector3_innfos[] m_vectors = new vector3_innfos[16];
    //vector3_innfos[] m_start = new vector3_innfos[16];
    //Vector3[] unity_vec = new Vector3[16];
    //Vector3[] unity_controler = new Vector3[16];
    public Animator m_ani;
    public Dictionary<HumanBodyBones, Quaternion> temp = new Dictionary<HumanBodyBones, Quaternion>();
    // 字典存储骨骼 与  sensor传感器所传四元数
    public SortedList<HumanBodyBones, Quaternion> temp2 = new SortedList<HumanBodyBones, Quaternion>();
    //  
    public Transform[] avatar;
    public Quate quat;//一个模块包的四元数  小宝的算法
    public Quate[] quats = new Quate[58];//全部模块的实时四元数  小宝的算法
    public Quate[] quatsStart = new Quate[58];
    //全部模块的初始四元数  小宝的算法
    // Quaternion[] q = new Quaternion[16];//模型可操作部位的初始四元数
   // Quaternion[] correct = new Quaternion[16];//模型可操作部位的相对四元数


    float[] bat = new float[16];//所有模块的电池信息，尚未使用
    short[] receivedPackage = new short[23];//处理一个模块数据包使用的临时变量
    byte[] RXBuff = new byte[1];//读缓冲区
    int packageDataBitCount;//模块数据包校验用计数器
    short[] package = new short[23];//接收到的一个模块数据包
    Thread dataReceiveThread;//线程
    bool bRead = true;//线程回调函数循环条件
    bool[] bResult = new bool[16];//全部模块初始位置是否已存储
                                  //public GameObject[] c;//全部模块对应的游戏对象，外部赋值
                                  //public GameObject[] a0;//0号模块的相对四元数，外部赋值，以下以此类推
                                  //public GameObject[] a1;
                                  //public GameObject[] a2;
                                  //public GameObject[] a3;
                                  //public GameObject[] a4;
                                  //public GameObject[] a5;
                                  //public GameObject[] a6;
                                  //public GameObject[] a7;
                                  //public GameObject[] a8;

    #endregion
    //初始化声明变量
    void Start()
    {   avatar = new Transform[58];
        avatar = GameObject.Find("unitychan").GetComponentsInChildren<Transform>();
     
        m_ani  = GameObject.Find("unitychan").GetComponent<Animator>();
        dataReceiveThread = new Thread(DataReceiveFunction);
        dataReceiveThread.IsBackground = true;
        dataReceiveThread.Start();
        /*      step(); */
       

    }
    public Quate normalize(Quate quat)//四元数归一化
    {
        quat = new Quate();
        float n = quat.x * quat.x + quat.y * quat.y + quat.z * quat.z + quat.w * quat.w;
        if (n == 1)
        {
            return quat;
        }
        n = 1.0f / Mathf.Sqrt(n);
        float sqrtn = Mathf.Sqrt(n);
        quat.x /= sqrtn; quat.y /= sqrtn;
        quat.z /= sqrtn; quat.w /= sqrtn;
        return quat;
    }
    // Use this for initialization  
    //获取初始传输数据
    //public Quate Start_Align(HumanBodyBones bone ) {
        
    //    Transform t = GetTransByName(bone.ToString());
      


    //    return quat;
        
    //}
    public Quaternion GetReceived(HumanBodyBones bone)
    {      for (int i = 0; i <avatar .Length; i++)
        {
            if (avatar[i].gameObject.name == bone.ToString())
            {
            
                quat.x =  avatar[i].transform.localRotation.x;
                quat.y =  avatar[i].transform.localRotation.y;
                quat.w =  avatar[i].transform.localRotation.w;
                quat.z =  avatar[i].transform.localRotation.z;
            }
        }

        int index = GetIndexByHumanBodyBones(bone);
        Quate n;
        Quaternion o;
        Quaternion p;
        Quaternion k = new Quaternion();
        Quaternion rot = new Quaternion();

        n = quats[index] / quatsStart[index];
        k.x = quat.x;
        k.x = quat.y;
        k.z = quat.z;
        k.w = quat.w;
        p.x = n.x;
        p.y = n.y;
        p.z = n.z;
        p.w = n.w;

        o.x = quats[index].x;
        o.y = quats[index].y;
        o.z = quats[index].z;
        o.w = quats[index].w;

        
        //i = o.eulerAngles;
        //i.z = i.z * -1f;
        //o = Quaternion.Euler(i);
  
        //g = p.eulerAngles;
      
        rot = o*p;
      

        //print(GetBoneIndex(bone.ToString()));
        //if (quats[index].x == 0 && quats[index].y == 0 && quats[index].z == 0 && quats[index].w == 0)
        //{   Transform t = GetTransByName(bone.ToString());
        //    return t.localRotation;

        //   }
        //else
        //{     // 尝试模块没有运动 返回原来的旋转
        // override object.GetHashCode 
        return rot;
        //}

    }
    //初始化姿态校正参数
    //模型初始化参数
    public void applymotion_rotation()
    {
        #region
        // apply rotations

        // legs
        Set_intance( HumanBodyBones.RightUpperLeg, GetReceived(HumanBodyBones.RightUpperLeg));
        Set_intance( HumanBodyBones.RightLowerLeg, GetReceived(HumanBodyBones.RightLowerLeg));
        Set_intance( HumanBodyBones.RightFoot, GetReceived(HumanBodyBones.RightFoot));
        Set_intance( HumanBodyBones.LeftUpperLeg, GetReceived(HumanBodyBones.LeftUpperLeg));
        Set_intance( HumanBodyBones.LeftLowerLeg, GetReceived(HumanBodyBones.LeftLowerLeg));
        Set_intance( HumanBodyBones.LeftFoot, GetReceived(HumanBodyBones.LeftFoot));

        // spine
        Set_intance( HumanBodyBones.Spine, GetReceived(HumanBodyBones.Spine));
        Set_intance( HumanBodyBones.Neck, GetReceived(HumanBodyBones.Neck));
        Set_intance( HumanBodyBones.Head, GetReceived(HumanBodyBones.Head));

        // right arm
        Set_intance( HumanBodyBones.RightShoulder, GetReceived(HumanBodyBones.RightShoulder));
        Set_intance( HumanBodyBones.RightUpperArm, GetReceived(HumanBodyBones.RightUpperArm));
        Set_intance( HumanBodyBones.RightLowerArm, GetReceived(HumanBodyBones.RightLowerArm));

        // right hand
        Set_intance( HumanBodyBones.RightHand, GetReceived(HumanBodyBones.RightHand));
        Set_intance( HumanBodyBones.RightThumbProximal, GetReceived(HumanBodyBones.RightThumbProximal));
        Set_intance( HumanBodyBones.RightThumbIntermediate, GetReceived(HumanBodyBones.RightThumbIntermediate));
        Set_intance( HumanBodyBones.RightThumbDistal, GetReceived(HumanBodyBones.RightThumbDistal));

        Set_intance(HumanBodyBones.RightIndexProximal, GetReceived(HumanBodyBones.RightIndexProximal));// * GetReceived(HumanBodyBones.RightInHandIndex));
        Set_intance( HumanBodyBones.RightIndexIntermediate, GetReceived(HumanBodyBones.RightIndexIntermediate));
        Set_intance( HumanBodyBones.RightIndexDistal, GetReceived(HumanBodyBones.RightIndexDistal));

        Set_intance( HumanBodyBones.RightMiddleProximal, GetReceived(HumanBodyBones.RightMiddleProximal));//* GetReceived(HumanBodyBones.RightInHandMiddle));
        Set_intance( HumanBodyBones.RightMiddleIntermediate, GetReceived(HumanBodyBones.RightMiddleIntermediate));
        Set_intance( HumanBodyBones.RightMiddleDistal, GetReceived(HumanBodyBones.RightMiddleDistal));

        Set_intance( HumanBodyBones.RightRingProximal, GetReceived(HumanBodyBones.RightRingProximal));// * GetReceived(HumanBodyBones.RightInHandRing));
        Set_intance( HumanBodyBones.RightRingIntermediate, GetReceived(HumanBodyBones.RightRingIntermediate));
        Set_intance(HumanBodyBones.RightRingDistal, GetReceived(HumanBodyBones.RightRingDistal));

        Set_intance( HumanBodyBones.RightLittleProximal, GetReceived(HumanBodyBones.RightLittleProximal));// * GetReceived(HumanBodyBones.RightInHandPinky));
        Set_intance( HumanBodyBones.RightLittleIntermediate, GetReceived(HumanBodyBones.RightLittleIntermediate));
        Set_intance( HumanBodyBones.RightLittleDistal, GetReceived(HumanBodyBones.RightLittleDistal));

        // left arm
        Set_intance( HumanBodyBones.LeftShoulder, GetReceived(HumanBodyBones.LeftShoulder));
        Set_intance(HumanBodyBones.LeftUpperArm, GetReceived(HumanBodyBones.LeftUpperArm));
        Set_intance( HumanBodyBones.LeftLowerArm, GetReceived(HumanBodyBones.LeftLowerArm));

        // left hand
        Set_intance( HumanBodyBones.LeftHand, GetReceived(HumanBodyBones.LeftHand));
        Set_intance( HumanBodyBones.LeftThumbProximal, GetReceived(HumanBodyBones.LeftThumbProximal));
        Set_intance(HumanBodyBones.LeftThumbIntermediate, GetReceived(HumanBodyBones.LeftThumbIntermediate));
        Set_intance( HumanBodyBones.LeftThumbDistal, GetReceived(HumanBodyBones.LeftThumbDistal));

        Set_intance( HumanBodyBones.LeftIndexProximal, GetReceived(HumanBodyBones.LeftIndexProximal));//* GetReceived(HumanBodyBones.LeftInHandIndex));
        Set_intance(HumanBodyBones.LeftIndexIntermediate, GetReceived(HumanBodyBones.LeftIndexIntermediate));
        Set_intance( HumanBodyBones.LeftIndexDistal, GetReceived(HumanBodyBones.LeftIndexDistal));

        Set_intance(HumanBodyBones.LeftMiddleProximal, GetReceived(HumanBodyBones.LeftMiddleProximal));// * GetReceived(HumanBodyBones.LeftInHandMiddle));
        Set_intance( HumanBodyBones.LeftMiddleIntermediate, GetReceived(HumanBodyBones.LeftMiddleIntermediate));
        Set_intance( HumanBodyBones.LeftMiddleDistal, GetReceived(HumanBodyBones.LeftMiddleDistal));

        Set_intance( HumanBodyBones.LeftRingProximal, GetReceived(HumanBodyBones.LeftRingProximal));// * GetReceived(HumanBodyBones.LeftInHandRing));
        Set_intance( HumanBodyBones.LeftRingIntermediate, GetReceived(HumanBodyBones.LeftRingIntermediate));
        Set_intance( HumanBodyBones.LeftRingDistal, GetReceived(HumanBodyBones.LeftRingDistal));

        Set_intance( HumanBodyBones.LeftLittleProximal, GetReceived(HumanBodyBones.LeftLittleProximal));// * GetReceived(HumanBodyBones.LeftInHandPinky));
        Set_intance( HumanBodyBones.LeftLittleIntermediate, GetReceived(HumanBodyBones.LeftLittleIntermediate));
        Set_intance( HumanBodyBones.LeftLittleDistal, GetReceived(HumanBodyBones.LeftLittleDistal));

        #endregion
    }
    //void rotate() { //为模型所有可操作部位赋值
    //    for (int i = 0; i < c.Length; ++i)
    //    {

    //        c[i].transform.rotation = quats[i] * q[i] * correct[i];
    //        //c[i].transform.position = (Vector3)m_vectors[i] + unity_vec[i]+unity_controler[i];
    //    }
    //    if (Input.GetKeyDown(KeyCode.Escape))
    //    {
    //        Application.Quit();
    //    }
    //}
    //线程回调函数
    //void step(){//保存模型可操作部位初始四元数，可以和相对旋转合并计算，但为便于调试暂单独存储
    //    for (int i = 0; i < c.Length; ++i)
    //    {
    //        q[i] = c[i].transform.localRotation;
    //        //unity_vec[i] = c[i].transform.localPosition;
    //    }
    //    //初始化所有模型可操作部位相对四元数的W值
    //    for (int i = 0; i < 16; ++i)
    //    {
    //        correct[i].w = 1;
    //    }
    //    //计算并保存所有模型可操作部位的相对四元数，当前使用9个模块
    //    for (int i = 0; i < a0.Length; ++i)
    //    {
    //        correct[0] *= a0[i].transform.localRotation;
    //        //unity_controler[0] = a0[i].transform.localPosition;
    //    }
    //    for (int i = 0; i < a1.Length; ++i)
    //    {
    //        correct[1] *= a1[i].transform.localRotation;
    //        //unity_controler[1] = a1[i].transform.localPosition;
    //    }
    //    for (int i = 0; i < a2.Length; ++i)
    //    {
    //        correct[2] *= a2[i].transform.localRotation;
    //        //unity_controler[2] += a2[i].transform.localPosition;
    //    }
    //    for (int i = 0; i < a3.Length; ++i)
    //    {
    //        correct[3] *= a3[i].transform.localRotation;
    //        //unity_controler[3] += a3[i].transform.localPosition;
    //    }
    //    for (int i = 0; i < a4.Length; ++i)
    //    {
    //        correct[4] *= a4[i].transform.localRotation;
    //        //unity_controler[4] += a4[i].transform.localPosition;
    //    }
    //    for (int i = 0; i < a5.Length; ++i)
    //    {
    //        correct[5] *= a5[i].transform.localRotation;
    //        //unity_controler[5] += a5[i].transform.localPosition;
    //    }
    //    for (int i = 0; i < a6.Length; ++i)
    //    {
    //        correct[6] *= a6[i].transform.localRotation;
    //        //unity_controler[6] += a6[i].transform.localPosition;
    //    }
    //    for (int i = 0; i < a7.Length; ++i)
    //    {
    //        correct[7] *= a7[i].transform.localRotation;
    //        //unity_controler[7] += a7[i].transform.localPosition;
    //    }
    //    for (int i = 0; i < a8.Length; ++i)
    //    {
    //        correct[8] *= a8[i].transform.localRotation;
    //        //unity_controler[8] += a8[i].transform.localPosition;
    //    }
    //}
    // Update is called once per frame


    //int GetIndexByHumanBodyBones(HumanBodyBones bone)
    //   #region  通过系统定义骨骼枚举获取其对应的index
    //   {
    //       int index = 0;
    //       if (GetBoneIndex(bone.ToString()) == 0)
    //       {
    //           index = 0;   //躯干
    //       }
    //       else if (GetBoneIndex(bone.ToString()) == 10)
    //       {
    //           index = 1;    //头
    //       }
    //       else if (GetBoneIndex(bone.ToString()) == 14)
    //       {
    //           index = 2;    //右肩
    //       }
    //       else if (GetBoneIndex(bone.ToString()) == 16)
    //       {
    //           index = 3;     //右前臂
    //       }
    //       else if (GetBoneIndex(bone.ToString()) == 18 || (GetBoneIndex(bone.ToString()) <= 53 && GetBoneIndex(bone.ToString()) >= 39))
    //       {
    //           index = 4;     //右手
    //       }
    //       else if (GetBoneIndex(bone.ToString()) == 13)
    //       {
    //           index = 5;   //左肩
    //       }
    //       else if (GetBoneIndex(bone.ToString()) == 15)
    //       {
    //           index = 6;    //左前臂
    //       }
    //       else if (GetBoneIndex(bone.ToString()) == 17 || (GetBoneIndex(bone.ToString()) >= 24 && GetBoneIndex(bone.ToString()) <= 38))
    //       {
    //           index = 7;    //左手
    //       }
    //       else
    //       {
    //           index = 8;    //剩余
    //       }
    //       return index;


    //   }
    //   #endregion


    //int GetIndexByHumanBodyBones(HumanBodyBones bone)
    //#region  通过系统定义骨骼枚举获取其对应的index
    //{
    //    int index = 0;
    //    if (GetBoneIndex(bone.ToString()) == 0)
    //    {
    //        index = 0;   //躯干
    //    }
    //    else if (GetBoneIndex(bone.ToString()) == 10)
    //    {
    //        index = 1;    //头
    //    }
    //    else if (GetBoneIndex(bone.ToString()) == 14)
    //    {
    //        index = 2;    //右肩
    //    }
    //    else if (GetBoneIndex(bone.ToString()) == 16)
    //    {
    //        index = 3;     //右前臂
    //    }
    //    else if (GetBoneIndex(bone.ToString()) == 18 || (GetBoneIndex(bone.ToString()) <= 53 && GetBoneIndex(bone.ToString()) >= 39))
    //    {
    //        index = 4;     //右手
    //    }
    //    else if (GetBoneIndex(bone.ToString()) == 13)
    //    {
    //        index = 5;   //左肩
    //    }
    //    else if (GetBoneIndex(bone.ToString()) == 15)
    //    {
    //        index = 6;    //左前臂
    //    }
    //    else if (GetBoneIndex(bone.ToString()) == 17 || (GetBoneIndex(bone.ToString()) >= 24 && GetBoneIndex(bone.ToString()) <= 38))
    //    {
    //        index = 7;    //左手
    //    }
    //    else
    //    {
    //        index = 8;    //剩余
    //    }
    //    return index;


    //}
    //#endregion
    //int GetIndexByHumanBodyBones(HumanBodyBones bone)
    //#region  通过系统定义骨骼枚举获取其对应的index
    //{
    //    int index = 0;
    //    if (GetBoneIndex(bone.ToString()) == 0)
    //    {
    //        index = 0;   //躯干
    //    }
    //    else if (GetBoneIndex(bone.ToString()) == 10)
    //    {
    //        index = 1;    //头
    //    }
    //    else if (GetBoneIndex(bone.ToString()) == 14)
    //    {
    //        index = 2;    //右肩
    //    }
    //    else if (GetBoneIndex(bone.ToString()) == 16)
    //    {
    //        index = 3;     //右前臂
    //    }
    //    else if (GetBoneIndex(bone.ToString()) == 18 || (GetBoneIndex(bone.ToString()) <= 53 && GetBoneIndex(bone.ToString()) >= 39))
    //    {
    //        index = 4;     //右手
    //    }
    //    else if (GetBoneIndex(bone.ToString()) == 13)
    //    {
    //        index = 5;   //左肩
    //    }
    //    else if (GetBoneIndex(bone.ToString()) == 15)
    //    {
    //        index = 6;    //左前臂
    //    }
    //    else if (GetBoneIndex(bone.ToString()) == 17 || (GetBoneIndex(bone.ToString()) >= 24 && GetBoneIndex(bone.ToString()) <= 38))
    //    {
    //        index = 7;    //左手
    //    }
    //    else
    //    {
    //        index = 8;    //剩余
    //    }
    //    return index;


    //}
    //#endregion



    int GetIndexByHumanBodyBones(HumanBodyBones bone)
    #region  通过系统定义骨骼枚举获取其对应的index
    {
        int index = 0;
        if (GetBoneIndex(bone.ToString()) == 0)
        {
            index = 0;   //躯干
        }
        else if (GetBoneIndex(bone.ToString()) == 10)
        {
            index = 1;    //头
        }
        else if (GetBoneIndex(bone.ToString()) == 14)
        {
            index = 2;    //右肩
        }
        else if (GetBoneIndex(bone.ToString()) == 16)
        {
            index = 3;     //右前臂
        }
        else if (GetBoneIndex(bone.ToString()) == 18 || (GetBoneIndex(bone.ToString()) <= 53 && GetBoneIndex(bone.ToString()) >= 39))
        {
            index = 4;     //右手
        }
        else if (GetBoneIndex(bone.ToString()) == 13)
        {
            index = 5;   //左肩
        }
        else if (GetBoneIndex(bone.ToString()) == 15)
        {
            index = 6;    //左前臂
        }
        else if (GetBoneIndex(bone.ToString()) == 17 || (GetBoneIndex(bone.ToString()) >= 24 && GetBoneIndex(bone.ToString()) <= 38))
        {
            index = 7;    //左手
        }
        else
        {
            index = 8;    //剩余
        }
        return index;


    }
    #endregion

    //int GetIndexByHumanBodyBones(HumanBodyBones bone)
    //#region  通过系统定义骨骼枚举获取其对应的index
    //{
    //    int index = 0;
    //    if (GetBoneIndex(bone.ToString()) == 39)
    //    {
    //        index = 1;   //拇指1
    //    }
    //    else if (GetBoneIndex(bone.ToString()) == 40)
    //    {
    //        index = 2;    //拇指2
    //    }
    //    else if (GetBoneIndex(bone.ToString()) == 41)
    //    {
    //        index = 3;    //拇指3
    //    }
    //    else if (GetBoneIndex(bone.ToString()) == 42)
    //    {
    //        index = 4;     //食指1
    //    }

    //    else if (GetBoneIndex(bone.ToString()) == 43)
    //    {
    //        index = 5;   //食指2
    //    }
    //    else if (GetBoneIndex(bone.ToString()) == 44)
    //    {
    //        index = 6;    //食指3
    //    }
    //    else if (GetBoneIndex(bone.ToString()) == 45)
    //    {
    //        index = 7;    //中指1
    //    }
    //    else if (GetBoneIndex(bone.ToString()) == 46)
    //    {
    //        index = 8;    //中指2
    //    }
    //    else if (GetBoneIndex(bone.ToString()) == 47)
    //    {
    //        index = 9;    //中指3
    //    }
    //    else if (GetBoneIndex(bone.ToString()) == 48)
    //    {
    //        index = 10;    //无名指1
    //    }
    //    else if (GetBoneIndex(bone.ToString()) == 49)
    //    {
    //        index = 11;    //无名指2
    //    }
    //    else if (GetBoneIndex(bone.ToString()) == 50)
    //    {
    //        index = 12;    //无名指3
    //    }
    //    else if (GetBoneIndex(bone.ToString()) == 51)
    //    {
    //        index = 13;    //小指1
    //    }
    //    else if (GetBoneIndex(bone.ToString()) == 52)
    //    {
    //        index = 14;    //小指2
    //    }
    //    else if (GetBoneIndex(bone.ToString()) == 53)
    //    {
    //        index = 15;    //小指2
    //    }




    //    else
    //    {
    //        index = 16;    //剩余
    //    }
    //    return index;
    //}
    //#endregion
    Transform GetTransByName(string Name)
    {                
        Transform[] arr = GameObject.Find("unitychan").GetComponentsInChildren<Transform>();
        for (int i = 0; i < arr.Length; i++)
        { if (arr[i].gameObject.name == Name)
            {   return arr[i];
            } }
        return null; }
    // 单例 获取模型骨骼  

    public void Set_intance( HumanBodyBones bone, Quaternion rotation)
    {
     
        Transform t = GetTransByName(bone.ToString());
        //Vector3 position;
 
        //position = rotation.eulerAngles;
      if (rotation.x != 0 && rotation.y != 0 && rotation.z != 0 && rotation.w != 0
     &&!float.IsNaN(rotation.x)&& !float.IsNaN(rotation.y)&&!float.IsNaN(rotation.z)&& !float.IsNaN(rotation.w))
        {
            t.localRotation = rotation;
        }
        //    if (float.IsInfinity(position.x)&&float.IsInfinity(position.y)&&float.IsInfinity(position.z))
        //    {
        //        t.localPosition = position;
        //    }
        //    t.localPosition = position;
        //}
        else
        {

            t.localRotation = t.localRotation;
            t.localPosition = t.localPosition;
        }
    }


    //单例骨骼旋转，移动赋值骨骼校准获取数据

    void Update()
    {
        //人体骨骼实时赋值 (update调用)
        applymotion_rotation();
    }
    public string GetBoneName(int id)
    {
        return Enum.GetName(typeof(HumanBodyBones), (HumanBodyBones)id);
    }
    public int GetBoneIndex(string name)
    {
        for (int i = 0; i < (int)HumanBodyBones.LastBone; ++i)
        {
            if (GetBoneName(i) == name)
            {
                return i;
            }
        }

        return -1;
    }

    //数据包处理函数
    public void ClosePort()
    {
        try
        {
            StartList.sp.Close();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }
    //关闭串口
    void OnApplicationQuit()
    {
        bRead = false;
        ClosePort();
    }
    //自定义四元数结构体
    public struct Quate
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public static implicit operator Quaternion(Quate v)
        {
            Quaternion q;
            q.x = v.x;
            q.y = v.y;
            q.z = v.z;
            q.w = v.w;
            return q;
        }
        public static Quate operator /(Quate q, Quaternion r)
        {
            Quate t;
            float temp = r.x * r.x + r.y * r.y + r.z * r.z + r.w * r.w;
            t.w = (r.w * q.w + r.x * q.x + r.y * q.y + r.z * q.z) / temp;
            t.x = (r.w * q.x - r.x * q.w - r.y * q.z + r.z * q.y) / temp;
            t.y = (r.w * q.y + r.x * q.z - r.y * q.w - r.z * q.x) / temp;
            t.z = (r.w * q.z - r.x * q.y + r.y * q.x - r.z * q.w) / temp;
            return t;
        }

    }
        //public struct vector3_innfos {
        //        public float x;
        //        public float y;
        //        public float z;

        //        public static  explicit  operator Vector3 (vector3_innfos  m) {
        //            Vector3 temp;
        //            temp.x = m.x;
        //            temp.y = m.y;
        //            temp.z = m.z;
        //            return temp;  }

        //        public static vector3_innfos operator +(vector3_innfos g , Vector3 e) {
        //            vector3_innfos gg;
        //            gg.x = g.x + e.x;
        //            gg.y = g.y + e.y;
        //            gg.z = g.z + e.z;

        //            return gg;
        //        }

        //    }
        //vector3_innfos nomorlize(vector3_innfos t)
        //    {
        //        float tangfei = t.x * t.x + t.y * t.y + t.z * t.z;
        //        //向量的模 
        //        if (tangfei == 1)
        //        {
        //            return t;
        //        }
        //        tangfei = 1 / Mathf.Sqrt(tangfei);
        //        float temp2 = Mathf.Sqrt(tangfei);
        //        t.x /= temp2;
        //        t.y /= temp2;
        //        t.z /= temp2;

        //        return t;
        //    }



    Quate[] getPackage(short[] package)
        {
            for (int j = 0; j < 16; ++j)
            {
                for (int i = 0; i < 23; ++i)
                {
                    receivedPackage[i] = package[i];
                }
                Quate qtemp;
                qtemp.x = qtemp.y = qtemp.z = qtemp.w = 0;
                quat.x = quat.y = quat.z = quat.w;
                float aaa = 0f;
                aaa = four_bytes(receivedPackage[3], receivedPackage[4], receivedPackage[5], receivedPackage[6]);
                qtemp.w = aaa * 1 / (1 << 30);
                aaa = four_bytes(receivedPackage[7], receivedPackage[8], receivedPackage[9], receivedPackage[10]);
                qtemp.x = aaa * 1 / (1 << 30);
                aaa = four_bytes(receivedPackage[11], receivedPackage[12], receivedPackage[13], receivedPackage[14]);
                qtemp.y = aaa * 1 / (1 << 30);
                aaa = four_bytes(receivedPackage[15], receivedPackage[16], receivedPackage[17], receivedPackage[18]);
                qtemp.z = aaa * 1 / (1 << 30);
                quat.w = qtemp.w; quat.x = qtemp.x; quat.y = qtemp.y; quat.z = qtemp.z;
                quat = normalize(quat);
                if (!bResult[receivedPackage[1]])
                {
                    quatsStart[receivedPackage[1]] = quat;
                    bResult[receivedPackage[1]] = true;
                }
                quats[receivedPackage[1]] = quat / quatsStart[receivedPackage[1]];
            }
            return quats;
        }
        //四元数处理函数

    void DataReceiveFunction()
        {
            while (  bRead)
            {
                try
                {
                    //通过read函数获取串口数据
                    StartList.sp.Read(RXBuff, 0, 1);
                    package[packageDataBitCount] = Convert.ToInt16(RXBuff[0]);
                    //数据包完整性判断
                    if (package[0] == '$')
                    {
                        if (packageDataBitCount == 22 && package[21] == 0x0D && package[22] == 0x0A)
                        {
                            //处理数据包
                            getPackage(package);
                            packageDataBitCount = 0;
                        }
                        else if (packageDataBitCount >= 22)
                        {
                            packageDataBitCount = 0;
                        }
                        else
                        {
                            ++packageDataBitCount;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.Message);
                }
            }
        }
  float four_bytes(int a, int b, int c, int d)
        {
            d = (a << 24) + (b << 16) + (c << 8) + d;
            return d;
        }
        //电池数据处理函数,单位为伏特
   float two_bytes(int a, int b)
        {
            float temp = ((a << 8) + b) / 1000;
            return temp;
        }
        //void build(  HumanBodyBones    a , HumanBone b    )
        //{
        //    b.boneName = a.ToString();
        //    b.humanName = " ";

        //}
   public float q0; public float q1; public float q2; public float q3; public float v1; public float v2; public float v3;
        public Matrix4x4 quatertion_to_vector3(Quaternion alpha, Vector3 delta)
        {
            Matrix4x4 trans = new Matrix4x4();
            alpha.x = q0; alpha.y = q1; alpha.z = q2; alpha.w = q3;
            delta.x = v1; delta.y = v2; delta.z = v3;
            trans.m00 = 1 - (2 * q2 * q2 - 2 * q3 * q3);
            trans.m01 = 2 * ((q1 * q2) + (q0 * q3));
            trans.m02 = 2 * ((q1 * q3) - (q0 * q2));
            trans.m03 = 0;
            trans.m10 = 2 * ((q1 * q2) - (q0 * q3));
            trans.m11 = 1 - (2 * q1 * q1 - 2 * q3 * q3);
            trans.m12 = 2 * ((q2 * q3) + (q0 * q1));
            trans.m13 = 0;
            trans.m20 = 2 * ((q1 * q3) + (q0 * q2));
            trans.m21 = 2 * ((q2 * q3) - (q0 * q1));
            trans.m22 = 1 - (2 * q1 * q1 - 2 * q2 * q2);
            trans.m23 = 0;
            trans.m30 = 0;
            trans.m31 = 0;
            trans.m32 = 0;
            trans.m33 = 0;

            return trans;
            //TO DO
        }
        //四元数转化三维向量函数




    } 
