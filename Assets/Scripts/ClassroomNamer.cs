using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ClassroomNamer : MonoBehaviour
{
    public string name;

    public string templateRU = "Кабинет {name}";
    public string templateEN = "Classroom {name}";

    private void OnValidate()
    {
        var u = GetComponent<IMDF.Unit>();

        u.altName.ru = name;
        u.altName.en = name;

        u.localizedName.ru = templateRU.Replace("{name}", name);
        u.localizedName.en = templateRU.Replace("{name}", name);

        u.OnValidate();

    }
}
