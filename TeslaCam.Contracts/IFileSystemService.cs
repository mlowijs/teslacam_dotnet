﻿using System.Collections.Generic;
using System.Threading;
using TeslaCam.Model;

namespace TeslaCam.Contracts
{
    public interface IFileSystemService
    {
        IEnumerable<Clip> GetClips(ClipType clipType);
        void ArchiveClips(IEnumerable<Clip> clips, CancellationToken cancellationToken);
        void TruncateClip(Clip clip);
        void DeleteClips(IEnumerable<Clip> clips, CancellationToken cancellationToken);

        IEnumerable<Clip> GetArchivedClips();
        bool IsArchived(Clip clip);
    }
}