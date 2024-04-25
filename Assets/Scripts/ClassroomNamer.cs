using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Category = IMDF.Feature.Occupant.Category;

public class ClassroomNamer : MonoBehaviour
{
    public string name;

    public string templateRU = "";
    public string templateEN = "";

    [HideInInspector]
    public Dictionary<Category, string> dictRU = new() {
            { Category.classroom, "Кабинет {name}" },
            { Category.laboratory, "Лабораторная {name}" },
            { Category.auditorium, "Лекторий {name}" },
            { Category.restroomFemale, "Туалет женский" },
            { Category.restroomMale, "Туалет мужской" },
        };


    [HideInInspector]
    public Dictionary<Category, string> dictEN = new() {
            { Category.classroom, "Classroom {name}" },
            { Category.laboratory, "Lab {name}" },
            { Category.auditorium, "Auditorium {name}" },
            { Category.restroomFemale, "Restroom female" },
            { Category.restroomMale, "Restroom male" },

        };

    private void OnValidate()
    {
        var u = GetComponent<IMDF.Unit>();

        if (!u) return;

        var ru = templateRU;
        var en = templateEN;

        if (string.IsNullOrWhiteSpace(ru))
            dictRU.TryGetValue(u.occupantCategory, out ru);

        if (string.IsNullOrWhiteSpace(en))
            dictEN.TryGetValue(u.occupantCategory, out en);


        if (u.occupantCategory == Category.restroomMale || u.occupantCategory == Category.restroomFemale)
        {
            u.altName.ru = u.occupantCategory == Category.restroomMale ? "Туалет М." : "Туалет Ж.";
            u.altName.en = u.occupantCategory == Category.restroomMale ? "Restroom M." : "Restroom F.";
        }
        else
        {
            u.altName.ru = name;
            u.altName.en = name;
        }

        u.localizedName.ru = ru.Replace("{name}", name);
        u.localizedName.en = en.Replace("{name}", name);
        u.OnValidate();

    }
}
