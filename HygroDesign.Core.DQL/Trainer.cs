using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using static Tensorflow.Binding;
using Tensorflow;
using Tensorflow.NumPy;
using Tensorflow.Keras.Engine;
using static Tensorflow.KerasApi;

namespace HygroDesign.Core.DQL
{
    public class Trainer
    {
        Random random = new Random();

        private static int InputLength = 3;
        private static int OutputLength = 2;

        private static int MaxMemory = 10000;
        private static int BatchSize = 64;

        private static double Alpha = 1;
        private static double Gamma = 0.5;
        private static double Lamda = 0.005;
        private static double InitialEpsilon = 0.8;
        private static double FinalEpsilon = 0.05;


        public List<Functional> Models = new List<Functional>();
        public List<List<Tuple<List<double>, int, double, List<double>>>> Memory = new List<List<Tuple<List<double>, int, double, List<double>>>>();

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

        private Tuple<int, bool> eGreedyPolicy(Tensor qEstimates,double epsilon, int outputLength)
        {
            Random random = new Random();

            if (random.NextDouble() <= epsilon)
            {
                return new Tuple<int, bool>(random.Next(0, outputLength - 1), true);
            }
            else
            {
                return new Tuple<int, bool>((int)np.argmax((NDArray)qEstimates), false);
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

            if (Memory.Count > MaxMemory) Memory.RemoveAt(0);

            List<List<List<double>>> batch = new List<List<List<double>>>();
            if (BatchSize > Memory.Count) batch = (List<List<List<double>>>)Memory.OrderBy(v => random.Next()).Take(Memory.Count);
            else batch = (List<List<List<double>>>)Memory.OrderBy(v => random.Next()).Take(BatchSize);

            List<List<double>> tempStates = new List<List<double>>();
            List<List<double>> tempNextStates = new List<List<double>>();
            foreach (var sample in batch) tempStates.Add(sample[0]);
            foreach (var sample in batch) tempNextStates.Add(sample[3]);

            NDArray states = np.array(tempStates.ToArray());
            NDArray nextStates = np.array(tempNextStates.ToArray());

            Tensors qsa = Models[agentID].predict(states);
            Tensors qsad = Models[agentID].predict(nextStates);

            var x = np.zeros(shape: (batch.Count, InputLength));
            var y = np.zeros(shape: (batch.Count, OutputLength));

            for (int j = 0; j < batch.Count; j++)
            {
                int index = j;
                List<List<double>> sample = batch[j];
                NDArray stIn = np.array(sample[0].ToArray());
                int actn = (int)sample[1][0];
                double rwrd = sample[2][0];
                List<double> nxtSt = sample[3];
                Tensors currentQ = qsa[index];

                currentQ[actn] = Alpha * (rwrd + Gamma * np.amax((NDArray)qsad[index]));

                x[index] = stIn;
                y[index] = (NDArray)currentQ;
            }

            Models[agentID].fit(x, y);

            return FinalEpsilon + (1 - Lamda) * (agentEpsilon - FinalEpsilon);
        }
    }
}
