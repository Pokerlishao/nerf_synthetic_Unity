/*
 * Making a nerf_synthetic format dataset(https://drive.google.com/drive/folders/1JDdLGDruGNXWnM1eqY1FNL9PlStjaKWi) in Unity 
 * Author: pokerlishao@gmail.com
*/

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using Newtonsoft.Json; //Needs to be enabled in Unity's package manager
using JsonStruct;


//Randomly generate camera positions on a hemisphere and face the specified direction
public class dataset : MonoBehaviour
{
    public bool samll_test = true;  //Generate small test datasets(For making sure if your scene and camera are set up correctly)
    public bool test = false;       //Generate test datasets
    public bool train = false;      //Generate train datasets
    public bool val = false;        //Generate val datasets


    // Resolution is not controllable in editor mode
    private int screen_width = 800;
    private int screen_height = 800;

    private Vector3 camera_position = new Vector3(0f, 0f, 0f);          //The center of the hemisphere where the camera is placed
    private float radius = 100.5f;                                      //The radius of the hemisphere where the camera is placed
    private Vector3 camera_target = new Vector3(-41.78f, -14f, 73.5f);  //The center point camera facing towards 
    private string dataset_name = "blender";

    private int num_test = 200;
    private int num_train = 100;
    private int num_val = 100;



    Camera mcamera;
    JsonData jsonData;

    void Start()
    {
        Time.timeScale = 0.0f; //Setting the scene to static

        jsonData = new JsonData();
        string dataset_path = "./" + dataset_name + "/";
        if (!Directory.Exists(dataset_path)) Directory.CreateDirectory(dataset_path);

        float x_fov = Camera.HorizontalToVerticalFieldOfView(Camera.main.fieldOfView, (float)Screen.width/ (float) Screen.height);
        x_fov = x_fov * Mathf.Deg2Rad; 
        jsonData.camera_angle_x = x_fov;
        jsonData.frames = new List<FrameData>();
        
        Screen.SetResolution(screen_width, screen_height,false);
        mcamera = GetComponent<Camera>();
        StartCoroutine(random_capture());
    }


    void Update()
    {
        
    }


    private IEnumerator random_capture(){
        if (samll_test)
        {
            jsonData.frames.Clear();
            string train_path = "./" + dataset_name + "/small_test/";
            if (!Directory.Exists(train_path)) Directory.CreateDirectory(train_path);

            for (int index = 0; index < 10; index++)
            {
                RandomPlaceCameara();
                yield return new WaitForEndOfFrame();
                save_CameraInfo("./smalltest/", index);
                CaptureScreenshot(train_path, index);
            }
            // string jsonStr = JsonUtility.ToJson(jsonData);
            string train_jsonStr = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
            string train_filepath = "./" + dataset_name + "/transforms_small_test.json";
            File.WriteAllText(train_filepath, train_jsonStr);
            Debug.Log("Json data saved: " + train_filepath);
        }
        /**train**/
        if (train)
        {
            jsonData.frames.Clear();
            string train_path = "./" + dataset_name + "/train/";
            if (!Directory.Exists(train_path)) Directory.CreateDirectory(train_path);

            for (int index = 0; index < num_train; index++)
            {
                RandomPlaceCameara();
                yield return new WaitForEndOfFrame();
                save_CameraInfo("./train/", index);
                CaptureScreenshot(train_path, index);
            }
            // string jsonStr = JsonUtility.ToJson(jsonData);
            string train_jsonStr = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
            string train_filepath = "./" + dataset_name + "/transforms_train.json";
            File.WriteAllText(train_filepath, train_jsonStr);
            Debug.Log("Json data saved: " + train_filepath);
        }

        /***val***/
        if (val)
        {
            jsonData.frames.Clear();
            string val_path = "./" + dataset_name + "/val/";
            if (!Directory.Exists(val_path)) Directory.CreateDirectory(val_path);

            for (int index = 0; index < num_val; index++)
            {
                RandomPlaceCameara();
                yield return new WaitForEndOfFrame();
                save_CameraInfo("./val/", index);
                CaptureScreenshot(val_path, index);
            }
            // string jsonStr = JsonUtility.ToJson(jsonData);
            string val_jsonStr = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
            string val_filepath = "./" + dataset_name + "/transforms_val.json";
            File.WriteAllText(val_filepath, val_jsonStr);
            Debug.Log("Json data saved: " + val_filepath);
        }

        /****test****/
        if (test)
        {
            jsonData.frames.Clear();
            string test_path = "./" + dataset_name + "/test/";
            if (!Directory.Exists(test_path)) Directory.CreateDirectory(test_path);

            for (int index = 0; index < num_test; index++)
            {
                RandomPlaceCameara();
                yield return new WaitForEndOfFrame();
                save_CameraInfo("./test/", index);
                CaptureScreenshot(test_path, index);
            }
            // string jsonStr = JsonUtility.ToJson(jsonData);
            string test_jsonStr = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
            string test_filepath = "./" + dataset_name + "/transforms_test.json";
            File.WriteAllText(test_filepath, test_jsonStr);
            Debug.Log("Json data saved: " + test_filepath);
        }
    }

    void RandomPlaceCameara(){
        Vector3 randomPoint = SamplePointOnHemisphere();
        mcamera.transform.position = randomPoint;
        Vector3 tocamera_target = camera_target - randomPoint;
        tocamera_target = tocamera_target.normalized;
        mcamera.transform.rotation = Quaternion.LookRotation(tocamera_target);
    }

    void save_CameraInfo(string type, int index){
        FrameData frame = new FrameData();
        frame.file_path = type + "r_" + index.ToString();
        frame.rotation = 0.031415926535897934f;

        Matrix4x4 m = Camera.main.cameraToWorldMatrix;
        frame.FromMatrix(m, camera_position);

        jsonData.frames.Add(frame);

        /**csv file**/
        // StreamWriter sw = new StreamWriter(filename);
        // // Debug.Log(m);
        // for(int i = 0; i <4;i++){
        //     string temp = m[i, 0].ToString();
        //     for(int j = 1;j<4;j++){
        //         temp = temp +  ", " + m[i, j].ToString();
        //     }
        //     temp +=  '\n';
        //     sw.Write(temp);
        // }
        // sw.Close();
        // Debug.Log("Write file: " + filename);
        //csv
    }

    private Vector3 SamplePointOnHemisphere()
    {
        float u = Random.Range(0.0f, 1.0f); //Controls the height of the Y-axis, from 90 degrees to 0 degrees
        float v = Random.Range(0.0f, 1.0f);

        float theta = Mathf.Acos(u);
        float phi = 2 * Mathf.PI * v;

        float x = radius * Mathf.Sin(theta) * Mathf.Cos(phi) + camera_position[0];
        float z = radius * Mathf.Sin(theta) * Mathf.Sin(phi) + camera_position[2];
        float y = radius * Mathf.Cos(theta) + camera_position[1];

        return new Vector3(x, y, z);
    }



    public void CaptureScreenshot(string type, int index)
    {
        Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);
        screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenshot.Apply();

        Color32[] pixels = screenshot.GetPixels32();
        for(int i = 0; i < pixels.Length; i++)
        {
            if ((pixels[i].r + pixels[i].g + pixels[i].b == 0)) pixels[i].a = 0;
        }
        screenshot.SetPixels32(pixels);
        screenshot.Apply();

        byte[] bytes = screenshot.EncodeToPNG();
        string filename = type + "r_" + index.ToString() + ".png";
        System.IO.File.WriteAllBytes(filename, bytes);

        Debug.Log("Save Capture: " + filename);
    }
}


namespace JsonStruct
{
    [System.Serializable]
    public class FrameData
    {
        public string file_path;
        public float rotation;
        public List<List<float>> transform_matrix;

        public void FromMatrix(Matrix4x4 matrix, Vector3 camera_offset)
        {
            transform_matrix = new List<List<float>>();
            for (int row = 0; row < 4; row++)
            {
                List<float> rowData = new List<float>();
                for (int col = 0; col < 4; col++)
                {
                    rowData.Add(matrix[row, col]);
                }
                transform_matrix.Add(rowData);
            }

            // Move to origin point
            transform_matrix[0][3] -= camera_offset[0];
            transform_matrix[1][3] -= camera_offset[1];
            transform_matrix[2][3] -= camera_offset[2];
        }
    }

    [System.Serializable]
    public class JsonData
    {
        public float camera_angle_x;
        public List<FrameData> frames;
    }
}

