using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
//Позволяет двигать камеру по оси Х только при нажатии ПКМ
public class CameraControls : MonoBehaviour
{
    private CinemachineFreeLook vcam;
   
    void Start()
    {
        vcam = GetComponent<CinemachineFreeLook>();
    }

   
    void Update()
    {
        if (Input.GetKey(KeyCode.Mouse1))
            vcam.m_XAxis.m_InputAxisName = "Mouse X";
        else
        {
            vcam.m_XAxis.m_InputAxisName = "";
            vcam.m_XAxis.m_InputAxisValue = 0;
        }
    }
}
