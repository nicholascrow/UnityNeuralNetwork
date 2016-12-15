using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Drawing;
using AForge;
using AForge.Imaging.Filters;
using System;

public class TestBMP : MonoBehaviour {
    public GameObject sphere;
    public Material green, red;
    //size of resized image, should be 30 or less
    int imgSizexy = 30;

    //camera which we will be writing from
    public Camera c;

    //stream to write pixel data from
    private StreamWriter writeNumbers;

    public SteamVR_TrackedObject trackedObj;


    // Use this for initialization
    void Start() {

        //write numbers to this file
//        writeNumbers = File.CreateText(Application.dataPath + "/2.csv");

        sphere.GetComponent<Renderer>().material = green;

        StartCoroutine(Other());

    }

    public void Update() {
        var device = SteamVR_Controller.Input((int)trackedObj.index);
        if(device.GetTouchDown(SteamVR_Controller.ButtonMask.Grip)) {
            sphere.GetComponent<Renderer>().material = red;
            Bitmap b = FilterImage(TakeCameraSnapshot());

            b.Save(Application.dataPath + "/test.jpg");

            StartCoroutine(SaveImageAsInt(b));

        }
    }

    public void OnApplicationQuit() {
        writeNumbers.Close();
    }


    Bitmap FilterImage(Texture2D x) {
        //import the img as a bitmap
        Bitmap b = new Bitmap(Image.FromStream(new MemoryStream(x.EncodeToJPG())), new Size(imgSizexy, imgSizexy));
        // new Bitmap(Image.FromFile(path), new Size(imgSizexy, imgSizexy));

        //grayscale it
        Grayscale gray = new Grayscale(.33, .33, .33);
        Bitmap grayImg = gray.Apply(b);

        //threshold it
        Threshold filter = new Threshold(150);
        filter.ApplyInPlace(grayImg);

        return grayImg;
    }

    IEnumerator SaveImageAsInt(Bitmap imgNow) {
        double[] imgAsNumber = new double[imgSizexy * imgSizexy];
        //create csv file
        //var sr = File.CreateText(Application.dataPath + "/test.csv");
        string writeLinetoFile = "";
        //for the x pixels
        for(int i = 0; i < imgSizexy; i++) {
            //for the y pixels
            for(int j = 0; j < imgSizexy; j++) {
                if(imgNow.GetPixel(i, j).Name.Contains("ff000000")) {
                    //-1 for black
                    imgAsNumber[i * imgSizexy + j] = -1;
                }
                else if(imgNow.GetPixel(i, j).Name.Contains("ffffffff")) {
                    //1 for white
                    imgAsNumber[i * imgSizexy + j] = 1;
                }
                else {
                    throw new InvalidDataException();
                }

                writeLinetoFile += imgAsNumber[i * imgSizexy + j] + ",";
            }
            yield return null;
        }
       // print(writeLinetoFile);
        writeNumbers.WriteLine(writeLinetoFile.TrimEnd(','));
        sphere.GetComponent<Renderer>().material = green;
    }

    Texture2D TakeCameraSnapshot() {
        // Setup a camera, texture and render texture
        Camera cam = c;
        RenderTexture r = new RenderTexture(512, 512, 24);
        cam.targetTexture = r;
        cam.Render();

        //create texture to put the cam stuff onto
        Texture2D tex = new Texture2D(512, 512, TextureFormat.ARGB32, false);

        // Read pixels to texture
        RenderTexture.active = r;
        tex.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);

        return tex;

    }

    IEnumerator Other() {
        sphere.GetComponent<Renderer>().material = red;
        AForge.Neuro.ActivationNetwork network = new AForge.Neuro.ActivationNetwork(new AForge.Neuro.BipolarSigmoidFunction(2), imgSizexy * imgSizexy, 3);
        network.Randomize();
        AForge.Neuro.Learning.PerceptronLearning learning = new AForge.Neuro.Learning.PerceptronLearning(network);
        learning.LearningRate = 1;


        string[] lines1 = File.ReadAllLines(Application.dataPath + "/1.csv");

        string[] lines2 = File.ReadAllLines(Application.dataPath + "/2.csv");

        //double[][] input = new double[lines.Length][];
        //for(int i = 0; i < lines.Length; i++) {
        //    input[i] = new double[imgSizexy * imgSizexy];
        //    lines[i].Split(',').CopyTo(input[i], 0);
        //}

        double[][] input = new double[6][];
        for(int i = 0; i < 6; i++) {
            yield return null;
            if(i < 3) {
                double[] p = new double[lines1[i].Split(',').Length];
                string[] s = lines1[i].Split(',');
                for(int j = 0; j < s.Length; j++) {
                    p[j] = Convert.ToInt32(s[j]);
                }
                input[i] = p;
            }
            else {
                double[] p = new double[lines2[i - 3].Split(',').Length];
                string[] s = lines2[i - 3].Split(',');
                for(int j = 0; j < s.Length; j++) {
                    p[j] = Convert.ToInt32(s[j]);
                }
                input[i] = p;
            }

        }



        bool needToStop = false;
        int iteration = 0;
        while(!needToStop) {
            yield return null;
            double error = learning.RunEpoch(input, new double[6][] {
                    new double[3] { 1, -1, -1 },new double[3] { 1, -1, -1 },new double[3] { 1, -1, -1 },//A
                    new double[3] { -1, 1, -1 },new double[3] { -1, 1, -1 },new double[3] { -1, 1, -1 }}//C
                                                                                                        /*new double[9][]{ input[0],input[0],input[0],input[1],input[1],input[1],input[2],input[2],input[2]}*/
                );
            //learning.LearningRate -= learning.LearningRate / 1000;
            if(error == 0)
                break;
            else if(iteration < 1000)
                iteration++;
            else
                needToStop = true;
            Debug.LogFormat("{0} {1}", error, iteration);
        }

        Bitmap b = AForge.Imaging.Image.FromFile(Application.dataPath + "/test.jpg");
        //Reading A Sample to test Netwok
        double[] sample = new double[900];
        for(int j = 0; j < imgSizexy; j++)
            for(int k = 0; k < imgSizexy; k++) {
                if(b.GetPixel(j, k).Name.Contains("ff000000")) {
                    //-1 for black
                    sample[j * imgSizexy + k] = -1;
                }
                else if(b.GetPixel(j, k).Name.Contains("ffffffff")) {
                    //1 for white
                    sample[j * imgSizexy + k] = 1;
                }
                else {
                    throw new InvalidDataException();
                }

                //if(b.GetPixel(j, k).ToKnownColor() == KnownColor.White) {
                //    sample[j * imgSizexy + k] = -1;
                //}
                //else
                //    sample[j * imgSizexy + k] = 1;
            }
        string s1 = "{";
        foreach(double d in network.Compute(sample))
            s1 += d + ", ";
                s1 += "}";
            Debug.LogError(s1);//Output is Always C = {-1,-1,1}
        network.Save(Application.dataPath + "/OMGWORKING.bin");
        sphere.GetComponent<Renderer>().material = green;
    }


}

