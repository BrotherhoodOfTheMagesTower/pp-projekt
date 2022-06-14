﻿using NotesRepository.Data.Models;
using NotesRepository.Repositories;
using NotesRepository.Services.Azure;
using Directory = NotesRepository.Data.Models.Directory;

namespace NotesRepository.Services
{
    public class DirectoryService
    {
        private readonly NoteRepository _nr;
        private readonly UserRepository _ur;
        private readonly ImageRepository _ir;
        private readonly DirectoryRepository _dr;
        private readonly string containerName = "imagecontainer";
        private AzureStorageHelper _azureHelper;


        public DirectoryService(DirectoryRepository directoryRepository)
        {
            _dr = directoryRepository;
        }

        public DirectoryService(NoteRepository noteRepository, DirectoryRepository directoryRepository, UserRepository userRepository, ImageRepository imageRepository)
        {
            _nr = noteRepository;
            _ur = userRepository;
            _dr = directoryRepository;
            _ir = imageRepository;
        }

        public DirectoryService(NoteRepository noteRepository, DirectoryRepository directoryRepository, UserRepository userRepository, ImageRepository imageRepository, AzureStorageHelper azureStorageHelper)
        {
            _nr = noteRepository;
            _ur = userRepository;
            _dr = directoryRepository;
            _ir = imageRepository;
            _azureHelper = azureStorageHelper;
        }

        public async Task<Directory?> GetDirectoryByIdAsync(Guid directoryId)
            => await _dr.GetByIdAsync(directoryId);

        public async Task<Directory?> GetDirectoryByNameAsync(string name, string userId)
            => await _dr.GetDirectoryByNameAsync(name, userId);

        public async Task<bool> AddDirectoryAsync(Directory directory)
            => await _dr.AddAsync(directory);


        public async Task<Directory?> GetBinForParticularUserAsync(string userId)
          => await _dr.GetDirectoryByNameAsync("Bin", userId);

        public async Task<ICollection<Directory>?> GetAllDirectoriesForParticularUserAsync(string userId)
        => await _dr.GetAllDirectoriesForParticularUserAsync(userId);

        public async Task<ICollection<Directory>?> GetAllNotDeletedDirectoriesForParticularUserAsync(string userId)
        => await _dr.GetAllNotDeletedDirectoriesForParticularUserAsync(userId);

        public async Task<ICollection<Directory>?> GetAllDirectoriesWithoutParentDirectoryForParticularUserAsync(string userId)
        => await _dr.GetAllDirectoriesWithoutParentDirectoryForParticularUserAsync(userId);

        public ICollection<Directory>? GetAllDirectoriesWithoutParentDirectoryForParticularUserSync(string userId)
        => _dr.GetAllDirectoriesWithoutParentDirectoryForParticularUserSync(userId);

        public ICollection<Directory>? GetAllSubDirectoriesOfParticularDirectorySync(Guid directoryId)
            => _dr.GetAllSubDirectoriesOfParticularDirectorySync(directoryId);

        public async Task<bool> ChangeParentDirectoryForSubDirectory(Guid subDirectoryId, Guid directoryId)
            => await _dr.ChangeParentDirectoryForSubDirectory(subDirectoryId, directoryId);



        public bool RemoveDirectoriesSubdirectoriesAndNotesFromBinAndDbByDate(int daysOld = 30)
        {
            var directories = _dr.GetMainDirectoriesWhichShouldBeRemovedFromDb(daysOld).ToList();

            var singleNotes = _nr.GetAllSingleNotesWhichShouldBeRemovedFromDb(daysOld);
            if (singleNotes.Count > 0)
            {
                DeleteImagesFromNotesList(singleNotes);
                _nr.DeleteMany(singleNotes);
            }

            if (directories.Count > 0)
            {
                foreach (var directory in directories)
                {
                    var subDirectoryNotes = _nr.GetAllNotesForParticularDirectory(directory.DirectoryId);
                    DeleteImagesFromNotesList(subDirectoryNotes);
                    RemoveSubdirectoriesByDirectoryId(directory.DirectoryId);
                    _dr.DeleteById(directory.DirectoryId);
                }
                return true;

            }
            else return false;
        }

        public async Task<bool> RemoveDirectoriesSubdirectoriesAndNotesFromBin(Guid directoryId)
        {
            var directory = await _dr.GetByIdAsync(directoryId);
            if (directory != null)
            {
                RemoveSubdirectoriesByDirectoryId(directory.DirectoryId);
                _dr.DeleteById(directory.DirectoryId);
                return true;
            }
            return false;

        }



        public bool RemoveSubdirectoriesByDirectoryId(Guid directoryId)
        {
            var subDirectories = _dr.GetAllSubDirectoriesOfParticularDirectory(directoryId);

            if (subDirectories.Count > 0)
            {
                foreach (var subdirectory in subDirectories)
                {
                    var subDirectoryNotes = _nr.GetAllNotesForParticularDirectory(subdirectory.DirectoryId);
                    DeleteImagesFromNotesList(subDirectoryNotes);
                    RemoveSubdirectoriesByDirectoryId(subdirectory.DirectoryId);
                    _dr.DeleteById(subdirectory.DirectoryId);
                }
                return true;
            }
            return false;
        }

        public bool DeleteImagesFromNotesList(ICollection<Note> notesList)
        {
            if (notesList != null)
            {
                if (notesList.Count > 0)
                {
                    foreach (var note in notesList)
                    {
                        var imagesAttachedToNote = _ir.GetAllNoteImages(note.NoteId);
                        if (imagesAttachedToNote != null)
                        {
                            foreach (var image in imagesAttachedToNote)
                            {
                                _azureHelper.DeleteImageFromAzureNotAsync(image.Name, containerName);
                                _ir.Delete(image);
                            }
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> MoveDirectorySubdirectoriesAndNotesToBin(Guid directoryId)
        {
            var directory = await _dr.GetByIdAsync(directoryId);
            var bin = await _dr.GetDirectoryByNameAsync("Bin", directory.User.Id);

            if (directory != null && bin != null)
            {
                var result = await MarkDirectorySubdirectoriesAndNotesAsDeleted(directoryId);
                if (result == true)
                {
                    await _dr.ChangeParentDirectoryForSubDirectory(directoryId, bin.DirectoryId);
                    return true;
                }
                else return false;
            }
            else return false;

        }

        public async Task<bool> MarkDirectorySubdirectoriesAndNotesAsDeleted(Guid directoryId)
        {
            var directory = await _dr.GetByIdAsync(directoryId);

            if (directory == null)
            {
                return false;
            }

            var subDirectories = await _dr.GetAllSubDirectoriesOfParticularDirectoryAsync(directoryId);

            if (subDirectories != null)
            {
                if (subDirectories.Count > 0)
                {
                    foreach (var subdirectory in subDirectories)
                    {

                        await MarkDirectorySubdirectoriesAndNotesAsDeleted(subdirectory.DirectoryId);
                    }
                }
            }

            var notes = await _nr.GetAllNotesForParticularDirectoryAsync(directoryId);
            if (notes != null)
            {
                if (notes.Count > 0)
                {
                    foreach (var note in notes)
                    {
                        await _nr.MarkNoteAsDeletedAsync(note.NoteId);
                    }
                }
            }
            await _dr.MarkDirectoryAsDeletedAsync(directoryId);


            return true;
        }

        public async Task<bool> RestoreADirectorySubdirectoriesAndNotesFromBinToDirectory(Guid directoryId, Guid parentDirectoryId)
        {
            var directory = await _dr.GetByIdAsync(directoryId);
            var parentDirectory = await _dr.GetByIdAsync(parentDirectoryId);

            if (directory != null && parentDirectory != null)
            {
                var result = await MarkDirectorySubdirectoriesAndNotesAsNotDeleted(directoryId);
                if (result == true)
                {
                    await _dr.ChangeParentDirectoryForSubDirectory(directoryId, parentDirectoryId);
                    return true;
                }
                else return false;
            }
            else return false;

        }

        public async Task<bool> MarkDirectorySubdirectoriesAndNotesAsNotDeleted(Guid directoryId)
        {
            var directory = await _dr.GetByIdAsync(directoryId);

            if (directory == null)
            {
                return false;
            }

            var subDirectories = await _dr.GetAllSubDirectoriesOfParticularDirectoryAsync(directoryId);

            if (subDirectories != null)
            {
                if (subDirectories.Count > 0)
                {
                    foreach (var subdirectory in subDirectories)
                    {

                        await MarkDirectorySubdirectoriesAndNotesAsNotDeleted(subdirectory.DirectoryId);
                    }
                }
            }

            var notes = await _nr.GetAllNotesForParticularDirectoryAsync(directoryId);
            if (notes != null)
            {
                if (notes.Count > 0)
                {
                    foreach (var note in notes)
                    {
                        await _nr.MarkNoteAsNotDeletedAsync(note.NoteId);
                    }
                }
            }
            await _dr.MarkDirectoryAsNotDeletedAsync(directoryId);


            return true;
        }

        public async Task<bool> UpdateAsync(Directory _directory)
            => await _dr.UpdateAsync(_directory);

        public async Task<bool> CheckIfTheFolderTitleExistsForParticularUser(string title, string userId)
        {
            var directory = await _dr.GetDirectoryByNameAsync(title, userId);
            if (directory == null)
                return false;
            else return true;
        }
    }

    //to develop in future

    //public async Task<bool> DeleteDirectoryByIdAsync(Guid directoryId)
    //{
    //    var subDirectoryNotes = _nr.GetAllNotesForParticularDirectory(directoryId);
    //    DeleteImagesFromNotesList(subDirectoryNotes);
    //    // Sub directories
    //    var subDirectories = await _dr.GetAllSubDirectoriesOfParticularDirectoryAsync(directoryId);
    //    if (subDirectories != null)
    //    {
    //        foreach (var directory in subDirectories)
    //        {
    //            var notesFromSubDirectory = await _nr.GetAllNotesForParticularDirectoryAsync(directory.DirectoryId);
    //            DeleteImagesFromNotesList(notesFromSubDirectory);
    //        }
    //    }

    //    // Current notes
    //    var notesFromParentDirectory = _nr.GetAllNotesForParticularDirectory(directoryId);
    //    DeleteImagesFromNotesList(notesFromParentDirectory);
    //    return await _dr.DeleteByIdAsync(directoryId);
    //}

    //public async Task<bool> DeleteManyDirectoriesAsync(ICollection<Directory> directories)
    //{
    //    foreach (Directory directory in directories)
    //    {
    //        // Sub directories
    //        var subDirectories = await _dr.GetAllSubDirectoriesOfParticularDirectoryAsync(directory.DirectoryId);
    //        if (subDirectories != null)
    //        {
    //            foreach (var dir in subDirectories)
    //            {
    //                var notesFromSubDirectory = await _nr.GetAllNotesForParticularDirectoryAsync(dir.DirectoryId);
    //                DeleteImagesFromNotesList(notesFromSubDirectory);
    //                await _dr.DeleteAsync(dir);
    //            }
    //        }

    //        // Current notes
    //        var notesFromParentDirectory = _nr.GetAllNotesForParticularDirectory(directory.DirectoryId);
    //        DeleteImagesFromNotesList(notesFromParentDirectory);
    //    }
    //    return await _dr.DeleteManyAsync(directories);
    //}

    //public async Task<ICollection<Directory>?> GetAllDirectoriesFromBinForParticularUserAsync(string userId)
    //{
    //    var directory = await _dr.GetDirectoryByNameAsync("Bin", userId);
    //    if (directory != null)
    //    {
    //        return await _dr.GetAllSubDirectoriesOfParticularDirectoryAsync(directory.DirectoryId);
    //    }
    //    else return null;
    //}

    //public async Task<Directory?> GetDefaultDirectoryForParticularUserAsync(string userId)
    //  => await _dr.GetDirectoryByNameAsync("Default", userId);
    //public async Task<ICollection<Directory>?> GetAllSubDirectoriesOfParticularDirectoryAsync(Guid directoryId)
    //    => await _dr.GetAllSubDirectoriesOfParticularDirectoryAsync(directoryId);
}