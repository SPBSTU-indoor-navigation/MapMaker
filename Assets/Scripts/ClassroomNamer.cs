using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ClassroomNamer : MonoBehaviour
{
    public string name;

    public string templateRU = "";
    public string templateEN = "";

    private void OnValidate()
    {
        var u = GetComponent<IMDF.Unit>();

        var ru = templateRU;
        var en = templateEN;

        if (u.occupantCategory == IMDF.Feature.Occupant.Category.classroom)
        {
            if (string.IsNullOrWhiteSpace(ru))
                ru = "Кабинет {name}";
            if (string.IsNullOrWhiteSpace(en))
                en = "Classroom {name}";
        }
        else if (u.occupantCategory == IMDF.Feature.Occupant.Category.laboratory)
        {
            if (string.IsNullOrWhiteSpace(ru))
                ru = "Лабораторная {name}";
            if (string.IsNullOrWhiteSpace(en))
                en = "Lab {name}";
        }
        else if (u.occupantCategory == IMDF.Feature.Occupant.Category.auditorium)
        {
            if (string.IsNullOrWhiteSpace(ru))
                ru = "Лекторий {name}";
            if (string.IsNullOrWhiteSpace(en))
                en = "Auditorium {name}";
        }

        u.altName.ru = name;
        u.altName.en = name;

        u.localizedName.ru = ru.Replace("{name}", name);
        u.localizedName.en = en.Replace("{name}", name);

        u.OnValidate();

    }
}
