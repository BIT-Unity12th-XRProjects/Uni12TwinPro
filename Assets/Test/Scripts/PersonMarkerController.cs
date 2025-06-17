using TMPro;
using UnityEngine;

class StudentInfo
{
    public int id;
    public string name;
    public string birth;
    // public string 
}

public class PersonMarkerController : MonoBehaviour
{
    [SerializeField] private TextMeshPro _nameText;

    public void SetName(string name)
    {
        _nameText.text = name;
    }
}
