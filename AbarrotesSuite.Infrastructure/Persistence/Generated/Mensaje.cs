using System;
using System.Collections.Generic;

namespace AbarrotesSuite.Infrastructure.Persistence.Generated;

public partial class Mensaje
{
    public long Id { get; set; }

    public long ConversacionId { get; set; }

    public string Rol { get; set; } = null!;

    public string Texto { get; set; } = null!;

    public DateTime Ts { get; set; }

    public virtual Conversacione Conversacion { get; set; } = null!;
}
