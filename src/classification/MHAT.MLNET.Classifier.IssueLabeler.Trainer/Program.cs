﻿using MHAT.MLNET.Classifier.IssueLabeler.Core.Model;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MHAT.MLNET.Classifier.IssueLabeler.Trainer
{
    class Program
    {
        private static string AppPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);

        private static string DataPath => Path.Combine(AppPath, "Data", "corefx-issues-train.tsv");

        private static string TestPath => Path.Combine(AppPath, "Data", "corefx-issues-test.tsv");

        private static string ModelPath => Path.Combine(AppPath, "IssueLabelerModel.zip");

        static async Task Main(string[] args)
        {
            await TrainAsync();

            Console.ReadLine();
        }

        private static async Task TrainAsync()
        {
            Console.WriteLine("========準備訓練資料=============");

            var pipeline = new LearningPipeline();

            pipeline.Add(new TextLoader(DataPath).CreateFrom<GitHubIssue>(useHeader: true));

            // 把Area轉換成為Dictionary數字
            pipeline.Add(new Dictionarizer(("Area", "Label")));

            // 把兩個用來訓練的欄位變成數字的vector
            pipeline.Add(new TextFeaturizer("Title", "Title"));
            pipeline.Add(new TextFeaturizer("Description", "Description"));

            // Title 和 Description合并變成訓練的闌尾
            pipeline.Add(new ColumnConcatenator("Features", "Title", "Description"));

            // 使用StochasticDualCoordinateAscent演算法
            pipeline.Add(new StochasticDualCoordinateAscentClassifier());

            // 把判斷出來的數字轉回文字版本
            pipeline.Add(new PredictedLabelColumnOriginalValueConverter()
                { PredictedLabelColumn = "PredictedLabel" });


        }
    }
}
