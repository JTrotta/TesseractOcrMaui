using TesseractOcrMaui.Results;

namespace TesseractOcrMaui;

/// <summary>
/// High level abstraction to use Tesseract ocr.
/// </summary>
public interface ITesseract
{
    /// <summary>
    /// Load traineddata files to configured TessDataFolder.
    /// This method should not throw.
    /// </summary>
    /// <returns>DataLoadResult representing loaded trained data status, including possible errors.</returns>
    Task<DataLoadResult> LoadTraineddataAsync();

    /// <summary>
    /// Recognize text from given image. DOES NOT LOAD required TRAINEDDATA!
    /// <para/>[DEFAUTL_IMPL] Only can throw DllNotFoundException
    /// </summary>
    /// <param name="imagePath">Path to image containing file name and extension.</param>
    /// <param name="useGrayScale">Transform image to gray scale</param>
    /// <param name="rotationDegree">Rotate image in degrees (0..360)</param>
    /// <returns>RecognizionResult representing status of recognizion, including possible errors.</returns>
    /// <exception cref="DllNotFoundException">[DEFAUTL_IMPL] If tesseract or any other library is not found.</exception>
    RecognizionResult RecognizeText(string imagePath, bool useGrayScale = false, float rotationDegree = 0);

    /// <summary>
    /// Recognize text from given image. DOES NOT LOAD required TRAINEDDATA!
    /// <para/>[DEFAUTL_IMPL] Only can throw DllNotFoundException
    /// </summary>
    /// <param name="imageArray">Array of the image.</param>
    /// <param name="useGrayScale">Transform image to gray scale</param>
    /// <param name="rotationDegree">Rotate image in degrees (0..360)</param>
    /// <returns>RecognizionResult representing status of recognizion, including possible errors.</returns>
    /// <exception cref="DllNotFoundException">[DEFAUTL_IMPL] If tesseract or any other library is not found.</exception>
    RecognizionResult RecognizeText(byte[] imageArray, bool useGrayScale = false, float rotationDegree = 0);

    /// <summary>
    /// Load traineddata files from app packages and run recognizion process async.
    /// <para/>[DEFAUTL_IMPL] Only can throw DllNotFoundException
    /// </summary>
    /// <param name="imagePath">Path to image containing file name and extension.</param>
    /// <param name="useGrayScale">Transform image to gray scale</param>
    /// <param name="rotationDegree">Rotate image in degrees (0..360)</param>
    /// <returns>Task of RecognizionResult representing status of recognizion, including possible errors.</returns>
    /// <exception cref="DllNotFoundException">[DEFAUTL_IMPL] If tesseract or any other library is not found.</exception>
    Task<RecognizionResult> RecognizeTextAsync(string imagePath, bool useGrayScale = false, float rotationDegree = 0);

    /// <summary>
    /// Load traineddata files from app packages and run recognizion process async.
    /// <para/>[DEFAUTL_IMPL] Only can throw DllNotFoundException
    /// </summary>
    /// <param name="imageArray">Array of the image.</param>
    /// <param name="useGrayScale">Transform image to gray scale</param>
    /// <param name="rotationDegree">Rotate image in degrees (0..360)</param>
    /// <returns>Task of RecognizionResult representing status of recognizion, including possible errors.</returns>
    /// <exception cref="DllNotFoundException">[DEFAUTL_IMPL] If tesseract or any other library is not found.</exception>
    Task<RecognizionResult> RecognizeTextAsync(byte[] imageArray, bool useGrayScale = false, float rotationDegree = 0);

    /// <summary>
    /// Folder where tessdata should be stored. 
    /// Provided by ITessDataProvider. 
    /// Load with LoadTraineddataAsync();
    /// </summary>
    string TessDataFolder { get; }
}
