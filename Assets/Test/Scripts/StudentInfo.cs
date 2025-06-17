using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StudentInfoData
{
    public string name;
    public string birth;
    public string feature;
}

[Serializable]
public class StudentInfoDataList
{
    public List<StudentInfoData> data;
}

public class StudentInfo
{
    private StudentInfoDataList _studentInfoDataList;
    
    public List<StudentInfoData> Infos { get; private set; }
    
    public StudentInfo()
    {
        string path = "Json/StudentInfo";
        TextAsset textAsset = Resources.Load<TextAsset>(path);

        if (textAsset is null)
        {
            Debug.LogError("textAsset is null");
            return;
        }
        
        _studentInfoDataList = JsonUtility.FromJson<StudentInfoDataList>(textAsset.text);
        Infos = _studentInfoDataList.data;
    }
}