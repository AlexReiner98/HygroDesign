using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using static Tensorflow.Binding;
using Tensorflow;
using Tensorflow.Keras;
using Tensorflow.NumPy;
using Tensorflow.Keras.Engine;
using static Tensorflow.KerasApi;

namespace HygroDesign.Core.DQL
{
    public class Trainer
    {
        Random random = new Random();

        private static int InputLength = 2;
        private static int OutputLength = 2;

        private static int MaxMemory = 10000;
        private static int BatchSize = 16;

        private static double Alpha = 1;
        private static double Gamma = 0.5;
        private static double Lamda = 0.005;
        private static double InitialEpsilon = 0.8;
        private static double FinalEpsilon = 0.05;


        public List<Model> Models = new List<Model>();
        public List<List<Tuple<List<double>, int, double, List<double>>>> Memory = new List<List<Tuple<List<double>, int, double, List<double>>>>();

        /*
        public void BuildModel(int inputLength, int outputLength)
        {
            var inputs = keras.Input(shape: (inputLength));

            var x = keras.layers.Dense(
            64,
            activation: keras.activations.Relu,
            use_bias: true,
            kernel_initializer: null,
            bias_initializer: null);

            x = keras.layers.Dense(
            32,
            activation: keras.activations.Relu,
            use_bias: true,
            kernel_initializer: null,
            bias_initializer: null
            );

            x = keras.layers.Dense(
            16,
            activation: keras.activations.Relu,
            use_bias: true,
            kernel_initializer: null,
            bias_initializer: null
            );
            var outputs = keras.layers.Dense(
                units: outputLength
                );

            var model = keras.Model(
                inputs: inputs,
                outputs: (Tensor)outputs
                );

            model.compile("adam", "mse", null);

            Models.Add(model);
        }
        */
        public void BuildModel(int inputLength, int outputLength)
        {
            var inputs = keras.Input(shape: inputLength);
            var outputs = keras.Input(shape: outputLength);

            List<ILayer> layers = new List<ILayer>();
            layers.add(keras.layers.Dense(64,
            input_shape: inputLength,
            activation: keras.activations.Relu,
            use_bias: true,
            kernel_initializer: null,
            bias_initializer: null));

            layers.add(keras.layers.Dense(
            32,
            activation: keras.activations.Relu,
            use_bias: true,
            kernel_initializer: null,
            bias_initializer: null
            ));

            layers.add(keras.layers.Dense(
            16,
            activation: keras.activations.Relu,
            use_bias: true,
            kernel_initializer: null,
            bias_initializer: null
            ));

            Tensorflow.Keras.ArgsDefinition.ModelArgs args = new Tensorflow.Keras.ArgsDefinition.ModelArgs();
            args.Inputs = inputs;
            args.Outputs = outputs;
            var model = new Model(args);

            

            /*
            var model = keras.Model(
               inputs: inputs,
               outputs: outputs
               );
            */

            string[] metrics = new string[1];
            metrics[0] = "mae";

            model.compile("rmsprop", "mse", metrics);
            Models.Add(model);
        }


            private Tuple<int, bool> eGreedyPolicy(Tensor qEstimates,double epsilon, int outputLength)
        {
            Random random = new Random();

            if (random.NextDouble() <= epsilon)
            {
                return new Tuple<int, bool>(random.Next(0, outputLength - 1), true);
            }
            else
            {
                return new Tuple<int, bool>((int)np.argmax((NDArray)qEstimates.ToArray<double>()), false);
            }
        }

        public int GetAction(List<double> stateIn, int agentID, double agentEpsilon,int outputLength)
        {
            Tensor qEstimates = Models[agentID].predict(np.array(stateIn.ToArray()));
            Tuple<int, bool> greedy = eGreedyPolicy(qEstimates,agentEpsilon,outputLength);
            return greedy.Item1;
        }

        public double Train(int agentID, Tuple<List<double>, int, double, List<double>> memorySample, double agentEpsilon)
        {
            Memory[agentID].Add(memorySample);

            if (Memory[agentID].Count > MaxMemory) Memory[agentID].RemoveAt(0);

            List<Tuple<List<double>, int, double, List<double>>> batch1 = new List<Tuple<List<double>, int, double, List<double>>>(Memory[agentID]);
            batch1.OrderBy(v => random.Next());
            List<Tuple<List<double>, int, double, List<double>>> batch;
            if (BatchSize > Memory[agentID].Count) batch = batch1.Take(Memory[agentID].Count).ToList();
            else batch = batch1.OrderBy(v => random.Next()).Take(BatchSize).ToList();

            /*
            List<Tuple<List<double>, int, double, List<double>>> batch = new List<Tuple<List<double>, int, double, List<double>>>();
            if (BatchSize > Memory[agentID].Count) batch = (List<Tuple<List<double>, int, double, List<double>>>)Memory[agentID].OrderBy(v => random.Next()).Take(Memory[agentID].Count);
            else batch = (List<Tuple<List<double>, int, double, List<double>>>)Memory[agentID].OrderBy(v => random.Next()).Take(BatchSize);
            */

            double[,] tempStates = new double[batch.Count,InputLength];
            double[,] nextStates = new double[batch.Count,InputLength];
            for (int i = 0; i < batch.Count; i++)
            {
                for (int j = 0; j < batch[i].Item1.Count; j++)
                {
                    tempStates[i,j] = batch[i].Item1[j];
                    nextStates[i,j] = batch[i].Item4[j];
                }
            }

            /*
            List<double[]> temptempStates = new List<double[]>();
            List<double[]> tempNextStates = new List<double[]>(); 
            foreach (var sample in batch) temptempStates.Add(sample.Item1.ToArray());
            tempStates = temptempStates.ToArray();
            foreach (var sample in batch) tempNextStates.Add(sample.Item4.ToArray());
            */

            NDArray states = np.array(tempStates);
            NDArray next = np.array(nextStates);

            Tensor qsa = Models[agentID].predict(states, batch_size: BatchSize, steps: 1);
            Tensor qsad = Models[agentID].predict(next, batch_size: BatchSize, steps: 1);

            var x = np.zeros(shape: (batch.Count, InputLength));
            var y = np.zeros(shape: (batch.Count, OutputLength));

            for (int j = 0; j < batch.Count; j++)
            {
                //int index = j;
                Tuple<List<double>, int, double, List<double>> sample = batch[j];
                NDArray stIn = np.array(sample.Item1.ToArray());
                int actn = sample.Item2;
                double rwrd = sample.Item3;
                List<double> nxtSt = sample.Item4;
                NDArray currentQ = np.array(qsa[j].ToArray<double>());

                double newQ = Alpha * (rwrd + Gamma * np.amax(qsad[j].ToArray<double>()));
                currentQ[actn] = newQ;

                x[j] = stIn;
                y[j] = currentQ;
            }

            Models[agentID].fit(x, y,batch_size: BatchSize);

            return FinalEpsilon + (1 - Lamda) * (agentEpsilon - FinalEpsilon);
        }

        public double[] GetRow(double[,] matrix, int rowNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(1))
                    .Select(x => matrix[rowNumber, x])
                    .ToArray();
        }
    }
}
