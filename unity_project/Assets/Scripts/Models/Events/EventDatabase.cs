using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class EventDatabase : MonoBehaviour {
    // Base path containing all events data
    string base_path = "";
    readonly Dictionary<string, EventSummary> _events = new Dictionary<string, EventSummary>();

    EventData _curr_event;

    void Awake() {
        //Base path is initialized here because Unity doesn't allow using persistentDataPath outside a function.
        base_path = Application.persistentDataPath + "/Events/";
    }

    public Dictionary<string, EventSummary> GetEventsList() {
        if (_events.Count == 0)
            DownloadEventsList();

        return _events;
    }

    public EventData GetLoadedEvent() {
        if (_curr_event != null)
            return _curr_event;
        else
            throw new NullReferenceException("No event has been loaded, yet an EventData has been requested!");
    }

    public IEnumerator LoadEvent(string event_ID, Transform photosphere) {
        photosphere.GetComponent<MeshRenderer>().material.mainTexture = null;
        _curr_event = null;
        yield return Resources.UnloadUnusedAssets();
        
        if (!_events[event_ID].OnLocalStorage)
            DownloadEvent(event_ID);

        //Create new event structure to hold its properties
        EventData new_evt = new EventData();

        //Get all images path in event folder
        string[] images_name = Directory.GetFiles(base_path + event_ID, "*.jpg");

        //Load each image into a byte array, then in a Texture2D structure
        Texture2D[] tex_a = new Texture2D[images_name.Length];
        for (int i = 0; i < images_name.Length; i++) {
            try {
                byte[] image_bytes = File.ReadAllBytes(images_name[i]);
                tex_a[i] = new Texture2D(2, 2);
                tex_a[i].LoadImage(image_bytes);
            }
            catch (UnityException e) {
                Log.Print("Could not load Event photosphere! " + e.Message, Log.Colors.Error);
                break;
            }
        }

        try {
            //Get data from data folder. 
            StreamReader file = new StreamReader(base_path + event_ID + "/data/data.txt");
            string[] str;
            while ((str = TextRead.ReadCurrLine(file)) != null) {
                switch (str[0]) {
                    case "Name":
                        new_evt.Name = str[1];
                        break;
                    case "Date":
                        new_evt.Date = DateTime.ParseExact(str[1], "yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    case "Description":
                        new_evt.Description = str[1];
                        break;
                    case "ArrowPointsTo":
                        new_evt.ArrowPointsTo = new Vector2(float.Parse(str[1]), float.Parse(str[2]));
                        break;
                    case "PhotosphereTypes":
                        if (str.Length - 1 != tex_a.Length)
                            throw new FormatException("Wrong number of fields in PhotosphereTypes!");
                        new_evt.PhotosphereTypes = new string[tex_a.Length];
                        for (int i = 0; i < new_evt.PhotosphereTypes.Length; i++)
                            new_evt.PhotosphereTypes[i] = str[i + 1];
                        break;
                }
            }

            file.Close();
        }
        catch (Exception e) {
            Log.Print("Could not load Event data file! " + e.Message, Log.Colors.Error);
        }

        // Get audio file, if present
        string[] audio_name = Directory.GetFiles(base_path + event_ID + "/data", "*.mp3");
        switch (audio_name.Length) {
            case 0:
                new_evt.WaveSound = null;
                break;
            case 1:
                yield return StartCoroutine(GetAudioClip(new_evt, audio_name[0]));
                break;
            default:
                throw new Exception("Wrong number of audio files for event " + event_ID + "!");
        }

        //Write event properties into new structure
        new_evt.ID = event_ID;
        new_evt.Photospheres = tex_a;

        _curr_event = new_evt;
    }

    IEnumerator GetAudioClip(EventData evt_data, string full_path) {
        UnityWebRequest location = UnityWebRequestMultimedia.GetAudioClip("file://" + full_path, AudioType.MPEG);
        yield return location.SendWebRequest();

        if (location.isNetworkError)
            throw new Exception("Network Error!");
        else
            evt_data.WaveSound = DownloadHandlerAudioClip.GetContent(location);
    }

    void DownloadEventsList() {
        try { 
            string text = Resources.Load<TextAsset>("Packets/EventsList").text;
            StreamReader list_file = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(text)));
            string[] str;
            while ((str = TextRead.ReadCurrLine(list_file)) != null) {
                EventSummary new_summary = new EventSummary {
                    ID = str[0],
                    Name = str[1],
                    Date = DateTime.ParseExact(str[2], "yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
                    PhotospheresNumber = int.Parse(str[3]),
                    Size = float.Parse(str[4]),
                    Hash = str[5],
                    OnLocalStorage = false
                };

                _events.Add(str[0], new_summary);
            }

            list_file.Close();
        }
        catch (Exception e) {
            Log.Print("Could not download events list! " + e.Message, Log.Colors.Error);
        }

        foreach(KeyValuePair<string, EventSummary> pair in _events) {
            string event_ID = pair.Key;
            EventSummary evt = pair.Value;

            if (Directory.Exists(base_path + event_ID)) {
                try {
                string hash = "";
                for (int i = 0; i < evt.PhotospheresNumber; i++) {
                    if (File.Exists(base_path + event_ID + "/" + i + ".jpg"))
                        hash += HashExtension.GetHashMD5(base_path + event_ID + "/" + i + ".jpg");
                    else
                        throw new FileLoadException();
                }

                if (File.Exists(base_path + event_ID + "/data/data.txt"))
                    hash += HashExtension.GetHashMD5(base_path + event_ID + "/data/data.txt");
                else
                    throw new FileLoadException();
                if (File.Exists(base_path + event_ID + "/data/sound.mp3"))
                    hash += HashExtension.GetHashMD5(base_path + event_ID + "/data/sound.mp3");
                else
                    throw new FileLoadException();

                //Debug.Log(event_ID + ": " + hash);
                
                if (hash == evt.Hash)
                    evt.OnLocalStorage = true;
                else
                    throw new FileLoadException();
                }
                catch (FileLoadException) {
                    Directory.Delete(base_path + event_ID, true);
                    _events[event_ID].OnLocalStorage = false;
                }
            }
        }
    }

    public void DownloadEvent(string event_ID) {
        Directory.CreateDirectory(base_path + event_ID);
        Directory.CreateDirectory(base_path + event_ID + "/data");

        for (int i = 0; i < _events[event_ID].PhotospheresNumber; i++) {
            TextAsset image = Resources.Load<TextAsset>("Packets/" + event_ID + "/" + i);
            File.WriteAllBytes(base_path + event_ID + "/" + i + ".jpg", image.bytes);
        }

        TextAsset evt_data = Resources.Load("Packets/" + event_ID + "/data") as TextAsset;
        File.WriteAllBytes(base_path + event_ID + "/data/data.txt", evt_data.bytes);

        TextAsset wave_sound = Resources.Load("Packets/" + event_ID + "/sound") as TextAsset;
        if (wave_sound != null)
            File.WriteAllBytes(base_path + event_ID + "/data/sound.mp3", wave_sound.bytes);

        _events[event_ID].OnLocalStorage = true;
    }

    public void DeleteEvent(string event_ID) {
        if (_events[event_ID].OnLocalStorage)
            Directory.Delete(base_path + event_ID, true);

        _events[event_ID].OnLocalStorage = false;
    }
}

public class EventData {
    //Internal name, must be unique
    public string ID { get; set; }
    //Name displayed to user
    public string Name { get; set; }
    //Date of the GW event;
    public DateTime Date { get; set; }
    //Brief Description of the GW event
    public string Description { get; set; }
    //Equirectangular images of the event;
    public Texture2D[] Photospheres { get; set; }

    public int CurrentPhotosphere { get; set; } = 0;

    public Vector2 ArrowPointsTo = new Vector2(0, 90);
    public AudioClip WaveSound { get; set; } = null;

    public string[] PhotosphereTypes { get; set; }
}

//Contains brief data pertaining to an event (used in the selection menu).
public class EventSummary {
    //Internal name, must be unique
    public string ID { get; set; }
    //Name displayed to user
    public string Name { get; set; }
    //Date of the GW event;
    public DateTime Date { get; set; }
    public int PhotospheresNumber { get; set; }
    // Memory size for the event, in MBs
    public float Size { get; set; }

    public string Hash { get; set; }
    public bool OnLocalStorage { get; set; }
}