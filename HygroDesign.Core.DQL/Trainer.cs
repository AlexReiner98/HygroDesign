using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Python.Runtime;

//using static Tensorflow.Binding;
//using Tensorflow;
//using Tensorflow.Keras;
//using Tensorflow.NumPy;
//using Tensorflow.Keras.Engine;
//using static Tensorflow.KerasApi;
//using Tensorflow.Keras.Models;

using Keras;
using Keras.Models;
using Keras.Layers;
using Numpy;



namespace HygroDesign.Core.DQL
{
    public class AgentModel
    {
        Random random = new Random();
        public int InputLength;
        public int OutputLength;
        public string ModelSummary;

        private static int MaxMemory = 10000;
        private static int BatchSize = 16;

        private static double Alpha = 1;
        private static double Gamma = 0.5;
        private static double Lamda = 0.005;
        private static double InitialEpsilon = 0.8;
        private static double FinalEpsilon = 0.05;

        Sequential Model;

        public List<Tuple<List<double>, int, double, List<double>>> Memory = new List<Tuple<List<double>, int, double, List<double>>>();

        public AgentModel(int inputLength, int outputLength)
        {

            Sequential model = new Sequential();

            model.Add(new Dense(30, activation:"relu", input_shape: new Shape(inputLength)));
            model.Add(new Dense(outputLength, null, "relu"));

            model.Compile(loss: "mse",
            optimizer: "adam",
            metrics: new[] { "mae" });

            Model = model;
            InputLength = inputLength;
            OutputLength = outputLength;
        }


        private Tuple<int, bool> eGreedyPolicy(NDarray qEstimates, double epsilon, int outputLength)
        {
            Random random = new Random();

            if (random.NextDouble() <= epsilon)
            {
                return new Tuple<int, bool>(random.Next(0, outputLength), true);
            }
            else
            {
                return new Tuple<int, bool>((int)np.argmax(qEstimates), false);
            }
        }

        public int GetAction(List<double> stateIn, double agentEpsilon,int outputLength)
        {
            var stateInArray = new NDarray(stateIn.ToArray());
            NDarray qEstimates = Model.Predict(stateInArray, batch_size: 16, steps: 1);
            Tuple<int, bool> greedy = eGreedyPolicy(qEstimates,agentEpsilon,outputLength);
            return greedy.Item1;
        }

        public double Train(int agentID, Tuple<List<double>, int, double, List<double>> memorySample, double agentEpsilon)
        {
            Memory.Add(memorySample);

            if (Memory.Count > MaxMemory) Memory.RemoveAt(0);

            List<Tuple<List<double>, int, double, List<double>>> batch1 = new List<Tuple<List<double>, int, double, List<double>>>(Memory);
            batch1.OrderBy(v => random.Next());
            List<Tuple<List<double>, int, double, List<double>>> batch;
            if (BatchSize > Memory.Count) batch = batch1.Take(Memory.Count).ToList();
            else batch = batch1.OrderBy(v => random.Next()).Take(BatchSize).ToList();


            double[,] tempStates = new double[batch.Count,InputLength];
            double[,] nextStates = new double[batch.Count,InputLength];
            for (int i = 0; i < batch.Count; i++)
            {
                for (int j = 0; j < batch[j].Item1.Count; j++)
                {
                    tempStates[i,j] = batch[i].Item1[j];
                    nextStates[i,j] = batch[i].Item4[j];
                }
            }

            NDarray states = new NDarray(tempStates);
            NDarray next = new NDarray(nextStates);

            NDarray qsa = Model.Predict(states, batch_size: BatchSize, steps: 1);
            NDarray qsad = Model.Predict(next, batch_size: BatchSize, steps: 1);

            var x = np.zeros(shape: (batch.Count, InputLength));
            var y = np.zeros(shape: (batch.Count, OutputLength));

            for (int j = 0; j < batch.Count; j++)
            {
                //int index = j;
                Tuple<List<double>, int, double, List<double>> sample = batch[j];
                NDarray stIn = new NDarray(sample.Item1.ToArray());
                int actn = sample.Item2;
                double rwrd = sample.Item3;
                List<double> nxtSt = sample.Item4;
                NDarray currentQ = qsa[j];

                var newQ = Alpha * (rwrd + Gamma * np.amax(qsad[j]));
                currentQ[0,actn] = newQ;

                x[j] = stIn;
                y[j] = currentQ;
            }

            Model.Fit(x, y,batch_size: BatchSize);

            return FinalEpsilon + (1 - Lamda) * (agentEpsilon - FinalEpsilon);
        }
    }
}
