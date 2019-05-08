using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GWEventHandler : MonoBehaviour {

    //Base path containing all events data
    string base_path = "";
    DebugMessages _debug_messages;

    void Awake()
    {
        //Base path containing is initialized here because of how Unity works
        base_path = Application.persistentDataPath + "/Events/";
    }

    public void Init(DebugMessages debug_messages)
    {
        _debug_messages = debug_messages;
    }

    public string[] GetEventNames()
    {
        string[] names = Directory.GetDirectories(base_path);
        for (int i = 0; i < names.Length; i++)
        {
            names[i] = names[i].Replace(Application.persistentDataPath + "/Events/", "");
        }

        return names;
    }

	public EventData FetchEvent(string folder_name)
    {
        //Create new event structure to hold its properties
        EventData new_evt;
        
        //Load image data into byte array, then into Texture2D structure
        byte[] imageBytes = File.ReadAllBytes(base_path + folder_name + "/photosphere.jpg");
        Texture2D tex = new Texture2D(2, 2);
        try
        {
            tex.LoadImage(imageBytes);
        }
        catch (UnityException e)
        {
            _debug_messages.Print("Could not load Photosphere!\n", DebugMessages.Colors.Error);
        }

        //Write event properties into new structure
        new_evt.photosphere = tex;
        new_evt.internal_name = folder_name;
        new_evt.name = "Event";                             //TO BE CHANGED
        new_evt.date = new DateTime(2000, 1, 1, 1, 1, 0);   //TO BE CHANGED

        //Return newly created structure
        return new_evt;
    }

    public void ApplyEventPhotosphere(Transform sphere, EventData evt)
    {
        //Apply texture to sphere from an EventData instance
        sphere.gameObject.GetComponent<Renderer>().material.mainTexture = evt.photosphere;
    }
}

public struct EventData
{
    //Internal name, must be unique
    public string internal_name;
    //Name displayed to user
    public string name;
    //Date of the GW event;
    public DateTime date;
    //Equirectangular image of the event;
    public Texture2D photosphere;
}