namespace EnglishCenter.API.DTOs;

public sealed class ApplicationDto
{
    public int AppId { get; set; }

    public int SenderId { get; set; }

    public string SenderName { get; set; } = string.Empty;

    public string SenderRole { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    public string? Type { get; set; }

    public string? Status { get; set; }

    public string? AdminResponse { get; set; }

    public DateTime? CreatedAt { get; set; }
}

public sealed class RespondApplicationRequest
{
    public string Status { get; set; } = string.Empty;

    public string? AdminResponse { get; set; }
}