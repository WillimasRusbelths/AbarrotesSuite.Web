using System;
using System.Collections.Generic;

namespace AbarrotesSuite.Infrastructure.Persistence.Generated;

public partial class Conversacione
{
    public long Id { get; set; }

    public long? UsuarioId { get; set; }

    public DateTime CreadoEn { get; set; }

    public virtual ICollection<Mensaje> Mensajes { get; set; } = new List<Mensaje>();
}
