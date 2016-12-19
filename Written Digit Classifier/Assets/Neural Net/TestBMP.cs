using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Drawing;
using AForge;
using AForge.Imaging.Filters;
using System;
using UnityEngine.UI;
using Accord;

public class TestBMP : MonoBehaviour {
    enum Mode {
        Train = 0,
        Test,
        MakeData,
        LoadLearnedModel
    }
    enum LearningType {
        Perceptron=0,
        BackProp
    }
    private Mode mode = Mode.MakeData;
    private LearningType learningMethod = LearningType.BackProp;

    #region non-neural network declarations
    //text to display on
    public Text epochText, indexText, errorText;

    //colored sphere which changes colors if you need to wait for training
    public GameObject sphere;

    //colors
    public Material green, red;

    //the controller so we can perform actions
    public SteamVR_TrackedObject trackedObj;

    public bool ClearCanvas = false;
    #endregion

    #region neuralnet declarations

    public GameObject addTextureHere;

    public int currentAddIndex = 0;
    //size of resized image, should be 30 or less
    int imgSizexy = 30;

    //camera which we will be writing from
    public Camera c;

    //stream to write pixel data from
    //private StreamWriter writeNumbers;
    private List<StreamWriter> writeNumbers;
    //the network itself
    Accord.Neuro.ActivationNetwork network;

    private List<double[]>[] numberArray;

    #endregion

    // Use this for initialization
    void Start() {
        writeNumbers = new List<StreamWriter>();
        sphere.GetComponent<Renderer>().material = green;

        switch(mode) {
            case Mode.Train:
                TrainNetwork();
                break;
            case Mode.Test:
                break;
            case Mode.MakeData:
                writeNumbers.Add(File.AppendText(Application.dataPath + "/" + 0 + ".csv"));
                writeNumbers.Add(File.AppendText(Application.dataPath + "/" + 1 + ".csv"));
                //writeNumbers = File.AppendText(Application.dataPath + "/" + currentAddIndex + ".csv");
                break;
            case Mode.LoadLearnedModel:
                LoadLearnedModel();
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
            Bitmap[] b = FilterImage(TakeCameraSnapshot(), 10);

            switch(mode) {
                case Mode.Train:

                    break;
                case Mode.Test:
                    // beginning of array
                    string networkOutput = "{ ";
                    //for each item in the computation array we print the output\
                    //print(GetSample(b).Length);
                    double[] computation = network.Compute(GetSample(b[0]));
                    foreach(double d in computation) {
                        networkOutput += d + ", ";
                    }
                    //final brace for presentation
                    networkOutput = networkOutput.TrimEnd(',') + " }";

                    if(computation[0] > 0) {
                        epochText.text = "You entered a 0!" + networkOutput;
                        //one
                    }
                    else if(computation[0] <= 0) {
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
                case Mode.MakeData:

                    //make a sphere this color
                    sphere.GetComponent<Renderer>().material = red;

                    //save an image for testing
                    b[0].Save(Application.dataPath + "/test.jpg");

                    //save the image into csv file
                    for(int i = 0; i < b.Length; i++) {
                        StartCoroutine(SaveImageAsInt(b[i]));
                    }

                    break;
                case Mode.LoadLearnedModel:
                    //make a sphere this color
                    sphere.GetComponent<Renderer>().material = red;
                    break;
                default:
                    break;
            }
        }
        else if(device.GetTouchDown(SteamVR_Controller.ButtonMask.ApplicationMenu)) {
            if(mode == Mode.MakeData) {
                mode = Mode.Train;
                StartCoroutine(displayImage());
            }
            else {
                mode = Mode.MakeData;
            }
        }
        else if(device.GetTouchDown(SteamVR_Controller.ButtonMask.Trigger)) {
            currentAddIndex += 1;
            currentAddIndex = currentAddIndex % 2;
            //indexText.text = "Current Index: " + currentAddIndex;
            indexText.text = "Current Index: " + currentAddIndex
                 + "\nIndex 0: " + numberArray[0].Count + " items"
                 + "\nIndex 1: " + numberArray[1].Count + " items";
        }

        if(mode == Mode.Train) {
            TrainNetwork();

            mode = Mode.Test;
        }
    }

    public void OnApplicationQuit() {
        if(writeNumbers != null) {
            foreach(var item in writeNumbers) {
                item.Close();
            }
        }
           // writeNumbers.Close();
    }

    public void LoadLearnedModel() {
        print(Application.dataPath + "/net");
        print(File.Exists(Application.dataPath + "/net"));
        network = (Accord.Neuro.ActivationNetwork)Accord.Neuro.Network.Load(Application.dataPath + "/net");
        mode = Mode.Test;
    }
    Bitmap[] FilterImage(Texture2D x, int numRotations) {
        //import the img as a bitmap
        System.Drawing.Image i = System.Drawing.Image.FromStream(new MemoryStream(x.EncodeToJPG()));
        Bitmap b = new Bitmap(i);

        //grayscale it
        Grayscale gray = new Grayscale(.33, .33, .33);
        Bitmap grayImg = gray.Apply(b);

        //threshold it
        Threshold threshFilter = new Threshold(150);
        threshFilter.ApplyInPlace(grayImg);

        Invert invFilter = new Invert();
        invFilter.ApplyInPlace(grayImg);

        //new stuff
        ExtractBiggestBlob a = new ExtractBiggestBlob();
        grayImg = a.Apply(grayImg);
        //grayImg.Save(Application.dataPath + "/qh.jpg");
        // invFilter.ApplyInPlace(grayImg);
        threshFilter.ApplyInPlace(grayImg);

       // Bitmap newImg = new Bitmap(grayImg, new Size(30, 30));
        Bitmap[] images = new Bitmap[numRotations*2];
        int index = 0;
        for(int rotation = 0; rotation < numRotations; rotation ++) {
            RotateNearestNeighbor rotateFilter = new RotateNearestNeighbor(rotation, true);
            images[index] = new Bitmap(rotateFilter.Apply(grayImg), new Size(30,30));
            RotateNearestNeighbor rotateFilter1 = new RotateNearestNeighbor(-rotation, true);
            images[index+1] = new Bitmap(rotateFilter1.Apply(grayImg), new Size(30, 30));
            index++;
        }


        return images; //new Bitmap(grayImg, new Size(30, 30));
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

                if(imgNow.GetPixel(i, j).R < 100) {
                    //-1 for black
                    imgAsNumber[i * imgSizexy + j] = -1;
                }
                else if(imgNow.GetPixel(i, j).R >= 100) {
                    //1 for white
                    imgAsNumber[i * imgSizexy + j] = 1;
                }
                else {
                    print(imgNow.GetPixel(i, j).Name);
                    throw new InvalidDataException();
                }

                writeLinetoFile += imgAsNumber[i * imgSizexy + j] + ",";
            }

            // need to implement so as to add things other than just 0.

            yield return null;
        }
        numberArray[currentAddIndex].Add(imgAsNumber);
        indexText.text = "Current Index: " + currentAddIndex
            + "\nIndex 0: " + numberArray[0].Count + " items"
            + "\nIndex 1: " + numberArray[1].Count + " items";


        writeNumbers[currentAddIndex].WriteLine(writeLinetoFile.TrimEnd(','));
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

    void TrainNetwork() {

        //while we train the network we shouldnt allow user input
        sphere.GetComponent<Renderer>().material = red;

        switch(learningMethod) {
            case LearningType.Perceptron:
                StartCoroutine(TrainPerceptron());
                break;
            case LearningType.BackProp:
                StartCoroutine(TrainBackprop());
                break;
            default:
                break;
        }
        

        Bitmap b = AForge.Imaging.Image.FromFile(Application.dataPath + "/test.jpg");
        //Reading A Sample to test Netwok
        double[] sample = new double[imgSizexy * imgSizexy];
        for(int j = 0; j < imgSizexy; j++)
            for(int k = 0; k < imgSizexy; k++) {
                if(b.GetPixel(j, k).R < 100) {
                    //-1 for black
                    sample[j * imgSizexy + k] = -1;
                }
                else if(b.GetPixel(j, k).R > 100) {
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
    IEnumerator TrainPerceptron() {

        //this is the network itself -- what does everything
        network = new Accord.Neuro.ActivationNetwork(
            new Accord.Neuro.BipolarSigmoidFunction(2), //this function maps from -1 to 1 --> alpha is the 2.
            imgSizexy * imgSizexy, //this is the input, which should be the size of our image (in pixels)
           2); //the output --> for now this is 2, it is the number of outputs.

        network.Randomize(); // randomize the weights in the network so we can train it.

        //the learning method AKA how we are teaching the network to learn
        Accord.Neuro.Learning.PerceptronLearning learning = new Accord.Neuro.Learning.PerceptronLearning(network);

        //The rate at which the network learns
        learning.LearningRate = 1;

      
        //creating the input array

        //we check both sets of images, and check which one is smaller.
        int smallestArraySize = numberArray[0].Count <= numberArray[1].Count ? numberArray[0].Count : numberArray[1].Count;

        //the input array which we feed into the network each epoch
        double[][] input = new double[smallestArraySize * 2][];

        //move the inputs from earlier steps into something we can read
        for(int i = 0; i < input.GetLength(0); i++) {
            if(i < smallestArraySize) {
                input[i] = numberArray[0][i];
            }
            else {
                input[i] = numberArray[1][i - smallestArraySize];
            }
        }
        if(input.Length == 0) {
            throw new NotSupportedException("You need to add something for both characters!");
        }
        
        //this is the output array which outputs our result
        double[][] output = new double[smallestArraySize * 2][];
        for(int i = 0; i < smallestArraySize; i++) {
            //not sure if this is correct
            output[i] = new double[3] { 1, 1, 1 };
        }
        for(int i = smallestArraySize; i < smallestArraySize * 2; i++) {
            output[i] = new double[3] { -1, -1, -1 };
        }




        //should stop?
        bool needToStop = false;

        //num iterations
        int iteration = 0;

        //when to stop iterations
        int maxIteration = 5000;

        //run this until we have to stop due to finishing training or max training
        while(!needToStop) {
            epochText.text = "Current Epoch: " + iteration + "/" + maxIteration;
            yield return null;
            double error = learning.RunEpoch(input, output);
            errorText.text = "Percent Error: " + error.ToString("0." + new string('#', 20));
            //learning.LearningRate -= learning.LearningRate / 1000;
            if(error == 0) {
                break;
            }
            else if(iteration < maxIteration)
                iteration++;
            else
                needToStop = true;
            Debug.LogFormat("{0} {1}", error, iteration);
        }
    }
    IEnumerator TrainBackprop() {
        network = new Accord.Neuro.ActivationNetwork(
            new Accord.Neuro.BipolarSigmoidFunction(.5),
            imgSizexy * imgSizexy,
           100, 2);

        network.Randomize();

       
        Accord.Neuro.Learning.ResilientBackpropagationLearning learning = new Accord.Neuro.Learning.ResilientBackpropagationLearning(network);
        learning.LearningRate = .1;
         // learning.Momentum = 0;

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
        if(input.Length == 0) {
            throw new NotSupportedException("You need to add something for both characters!");
        }

        // numberArray[0].CopyTo(input, 0);
        // numberArray[1].CopyTo(input, smallestArraySize);

        double[][] output = new double[smallestArraySize * 2][];
        for(int i = 0; i < smallestArraySize; i++) {
            //not sure if this is correct
            output[i] = new double[2] { 1, 1};
        }
        for(int i = smallestArraySize; i < smallestArraySize * 2; i++) {
            output[i] = new double[2] { -1, -1};
        }




        bool needToStop = false;
        int iteration = 0;
        int maxIteration = 5000;
        while(!needToStop) {
            epochText.text = "Current Epoch: " + iteration + "/" + maxIteration;
            yield return null;
            double error = learning.RunEpoch(input, output);
            errorText.text = "Percent Error: " + error.ToString("0." + new string('#', 20));
            //learning.LearningRate -= learning.LearningRate / 1000;
            if(error == 0) {
                break;
            }
            else if(iteration < maxIteration)
                iteration++;
            else
                needToStop = true;
            Debug.LogFormat("{0} {1}", error, iteration);
        }
    }



    double[] GetSample(Bitmap b) {
        double[] sample = new double[imgSizexy * imgSizexy];
        for(int j = 0; j < imgSizexy; j++)
            for(int k = 0; k < imgSizexy; k++) {


                if(b.GetPixel(j, k).R < 100) {
                    //-1 for black
                    sample[j * imgSizexy + k] = -1;
                }
                else if(b.GetPixel(j, k).R >= 100) {
                    //1 for white
                    sample[j * imgSizexy + k] = 1;
                }
                else {
                    throw new InvalidDataException();
                }


            }
        return sample;
    }

    IEnumerator displayImage() {
        // Bitmap b = new Bitmap(numberArray[0].Count * 30,60);

        /*  Texture2D x = new Texture2D(30, 30);
          for(int i = 0; i < 30; i++) {
              yield return null;
              for(int j = 0; j < 30; j++) {
                  x.SetPixel(i,j, (numberArray[0][0][i *30 + j] == 1) ? UnityEngine.Color.white : UnityEngine.Color.black);
              }
          }*/

        Texture2D x = new Texture2D(numberArray[0].Count * 30, 60);
        for(int k = 0; k < 30; k++) {
            for(int i = 0; i < numberArray[0].Count; i++) {
                yield return null;
                for(int j = 0; j < 30; j++) {
                    x.SetPixel(i * 30 + j, k, (numberArray[0][i][j * 30 + k] == -1) ? UnityEngine.Color.white : UnityEngine.Color.black);
                }
            }
        }
        for(int k = 30; k < 60; k++) {
            for(int i = 0; i < numberArray[0].Count; i++) {
                yield return null;
                for(int j = 0; j < 30; j++) {
                    try {


                        x.SetPixel(i * 30 + j, k, (numberArray[1][i][j * 30 + k] == -1) ? UnityEngine.Color.white : UnityEngine.Color.black);
                    }
                    catch(Exception) {
                        continue;
                    }
                }
            }
        }
        x.filterMode = FilterMode.Point;
        x.Apply();
        Material m = new Material(Shader.Find("Standard"));
        m.SetTexture("_MainTex", x);
        addTextureHere.GetComponent<Renderer>().material = m;
        addTextureHere.GetComponent<Renderer>().sharedMaterial = m;



    }

}

