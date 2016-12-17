using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using AForge;
using AForge.Imaging.Filters;
using System;
using UnityEngine.UI;
public class ThreadedTrainer {
    public Thread t;

    //the network itself
    public AForge.Neuro.ActivationNetwork network;
    public string epoch = "", errorStr = "";


    public void trainPerceptron(int imgSizexy, List<double[]>[] numberArray) {

        //this is the network itself -- what does everything
        network = new AForge.Neuro.ActivationNetwork(
            new AForge.Neuro.BipolarSigmoidFunction(2), //this function maps from -1 to 1 --> alpha is the 2.
            imgSizexy * imgSizexy, //this is the input, which should be the size of our image (in pixels)
           2); //the output --> for now this is 2, it is the number of outputs.

        network.Randomize(); // randomize the weights in the network so we can train it.

        //the learning method AKA how we are teaching the network to learn
        AForge.Neuro.Learning.PerceptronLearning learning = new AForge.Neuro.Learning.PerceptronLearning(network);

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
            epoch = "Current Epoch: " + iteration + "/" + maxIteration;
            
            double error = learning.RunEpoch(input, output);
            errorStr = "Percent Error: " + error.ToString("0." + new string('#', 20));
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

    public void trainBackprop() {

    }
    public void train(int which, int imgSizexy, List<double[]>[] numberArray) {
        if(which == 0) {
            t = new Thread(() => trainPerceptron(imgSizexy,numberArray));
            t.Start();
            
        }
        else {
            t = new Thread(() => trainBackprop());
            t.Start();
        }

    }

}
