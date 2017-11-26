using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolvMaps
{
    public class Population
    {
        public DNA[] pop = new DNA[1000];
    }

    public class GeneticAlgorithm
    {
        public List<DNA> population { get; private set; }
        public int Generation { get; private set; }
        public float BestFitness { get; private set; }
        public char[] BestGenes { get; private set; }

        public int Elitism;
        public float MutationRate = 0.01f;

        private List<DNA> newPopulation;
        private Random random;
        private float fitnessSum;
        private int dnaSize;
        private Func<int, float> fitnessFunc;

        public GeneticAlgorithm(int populationSize, int dnaSize, Random random, int elitism, float mutationRate, Func<int, float> fitnessFunc)
        {
            Generation = 1;
            Elitism = elitism;
            MutationRate = mutationRate;
            population = new List<DNA>(populationSize);
            newPopulation = new List<DNA>(populationSize);
            this.random = random;
            this.dnaSize = dnaSize;
            this.fitnessFunc = fitnessFunc;

            BestGenes = new char[dnaSize];

            for (int i = 0; i < populationSize; i++)
            {
                population.Add(new DNA(dnaSize, random, fitnessFunc, true));
            }
        }

        public void NewGeneration(int numNewDNA = 0, bool crossoverNewDNA = false)
        {
            int finalCount = population.Count + numNewDNA;

            if (finalCount <= 0)
            {
                return;
            }

            if (population.Count > 0)
            {
                CalculateFitness();
                population.Sort(CompareDNA);
            }
            newPopulation.Clear();

            for (int i = 0; i < population.Count; i++)
            {
                if (i < Elitism && i < population.Count)
                {
                    newPopulation.Add(population[i]);
                }
                else if (i < population.Count || crossoverNewDNA)
                {
                    DNA parent1 = ChooseParent();
                    DNA parent2 = ChooseParent();

                    DNA child = parent1.Crossover(parent2);

                    child.Mutate(MutationRate);

                    newPopulation.Add(child);
                }
                else
                {
                    newPopulation.Add(new DNA(dnaSize, random, fitnessFunc, true));
                }
            }

            List<DNA> tmpList = population;
            population = newPopulation;
            newPopulation = tmpList;

            Generation++;
        }

        private int CompareDNA(DNA a, DNA b)
        {
            if (a.Fitness > b.Fitness)
                return -1;
            else if (a.Fitness < b.Fitness)
                return 1;
            else
                return 0;
        }

        private void CalculateFitness()
        {
            fitnessSum = 0;
            DNA best = population[0];

            for (int i = 0; i < population.Count; i++)
            {
                fitnessSum += population[i].fitnessFunc(i);

                if (population[i].Fitness > best.Fitness)
                {
                    best = population[i];
                }
            }

            BestFitness = best.Fitness;
            best.Genes.CopyTo(BestGenes, 0);
        }

        private DNA ChooseParent()
        {
            double randomNumber = random.NextDouble() * fitnessSum;

            for (int i = 0; i < population.Count; i++)
            {
                if (randomNumber < population[i].Fitness)
                {
                    return population[i];
                }

                randomNumber -= population[i].Fitness;
            }

            return null;
        }
    }



    public class DNA
    {
        public char[] Genes { get; private set; }
        public float Fitness { get; private set; }
        Random rand;
        public Func<int, float> fitnessFunc;

        string validCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ,.|!#$%&/()=? ";


        public DNA(int size, Random rand,  Func<int, float> fitnessFunc, bool initialize = true)
        {
            Genes = new char[size];
            this.rand = rand;
            this.fitnessFunc = fitnessFunc;

            if (initialize)
            {
                for(int i = 0; i < Genes.Length; i++)
                {
                    Genes[i] = getRandomGene(validCharacters);
                }
            }
        }

        public char getRandomGene(string validCharacters)
        {
            int i = rand.Next(validCharacters.Length);
            return validCharacters[i];
        }

        public DNA Crossover (DNA otherParent)
        {
            DNA child = new DNA(Genes.Length, rand, fitnessFunc, false);

            for (int i = 0; i < Genes.Length; i++)
            {
                child.Genes[i] = rand.NextDouble() < 0.5 ? Genes[i] : otherParent.Genes[i];
            }
            return child;
        }

        public void Mutate (float rate)
        {
            for (int i = 0; i < Genes.Length; i++)
            {
                if(rand.NextDouble() < rate)
                {
                    Genes[i] = getRandomGene(validCharacters);
                }
            }
        }

    }

    public class Test
    {
        string targetString = "Erik, Eric, and Dominic are Making a Brain.";
        int populationSize = 200;
        float mutationRate = 0.01f;
        int elitism = 5;

        GeneticAlgorithm ga;
        System.Random rand;

        public Test()
        {
            rand = new System.Random();
            ga = new GeneticAlgorithm(populationSize, targetString.Length, rand, elitism, mutationRate, FitnessFunc);
        }

        public void Run()
        {
            bool keepRunning = true;

            while (keepRunning)
            {
                ga.NewGeneration();

                if (ga.BestFitness == 1)
                    keepRunning = false;
            }
            Print(ga.BestGenes, ga.BestFitness, ga.Generation, ga.population.Count);
        }

        private void Print(char[] bestGenes, float bestFitness, int generation, int popSize)
        {
            Console.WriteLine("Target Text = " + targetString);
            Console.WriteLine("Best Text = " + bestGenes);
            Console.WriteLine("Best Fitness = " + bestFitness);
            Console.WriteLine("Generation = " + generation);
        }

        private float FitnessFunc(int index)
        {
            float score = 0;
            DNA dna = ga.population[index];

            for (int i = 0; i < dna.Genes.Length; i++)
            {
                if (dna.Genes[i] == targetString[i])
                {
                    score += 1;
                }
            }

            score /= targetString.Length;

            score = (float)(Math.Pow(2, score) - 1) / (2 - 1);

            return score;
        }

    }
}
