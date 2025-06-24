using TMPro;
using UnityEngine;

public class PersonMarkerController : MonoBehaviour
{
    [SerializeField] private TextMeshPro _nameText;
    [SerializeField] private  TextMeshPro _birthText;
    [SerializeField] private  TextMeshPro _featureText;
    public void SetInfo(StudentInfoData info)
    {
        _nameText.text = info.name;
        _birthText.text = info.birth;
        _featureText.text = info.feature;
    }
}
