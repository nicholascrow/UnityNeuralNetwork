﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Drawing;
using AForge;
using AForge.Imaging.Filters;
using System;
using UnityEngine.UI;

public class TestBMP : MonoBehaviour {
    enum Status {
        Train = 0,
        Test = 1,
        MakeData = 2
    }

    private Status status = Status.MakeData;


    #region non-neural network declarations
    //text to display on
    public Text epochText, indexText;

    //colored sphere which changes colors if you need to wait for training
    public GameObject sphere;

    //colors
    public Material green, red;

    //the controller so we can perform actions
    public SteamVR_TrackedObject trackedObj;

    public bool ClearCanvas = false;
    #endregion

    #region neuralnet declarations

    public int currentAddIndex = 0;
    //size of resized image, should be 30 or less
    int imgSizexy = 30;

    //camera which we will be writing from
    public Camera c;

    //stream to write pixel data from
    private StreamWriter writeNumbers;

    //the network itself
    AForge.Neuro.ActivationNetwork network;

    private List<double[]>[] numberArray;

    #endregion

    // Use this for initialization
    void Start() {

        sphere.GetComponent<Renderer>().material = green;

        switch(status) {
            case Status.Train:
                StartCoroutine(TrainNetwork());
                break;
            case Status.Test:
                break;
            case Status.MakeData:
                writeNumbers = File.AppendText(Application.dataPath + "/" + 2 + ".csv");
                break;
            default:
                break;
        }

        numberArray = new List<double[]>[2];
        for(int i = 0; i < 2; i++) {
            numberArray[i] = new List<double[]>();
        }
    }

    public void Update() {

        var device = SteamVR_Controller.Input((int)trackedObj.index);

        if(device.GetTouchDown(SteamVR_Controller.ButtonMask.Grip)) {
            //filter the image just drawn on the canvas
            Bitmap b = FilterImage(TakeCameraSnapshot());

            switch(status) {
                case Status.Train:

                    break;
                case Status.Test:
                    // beginning of array
                    string networkOutput = "{ ";
                    //for each item in the computation array we print the output\
                    print(GetSample(b).Length);
                    double[] computation = network.Compute(GetSample(b));
                    foreach(double d in computation) {
                        networkOutput += d + ", ";
                    }
                    //final brace for presentation
                    networkOutput = networkOutput.TrimEnd(',') + " }";

                    if(computation[0] == 1) {
                        epochText.text = "You entered a 0!" + networkOutput;
                        //one
                    }
                    else if(computation[0] == -1 ) {
                        epochText.text = "You entered a 1!" + networkOutput;
                        //two
                    }
                    else
                        epochText.text = networkOutput;



                    //print this to the console
                    Debug.LogError(networkOutput);
                    ClearCanvas = true;
                    b = null;
                    break;
                case Status.MakeData:

                    //make a sphere this color
                    sphere.GetComponent<Renderer>().material = red;

                    //save an image for testing
                    b.Save(Application.dataPath + "/test.jpg");

                    //save the image into csv file
                    StartCoroutine(SaveImageAsInt(b));

                    break;
                default:
                    break;
            }
        }
        else if(device.GetTouchDown(SteamVR_Controller.ButtonMask.ApplicationMenu)) {
            if(status == Status.MakeData) {
                status = Status.Train;
            }
            else {
                status = Status.MakeData;
            }
        }
        else if(device.GetTouchDown(SteamVR_Controller.ButtonMask.Trigger)) {
            currentAddIndex += 1;
            currentAddIndex = currentAddIndex % 2;
            indexText.text = "Current Index: " + currentAddIndex;
        }

        if(status == Status.Train) {
            StartCoroutine(TrainNetwork());

            status = Status.Test;
        }
    }

    public void OnApplicationQuit() {
        if(writeNumbers != null)
            writeNumbers.Close();
    }


    Bitmap FilterImage(Texture2D x) {
        //import the img as a bitmap
        Bitmap b = new Bitmap(System.Drawing.Image.FromStream(new MemoryStream(x.EncodeToJPG())), new Size(imgSizexy, imgSizexy));
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

            // need to implement so as to add things other than just 0.

            yield return null;
        }
        numberArray[currentAddIndex].Add(imgAsNumber);
        // print(writeLinetoFile);
        writeNumbers.WriteLine(writeLinetoFile.TrimEnd(','));
        sphere.GetComponent<Renderer>().material = green;
        ClearCanvas = true;
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

    IEnumerator TrainNetwork() {

        //while we train the network we shouldnt allow user input
        sphere.GetComponent<Renderer>().material = red;


        network = new AForge.Neuro.ActivationNetwork(new AForge.Neuro.BipolarSigmoidFunction(2), imgSizexy * imgSizexy, 3);
        network.Randomize();
        AForge.Neuro.Learning.PerceptronLearning learning = new AForge.Neuro.Learning.PerceptronLearning(network);
        //AForge.Neuro.Learning.BackPropagationLearning learning = new AForge.Neuro.Learning.BackPropagationLearning(network);
        learning.LearningRate = 1;

        /*THE BELOW COULD BE REMOVED WITH OPTIMIZATION THAT DOESNT SAVE TO THE FILE SYSTEM
        string[] lines1 = File.ReadAllLines(Application.dataPath + "/1.csv");

        string[] lines2 = File.ReadAllLines(Application.dataPath + "/2.csv");


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

        double[][] output = new double[6][] {
                    new double[3] { 1, 1, 1},new double[3] { 1, 1 ,1},new double[3] { 1, 1 ,1},//1
                    new double[3] { -1, -1 ,-1},new double[3] { -1, -1 ,-1},new double[3] { -1, -1 ,-1}//2
        };
           


        END COULD BE REMOVED*/




        //creating the input array
        int smallestArraySize = numberArray[0].Count <= numberArray[1].Count ? numberArray[0].Count : numberArray[1].Count;
        double[][] input = new double[smallestArraySize * 2][];
        for(int i = 0; i < input.GetLength(0); i++) {
            print("input is actually the right size");
            if(i < smallestArraySize) {
                input[i] = numberArray[0][i];
            }
            else {
                input[i] = numberArray[1][i - smallestArraySize];
            }
        }
        print(numberArray[0].Count);// shold be 3
        print(input.GetLength(0));
        print(input[0][0]);

        // numberArray[0].CopyTo(input, 0);
        // numberArray[1].CopyTo(input, smallestArraySize);

        double[][] output = new double[smallestArraySize * 2][];
        for(int i = 0; i < smallestArraySize; i++) {
            //not sure if this is correct
            output[i] = new double[3] { 1, 1, 1 };
        }
        for(int i = smallestArraySize; i < smallestArraySize * 2; i++) {
            output[i] = new double[3] { -1, -1, -1 };
        }




        bool needToStop = false;
        int iteration = 0;
        int maxIteration = 1000;
        while(!needToStop) {
            epochText.text = "Current Epoch: " + iteration + "/" + maxIteration;
            yield return null;
            double error = learning.RunEpoch(input, output);

            learning.LearningRate -= learning.LearningRate / 1000;
            if(error == 0) {
                print("why?");
               // iteration++;
                break;
            }
            else if(iteration < maxIteration)
                iteration++;
            else
                needToStop = true;
            Debug.LogFormat("{0} {1}", error, iteration);
        }

        Bitmap b = AForge.Imaging.Image.FromFile(Application.dataPath + "/test.jpg");
        //Reading A Sample to test Netwok
        double[] sample = new double[imgSizexy * imgSizexy];
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


            }

        //display thingy
        string s1 = "{ ";
        foreach(double d in network.Compute(sample))
            s1 += d + ", ";
        s1 += " }";
        Debug.LogError(s1);

        //save the network to this file which can then be loaded at runtime
        network.Save(Application.dataPath + "/OMGWORKING.bin");
        sphere.GetComponent<Renderer>().material = green;


    }

    double[] GetSample(Bitmap b) {
        double[] sample = new double[imgSizexy * imgSizexy];
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


            }
        return sample;
    }
}

