using System;
using System.Collections.Generic;


public static class FileNamesContainer
{
    private static Dictionary<Type, string> _fileNames;

    static FileNamesContainer()
    {
        _fileNames = new Dictionary<Type, string>();
    }

    public static void Add(Type dataType, string fileName)
    {
        _fileNames[dataType] = fileName;
    }

    public static string GetFileName(Type dataType)
    {
        if (_fileNames.ContainsKey(dataType))
        {
            return _fileNames[dataType];
        }
        else
        {
            throw new ArgumentException($"No file name found for type {dataType.FullName}");
        }
    }
}