using MeetingExtractor.Application.Interfaces;
using MeetingExtractor.Domain.Entities;
using MeetingExtractor.Domain.Enums;
using MeetingExtractor.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

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
    private readonly IWhisperService _whisperService;
    private readonly IAiProvider _aiProvider;

    public MeetingsController(ApplicationDbContext context, IWhisperService whisperService, IAiProvider aiProvider)
    {
        _context = context;
        _whisperService = whisperService;
        _aiProvider = aiProvider;
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
        {
            return BadRequest("Audio file is empty.");
        }

        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedAudios");

        var fileName = $"{meeting.Id}_{audioFile.FileName}";
        var filePath = Path.Combine(uploadsPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await audioFile.CopyToAsync(stream);
        }

        meeting.AudioUrl = filePath;
        meeting.Status = MeetingStatus.Transcribing;
        await _context.SaveChangesAsync();

        var transcript = await _whisperService.TranscribeAsync(filePath);

        meeting.Transcript = transcript;
        meeting.Status = MeetingStatus.Done;
        await _context.SaveChangesAsync();

        var prompt = $$"""
            You are an assistant that extracts structured information from meeting transcripts.
            Given the transcript below, return ONLY a valid JSON object, no extra text, no markdown code block, in this exact format:
            {
              "summary": "a short 2-3 sentence summary of the meeting",
              "action_items": [
                { "task": "description of the task", "owner": "person responsible or null", "deadline": "YYYY-MM-DD or null" }
              ]
            }

            Transcript:
            {{transcript}}
            """;

        var aiResponse = await _aiProvider.GenerateReplyAsync(prompt);
        try
        {
            using var document = JsonDocument.Parse(aiResponse);
            var root = document.RootElement;

            meeting.Summary = root.GetProperty("summary").GetString();

            if (root.TryGetProperty("action_items", out var actionItemsElement))
            {
                foreach (var item in actionItemsElement.EnumerateArray())
                {
                    var actionItem = new ActionItem
                    {
                        Id = Guid.NewGuid(),
                        MeetingId = meeting.Id,
                        Task = item.GetProperty("task").GetString() ?? string.Empty,
                        Owner = item.TryGetProperty("owner", out var owner) ? owner.GetString() : null,
                        Deadline = item.TryGetProperty("deadline", out var deadline)
                            && DateTime.TryParse(deadline.GetString(), out var parsedDate)
                                ? parsedDate
                                : null
                    };
                    _context.ActionItems.Add(actionItem);

                }
            }
            meeting.Status = MeetingStatus.Done;
        }
        catch(JsonException)
        {
            meeting.Status = MeetingStatus.Failed;
        }
        await _context.SaveChangesAsync();

        return Ok(meeting);
    }
}