using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Drawing;
using AForge;
using AForge.Imaging.Filters;

public class TestBMP : MonoBehaviour {

    //size of resized image, should be 30 or less
    int imgSizexy = 30;
    
    //camera which we will be writing from
    public Camera c;

    //stream to write pixel data from
    private StreamWriter writeNumbers;

    // Use this for initialization
    void Start() {

        //write numbers to this file
        writeNumbers = File.CreateText(Application.dataPath + "/1.csv");
        
        Bitmap b = FilterImage(TakeCameraSnapshot());

         b.Save(Application.dataPath + "/HIIII.jpg");

        StartCoroutine(SaveImageAsInt(b));



    }

    public void OnApplicationQuit() {
        writeNumbers.Close();
    }

  
    Bitmap FilterImage(Texture2D x) {
        //import the img as a bitmap
        Bitmap b = new Bitmap(Image.FromStream(new MemoryStream(x.EncodeToJPG())), new Size(30, 30));
        // new Bitmap(Image.FromFile(path), new Size(imgSizexy, imgSizexy));

        //grayscale it
        Grayscale gray = new Grayscale(.33, .33, .33);
        Bitmap grayImg = gray.Apply(b);

        //threshold it
        Threshold filter = new Threshold(100);
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
                    //0 for black
                    imgAsNumber[i * imgSizexy + j] = 0;
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
        writeNumbers.WriteLine(writeLinetoFile.TrimEnd(','));
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

    void Other() {
        /*start copied*/
        //AForge.Neuro.ActivationNetwork network = new AForge.Neuro.ActivationNetwork(new AForge.Neuro.BipolarSigmoidFunction(2), imgSizexy * imgSizexy, 3);
        //network.Randomize();
        //AForge.Neuro.Learning.PerceptronLearning learning = new AForge.Neuro.Learning.PerceptronLearning(network);
        //learning.LearningRate = 1;
        /*end copied*/
        double[][] input = new double[4][];
        for(int i = 0; i < 4; i++) {
            input[i] = new double[imgSizexy * imgSizexy];
        }


        //   Bitmap b = FilterImage(Application.dataPath + "/cat.jpg");
        //   b.Save(Application.dataPath + "/hi.jpg");
        // StartCoroutine(SaveNewFiles());






        ////text file with pixel values
        //var sr = File.CreateText(Application.dataPath + "/test");

        ////for the x pixels
        //for(int i = 0; i < imgSizexy; i++) {

        //    //print string
        //    string writeLinetoFile = "";

        //    //for the y pixels
        //    for(int j = 0; j < imgSizexy; j++) {
        //        if(grayImg.GetPixel(i, j).Name.Contains("ff000000")) {
        //            //0 for black
        //            img[i * imgSizexy + j] = 0;
        //        }
        //        else if(grayImg.GetPixel(i, j).Name.Contains("ffffffff")) {
        //            //1 for white
        //            img[i * imgSizexy + j] = 1;
        //        }
        //        else {
        //            throw new InvalidDataException();
        //        }

        //        writeLinetoFile += img[i * imgSizexy + j];
        //    }
        //    sr.WriteLine(writeLinetoFile);
        //}
        //sr.Close();






        //    bool needToStop = false;
        //    int iteration = 0;
        //    while(!needToStop) {
        //        double error = learning.RunEpoch(input, new double[9][] {
        //            new double[3] { 1, -1, -1 },new double[3] { 1, -1, -1 },new double[3] { 1, -1, -1 },//A
        //            new double[3] { -1, 1, -1 },new double[3] { -1, 1, -1 },new double[3] { -1, 1, -1 },//B
        //            new double[3] { -1, -1, 1 },new double[3] { -1, -1, 1 },new double[3] { -1, -1, 1 }}//C
        //                                                                                                 /*new double[9][]{ input[0],input[0],input[0],input[1],input[1],input[1],input[2],input[2],input[2]}*/
        //            );
        //        //learning.LearningRate -= learning.LearningRate / 1000;
        //        if(error == 0)
        //            break;
        //        else if(iteration < 1000)
        //            iteration++;
        //        else
        //            needToStop = true;
        //        System.Diagnostics.Debug.WriteLine("{0} {1}", error, iteration);
        //    }
        //    Bitmap b = AForge.Imaging.Image.FromFile(path + "\\b1.bmp");
        //    //Reading A Sample to test Netwok
        //    double[] sample = new double[900];
        //    for(int j = 0; j < imgSizexy; j++)
        //        for(int k = 0; k < imgSizexy; k++) {
        //            if(b.GetPixel(j, k).ToKnownColor() == KnownColor.White) {
        //                sample[j * imgSizexy + k] = -1;
        //            }
        //            else
        //                sample[j * imgSizexy + k] = 1;
        //        }
        //    foreach(double d in network.Compute(sample))
        //        System.Diagnostics.Debug.WriteLine(d);//Output is Always C = {-1,-1,1}
        //}


    }


}
