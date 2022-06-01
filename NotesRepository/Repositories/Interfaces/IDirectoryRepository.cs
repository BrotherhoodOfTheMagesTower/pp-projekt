﻿using NotesRepository.Data.Models;
using Directory = NotesRepository.Data.Models.Directory;

namespace NotesRepository.Repositories.Interfaces
{
    public interface IDirectoryRepository : IRepository<Directory>
    {
        Task<Directory?> GetDirectoryByNameAsync(string name, string userId);
        Task<ICollection<Directory>> GetAllDirectoriesForParticularUserAsync(string userId);
        Task<ICollection<Directory>> GetAllNotDeletedDirectoriesForParticularUserAsync(string userId);
        ICollection<Directory> GetAllSubDirectoriesOfParticularDirectorySync(Guid directoryId);
        Task<ICollection<Directory>?> GetAllSubDirectoriesOfParticularDirectoryAsync(Guid directoryId);
        Task<ICollection<Directory>?> GetAllDirectoriesWithoutParentDirectoryForParticularUserAsync(string userId);
        ICollection<Directory> GetMainDirectoriesWhichShouldBeRemovedFromDb(int daysOld);
        Task<bool> ChangeParentDirectoryForSubDirectory(Guid subDirectoryId, Guid directoryId);
        Task<bool> DeleteManyAsync(ICollection<Directory> directories);
        Task<bool> DeleteAllSubDirectoriesForParticularDirectoryAsync(Guid directoryId);
        Task<bool> MarkDirectoryAsDeletedAsync(Guid directoryId);
        Task<bool> MarkDirectoryAsNotDeletedAsync(Guid directoryId);

    }
}


    
