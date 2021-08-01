namespace ORMqb.Testing
{
    public class SaldoTestata
    {
        public string Stabilimento { get; set; }

        public string Magazzino { get; set; }

        public string Progressivo { get; set; }
    }

    public class SaldoDettaglio
    {
        public string Stabilimento { get; set; }

        public string Magazzino { get; set; }

        public string Progressivo { get; set; }

        public string Articolo { get; set; }

        public int IdLotto { get; set; }

        public decimal QtPezzi { get; set; }
    }

    public class AcemaTestata
    {
        public string Stabilimento { get; set; }

        public string Magazzino { get; set; }

        public string TipoNominativo { get; set; }

        public string Nominativo { get; set; }

        public string TipoDocumento { get; set; }

        public int AnnoDocumento { get; set; }

        public string NumeroDocumento { get; set; }

        public int Id { get; set; }
    }

    public class AcemaDettaglio
    {
        public string Stabilimento { get; set; }

        public string Magazzino { get; set; }

        public string TipoNominativo { get; set; }

        public string Nominativo { get; set; }

        public string TipoDocumento { get; set; }

        public int AnnoDocumento { get; set; }

        public string NumeroDocumento { get; set; }

        public string RigaDocumento { get; set; }

        public string Articolo { get; set; }
    }

    public class Articolo
    {
        public string CodiceArticolo { get; set; }

        public string Descrizione { get; set; }

        public string TipoArticolo { get; set; }
    }

    public class SpGetProgressivo
    {
        public string Stabilimento { get; set; }
        public string Magazzino { get; set; }
        public int Anno { get; set; }
        public string Caso { get; set; }
        public string Progressivo { get; set; }
        public string Errore { get; set; }
    }
}
