using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GWEventDatabase : MonoBehaviour {

    //Base path containing all events data.
    string base_path = "";
    //Array containing summaries for all events.
    public EventSummary[] evt_summaries = null;

    //Struct containing currently loaded event.
    public EventData evt_data;

    DebugMessages _debug_messages;

    void Awake()
    {
        //Base path is initialized here because Unity doesn't allow using persistentDataPath outside a function.
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

    public void LoadEventSummaries()
    {
        string[] evt_names = GetEventNames();
        evt_summaries = new EventSummary[evt_names.Length];

        for(int i = 0; i < evt_summaries.Length; i++)
        {
            evt_summaries[i].NullAll();
            evt_summaries[i].internal_name = evt_names[i];

            try
            {
                //Get data from data folder. 
                using (StreamReader file = new StreamReader(base_path + evt_names[i] + "/data/data.txt"))
                {
                    string[] str;
                    while ((str = FileRead.ReadCurrLine(file)) != null)
                    {
                        switch (str[0])
                        {
                            case "Name":
                                evt_summaries[i].name = str[1];
                                break;
                            case "Date":
                                evt_summaries[i].date = DateTime.Parse(str[1]);
                                break;
                        }
                    }

                    file.Close();
                }
            }
            catch (Exception e)
            {
                _debug_messages.Print("Could not load Event data file! " + e.Message, DebugMessages.Colors.Error);
            }

            Texture2D portrait = new Texture2D(2, 2);
            evt_summaries[i].portrait = portrait;
        }
    }

	public EventData FetchEvent(string folder_name)
    {
        //Create new event structure to hold its properties
        EventData new_evt = new EventData();
        new_evt.NullAll();

        //Get all images path in event folder
        string[] images_name = Directory.GetFiles(base_path + folder_name, "*.jpg");

        //Load each image into a byte array, then in a Texture2D structure
        Texture2D[] tex_a = new Texture2D[images_name.Length];
        for (int i = 0; i < images_name.Length; i++)
        {
            byte[] image_bytes = File.ReadAllBytes(images_name[i]);
            tex_a[i] = new Texture2D(2, 2);
            try
            {
                tex_a[i].LoadImage(image_bytes);
            }
            catch (UnityException e)
            {
                _debug_messages.Print("Could not load Event Image data! " + e.Message, DebugMessages.Colors.Error);
                break;
            }
        }

        try
        {
            //Get data from data folder. 
            using (StreamReader file = new StreamReader(base_path + folder_name + "/data/data.txt"))
            {
                string[] str;
                while ((str = FileRead.ReadCurrLine(file)) != null)
                {
                    switch (str[0])
                    {
                        case "Name":
                            new_evt.name = str[1];
                            break;
                        case "Date":
                            new_evt.date = DateTime.Parse(str[1]);
                            break;
                        case "Description":
                            new_evt.description = str[1];
                            break;
                    }
                }

                file.Close();
            }
        }
        catch (Exception e)
        {
            _debug_messages.Print("Could not load Event data file! " + e.Message, DebugMessages.Colors.Error);
        }

        //Write event properties into new structure
        new_evt.internal_name = folder_name;
        new_evt.photospheres = tex_a;

        //Return newly created structure
        return new_evt;
    }

    public void ApplyEventPhotosphere(Transform sphere, EventData evt, int index)
    {
        if (index >= 0 && index < evt.photospheres.Length)
        {
            //Apply texture to sphere from an EventData instance
            sphere.gameObject.GetComponent<Renderer>().material.mainTexture = evt.photospheres[index];
        }
        else
            _debug_messages.Print("Could not load Photosphere! Index error!\n", DebugMessages.Colors.Error);
    }
}

//Contains full data pertaining to an event. Only one at a time in memory.
public struct EventData
{
    public void NullAll()
    {
        internal_name = null;
        name = null;
        date = default(DateTime);
        description = null;
        photospheres = null;
    }
    //Checks that data between event and summary is consistent
    public bool ConsistencyCheck(EventSummary summary)
    {
        //TO WRITE
        return true;
    }

    //Internal name, must be unique
    public string internal_name;
    //Name displayed to user
    public string name;
    //Date of the GW event;
    public DateTime date;
    //Brief Description of the GW event
    public string description;
    //Equirectangular images of the event;
    public Texture2D[] photospheres;
}

//Contains brief data pertaining to an event (used in the selection menu).
public struct EventSummary
{
    public void NullAll()
    {
        internal_name = null;
        name = null;
        date = default(DateTime);
        portrait = null;
    }

    //Internal name, must be unique
    public string internal_name;
    //Name displayed to user
    public string name;
    //Date of the GW event;
    public DateTime date;
    //Small portrait of the event (first image found);
    public Texture2D portrait;
}