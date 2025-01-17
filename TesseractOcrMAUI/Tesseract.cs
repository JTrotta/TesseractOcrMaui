﻿using TesseractOcrMaui.Results;
using TesseractOcrMaui.Tessdata;
using System.Runtime.Versioning;
using Microsoft.Maui.Animations;

namespace TesseractOcrMaui;

/// <summary>
/// High-level functionality with Tesseract ocr. Default implementation for ITesseract interface.
/// </summary>
public class Tesseract : ITesseract
{
    /// <summary>
    /// New instance of High-level functionality with Tesseract ocr.
    /// </summary>
    /// <param name="tessDataProvider">Interface object used to provide needed paths to recognizion process.</param>
    /// <param name="logger"></param>
    public Tesseract(ITessDataProvider tessDataProvider, ILogger<ITesseract>? logger)
    {
        TessDataProvider = tessDataProvider;
        Logger = logger ?? NullLogger<ITesseract>.Instance;
    }

    /// <summary>
    /// New instance of High-level functionality with Tesseract ocr.
    /// </summary>
    /// <param name="tessdataFolder">Path to tessdata folder.</param>
    /// <param name="traineddataFileNames">Array of traineddata file names that exist in tessdata folder. Include extensions, not path.</param>
    /// <param name="logger">Logger to be used when running recognizion and other processes.</param>
    /// <exception cref="ArgumentNullException">
    /// If traineddata file name is null or empty 
    /// OR tessdataFolder is null.
    /// </exception>
    public Tesseract(string tessdataFolder, string[] traineddataFileNames, ILogger<ITesseract>? logger = null)
    {
        if (tessdataFolder is null)
        {
            throw new ArgumentNullException(nameof(tessdataFolder));
        }

        var traineddataCollection = new TrainedDataCollection();
        foreach (var file in traineddataFileNames)
        {
            traineddataCollection.AddNonPackageFile(file);
        }
        TessDataProvider = new TessDataProvider(traineddataCollection, new TessDataProviderConfiguration()
        {
            TessDataFolder = tessdataFolder
        });
        Logger = logger ?? NullLogger<ITesseract>.Instance;
    }

    /// <inheritdoc/>
    public string TessDataFolder => TessDataProvider.TessDataFolder;

    ITessDataProvider TessDataProvider { get; }
    ILogger<ITesseract> Logger { get; }

    /// <inheritdoc/>
    public async Task<DataLoadResult> LoadTraineddataAsync()
    {
        var result = await TessDataProvider.LoadFromPackagesAsync();
        result.LogLoadErrorsIfNotAllSuccess(Logger);
        return result;
    }

    /// <inheritdoc/>
    public RecognizionResult RecognizeText(string imagePath, bool useGrayScale = false, float rotationDegree = 0)
    {
        Logger.LogInformation("Tesseract, recognize image '{path}'.", imagePath);
        
        var tessData = TessDataProvider.TessDataFolder;
        var languages = TessDataProvider.GetAllFileNames();
        return Recognize(tessData, languages, imagePath, useGrayScale, rotationDegree);
    }

    /// <inheritdoc/>
    public RecognizionResult RecognizeText(byte[] imageArray, bool useGrayScale = false, float rotationDegree = 0)
    {
        Logger.LogInformation("Tesseract, recognize imageArray.");

        var tessData = TessDataProvider.TessDataFolder;
        var languages = TessDataProvider.GetAllFileNames();
        return Recognize(tessData, languages, imageArray, useGrayScale, rotationDegree );
    }

    /// <inheritdoc/>
    public async Task<RecognizionResult> RecognizeTextAsync(string imagePath, bool useGrayScale = false, float rotationDegree = 0)
    {
        Logger.LogInformation("Tesseract, recognize image '{path}' async.", imagePath);

        // Load traineddata if all files not loaded yet.
        if (TessDataProvider.IsAllDataLoaded is false)
        {
            var loadResult = await LoadTraineddataAsync();
            if (loadResult.NotSuccess())
            {
                return new RecognizionResult
                {
                    Status = RecognizionStatus.CannotLoadTessData,
                    Message = $"Failed to load '{loadResult.GetErrorCount()}' traineddata files. " +
                        $"Errors: '{loadResult.GetErrorsString()}'"
                };
            }
        }
        
        var tessData = TessDataProvider.TessDataFolder;
        var languages = TessDataProvider.AvailableLanguages;
        return await Task.Run(() => Recognize(tessData, languages, imagePath, useGrayScale, rotationDegree));
    }

    /// <inheritdoc/>
    public async Task<RecognizionResult> RecognizeTextAsync(byte[] imageArray, bool useGrayScale = false, float rotationDegree = 0)
    {
        Logger.LogInformation("Tesseract, recognize image  async.");

        // Load traineddata if all files not loaded yet.
        if (TessDataProvider.IsAllDataLoaded is false)
        {
            var loadResult = await LoadTraineddataAsync();
            if (loadResult.NotSuccess())
            {
                return new RecognizionResult
                {
                    Status = RecognizionStatus.CannotLoadTessData,
                    Message = $"Failed to load '{loadResult.GetErrorCount()}' traineddata files. " +
                        $"Errors: '{loadResult.GetErrorsString()}'"
                };
            }
        }

        var tessData = TessDataProvider.TessDataFolder;
        var languages = TessDataProvider.AvailableLanguages;
        return await Task.Run(() => Recognize(tessData, languages, imageArray, useGrayScale, rotationDegree));
    }

    internal RecognizionResult Recognize(string tessDataFolder, string[] traineddataFileNames, string imagePath, bool useGrayScale, float rotationDegree)
    {
        using var image = Pix.LoadFromFile(imagePath);
        TransformImage(image, useGrayScale, rotationDegree);
        return Recognize(tessDataFolder, traineddataFileNames, image);
    }

    /// <summary>
    /// Prepare Pix image to be recogined by Tesseract
    /// </summary>
    /// <param name="tessDataFolder"></param>
    /// <param name="traineddataFileNames"></param>
    /// <param name="imageArray"></param>
    /// <param name="rotationDegree"></param>
    /// <returns></returns>
    internal RecognizionResult Recognize(string tessDataFolder, string[] traineddataFileNames, byte[] imageArray, bool useGrayScale, float rotationDegree)
    {
        using var image = Pix.LoadFromMemory(imageArray);
        TransformImage(image, useGrayScale, rotationDegree);
        return Recognize(tessDataFolder, traineddataFileNames, image);
    }

    internal void TransformImage(Pix image, bool useGrayScale = false, float rotationDegree = 0)
    {
        float rotation = (float)((Math.PI / 180) * rotationDegree);
        image = image.Rotate(rotation);
        if (useGrayScale)
            image = image.ConvertRGBToGray();
    }

    /// <summary>
    /// Recognize text in image. This method can only throw DllNotFoundException.
    /// </summary>
    /// <param name="tessDataFolder">Path to folder containing traineddata files.</param>
    /// <param name="traineddataFileNames">Array of traineddata file names, which include .traineddata extension.</param>
    /// <param name="image">Pix Image to be recognized.</param>
    /// <returns>RecognizionResult, information about recognizion status</returns>
    /// <exception cref="DllNotFoundException">If tesseract or any other library is not found.</exception>
    internal RecognizionResult Recognize(string tessDataFolder, string[] traineddataFileNames, Pix image)
    {
        var (languages, langParseResult) = TrainedDataToLanguage(tessDataFolder, traineddataFileNames);
        if (string.IsNullOrWhiteSpace(languages))
        {
            return langParseResult.GetValueOrDefault(
                new RecognizionResult
                {
                    Status = RecognizionStatus.NoLanguagesAvailable
                });
        }
        if (image == null)
        {
            Logger.LogWarning("Cannot recognize text. Image is empty.");
            return new RecognizionResult
            {
                Status = RecognizionStatus.ImageNotFound,
                Message = "Image is empty"
            };
        }
        if (string.IsNullOrWhiteSpace(tessDataFolder))
        {
            Logger.LogWarning("Tessdata folder is set to empty path => use 'assemblyLocation/tessdata'.");
            return new RecognizionResult
            {
                Status = RecognizionStatus.TessDataFolderNotProvided,
                Message = "Traineddata folder was null or empty."
            };
        }

        Logger.LogInformation("Recognize image");

        string? text = null;
        float confidence = -1f;

        // Recognize and catch any exceptions that can be thrown
        try
        {
            // nulls are alredy checked, can't throw.
            using var engine = new TessEngine(languages, tessDataFolder, Logger);

            // image can't be null here
            using var page = engine.ProcessImage(image);

            // SegMode can't be OsdOnly in here.
            confidence = page.GetConfidence();

            // SegMode can't be OsdOnly in here.
            text = page.GetText();
        }
        catch (IOException)
        {
            return new()
            {
                Status = RecognizionStatus.ImageNotFound,
                Message = $"Image cannot be processed."
            };
        }
        catch (ArgumentException)
        {
            return new()
            {
                Status = RecognizionStatus.InvalidImage,
                Message = "Cannot process Pix image, height or width has invalid value."
            };
        }
        catch (InvalidOperationException)
        {
            return new()
            {
                Status = RecognizionStatus.ImageAlredyProcessed,
                Message = "Image must be disposed after use."
            };
        }
        catch (ImageRecognizionException)
        {
            return new()
            {
                Status = RecognizionStatus.CannotRecognizeText,
                Message = "Library cannot recognize image for some reason."
            };
        }
        catch (InvalidBytesException ex)
        {
            return new()
            {
                Status = RecognizionStatus.CannotRecognizeText,
                Message = $"Recognized text contained invalid bytes. " +
                $"See inner exception '{ex.GetType().Name}', '{ex.Message}'."
            };
        }
        catch (TesseractException)
        {
            return new()
            {
                Status = RecognizionStatus.CannotRecognizeText,
                Message = "Library cannot thresholded image."
            };
        }
        catch (DllNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new()
            {
                Status = RecognizionStatus.UnknowError,
                Message = $"Failed to ocr for unknown reason '{ex.GetType().Name}': '{ex.Message}'."
            };
        }


        Logger.LogInformation("Recognized image with confidence '{value}'", confidence);
        Logger.LogInformation("Image contained text with length '{value}'", text?.Length ?? 0);
        return new RecognizionResult
        {
            Status = RecognizionStatus.Success,
            RecognisedText = text,
            Confidence = confidence,
        };
    }

    /// <summary>
    /// Traineddata file array to '+' separated languages list. 
    /// </summary>
    /// <param name="tessdataFolder"></param>
    /// <param name="traineddataFileNames"></param>
    /// <returns>(null, ErrorResult) if failed, othewise (lang, null).</returns>
    private static (string?, RecognizionResult?) TrainedDataToLanguage(in string tessdataFolder, params string[] traineddataFileNames)
    {
        if (string.IsNullOrWhiteSpace(tessdataFolder))
        {
            return (null, new RecognizionResult
            {
                Status = RecognizionStatus.TessDataFolderNotProvided,
                Message = "Tessdata folder is null or empty."
            });
        }

        var files = traineddataFileNames.Where(x => string.IsNullOrWhiteSpace(x) is false);
        List<string> languages = new();
        foreach (var file in files)
        {
            string filePath = Path.Combine(tessdataFolder, file);
            if (File.Exists(filePath) is false)
            {
                continue;
            }
            var lang = Path.GetFileNameWithoutExtension(file);
            if (string.IsNullOrWhiteSpace(lang))
            {
                continue;
            }
            languages.Add(lang);
        }
        if (languages.Count <= 0)
        {
            return (null, new RecognizionResult
            {
                Status = RecognizionStatus.NoLanguagesAvailable,
                Message = "No languages provided or all traineddata files invalid."
            });
        }
        return (string.Join('+', languages), null);
    }
}
