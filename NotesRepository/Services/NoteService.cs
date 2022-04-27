﻿using NotesRepository.Data.Models;
using NotesRepository.Repositories;

namespace NotesRepository.Services
{
    public class NoteService
    {
        private readonly NoteRepository _nr;
        private readonly UserRepository _ur;
        private readonly EventRepository _er;
        private readonly DirectoryRepository _dr;
        private readonly ImageRepository _ir;

        public NoteService(NoteRepository noteRepository)
        {
            _nr = noteRepository;
        }

        public NoteService(NoteRepository noteRepository, UserRepository userRepository, EventRepository eventRepository, DirectoryRepository directoryRepository, ImageRepository imageRepository)
        {
            _nr = noteRepository;
            _ur = userRepository;
            _er = eventRepository;
            _dr = directoryRepository;
            _ir = imageRepository;
        }

        public async Task<Note?> GetNoteByIdAsync(Guid noteId)
            => await _nr.GetNoteByIdAsync(noteId);

        public async Task<List<Note>> GetAllUserNotesByIdAsync(string userId)
            => (await _nr.GetAllUserNotesAsync(userId)).ToList();

        public async Task<Note?> GetNoteByTitleAsync(string title)
            => await _nr.GetNoteByTitleAsync(title);

        public async Task<List<Note>> SearchNotesByTitleAndContentAsync(string searchText)
            => await _nr.SearchNoteByTitleAndContentAsync(searchText);

        public async Task<bool> AddNoteAsync(Note note)
            => await _nr.AddNoteAsync(note);
        
        public async Task<bool> AddNotesAsync(ICollection<Note> notes)
            => await _nr.AddNotesAsync(notes);

        public async Task<bool> UpdateNoteAsync(Note note)
            => await _nr.UpdateNoteAsync(note);

        public async Task<bool> DeleteNoteAsync(Note note)
            => await _nr.DeleteNoteAsync(note);

        public async Task<bool> DeleteNoteByIdAsync(Guid noteId)
            => await _nr.DeleteNoteByIdAsync(noteId);

        public async Task<bool> DeleteNotesAsync(ICollection<Note> notes)
            => await _nr.DeleteNotesAsync(notes);

        public async Task<bool> SetNoteAsCurrentlyEditedAsync(Guid noteId)
            => await _nr.SetNoteAsCurrentlyEditedAsync(noteId);
        
        public async Task<bool> SetNoteAsCurrentlyNotEditedAsync(Guid noteId)
            => await _nr.SetNoteAsCurrentlyNotEditedAsync(noteId);
        
        public async Task<bool> SetLastEditedTimeAndUserAsync(DateTime editedAt, string userId, Guid noteId)
        {
            var user = await _ur.GetUserByIdAsync(userId);
            var note = await _nr.GetNoteByIdAsync(noteId);
            if (user is not null && note is not null)
            {
                note.EditedAt = editedAt;
                note.EditedBy = user;
                return await _nr.UpdateNoteAsync(note);
            }
            return false;
        }
        
        public async Task<bool> MarkNoteAsDeletedAndStartTimerAsync(Guid noteId)
        {
            var note = await _nr.GetNoteByIdAsync(noteId);
            if (note is not null)
            {
                note.DeletedAt = DateTime.Now;
                note.IsMarkedAsDeleted = true;
                //TODO: add scheduler for 30 days (using Quartz.NET)
                return await _nr.UpdateNoteAsync(note);
            }
            return false;
        }
        
        public async Task<bool> PinNoteAsync(Guid noteId)
        {
            var note = await _nr.GetNoteByIdAsync(noteId);
            if (note is not null)
            {
                note.IsPinned = true;
                return await _nr.UpdateNoteAsync(note);
            }
            return false;
        }
        
        public async Task<bool> AttachEventToNoteAsync(Guid eventId, Guid noteId)
        {
            var _event = await _er.GetEventByIdAsync(eventId);
            var note = await _nr.GetNoteByIdAsync(noteId);
            if (note is not null && _event is not null)
            {
                await _er.DeleteEventAsync(_event);
                note.Event = _event;
                return await _nr.UpdateNoteAsync(note);
            }
            return false;
        }
        
        public async Task<bool> EditNoteContentAsync(Guid noteId, string newContent)
        {
            var note = await _nr.GetNoteByIdAsync(noteId);
            if (note is not null)
            {
                note.Content = newContent;
                return await _nr.UpdateNoteAsync(note);
            }
            return false;
        }
        
        public async Task<bool> EditNoteTitleAsync(Guid noteId, string newTitle)
        {
            var note = await _nr.GetNoteByIdAsync(noteId);
            if (note is not null)
            {
                note.Title = newTitle;
                return await _nr.UpdateNoteAsync(note);
            }
            return false;
        }
        
        public async Task<bool> ChangeNoteDirectoryAsync(Guid noteId, Guid newDirectoryId)
        {
            var note = await _nr.GetNoteByIdAsync(noteId);
            var directory = await _dr.GetDirectoryByIdAsync(newDirectoryId);
            if (note is not null && directory is not null)
            {
                note.Directory = directory;
                return await _nr.UpdateNoteAsync(note);
            }
            return false;
        }
        
        public async Task<bool> AttachImageToNoteAsync(Guid noteId, Guid imageId)
        {
            var note = await _nr.GetNoteByIdAsync(noteId);
            var image = await _ir.GetImageByIdAsync(imageId);
            if (note is not null && image is not null)
            {
                if (note.Images is null)
                    note.Images = new List<Image> { image };
                else
                    note.Images.Add(image);
                return await _nr.UpdateNoteAsync(note);
            }
            return false;
        }
        
        public async Task<bool> RemoveImageFromNoteAsync(Guid noteId, Guid imageId)
        {
            var note = await _nr.GetNoteByIdAsync(noteId);
            var image = await _ir.GetImageByIdAsync(imageId);
            if (note is not null && image is not null)
            {
                if (note.Images is null)
                    return false;
                else
                    note.Images.Remove(image);
                return await _nr.UpdateNoteAsync(note);
            }
            return false;
        }
    }
}