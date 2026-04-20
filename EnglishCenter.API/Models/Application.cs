using System;
using System.Collections.Generic;

namespace EnglishCenter.API.Models;

public partial class Application
{
    public int AppId { get; set; }

    public int SenderId { get; set; }

    public string Title { get; set; } = null!;

    public string? Content { get; set; }

    public string? Type { get; set; }

    public string? Status { get; set; }

    public string? AdminResponse { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User Sender { get; set; } = null!;
}
