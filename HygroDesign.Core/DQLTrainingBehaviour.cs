using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Collections;

using Tensorflow;
using Tensorflow.NumPy;
using Tensorflow.Keras.Engine;
using static Tensorflow.KerasApi;
using System.IO;

//using NumSharp;


namespace HygroDesign.Core
{
    public class DQLTraining
    {
        private static int InputLength;
        private static int OutputLength;

        private static double Alpha = 1;
        private static double Gamma = 0.5;
        private static double Lamda = 0.005;
        private static double InitialEpsilon = 0.8;
        private static double FinalEpsilon = 0.05;
        private static int MaxMemory = 10000;
        private static int BatchSize = 64;
        private List<List<List<double>>> Memory = new List<List<List<double>>>();
        private double Epsilon = InitialEpsilon;

        private static int Iterations;
        private static int Timeout = 10;
        private static int ModelSaveFrequency;
        private static string ModelSavePath;

        public List<double> Inputs = new List<double>();

        public DQLTraining(List<double>rawInputs, int numOutputs, string modelSavePath, int iterations, int modelSaveFrequency)
        {
            ModelSavePath = modelSavePath;
            Iterations = iterations;
            ModelSaveFrequency = modelSaveFrequency;
            OutputLength = numOutputs;
            InputLength = rawInputs.Count;
            Inputs = rawInputs;
        }

        private Functional BuildModel()
        {
            var inputs = keras.Input(shape: (InputLength));

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
                units: OutputLength
                );

            var model = keras.Model(
                inputs: inputs,
                outputs: (Tensor)outputs
                );

            model.compile("adam", "mse", null);

            return model;
        }

        private Tuple<int,bool> eGreedyPolicy(Tensor qEstimates)
        {
            Random random = new Random();

            if(random.NextDouble() <= Epsilon)
            {
                return new Tuple<int,bool>(random.Next(0,OutputLength-1),true);
            }
            else
            {
                return new Tuple<int, bool>((int)np.argmax((NDArray)qEstimates), false);
            }
        }

        public void Update()
        {

        }

        private void MainLoop()
        {
            Random random = new Random();

            Model model = BuildModel();

            List<double> prevState = new List<double>();
            List<double> resetState = new List<double>();
            

            for (int i = 0; i < Iterations; i++)
            {
                

                List<double> stateIn = Inputs;
                if (i == 0) resetState = stateIn;

                Tensor qEstimates = model.predict(np.array(stateIn.ToArray()));
                Tuple<int, bool> greedy = eGreedyPolicy(qEstimates);
                int action = greedy.Item1;
                bool randomAct = greedy.Item2;

                //this is where the action is sent to gh

                //this is where the reward is recieved from gh
                double reward = 1.0;

                List<List<double>> memorySample = new List<List<double>>();
                List<double> actions = new List<double>();
                actions.Add(action);
                List<double> rewards = new List<double>();
                rewards.Add(reward);
                memorySample.Add(prevState);
                memorySample.Add(actions);
                memorySample.Add(rewards);
                memorySample.Add(stateIn);
                Memory.Add(memorySample);

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

                Tensors qsa = model.predict(states);
                Tensors qsad = model.predict(nextStates);

                var x = np.zeros(shape: (batch.Count, InputLength));
                var y = np.zeros(shape:(batch.Count, OutputLength));

                for(int j = 0; j < batch.Count; j++)
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

                    model.fit(x, y);

                    Epsilon = FinalEpsilon + (1 - Lamda) * (Epsilon - FinalEpsilon);

                }
                prevState = stateIn;
                
                if(i % ModelSaveFrequency == 0 && i > 0)
                {
                    model.save(Path.Combine(ModelSavePath, String.Format("{0}.h5", i)));
                }
            }
        }
    }
}