﻿using TesseractOcrMAUILib.Tessdata;

namespace TesseractOcrMAUILib.Results;
public readonly struct DataLoadResult
{
    public required TessDataState State { get; init; }
    public string[]? InvalidFiles { get; init; }
    public string? Message { get; init; }
    public string[]? Errors { get; init; }
}