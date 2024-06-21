namespace Yelp_API.Models;

public class SentimentAnalyzer{
    private readonly MLContext? _mlContext;
    private readonly PredictionEngine<SentimentData, SentimentPrediction>? _predictionEngine;

    public SentimentAnalyzer()
    {
        _mlContext = new MLContext();

        var dataProcessPipeline = _mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Features", inputColumnName: nameof(SentimentData.SentimentText));
        var trainer = _mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: nameof(SentimentPrediction.Prediction), featureColumnName: "Features");
        var trainingPipeline = dataProcessPipeline.Append(trainer);

        var data = _mlContext.Data.LoadFromTextFile<SentimentData>("sentiment_labelled_sentences.txt", hasHeader: false);
        var model = trainingPipeline.Fit(data);
        _predictionEngine = _mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(model);
    }

    public Sentiment AnalyzeSentiment(string reviewText)
    {
        var sentimentData = new SentimentData { SentimentText = reviewText };
        var prediction = _predictionEngine!.Predict(sentimentData);

        return new Sentiment(prediction.Score, prediction.Prediction ? "Positive" : "Negative");
    }
}
