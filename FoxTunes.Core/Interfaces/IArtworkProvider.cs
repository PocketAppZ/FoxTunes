﻿namespace FoxTunes.Interfaces
{
    public interface IArtworkProvider : IStandardComponent
    {
        string Find(string path, ArtworkType type);

        string Find(IFileData fileData, ArtworkType type);
    }
}
