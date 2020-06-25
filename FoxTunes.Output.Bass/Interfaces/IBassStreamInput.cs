﻿using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IBassStreamInput : IBassStreamComponent
    {
        IEnumerable<int> Queue { get; }

        bool CheckFormat(BassOutputStream stream);

        bool Contains(BassOutputStream stream);

        int Position(BassOutputStream stream);

        bool Add(BassOutputStream stream);

        bool Remove(BassOutputStream stream, bool dispose);

        void Reset();
    }
}
