using MeetingExtractor.Domain.Entities;
using MeetingExtractor.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
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
    private static readonly List<Meeting> _meeting = new();

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_meeting);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(Guid id)
    {
        var meeting = _meeting.FirstOrDefault(m => m.Id == id);
        if (meeting is null)
        {
            return NotFound();
        }

        return Ok(meeting);
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateMeetingRequest request)
    {
        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Status = MeetingStatus.Pending,
        };

        _meeting.Add(meeting);
        return Ok(meeting);
    }
}