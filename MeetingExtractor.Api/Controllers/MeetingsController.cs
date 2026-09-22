using MeetingExtractor.Application.Interfaces;
using MeetingExtractor.Domain.Entities;
using MeetingExtractor.Domain.Enums;
using MeetingExtractor.Infrastructure.Persistence;
using MeetingExtractor.Application.Meetings;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Hangfire;

namespace MeetingExtractor.Api.Controllers;

// DTO
public class CreateMeetingRequest
{
    [Required(ErrorMessage = "Title can not be empty")]
    [MinLength(3, ErrorMessage = "Title must have at least 3 characters")]
    [MaxLength(200, ErrorMessage = "Title must not over 200 characters")]
    public string Title { get; set; } = string.Empty;
}


[ApiController]
[Route("api/[controller]")]
public class MeetingsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public MeetingsController(
        ApplicationDbContext context,
        IBackgroundJobClient backgroundJobClient)
    {
        _context = context;
        _backgroundJobClient = backgroundJobClient;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var meetings = await _context.Meetings.ToListAsync();
        return Ok(meetings);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var meeting = await _context.Meetings.FindAsync(id);

        if (meeting is null)
        {
            return NotFound();
        }

        return Ok(meeting);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMeetingRequest request)
    {
        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Status = MeetingStatus.Pending,
        };

        _context.Meetings.Add(meeting);
        await _context.SaveChangesAsync();

        return Ok(meeting);
    }

    [HttpPost("{id}/audio")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadAudio(Guid id, IFormFile audioFile)
    {
        var meeting = await _context.Meetings.FindAsync(id);

        if (meeting is null)
        {
            return NotFound();

        }

        if (audioFile.Length == 0)
            return BadRequest("Audio file is empty.");

        var uploadsPath = Path.Combine(
            Directory.GetCurrentDirectory(), "UploadedAudios");

        var fileName = $"{meeting.Id}_{audioFile.FileName}";
        var filePath = Path.Combine(uploadsPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await audioFile.CopyToAsync(stream);
        }

        meeting.AudioUrl = filePath;
        await _context.SaveChangesAsync();

        _backgroundJobClient.Enqueue<IMeetingProcessingService>(
            service => service.ProcessMeetingAudioAsync(meeting.Id));

        return Accepted(new { meetingId = meeting.Id, status = "Pending" });
    }
}