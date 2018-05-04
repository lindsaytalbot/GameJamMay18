using MightyKingdom;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;

public class XMLDictionary
{
    private Dictionary<string, string> stringValues = new Dictionary<string, string>();
    private Dictionary<string, float> floatValues = new Dictionary<string, float>();
    private Dictionary<string, int> intValues = new Dictionary<string, int>();

    public XMLDictionary()
    {

    }

    public XMLDictionary(string xmlValues)
    {
        MKLog.Log(xmlValues);
        int count = 0;

        try
        {
            XmlReader reader = XmlReader.Create(new StringReader(xmlValues));            

            string type = "";
            string key = "";
            string value = "";

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    type = reader.Name;
                    key = reader.GetAttribute("name");
                    value = reader.GetAttribute("value");

                    switch (type)
                    {
                        case "int":
                            SetInt(key, int.Parse(value));
                            break;
                        case "float":
                            SetFloat(key, float.Parse(value));
                            break;
                    }
                }
                else if (reader.NodeType == XmlNodeType.Text)
                {
                    if (type.Equals("string"))
                    {
                        SetString(key, reader.Value);
                    }
                }
                count++;
            }
        }
        catch (Exception e)
        {
            MKLog.LogError("Error reading XML file: " + e.ToString());
        }

        MKLog.Log("Read " + count + "lines ");
    }

    public void SetString(string key, string val)
    {
        stringValues[key] = val;
    }

    public string GetString(string key, string defValue)
    {
        if (stringValues.ContainsKey(key))
            return stringValues[key];
        return defValue;
    }

    public void SetInt(string key, int val)
    {
        intValues[key] = val;
    }

    public int GetInt(string key, int defValue)
    {
        if (intValues.ContainsKey(key))
            return intValues[key];
        return defValue;
    }

    public void SetFloat(string key, float val)
    {
        floatValues[key] = val;
    }

    public float GetFloat(string key, float defValue)
    {
        if (floatValues.ContainsKey(key))
            return floatValues[key];
        return defValue;
    }

    public bool HasKey(string key)
    {
        return stringValues.ContainsKey(key) || intValues.ContainsKey(key) || floatValues.ContainsKey(key);
    }

    public void DeleteKey(string key)
    {
        stringValues.Remove(key);
        intValues.Remove(key);
        floatValues.Remove(key);
    }

    public void DeleteAll()
    {
        stringValues.Clear();
        intValues.Clear();
        floatValues.Clear();
    }

    public string ToXMLString()
    {
        XElement xml = new XElement("map");

        foreach(string key in stringValues.Keys)
        {
            XElement value = new XElement("string",
                new XAttribute("name", key),
                stringValues[key]                               
            );
            xml.Add(value);
        }

        foreach (string key in intValues.Keys)
        {
            XElement value = new XElement("int",
                new XAttribute("name", key),
                new XAttribute("value", intValues[key])
            );
            xml.Add(value);
        }

        foreach (string key in floatValues.Keys)
        {
            XElement value = new XElement("float",
                new XAttribute("name", key),
                new XAttribute("value", floatValues[key])
            );
            xml.Add(value);
        }


        return xml.ToString();
    }

    public static List<object> ReturnChangedKeys(XMLDictionary dict1, XMLDictionary dict2)
    {
        List<object> changedKeys = new List<object>();
        
        //Dict 1
        foreach (string key in dict1.stringValues.Keys)
        {
            if (!dict2.stringValues.ContainsKey(key) || dict1.stringValues[key] != dict2.stringValues[key])
            {
                if (!changedKeys.Contains(key))
                    changedKeys.Add(key);
            }
        }

        foreach (string key in dict1.intValues.Keys)
        {
            if (!dict2.intValues.ContainsKey(key) || dict1.intValues[key] != dict2.intValues[key])
            {
                if (!changedKeys.Contains(key))
                    changedKeys.Add(key);
            }
        }

        foreach (string key in dict1.floatValues.Keys)
        {
            if (!dict2.floatValues.ContainsKey(key) || dict1.floatValues[key] != dict2.floatValues[key])
            {
                if (!changedKeys.Contains(key))
                    changedKeys.Add(key);
            }
        }

        //Dict 2
        foreach (string key in dict2.stringValues.Keys)
        {
            if (!dict1.stringValues.ContainsKey(key) || dict1.stringValues[key] != dict2.stringValues[key])
            {
                if (!changedKeys.Contains(key))
                    changedKeys.Add(key);
            }
        }

        foreach (string key in dict2.intValues.Keys)
        {
            if (!dict1.intValues.ContainsKey(key) || dict1.intValues[key] != dict2.intValues[key])
            {
                if (!changedKeys.Contains(key))
                    changedKeys.Add(key);
            }
        }

        foreach (string key in dict2.floatValues.Keys)
        {
            if (!dict1.floatValues.ContainsKey(key) || dict1.floatValues[key] != dict2.floatValues[key])
            {
                if (!changedKeys.Contains(key))
                    changedKeys.Add(key);
            }
        }

        return changedKeys;
    }
}
