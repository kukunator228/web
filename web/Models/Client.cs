using System;
using System.Collections.Generic;

namespace web.Models;

public partial class Client
{
    public int ClientId { get; set; }

    public string ClientFinstName { get; set; } = null!;

    public string ClientSecondName { get; set; } = null!;

    public string ClientPatronymic { get; set; } = null!;

    public string ClientEmail { get; set; } = null!;
}
