using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class DataTableParser<T> where T : IUsableId
{
    public Func<string[], Dictionary<string, int>, T> Parse;
    [SerializeField] Dictionary<string, T> values;
    public Dictionary<string, T> Values { get { return values; } }

    public DataTableParser(Func<string[], Dictionary<string, int>, T> Parse)
    {
        this.Parse = Parse;
        values = new Dictionary<string, T>();
    }

    public bool Load(in string csv)
    {
        if(csv == null)
        {
            Debug.Log("Csv 데이터가 null입니다.");
            return false;
        }

        string[] lines = Regex.Split(csv, @"\n(?=(?:[^$]*\$[^$]*\$)*[^$]*$)");

        string[] names = Regex.Split(lines[2], @",(?=(?:[^$]*\$[^$]*\$)*[^$]*$)");

        Dictionary<string, int> nameToIndexDict = new Dictionary<string, int>();

        for(int i = 0; i < names.Length; i++)
        {
            nameToIndexDict[names[i]] = i;
        }

        for (int i = 4; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i])) continue;

            string[] fields = Regex.Split(lines[i], @",(?=(?:[^$]*\$[^$]*\$)*[^$]*$)");

            for (int j = 0; j < fields.Length; j++)
            {
                fields[j] = fields[j].Trim().Trim('"').Trim('$');
                fields[j] = fields[j].Replace("$", "");
            }

            T value = Parse(fields, nameToIndexDict);

            if (value == null || string.IsNullOrEmpty(value.GetId())) continue;

            values.Add(value.GetId(), value);
        }

        return true;
    }
}