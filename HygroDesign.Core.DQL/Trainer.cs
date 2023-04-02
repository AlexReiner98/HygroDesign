using System.Collections.Generic;
using Keras;
using Keras.Models;
using Keras.Layers;
using Numpy;
using System;
using System.Linq;



namespace HygroDesign.Core.DQL
{

    public class AgentModel
    {
        private Model model;
        private int InputLength;
        private int OutputLength;
        private List<Tuple<double[], int, double, double[]>> Memory = new List<Tuple<double[], int, double, double[]>>();

        private static int batchSize = 32;

        private static double learningRate = 0.001;
        private static double discountFactor = 0.99;

        public double epsilon = 1;
        private static double epsilonMin = 0.1;
        private static double epsilonDecay = 0.9;
        private int totalSteps = 0;

        private Random random;

        public AgentModel(int inputLength, int outputLength)
        {
            random = new Random();
            // Define the model architecture
            Environment.SetEnvironmentVariable("KERAS_BACKEND", "tensorflow");

            // Set the Keras backend logger to use the .NET logger
            Keras.Keras.DisablePySysConsoleLog = true;


            // Define the model architecture
            var input = new Input(shape: new Keras.Shape(inputLength));
            var dense1 = new Dense(units: 64, activation: "relu");
            var dense1Output = dense1.Set(input);
            var dense2 = new Dense(units: 64, activation: "relu");
            var dense2Output = dense2.Set(dense1Output);
            var output = new Dense(units: outputLength, activation: "linear");
            var outputOutput = output.Set(dense2Output);

            // Create a new model with the symbolic tensor as the output
            var tfModel = new Model(inputs: new BaseLayer[] { input }, outputs: new BaseLayer[] { outputOutput });

            // Compile the model with a mean squared error loss function and an Adam optimizer
            tfModel.Compile(optimizer: "adam", loss: "mse");

            InputLength = inputLength;
            OutputLength = outputLength;
            model = tfModel;
        }

        public void Train(double[] state, int action, double reward, double[] newState)
        {
            // Store the experience in the replay buffer
            Memory.Add(new Tuple<double[], int, double, double[]>(state, action, reward, newState));

            // Sample a batch of experiences from the replay buffer
            var batch = Sample(Memory, batchSize: 32);

            // Unpack the batch into separate arrays for each component
            double[][] states;
            int[] actions;
            double[] rewards;
            double[][] newStates;
            UnpackTupleList(batch, out states, out actions, out rewards, out newStates);

            // Convert the input and target data to NumPy arrays
            var npStates = np.array(states.SelectMany(x => x).ToArray());
            var reShapedStates = npStates.reshape(states.Length, states[0].Length);
            var npActions = np.array(actions.ToArray());
            var npRewards = np.array(rewards.ToArray());
            var npNewStates = np.array(newStates.SelectMany(x => x).ToArray());
            var reShapedNewStates = npNewStates.reshape(newStates.Length, newStates[0].Length);

            // Calculate the Q value targets for each state
            var futureQValues = model.Predict(reShapedNewStates);
            var targets = npRewards + discountFactor * np.amax(futureQValues, axis: 1);


            // Train the model on the input and target data
            model.TrainOnBatch(reShapedStates, npActions, targets);
        }

        public double[][] GetPrediction(double[][] states)
        {
            // Flatten the input data and convert it to a NumPy array
            var npStates = np.array(states.SelectMany(x => x).ToArray());

            // Reshape the NumPy array to have the expected shape
            var reshaped = npStates.reshape(states.Length, states[0].Length);

            // Predict the output using the model
            var predictions = model.Predict(reshaped);

            // Convert the output to a C# array and return it
            return ConvertNpArrayToCsArray(predictions);
        }

        public int GetAction(double[] states)
        {
            double[][] stateArr = new double[1][];
            stateArr[0] = states;
            double[][] predictions = GetPrediction(stateArr);

            double randomValue = random.NextDouble();
            double epsilonThreshold = epsilonMin + (1.0 - epsilonMin) * Math.Pow(Math.Max(0.0, epsilonDecay), totalSteps);
            bool useRandom = randomValue < epsilonThreshold;
            totalSteps++;

            int action;
            if (useRandom)
            {
                action = random.Next(OutputLength);
            }
            else
            {
                action = Array.IndexOf(predictions[0], predictions[0].Max());
            }

            // Decay the epsilon value
            epsilon = Math.Max(epsilonMin, epsilon * epsilonDecay);

            return action;
        }


        //////////////////////
        //                  //
        //  Static methods  //
        //                  //
        //////////////////////

        public static List<Tuple<double[], int, double, double[]>> Sample(List<Tuple<double[], int, double, double[]>> memory, int batchSize)
        {
            var samples = new List<Tuple<double[], int, double, double[]>>();

            if (memory.Count < batchSize)
            {
                samples = memory;
                ShuffleList(samples);
                return samples;
            }

            var indices = Enumerable.Range(0, memory.Count).ToList();
            ShuffleList(indices);

            for (var i = 0; i < batchSize; i++)
            {
                var index = indices[i];
                samples.Add(memory[index]);
            }
            return samples;
        }

        public static void ShuffleList<T>(List<T> list)
        {
            Random rand = new Random();

            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }

        public static void UnpackTupleList(List<Tuple<double[], int, double, double[]>> tupleList,
                                    out double[][] prevStates,
                                    out int[] actions,
                                    out double[] rewards,
                                    out double[][] currentStates)
        {
            prevStates = new double[tupleList.Count][];
            actions = new int[tupleList.Count];
            rewards = new double[tupleList.Count];
            currentStates = new double[tupleList.Count][];

            for (int i = 0; i < tupleList.Count; i++)
            {
                prevStates[i] = tupleList[i].Item1;
                actions[i] = tupleList[i].Item2;
                rewards[i] = tupleList[i].Item3;
                currentStates[i] = tupleList[i].Item4;
            }
        }

        public double[][] ConvertNpArrayToCsArray(dynamic npArray)
        {
            int numRows = npArray.shape[0];
            int numCols = npArray.shape[1];

            
            double[][] result = new double[numRows][];
            for (int i = 0; i < numRows; i++)
            {
                result[i] = npArray.GetData<double>();
            }
            

            //double[][] result = new double[1][] { npArray.GetData<double>() };
            return result;
        }
    }
}


