using TMPro;
using UnityEngine;

public class PersonMarkerController : MonoBehaviour
{
    [SerializeField] private TextMeshPro _nameText;

    public void SetName(string name)
    {
        _nameText.text = name;
    }
}
