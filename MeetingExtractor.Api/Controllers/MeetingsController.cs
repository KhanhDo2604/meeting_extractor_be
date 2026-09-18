using MeetingExtractor.Domain.Entities;
using MeetingExtractor.Domain.Enums;
using MeetingExtractor.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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

    public MeetingsController(ApplicationDbContext context)
    {
        _context = context;
    }


    private static readonly List<Meeting> _meeting = new();

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
}